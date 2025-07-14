using System.Text.Json.Serialization;

namespace Json.Transform.Models;

/// <summary>
/// Represents transformation settings and configuration
/// </summary>
public class TransformSettings
{
    /// <summary>
    /// Whether to fail on missing source paths or continue processing
    /// </summary>
    [JsonPropertyName("strictMode")]
    public bool StrictMode { get; set; } = false;

    /// <summary>
    /// Whether to preserve null values in the output
    /// </summary>
    [JsonPropertyName("preserveNulls")]
    public bool PreserveNulls { get; set; } = true;

    /// <summary>
    /// Whether to create missing intermediate paths in the target
    /// </summary>
    [JsonPropertyName("createPaths")]
    public bool CreatePaths { get; set; } = true;

    /// <summary>
    /// Maximum depth for nested transformations to prevent infinite recursion
    /// </summary>
    [JsonPropertyName("maxDepth")]
    public int MaxDepth { get; set; } = 10;

    /// <summary>
    /// Whether to enable detailed logging and tracing
    /// </summary>
    [JsonPropertyName("enableTracing")]
    public bool EnableTracing { get; set; } = false;
}

/// <summary>
/// Main container for transformation rules and configuration
/// </summary>
public class TransformTemplate
{
    /// <summary>
    /// List of transformation mappings to apply
    /// </summary>
    [JsonPropertyName("mappings")]
    public List<Mapping> Mappings { get; set; } = new();

    /// <summary>
    /// Transformation settings and configuration
    /// </summary>
    [JsonPropertyName("settings")]
    public TransformSettings? Settings { get; set; }

    /// <summary>
    /// Version of the transformation template for compatibility tracking
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Optional description of what this transformation does
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
