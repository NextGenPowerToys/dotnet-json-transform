# NextGenPowerToys.JSQL

A powerful SQL-to-JSON transformation analyzer that takes JSON examples and SQL queries to generate NextGenPowerToys.Json.Transform templates. This library analyzes compatibility, validates field mappings, and provides detailed error reporting when transformations are not possible.

## Features

- **SQL Query Analysis**: Parse and analyze SQL SELECT statements with support for:
  - Field selections with aliases
  - WHERE conditions with multiple operators
  - Aggregation functions (COUNT, SUM, AVG, MIN, MAX)
  - ORDER BY clauses
  - GROUP BY operations
  - Function calls and calculated fields

- **JSON Schema Analysis**: Automatically discover and analyze JSON structures:
  - Field discovery with type inference
  - Nested object and array support
  - Data type compatibility assessment
  - Array aggregation capability analysis

- **Compatibility Validation**: Comprehensive validation between SQL and JSON:
  - Field mapping validation
  - Data type compatibility checking
  - SQL function support validation
  - Performance impact assessment

- **Template Generation**: Generate Json.Transform templates when compatible:
  - Field selection transformations
  - Conditional filtering
  - Aggregation operations
  - Ordering and sorting
  - Optimization recommendations

- **Error Reporting**: Detailed error analysis with suggestions:
  - Field not found errors
  - Type mismatch detection
  - Unsupported operation identification
  - Alternative solution suggestions

## Installation

```bash
dotnet add package NextGenPowerToys.JSQL
```

## Quick Start

### Basic Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using NextGenPowerToys.JSQL;
using NextGenPowerToys.JSQL.Core;
using System.Text.Json.Nodes;

// Setup dependency injection
var services = new ServiceCollection();
services.AddNextGenPowerToysJSQL();
var serviceProvider = services.BuildServiceProvider();

// Get the analyzer
var analyzer = serviceProvider.GetRequiredService<ITransformationAnalyzer>();

// Example JSON data
var jsonExample = JsonNode.Parse("""
{
  "users": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "john@example.com",
      "age": 30,
      "active": true
    }
  ]
}
""");

// SQL query
var sqlQuery = "SELECT name, email FROM users WHERE active = true";

// Analyze transformation
var result = await analyzer.AnalyzeTransformationAsync(jsonExample, sqlQuery);

if (result.IsSuccess)
{
    Console.WriteLine("Template generated successfully!");
    Console.WriteLine(result.Template?.ToJsonString());
}
else
{
    Console.WriteLine("Transformation not possible:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error.Message}");
    }
}
```

### Advanced Configuration

```csharp
services.AddNextGenPowerToysJSQL(options =>
{
    options.MaxComplexity = 5.0;
    options.DetailedErrors = true;
    options.EnableOptimizations = true;
    options.AnalysisTimeoutMs = 30000;
    options.FunctionCompatibilityOverrides["CUSTOM_FUNC"] = true;
});
```

### Batch Analysis

```csharp
var batchResults = await analyzer.BatchAnalyzeAsync(new[]
{
    (jsonExample1, sqlQuery1),
    (jsonExample2, sqlQuery2),
    (jsonExample3, sqlQuery3)
});

foreach (var result in batchResults)
{
    Console.WriteLine($"Success: {result.IsSuccess}");
    Console.WriteLine($"Compatibility Score: {result.Compatibility?.Score:F2}");
}
```

## Supported SQL Features

### ✅ Supported
- **SELECT statements** with field lists
- **WHERE clauses** with comparison operators (=, !=, <, >, <=, >=, LIKE, IN)
- **Aggregate functions**: COUNT, SUM, AVG, MIN, MAX
- **String functions**: UPPER, LOWER, SUBSTRING, LEN, TRIM, CONCAT
- **Math functions**: ABS, ROUND, CEILING, FLOOR
- **Conditional functions**: CASE, ISNULL, COALESCE
- **ORDER BY** with ASC/DESC
- **GROUP BY** operations
- **Field aliases** with AS keyword
- **Calculated fields** with arithmetic operations

### ❌ Not Supported
- **JOIN operations** (use nested JSON instead)
- **Window functions** (ROW_NUMBER, RANK, LEAD, LAG)
- **Subqueries** (complex nested queries)
- **Date functions** requiring current timestamp (GETDATE)
- **INSERT/UPDATE/DELETE** statements
- **Complex stored procedures**

## JSON Requirements

- Root can be object or array
- Field names should match SQL column references
- Array fields required for aggregation operations
- Nested objects supported with dot notation
- Mixed data types handled with type inference

## Error Types and Solutions

### Field Not Found
**Problem**: SQL references a field that doesn't exist in JSON
**Solution**: Add the missing field to JSON or adjust SQL query

### Type Mismatch
**Problem**: SQL expects different data type than JSON provides
**Solution**: Convert data types or modify SQL to handle type differences

### Unsupported Function
**Problem**: SQL uses functions not supported in JSON transformations
**Solution**: Replace with supported alternatives or remove function

### Invalid Aggregation
**Problem**: Aggregation attempted on non-array field
**Solution**: Ensure target field is an array or remove aggregation

## Examples

See the [Examples](Examples/UsageExamples.cs) directory for comprehensive usage examples including:
- Basic field selection and filtering
- Complex aggregations with grouping
- Error handling and alternative suggestions
- Compatibility validation
- Batch processing

## API Reference

### ITransformationAnalyzer

Main interface for SQL-to-JSON transformation analysis.

#### Methods

- `AnalyzeTransformationAsync(JsonNode, string)` - Analyze single transformation
- `BatchAnalyzeAsync(IEnumerable<(JsonNode, string)>)` - Batch analysis
- `ValidateCompatibilityAsync(JsonNode, string)` - Compatibility validation only
- `EstimatePerformanceAsync(JsonNode, string)` - Performance estimation

### AnalysisResult

Result object containing:
- `IsSuccess` - Whether transformation is possible
- `Template` - Generated Json.Transform template
- `Errors` - List of blocking errors
- `Warnings` - List of potential issues
- `Compatibility` - Detailed compatibility report
- `Performance` - Performance estimates
- `Alternatives` - Suggested alternatives

### Configuration Options

- `MaxComplexity` - Maximum allowed SQL complexity (default: 10.0)
- `MaxNestingDepth` - Maximum JSON nesting depth (default: 10)
- `AnalysisTimeoutMs` - Analysis timeout in milliseconds (default: 30000)
- `EnableOptimizations` - Enable template optimizations (default: true)
- `DetailedErrors` - Generate detailed error messages (default: true)

## Performance Considerations

- Analysis complexity scales with SQL query complexity and JSON structure depth
- Batch analysis is more efficient for multiple transformations
- Template optimization can improve runtime performance
- Memory usage scales with JSON document size

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add comprehensive tests
4. Ensure all existing tests pass
5. Submit a pull request

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Changelog

### Version 1.0.0
- Initial release
- Complete SQL parsing and analysis
- JSON schema discovery and validation
- Template generation for Json.Transform
- Comprehensive error reporting
- Performance estimation
- Batch processing support
- Extensive configuration options
