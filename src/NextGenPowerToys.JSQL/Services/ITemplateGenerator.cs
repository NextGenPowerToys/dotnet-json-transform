using System.Text.Json.Nodes;
using NextGenPowerToys.JSQL.Models;
using Microsoft.Extensions.Logging;

namespace NextGenPowerToys.JSQL.Services
{
    /// <summary>
    /// Service for generating Json.Transform templates from SQL queries and JSON examples
    /// </summary>
    public interface ITemplateGenerator
    {
        /// <summary>
        /// Generates a Json.Transform template from SQL analysis and JSON schema
        /// </summary>
        /// <param name="sqlAnalysis">Analyzed SQL query structure</param>
        /// <param name="jsonSchema">Analyzed JSON schema</param>
        /// <param name="jsonExample">Original JSON example</param>
        /// <param name="fieldMappings">Validated field mappings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated template or null if generation fails</returns>
        Task<JsonNode?> GenerateTemplateAsync(
            SqlAnalysisResult sqlAnalysis,
            JsonSchema jsonSchema,
            JsonNode jsonExample,
            List<FieldMapping> fieldMappings,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates transformations for field selections
        /// </summary>
        /// <param name="sqlFields">SQL fields to select</param>
        /// <param name="fieldMappings">Field mappings between SQL and JSON</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Field transformation template</returns>
        Task<JsonNode> GenerateFieldSelectionsAsync(
            List<SqlField> sqlFields,
            List<FieldMapping> fieldMappings,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates transformations for aggregation operations
        /// </summary>
        /// <param name="aggregates">SQL aggregates to transform</param>
        /// <param name="arrayFields">Available array fields in JSON</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Aggregation transformation template</returns>
        Task<JsonNode?> GenerateAggregationsAsync(
            List<SqlAggregate> aggregates,
            List<JsonField> arrayFields,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates transformations for WHERE conditions
        /// </summary>
        /// <param name="conditions">SQL conditions to transform</param>
        /// <param name="fieldMappings">Field mappings between SQL and JSON</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Condition transformation template</returns>
        Task<JsonNode?> GenerateConditionsAsync(
            List<SqlCondition> conditions,
            List<FieldMapping> fieldMappings,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates transformations for ORDER BY clauses
        /// </summary>
        /// <param name="orderByFields">Fields to order by</param>
        /// <param name="fieldMappings">Field mappings between SQL and JSON</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Ordering transformation template</returns>
        Task<JsonNode?> GenerateOrderingAsync(
            List<string> orderByFields,
            List<FieldMapping> fieldMappings,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Optimizes the generated template for performance
        /// </summary>
        /// <param name="template">Template to optimize</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Optimized template and list of applied optimizations</returns>
        Task<(JsonNode optimizedTemplate, List<string> optimizations)> OptimizeTemplateAsync(
            JsonNode template,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates that the generated template is compatible with Json.Transform
        /// </summary>
        /// <param name="template">Template to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if template is valid, false otherwise</returns>
        Task<bool> ValidateTemplateAsync(
            JsonNode template,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of template generator service
    /// </summary>
    public class TemplateGenerator : ITemplateGenerator
    {
        private readonly ILogger<TemplateGenerator> _logger;

        // Json.Transform operation mappings
        private static readonly Dictionary<string, string> AggregateOperations = new()
        {
            { "COUNT", "count" },
            { "SUM", "sum" },
            { "AVG", "avg" },
            { "MIN", "min" },
            { "MAX", "max" }
        };

        private static readonly Dictionary<string, string> ComparisonOperators = new()
        {
            { "=", "eq" },
            { "!=", "ne" },
            { "<>", "ne" },
            { "<", "lt" },
            { "<=", "le" },
            { ">", "gt" },
            { ">=", "ge" },
            { "LIKE", "contains" },
            { "IN", "in" },
            { "IS NULL", "is_null" },
            { "IS NOT NULL", "is_not_null" }
        };

        public TemplateGenerator(ILogger<TemplateGenerator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<JsonNode?> GenerateTemplateAsync(
            SqlAnalysisResult sqlAnalysis,
            JsonSchema jsonSchema,
            JsonNode jsonExample,
            List<FieldMapping> fieldMappings,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Starting template generation for query type: {QueryType}", sqlAnalysis.QueryType);

            try
            {
                if (sqlAnalysis.QueryType != SqlQueryType.Select)
                {
                    _logger.LogWarning("Only SELECT queries are supported for template generation");
                    return null;
                }

                var template = new JsonObject();

                // Generate field selections
                var fieldSelections = await GenerateFieldSelectionsAsync(
                    sqlAnalysis.SelectFields, fieldMappings, cancellationToken);
                template["select"] = fieldSelections;

                // Generate conditions if present
                if (sqlAnalysis.WhereConditions.Count > 0)
                {
                    var conditions = await GenerateConditionsAsync(
                        sqlAnalysis.WhereConditions, fieldMappings, cancellationToken);
                    if (conditions != null)
                        template["where"] = conditions;
                }

                // Generate aggregations if present
                if (sqlAnalysis.Aggregates.Count > 0)
                {
                    var aggregations = await GenerateAggregationsAsync(
                        sqlAnalysis.Aggregates, jsonSchema.ArrayFields, cancellationToken);
                    if (aggregations != null)
                        template["aggregate"] = aggregations;
                }

                // Generate ordering if present
                if (sqlAnalysis.OrderByFields.Count > 0)
                {
                    var ordering = await GenerateOrderingAsync(
                        sqlAnalysis.OrderByFields.Select(o => o.Field).ToList(), fieldMappings, cancellationToken);
                    if (ordering != null)
                        template["orderBy"] = ordering;
                }

                // Add GROUP BY if present
                if (sqlAnalysis.GroupByFields.Count > 0)
                {
                    template["groupBy"] = JsonValue.Create(string.Join(",", sqlAnalysis.GroupByFields.Select(g => g.Name)));
                }

                // Validate and optimize the template
                if (await ValidateTemplateAsync(template, cancellationToken))
                {
                    var (optimizedTemplate, optimizations) = await OptimizeTemplateAsync(template, cancellationToken);
                    
                    _logger.LogDebug("Template generation completed with {OptimizationCount} optimizations", 
                        optimizations.Count);
                    
                    return optimizedTemplate;
                }

                _logger.LogWarning("Generated template failed validation");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during template generation");
                return null;
            }
        }

        public async Task<JsonNode> GenerateFieldSelectionsAsync(
            List<SqlField> sqlFields,
            List<FieldMapping> fieldMappings,
            CancellationToken cancellationToken = default)
        {
            var selections = new JsonObject();

            foreach (var sqlField in sqlFields)
            {
                var mapping = fieldMappings.FirstOrDefault(fm => fm.SqlField == sqlField.Name);
                
                if (mapping?.IsCompatible == true && mapping.JsonPath != null)
                {
                    var outputName = sqlField.Alias ?? sqlField.Name;
                    
                    if (sqlField.IsCalculated)
                    {
                        // Handle calculated fields
                        selections[outputName] = GenerateCalculatedFieldTransform(sqlField, mapping);
                    }
                    else if (sqlField.IsAggregate)
                    {
                        // Handle aggregate fields
                        selections[outputName] = GenerateAggregateFieldTransform(sqlField, mapping);
                    }
                    else
                    {
                        // Simple field mapping
                        selections[outputName] = JsonValue.Create(mapping.JsonPath);
                    }
                }
                else
                {
                    // Field not found or incompatible - use null or default
                    var outputName = sqlField.Alias ?? sqlField.Name;
                    selections[outputName] = JsonValue.Create<string?>(null);
                }
            }

            return await Task.FromResult(selections);
        }

        public async Task<JsonNode?> GenerateAggregationsAsync(
            List<SqlAggregate> aggregates,
            List<JsonField> arrayFields,
            CancellationToken cancellationToken = default)
        {
            if (aggregates.Count == 0)
                return null;

            var aggregations = new JsonObject();

            foreach (var aggregate in aggregates)
            {
                var targetField = arrayFields.FirstOrDefault(af => 
                    af.Name.Equals(aggregate.Field, StringComparison.OrdinalIgnoreCase));

                if (targetField != null && AggregateOperations.ContainsKey(aggregate.Function))
                {
                    var operation = AggregateOperations[aggregate.Function];
                    var aggregateKey = $"{aggregate.Function.ToLower()}_{aggregate.Field}";
                    
                    aggregations[aggregateKey] = new JsonObject
                    {
                        ["operation"] = JsonValue.Create(operation),
                        ["field"] = JsonValue.Create(targetField.Path),
                        ["target"] = JsonValue.Create(aggregate.Field)
                    };
                }
            }

            return await Task.FromResult(aggregations.Count > 0 ? aggregations : null);
        }

        public async Task<JsonNode?> GenerateConditionsAsync(
            List<SqlCondition> conditions,
            List<FieldMapping> fieldMappings,
            CancellationToken cancellationToken = default)
        {
            if (conditions.Count == 0)
                return null;

            var conditionsArray = new JsonArray();

            foreach (var condition in conditions)
            {
                var mapping = fieldMappings.FirstOrDefault(fm => 
                    fm.SqlField.Equals(condition.Field, StringComparison.OrdinalIgnoreCase));

                if (mapping?.IsCompatible == true && mapping.JsonPath != null)
                {
                    var conditionObj = new JsonObject
                    {
                        ["field"] = JsonValue.Create(mapping.JsonPath),
                        ["operator"] = JsonValue.Create(MapOperator(condition.Operator)),
                        ["value"] = CreateValueNode(condition.RightValue)
                    };

                    conditionsArray.Add(conditionObj);
                }
            }

            return await Task.FromResult(conditionsArray.Count > 0 ? conditionsArray : null);
        }

        public async Task<JsonNode?> GenerateOrderingAsync(
            List<string> orderByFields,
            List<FieldMapping> fieldMappings,
            CancellationToken cancellationToken = default)
        {
            if (orderByFields.Count == 0)
                return null;

            var orderArray = new JsonArray();

            foreach (var field in orderByFields)
            {
                var fieldName = ExtractFieldName(field);
                var direction = ExtractDirection(field);
                
                var mapping = fieldMappings.FirstOrDefault(fm => 
                    fm.SqlField.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                if (mapping?.IsCompatible == true && mapping.JsonPath != null)
                {
                    var orderObj = new JsonObject
                    {
                        ["field"] = JsonValue.Create(mapping.JsonPath),
                        ["direction"] = JsonValue.Create(direction.ToLowerInvariant())
                    };

                    orderArray.Add(orderObj);
                }
            }

            return await Task.FromResult(orderArray.Count > 0 ? orderArray : null);
        }

        public async Task<(JsonNode optimizedTemplate, List<string> optimizations)> OptimizeTemplateAsync(
            JsonNode template,
            CancellationToken cancellationToken = default)
        {
            var optimizations = new List<string>();
            var optimizedTemplate = template.DeepClone();

            try
            {
                // Optimization 1: Combine similar field selections
                if (optimizedTemplate is JsonObject templateObj && templateObj["select"] is JsonObject selectObj)
                {
                    var combinedSelections = CombineSimilarSelections(selectObj);
                    if (combinedSelections.Count < selectObj.Count)
                    {
                        templateObj["select"] = combinedSelections;
                        optimizations.Add("Combined similar field selections");
                    }
                }

                // Optimization 2: Simplify conditions
                if (optimizedTemplate is JsonObject templateObj2 && templateObj2["where"] is JsonArray whereArray)
                {
                    var simplifiedConditions = SimplifyConditions(whereArray);
                    if (simplifiedConditions.Count < whereArray.Count)
                    {
                        templateObj2["where"] = simplifiedConditions;
                        optimizations.Add("Simplified condition logic");
                    }
                }

                // Optimization 3: Remove redundant aggregations
                if (optimizedTemplate is JsonObject templateObj3 && templateObj3["aggregate"] is JsonObject aggObj)
                {
                    var optimizedAggregations = RemoveRedundantAggregations(aggObj);
                    if (optimizedAggregations.Count < aggObj.Count)
                    {
                        templateObj3["aggregate"] = optimizedAggregations;
                        optimizations.Add("Removed redundant aggregations");
                    }
                }

                // Optimization 4: Optimize field paths
                OptimizeFieldPaths(optimizedTemplate, optimizations);

                _logger.LogDebug("Applied {OptimizationCount} optimizations to template", optimizations.Count);
                
                return await Task.FromResult((optimizedTemplate, optimizations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during template optimization");
                return (template, optimizations);
            }
        }

        public async Task<bool> ValidateTemplateAsync(
            JsonNode template,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (template is not JsonObject templateObj)
                    return false;

                // Validate required structure
                if (!templateObj.ContainsKey("select"))
                    return false;

                // Validate select clause
                if (templateObj["select"] is not JsonObject selectObj || selectObj.Count == 0)
                    return false;

                // Validate conditions format if present
                if (templateObj.ContainsKey("where"))
                {
                    if (templateObj["where"] is not JsonArray whereArray)
                        return false;
                        
                    foreach (var condition in whereArray)
                    {
                        if (!ValidateConditionStructure(condition))
                            return false;
                    }
                }

                // Validate aggregations format if present
                if (templateObj.ContainsKey("aggregate"))
                {
                    if (templateObj["aggregate"] is not JsonObject)
                        return false;
                }

                // Validate ordering format if present
                if (templateObj.ContainsKey("orderBy"))
                {
                    if (templateObj["orderBy"] is not JsonArray)
                        return false;
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating template");
                return false;
            }
        }

        #region Private Helper Methods

        private JsonNode GenerateCalculatedFieldTransform(SqlField sqlField, FieldMapping mapping)
        {
            // For calculated fields, create a transform object
            var transform = new JsonObject
            {
                ["source"] = JsonValue.Create(mapping.JsonPath),
                ["expression"] = JsonValue.Create(sqlField.Expression)
            };

            // Add type conversion if needed
            if (!string.IsNullOrEmpty(mapping.ConversionRequired))
            {
                transform["convert"] = JsonValue.Create(mapping.TypeInfo?.SqlType?.ToLowerInvariant());
            }

            return transform;
        }

        private JsonNode GenerateAggregateFieldTransform(SqlField sqlField, FieldMapping mapping)
        {
            // Extract aggregate function from expression
            var functionMatch = System.Text.RegularExpressions.Regex.Match(
                sqlField.Expression?.RawExpression ?? "", @"([A-Z_]+)\s*\(", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (functionMatch.Success && AggregateOperations.ContainsKey(functionMatch.Groups[1].Value.ToUpperInvariant()))
            {
                var operation = AggregateOperations[functionMatch.Groups[1].Value.ToUpperInvariant()];
                
                return new JsonObject
                {
                    ["operation"] = JsonValue.Create(operation),
                    ["field"] = JsonValue.Create(mapping.JsonPath)
                };
            }

            // Fallback to simple mapping
            return JsonValue.Create(mapping.JsonPath);
        }

        private string MapOperator(SqlOperator sqlOperator)
        {
            return ComparisonOperators.GetValueOrDefault(sqlOperator.ToString().ToUpperInvariant(), "eq");
        }

        private JsonNode CreateValueNode(object? value)
        {
            if (value == null)
                return JsonValue.Create((string?)null);
                
            var valueStr = value.ToString();
            if (valueStr == null)
                return JsonValue.Create((string?)null);
                
            // Try to parse as different types
            if (int.TryParse(valueStr, out int intValue))
                return JsonValue.Create(intValue);
                
            if (double.TryParse(valueStr, out double doubleValue))
                return JsonValue.Create(doubleValue);
                
            if (bool.TryParse(valueStr, out bool boolValue))
                return JsonValue.Create(boolValue);
                
            if (valueStr.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return JsonValue.Create<string?>(null);

            // Remove quotes if present
            if (valueStr.StartsWith("'") && valueStr.EndsWith("'"))
                valueStr = valueStr.Substring(1, valueStr.Length - 2);

            return JsonValue.Create(valueStr);
        }

        private string ExtractFieldName(string orderByField)
        {
            var parts = orderByField.Split(' ');
            return parts[0].Trim();
        }

        private string ExtractDirection(string orderByField)
        {
            var parts = orderByField.Split(' ');
            return parts.Length > 1 && parts[1].ToUpperInvariant() == "DESC" ? "DESC" : "ASC";
        }

        private JsonObject CombineSimilarSelections(JsonObject selectObj)
        {
            // For now, return as-is. In production, implement logic to combine similar field patterns
            return selectObj;
        }

        private JsonArray SimplifyConditions(JsonArray whereArray)
        {
            // For now, return as-is. In production, implement logic to simplify condition trees
            return whereArray;
        }

        private JsonObject RemoveRedundantAggregations(JsonObject aggObj)
        {
            // For now, return as-is. In production, implement logic to remove duplicate aggregations
            return aggObj;
        }

        private void OptimizeFieldPaths(JsonNode template, List<string> optimizations)
        {
            // For now, no path optimization. In production, implement JSONPath optimization
        }

        private bool ValidateConditionStructure(JsonNode? condition)
        {
            if (condition is not JsonObject condObj)
                return false;

            return condObj.ContainsKey("field") && 
                   condObj.ContainsKey("operator") && 
                   condObj.ContainsKey("value");
        }

        #endregion
    }
}
