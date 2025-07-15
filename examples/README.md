# Json.Transform Examples

This folder contains comprehensive examples demonstrating the Json.Transform library functionality.

## Running Examples

### Basic Examples
```bash
dotnet run --project examples/Json.Transform.Examples.csproj
```

### Generate HTML Demo
```bash
dotnet run --project examples/Json.Transform.Examples.csproj -- --demo
```

### Run Tests First, Then Examples
```bash
dotnet run --project examples/Json.Transform.Examples.csproj -- --tests
```

### Generate Demo Without Opening Browser
```bash
dotnet run --project examples/Json.Transform.Examples.csproj -- --demo --no-browser
```

## Examples Included

1. **Simple Field Mapping**: Basic field-to-field transformation
2. **Conditional Logic**: Age-based status assignment using if/else conditions
3. **String Concatenation**: Combining multiple fields with templates
4. **Aggregation Operations**: Sum, count, and average calculations on arrays
5. **Mathematical Operations**: Arithmetic operations on numeric fields
6. **Complex Transformation**: Multi-feature transformation combining all capabilities

## Features Demonstrated

- ✅ **Field Mapping**: Copy/move fields between JSON structures
- ✅ **Aggregation**: Sum, average, count operations on arrays
- ✅ **Conditional Logic**: If/else conditions with comparison operators
- ✅ **String Concatenation**: Template-based string building
- ✅ **Mathematical Operations**: Arithmetic calculations
- ✅ **Constants**: Static value injection (timestamps, versions, etc.)
- ✅ **Nested Transformations**: Deep object structure mapping

## HTML Demo Generation

The examples project can also generate an interactive HTML demo page that showcases all transformation capabilities with live results. This is useful for:

- Presentations and demonstrations
- Documentation and tutorials
- Testing and validation
- Sharing transformation examples

The generated HTML file (`demo.html`) contains formatted JSON examples, transformation templates, and results with syntax highlighting.
