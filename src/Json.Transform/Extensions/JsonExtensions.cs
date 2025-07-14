using System.Text.Json.Nodes;

namespace Json.Transform.Extensions;

/// <summary>
/// Extension methods for JSON manipulation and utilities
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Safely gets a string value from a JsonNode with a default fallback
    /// </summary>
    /// <param name="node">The JSON node</param>
    /// <param name="defaultValue">Default value if node is null or conversion fails</param>
    /// <returns>The string value or default</returns>
    public static string GetStringOrDefault(this JsonNode? node, string defaultValue = "")
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var stringValue))
            return stringValue;

        return node?.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Safely gets an integer value from a JsonNode with a default fallback
    /// </summary>
    /// <param name="node">The JSON node</param>
    /// <param name="defaultValue">Default value if node is null or conversion fails</param>
    /// <returns>The integer value or default</returns>
    public static int GetIntOrDefault(this JsonNode? node, int defaultValue = 0)
    {
        if (node is JsonValue value)
        {
            if (value.TryGetValue<int>(out var intValue))
                return intValue;
            
            if (value.TryGetValue<string>(out var stringValue) && 
                int.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Safely gets a double value from a JsonNode with a default fallback
    /// </summary>
    /// <param name="node">The JSON node</param>
    /// <param name="defaultValue">Default value if node is null or conversion fails</param>
    /// <returns>The double value or default</returns>
    public static double GetDoubleOrDefault(this JsonNode? node, double defaultValue = 0.0)
    {
        if (node is JsonValue value)
        {
            if (value.TryGetValue<double>(out var doubleValue))
                return doubleValue;
            
            if (value.TryGetValue<int>(out var intValue))
                return intValue;
            
            if (value.TryGetValue<string>(out var stringValue) && 
                double.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Safely gets a boolean value from a JsonNode with a default fallback
    /// </summary>
    /// <param name="node">The JSON node</param>
    /// <param name="defaultValue">Default value if node is null or conversion fails</param>
    /// <returns>The boolean value or default</returns>
    public static bool GetBoolOrDefault(this JsonNode? node, bool defaultValue = false)
    {
        if (node is JsonValue value)
        {
            if (value.TryGetValue<bool>(out var boolValue))
                return boolValue;
            
            if (value.TryGetValue<string>(out var stringValue))
            {
                return stringValue.ToLower() switch
                {
                    "true" or "1" or "yes" or "on" => true,
                    "false" or "0" or "no" or "off" => false,
                    _ => defaultValue
                };
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Checks if a JsonNode represents a null or empty value
    /// </summary>
    /// <param name="node">The JSON node to check</param>
    /// <returns>True if the node is null, undefined, or represents an empty value</returns>
    public static bool IsNullOrEmpty(this JsonNode? node)
    {
        if (node == null)
            return true;

        if (node is JsonValue value)
        {
            if (value.TryGetValue<string>(out var stringValue))
                return string.IsNullOrEmpty(stringValue);
            
            // Check for null JSON value
            try
            {
                return value.GetValue<object>() == null;
            }
            catch
            {
                return false;
            }
        }

        if (node is JsonArray array)
            return array.Count == 0;

        if (node is JsonObject obj)
            return obj.Count == 0;

        return false;
    }

    /// <summary>
    /// Checks if a JsonNode has a specific property
    /// </summary>
    /// <param name="node">The JSON node to check</param>
    /// <param name="propertyName">The property name to look for</param>
    /// <returns>True if the property exists</returns>
    public static bool HasProperty(this JsonNode? node, string propertyName)
    {
        if (node is JsonObject obj)
            return obj.ContainsKey(propertyName);

        return false;
    }

    /// <summary>
    /// Safely gets a property value from a JsonObject
    /// </summary>
    /// <param name="node">The JSON node</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The property value or null if not found</returns>
    public static JsonNode? GetProperty(this JsonNode? node, string propertyName)
    {
        if (node is JsonObject obj && obj.ContainsKey(propertyName))
            return obj[propertyName];

        return null;
    }

    /// <summary>
    /// Converts a JsonNode to a strongly-typed value
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <param name="node">The JSON node</param>
    /// <param name="defaultValue">Default value if conversion fails</param>
    /// <returns>The converted value or default</returns>
    public static T? GetValueOrDefault<T>(this JsonNode? node, T? defaultValue = default)
    {
        if (node is JsonValue value)
        {
            try
            {
                return value.GetValue<T>();
            }
            catch
            {
                // Try string conversion for complex types
                if (typeof(T) == typeof(string))
                    return (T?)(object?)node.ToString();
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Creates a deep copy of a JsonNode
    /// </summary>
    /// <param name="node">The JSON node to clone</param>
    /// <returns>A deep copy of the node</returns>
    public static JsonNode? DeepClone(this JsonNode? node)
    {
        if (node == null)
            return null;

        return JsonNode.Parse(node.ToJsonString());
    }

    /// <summary>
    /// Merges two JsonObjects, with the second object taking precedence
    /// </summary>
    /// <param name="target">The target object to merge into</param>
    /// <param name="source">The source object to merge from</param>
    /// <param name="deepMerge">Whether to perform deep merging of nested objects</param>
    public static void Merge(this JsonObject target, JsonObject? source, bool deepMerge = true)
    {
        if (source == null)
            return;

        foreach (var kvp in source)
        {
            if (deepMerge && target.ContainsKey(kvp.Key) && 
                target[kvp.Key] is JsonObject targetObj && 
                kvp.Value is JsonObject sourceObj)
            {
                targetObj.Merge(sourceObj, deepMerge);
            }
            else
            {
                target[kvp.Key] = kvp.Value?.DeepClone();
            }
        }
    }

    /// <summary>
    /// Flattens a nested JSON object to a dictionary with dot-notation keys
    /// </summary>
    /// <param name="node">The JSON node to flatten</param>
    /// <param name="prefix">The prefix for keys (used internally for recursion)</param>
    /// <returns>A dictionary with flattened key-value pairs</returns>
    public static Dictionary<string, object?> Flatten(this JsonNode? node, string prefix = "")
    {
        var result = new Dictionary<string, object?>();

        if (node == null)
            return result;

        switch (node)
        {
            case JsonObject obj:
                foreach (var kvp in obj)
                {
                    var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
                    var flattened = kvp.Value.Flatten(key);
                    foreach (var flatKvp in flattened)
                        result[flatKvp.Key] = flatKvp.Value;
                }
                break;

            case JsonArray array:
                for (int i = 0; i < array.Count; i++)
                {
                    var key = string.IsNullOrEmpty(prefix) ? $"[{i}]" : $"{prefix}[{i}]";
                    var flattened = array[i].Flatten(key);
                    foreach (var flatKvp in flattened)
                        result[flatKvp.Key] = flatKvp.Value;
                }
                break;

            case JsonValue value:
                result[prefix] = value.GetValue<object>();
                break;
        }

        return result;
    }

    /// <summary>
    /// Validates that a JsonNode matches a basic schema
    /// </summary>
    /// <param name="node">The JSON node to validate</param>
    /// <param name="requiredProperties">List of required property names for objects</param>
    /// <returns>True if the node is valid according to the schema</returns>
    public static bool ValidateSchema(this JsonNode? node, params string[] requiredProperties)
    {
        if (node is JsonObject obj)
        {
            return requiredProperties.All(prop => obj.ContainsKey(prop));
        }

        return requiredProperties.Length == 0;
    }

    /// <summary>
    /// Gets all property names from a JsonObject
    /// </summary>
    /// <param name="node">The JSON node</param>
    /// <returns>Collection of property names</returns>
    public static IEnumerable<string> GetPropertyNames(this JsonNode? node)
    {
        if (node is JsonObject obj)
            return obj.Select(kvp => kvp.Key);

        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Counts the number of elements in a JsonArray or properties in a JsonObject
    /// </summary>
    /// <param name="node">The JSON node</param>
    /// <returns>The count of elements/properties</returns>
    public static int Count(this JsonNode? node)
    {
        return node switch
        {
            JsonArray array => array.Count,
            JsonObject obj => obj.Count,
            _ => 0
        };
    }
}
