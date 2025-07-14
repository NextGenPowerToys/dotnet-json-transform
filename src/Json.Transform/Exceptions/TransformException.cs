namespace Json.Transform.Exceptions;

/// <summary>
/// Base exception for all transformation-related errors
/// </summary>
public class TransformException : Exception
{
    public TransformException() : base() { }

    public TransformException(string message) : base(message) { }

    public TransformException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a JSONPath expression cannot be resolved
/// </summary>
public class PathNotFoundException : TransformException
{
    public string Path { get; }

    public PathNotFoundException(string path) : base($"Path not found: {path}")
    {
        Path = path;
    }

    public PathNotFoundException(string path, string message) : base(message)
    {
        Path = path;
    }

    public PathNotFoundException(string path, string message, Exception innerException) 
        : base(message, innerException)
    {
        Path = path;
    }

    public PathNotFoundException(string path, Exception innerException) 
        : base($"Path not found: {path}", innerException)
    {
        Path = path;
    }
}

/// <summary>
/// Exception thrown when a conditional expression is invalid or cannot be evaluated
/// </summary>
public class InvalidConditionException : TransformException
{
    public string Condition { get; }

    public InvalidConditionException(string condition) : base($"Invalid condition: {condition}")
    {
        Condition = condition;
    }

    public InvalidConditionException(string condition, string message) : base(message)
    {
        Condition = condition;
    }

    public InvalidConditionException(string condition, Exception innerException) 
        : base($"Invalid condition: {condition}", innerException)
    {
        Condition = condition;
    }
}

/// <summary>
/// Exception thrown when a mathematical operation fails
/// </summary>
public class MathOperationException : TransformException
{
    public string Operation { get; }

    public MathOperationException(string operation) : base($"Math operation failed: {operation}")
    {
        Operation = operation;
    }

    public MathOperationException(string operation, string message) : base(message)
    {
        Operation = operation;
    }

    public MathOperationException(string operation, string message, Exception innerException) 
        : base(message, innerException)
    {
        Operation = operation;
    }

    public MathOperationException(string operation, Exception innerException) 
        : base($"Math operation failed: {operation}", innerException)
    {
        Operation = operation;
    }
}

/// <summary>
/// Exception thrown when an aggregation operation fails
/// </summary>
public class AggregationException : TransformException
{
    public string Operation { get; }

    public AggregationException(string operation) : base($"Aggregation failed: {operation}")
    {
        Operation = operation;
    }

    public AggregationException(string operation, string message) : base(message)
    {
        Operation = operation;
    }

    public AggregationException(string operation, string message, Exception innerException) 
        : base(message, innerException)
    {
        Operation = operation;
    }

    public AggregationException(string operation, Exception innerException) 
        : base($"Aggregation failed: {operation}", innerException)
    {
        Operation = operation;
    }
}
