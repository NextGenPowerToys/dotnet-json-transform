using System.Text.Json.Serialization;

namespace Json.Transform.Models;

/// <summary>
/// Represents a conditional logic rule for transformations
/// </summary>
public class Condition
{
    /// <summary>
    /// The condition expression to evaluate (e.g., "$.user.age >= 18")
    /// </summary>
    [JsonPropertyName("if")]
    public string? If { get; set; }

    /// <summary>
    /// The value to use if the condition is true
    /// </summary>
    [JsonPropertyName("then")]
    public object? Then { get; set; }

    /// <summary>
    /// The value to use if the condition is false
    /// </summary>
    [JsonPropertyName("else")]
    public object? Else { get; set; }

    /// <summary>
    /// Additional conditions for elseif logic
    /// </summary>
    [JsonPropertyName("elseif")]
    public List<Condition>? ElseIf { get; set; }
}
