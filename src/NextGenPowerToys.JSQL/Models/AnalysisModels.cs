using System.Text.Json.Nodes;

namespace NextGenPowerToys.JSQL.Models
{
    /// <summary>
    /// Represents the result of analyzing a JSON example and SQL query for transformation compatibility
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// Indicates whether the transformation is possible
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Generated transformation template (only available on success)
        /// </summary>
        public JsonNode? Template { get; set; }

        /// <summary>
        /// List of errors that prevent transformation
        /// </summary>
        public List<AnalysisError> Errors { get; set; } = new();

        /// <summary>
        /// List of warnings about potential issues
        /// </summary>
        public List<AnalysisWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Detailed compatibility assessment report
        /// </summary>
        public CompatibilityReport? Compatibility { get; set; }

        /// <summary>
        /// Alternative suggestions when transformation fails
        /// </summary>
        public AlternativeOptions? Alternatives { get; set; }

        /// <summary>
        /// Performance estimates for the transformation
        /// </summary>
        public PerformanceEstimate? Performance { get; set; }

        /// <summary>
        /// Optimizations applied during template generation
        /// </summary>
        public List<string> AppliedOptimizations { get; set; } = new();
    }

    /// <summary>
    /// Represents an error that prevents transformation
    /// </summary>
    public class AnalysisError
    {
        /// <summary>
        /// Type of error encountered
        /// </summary>
        public ErrorType Type { get; set; }

        /// <summary>
        /// Human-readable error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The SQL part that caused the error
        /// </summary>
        public string? SqlPart { get; set; }

        /// <summary>
        /// The JSON path related to the error
        /// </summary>
        public string? JsonPath { get; set; }

        /// <summary>
        /// Suggested solution for fixing the error
        /// </summary>
        public string? Suggestion { get; set; }

        /// <summary>
        /// Severity level of the error
        /// </summary>
        public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;
    }

    /// <summary>
    /// Represents a warning about potential issues
    /// </summary>
    public class AnalysisWarning
    {
        /// <summary>
        /// Type of warning
        /// </summary>
        public WarningType Type { get; set; }

        /// <summary>
        /// Warning message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Solution or mitigation applied
        /// </summary>
        public string? Solution { get; set; }

        /// <summary>
        /// Additional context for the warning
        /// </summary>
        public string? Context { get; set; }
    }

    /// <summary>
    /// Types of errors that can occur during analysis
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// SQL references a field that doesn't exist in JSON
        /// </summary>
        FieldNotFound,

        /// <summary>
        /// Data type incompatibility between SQL expectation and JSON reality
        /// </summary>
        TypeMismatch,

        /// <summary>
        /// SQL function not supported by transformation engine
        /// </summary>
        UnsupportedSqlFunction,

        /// <summary>
        /// Invalid aggregation operation
        /// </summary>
        InvalidAggregation,

        /// <summary>
        /// JOIN references data not available in JSON
        /// </summary>
        MissingJoinData,

        /// <summary>
        /// Subquery too complex to convert
        /// </summary>
        ComplexSubquery,

        /// <summary>
        /// SQL operator not supported
        /// </summary>
        UnsupportedOperator,

        /// <summary>
        /// Multiple tables referenced but JSON has single structure
        /// </summary>
        MultipleTablesInSingleJson,

        /// <summary>
        /// Aggregation attempted on non-array field
        /// </summary>
        AggregationOnNonArray
    }

    /// <summary>
    /// Types of warnings
    /// </summary>
    public enum WarningType
    {
        /// <summary>
        /// Null values may cause issues
        /// </summary>
        NullValueHandling,

        /// <summary>
        /// Performance may be impacted
        /// </summary>
        PerformanceImpact,

        /// <summary>
        /// Type conversion required
        /// </summary>
        TypeConversion,

        /// <summary>
        /// Complex operation simplified
        /// </summary>
        OperationSimplified
    }

    /// <summary>
    /// Severity levels for errors
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Informational message
        /// </summary>
        Info,

        /// <summary>
        /// Warning that doesn't prevent transformation
        /// </summary>
        Warning,

        /// <summary>
        /// Error that prevents transformation
        /// </summary>
        Error,

        /// <summary>
        /// Critical error
        /// </summary>
        Critical
    }

    /// <summary>
    /// Detailed compatibility assessment
    /// </summary>
    public class CompatibilityReport
    {
        /// <summary>
        /// Overall compatibility score (0.0 to 1.0)
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Field mapping results
        /// </summary>
        public List<FieldMapping> FieldMappings { get; set; } = new();

        /// <summary>
        /// SQL features used and their support status
        /// </summary>
        public Dictionary<string, bool> FeatureSupport { get; set; } = new();

        /// <summary>
        /// Complexity assessment
        /// </summary>
        public ComplexityAssessment Complexity { get; set; } = new();
    }

    /// <summary>
    /// Alternative options when transformation fails
    /// </summary>
    public class AlternativeOptions
    {
        /// <summary>
        /// Suggestions for making transformation possible
        /// </summary>
        public List<string> Suggestions { get; set; } = new();

        /// <summary>
        /// Simplified SQL query that would work
        /// </summary>
        public string? SimplifiedQuery { get; set; }

        /// <summary>
        /// Required JSON structure modifications
        /// </summary>
        public List<string> RequiredJsonChanges { get; set; } = new();
    }

    /// <summary>
    /// Performance estimation for the transformation
    /// </summary>
    public class PerformanceEstimate
    {
        /// <summary>
        /// Estimated execution time in milliseconds
        /// </summary>
        public double EstimatedExecutionTimeMs { get; set; }

        /// <summary>
        /// Estimated memory usage in KB
        /// </summary>
        public double EstimatedMemoryUsageKB { get; set; }

        /// <summary>
        /// Complexity factor (1.0 = simple, higher = more complex)
        /// </summary>
        public double ComplexityFactor { get; set; }
    }

    /// <summary>
    /// Mapping between SQL field and JSON path
    /// </summary>
    public class FieldMapping
    {
        /// <summary>
        /// SQL field name
        /// </summary>
        public string SqlField { get; set; } = string.Empty;

        /// <summary>
        /// Corresponding JSON path
        /// </summary>
        public string? JsonPath { get; set; }

        /// <summary>
        /// Whether the mapping is compatible
        /// </summary>
        public bool IsCompatible { get; set; }

        /// <summary>
        /// Data type information
        /// </summary>
        public DataTypeInfo? TypeInfo { get; set; }

        /// <summary>
        /// Required conversion if any
        /// </summary>
        public string? ConversionRequired { get; set; }
    }

    /// <summary>
    /// Complexity assessment of the transformation
    /// </summary>
    public class ComplexityAssessment
    {
        /// <summary>
        /// Number of joins required
        /// </summary>
        public int JoinCount { get; set; }

        /// <summary>
        /// Number of aggregations
        /// </summary>
        public int AggregationCount { get; set; }

        /// <summary>
        /// Number of conditions
        /// </summary>
        public int ConditionCount { get; set; }

        /// <summary>
        /// Nesting depth required
        /// </summary>
        public int NestingDepth { get; set; }

        /// <summary>
        /// Overall complexity score
        /// </summary>
        public double OverallComplexity { get; set; }
    }
}
