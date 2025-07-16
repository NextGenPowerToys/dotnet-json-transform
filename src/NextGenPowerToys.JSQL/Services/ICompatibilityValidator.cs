using NextGenPowerToys.JSQL.Models;
using Microsoft.Extensions.Logging;

namespace NextGenPowerToys.JSQL.Services
{
    /// <summary>
    /// Service for validating compatibility between SQL queries and JSON structures
    /// </summary>
    public interface ICompatibilityValidator
    {
        /// <summary>
        /// Validates compatibility between SQL query and JSON structure
        /// </summary>
        /// <param name="sqlAnalysis">Analyzed SQL query structure</param>
        /// <param name="jsonSchema">Analyzed JSON schema</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Compatibility validation result</returns>
        Task<ValidationResult> ValidateCompatibilityAsync(
            SqlAnalysisResult sqlAnalysis, 
            JsonSchema jsonSchema, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates individual field mappings between SQL and JSON
        /// </summary>
        /// <param name="sqlFields">SQL fields from query</param>
        /// <param name="jsonFields">Available JSON fields</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Field mapping validation results</returns>
        Task<List<FieldMapping>> ValidateFieldMappingsAsync(
            List<SqlField> sqlFields, 
            List<JsonField> jsonFields, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates SQL function compatibility with JSON transformation
        /// </summary>
        /// <param name="sqlFunctions">SQL functions used in query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Function compatibility results</returns>
        Task<Dictionary<string, bool>> ValidateFunctionCompatibilityAsync(
            List<SqlAggregate> sqlFunctions, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates aggregation operations against JSON array structures
        /// </summary>
        /// <param name="aggregates">SQL aggregates from query</param>
        /// <param name="arrayFields">Available array fields in JSON</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Aggregation validation results</returns>
        Task<List<AggregationValidation>> ValidateAggregationsAsync(
            List<SqlAggregate> aggregates, 
            List<JsonField> arrayFields, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates error messages for incompatible elements
        /// </summary>
        /// <param name="compatibilityReport">Compatibility assessment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of error messages with suggestions</returns>
        Task<List<AnalysisError>> GenerateErrorMessagesAsync(
            CompatibilityReport compatibilityReport, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates alternative suggestions for incompatible queries
        /// </summary>
        /// <param name="sqlAnalysis">SQL analysis result</param>
        /// <param name="jsonSchema">JSON schema</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Alternative options</returns>
        Task<AlternativeOptions> GenerateAlternativesAsync(
            SqlAnalysisResult sqlAnalysis, 
            JsonSchema jsonSchema, 
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Aggregation validation result
    /// </summary>
    public class AggregationValidation
    {
        /// <summary>
        /// The SQL aggregate being validated
        /// </summary>
        public SqlAggregate Aggregate { get; set; } = new();

        /// <summary>
        /// Whether the aggregation is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Target JSON field for aggregation
        /// </summary>
        public JsonField? TargetField { get; set; }

        /// <summary>
        /// Error message if validation fails
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Suggested alternative approach
        /// </summary>
        public string? Suggestion { get; set; }
    }

    /// <summary>
    /// Default implementation of compatibility validator service
    /// </summary>
    public class CompatibilityValidator : ICompatibilityValidator
    {
        private readonly ILogger<CompatibilityValidator> _logger;

        // Supported data type conversions
        private static readonly Dictionary<(string SqlType, string JsonType), double> TypeCompatibility = new()
        {
            // Exact matches
            { ("NVARCHAR", "string"), 1.0 },
            { ("VARCHAR", "string"), 1.0 },
            { ("INT", "number"), 1.0 },
            { ("DECIMAL", "number"), 1.0 },
            { ("FLOAT", "number"), 1.0 },
            { ("BIT", "boolean"), 1.0 },
            
            // Compatible conversions
            { ("NVARCHAR", "number"), 0.8 }, // String to number conversion possible
            { ("NVARCHAR", "boolean"), 0.7 }, // String to boolean conversion possible
            { ("INT", "string"), 0.9 }, // Number to string is safe
            { ("DECIMAL", "string"), 0.9 },
            { ("BIT", "string"), 0.9 },
            
            // Risky conversions
            { ("INT", "boolean"), 0.5 }, // 0/1 to boolean
            { ("DECIMAL", "boolean"), 0.3 }, // Risky conversion
            
            // Incompatible
            { ("TABLE", "string"), 0.0 }, // Cannot convert complex types
            { ("TABLE", "number"), 0.0 },
            { ("TABLE", "boolean"), 0.0 }
        };

        public CompatibilityValidator(ILogger<CompatibilityValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ValidationResult> ValidateCompatibilityAsync(
            SqlAnalysisResult sqlAnalysis, 
            JsonSchema jsonSchema, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Starting compatibility validation");

            try
            {
                var report = new CompatibilityReport();

                // Validate field mappings
                report.FieldMappings = await ValidateFieldMappingsAsync(
                    sqlAnalysis.Fields, jsonSchema.Fields, cancellationToken);

                // Validate function compatibility
                report.FeatureSupport = await ValidateFunctionCompatibilityAsync(
                    sqlAnalysis.Aggregates, cancellationToken);

                // Add SQL feature compatibility
                await AddSqlFeatureCompatibility(report, sqlAnalysis);

                // Calculate overall compatibility score
                report.Score = CalculateCompatibilityScore(report);

                // Set complexity assessment
                report.Complexity = new ComplexityAssessment 
                { 
                    OverallComplexity = sqlAnalysis.Complexity 
                };

                // Convert to ValidationResult
                var result = new ValidationResult
                {
                    Score = report.Score,
                    IsValid = report.Score >= 0.7, // Consider valid if score is 70% or higher
                    FieldMappings = report.FieldMappings,
                    Errors = new List<ValidationError>(),
                    Warnings = new List<ValidationWarning>()
                };

                // Add errors and warnings based on compatibility issues
                foreach (var mapping in report.FieldMappings.Where(m => !m.IsCompatible))
                {
                    result.Errors.Add(new ValidationError
                    {
                        SqlElement = mapping.SqlField,
                        Issue = $"Field '{mapping.SqlField}' cannot be mapped to JSON structure",
                        Severity = ErrorSeverity.Error
                    });
                }

                foreach (var feature in report.FeatureSupport.Where(f => !f.Value))
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        Message = $"SQL feature '{feature.Key}' may not be fully supported",
                        Recommendation = "Consider using alternative approaches"
                    });
                }

                _logger.LogDebug("Compatibility validation completed. Score: {Score}, IsValid: {IsValid}", result.Score, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during compatibility validation");
                throw;
            }
        }

        public async Task<List<FieldMapping>> ValidateFieldMappingsAsync(
            List<SqlField> sqlFields, 
            List<JsonField> jsonFields, 
            CancellationToken cancellationToken = default)
        {
            var mappings = new List<FieldMapping>();

            foreach (var sqlField in sqlFields)
            {
                var mapping = new FieldMapping
                {
                    SqlField = sqlField.Name
                };

                // Find matching JSON field
                var jsonField = FindMatchingJsonField(sqlField, jsonFields);
                
                if (jsonField != null)
                {
                    mapping.JsonPath = jsonField.Path;
                    mapping.TypeInfo = CreateDataTypeInfo(sqlField, jsonField);
                    mapping.IsCompatible = mapping.TypeInfo.IsCompatible;
                    
                    if (!mapping.IsCompatible)
                    {
                        mapping.ConversionRequired = DetermineRequiredConversion(sqlField, jsonField);
                    }
                }
                else
                {
                    mapping.IsCompatible = false;
                    mapping.TypeInfo = new DataTypeInfo
                    {
                        SqlType = sqlField.DataType,
                        JsonType = "not found",
                        IsCompatible = false,
                        Confidence = 0.0
                    };
                }

                mappings.Add(mapping);
            }

            return await Task.FromResult(mappings);
        }

        public async Task<Dictionary<string, bool>> ValidateFunctionCompatibilityAsync(
            List<SqlAggregate> sqlFunctions, 
            CancellationToken cancellationToken = default)
        {
            var compatibility = new Dictionary<string, bool>();

            foreach (var function in sqlFunctions)
            {
                // Determine if the function type is supported
                bool isSupported = function.Type switch
                {
                    SqlAggregateType.Count => true,
                    SqlAggregateType.Sum => true,
                    SqlAggregateType.Avg => true,
                    SqlAggregateType.Min => true,
                    SqlAggregateType.Max => true,
                    _ => false
                };
                
                compatibility[function.Function] = isSupported;
            }

            return await Task.FromResult(compatibility);
        }

        public async Task<List<AggregationValidation>> ValidateAggregationsAsync(
            List<SqlAggregate> aggregates, 
            List<JsonField> arrayFields, 
            CancellationToken cancellationToken = default)
        {
            var validations = new List<AggregationValidation>();

            foreach (var aggregate in aggregates)
            {
                var validation = new AggregationValidation
                {
                    Aggregate = aggregate
                };

                // Find target array field
                var targetField = FindTargetArrayField(aggregate, arrayFields);
                
                if (targetField != null)
                {
                    validation.TargetField = targetField;
                    validation.IsValid = ValidateAggregateOperation(aggregate, targetField);
                    
                    if (!validation.IsValid)
                    {
                        validation.ErrorMessage = $"Cannot perform {aggregate.Function} on {targetField.Type} field";
                        validation.Suggestion = GenerateAggregationSuggestion(aggregate, targetField);
                    }
                }
                else
                {
                    validation.IsValid = false;
                    validation.ErrorMessage = $"No suitable array field found for aggregation {aggregate.Function}";
                    validation.Suggestion = "Ensure the JSON contains array fields for aggregation operations";
                }

                validations.Add(validation);
            }

            return await Task.FromResult(validations);
        }

        public async Task<List<AnalysisError>> GenerateErrorMessagesAsync(
            CompatibilityReport compatibilityReport, 
            CancellationToken cancellationToken = default)
        {
            var errors = new List<AnalysisError>();

            // Generate errors for incompatible field mappings
            foreach (var mapping in compatibilityReport.FieldMappings.Where(m => !m.IsCompatible))
            {
                var errorType = mapping.JsonPath == null ? ErrorType.FieldNotFound : ErrorType.TypeMismatch;
                
                var error = new AnalysisError
                {
                    Type = errorType,
                    Message = GenerateFieldMappingErrorMessage(mapping),
                    SqlPart = mapping.SqlField,
                    JsonPath = mapping.JsonPath,
                    Suggestion = GenerateFieldMappingSuggestion(mapping),
                    Severity = ErrorSeverity.Error
                };

                errors.Add(error);
            }

            // Generate errors for unsupported functions
            foreach (var function in compatibilityReport.FeatureSupport.Where(f => !f.Value))
            {
                var error = new AnalysisError
                {
                    Type = ErrorType.UnsupportedSqlFunction,
                    Message = $"SQL function '{function.Key}' is not supported in JSON transformations",
                    SqlPart = function.Key,
                    Suggestion = GetFunctionAlternativeSuggestion(function.Key),
                    Severity = ErrorSeverity.Error
                };

                errors.Add(error);
            }

            return await Task.FromResult(errors);
        }

        public async Task<AlternativeOptions> GenerateAlternativesAsync(
            SqlAnalysisResult sqlAnalysis, 
            JsonSchema jsonSchema, 
            CancellationToken cancellationToken = default)
        {
            var alternatives = new AlternativeOptions();

            // Generate general suggestions
            alternatives.Suggestions = GenerateGeneralSuggestions(sqlAnalysis, jsonSchema);

            // Generate simplified query if possible
            alternatives.SimplifiedQuery = GenerateSimplifiedQuery(sqlAnalysis);

            // Generate required JSON changes
            alternatives.RequiredJsonChanges = GenerateRequiredJsonChanges(sqlAnalysis, jsonSchema);

            return await Task.FromResult(alternatives);
        }

        #region Private Helper Methods

        private JsonField? FindMatchingJsonField(SqlField sqlField, List<JsonField> jsonFields)
        {
            // Direct name match
            var directMatch = jsonFields.FirstOrDefault(jf => 
                string.Equals(jf.Name, sqlField.Name, StringComparison.OrdinalIgnoreCase));
            
            if (directMatch != null) return directMatch;

            // Remove table prefix and try again
            if (sqlField.TableReference != null)
            {
                var fieldNameOnly = sqlField.Name.Split('.').Last();
                var prefixMatch = jsonFields.FirstOrDefault(jf => 
                    string.Equals(jf.Name, fieldNameOnly, StringComparison.OrdinalIgnoreCase));
                
                if (prefixMatch != null) return prefixMatch;
            }

            // Fuzzy matching for common variations
            return jsonFields.FirstOrDefault(jf => 
                IsFieldNameSimilar(jf.Name, sqlField.Name));
        }

        private bool IsFieldNameSimilar(string jsonFieldName, string sqlFieldName)
        {
            // Handle common naming conventions
            var jsonNormalized = jsonFieldName.ToLowerInvariant().Replace("_", "").Replace("-", "");
            var sqlNormalized = sqlFieldName.ToLowerInvariant().Replace("_", "").Replace("-", "");

            return jsonNormalized == sqlNormalized;
        }

        private DataTypeInfo CreateDataTypeInfo(SqlField sqlField, JsonField jsonField)
        {
            var compatibility = GetTypeCompatibility(sqlField.DataType, jsonField.Type);
            
            return new DataTypeInfo
            {
                SqlType = sqlField.DataType,
                JsonType = jsonField.Type,
                IsCompatible = compatibility > 0.7,
                Confidence = compatibility
            };
        }

        private double GetTypeCompatibility(string sqlType, string jsonType)
        {
            var key = (sqlType.ToUpperInvariant(), jsonType.ToLowerInvariant());
            return TypeCompatibility.GetValueOrDefault(key, 0.0);
        }

        private string? DetermineRequiredConversion(SqlField sqlField, JsonField jsonField)
        {
            var compatibility = GetTypeCompatibility(sqlField.DataType, jsonField.Type);
            
            if (compatibility == 0.0)
                return null; // No conversion possible
            
            if (compatibility < 1.0)
                return $"Convert {jsonField.Type} to {sqlField.DataType}";
                
            return null; // No conversion needed
        }

        private async Task AddSqlFeatureCompatibility(CompatibilityReport report, SqlAnalysisResult sqlAnalysis)
        {
            // Add compatibility for various SQL features
            report.FeatureSupport["JOIN"] = sqlAnalysis.Joins.Count == 0;
            report.FeatureSupport["SUBQUERY"] = !ContainsSubqueries(sqlAnalysis.OriginalQuery);
            report.FeatureSupport["WINDOW_FUNCTIONS"] = !ContainsWindowFunctions(sqlAnalysis.OriginalQuery);
            report.FeatureSupport["COMPLEX_CONDITIONS"] = sqlAnalysis.Conditions.Count <= 5;

            await Task.CompletedTask;
        }

        private bool ContainsSubqueries(string sqlQuery)
        {
            // Simplified subquery detection
            var openParens = sqlQuery.Count(c => c == '(');
            var selectCount = System.Text.RegularExpressions.Regex.Matches(
                sqlQuery, @"\bSELECT\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
            
            return selectCount > 1 && openParens > 0;
        }

        private bool ContainsWindowFunctions(string sqlQuery)
        {
            return sqlQuery.ToUpperInvariant().Contains("OVER(") || 
                   sqlQuery.ToUpperInvariant().Contains("OVER (");
        }

        private double CalculateCompatibilityScore(CompatibilityReport report)
        {
            var totalFields = report.FieldMappings.Count;
            if (totalFields == 0) return 1.0;

            var compatibleFields = report.FieldMappings.Count(fm => fm.IsCompatible);
            var fieldScore = (double)compatibleFields / totalFields;

            var supportedFeatures = report.FeatureSupport.Count(fs => fs.Value);
            var totalFeatures = Math.Max(report.FeatureSupport.Count, 1);
            var featureScore = (double)supportedFeatures / totalFeatures;

            // Weighted average: 70% field compatibility, 30% feature compatibility
            return (fieldScore * 0.7) + (featureScore * 0.3);
        }

        private JsonField? FindTargetArrayField(SqlAggregate aggregate, List<JsonField> arrayFields)
        {
            return arrayFields.FirstOrDefault(af => 
                string.Equals(af.Name, aggregate.Field, StringComparison.OrdinalIgnoreCase) ||
                af.Path.Contains(aggregate.Field));
        }

        private bool ValidateAggregateOperation(SqlAggregate aggregate, JsonField targetField)
        {
            if (!targetField.IsArray)
                return false;

            return aggregate.Function.ToUpperInvariant() switch
            {
                "COUNT" => true, // COUNT works on any array
                "SUM" or "AVG" => targetField.Type == "number",
                "MIN" or "MAX" => targetField.Type == "number" || targetField.Type == "string",
                _ => false
            };
        }

        private string GenerateAggregationSuggestion(SqlAggregate aggregate, JsonField targetField)
        {
            if (!targetField.IsArray)
                return $"Field '{targetField.Name}' must be an array for aggregation operations";

            return aggregate.Function.ToUpperInvariant() switch
            {
                "SUM" or "AVG" when targetField.Type != "number" => 
                    $"Use COUNT instead of {aggregate.Function} for non-numeric arrays",
                "MIN" or "MAX" when targetField.Type == "boolean" => 
                    "MIN/MAX operations are not supported on boolean arrays",
                _ => "Consider using a different aggregation function"
            };
        }

        private string GenerateFieldMappingErrorMessage(FieldMapping mapping)
        {
            if (mapping.JsonPath == null)
                return $"SQL field '{mapping.SqlField}' not found in JSON structure";

            return $"Data type mismatch: SQL field '{mapping.SqlField}' ({mapping.TypeInfo?.SqlType}) " +
                   $"cannot be mapped to JSON field at '{mapping.JsonPath}' ({mapping.TypeInfo?.JsonType})";
        }

        private string GenerateFieldMappingSuggestion(FieldMapping mapping)
        {
            if (mapping.JsonPath == null)
                return $"Ensure JSON contains a field named '{mapping.SqlField}' or adjust the SQL query";

            if (mapping.ConversionRequired != null)
                return $"Consider {mapping.ConversionRequired.ToLowerInvariant()} or modify the SQL query to handle type differences";

            return "Review field names and data types for compatibility";
        }

        private string GetFunctionAlternativeSuggestion(string functionName)
        {
            return functionName.ToUpperInvariant() switch
            {
                "ROW_NUMBER" => "Use array indexing or ordering in JSON transformation",
                "RANK" => "Consider using ORDER BY with custom ranking logic",
                "GETDATE" => "Use a parameter or literal date value instead",
                "LEAD" or "LAG" => "These window functions are not supported in JSON transformations",
                _ => "Consider removing this function or using an alternative approach"
            };
        }

        private List<string> GenerateGeneralSuggestions(SqlAnalysisResult sqlAnalysis, JsonSchema jsonSchema)
        {
            var suggestions = new List<string>();

            if (sqlAnalysis.Joins.Count > 0)
                suggestions.Add("Remove JOINs and use nested JSON structures instead");

            if (sqlAnalysis.Functions.Any(f => !IsSupportedAggregateType(f.Type)))
                suggestions.Add("Replace unsupported SQL functions with simpler alternatives");

            if (sqlAnalysis.Complexity > 5.0)
                suggestions.Add("Simplify the query by reducing conditions and complexity");

            if (jsonSchema.ArrayFields.Count == 0 && sqlAnalysis.Aggregates.Count > 0)
                suggestions.Add("Ensure JSON contains array fields for aggregation operations");

            return suggestions;
        }

        private string? GenerateSimplifiedQuery(SqlAnalysisResult sqlAnalysis)
        {
            if (sqlAnalysis.Complexity <= 3.0)
                return null; // Already simple enough

            // Generate a simplified version
            var fields = string.Join(", ", sqlAnalysis.Fields.Take(5).Select(f => f.Name));
            var table = sqlAnalysis.Tables.FirstOrDefault()?.Name ?? "data";
            
            return $"SELECT {fields} FROM {table}";
        }

        private List<string> GenerateRequiredJsonChanges(SqlAnalysisResult sqlAnalysis, JsonSchema jsonSchema)
        {
            var changes = new List<string>();

            // Check for missing fields
            var missingFields = sqlAnalysis.Fields
                .Where(sf => !jsonSchema.Fields.Any(jf => 
                    string.Equals(jf.Name, sf.Name, StringComparison.OrdinalIgnoreCase)))
                .Select(sf => sf.Name);

            foreach (var field in missingFields)
            {
                changes.Add($"Add field '{field}' to JSON structure");
            }

            // Check for required arrays
            if (sqlAnalysis.Aggregates.Count > 0 && jsonSchema.ArrayFields.Count == 0)
            {
                changes.Add("Convert appropriate fields to arrays to support aggregation operations");
            }

            return changes;
        }

        private bool IsSupportedAggregateType(SqlAggregateType type)
        {
            return type switch
            {
                SqlAggregateType.Count => true,
                SqlAggregateType.Sum => true,
                SqlAggregateType.Avg => true,
                SqlAggregateType.Min => true,
                SqlAggregateType.Max => true,
                _ => false
            };
        }

        #endregion
    }
}
