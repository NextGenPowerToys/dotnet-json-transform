using System.Text.Json.Serialization;

namespace Json.Transform.Models;

/// <summary>
/// Represents an aggregation rule for processing arrays
/// </summary>
public class AggregationRule
{
    /// <summary>
    /// The type of aggregation operation (sum, avg, min, max, count, first, last, join)
    /// </summary>
    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// The path within array items to aggregate (optional for count operation)
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// Separator for join operations
    /// </summary>
    [JsonPropertyName("separator")]
    public string? Separator { get; set; }
}
