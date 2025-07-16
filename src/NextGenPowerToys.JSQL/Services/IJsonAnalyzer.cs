using System.Text.Json.Nodes;
using NextGenPowerToys.JSQL.Models;
using Microsoft.Extensions.Logging;

namespace NextGenPowerToys.JSQL.Services
{
    /// <summary>
    /// Service for analyzing JSON structures and schemas
    /// </summary>
    public interface IJsonAnalyzer
    {
        /// <summary>
        /// Analyzes a JSON example to extract schema information
        /// </summary>
        /// <param name="jsonExample">The JSON example to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>JSON schema analysis result</returns>
        Task<JsonSchema> AnalyzeSchemaAsync(JsonNode jsonExample, CancellationToken cancellationToken = default);

        /// <summary>
        /// Discovers all available fields in a JSON structure
        /// </summary>
        /// <param name="jsonExample">The JSON example to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered fields with paths</returns>
        Task<List<JsonField>> DiscoverFieldsAsync(JsonNode jsonExample, CancellationToken cancellationToken = default);

        /// <summary>
        /// Infers data types for JSON fields
        /// </summary>
        /// <param name="jsonExample">The JSON example to analyze</param>
        /// <param name="fieldPath">The JSONPath to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Data type information</returns>
        Task<DataTypeInfo> InferDataTypeAsync(JsonNode jsonExample, string fieldPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates if a JSONPath exists in the given JSON structure
        /// </summary>
        /// <param name="jsonExample">The JSON example to check</param>
        /// <param name="jsonPath">The JSONPath to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if path exists, false otherwise</returns>
        Task<bool> ValidatePathAsync(JsonNode jsonExample, string jsonPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes array structures to determine aggregation capabilities
        /// </summary>
        /// <param name="jsonExample">The JSON example to analyze</param>
        /// <param name="arrayPath">Path to the array field</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Array analysis information</returns>
        Task<ArrayInfo> AnalyzeArrayAsync(JsonNode jsonExample, string arrayPath, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of JSON analyzer service
    /// </summary>
    public class JsonAnalyzer : IJsonAnalyzer
    {
        private readonly ILogger<JsonAnalyzer> _logger;

        public JsonAnalyzer(ILogger<JsonAnalyzer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<JsonSchema> AnalyzeSchemaAsync(JsonNode jsonExample, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Starting JSON schema analysis");

            try
            {
                var schema = new JsonSchema();
                var fields = await DiscoverFieldsAsync(jsonExample, cancellationToken);
                
                schema.Fields = fields;
                schema.RootType = DetermineRootType(jsonExample);
                schema.MaxDepth = CalculateMaxDepth(jsonExample);
                schema.ArrayFields = fields.Where(f => f.IsArray).ToList();
                schema.RequiredFields = DetermineRequiredFields(fields);
                schema.Complexity = CalculateComplexity(fields);

                _logger.LogDebug("JSON schema analysis completed. Found {FieldCount} fields", fields.Count);
                return schema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during JSON schema analysis");
                throw;
            }
        }

        public async Task<List<JsonField>> DiscoverFieldsAsync(JsonNode jsonExample, CancellationToken cancellationToken = default)
        {
            var fields = new List<JsonField>();
            await DiscoverFieldsRecursive(jsonExample, "$", fields, 0, cancellationToken);
            return fields;
        }

        public async Task<DataTypeInfo> InferDataTypeAsync(JsonNode jsonExample, string fieldPath, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = GetValueAtPath(jsonExample, fieldPath);
                return await Task.FromResult(InferTypeFromValue(value));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to infer data type for path {Path}", fieldPath);
                return new DataTypeInfo
                {
                    JsonType = "unknown",
                    SqlType = "NVARCHAR",
                    IsCompatible = false,
                    Confidence = 0.0
                };
            }
        }

        public async Task<bool> ValidatePathAsync(JsonNode jsonExample, string jsonPath, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = GetValueAtPath(jsonExample, jsonPath);
                return await Task.FromResult(value != null);
            }
            catch
            {
                return false;
            }
        }

        public async Task<ArrayInfo> AnalyzeArrayAsync(JsonNode jsonExample, string arrayPath, CancellationToken cancellationToken = default)
        {
            try
            {
                var arrayNode = GetValueAtPath(jsonExample, arrayPath) as JsonArray;
                if (arrayNode == null)
                {
                    throw new ArgumentException($"Path {arrayPath} does not point to an array");
                }

                var arrayInfo = new ArrayInfo
                {
                    Path = arrayPath,
                    ElementCount = arrayNode.Count,
                    ElementType = DetermineArrayElementType(arrayNode),
                    IsHomogeneous = IsHomogeneousArray(arrayNode),
                    SupportedAggregations = DetermineSupportedAggregations(arrayNode),
                    NestedArrays = FindNestedArrays(arrayNode, arrayPath)
                        .Select(path => new ArrayInfo { Path = path, Name = path.Split('.').LastOrDefault() ?? path })
                        .ToList()
                };

                return await Task.FromResult(arrayInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing array at path {Path}", arrayPath);
                throw;
            }
        }

        private async Task DiscoverFieldsRecursive(JsonNode? node, string currentPath, List<JsonField> fields, int depth, CancellationToken cancellationToken)
        {
            if (node == null || depth > 10) // Prevent infinite recursion
                return;

            cancellationToken.ThrowIfCancellationRequested();

            switch (node)
            {
                case JsonObject obj:
                    foreach (var property in obj)
                    {
                        var fieldPath = currentPath == "$" ? $"$.{property.Key}" : $"{currentPath}.{property.Key}";
                        var field = new JsonField
                        {
                            Name = property.Key,
                            Path = fieldPath,
                            Type = DetermineJsonType(property.Value),
                            IsArray = property.Value is JsonArray,
                            IsNullable = property.Value == null,
                            Depth = depth + 1
                        };

                        fields.Add(field);
                        await DiscoverFieldsRecursive(property.Value, fieldPath, fields, depth + 1, cancellationToken);
                    }
                    break;

                case JsonArray array:
                    if (array.Count > 0)
                    {
                        // Analyze first element as representative
                        await DiscoverFieldsRecursive(array[0], $"{currentPath}[0]", fields, depth + 1, cancellationToken);
                    }
                    break;
            }
        }

        private JsonNode? GetValueAtPath(JsonNode jsonExample, string path)
        {
            // Simplified JSONPath evaluation - in production, use JsonPath.Net
            var parts = path.Split('.', '[', ']').Where(p => !string.IsNullOrEmpty(p) && p != "$").ToArray();
            JsonNode? current = jsonExample;

            foreach (var part in parts)
            {
                if (int.TryParse(part, out int index))
                {
                    if (current is JsonArray array && index < array.Count)
                        current = array[index];
                    else
                        return null;
                }
                else
                {
                    if (current is JsonObject obj && obj.ContainsKey(part))
                        current = obj[part];
                    else
                        return null;
                }
            }

            return current;
        }

        private string DetermineRootType(JsonNode node)
        {
            return node switch
            {
                JsonObject => "object",
                JsonArray => "array",
                JsonValue value when value.TryGetValue<string>(out _) => "string",
                JsonValue value when value.TryGetValue<int>(out _) => "number",
                JsonValue value when value.TryGetValue<bool>(out _) => "boolean",
                _ => "unknown"
            };
        }

        private int CalculateMaxDepth(JsonNode node, int currentDepth = 0)
        {
            if (currentDepth > 20) return currentDepth; // Prevent stack overflow

            return node switch
            {
                JsonObject obj => obj.Count == 0 ? currentDepth : 
                    obj.Max(p => CalculateMaxDepth(p.Value, currentDepth + 1)),
                JsonArray array => array.Count == 0 ? currentDepth : 
                    array.Max(item => CalculateMaxDepth(item, currentDepth + 1)),
                _ => currentDepth
            };
        }

        private List<string> DetermineRequiredFields(List<JsonField> fields)
        {
            return fields.Where(f => !f.IsNullable && f.Depth == 1).Select(f => f.Name).ToList();
        }

        private double CalculateComplexity(List<JsonField> fields)
        {
            var baseComplexity = fields.Count * 0.1;
            var arrayComplexity = fields.Count(f => f.IsArray) * 0.5;
            var depthComplexity = fields.Max(f => f.Depth) * 0.3;

            return Math.Min(baseComplexity + arrayComplexity + depthComplexity, 10.0);
        }

        private string DetermineJsonType(JsonNode? node)
        {
            return node switch
            {
                null => "null",
                JsonObject => "object",
                JsonArray => "array",
                JsonValue value when value.TryGetValue<string>(out _) => "string",
                JsonValue value when value.TryGetValue<int>(out _) => "number",
                JsonValue value when value.TryGetValue<double>(out _) => "number",
                JsonValue value when value.TryGetValue<bool>(out _) => "boolean",
                _ => "unknown"
            };
        }

        private DataTypeInfo InferTypeFromValue(JsonNode? value)
        {
            return value switch
            {
                null => new DataTypeInfo { JsonType = "null", SqlType = "NULL", IsCompatible = true, Confidence = 1.0 },
                JsonValue jv when jv.TryGetValue<string>(out _) => new DataTypeInfo { JsonType = "string", SqlType = "NVARCHAR", IsCompatible = true, Confidence = 1.0 },
                JsonValue jv when jv.TryGetValue<int>(out _) => new DataTypeInfo { JsonType = "number", SqlType = "INT", IsCompatible = true, Confidence = 1.0 },
                JsonValue jv when jv.TryGetValue<double>(out _) => new DataTypeInfo { JsonType = "number", SqlType = "DECIMAL", IsCompatible = true, Confidence = 1.0 },
                JsonValue jv when jv.TryGetValue<bool>(out _) => new DataTypeInfo { JsonType = "boolean", SqlType = "BIT", IsCompatible = true, Confidence = 1.0 },
                JsonObject => new DataTypeInfo { JsonType = "object", SqlType = "NVARCHAR", IsCompatible = false, Confidence = 0.5 },
                JsonArray => new DataTypeInfo { JsonType = "array", SqlType = "TABLE", IsCompatible = false, Confidence = 0.5 },
                _ => new DataTypeInfo { JsonType = "unknown", SqlType = "NVARCHAR", IsCompatible = false, Confidence = 0.0 }
            };
        }

        private string DetermineArrayElementType(JsonArray array)
        {
            if (array.Count == 0) return "unknown";

            var firstElement = array[0];
            return DetermineJsonType(firstElement);
        }

        private bool IsHomogeneousArray(JsonArray array)
        {
            if (array.Count <= 1) return true;

            var firstType = DetermineJsonType(array[0]);
            return array.All(item => DetermineJsonType(item) == firstType);
        }

        private List<string> DetermineSupportedAggregations(JsonArray array)
        {
            var aggregations = new List<string> { "COUNT" };

            if (array.Count == 0) return aggregations;

            var elementType = DetermineArrayElementType(array);
            
            if (elementType == "number")
            {
                aggregations.AddRange(new[] { "SUM", "AVG", "MIN", "MAX" });
            }
            else if (elementType == "string")
            {
                aggregations.AddRange(new[] { "MIN", "MAX" });
            }

            return aggregations;
        }

        private List<string> FindNestedArrays(JsonArray array, string basePath)
        {
            var nestedArrays = new List<string>();

            for (int i = 0; i < Math.Min(array.Count, 3); i++) // Check first 3 elements
            {
                if (array[i] is JsonObject obj)
                {
                    foreach (var property in obj)
                    {
                        if (property.Value is JsonArray)
                        {
                            nestedArrays.Add($"{basePath}[*].{property.Key}");
                        }
                    }
                }
            }

            return nestedArrays.Distinct().ToList();
        }
    }
}
