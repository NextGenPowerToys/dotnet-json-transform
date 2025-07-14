using System.Text.Json.Nodes;
using Json.Transform.Exceptions;
using Json.Transform.Models;

namespace Json.Transform.Core;

/// <summary>
/// Handles mathematical operations on numeric values
/// </summary>
public class MathProcessor
{
    /// <summary>
    /// Performs a mathematical operation
    /// </summary>
    /// <param name="mathOperation">The mathematical operation configuration</param>
    /// <param name="sourceData">The source JSON data for resolving JSONPath expressions</param>
    /// <returns>The result of the mathematical operation</returns>
    public static JsonNode? PerformOperation(MathOperation mathOperation, JsonNode? sourceData)
    {
        if (mathOperation?.Operands == null || mathOperation.Operands.Count == 0)
            return null;

        try
        {
            var operation = mathOperation.Operation.ToLower();
            var operands = ResolveOperands(mathOperation.Operands, sourceData);

            return operation switch
            {
                "add" or "+" => Add(operands),
                "subtract" or "-" => Subtract(operands),
                "multiply" or "*" => Multiply(operands),
                "divide" or "/" => Divide(operands),
                "power" or "pow" or "^" => Power(operands),
                "sqrt" => SquareRoot(operands),
                "abs" => Absolute(operands),
                "round" => Round(operands, mathOperation.Precision ?? 0),
                "ceil" or "ceiling" => Ceiling(operands),
                "floor" => Floor(operands),
                "mod" or "%" => Modulo(operands),
                _ => throw new MathOperationException(operation, $"Unknown math operation: {operation}")
            };
        }
        catch (Exception ex) when (!(ex is MathOperationException))
        {
            throw new MathOperationException(mathOperation.Operation, $"Failed to perform {mathOperation.Operation} operation", ex);
        }
    }

    /// <summary>
    /// Performs a simple mathematical operation with two operands
    /// </summary>
    /// <param name="operation">The operation to perform</param>
    /// <param name="left">Left operand (can be JSONPath or numeric value)</param>
    /// <param name="right">Right operand (can be JSONPath or numeric value)</param>
    /// <param name="sourceData">The source JSON data</param>
    /// <returns>The result of the operation</returns>
    public static JsonNode? PerformSimpleOperation(string operation, object left, object right, JsonNode? sourceData)
    {
        var mathOp = new MathOperation
        {
            Operation = operation,
            Operands = new List<object> { left, right }
        };

        return PerformOperation(mathOp, sourceData);
    }

    private static List<double> ResolveOperands(List<object> operands, JsonNode? sourceData)
    {
        var resolvedOperands = new List<double>();

        foreach (var operand in operands)
        {
            var numericValue = ResolveNumericValue(operand, sourceData);
            if (numericValue.HasValue)
                resolvedOperands.Add(numericValue.Value);
        }

        return resolvedOperands;
    }

    private static double? ResolveNumericValue(object operand, JsonNode? sourceData)
    {
        if (operand == null)
            return null;

        // If it's already a number
        if (IsNumeric(operand))
            return Convert.ToDouble(operand);

        var operandStr = operand.ToString();
        if (string.IsNullOrEmpty(operandStr))
            return null;

        // Check if it's a JSONPath expression
        if (operandStr.StartsWith("$."))
        {
            var resolved = PathResolver.ResolveSingle(sourceData, operandStr);
            return ExtractNumericValue(resolved);
        }

        // Try to parse as number
        if (double.TryParse(operandStr, out var numValue))
            return numValue;

        return null;
    }

    private static double? ExtractNumericValue(JsonNode? node)
    {
        if (node is JsonValue value)
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

    private static bool IsNumeric(object value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    private static JsonNode Add(List<double> operands)
    {
        if (operands.Count == 0) return JsonValue.Create(0);
        
        var result = operands[0];
        for (int i = 1; i < operands.Count; i++)
            result += operands[i];
        
        return JsonValue.Create(result);
    }

    private static JsonNode Subtract(List<double> operands)
    {
        if (operands.Count == 0) return JsonValue.Create(0);
        if (operands.Count == 1) return JsonValue.Create(-operands[0]);
        
        var result = operands[0];
        for (int i = 1; i < operands.Count; i++)
            result -= operands[i];
        
        return JsonValue.Create(result);
    }

    private static JsonNode Multiply(List<double> operands)
    {
        if (operands.Count == 0) return JsonValue.Create(0);
        
        var result = operands[0];
        for (int i = 1; i < operands.Count; i++)
            result *= operands[i];
        
        return JsonValue.Create(result);
    }

    private static JsonNode Divide(List<double> operands)
    {
        if (operands.Count < 2)
            throw new MathOperationException("divide", "Division requires at least 2 operands");
        
        var result = operands[0];
        for (int i = 1; i < operands.Count; i++)
        {
            if (operands[i] == 0)
                throw new MathOperationException("divide", "Division by zero");
            result /= operands[i];
        }
        
        return JsonValue.Create(result);
    }

    private static JsonNode Power(List<double> operands)
    {
        if (operands.Count != 2)
            throw new MathOperationException("power", "Power operation requires exactly 2 operands");
        
        var result = Math.Pow(operands[0], operands[1]);
        return JsonValue.Create(result);
    }

    private static JsonNode SquareRoot(List<double> operands)
    {
        if (operands.Count != 1)
            throw new MathOperationException("sqrt", "Square root operation requires exactly 1 operand");
        
        if (operands[0] < 0)
            throw new MathOperationException("sqrt", "Cannot take square root of negative number");
        
        var result = Math.Sqrt(operands[0]);
        return JsonValue.Create(result);
    }

    private static JsonNode Absolute(List<double> operands)
    {
        if (operands.Count != 1)
            throw new MathOperationException("abs", "Absolute value operation requires exactly 1 operand");
        
        var result = Math.Abs(operands[0]);
        return JsonValue.Create(result);
    }

    private static JsonNode Round(List<double> operands, int precision)
    {
        if (operands.Count != 1)
            throw new MathOperationException("round", "Round operation requires exactly 1 operand");
        
        var result = Math.Round(operands[0], precision);
        return JsonValue.Create(result);
    }

    private static JsonNode Ceiling(List<double> operands)
    {
        if (operands.Count != 1)
            throw new MathOperationException("ceil", "Ceiling operation requires exactly 1 operand");
        
        var result = Math.Ceiling(operands[0]);
        return JsonValue.Create(result);
    }

    private static JsonNode Floor(List<double> operands)
    {
        if (operands.Count != 1)
            throw new MathOperationException("floor", "Floor operation requires exactly 1 operand");
        
        var result = Math.Floor(operands[0]);
        return JsonValue.Create(result);
    }

    private static JsonNode Modulo(List<double> operands)
    {
        if (operands.Count != 2)
            throw new MathOperationException("mod", "Modulo operation requires exactly 2 operands");
        
        if (operands[1] == 0)
            throw new MathOperationException("mod", "Modulo by zero");
        
        var result = operands[0] % operands[1];
        return JsonValue.Create(result);
    }
}
