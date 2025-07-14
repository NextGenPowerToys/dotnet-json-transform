using System.Text.Json.Nodes;
using Json.Transform.Exceptions;

namespace Json.Transform.Core;

/// <summary>
/// Handles aggregation operations on arrays
/// </summary>
public class AggregationProcessor
{
    /// <summary>
    /// Performs aggregation on an array of JSON nodes
    /// </summary>
    /// <param name="array">The array to aggregate</param>
    /// <param name="operation">The aggregation operation (sum, avg, min, max, count, first, last, join)</param>
    /// <param name="path">Optional path within array items to aggregate</param>
    /// <param name="separator">Separator for join operations</param>
    /// <returns>The aggregated result</returns>
    public static JsonNode? Aggregate(JsonArray array, string operation, string? path = null, string? separator = null)
    {
        if (array == null || array.Count == 0)
        {
            return operation.ToLower() switch
            {
                "count" => JsonValue.Create(0),
                "sum" => JsonValue.Create(0),
                "join" => JsonValue.Create(""),
                _ => null
            };
        }

        try
        {
            return operation.ToLower() switch
            {
                "sum" => Sum(array, path),
                "avg" or "average" => Average(array, path),
                "min" => Min(array, path),
                "max" => Max(array, path),
                "count" => JsonValue.Create(array.Count),
                "first" => First(array, path),
                "last" => Last(array, path),
                "join" => Join(array, path, separator ?? ","),
                _ => throw new AggregationException(operation, $"Unknown aggregation operation: {operation}")
            };
        }
        catch (Exception ex) when (!(ex is AggregationException))
        {
            throw new AggregationException(operation, $"Failed to perform {operation} aggregation", ex);
        }
    }

    private static JsonNode? Sum(JsonArray array, string? path)
    {
        double sum = 0;
        var hasValues = false;

        foreach (var item in array)
        {
            var value = ExtractNumericValue(item, path);
            if (value.HasValue)
            {
                sum += value.Value;
                hasValues = true;
            }
        }

        return hasValues ? JsonValue.Create(sum) : JsonValue.Create(0);
    }

    private static JsonNode? Average(JsonArray array, string? path)
    {
        double sum = 0;
        int count = 0;

        foreach (var item in array)
        {
            var value = ExtractNumericValue(item, path);
            if (value.HasValue)
            {
                sum += value.Value;
                count++;
            }
        }

        return count > 0 ? JsonValue.Create(sum / count) : null;
    }

    private static JsonNode? Min(JsonArray array, string? path)
    {
        double? min = null;

        foreach (var item in array)
        {
            var value = ExtractNumericValue(item, path);
            if (value.HasValue)
            {
                if (!min.HasValue || value.Value < min.Value)
                    min = value.Value;
            }
        }

        return min.HasValue ? JsonValue.Create(min.Value) : null;
    }

    private static JsonNode? Max(JsonArray array, string? path)
    {
        double? max = null;

        foreach (var item in array)
        {
            var value = ExtractNumericValue(item, path);
            if (value.HasValue)
            {
                if (!max.HasValue || value.Value > max.Value)
                    max = value.Value;
            }
        }

        return max.HasValue ? JsonValue.Create(max.Value) : null;
    }

    private static JsonNode? First(JsonArray array, string? path)
    {
        if (array.Count == 0) return null;

        if (string.IsNullOrEmpty(path))
            return array[0];

        return PathResolver.ResolveSingle(array[0], path);
    }

    private static JsonNode? Last(JsonArray array, string? path)
    {
        if (array.Count == 0) return null;

        if (string.IsNullOrEmpty(path))
            return array[array.Count - 1];

        return PathResolver.ResolveSingle(array[array.Count - 1], path);
    }

    private static JsonNode? Join(JsonArray array, string? path, string separator)
    {
        var values = new List<string>();

        foreach (var item in array)
        {
            JsonNode? valueNode;
            if (string.IsNullOrEmpty(path))
            {
                valueNode = item;
            }
            else
            {
                valueNode = PathResolver.ResolveSingle(item, path);
            }

            if (valueNode != null)
            {
                var stringValue = ExtractStringValue(valueNode);
                if (!string.IsNullOrEmpty(stringValue))
                    values.Add(stringValue);
            }
        }

        return JsonValue.Create(string.Join(separator, values));
    }

    private static double? ExtractNumericValue(JsonNode? node, string? path)
    {
        JsonNode? targetNode;
        if (string.IsNullOrEmpty(path))
        {
            targetNode = node;
        }
        else
        {
            targetNode = PathResolver.ResolveSingle(node, path);
        }

        if (targetNode is JsonValue value)
        {
            if (value.TryGetValue<double>(out var doubleVal))
                return doubleVal;
            if (value.TryGetValue<int>(out var intVal))
                return intVal;
            if (value.TryGetValue<long>(out var longVal))
                return longVal;
            if (value.TryGetValue<decimal>(out var decimalVal))
                return (double)decimalVal;
            if (value.TryGetValue<float>(out var floatVal))
                return floatVal;

            // Try to parse string as number
            if (value.TryGetValue<string>(out var stringVal) && 
                double.TryParse(stringVal, out var parsedVal))
                return parsedVal;
        }

        return null;
    }

    private static string ExtractStringValue(JsonNode node)
    {
        return node switch
        {
            JsonValue value when value.TryGetValue<string>(out var str) => str,
            JsonValue value => value.ToString(),
            _ => node.ToString()
        };
    }
}
