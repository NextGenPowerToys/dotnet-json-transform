using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Json.Transform.Examples.Api;

/// <summary>
/// Request model for JSON transformation
/// </summary>
public class TransformRequest
{
    /// <summary>
    /// The source JSON data to transform
    /// </summary>
    [Required]
    [JsonPropertyName("sourceJson")]
    public string SourceJson { get; set; } = "";

    /// <summary>
    /// The transformation template defining how to transform the source JSON
    /// </summary>
    [Required]
    [JsonPropertyName("templateJson")]
    public string TemplateJson { get; set; } = "";
}

/// <summary>
/// Response model for JSON transformation
/// </summary>
public class TransformResponse
{
    /// <summary>
    /// The transformed JSON result
    /// </summary>
    [JsonPropertyName("resultJson")]
    public string ResultJson { get; set; } = "";

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    [JsonPropertyName("executionTimeMs")]
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Whether the transformation was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Error message if transformation failed
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Example transformation scenarios for testing
/// </summary>
public class ExampleScenario
{
    /// <summary>
    /// Name of the example scenario
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Description of what this scenario demonstrates
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// Example source JSON
    /// </summary>
    [JsonPropertyName("sourceJson")]
    public string SourceJson { get; set; } = "";

    /// <summary>
    /// Example transformation template
    /// </summary>
    [JsonPropertyName("templateJson")]
    public string TemplateJson { get; set; } = "";
}
