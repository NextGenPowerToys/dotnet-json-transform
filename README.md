# Json.Transform

A powerful .NET JSON transformation engine that maps source JSON data to target JSON structures using configurable transformation templates.

## 🚀 Quick Start

### Multiple Ways to Experience Json.Transform

#### 1. � **Interactive Playground** (Recommended)
```bash
cd examples
dotnet run -- --api --port 5260
# Visit http://localhost:5260/playground for the interactive editor
```

#### 2. 🌐 **REST API & Swagger**
```bash
cd examples
dotnet run -- --api
# Visit http://localhost:5000 for Swagger UI and API documentation
```

#### 3. 🎬 **Live Console Demo**
```bash
cd examples
dotnet run -- --demo    # Generate and open HTML demo
dotnet run              # Run console examples
```

#### 4. 🧪 **Run Tests & Benchmarks**
```bash
dotnet test                                    # Run all tests
dotnet run --project benchmarks --configuration Release  # Performance benchmarks
cd examples && dotnet run -- --tests          # Quick test validation
```

## ✨ Features

🔄 **Field Mapping**: Copy/move fields between JSON structures  
📊 **Aggregation**: Sum, average, min, max operations on arrays  
🎯 **Conditional Logic**: If/else conditions with complex expressions  
🧮 **Math Operations**: Arithmetic operations on numeric fields  
🔗 **String Concatenation**: Combine multiple fields with templates  
📝 **Constants**: Inject static values (timestamps, GUIDs, etc.)  
🏗️ **Nested Transformations**: Deep object structure mapping  
🎮 **Interactive Playground**: Live web-based transformation editor  
🌐 **REST API**: Complete Swagger UI for testing transformations  
⚡ **High Performance**: Built on System.Text.Json for speed  

## 📦 Installation

```bash
dotnet add package Json.Transform
```

### Dependencies
- **JsonPath.Net** (1.0.5) - JSONPath expression evaluation  
- **System.Text.Json** - High-performance JSON processing (built-in)

## API Reference

### 🌐 Interactive Web API

Start the REST API server with Swagger UI:

```bash
cd examples
dotnet run -- --api --port 5001
# Visit http://localhost:5001 for interactive API documentation
```

**Available Endpoints:**
- `POST /api/transform` - Transform JSON with custom templates
- `GET /api/examples` - Get predefined example scenarios  
- `POST /api/transform/example/{name}` - Run specific example
- `GET /api/health` - Health check

**Example API Usage:**
```bash
curl -X POST "http://localhost:5001/api/transform" \
  -H "Content-Type: application/json" \
  -d '{
    "sourceJson": "{\"user\": {\"name\": \"John\", \"age\": 30}}",
    "templateJson": "{\"mappings\": [{\"from\": \"$.user.name\", \"to\": \"$.customer.fullName\"}]}"
  }'
```

### 💻 Programmatic Usage

```csharp
using Json.Transform.Core;

var sourceJson = """
{
    "user": {
        "name": "John Doe",
        "age": 25,
        "email": "john@example.com"
    }
}
""";

var templateJson = """
{
    "mappings": [
        {
            "from": "$.user.name",
            "to": "$.customer.fullName"
        },
        {
            "from": "$.user.email",
            "to": "$.customer.contactInfo.email"
        }
    ]
}
""";

var transformer = new JsonTransformer();
var result = transformer.Transform(sourceJson, templateJson);

Console.WriteLine(result);
// Output:
// {
//   "customer": {
//     "fullName": "John Doe",
//     "contactInfo": {
//       "email": "john@example.com"
//     }
//   }
// }
```

## Transformation Features

### 1. Field Mapping
Copy values from source paths to destination paths:

```json
{
    "mappings": [
        {
            "from": "$.user.name",
            "to": "$.customer.fullName"
        }
    ]
}
```

### 2. Constant Values
Set static values including special constants:

```json
{
    "mappings": [
        {
            "to": "$.metadata.timestamp",
            "value": "now"
        },
        {
            "to": "$.metadata.version",
            "value": "1.0"
        }
    ]
}
```

Available constants: `now`, `utcnow`, `guid`, `newguid`, `timestamp`, `true`, `false`, `null`

### 3. Conditional Logic
Apply if/else logic with complex expressions:

```json
{
    "mappings": [
        {
            "from": "$.user.age",
            "to": "$.customer.category",
            "conditions": [
                {
                    "if": "$.user.age >= 18",
                    "then": "Adult",
                    "else": "Minor"
                }
            ]
        }
    ]
}
```

Supported operators: `>=`, `<=`, `==`, `!=`, `>`, `<`, `contains`, `startsWith`, `endsWith`

### 4. String Concatenation
Combine multiple fields using templates:

```json
{
    "mappings": [
        {
            "to": "$.customer.displayName",
            "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
        }
    ]
}
```

### 5. Aggregation Operations
Perform calculations on arrays:

```json
{
    "mappings": [
        {
            "from": "$.orders[*].total",
            "to": "$.summary.totalAmount",
            "aggregate": "sum"
        },
        {
            "from": "$.orders",
            "to": "$.summary.orderCount",
            "aggregate": "count"
        }
    ]
}
```

Available operations: `sum`, `avg`, `min`, `max`, `count`, `first`, `last`, `join`

### 6. Mathematical Operations
Perform arithmetic calculations:

```json
{
    "mappings": [
        {
            "to": "$.order.total",
            "math": {
                "operation": "add",
                "operands": ["$.order.subtotal", "$.order.tax"]
            }
        }
    ]
}
```

Available operations: `add`, `subtract`, `multiply`, `divide`, `power`, `sqrt`, `abs`, `round`, `ceil`, `floor`, `mod`

## Advanced Usage

### Async Transformations

```csharp
var result = await transformer.TransformAsync(sourceJson, templateJson);
```

### Custom Settings

```csharp
var settings = new TransformSettings
{
    StrictMode = true,
    PreserveNulls = false,
    CreatePaths = true,
    EnableTracing = true
};

var transformer = new JsonTransformer(settings);
```

### Template Validation

```csharp
var errors = transformer.ValidateTemplate(templateJson);
if (errors.Any())
{
    foreach (var error in errors)
        Console.WriteLine(error);
}
```

### Strongly-Typed API

```csharp
var sourceData = JsonNode.Parse(sourceJson);
var template = JsonTransformer.ParseTemplate(templateJson);
var result = transformer.Transform(sourceData, template);
```

## Error Handling

The library provides comprehensive error handling with custom exception types:

- `TransformException`: Base exception for all transformation errors
- `PathNotFoundException`: When JSONPath expressions cannot be resolved
- `InvalidConditionException`: When conditional expressions are invalid
- `MathOperationException`: When mathematical operations fail
- `AggregationException`: When aggregation operations fail

## 📊 Performance Benchmarks

**Latest benchmark results (macOS M1 Pro, .NET 9.0):**

| Operation | Mean Time | Relative Performance | Memory Usage |
|-----------|-----------|---------------------|--------------|
| Math Operations | 4.4 μs | Fastest (baseline) | 8.04 KB |
| Conditional Logic | 4.6 μs | 1.05x | 8.42 KB |
| Simple Field Mapping | 62.7 μs | 14.3x | 8.76 KB |
| Large Data Aggregation | 119.2 μs | 27.1x | 255.9 KB |
| String Concatenation | 439.4 μs | 100x | 17.76 KB |
| Complex Transformation | 485.6 μs | 110x | 25.75 KB |

**Run benchmarks yourself:**
```bash
dotnet run --project benchmarks --configuration Release
# View HTML report at: BenchmarkDotNet.Artifacts/results/
```

**Performance Targets:**
- **Throughput**: 1000+ transformations/sec for medium JSON (~5KB)
- **Memory**: < 50MB for 1000 transformations  
- **Latency**: < 10ms per transformation (P95)

## 🎯 Examples & Demo

### Interactive Examples

The `examples/` project provides multiple ways to explore the library:

```bash
cd examples

# 🌐 Web API with Swagger UI (Interactive)
dotnet run -- --api                    # http://localhost:5000

# 🎬 Generate HTML Demo
dotnet run -- --demo                   # Creates demo.html

# 🧪 Run Tests + Examples  
dotnet run -- --tests                  # Validate & run examples

# 📝 Console Examples Only
dotnet run                              # Display transformation examples
```

### Example Scenarios

The library includes 6 comprehensive transformation scenarios:

1. **Field Mapping** - Basic field copying and restructuring
2. **Conditional Logic** - Age-based categorization with if/else
3. **Aggregation** - Sum, average, count operations on arrays
4. **Math Operations** - Arithmetic calculations with mixed operands
5. **String Concatenation** - Template-based string building
6. **Complex Transformation** - Multi-step nested transformations

## 🎮 Interactive Playground

The **JSON Transform Playground** provides a modern, web-based interface for real-time JSON transformation testing and experimentation.

### 🚀 Getting Started

```bash
cd examples
dotnet run -- --api --port 5260
# Open browser to: http://localhost:5260/playground
```

### ✨ Playground Features

- **🎨 Modern UI**: Clean, professional interface with blue-gray color scheme
- **📝 Dual Editors**: Side-by-side JSON source and transform template editors
- **⚡ Real-time Transformation**: Instant results as you type
- **📚 Example Library**: Pre-loaded examples for common transformation patterns
- **🔧 Interactive Controls**: Format JSON, clear editors, copy results
- **📱 Responsive Design**: Works seamlessly on desktop, tablet, and mobile
- **🎯 Syntax Highlighting**: JSON syntax highlighting for better readability
- **⚠️ Error Handling**: Clear error messages and validation feedback

### 🎭 Example Scenarios Available

1. **Simple Field Mapping**: Basic property copying and renaming
2. **Conditional Logic**: Age-based status assignment with if/else conditions
3. **String Concatenation**: Combining multiple fields with templates
4. **Aggregation Operations**: Sum, count, and average calculations
5. **Mathematical Operations**: Arithmetic on numeric fields
6. **Complex Transformations**: Multi-level mapping with all features combined

### 🔄 How to Use the Playground

1. **Load an Example**: Click any example button to load sample data
2. **Edit Source JSON**: Modify the left editor with your input data
3. **Edit Transform Template**: Update the right editor with transformation rules
4. **Transform**: Click "🚀 Transform" to see results instantly
5. **Copy Results**: Use the copy button to get the transformed JSON

### 🎨 Playground Interface

- **Header**: Compact title and description
- **Control Bar**: Example buttons, format, clear, and transform controls
- **Editor Panels**: Source JSON (left) and Transform Template (right)
- **Output Panel**: Transformation results with success/error indicators
- **Status Indicators**: Visual feedback for transformation state

### 📖 Transform Template Format

The playground uses the standard Json.Transform template format:

```json
{
  "mappings": [
    {
      "from": "$.source.path",
      "to": "$.target.path"
    },
    {
      "to": "$.target.computed",
      "value": "constant value"
    },
    {
      "from": "$.array[*].value",
      "to": "$.summary.total",
      "aggregate": "sum"
    }
  ]
}
```

## 🛠️ Building and Testing

```bash
# Clone and setup
git clone https://github.com/yourusername/Json.Transform.git
cd Json.Transform
dotnet restore

# Build the solution
dotnet build

# Run all tests (8 unit tests)
dotnet test

# Run performance benchmarks
dotnet run --project benchmarks --configuration Release

# Try the interactive examples
cd examples
dotnet run -- --api    # Web API + Swagger UI
dotnet run -- --demo   # HTML demo generation
dotnet run              # Console examples
```

## 🏗️ Project Structure

```
Json.Transform/
├── src/Json.Transform/           # Core library
│   ├── Core/                     # Transformation engine
│   ├── Models/                   # Data models
│   ├── Extensions/               # Helper extensions
│   └── Exceptions/               # Custom exceptions
├── tests/Json.Transform.Tests/   # Unit tests (8 tests)
├── benchmarks/                   # Performance benchmarks  
├── examples/                     # Interactive examples + API
├── BenchmarkDotNet.Artifacts/    # Generated benchmark reports
└── Documentation files
```

## 📦 Dependencies

- **.NET 9.0**: Latest .NET runtime for optimal performance
- **JsonPath.Net** (1.0.5): JSONPath expression evaluation
- **System.Text.Json**: Built-in high-performance JSON handling

**Development Dependencies:**
- **xUnit** (2.6.3): Unit testing framework
- **FluentAssertions** (6.12.0): Readable test assertions  
- **BenchmarkDotNet** (0.13.12): Performance benchmarking
- **Swashbuckle.AspNetCore** (7.0.0): Swagger/OpenAPI support

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Roadmap

- [ ] Custom transformation functions
- [ ] Schema validation support
- [ ] Visual transformation designer
- [ ] Template composition and inheritance
- [ ] Performance monitoring and metrics
- [ ] Language bindings (Python, JavaScript)

---

**Built with ❤️ for the .NET community**
