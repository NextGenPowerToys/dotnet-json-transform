using System.Text.Json.Nodes;

namespace NextGenPowerToys.JSQL.Models
{
    /// <summary>
    /// Represents the schema of a JSON document
    /// </summary>
    public class JsonSchema
    {
        /// <summary>
        /// All discovered fields in the JSON
        /// </summary>
        public List<JsonField> Fields { get; set; } = new();

        /// <summary>
        /// Root data type (object, array, etc.)
        /// </summary>
        public string RootType { get; set; } = string.Empty;

        /// <summary>
        /// Maximum nesting depth found
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Fields that are arrays (for aggregation support)
        /// </summary>
        public List<JsonField> ArrayFields { get; set; } = new();

        /// <summary>
        /// Required fields (non-nullable at root level)
        /// </summary>
        public List<string> RequiredFields { get; set; } = new();

        /// <summary>
        /// Overall complexity score of the JSON structure
        /// </summary>
        public double Complexity { get; set; }

        /// <summary>
        /// Arrays found in the JSON structure
        /// </summary>
        public List<ArrayInfo> Arrays { get; set; } = new();

        /// <summary>
        /// All available JSONPath expressions
        /// </summary>
        public List<string> AvailablePaths { get; set; } = new();

        /// <summary>
        /// Root object structure
        /// </summary>
        public Dictionary<string, object> RootStructure { get; set; } = new();

        /// <summary>
        /// Estimated document size
        /// </summary>
        public long EstimatedSizeBytes { get; set; }

        /// <summary>
        /// Whether the document has consistent structure
        /// </summary>
        public bool HasConsistentStructure { get; set; }
    }

    /// <summary>
    /// Information about an array in the JSON
    /// </summary>
    public class ArrayInfo
    {
        /// <summary>
        /// JSONPath to the array
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Name of the array property
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Number of items in the array
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Element count (compatibility property)
        /// </summary>
        public int ElementCount { get; set; }

        /// <summary>
        /// Element type (compatibility property)
        /// </summary>
        public string ElementType { get; set; } = "unknown";

        /// <summary>
        /// Schema of array items
        /// </summary>
        public Dictionary<string, DataTypeInfo> ItemSchema { get; set; } = new();

        /// <summary>
        /// Whether all items have the same structure
        /// </summary>
        public bool IsHomogeneous { get; set; }

        /// <summary>
        /// Maximum nesting depth within the array
        /// </summary>
        public int MaxItemDepth { get; set; }

        /// <summary>
        /// Available paths within array items
        /// </summary>
        public List<string> ItemPaths { get; set; } = new();

        /// <summary>
        /// Supported aggregations (compatibility property)
        /// </summary>
        public List<string> SupportedAggregations { get; set; } = new() { "count", "sum", "avg", "min", "max" };

        /// <summary>
        /// Nested arrays (compatibility property)
        /// </summary>
        public List<ArrayInfo> NestedArrays { get; set; } = new();
    }

    /// <summary>
    /// Represents compatibility between SQL field and JSON path
    /// </summary>
    public class FieldCompatibility
    {
        /// <summary>
        /// Whether the field is compatible
        /// </summary>
        public bool IsCompatible { get; set; }

        /// <summary>
        /// Confidence in the compatibility assessment (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// JSONPath that matches the SQL field
        /// </summary>
        public string? JsonPath { get; set; }

        /// <summary>
        /// JSON data type information
        /// </summary>
        public DataTypeInfo? JsonType { get; set; }

        /// <summary>
        /// Expected SQL data type
        /// </summary>
        public DataTypeInfo? ExpectedSqlType { get; set; }

        /// <summary>
        /// Type of conversion required, if any
        /// </summary>
        public string? ConversionRequired { get; set; }

        /// <summary>
        /// Alternative paths that might work
        /// </summary>
        public List<string> AlternativePaths { get; set; } = new();

        /// <summary>
        /// Issues found with the compatibility
        /// </summary>
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// Information about data types
    /// </summary>
    public class DataTypeInfo
    {
        /// <summary>
        /// Primary data type (string, number, boolean, object, array, null)
        /// </summary>
        public string PrimaryType { get; set; } = string.Empty;

        /// <summary>
        /// More specific type if determinable (integer, decimal, datetime, etc.)
        /// </summary>
        public string? SpecificType { get; set; }

        /// <summary>
        /// SQL data type
        /// </summary>
        public string SqlType { get; set; } = string.Empty;

        /// <summary>
        /// JSON data type
        /// </summary>
        public string JsonType { get; set; } = string.Empty;

        /// <summary>
        /// Whether types are compatible
        /// </summary>
        public bool IsCompatible { get; set; }

        /// <summary>
        /// Confidence in type detection (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Whether the field can be null
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Whether the field is always present
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Sample values observed
        /// </summary>
        public List<object> SampleValues { get; set; } = new();

        /// <summary>
        /// Minimum value for numeric types
        /// </summary>
        public object? MinValue { get; set; }

        /// <summary>
        /// Maximum value for numeric types
        /// </summary>
        public object? MaxValue { get; set; }

        /// <summary>
        /// Pattern for string types
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>
        /// Average length for string types
        /// </summary>
        public double? AvgLength { get; set; }

        /// <summary>
        /// Whether the type is consistent across all instances
        /// </summary>
        public bool IsConsistent { get; set; }
    }

    /// <summary>
    /// Validation results for SQL-JSON compatibility
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether the transformation is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Overall compatibility score (0.0 to 1.0)
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Validation errors found
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new();

        /// <summary>
        /// Validation warnings
        /// </summary>
        public List<ValidationWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Field mapping results for template generation
        /// </summary>
        public List<FieldMapping> FieldMappings { get; set; } = new();

        /// <summary>
        /// Performance impact assessment
        /// </summary>
        public PerformanceImpact Performance { get; set; } = new();

        /// <summary>
        /// Recommendations for improvement
        /// </summary>
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Represents a validation error
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// SQL element that caused the error
        /// </summary>
        public string SqlElement { get; set; } = string.Empty;

        /// <summary>
        /// Description of the issue
        /// </summary>
        public string Issue { get; set; } = string.Empty;

        /// <summary>
        /// JSON context where the issue occurs
        /// </summary>
        public string JsonContext { get; set; } = string.Empty;

        /// <summary>
        /// Suggested solution
        /// </summary>
        public string Suggestion { get; set; } = string.Empty;

        /// <summary>
        /// Severity of the error
        /// </summary>
        public ErrorSeverity Severity { get; set; }

        /// <summary>
        /// Error category
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a validation warning
    /// </summary>
    public class ValidationWarning
    {
        /// <summary>
        /// Warning message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Context information
        /// </summary>
        public string Context { get; set; } = string.Empty;

        /// <summary>
        /// Recommended action
        /// </summary>
        public string? Recommendation { get; set; }

        /// <summary>
        /// Impact level
        /// </summary>
        public WarningLevel Level { get; set; }
    }

    /// <summary>
    /// Warning levels
    /// </summary>
    public enum WarningLevel
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Performance impact assessment
    /// </summary>
    public class PerformanceImpact
    {
        /// <summary>
        /// Expected performance level
        /// </summary>
        public PerformanceLevel Level { get; set; }

        /// <summary>
        /// Factors affecting performance
        /// </summary>
        public List<string> Factors { get; set; } = new();

        /// <summary>
        /// Estimated execution time multiplier
        /// </summary>
        public double ExecutionTimeMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Estimated memory usage multiplier
        /// </summary>
        public double MemoryUsageMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Optimization suggestions
        /// </summary>
        public List<string> OptimizationSuggestions { get; set; } = new();
    }

    /// <summary>
    /// Performance levels
    /// </summary>
    public enum PerformanceLevel
    {
        Excellent,
        Good,
        Fair,
        Poor,
        Unacceptable
    }

    /// <summary>
    /// Aggregation validation results
    /// </summary>
    public class AggregationValidation
    {
        /// <summary>
        /// Whether aggregations are valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Issues found with aggregations
        /// </summary>
        public List<string> Issues { get; set; } = new();

        /// <summary>
        /// Supported aggregations
        /// </summary>
        public List<string> SupportedAggregations { get; set; } = new();

        /// <summary>
        /// Unsupported aggregations
        /// </summary>
        public List<string> UnsupportedAggregations { get; set; } = new();

        /// <summary>
        /// Performance impact of aggregations
        /// </summary>
        public PerformanceImpact Performance { get; set; } = new();
    }

    /// <summary>
    /// JOIN validation results
    /// </summary>
    public class JoinValidation
    {
        /// <summary>
        /// Whether JOINs are valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Issues found with JOINs
        /// </summary>
        public List<string> Issues { get; set; } = new();

        /// <summary>
        /// Available join paths
        /// </summary>
        public List<string> AvailableJoinPaths { get; set; } = new();

        /// <summary>
        /// Missing join data
        /// </summary>
        public List<string> MissingJoinData { get; set; } = new();

        /// <summary>
        /// Suggested data restructuring
        /// </summary>
        public List<string> RestructuringSuggestions { get; set; } = new();

        /// <summary>
        /// Alternative approaches
        /// </summary>
        public List<string> Alternatives { get; set; } = new();
    }

    /// <summary>
    /// Compatibility score breakdown
    /// </summary>
    public class CompatibilityScore
    {
        /// <summary>
        /// Overall score (0.0 to 1.0)
        /// </summary>
        public double Overall { get; set; }

        /// <summary>
        /// Field mapping score
        /// </summary>
        public double FieldMapping { get; set; }

        /// <summary>
        /// Type compatibility score
        /// </summary>
        public double TypeCompatibility { get; set; }

        /// <summary>
        /// Feature support score
        /// </summary>
        public double FeatureSupport { get; set; }

        /// <summary>
        /// Performance score
        /// </summary>
        public double Performance { get; set; }

        /// <summary>
        /// Complexity score
        /// </summary>
        public double Complexity { get; set; }
    }

    /// <summary>
    /// Represents a field discovered in JSON structure
    /// </summary>
    public class JsonField
    {
        /// <summary>
        /// Field name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// JSONPath to this field
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// JSON data type
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Whether this field is an array
        /// </summary>
        public bool IsArray { get; set; }

        /// <summary>
        /// Whether this field can be null
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Nesting depth of this field
        /// </summary>
        public int Depth { get; set; }
    }

    /// <summary>
    /// Represents an SQL function used in a query
    /// </summary>
    public class SqlFunction
    {
        /// <summary>
        /// Function name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Whether this function is supported in transformations
        /// </summary>
        public bool IsSupported { get; set; }

        /// <summary>
        /// Function category (Aggregate, String, Math, etc.)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Function parameters
        /// </summary>
        public List<string> Parameters { get; set; } = new();
    }
}
