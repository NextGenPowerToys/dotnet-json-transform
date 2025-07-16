using System.Text.Json.Serialization;

namespace Json.Transform.Models;

/// <summary>
/// Represents an aggregation rule with optional conditions
/// </summary>
public class AggregationRule
{
    /// <summary>
    /// Type of aggregation (sum, count, min, max, avg, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Field to aggregate (for sum, min, max, avg operations)
    /// </summary>
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    /// <summary>
    /// Condition to filter array elements before aggregation
    /// </summary>
    [JsonPropertyName("condition")]
    public string? Condition { get; set; }
}
