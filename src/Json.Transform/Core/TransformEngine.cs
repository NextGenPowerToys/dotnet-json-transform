using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Transform.Exceptions;
using Json.Transform.Models;

namespace Json.Transform.Core;

/// <summary>
/// Core transformation engine that orchestrates all transformation operations
/// </summary>
public class TransformEngine
{
    private readonly TransformSettings _settings;
    private int _currentDepth = 0;

    public TransformEngine(TransformSettings? settings = null)
    {
        _settings = settings ?? new TransformSettings();
    }

    /// <summary>
    /// Transforms source JSON data using the provided template
    /// </summary>
    /// <param name="sourceData">The source JSON data</param>
    /// <param name="template">The transformation template</param>
    /// <returns>The transformed JSON data</returns>
    public JsonNode Transform(JsonNode? sourceData, TransformTemplate template)
    {
        if (template?.Mappings == null)
            throw new TransformException("Transform template or mappings cannot be null");

        _currentDepth = 0;
        var result = new JsonObject();

        foreach (var mapping in template.Mappings.Where(m => m.Enabled))
        {
            try
            {
                ProcessMapping(sourceData, result, mapping);
            }
            catch (Exception ex) when (_settings.StrictMode)
            {
                throw new TransformException($"Failed to process mapping to '{mapping.To}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // In non-strict mode, log the error and continue
                if (_settings.EnableTracing)
                {
                    Console.WriteLine($"Warning: Failed to process mapping to '{mapping.To}': {ex.Message}");
                }
            }
        }

        return result;
    }

    private void ProcessMapping(JsonNode? sourceData, JsonObject result, Mapping mapping)
    {
        if (string.IsNullOrEmpty(mapping.To))
            return;

        JsonNode? value = null;

        // Process based on mapping type
        if (mapping.Value != null)
        {
            // Constant value mapping
            value = ProcessConstantValue(mapping.Value);
        }
        else if (mapping.Math != null)
        {
            // Mathematical operation
            value = MathProcessor.PerformOperation(mapping.Math, sourceData);
        }
        else if (!string.IsNullOrEmpty(mapping.Concat))
        {
            // String concatenation
            value = ProcessConcatenation(mapping.Concat, sourceData);
        }
        else if (!string.IsNullOrEmpty(mapping.Aggregate))
        {
            // Aggregation operation - handle conditional aggregation if conditions are present
            if (mapping.Conditions != null && mapping.Conditions.Count > 0)
            {
                value = ProcessConditionalAggregation(sourceData, mapping);
            }
            else
            {
                value = ProcessAggregation(sourceData, mapping);
            }
        }
        else if (mapping.Aggregation != null)
        {
            // New aggregation format with built-in conditions
            value = ProcessAdvancedAggregation(sourceData, mapping);
        }
        else if (!string.IsNullOrEmpty(mapping.From))
        {
            // Standard field mapping
            value = ProcessFieldMapping(sourceData, mapping);
        }
        else if (mapping.Template != null)
        {
            // Nested transformation
            value = ProcessNestedTransformation(sourceData, mapping.Template);
        }

        // Apply conditional logic if present (but not for aggregation, which handles its own conditions)
        if (mapping.Conditions != null && mapping.Conditions.Count > 0 && string.IsNullOrEmpty(mapping.Aggregate))
        {
            value = ProcessConditions(mapping.Conditions, sourceData, value);
        }

        // Use default value if result is null
        if (value == null && mapping.Default != null)
        {
            value = ProcessConstantValue(mapping.Default);
        }

        // Set the value in the result if it's not null or if we preserve nulls
        if (value != null || _settings.PreserveNulls)
        {
            PathResolver.SetValue(result, mapping.To, value, _settings.CreatePaths);
        }
    }

    private JsonNode? ProcessConstantValue(object value)
    {
        if (value == null)
            return null;

        var stringValue = value.ToString();
        
        // Handle special constant values
        return stringValue?.ToLower() switch
        {
            "now" => JsonValue.Create(DateTime.Now.ToString("o")),
            "utcnow" => JsonValue.Create(DateTime.UtcNow.ToString("o")),
            "guid" => JsonValue.Create(Guid.NewGuid().ToString()),
            "newguid" => JsonValue.Create(Guid.NewGuid().ToString()),
            "timestamp" => JsonValue.Create(DateTimeOffset.Now.ToUnixTimeSeconds()),
            "true" => JsonValue.Create(true),
            "false" => JsonValue.Create(false),
            "null" => null,
            _ => ConvertToJsonNode(value)
        };
    }

    private JsonNode? ProcessFieldMapping(JsonNode? sourceData, Mapping mapping)
    {
        if (string.IsNullOrEmpty(mapping.From) || sourceData == null)
            return null;

        try
        {
            return PathResolver.ResolveSingle(sourceData, mapping.From);
        }
        catch (PathNotFoundException) when (!_settings.StrictMode)
        {
            return null;
        }
    }

    private JsonNode? ProcessAggregation(JsonNode? sourceData, Mapping mapping)
    {
        if (string.IsNullOrEmpty(mapping.From) || string.IsNullOrEmpty(mapping.Aggregate) || sourceData == null)
            return null;

        try
        {
            // Special handling for count operation
            if (mapping.Aggregate.ToLower() == "count")
            {
                // For count, we want to count the elements in the array
                var resolvedNode = PathResolver.ResolveSingle(sourceData, mapping.From);
                if (resolvedNode is JsonArray array)
                {
                    return JsonValue.Create(array.Count);
                }
                else
                {
                    // If it resolves to a single item, count as 1, if null count as 0
                    return JsonValue.Create(resolvedNode != null ? 1 : 0);
                }
            }
            else
            {
                // For other aggregations, resolve as array
                var arrayData = PathResolver.ResolveArray(sourceData, mapping.From);
                return AggregationProcessor.Aggregate(arrayData, mapping.Aggregate);
            }
        }
        catch (Exception ex) when (!_settings.StrictMode)
        {
            if (_settings.EnableTracing)
            {
                Console.WriteLine($"Aggregation failed for {mapping.From}: {ex.Message}");
            }
            return null;
        }
    }

    private JsonNode? ProcessConcatenation(string template, JsonNode? sourceData)
    {
        if (string.IsNullOrEmpty(template) || sourceData == null)
            return JsonValue.Create(template);

        try
        {
            // Find all JSONPath expressions in the template (e.g., {$.user.name})
            var pathRegex = new Regex(@"\{\$[^}]+\}", RegexOptions.Compiled);
            var result = template;

            foreach (Match match in pathRegex.Matches(template))
            {
                var pathExpression = match.Value;
                var path = pathExpression.Substring(1, pathExpression.Length - 2); // Remove { and }
                
                var value = PathResolver.ResolveSingle(sourceData, path);
                var stringValue = ExtractStringValue(value) ?? "";
                
                result = result.Replace(pathExpression, stringValue);
            }

            return JsonValue.Create(result);
        }
        catch (Exception ex) when (!_settings.StrictMode)
        {
            if (_settings.EnableTracing)
            {
                Console.WriteLine($"Concatenation failed for template '{template}': {ex.Message}");
            }
            return JsonValue.Create(template);
        }
    }

    private JsonNode? ProcessNestedTransformation(JsonNode? sourceData, TransformTemplate template)
    {
        if (_currentDepth >= _settings.MaxDepth)
            throw new TransformException($"Maximum transformation depth ({_settings.MaxDepth}) exceeded");

        _currentDepth++;
        try
        {
            var nestedEngine = new TransformEngine(_settings);
            return nestedEngine.Transform(sourceData, template);
        }
        finally
        {
            _currentDepth--;
        }
    }

    private JsonNode? ProcessConditions(List<Condition> conditions, JsonNode? sourceData, JsonNode? currentValue)
    {
        foreach (var condition in conditions)
        {
            try
            {
                var result = ConditionEvaluator.EvaluateCondition(condition, sourceData);
                if (result != null)
                {
                    return ConvertToJsonNode(result);
                }
            }
            catch (Exception ex) when (!_settings.StrictMode)
            {
                if (_settings.EnableTracing)
                {
                    Console.WriteLine($"Condition evaluation failed: {ex.Message}");
                }
            }
        }

        return currentValue;
    }

    private JsonNode? ProcessConditionalAggregation(JsonNode? sourceData, Mapping mapping)
    {
        if (string.IsNullOrEmpty(mapping.From) || string.IsNullOrEmpty(mapping.Aggregate) || sourceData == null)
            return null;

        try
        {
            // Special handling for count operation - count all elements regardless of conditions
            if (mapping.Aggregate.ToLower() == "count")
            {
                var resolvedNode = PathResolver.ResolveSingle(sourceData, mapping.From);
                if (resolvedNode is JsonArray array)
                {
                    return JsonValue.Create(array.Count);
                }
                else
                {
                    return JsonValue.Create(resolvedNode != null ? 1 : 0);
                }
            }

            // For other aggregations, resolve as array and apply conditions to each element
            var arrayData = PathResolver.ResolveArray(sourceData, mapping.From);
            if (arrayData == null || arrayData.Count == 0)
                return null;

            // Apply conditions to each array element
            var processedValues = new List<JsonNode>();
            for (int i = 0; i < arrayData.Count; i++)
            {
                var arrayElement = arrayData[i];
                
                // Create temporary source data for condition evaluation
                // We need to create a context where the current array element can be evaluated
                var tempSourceData = CreateArrayElementContext(sourceData, mapping.From, i);
                
                // Apply conditions to this element
                if (mapping.Conditions != null && arrayElement != null)
                {
                    var conditionResult = ProcessConditionsForArrayElement(mapping.Conditions, tempSourceData, arrayElement, mapping.From, i);
                    
                    if (conditionResult != null)
                    {
                        processedValues.Add(conditionResult);
                    }
                }
            }

            // Aggregate the processed values
            var jsonArray = new JsonArray();
            foreach (var processedValue in processedValues)
            {
                jsonArray.Add(processedValue);
            }
            return AggregationProcessor.Aggregate(jsonArray, mapping.Aggregate);
        }
        catch (Exception ex) when (!_settings.StrictMode)
        {
            if (_settings.EnableTracing)
            {
                Console.WriteLine($"Conditional aggregation failed for {mapping.From}: {ex.Message}");
            }
            return null;
        }
    }

    /// <summary>
    /// Creates a context for evaluating conditions on a specific array element
    /// </summary>
    private JsonNode CreateArrayElementContext(JsonNode sourceData, string arrayPath, int index)
    {
        // Create a copy of the source data for this context
        var contextData = JsonNode.Parse(sourceData.ToJsonString());
        
        // For array element conditions, we need to modify the path to point to the specific element
        // e.g., "$.orders[*].amount" becomes "$.orders[0].amount" for index 0
        return contextData ?? sourceData;
    }

    /// <summary>
    /// Processes conditions for a specific array element
    /// </summary>
    private JsonNode? ProcessConditionsForArrayElement(List<Condition> conditions, JsonNode sourceData, JsonNode arrayElement, string arrayPath, int index)
    {
        foreach (var condition in conditions)
        {
            try
            {
                // Modify the condition to evaluate against the specific array element
                var modifiedCondition = CreateConditionForArrayElement(condition, arrayPath, index);
                var conditionResult = ConditionEvaluator.EvaluateCondition(modifiedCondition, sourceData);
                
                if (conditionResult != null)
                {
                    // If condition is met, return the appropriate value
                    if (modifiedCondition.Then != null)
                    {
                        // If "then" is a JSONPath, resolve it; otherwise use as literal
                        if (modifiedCondition.Then.ToString()?.StartsWith("$") == true)
                        {
                            var thenPath = CreatePathForArrayElement(modifiedCondition.Then.ToString(), arrayPath, index);
                            return PathResolver.ResolveSingle(sourceData, thenPath);
                        }
                        else
                        {
                            return ConvertToJsonNode(modifiedCondition.Then);
                        }
                    }
                }
            }
            catch (Exception ex) when (!_settings.StrictMode)
            {
                if (_settings.EnableTracing)
                {
                    Console.WriteLine($"Array element condition evaluation failed: {ex.Message}");
                }
            }
        }

        // If no condition matched, check for else clause in last condition
        var lastCondition = conditions.LastOrDefault();
        if (lastCondition?.Else != null)
        {
            if (lastCondition.Else.ToString()?.StartsWith("$") == true)
            {
                var elsePath = CreatePathForArrayElement(lastCondition.Else.ToString(), arrayPath, index);
                return PathResolver.ResolveSingle(sourceData, elsePath);
            }
            else
            {
                return ConvertToJsonNode(lastCondition.Else);
            }
        }

        // Default: return the original array element
        return arrayElement;
    }

    /// <summary>
    /// Creates a condition for a specific array element by replacing [*] with [index]
    /// </summary>
    private Condition CreateConditionForArrayElement(Condition condition, string arrayPath, int index)
    {
        var modifiedCondition = new Condition
        {
            If = ReplaceArrayWildcard(condition.If, arrayPath, index),
            Then = condition.Then,
            Else = condition.Else
        };

        return modifiedCondition;
    }

    /// <summary>
    /// Creates a JSONPath for a specific array element by replacing [*] with [index]
    /// </summary>
    private string CreatePathForArrayElement(string? path, string arrayPath, int index)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        return ReplaceArrayWildcard(path, arrayPath, index);
    }

    /// <summary>
    /// Replaces array wildcard [*] with specific index [i]
    /// </summary>
    private string ReplaceArrayWildcard(string? expression, string arrayPath, int index)
    {
        if (string.IsNullOrEmpty(expression))
            return string.Empty;

        // Replace [*] with [index] in the expression
        // This handles cases like "$.orders[*].amount" -> "$.orders[0].amount"
        return expression.Replace("[*]", $"[{index}]");
    }

    /// <summary>
    /// Processes the new advanced aggregation format with built-in conditions
    /// </summary>
    private JsonNode? ProcessAdvancedAggregation(JsonNode? sourceData, Mapping mapping)
    {
        if (mapping.Aggregation == null || string.IsNullOrEmpty(mapping.From) || sourceData == null)
            return null;

        try
        {
            // Resolve the array data
            var arrayData = PathResolver.ResolveArray(sourceData, mapping.From);
            if (arrayData == null || arrayData.Count == 0)
            {
                return null;
            }

            // Apply condition filter if specified
            var filteredValues = new List<JsonNode>();
            
            if (!string.IsNullOrEmpty(mapping.Aggregation.Condition))
            {
                for (int i = 0; i < arrayData.Count; i++)
                {
                    var arrayElement = arrayData[i];
                    if (arrayElement == null) 
                    {
                        continue;
                    }

                    // Create a context where 'item' refers to the current array element
                    var itemContextJson = $"{{\"item\":{arrayElement.ToJsonString()}}}";
                    var itemContext = JsonNode.Parse(itemContextJson);
                    
                    if (itemContext == null)
                    {
                        continue;
                    }

                    // Evaluate the condition using the ConditionEvaluator with complex expression support
                    bool conditionMet = false;
                    try
                    {
                        conditionMet = ConditionEvaluator.EvaluateExpression(mapping.Aggregation.Condition, itemContext);
                    }
                    catch (Exception ex)
                    {
                        if (_settings.EnableTracing)
                        {
                            Console.WriteLine($"Debug - Condition evaluation failed for element {i}: {ex.Message}");
                        }
                        continue;
                    }

                    if (conditionMet)
                    {
                        // Get the field value if specified, otherwise use the whole element
                        if (!string.IsNullOrEmpty(mapping.Aggregation.Field))
                        {
                            if (arrayElement is JsonObject obj && obj.TryGetPropertyValue(mapping.Aggregation.Field, out var fieldValue))
                            {
                                filteredValues.Add(fieldValue?.DeepClone());
                            }
                        }
                        else
                        {
                            filteredValues.Add(arrayElement.DeepClone());
                        }
                    }
                }
            }
            else
            {
                // No condition, use all elements
                for (int i = 0; i < arrayData.Count; i++)
                {
                    var arrayElement = arrayData[i];
                    if (arrayElement == null) continue;

                    if (!string.IsNullOrEmpty(mapping.Aggregation.Field))
                    {
                        if (arrayElement is JsonObject obj && obj.TryGetPropertyValue(mapping.Aggregation.Field, out var fieldValue))
                        {
                            filteredValues.Add(fieldValue?.DeepClone());
                        }
                    }
                    else
                    {
                        filteredValues.Add(arrayElement.DeepClone());
                    }
                }
            }

            // Perform aggregation on filtered values
            if (filteredValues.Count == 0)
            {
                return mapping.Aggregation.Type.ToLower() == "count" ? JsonValue.Create(0) : null;
            }

            var jsonArray = new JsonArray();
            foreach (var value in filteredValues)
            {
                jsonArray.Add(value);
            }

            var result = AggregationProcessor.Aggregate(jsonArray, mapping.Aggregation.Type);
            return result;
        }
        catch (Exception ex) when (!_settings.StrictMode)
        {
            if (_settings.EnableTracing)
            {
                Console.WriteLine($"Advanced aggregation failed for {mapping.From}: {ex.Message}");
            }
            return null;
        }
    }

    private static JsonNode? ConvertToJsonNode(object? value)
    {
        if (value == null)
            return null;

        return value switch
        {
            JsonNode node => node,
            string str => JsonValue.Create(str),
            bool boolean => JsonValue.Create(boolean),
            int integer => JsonValue.Create(integer),
            long longValue => JsonValue.Create(longValue),
            double doubleValue => JsonValue.Create(doubleValue),
            decimal decimalValue => JsonValue.Create(decimalValue),
            float floatValue => JsonValue.Create(floatValue),
            DateTime dateTime => JsonValue.Create(dateTime.ToString("o")),
            DateTimeOffset dateTimeOffset => JsonValue.Create(dateTimeOffset.ToString("o")),
            Guid guid => JsonValue.Create(guid.ToString()),
            _ => JsonValue.Create(value.ToString())
        };
    }

    private static string ExtractStringValue(JsonNode? node)
    {
        if (node == null)
            return "";

        return node switch
        {
            JsonValue value when value.TryGetValue<string>(out var str) => str,
            JsonValue value => value.ToString(),
            _ => node.ToString()
        };
    }
}
