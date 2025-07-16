using System.Text.Json.Nodes;
using NextGenPowerToys.JSQL.Models;
using NextGenPowerToys.JSQL.Services;
using Microsoft.Extensions.Logging;

namespace NextGenPowerToys.JSQL.Core
{
    /// <summary>
    /// Core interface for analyzing SQL-to-JSON transformation compatibility
    /// </summary>
    public interface ITransformationAnalyzer
    {
        /// <summary>
        /// Analyzes the compatibility between a JSON example and SQL query,
        /// returning either a transformation template or detailed error analysis
        /// </summary>
        /// <param name="jsonExample">Sample JSON data structure</param>
        /// <param name="sqlQuery">SQL SELECT statement to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Analysis result with template or errors</returns>
        Task<AnalysisResult> AnalyzeTransformation(
            string jsonExample, 
            string sqlQuery, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Quick validation of compatibility without full template generation
        /// </summary>
        /// <param name="jsonExample">Sample JSON data structure</param>
        /// <param name="sqlQuery">SQL SELECT statement to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result with compatibility score</returns>
        Task<ValidationResult> ValidateCompatibility(
            string jsonExample, 
            string sqlQuery, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes multiple SQL queries against the same JSON schema
        /// </summary>
        /// <param name="jsonExample">Sample JSON data structure</param>
        /// <param name="sqlQueries">List of SQL queries to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary of query to analysis result</returns>
        Task<Dictionary<string, AnalysisResult>> BatchAnalyze(
            string jsonExample, 
            IEnumerable<string> sqlQueries, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a transformation template from validated JSON and SQL analysis
        /// </summary>
        /// <param name="jsonData">Parsed JSON data</param>
        /// <param name="sqlAnalysis">SQL analysis result</param>
        /// <returns>Generated transformation template</returns>
        JsonNode GenerateTemplate(JsonNode jsonData, SqlAnalysisResult sqlAnalysis);

        /// <summary>
        /// Gets detailed compatibility report between JSON schema and SQL analysis
        /// </summary>
        /// <param name="jsonData">Parsed JSON data</param>
        /// <param name="sqlAnalysis">SQL analysis result</param>
        /// <returns>Detailed compatibility report</returns>
        CompatibilityReport GetCompatibilityReport(JsonNode jsonData, SqlAnalysisResult sqlAnalysis);
    }

    /// <summary>
    /// Configuration options for transformation analysis
    /// </summary>
    public class AnalysisOptions
    {
        /// <summary>
        /// Whether to include performance optimization suggestions
        /// </summary>
        public bool IncludeOptimizations { get; set; } = true;

        /// <summary>
        /// Whether to provide alternative solutions when transformation fails
        /// </summary>
        public bool ProvideAlternatives { get; set; } = true;

        /// <summary>
        /// Maximum complexity score allowed (0.0 to 1.0)
        /// </summary>
        public double MaxComplexityThreshold { get; set; } = 0.8;

        /// <summary>
        /// Whether to validate data type compatibility strictly
        /// </summary>
        public bool StrictTypeValidation { get; set; } = false;

        /// <summary>
        /// Whether to include detailed error context
        /// </summary>
        public bool IncludeDetailedErrors { get; set; } = true;

        /// <summary>
        /// Maximum number of sample values to analyze per field
        /// </summary>
        public int MaxSampleSize { get; set; } = 100;

        /// <summary>
        /// Whether to generate examples in error messages
        /// </summary>
        public bool GenerateExamples { get; set; } = true;

        /// <summary>
        /// Timeout for analysis operations
        /// </summary>
        public TimeSpan AnalysisTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Custom field mapping rules
        /// </summary>
        public Dictionary<string, string> CustomFieldMappings { get; set; } = new();

        /// <summary>
        /// Whether to cache analysis results
        /// </summary>
        public bool EnableCaching { get; set; } = true;
    }

    /// <summary>
    /// Main implementation of the transformation analyzer
    /// </summary>
    public class TransformationAnalyzer : ITransformationAnalyzer
    {
        private readonly IJsonAnalyzer _jsonAnalyzer;
        private readonly ISqlParser _sqlParser;
        private readonly ICompatibilityValidator _validator;
        private readonly ITemplateGenerator _templateGenerator;
        private readonly AnalysisOptions _options;
        private readonly ILogger<TransformationAnalyzer>? _logger;

        /// <summary>
        /// Initializes a new instance of the TransformationAnalyzer
        /// </summary>
        /// <param name="jsonAnalyzer">JSON analysis service</param>
        /// <param name="sqlParser">SQL parsing service</param>
        /// <param name="validator">Compatibility validation service</param>
        /// <param name="templateGenerator">Template generation service</param>
        /// <param name="options">Analysis options</param>
        /// <param name="logger">Logger instance</param>
        public TransformationAnalyzer(
            IJsonAnalyzer jsonAnalyzer,
            ISqlParser sqlParser,
            ICompatibilityValidator validator,
            ITemplateGenerator templateGenerator,
            AnalysisOptions? options = null,
            ILogger<TransformationAnalyzer>? logger = null)
        {
            _jsonAnalyzer = jsonAnalyzer ?? throw new ArgumentNullException(nameof(jsonAnalyzer));
            _sqlParser = sqlParser ?? throw new ArgumentNullException(nameof(sqlParser));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _templateGenerator = templateGenerator ?? throw new ArgumentNullException(nameof(templateGenerator));
            _options = options ?? new AnalysisOptions();
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<AnalysisResult> AnalyzeTransformation(
            string jsonExample, 
            string sqlQuery, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Starting transformation analysis for SQL query: {SqlQuery}", 
                    sqlQuery.Length > 100 ? sqlQuery.Substring(0, 100) + "..." : sqlQuery);

                // Validate inputs
                if (string.IsNullOrWhiteSpace(jsonExample))
                    throw new ArgumentException("JSON example cannot be null or empty", nameof(jsonExample));

                if (string.IsNullOrWhiteSpace(sqlQuery))
                    throw new ArgumentException("SQL query cannot be null or empty", nameof(sqlQuery));

                // Parse JSON example
                JsonNode? jsonData;
                try
                {
                    jsonData = JsonNode.Parse(jsonExample);
                    if (jsonData == null)
                        throw new ArgumentException("JSON example could not be parsed", nameof(jsonExample));
                }
                catch (Exception ex)
                {
                    return new AnalysisResult
                    {
                        IsSuccess = false,
                        Errors = new List<AnalysisError>
                        {
                            new AnalysisError
                            {
                                Type = ErrorType.FieldNotFound,
                                Message = $"Invalid JSON format: {ex.Message}",
                                Suggestion = "Please provide valid JSON data"
                            }
                        }
                    };
                }

                // Analyze JSON schema
                var jsonSchema = await _jsonAnalyzer.AnalyzeSchemaAsync(jsonData);
                _logger?.LogDebug("JSON schema analyzed. Found {FieldCount} fields and {ArrayCount} arrays", 
                    jsonSchema.Fields.Count, jsonSchema.Arrays.Count);

                // Parse SQL query
                SqlAnalysisResult sqlAnalysis;
                try
                {
                    sqlAnalysis = await _sqlParser.ParseQueryAsync(sqlQuery);
                    _logger?.LogDebug("SQL query parsed. Query type: {QueryType}, Complexity: {Complexity}", 
                        sqlAnalysis.QueryType, sqlAnalysis.ComplexityScore);
                }
                catch (Exception ex)
                {
                    return new AnalysisResult
                    {
                        IsSuccess = false,
                        Errors = new List<AnalysisError>
                        {
                            new AnalysisError
                            {
                                Type = ErrorType.UnsupportedSqlFunction,
                                Message = $"SQL parsing failed: {ex.Message}",
                                SqlPart = sqlQuery,
                                Suggestion = "Please check SQL syntax and ensure it's a valid SELECT statement"
                            }
                        }
                    };
                }

                // Validate compatibility
                var validationResult = await _validator.ValidateCompatibilityAsync(sqlAnalysis, jsonSchema);
                
                var analysisResult = new AnalysisResult
                {
                    IsSuccess = validationResult.IsValid,
                    Compatibility = new CompatibilityReport
                    {
                        Score = validationResult.Score,
                        FeatureSupport = await GetFeatureSupport(sqlAnalysis)
                    }
                };

                if (validationResult.IsValid)
                {
                    // Generate transformation template
                    try
                    {
                        var template = await _templateGenerator.GenerateTemplateAsync(sqlAnalysis, jsonSchema, jsonData, validationResult.FieldMappings);
                        analysisResult.Template = template;
                        
                        // Add performance estimation
                        analysisResult.Performance = EstimatePerformance(sqlAnalysis, jsonSchema);
                        
                        // Add optimization suggestions
                        if (_options.IncludeOptimizations)
                        {
                            analysisResult.AppliedOptimizations = GenerateOptimizations(sqlAnalysis, jsonSchema);
                        }

                        _logger?.LogInformation("Transformation template generated successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to generate transformation template");
                        analysisResult.IsSuccess = false;
                        analysisResult.Errors.Add(new AnalysisError
                        {
                            Type = ErrorType.UnsupportedSqlFunction,
                            Message = $"Template generation failed: {ex.Message}",
                            Suggestion = "The SQL query may be too complex or use unsupported features"
                        });
                    }
                }
                else
                {
                    // Convert validation errors to analysis errors
                    analysisResult.Errors = validationResult.Errors.Select(ve => new AnalysisError
                    {
                        Type = MapErrorType(ve.Category),
                        Message = ve.Issue,
                        SqlPart = ve.SqlElement,
                        JsonPath = ve.JsonContext,
                        Suggestion = ve.Suggestion,
                        Severity = ve.Severity
                    }).ToList();

                    // Add alternatives if requested
                    if (_options.ProvideAlternatives)
                    {
                        analysisResult.Alternatives = await GenerateAlternatives(sqlAnalysis, jsonSchema);
                    }
                }

                // Convert validation warnings to analysis warnings
                analysisResult.Warnings = validationResult.Warnings.Select(vw => new AnalysisWarning
                {
                    Type = MapWarningType(vw.Level),
                    Message = vw.Message,
                    Context = vw.Context,
                    Solution = vw.Recommendation
                }).ToList();

                return analysisResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error during transformation analysis");
                return new AnalysisResult
                {
                    IsSuccess = false,
                    Errors = new List<AnalysisError>
                    {
                        new AnalysisError
                        {
                            Type = ErrorType.UnsupportedSqlFunction,
                            Message = $"Analysis failed: {ex.Message}",
                            Severity = ErrorSeverity.Critical
                        }
                    }
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ValidationResult> ValidateCompatibility(
            string jsonExample, 
            string sqlQuery, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Parse inputs
                var jsonData = JsonNode.Parse(jsonExample);
                if (jsonData == null)
                    throw new ArgumentException("Invalid JSON format", nameof(jsonExample));

                var jsonSchema = await _jsonAnalyzer.AnalyzeSchemaAsync(jsonData);
                var sqlAnalysis = await _sqlParser.ParseQueryAsync(sqlQuery);
                
                // Perform validation
                return await _validator.ValidateCompatibilityAsync(sqlAnalysis, jsonSchema);
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Score = 0.0,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Issue = $"Validation failed: {ex.Message}",
                            Severity = ErrorSeverity.Critical,
                            Suggestion = "Please check input format and try again"
                        }
                    }
                };
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, AnalysisResult>> BatchAnalyze(
            string jsonExample, 
            IEnumerable<string> sqlQueries, 
            CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, AnalysisResult>();
            
            // Parse JSON once for all queries
            var jsonData = JsonNode.Parse(jsonExample);
            if (jsonData == null)
                throw new ArgumentException("Invalid JSON format", nameof(jsonExample));

            var jsonSchema = await _jsonAnalyzer.AnalyzeSchemaAsync(jsonData);

            // Analyze each query
            var tasks = sqlQueries.Select(async query =>
            {
                var result = await AnalyzeTransformation(jsonExample, query, cancellationToken);
                return new { Query = query, Result = result };
            });

            var completedTasks = await Task.WhenAll(tasks);

            foreach (var task in completedTasks)
            {
                results[task.Query] = task.Result;
            }

            return results;
        }

        /// <inheritdoc/>
        public JsonNode GenerateTemplate(JsonNode jsonData, SqlAnalysisResult sqlAnalysis)
        {
            return GenerateTemplateAsync(jsonData, sqlAnalysis).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public CompatibilityReport GetCompatibilityReport(JsonNode jsonData, SqlAnalysisResult sqlAnalysis)
        {
            return GetCompatibilityReportAsync(jsonData, sqlAnalysis).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<JsonNode> GenerateTemplateAsync(JsonNode jsonData, SqlAnalysisResult sqlAnalysis)
        {
            var jsonSchema = await _jsonAnalyzer.AnalyzeSchemaAsync(jsonData);
            var validationResult = await _validator.ValidateCompatibilityAsync(sqlAnalysis, jsonSchema);
            return await _templateGenerator.GenerateTemplateAsync(sqlAnalysis, jsonSchema, jsonData, validationResult.FieldMappings) ?? new JsonObject();
        }

        /// <inheritdoc/>
        public async Task<CompatibilityReport> GetCompatibilityReportAsync(JsonNode jsonData, SqlAnalysisResult sqlAnalysis)
        {
            var jsonSchema = await _jsonAnalyzer.AnalyzeSchemaAsync(jsonData);
            var validationResult = await _validator.ValidateCompatibilityAsync(sqlAnalysis, jsonSchema);
            
            return new CompatibilityReport
            {
                Score = validationResult.Score,
                FeatureSupport = await GetFeatureSupport(sqlAnalysis),
                Complexity = new ComplexityAssessment
                {
                    OverallComplexity = sqlAnalysis.ComplexityScore,
                    JoinCount = sqlAnalysis.Joins.Count,
                    AggregationCount = sqlAnalysis.Aggregates.Count,
                    ConditionCount = sqlAnalysis.WhereConditions.Count
                },
                FieldMappings = validationResult.FieldMappings ?? new List<FieldMapping>()
            };
        }

        #region Private Helper Methods

        private async Task<Dictionary<string, bool>> GetFeatureSupport(SqlAnalysisResult sqlAnalysis)
        {
            return new Dictionary<string, bool>
            {
                ["BasicSelect"] = true,
                ["WhereClause"] = sqlAnalysis.WhereConditions.Any(),
                ["Joins"] = sqlAnalysis.Joins.Any(),
                ["Aggregation"] = sqlAnalysis.Aggregates.Any(),
                ["GroupBy"] = sqlAnalysis.GroupByFields.Any(),
                ["OrderBy"] = sqlAnalysis.OrderByFields.Any(),
                ["Having"] = sqlAnalysis.Having != null,
                ["Limit"] = sqlAnalysis.Limit != null,
                ["StringOperations"] = ContainsStringOperations(sqlAnalysis),
                ["MathOperations"] = ContainsMathOperations(sqlAnalysis),
                ["ConditionalLogic"] = ContainsConditionalLogic(sqlAnalysis)
            };
        }

        private bool ContainsStringOperations(SqlAnalysisResult sqlAnalysis)
        {
            return sqlAnalysis.WhereConditions.Any(c => 
                c.Operator == SqlOperator.Like || 
                c.Operator == SqlOperator.NotLike) ||
                sqlAnalysis.SelectFields.Any(f => 
                    f.Expression?.FunctionName?.ToUpper() is "CONCAT" or "UPPER" or "LOWER" or "TRIM");
        }

        private bool ContainsMathOperations(SqlAnalysisResult sqlAnalysis)
        {
            return sqlAnalysis.SelectFields.Any(f => 
                f.Expression?.Operator is "+" or "-" or "*" or "/" or "%" ||
                f.Expression?.FunctionName?.ToUpper() is "ROUND" or "ABS" or "CEILING" or "FLOOR");
        }

        private bool ContainsConditionalLogic(SqlAnalysisResult sqlAnalysis)
        {
            return sqlAnalysis.SelectFields.Any(f => 
                f.Expression?.Type == SqlExpressionType.CaseStatement) ||
                sqlAnalysis.WhereConditions.Any(c => c.NestedConditions.Any());
        }

        private PerformanceEstimate EstimatePerformance(SqlAnalysisResult sqlAnalysis, JsonSchema jsonSchema)
        {
            var complexityFactor = 1.0;
            complexityFactor += sqlAnalysis.Joins.Count * 0.5;
            complexityFactor += sqlAnalysis.Aggregates.Count * 0.3;
            complexityFactor += sqlAnalysis.WhereConditions.Count * 0.1;

            var estimatedTimeMs = Math.Max(1.0, complexityFactor * 2.0);
            var estimatedMemoryKB = Math.Max(10.0, jsonSchema.EstimatedSizeBytes / 1024.0 * complexityFactor);

            return new PerformanceEstimate
            {
                EstimatedExecutionTimeMs = estimatedTimeMs,
                EstimatedMemoryUsageKB = estimatedMemoryKB,
                ComplexityFactor = complexityFactor
            };
        }

        private List<string> GenerateOptimizations(SqlAnalysisResult sqlAnalysis, JsonSchema jsonSchema)
        {
            var optimizations = new List<string>();

            if (sqlAnalysis.WhereConditions.Count > 3)
            {
                optimizations.Add("Consider simplifying WHERE conditions for better performance");
            }

            if (sqlAnalysis.Joins.Count > 2)
            {
                optimizations.Add("Multiple JOINs detected - consider denormalizing data structure");
            }

            if (!jsonSchema.HasConsistentStructure)
            {
                optimizations.Add("Inconsistent JSON structure may impact performance");
            }

            return optimizations;
        }

        private async Task<AlternativeOptions> GenerateAlternatives(SqlAnalysisResult sqlAnalysis, JsonSchema jsonSchema)
        {
            var alternatives = new AlternativeOptions();

            // Generate simplified query suggestions
            var availableFields = jsonSchema.Fields.Select(f => f.Name).ToList();
            var usedFields = sqlAnalysis.SelectFields.Select(f => f.Name).ToList();
            var missingFields = usedFields.Except(availableFields, StringComparer.OrdinalIgnoreCase).ToList();

            if (missingFields.Any())
            {
                alternatives.Suggestions.Add($"Add missing fields to JSON: {string.Join(", ", missingFields)}");
                alternatives.RequiredJsonChanges.AddRange(missingFields.Select(f => $"Add '{f}' field"));
            }

            // Generate simplified SQL
            if (sqlAnalysis.Joins.Any() || sqlAnalysis.Aggregates.Any())
            {
                var simpleFields = sqlAnalysis.SelectFields
                    .Where(f => f.IsSimpleField && availableFields.Contains(f.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(f => f.Name);

                if (simpleFields.Any())
                {
                    alternatives.SimplifiedQuery = $"SELECT {string.Join(", ", simpleFields)} FROM {sqlAnalysis.Tables.First().Name}";
                }
            }

            return alternatives;
        }

        private ErrorType MapErrorType(string category)
        {
            return category.ToLower() switch
            {
                "field" => ErrorType.FieldNotFound,
                "type" => ErrorType.TypeMismatch,
                "function" => ErrorType.UnsupportedSqlFunction,
                "aggregation" => ErrorType.InvalidAggregation,
                "join" => ErrorType.MissingJoinData,
                "subquery" => ErrorType.ComplexSubquery,
                "operator" => ErrorType.UnsupportedOperator,
                _ => ErrorType.UnsupportedSqlFunction
            };
        }

        private WarningType MapWarningType(WarningLevel level)
        {
            return level switch
            {
                WarningLevel.High => WarningType.PerformanceImpact,
                WarningLevel.Medium => WarningType.TypeConversion,
                WarningLevel.Low => WarningType.NullValueHandling,
                _ => WarningType.NullValueHandling
            };
        }

        #endregion
    }
}
