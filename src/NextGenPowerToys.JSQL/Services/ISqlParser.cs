using NextGenPowerToys.JSQL.Models;
using Microsoft.Extensions.Logging;

namespace NextGenPowerToys.JSQL.Services
{
    /// <summary>
    /// Service for parsing and analyzing SQL queries
    /// </summary>
    public interface ISqlParser
    {
        /// <summary>
        /// Parses a SQL query and extracts its structure
        /// </summary>
        /// <param name="sqlQuery">The SQL query to parse</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>SQL analysis result</returns>
        Task<SqlAnalysisResult> ParseQueryAsync(string sqlQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates SQL syntax
        /// </summary>
        /// <param name="sqlQuery">The SQL query to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if syntax is valid, false otherwise</returns>
        Task<bool> ValidateSyntaxAsync(string sqlQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts all field references from a SQL query
        /// </summary>
        /// <param name="sqlQuery">The SQL query to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of referenced fields</returns>
        Task<List<SqlField>> ExtractFieldsAsync(string sqlQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Identifies SQL functions used in the query
        /// </summary>
        /// <param name="sqlQuery">The SQL query to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of SQL functions</returns>
        Task<List<SqlFunction>> ExtractFunctionsAsync(string sqlQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes query complexity
        /// </summary>
        /// <param name="sqlQuery">The SQL query to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Complexity assessment</returns>
        Task<ComplexityAssessment> AnalyzeComplexityAsync(string sqlQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Simplifies a complex SQL query for better compatibility
        /// </summary>
        /// <param name="sqlQuery">The complex SQL query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Simplified query or null if cannot be simplified</returns>
        Task<string?> SimplifyQueryAsync(string sqlQuery, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of SQL parser service
    /// </summary>
    public class SqlParser : ISqlParser
    {
        private readonly ILogger<SqlParser> _logger;

        // Supported SQL functions mapping to transformation compatibility
        private static readonly Dictionary<string, bool> SupportedFunctions = new()
        {
            // Aggregate functions
            { "COUNT", true },
            { "SUM", true },
            { "AVG", true },
            { "MIN", true },
            { "MAX", true },
            
            // String functions
            { "UPPER", true },
            { "LOWER", true },
            { "SUBSTRING", true },
            { "LEN", true },
            { "LENGTH", true },
            { "TRIM", true },
            { "LTRIM", true },
            { "RTRIM", true },
            { "CONCAT", true },
            
            // Date functions
            { "YEAR", true },
            { "MONTH", true },
            { "DAY", true },
            { "DATEPART", true },
            { "GETDATE", false }, // Not compatible with JSON transformation
            
            // Math functions
            { "ABS", true },
            { "ROUND", true },
            { "CEILING", true },
            { "FLOOR", true },
            
            // Conditional functions
            { "CASE", true },
            { "ISNULL", true },
            { "COALESCE", true },
            
            // Window functions
            { "ROW_NUMBER", false },
            { "RANK", false },
            { "DENSE_RANK", false },
            { "LEAD", false },
            { "LAG", false }
        };

        public SqlParser(ILogger<SqlParser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SqlAnalysisResult> ParseQueryAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Starting SQL query analysis for query: {Query}", sqlQuery);

            try
            {
                var result = new SqlAnalysisResult
                {
                    RawSql = sqlQuery,
                    IsValid = await ValidateSyntaxAsync(sqlQuery, cancellationToken)
                };

                if (!result.IsValid)
                {
                    _logger.LogWarning("Invalid SQL syntax detected");
                    return result;
                }

                // Parse different components
                result.QueryType = DetermineQueryType(sqlQuery);
                result.SelectFields = await ExtractFieldsAsync(sqlQuery, cancellationToken);
                result.Tables = ExtractTables(sqlQuery);
                result.WhereConditions = ExtractConditions(sqlQuery);
                result.Joins = ExtractJoins(sqlQuery);
                result.Aggregates = ExtractAggregates(sqlQuery);
                result.OrderByFields = ExtractOrderBy(sqlQuery);
                result.GroupByFields = ExtractGroupBy(sqlQuery);
                result.Having = ExtractHaving(sqlQuery);
                var complexity = await AnalyzeComplexityAsync(sqlQuery, cancellationToken);
                result.ComplexityScore = complexity?.OverallComplexity ?? 0.0;

                _logger.LogDebug("SQL analysis completed. Query type: {QueryType}, Fields: {FieldCount}", 
                    result.QueryType, result.Fields.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SQL query analysis");
                throw;
            }
        }

        public async Task<bool> ValidateSyntaxAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            try
            {
                // Basic syntax validation - in production, use actual SQL parser
                var normalizedQuery = sqlQuery.Trim().ToUpperInvariant();
                
                // Check for basic SQL structure
                if (!normalizedQuery.StartsWith("SELECT"))
                {
                    return false;
                }

                // Check for balanced parentheses
                var openParens = sqlQuery.Count(c => c == '(');
                var closeParens = sqlQuery.Count(c => c == ')');
                if (openParens != closeParens)
                {
                    return false;
                }

                // Check for basic FROM clause (unless it's a simple SELECT with constants)
                if (!normalizedQuery.Contains("FROM") && !IsConstantSelect(normalizedQuery))
                {
                    return false;
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating SQL syntax");
                return false;
            }
        }

        public async Task<List<SqlField>> ExtractFieldsAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            var fields = new List<SqlField>();

            try
            {
                // Extract SELECT clause
                var selectClause = ExtractSelectClause(sqlQuery);
                var fieldExpressions = ParseFieldExpressions(selectClause);

                foreach (var expression in fieldExpressions)
                {
                    var field = new SqlField
                    {
                        Name = ExtractFieldName(expression),
                        Expression = new SqlExpression 
                        { 
                            Type = SqlExpressionType.FieldReference,
                            LeftOperand = expression.Trim(),
                            RawExpression = expression.Trim()
                        },
                        Alias = ExtractAlias(expression),
                        IsAggregate = IsAggregateExpression(expression),
                        IsCalculated = IsCalculatedField(expression),
                        DataType = InferSqlDataType(expression)
                    };

                    // Determine table reference
                    field.TableAlias = ExtractTableReference(expression);

                    fields.Add(field);
                }

                return await Task.FromResult(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting fields from SQL query");
                return fields;
            }
        }

        public async Task<List<SqlFunction>> ExtractFunctionsAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            var functions = new List<SqlFunction>();

            try
            {
                // Simple regex pattern to find function calls
                var functionPattern = @"\b([A-Z_]+)\s*\(";
                var matches = System.Text.RegularExpressions.Regex.Matches(sqlQuery.ToUpperInvariant(), functionPattern);

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var functionName = match.Groups[1].Value;
                    
                    if (SupportedFunctions.ContainsKey(functionName))
                    {
                        var function = new SqlFunction
                        {
                            Name = functionName,
                            IsSupported = SupportedFunctions[functionName],
                            Category = GetFunctionCategory(functionName),
                            Parameters = ExtractFunctionParameters(sqlQuery, match.Index)
                        };

                        functions.Add(function);
                    }
                }

                return await Task.FromResult(functions.DistinctBy(f => f.Name).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting functions from SQL query");
                return functions;
            }
        }

        public async Task<ComplexityAssessment> AnalyzeComplexityAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            try
            {
                var assessment = new ComplexityAssessment();

                // Count various complexity factors
                assessment.JoinCount = CountJoins(sqlQuery);
                assessment.AggregationCount = CountAggregations(sqlQuery);
                assessment.ConditionCount = CountConditions(sqlQuery);
                assessment.NestingDepth = CalculateNestingDepth(sqlQuery);

                // Calculate overall complexity score
                var baseScore = Math.Min(sqlQuery.Length / 100.0, 3.0); // Length factor
                var joinScore = assessment.JoinCount * 2.0;
                var aggregateScore = assessment.AggregationCount * 1.5;
                var conditionScore = assessment.ConditionCount * 0.5;
                var nestingScore = assessment.NestingDepth * 1.0;

                assessment.OverallComplexity = baseScore + joinScore + aggregateScore + conditionScore + nestingScore;

                return await Task.FromResult(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing SQL complexity");
                return new ComplexityAssessment();
            }
        }

        public async Task<string?> SimplifyQueryAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            try
            {
                var normalized = sqlQuery.ToUpperInvariant();

                // Remove complex features that can't be easily transformed
                var simplified = sqlQuery;

                // Remove window functions
                simplified = RemoveWindowFunctions(simplified);

                // Simplify complex JOINs
                simplified = SimplifyJoins(simplified);

                // Remove unsupported functions
                simplified = RemoveUnsupportedFunctions(simplified);

                // If significantly changed, return the simplified version
                if (simplified.Length < sqlQuery.Length * 0.8)
                {
                    return await Task.FromResult(simplified);
                }

                return await Task.FromResult<string?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simplifying SQL query");
                return null;
            }
        }

        #region Private Helper Methods

        private SqlQueryType DetermineQueryType(string sqlQuery)
        {
            var normalized = sqlQuery.Trim().ToUpperInvariant();
            
            if (normalized.StartsWith("SELECT")) return SqlQueryType.Select;
            if (normalized.StartsWith("INSERT")) return SqlQueryType.Insert;
            if (normalized.StartsWith("UPDATE")) return SqlQueryType.Update;
            if (normalized.StartsWith("DELETE")) return SqlQueryType.Delete;
            
            return SqlQueryType.Select; // Default to SELECT for unknown queries
        }

        private bool IsConstantSelect(string normalizedQuery)
        {
            // Check if it's a SELECT without FROM (like SELECT 1, SELECT 'Hello')
            return !normalizedQuery.Contains("FROM") && 
                   (normalizedQuery.Contains("SELECT 1") || 
                    normalizedQuery.Contains("SELECT '") ||
                    normalizedQuery.Contains("SELECT \""));
        }

        private string ExtractSelectClause(string sqlQuery)
        {
            var selectIndex = sqlQuery.ToUpperInvariant().IndexOf("SELECT");
            var fromIndex = sqlQuery.ToUpperInvariant().IndexOf("FROM");
            
            if (selectIndex == -1) return "";
            
            var endIndex = fromIndex == -1 ? sqlQuery.Length : fromIndex;
            return sqlQuery.Substring(selectIndex + 6, endIndex - selectIndex - 6).Trim();
        }

        private List<string> ParseFieldExpressions(string selectClause)
        {
            var expressions = new List<string>();
            var current = "";
            var parenDepth = 0;
            
            foreach (char c in selectClause)
            {
                if (c == '(') parenDepth++;
                else if (c == ')') parenDepth--;
                else if (c == ',' && parenDepth == 0)
                {
                    expressions.Add(current.Trim());
                    current = "";
                    continue;
                }
                
                current += c;
            }
            
            if (!string.IsNullOrWhiteSpace(current))
                expressions.Add(current.Trim());
                
            return expressions;
        }

        private string ExtractFieldName(string expression)
        {
            var alias = ExtractAlias(expression);
            if (!string.IsNullOrEmpty(alias))
                return alias;

            // Extract base field name
            var cleaned = expression.Trim();
            
            // Handle function calls
            if (cleaned.Contains("("))
            {
                var funcMatch = System.Text.RegularExpressions.Regex.Match(cleaned, @"([A-Z_]+)\s*\(");
                if (funcMatch.Success)
                    return funcMatch.Groups[1].Value;
            }

            // Handle table.field notation
            if (cleaned.Contains("."))
            {
                var parts = cleaned.Split('.');
                return parts.Last().Trim();
            }

            return cleaned;
        }

        private string? ExtractAlias(string expression)
        {
            var asIndex = expression.ToUpperInvariant().LastIndexOf(" AS ");
            if (asIndex > 0)
            {
                return expression.Substring(asIndex + 4).Trim();
            }

            // Check for implicit alias (space-separated)
            var parts = expression.Trim().Split(' ');
            if (parts.Length > 1 && !parts.Last().ToUpperInvariant().Contains("("))
            {
                return parts.Last().Trim();
            }

            return null;
        }

        private bool IsAggregateExpression(string expression)
        {
            var aggregateFunctions = new[] { "COUNT", "SUM", "AVG", "MIN", "MAX" };
            return aggregateFunctions.Any(func => 
                expression.ToUpperInvariant().Contains($"{func}("));
        }

        private bool IsCalculatedField(string expression)
        {
            return expression.Contains("+") || expression.Contains("-") || 
                   expression.Contains("*") || expression.Contains("/") ||
                   expression.Contains("CASE") || expression.Contains("(");
        }

        private string InferSqlDataType(string expression)
        {
            if (IsAggregateExpression(expression))
            {
                if (expression.ToUpperInvariant().Contains("COUNT"))
                    return "INT";
                if (expression.ToUpperInvariant().Contains("SUM") || 
                    expression.ToUpperInvariant().Contains("AVG"))
                    return "DECIMAL";
            }

            // Default to NVARCHAR for complex expressions
            return "NVARCHAR";
        }

        private string? ExtractTableReference(string expression)
        {
            if (expression.Contains("."))
            {
                var parts = expression.Split('.');
                if (parts.Length >= 2)
                    return parts[0].Trim();
            }
            return null;
        }

        private List<SqlTable> ExtractTables(string sqlQuery)
        {
            var tables = new List<SqlTable>();
            // Simplified table extraction - would use proper SQL parser in production
            var fromMatch = System.Text.RegularExpressions.Regex.Match(
                sqlQuery, @"FROM\s+([A-Za-z_][A-Za-z0-9_]*)",                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (fromMatch.Success)
            {
                tables.Add(new SqlTable { Name = fromMatch.Groups[1].Value });
            }
            
            return tables;
        }

        private List<SqlCondition> ExtractConditions(string sqlQuery)
        {
            // Simplified condition extraction
            return new List<SqlCondition>();
        }

        private List<SqlJoin> ExtractJoins(string sqlQuery)
        {
            // Simplified join extraction
            return new List<SqlJoin>();
        }

        private List<SqlAggregate> ExtractAggregates(string sqlQuery)
        {
            // Simplified aggregate extraction
            return new List<SqlAggregate>();
        }

        private List<SqlOrderBy> ExtractOrderBy(string sqlQuery)
        {
            return new List<SqlOrderBy>();
        }

        private List<SqlField> ExtractGroupBy(string sqlQuery)
        {
            return new List<SqlField>();
        }

        private SqlHaving? ExtractHaving(string sqlQuery)
        {
            return null;
        }

        private string GetFunctionCategory(string functionName)
        {
            return functionName switch
            {
                "COUNT" or "SUM" or "AVG" or "MIN" or "MAX" => "Aggregate",
                "UPPER" or "LOWER" or "SUBSTRING" or "LEN" or "LENGTH" or "TRIM" or "LTRIM" or "RTRIM" or "CONCAT" => "String",
                "YEAR" or "MONTH" or "DAY" or "DATEPART" or "GETDATE" => "Date",
                "ABS" or "ROUND" or "CEILING" or "FLOOR" => "Math",
                "CASE" or "ISNULL" or "COALESCE" => "Conditional",
                _ => "Other"
            };
        }

        private List<string> ExtractFunctionParameters(string sqlQuery, int functionStartIndex)
        {
            // Simplified parameter extraction
            return new List<string>();
        }

        private int CountJoins(string sqlQuery)
        {
            var joinKeywords = new[] { "JOIN", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN" };
            return joinKeywords.Sum(keyword => 
                System.Text.RegularExpressions.Regex.Matches(
                    sqlQuery, keyword, System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count);
        }

        private int CountAggregations(string sqlQuery)
        {
            var aggregateFunctions = new[] { "COUNT", "SUM", "AVG", "MIN", "MAX" };
            return aggregateFunctions.Sum(func => 
                System.Text.RegularExpressions.Regex.Matches(
                    sqlQuery, $@"\b{func}\s*\(", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count);
        }

        private int CountConditions(string sqlQuery)
        {
            var conditionKeywords = new[] { "WHERE", "AND", "OR" };
            return conditionKeywords.Sum(keyword => 
                System.Text.RegularExpressions.Regex.Matches(
                    sqlQuery, $@"\b{keyword}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count);
        }

        private int CalculateNestingDepth(string sqlQuery)
        {
            int maxDepth = 0;
            int currentDepth = 0;
            
            foreach (char c in sqlQuery)
            {
                if (c == '(')
                {
                    currentDepth++;
                    maxDepth = Math.Max(maxDepth, currentDepth);
                }
                else if (c == ')')
                {
                    currentDepth--;
                }
            }
            
            return maxDepth;
        }

        private string RemoveWindowFunctions(string sqlQuery)
        {
            // Remove OVER clauses - simplified implementation
            return System.Text.RegularExpressions.Regex.Replace(
                sqlQuery, @"OVER\s*\([^)]*\)", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private string SimplifyJoins(string sqlQuery)
        {
            // Simplified join simplification
            return sqlQuery;
        }

        private string RemoveUnsupportedFunctions(string sqlQuery)
        {
            var unsupportedFunctions = SupportedFunctions.Where(kvp => !kvp.Value).Select(kvp => kvp.Key);
            
            foreach (var func in unsupportedFunctions)
            {
                sqlQuery = System.Text.RegularExpressions.Regex.Replace(
                    sqlQuery, $@"\b{func}\s*\([^)]*\)", "NULL", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            return sqlQuery;
        }

        #endregion
    }
}
