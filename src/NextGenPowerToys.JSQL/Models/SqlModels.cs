using System.Text.Json.Nodes;

namespace NextGenPowerToys.JSQL.Models
{
    /// <summary>
    /// Represents the result of SQL query analysis
    /// </summary>
    public class SqlAnalysisResult
    {
        /// <summary>
        /// Type of SQL query (SELECT, INSERT, UPDATE, etc.)
        /// </summary>
        public SqlQueryType QueryType { get; set; }

        /// <summary>
        /// Tables referenced in the query
        /// </summary>
        public List<SqlTable> Tables { get; set; } = new();

        /// <summary>
        /// Fields selected in the query
        /// </summary>
        public List<SqlField> SelectFields { get; set; } = new();

        /// <summary>
        /// All fields referenced in the query (alias for SelectFields for compatibility)
        /// </summary>
        public List<SqlField> Fields => SelectFields;

        /// <summary>
        /// WHERE clause conditions
        /// </summary>
        public List<SqlCondition> WhereConditions { get; set; } = new();

        /// <summary>
        /// All conditions in the query (alias for WhereConditions for compatibility)
        /// </summary>
        public List<SqlCondition> Conditions => WhereConditions;

        /// <summary>
        /// JOIN clauses
        /// </summary>
        public List<SqlJoin> Joins { get; set; } = new();

        /// <summary>
        /// GROUP BY fields
        /// </summary>
        public List<SqlField> GroupByFields { get; set; } = new();

        /// <summary>
        /// ORDER BY specifications
        /// </summary>
        public List<SqlOrderBy> OrderByFields { get; set; } = new();

        /// <summary>
        /// Aggregate functions used
        /// </summary>
        public List<SqlAggregate> Aggregates { get; set; } = new();

        /// <summary>
        /// All functions used in the query (alias for Aggregates for compatibility)
        /// </summary>
        public List<SqlAggregate> Functions => Aggregates;

        /// <summary>
        /// HAVING clause conditions
        /// </summary>
        public SqlHaving? Having { get; set; }

        /// <summary>
        /// LIMIT/TOP clause
        /// </summary>
        public SqlLimit? Limit { get; set; }

        /// <summary>
        /// Raw SQL query text
        /// </summary>
        public string RawSql { get; set; } = string.Empty;

        /// <summary>
        /// Original query text (alias for RawSql for compatibility)
        /// </summary>
        public string OriginalQuery => RawSql;

        /// <summary>
        /// Complexity score of the query
        /// </summary>
        public double ComplexityScore { get; set; }

        /// <summary>
        /// Query complexity (alias for ComplexityScore for compatibility)
        /// </summary>
        public double Complexity => ComplexityScore;

        /// <summary>
        /// Whether the SQL query is valid
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Validation errors if any
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// ORDER BY fields (compatibility property)
        /// </summary>
        public List<string> OrderBy => OrderByFields.Select(o => o.Field).ToList();

        /// <summary>
        /// GROUP BY fields (compatibility property)
        /// </summary>
        public List<string> GroupBy => GroupByFields.Select(g => g.Name).ToList();
    }

    /// <summary>
    /// Types of SQL queries
    /// </summary>
    public enum SqlQueryType
    {
        Select,
        Insert,
        Update,
        Delete,
        Union,
        With // Common Table Expression
    }

    /// <summary>
    /// Represents a table in SQL query
    /// </summary>
    public class SqlTable
    {
        /// <summary>
        /// Table name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Table alias
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Schema name if specified
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Whether this is a subquery
        /// </summary>
        public bool IsSubquery { get; set; }

        /// <summary>
        /// Subquery details if applicable
        /// </summary>
        public SqlAnalysisResult? Subquery { get; set; }
    }

    /// <summary>
    /// Represents a field in SQL query
    /// </summary>
    public class SqlField
    {
        /// <summary>
        /// Field name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Field alias
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Table alias this field belongs to
        /// </summary>
        public string? TableAlias { get; set; }

        /// <summary>
        /// Table reference (alias for TableAlias for compatibility)
        /// </summary>
        public string? TableReference => TableAlias;

        /// <summary>
        /// Data type if determinable
        /// </summary>
        public string? DataType { get; set; }

        /// <summary>
        /// Whether this is a calculated field
        /// </summary>
        public bool IsCalculated { get; set; }

        /// <summary>
        /// Whether this is an aggregate function
        /// </summary>
        public bool IsAggregate { get; set; }

        /// <summary>
        /// SQL expression for calculated fields
        /// </summary>
        public SqlExpression? Expression { get; set; }

        /// <summary>
        /// Aggregate function details
        /// </summary>
        public SqlAggregate? AggregateFunction { get; set; }

        /// <summary>
        /// Whether this is a simple field reference
        /// </summary>
        public bool IsSimpleField => !IsCalculated && !IsAggregate && Expression == null;
    }

    /// <summary>
    /// Represents a SQL expression
    /// </summary>
    public class SqlExpression
    {
        /// <summary>
        /// Type of expression
        /// </summary>
        public SqlExpressionType Type { get; set; }

        /// <summary>
        /// Expression operator
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// Left operand
        /// </summary>
        public object? LeftOperand { get; set; }

        /// <summary>
        /// Right operand
        /// </summary>
        public object? RightOperand { get; set; }

        /// <summary>
        /// Function name for function expressions
        /// </summary>
        public string? FunctionName { get; set; }

        /// <summary>
        /// Function arguments
        /// </summary>
        public List<object> Arguments { get; set; } = new();

        /// <summary>
        /// Raw expression text
        /// </summary>
        public string RawExpression { get; set; } = string.Empty;
    }

    /// <summary>
    /// Types of SQL expressions
    /// </summary>
    public enum SqlExpressionType
    {
        Literal,
        FieldReference,
        Function,
        BinaryOperation,
        UnaryOperation,
        CaseStatement,
        Subquery
    }

    /// <summary>
    /// Represents a SQL condition
    /// </summary>
    public class SqlCondition
    {
        /// <summary>
        /// Left side of the condition
        /// </summary>
        public string LeftField { get; set; } = string.Empty;

        /// <summary>
        /// Field name (compatibility property)
        /// </summary>
        public string Field => LeftField;

        /// <summary>
        /// Comparison operator
        /// </summary>
        public SqlOperator Operator { get; set; }

        /// <summary>
        /// Right side value
        /// </summary>
        public object? RightValue { get; set; }

        /// <summary>
        /// Value (compatibility property)
        /// </summary>
        public object? Value => RightValue;

        /// <summary>
        /// Multiple values for IN/NOT IN operations
        /// </summary>
        public List<object> Values { get; set; } = new();

        /// <summary>
        /// Logical operator connecting to next condition
        /// </summary>
        public SqlLogicalOperator? LogicalOperator { get; set; }

        /// <summary>
        /// Nested conditions for complex logic
        /// </summary>
        public List<SqlCondition> NestedConditions { get; set; } = new();

        /// <summary>
        /// Raw condition text
        /// </summary>
        public string RawCondition { get; set; } = string.Empty;
    }

    /// <summary>
    /// SQL comparison operators
    /// </summary>
    public enum SqlOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Like,
        NotLike,
        In,
        NotIn,
        Between,
        NotBetween,
        IsNull,
        IsNotNull
    }

    /// <summary>
    /// SQL logical operators
    /// </summary>
    public enum SqlLogicalOperator
    {
        And,
        Or,
        Not
    }

    /// <summary>
    /// Represents a SQL JOIN
    /// </summary>
    public class SqlJoin
    {
        /// <summary>
        /// Type of JOIN
        /// </summary>
        public SqlJoinType JoinType { get; set; }

        /// <summary>
        /// Left table
        /// </summary>
        public string LeftTable { get; set; } = string.Empty;

        /// <summary>
        /// Right table
        /// </summary>
        public string RightTable { get; set; } = string.Empty;

        /// <summary>
        /// JOIN condition
        /// </summary>
        public SqlCondition? OnCondition { get; set; }

        /// <summary>
        /// Left table alias
        /// </summary>
        public string? LeftAlias { get; set; }

        /// <summary>
        /// Right table alias
        /// </summary>
        public string? RightAlias { get; set; }
    }

    /// <summary>
    /// Types of SQL JOINs
    /// </summary>
    public enum SqlJoinType
    {
        Inner,
        Left,
        Right,
        Full,
        Cross
    }

    /// <summary>
    /// Represents an ORDER BY clause
    /// </summary>
    public class SqlOrderBy
    {
        /// <summary>
        /// Field to order by
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Sort direction
        /// </summary>
        public SqlSortDirection Direction { get; set; }

        /// <summary>
        /// Table alias if specified
        /// </summary>
        public string? TableAlias { get; set; }
    }

    /// <summary>
    /// Sort directions
    /// </summary>
    public enum SqlSortDirection
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// Represents an aggregate function
    /// </summary>
    public class SqlAggregate
    {
        /// <summary>
        /// Aggregate function type
        /// </summary>
        public SqlAggregateType Type { get; set; }

        /// <summary>
        /// Function name (compatibility property)
        /// </summary>
        public string Function => Type.ToString();

        /// <summary>
        /// Field to aggregate
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Table alias
        /// </summary>
        public string? TableAlias { get; set; }

        /// <summary>
        /// Whether DISTINCT is used
        /// </summary>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// Conditions for conditional aggregation
        /// </summary>
        public List<SqlCondition> Conditions { get; set; } = new();

        /// <summary>
        /// Alias for the aggregate result
        /// </summary>
        public string? Alias { get; set; }
    }

    /// <summary>
    /// Types of aggregate functions
    /// </summary>
    public enum SqlAggregateType
    {
        Count,
        Sum,
        Avg,
        Min,
        Max,
        First,
        Last
    }

    /// <summary>
    /// Represents a HAVING clause
    /// </summary>
    public class SqlHaving
    {
        /// <summary>
        /// Conditions in the HAVING clause
        /// </summary>
        public List<SqlCondition> Conditions { get; set; } = new();

        /// <summary>
        /// Raw HAVING clause text
        /// </summary>
        public string RawHaving { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a LIMIT/TOP clause
    /// </summary>
    public class SqlLimit
    {
        /// <summary>
        /// Number of rows to limit
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Offset for pagination
        /// </summary>
        public int? Offset { get; set; }
    }
}
