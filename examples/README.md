# Json.Transform Examples

This project demonstrates the Json.Transform library with multiple execution modes: console examples, interactive web API, HTML demo generation, and test validation.

## 🚀 Quick Start

### 🌐 Interactive Web API (Recommended)
```bash
cd examples
dotnet run -- --api
# Visit http://localhost:5000 for Swagger UI
```

### 🎬 Console Examples
```bash
cd examples
dotnet run                    # Run transformation examples
dotnet run -- --demo        # Generate HTML demo
dotnet run -- --tests       # Run test validation
dotnet run -- --tests --demo # Combined mode
```

### ⚙️ Advanced Options
```bash
# API with custom port
dotnet run -- --api --port 5001

# Demo without opening browser
dotnet run -- --demo --no-browser

# Multiple options combined
dotnet run -- --tests --demo --no-browser
```

## 🌐 Web API Features

The examples project includes a full REST API with Swagger documentation:

### Available Endpoints
- `POST /api/transform` - Transform JSON with custom templates
- `GET /api/examples` - Get predefined example scenarios
- `POST /api/transform/example/{name}` - Run specific examples
- `GET /api/health` - Health check endpoint

### API Usage Examples
```bash
# Health check
curl http://localhost:5000/api/health

# Get available examples
curl http://localhost:5000/api/examples

# Transform custom JSON
curl -X POST "http://localhost:5000/api/transform" \
  -H "Content-Type: application/json" \
  -d '{
    "sourceJson": "{\"user\": {\"name\": \"John\", \"age\": 30}}",
    "templateJson": "{\"mappings\": [{\"from\": \"$.user.name\", \"to\": \"$.customer.fullName\"}]}"
  }'

# Run predefined example
curl -X POST "http://localhost:5000/api/transform/example/Field%20Mapping"
```

## 📊 Example Scenarios

The library includes 6 comprehensive transformation scenarios:

1. **Field Mapping** - Basic field copying and restructuring
2. **Conditional Logic** - Age-based categorization with if/else conditions  
3. **Aggregation** - Sum, average, count operations on arrays
4. **Math Operations** - Arithmetic calculations with mixed operands
5. **String Concatenation** - Template-based string building
6. **Complex Transformation** - Multi-step nested transformations

## ✨ Features Demonstrated

- ✅ **Field Mapping**: Copy/move fields between JSON structures
- ✅ **Aggregation**: Sum, average, count operations on arrays
- ✅ **Conditional Logic**: If/else conditions with comparison operators
- ✅ **Math Operations**: Arithmetic calculations with dynamic operands
- ✅ **String Concatenation**: Template-based string building
- ✅ **Constants**: Static value injection (timestamps, versions, etc.)
- ✅ **Nested Transformations**: Deep object structure mapping
- ✅ **REST API**: Interactive Swagger UI for testing transformations

## 🎬 HTML Demo Generation

The project generates an interactive HTML demo page with:

- **Live Results**: Real transformation outputs with actual data
- **Performance Metrics**: Execution times and success rates
- **Syntax Highlighting**: Beautiful JSON formatting
- **Interactive Examples**: All 6 transformation scenarios
- **Error Handling**: Demonstrations of validation and error cases

Perfect for presentations, documentation, and showcasing library capabilities.

## 🏗️ Project Structure

```
examples/
├── Program.cs              # Main entry point with multiple modes
├── DemoGenerator.cs        # HTML demo generation
├── Services/
│   └── TransformationService.cs  # API business logic
├── Api/
│   └── Models.cs           # API request/response models
└── README.md              # This file
```
- Documentation and tutorials
- Testing and validation
- Sharing transformation examples

The generated HTML file (`demo.html`) contains formatted JSON examples, transformation templates, and results with syntax highlighting.
