using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Transform.Core;
using Json.Transform.Exceptions;
using Json.Transform.Models;

namespace Json.Transform.Core;

/// <summary>
/// Main entry point for JSON transformations
/// </summary>
public class JsonTransformer
{
    private readonly TransformSettings _defaultSettings;

    /// <summary>
    /// Initializes a new instance of JsonTransformer with default settings
    /// </summary>
    public JsonTransformer() : this(new TransformSettings()) { }

    /// <summary>
    /// Initializes a new instance of JsonTransformer with custom settings
    /// </summary>
    /// <param name="settings">The default settings to use for transformations</param>
    public JsonTransformer(TransformSettings settings)
    {
        _defaultSettings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Transforms JSON data using a transformation template (string-based API)
    /// </summary>
    /// <param name="sourceJson">The source JSON data as a string</param>
    /// <param name="templateJson">The transformation template as a JSON string</param>
    /// <param name="settings">Optional settings to override defaults</param>
    /// <returns>The transformed JSON as a string</returns>
    public string Transform(string sourceJson, string templateJson, TransformSettings? settings = null)
    {
        if (string.IsNullOrEmpty(templateJson))
            throw new ArgumentException("Template JSON cannot be null or empty", nameof(templateJson));

        try
        {
            var sourceData = string.IsNullOrEmpty(sourceJson) ? null : JsonNode.Parse(sourceJson);
            var template = JsonSerializer.Deserialize<TransformTemplate>(templateJson);
            
            if (template == null)
                throw new TransformException("Failed to deserialize transformation template");

            var result = Transform(sourceData, template, settings);
            return result?.ToJsonString() ?? "null";
        }
        catch (JsonException ex)
        {
            throw new TransformException("Invalid JSON format", ex);
        }
    }

    /// <summary>
    /// Transforms JSON data using a transformation template (strongly-typed API)
    /// </summary>
    /// <param name="sourceData">The source JSON data as a JsonNode</param>
    /// <param name="template">The transformation template</param>
    /// <param name="settings">Optional settings to override defaults</param>
    /// <returns>The transformed JSON as a JsonNode</returns>
    public JsonNode? Transform(JsonNode? sourceData, TransformTemplate template, TransformSettings? settings = null)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));

        var effectiveSettings = settings ?? template.Settings ?? _defaultSettings;
        var engine = new TransformEngine(effectiveSettings);
        
        return engine.Transform(sourceData, template);
    }

    /// <summary>
    /// Transforms JSON data using a transformation template asynchronously
    /// </summary>
    /// <param name="sourceJson">The source JSON data as a string</param>
    /// <param name="templateJson">The transformation template as a JSON string</param>
    /// <param name="settings">Optional settings to override defaults</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transformed JSON as a string</returns>
    public async Task<string> TransformAsync(string sourceJson, string templateJson, 
        TransformSettings? settings = null, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Transform(sourceJson, templateJson, settings), cancellationToken);
    }

    /// <summary>
    /// Transforms JSON data using a transformation template asynchronously (strongly-typed API)
    /// </summary>
    /// <param name="sourceData">The source JSON data as a JsonNode</param>
    /// <param name="template">The transformation template</param>
    /// <param name="settings">Optional settings to override defaults</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transformed JSON as a JsonNode</returns>
    public async Task<JsonNode?> TransformAsync(JsonNode? sourceData, TransformTemplate template, 
        TransformSettings? settings = null, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Transform(sourceData, template, settings), cancellationToken);
    }

    /// <summary>
    /// Validates a transformation template for syntax and structure
    /// </summary>
    /// <param name="templateJson">The transformation template as a JSON string</param>
    /// <returns>A list of validation errors, empty if valid</returns>
    public List<string> ValidateTemplate(string templateJson)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(templateJson))
        {
            errors.Add("Template JSON cannot be null or empty");
            return errors;
        }

        try
        {
            var template = JsonSerializer.Deserialize<TransformTemplate>(templateJson);
            if (template == null)
            {
                errors.Add("Failed to deserialize transformation template");
                return errors;
            }

            return ValidateTemplate(template);
        }
        catch (JsonException ex)
        {
            errors.Add($"Invalid JSON format: {ex.Message}");
            return errors;
        }
    }

    /// <summary>
    /// Validates a transformation template for syntax and structure
    /// </summary>
    /// <param name="template">The transformation template</param>
    /// <returns>A list of validation errors, empty if valid</returns>
    public List<string> ValidateTemplate(TransformTemplate template)
    {
        var errors = new List<string>();

        if (template == null)
        {
            errors.Add("Template cannot be null");
            return errors;
        }

        if (template.Mappings == null || template.Mappings.Count == 0)
        {
            errors.Add("Template must contain at least one mapping");
            return errors;
        }

        for (int i = 0; i < template.Mappings.Count; i++)
        {
            var mapping = template.Mappings[i];
            var mappingErrors = ValidateMapping(mapping, i);
            errors.AddRange(mappingErrors);
        }

        return errors;
    }

    private List<string> ValidateMapping(Mapping mapping, int index)
    {
        var errors = new List<string>();
        var prefix = $"Mapping[{index}]";

        if (string.IsNullOrEmpty(mapping.To))
        {
            errors.Add($"{prefix}: 'to' property is required");
        }

        // Check that at least one source is specified
        var sourceCount = 0;
        if (!string.IsNullOrEmpty(mapping.From)) sourceCount++;
        if (mapping.Value != null) sourceCount++;
        if (mapping.Math != null) sourceCount++;
        if (!string.IsNullOrEmpty(mapping.Concat)) sourceCount++;
        if (mapping.Template != null) sourceCount++;

        if (sourceCount == 0)
        {
            errors.Add($"{prefix}: Must specify at least one source (from, value, math, concat, or template)");
        }

        if (sourceCount > 1)
        {
            errors.Add($"{prefix}: Cannot specify multiple sources simultaneously");
        }

        // Validate mathematical operations
        if (mapping.Math != null)
        {
            if (string.IsNullOrEmpty(mapping.Math.Operation))
            {
                errors.Add($"{prefix}.math: 'operation' is required");
            }

            if (mapping.Math.Operands == null || mapping.Math.Operands.Count == 0)
            {
                errors.Add($"{prefix}.math: 'operands' must contain at least one value");
            }
        }

        // Validate conditions
        if (mapping.Conditions != null)
        {
            for (int i = 0; i < mapping.Conditions.Count; i++)
            {
                var condition = mapping.Conditions[i];
                if (string.IsNullOrEmpty(condition.If))
                {
                    errors.Add($"{prefix}.conditions[{i}]: 'if' property is required");
                }
            }
        }

        // Validate nested templates
        if (mapping.Template != null)
        {
            var nestedErrors = ValidateTemplate(mapping.Template);
            errors.AddRange(nestedErrors.Select(e => $"{prefix}.template: {e}"));
        }

        return errors;
    }

    /// <summary>
    /// Creates a simple field mapping transformation template
    /// </summary>
    /// <param name="mappings">Dictionary of from->to field mappings</param>
    /// <returns>A transformation template</returns>
    public static TransformTemplate CreateSimpleMapping(Dictionary<string, string> mappings)
    {
        var template = new TransformTemplate();
        
        foreach (var kvp in mappings)
        {
            template.Mappings.Add(new Mapping
            {
                From = kvp.Key,
                To = kvp.Value
            });
        }

        return template;
    }

    /// <summary>
    /// Creates a transformation template from a JSON string
    /// </summary>
    /// <param name="templateJson">The template JSON string</param>
    /// <returns>A transformation template</returns>
    public static TransformTemplate ParseTemplate(string templateJson)
    {
        if (string.IsNullOrEmpty(templateJson))
            throw new ArgumentException("Template JSON cannot be null or empty", nameof(templateJson));

        try
        {
            var template = JsonSerializer.Deserialize<TransformTemplate>(templateJson);
            return template ?? throw new TransformException("Failed to deserialize transformation template");
        }
        catch (JsonException ex)
        {
            throw new TransformException("Invalid template JSON format", ex);
        }
    }
}
