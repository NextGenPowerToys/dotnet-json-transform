using System.Text.Json.Nodes;
using Json.Path;
using Json.Transform.Exceptions;

namespace Json.Transform.Core;

/// <summary>
/// Handles JSONPath expression evaluation and path manipulation
/// </summary>
public class PathResolver
{
    private static readonly Dictionary<string, JsonPath> _pathCache = new();
    private static readonly object _cacheLock = new();

    /// <summary>
    /// Resolves a JSONPath expression against a JSON node and returns the first matching value
    /// </summary>
    /// <param name="node">The JSON node to query</param>
    /// <param name="path">The JSONPath expression</param>
    /// <returns>The resolved value or null if not found</returns>
    public static JsonNode? ResolveSingle(JsonNode? node, string path)
    {
        if (node == null || string.IsNullOrEmpty(path))
            return null;

        try
        {
            var jsonPath = GetCompiledPath(path);
            var result = jsonPath.Evaluate(node);
            
            return result.Matches?.FirstOrDefault()?.Value;
        }
        catch (Exception ex)
        {
            throw new PathNotFoundException(path, ex);
        }
    }

    /// <summary>
    /// Resolves a JSONPath expression against a JSON node and returns all matching values
    /// </summary>
    /// <param name="node">The JSON node to query</param>
    /// <param name="path">The JSONPath expression</param>
    /// <returns>Array of resolved values</returns>
    public static JsonArray ResolveArray(JsonNode? node, string path)
    {
        if (node == null || string.IsNullOrEmpty(path))
            return new JsonArray();

        try
        {
            var jsonPath = GetCompiledPath(path);
            var result = jsonPath.Evaluate(node);
            
            var array = new JsonArray();
            if (result.Matches != null)
            {
                foreach (var match in result.Matches)
                {
                    if (match.Value != null)
                        array.Add(match.Value.DeepClone());
                }
            }
            
            return array;
        }
        catch (Exception ex)
        {
            throw new PathNotFoundException(path, ex);
        }
    }

    /// <summary>
    /// Sets a value at the specified JSONPath in the target node
    /// </summary>
    /// <param name="targetNode">The target JSON node to modify</param>
    /// <param name="path">The JSONPath expression</param>
    /// <param name="value">The value to set</param>
    /// <param name="createPath">Whether to create missing intermediate paths</param>
    public static void SetValue(JsonNode targetNode, string path, JsonNode? value, bool createPath = true)
    {
        if (targetNode == null || string.IsNullOrEmpty(path))
            return;

        try
        {
            // Handle root path
            if (path == "$" || path == "")
            {
                // Cannot replace root node directly
                return;
            }        // Parse the path to get segments
        var segments = ParsePathSegments(path);
        JsonNode? currentNode = targetNode;        // Navigate to the parent of the target location
        for (int i = 0; i < segments.Count - 1; i++)
        {
            var segment = segments[i];
            currentNode = NavigateOrCreateSegment(currentNode!, segment, createPath);
            
            if (currentNode == null)
                throw new PathNotFoundException(path, $"Cannot navigate to segment: {segment}");
        }        // Set the final value
        var finalSegment = segments.Last();
        SetSegmentValue(currentNode!, finalSegment, value);
        }
        catch (PathNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PathNotFoundException(path, $"Failed to set value at path: {path}", ex);
        }
    }

    /// <summary>
    /// Checks if a path exists in the given JSON node
    /// </summary>
    /// <param name="node">The JSON node to check</param>
    /// <param name="path">The JSONPath expression</param>
    /// <returns>True if the path exists, false otherwise</returns>
    public static bool PathExists(JsonNode? node, string path)
    {
        if (node == null || string.IsNullOrEmpty(path))
            return false;

        try
        {
            var jsonPath = GetCompiledPath(path);
            var result = jsonPath.Evaluate(node);
            return result.Matches?.Any() ?? false;
        }
        catch
        {
            return false;
        }
    }

    private static JsonPath GetCompiledPath(string path)
    {
        lock (_cacheLock)
        {
            if (!_pathCache.TryGetValue(path, out var jsonPath))
            {
                jsonPath = JsonPath.Parse(path);
                _pathCache[path] = jsonPath;
            }
            return jsonPath;
        }
    }

    private static List<string> ParsePathSegments(string path)
    {
        // Simple path segment parsing - this is a basic implementation
        // In a production environment, you might want to use a more robust parser
        var segments = new List<string>();
        
        // Remove the root '$' if present
        if (path.StartsWith("$."))
            path = path.Substring(2);
        else if (path.StartsWith("$"))
            path = path.Substring(1);

        if (string.IsNullOrEmpty(path))
            return segments;

        // Split by dots, handling array indexing
        var parts = path.Split('.');
        foreach (var part in parts)
        {
            if (!string.IsNullOrEmpty(part))
                segments.Add(part);
        }

        return segments;
    }

    private static JsonNode? NavigateOrCreateSegment(JsonNode currentNode, string segment, bool createPath)
    {
        // Handle array indexing
        if (segment.Contains('[') && segment.Contains(']'))
        {
            var propertyName = segment.Substring(0, segment.IndexOf('['));
            var indexPart = segment.Substring(segment.IndexOf('[') + 1, 
                segment.IndexOf(']') - segment.IndexOf('[') - 1);

            // Navigate to property first
            if (!string.IsNullOrEmpty(propertyName))
            {
                currentNode = NavigateToProperty(currentNode, propertyName, createPath);
                if (currentNode == null) return null;
            }

            // Handle array index
            if (int.TryParse(indexPart, out var index))
            {
                var array = currentNode.AsArray();
                if (index < array.Count)
                    return array[index];
                else if (createPath)
                {
                    // Expand array if needed
                    while (array.Count <= index)
                        array.Add(new JsonObject());
                    return array[index];
                }
            }
            
            return null;
        }
        else
        {
            return NavigateToProperty(currentNode, segment, createPath);
        }
    }

    private static JsonNode? NavigateToProperty(JsonNode currentNode, string propertyName, bool createPath)
    {
        if (currentNode is JsonObject obj)
        {
            if (obj.ContainsKey(propertyName))
                return obj[propertyName];
            else if (createPath)
            {
                var newObj = new JsonObject();
                obj[propertyName] = newObj;
                return newObj;
            }
        }
        
        return null;
    }

    private static void SetSegmentValue(JsonNode currentNode, string segment, JsonNode? value)
    {
        if (currentNode is JsonObject obj)
        {
            // Handle array indexing in final segment
            if (segment.Contains('[') && segment.Contains(']'))
            {
                var propertyName = segment.Substring(0, segment.IndexOf('['));
                var indexPart = segment.Substring(segment.IndexOf('[') + 1, 
                    segment.IndexOf(']') - segment.IndexOf('[') - 1);

                if (!string.IsNullOrEmpty(propertyName))
                {
                    // Ensure the property exists as an array
                    if (!obj.ContainsKey(propertyName))
                        obj[propertyName] = new JsonArray();
                    
                    if (int.TryParse(indexPart, out var index) && obj[propertyName] is JsonArray array)
                    {
                        // Expand array if needed
                        while (array.Count <= index)
                            array.Add(JsonValue.Create((object?)null));
                        
                        array[index] = value;
                    }
                }
            }
            else
            {
                obj[segment] = value;
            }
        }
    }
}
