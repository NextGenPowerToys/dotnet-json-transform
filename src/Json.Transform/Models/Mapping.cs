using System.Text.Json.Serialization;

namespace Json.Transform.Models;

/// <summary>
/// Represents a single transformation mapping rule
/// </summary>
public class Mapping
{
    /// <summary>
    /// Source JSONPath expression to extract data from
    /// </summary>
    [JsonPropertyName("from")]
    public string? From { get; set; }

    /// <summary>
    /// Destination JSONPath expression to write data to
    /// </summary>
    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Constant value to use instead of extracting from source
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// Aggregation operation to perform on array data
    /// </summary>
    [JsonPropertyName("aggregate")]
    public string? Aggregate { get; set; }

    /// <summary>
    /// Advanced aggregation rule with conditions
    /// </summary>
    [JsonPropertyName("aggregation")]
    public AggregationRule? Aggregation { get; set; }

    /// <summary>
    /// Mathematical operation to perform
    /// </summary>
    [JsonPropertyName("math")]
    public MathOperation? Math { get; set; }

    /// <summary>
    /// String concatenation template (e.g., "{$.user.firstName} {$.user.lastName}")
    /// </summary>
    [JsonPropertyName("concat")]
    public string? Concat { get; set; }

    /// <summary>
    /// Conditional logic for this mapping
    /// </summary>
    [JsonPropertyName("conditions")]
    public List<Condition>? Conditions { get; set; }

    /// <summary>
    /// Default value to use if source is null or not found
    /// </summary>
    [JsonPropertyName("default")]
    public object? Default { get; set; }

    /// <summary>
    /// Whether this mapping should be processed (useful for conditional mappings)
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Nested transformation template for complex object mapping
    /// </summary>
    [JsonPropertyName("template")]
    public TransformTemplate? Template { get; set; }
}
