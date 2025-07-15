using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Transform.Exceptions;
using Json.Transform.Models;

namespace Json.Transform.Core;

/// <summary>
/// Handles evaluation of conditional expressions with support for complex boolean logic
/// </summary>
public class ConditionEvaluator
{
    private static readonly Regex SimpleConditionRegex = new Regex(
        @"(?<left>[^\s>=<!]+)\s*(?<operator>>=|<=|==|!=|>|<|contains|startsWith|endsWith)\s*(?<right>.+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Evaluates a condition against the source JSON data
    /// </summary>
    /// <param name="condition">The condition to evaluate</param>
    /// <param name="sourceData">The source JSON data</param>
    /// <returns>The result value based on condition evaluation</returns>
    public static object? EvaluateCondition(Condition condition, JsonNode? sourceData)
    {
        if (condition.If == null)
            return condition.Then;

        try
        {
            var isTrue = EvaluateExpression(condition.If, sourceData);
            
            if (isTrue)
                return ResolveValue(condition.Then, sourceData);
            
            // Check elseif conditions
            if (condition.ElseIf != null)
            {
                foreach (var elseIfCondition in condition.ElseIf)
                {
                    var elseIfResult = EvaluateCondition(elseIfCondition, sourceData);
                    if (elseIfResult != null)
                        return elseIfResult;
                }
            }
            
            return ResolveValue(condition.Else, sourceData);
        }
        catch (Exception ex)
        {
            throw new InvalidConditionException(condition.If ?? "", ex);
        }
    }

    /// <summary>
    /// Evaluates a conditional expression with support for complex boolean logic (&amp;&amp;, ||)
    /// </summary>
    /// <param name="expression">The expression to evaluate</param>
    /// <param name="sourceData">The source JSON data</param>
    /// <returns>True if condition is met, false otherwise</returns>
    public static bool EvaluateExpression(string expression, JsonNode? sourceData)
    {
        if (string.IsNullOrEmpty(expression))
            return false;

        expression = expression.Trim();

        // Handle complex expressions with && and ||
        if (ContainsLogicalOperators(expression))
        {
            return EvaluateComplexExpression(expression, sourceData);
        }

        // Handle simple expressions
        return EvaluateSimpleExpression(expression, sourceData);
    }

    /// <summary>
    /// Checks if expression contains logical operators (&&, ||)
    /// </summary>
    private static bool ContainsLogicalOperators(string expression)
    {
        return expression.Contains("&&") || expression.Contains("||");
    }

    /// <summary>
    /// Evaluates complex expressions with boolean logic
    /// </summary>
    private static bool EvaluateComplexExpression(string expression, JsonNode? sourceData)
    {
        // Handle parentheses first
        expression = ResolveParentheses(expression, sourceData);

        // Split by || operators (lowest precedence)
        var orParts = SplitByOperator(expression, "||");
        if (orParts.Count > 1)
        {
            return orParts.Any(part => EvaluateExpression(part.Trim(), sourceData));
        }

        // Split by && operators (higher precedence)
        var andParts = SplitByOperator(expression, "&&");
        if (andParts.Count > 1)
        {
            return andParts.All(part => EvaluateExpression(part.Trim(), sourceData));
        }

        // If no logical operators found, evaluate as simple expression
        return EvaluateSimpleExpression(expression, sourceData);
    }

    /// <summary>
    /// Resolves expressions in parentheses
    /// </summary>
    private static string ResolveParentheses(string expression, JsonNode? sourceData)
    {
        while (expression.Contains('('))
        {
            var start = expression.LastIndexOf('(');
            var end = expression.IndexOf(')', start);
            
            if (end == -1)
                throw new InvalidConditionException(expression, "Mismatched parentheses");

            var innerExpression = expression.Substring(start + 1, end - start - 1);
            var result = EvaluateExpression(innerExpression, sourceData);
            
            expression = expression.Substring(0, start) + 
                        (result ? "true" : "false") + 
                        expression.Substring(end + 1);
        }
        return expression;
    }

    /// <summary>
    /// Splits expression by logical operator while respecting operator precedence
    /// </summary>
    private static List<string> SplitByOperator(string expression, string op)
    {
        var parts = new List<string>();
        var current = "";
        var i = 0;
        
        while (i < expression.Length)
        {
            if (i <= expression.Length - op.Length && 
                expression.Substring(i, op.Length) == op)
            {
                // Check if this operator is not part of a comparison operator
                if (!IsPartOfComparisonOperator(expression, i, op))
                {
                    parts.Add(current.Trim());
                    current = "";
                    i += op.Length;
                    continue;
                }
            }
            current += expression[i];
            i++;
        }
        
        parts.Add(current.Trim());
        return parts.Where(p => !string.IsNullOrEmpty(p)).ToList();
    }

    /// <summary>
    /// Checks if the operator at position is part of a comparison operator (like >= or <=)
    /// </summary>
    private static bool IsPartOfComparisonOperator(string expression, int position, string op)
    {
        if (op != "&" && op != "|") return false;
        
        // Check if preceded or followed by =, >, <
        if (position > 0)
        {
            var prev = expression[position - 1];
            if (prev == '>' || prev == '<' || prev == '=' || prev == '!')
                return true;
        }
        
        if (position < expression.Length - 1)
        {
            var next = expression[position + 1];
            if (next == '=' || next == '>' || next == '<')
                return true;
        }
        
        return false;
    }

    /// <summary>
    /// Evaluates a simple conditional expression (no boolean operators)
    /// </summary>
    private static bool EvaluateSimpleExpression(string expression, JsonNode? sourceData)
    {
        // Handle boolean literals
        if (expression.Equals("true", StringComparison.OrdinalIgnoreCase))
            return true;
        if (expression.Equals("false", StringComparison.OrdinalIgnoreCase))
            return false;

        var match = SimpleConditionRegex.Match(expression);
        if (!match.Success)
            throw new InvalidConditionException(expression, "Invalid condition format");

        var leftSide = match.Groups["left"].Value.Trim();
        var operatorStr = match.Groups["operator"].Value.Trim();
        var rightSide = match.Groups["right"].Value.Trim();

        var leftValue = ResolveValue(leftSide, sourceData);
        var rightValue = ResolveValue(rightSide, sourceData);

        return EvaluateComparison(leftValue, operatorStr, rightValue);
    }

    private static object? ResolveValue(object? value, JsonNode? sourceData)
    {
        if (value == null)
            return null;

        var valueStr = value.ToString();
        if (string.IsNullOrEmpty(valueStr))
            return null;

        // Check if it's a JSONPath expression
        if (valueStr.StartsWith("$."))
        {
            var resolved = PathResolver.ResolveSingle(sourceData, valueStr);
            return ExtractValue(resolved);
        }

        // Check if it's a quoted string
        if ((valueStr.StartsWith("\"") && valueStr.EndsWith("\"")) ||
            (valueStr.StartsWith("'") && valueStr.EndsWith("'")))
        {
            return valueStr.Substring(1, valueStr.Length - 2);
        }

        // Try to parse as number
        if (double.TryParse(valueStr, out var numValue))
            return numValue;

        // Try to parse as boolean
        if (bool.TryParse(valueStr, out var boolValue))
            return boolValue;

        // Return as string
        return valueStr;
    }

    private static object? ExtractValue(JsonNode? node)
    {
        if (node == null)
            return null;

        return node switch
        {
            JsonValue value when value.TryGetValue<string>(out var str) => str,
            JsonValue value when value.TryGetValue<double>(out var num) => num,
            JsonValue value when value.TryGetValue<bool>(out var boolean) => boolean,
            JsonValue value when value.TryGetValue<int>(out var integer) => integer,
            JsonValue value when value.TryGetValue<long>(out var longValue) => longValue,
            JsonValue value when value.TryGetValue<decimal>(out var decimalValue) => decimalValue,
            _ => node.ToString()
        };
    }

    private static bool EvaluateComparison(object? left, string operatorStr, object? right)
    {
        // Handle null comparisons
        if (left == null && right == null)
            return operatorStr is "==" or "<==" or ">==";
        
        if (left == null || right == null)
            return operatorStr is "!=" or "!==" or "<>";

        // Convert to comparable types
        var (leftComparable, rightComparable) = ConvertToComparableTypes(left, right);

        return operatorStr.ToLower() switch
        {
            "==" or "===" => AreEqual(leftComparable, rightComparable),
            "!=" or "!==" or "<>" => !AreEqual(leftComparable, rightComparable),
            ">" => Compare(leftComparable, rightComparable) > 0,
            ">=" => Compare(leftComparable, rightComparable) >= 0,
            "<" => Compare(leftComparable, rightComparable) < 0,
            "<=" => Compare(leftComparable, rightComparable) <= 0,
            "contains" => Contains(leftComparable, rightComparable),
            "startswith" => StartsWith(leftComparable, rightComparable),
            "endswith" => EndsWith(leftComparable, rightComparable),
            _ => throw new InvalidConditionException($"Unknown operator: {operatorStr}")
        };
    }

    private static (object?, object?) ConvertToComparableTypes(object left, object right)
    {
        // If both are numbers, ensure they're the same numeric type
        if (IsNumeric(left) && IsNumeric(right))
        {
            var leftNum = Convert.ToDouble(left);
            var rightNum = Convert.ToDouble(right);
            return (leftNum, rightNum);
        }

        // Convert both to strings for string operations
        return (left.ToString(), right.ToString());
    }

    private static bool IsNumeric(object value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        
        return left.Equals(right);
    }

    private static int Compare(object? left, object? right)
    {
        if (left == null && right == null) return 0;
        if (left == null) return -1;
        if (right == null) return 1;

        if (left is IComparable leftComparable && right is IComparable)
        {
            return leftComparable.CompareTo(right);
        }

        return string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool Contains(object? left, object? right)
    {
        if (left == null || right == null) return false;
        
        return left.ToString()?.Contains(right.ToString() ?? "", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private static bool StartsWith(object? left, object? right)
    {
        if (left == null || right == null) return false;
        
        return left.ToString()?.StartsWith(right.ToString() ?? "", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private static bool EndsWith(object? left, object? right)
    {
        if (left == null || right == null) return false;
        
        return left.ToString()?.EndsWith(right.ToString() ?? "", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
