using System.Text.Json.Serialization;

namespace Json.Transform.Models;

/// <summary>
/// Represents a mathematical operation configuration
/// </summary>
public class MathOperation
{
    /// <summary>
    /// The mathematical operation to perform (add, subtract, multiply, divide, power, sqrt, abs, round)
    /// </summary>
    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// The operands for the mathematical operation (can be JSONPath expressions or numeric values)
    /// </summary>
    [JsonPropertyName("operands")]
    public List<object> Operands { get; set; } = new();

    /// <summary>
    /// Number of decimal places for rounding operations
    /// </summary>
    [JsonPropertyName("precision")]
    public int? Precision { get; set; }
}
