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
            // Aggregation operation
            value = ProcessAggregation(sourceData, mapping);
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

        // Apply conditional logic if present
        if (mapping.Conditions != null && mapping.Conditions.Count > 0)
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
