# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- 🎮 **Interactive Playground**: Modern web-based playground for testing transformations
  - Dual-pane editor with syntax highlighting for JSON and templates
  - Real-time transformation execution with result preview
  - Built-in example library with one-click loading
  - Responsive design with professional blue-gray color theme
  - Glass morphism UI with smooth animations and custom scrollbars
- 🌐 **Enhanced Web Experience**: Integrated playground with API server
  - `/playground` endpoint serving the interactive interface
  - Static file serving for playground assets
  - Improved startup messaging with playground URL

### Changed
- **Documentation**: Comprehensive updates across all documentation files
  - Updated README.md to feature the playground prominently
  - Enhanced examples documentation with playground integration
  - Refreshed project status and contribution guidelines
- **Web API**: Enhanced startup experience with playground integration
- **UI/UX**: Modern, minimal design with high-contrast color scheme

## [1.1.0] - 2025-07-15

### Added
- 🌐 **Interactive Web API**: RESTful API with Swagger UI for testing transformations
- 🎯 **API Endpoints**: 
  - `POST /api/transform` - Transform JSON with custom templates
  - `GET /api/examples` - Get predefined example scenarios
  - `POST /api/transform/example/{name}` - Run specific examples
  - `GET /api/health` - Health check endpoint
- 🎬 **Enhanced Examples Project**: Consolidated demo/example structure with multiple execution modes
- 📊 **Performance Benchmarks**: Complete benchmark suite with HTML reporting
- 🧪 **Integrated Testing**: Built-in test runner with validation
- 🎨 **Multiple Execution Modes**:
  - Console examples (`dotnet run`)
  - Web API server (`dotnet run -- --api`)
  - HTML demo generation (`dotnet run -- --demo`)
  - Test validation (`dotnet run -- --tests`)
- 📈 **Benchmark Results**: Detailed performance metrics with memory usage analysis
- 🔧 **API Testing Scripts**: Automated curl-based API validation

### Changed
- **Project Structure**: Consolidated examples into single project with API capabilities
- **Documentation**: Updated README with interactive examples and API usage
- **Demo Generation**: Moved from separate Demo folder to integrated examples project
- **Performance**: Enhanced with detailed benchmarking and optimization

### Performance Results
- Math Operations: 4.4 μs (fastest)
- Conditional Logic: 4.6 μs  
- Simple Field Mapping: 62.7 μs
- Large Data Aggregation: 119.2 μs
- String Concatenation: 439.4 μs
- Complex Transformation: 485.6 μs

### Technical Improvements
- **Web SDK Integration**: Examples project now supports both console and web applications
- **Swagger Integration**: Full OpenAPI documentation with interactive testing
- **CORS Support**: Configured for development and testing scenarios
- **Error Handling**: Enhanced API error responses with detailed context
- **HTML Generation**: Improved demo output with performance metrics

## [1.0.0] - 2025-07-14

### Added
- 🔄 **Field Mapping**: Copy/move fields between JSON structures using JSONPath
- 📊 **Aggregation**: Sum, average, min, max, count operations on arrays
- 🎯 **Conditional Logic**: If/else conditions with complex expressions and nested logic
- 🧮 **Math Operations**: Arithmetic operations (add, subtract, multiply, divide, power, sqrt, abs, round)
- 🔗 **String Concatenation**: Combine multiple fields using template strings
- 📝 **Constants**: Inject static values (timestamps, GUIDs, booleans, null values)
- 🏗️ **Nested Transformations**: Deep object structure mapping with recursive templates
- ⚡ **High Performance**: Optimized for speed and memory efficiency
- 🛡️ **Error Handling**: Comprehensive exception handling with detailed error messages
- 🧪 **Extensible**: Plugin-ready architecture for custom transformations
- 📖 **Well Documented**: Complete XML documentation and examples

### Core Components
- **JsonTransformer**: Main API entry point with string-based and strongly-typed interfaces
- **TransformEngine**: Core transformation processing engine
- **PathResolver**: JSONPath expression evaluation and path manipulation
- **ConditionEvaluator**: Conditional logic processing with support for complex expressions
- **AggregationProcessor**: Array data aggregation with multiple operation types
- **MathProcessor**: Mathematical operations with dynamic operand resolution
- **JsonExtensions**: Utility extensions for JSON manipulation

### Supported JSONPath Features
- Property access: `$.user.name`
- Array indexing: `$.orders[0]`
- Wildcard selection: `$.orders[*].total`
- Deep scanning: `$..price`
- Array slicing: `$.items[1:3]`
- Filter expressions: `$.orders[?(@.status == 'completed')]`

### Supported Operators
- **Comparison**: `>=`, `<=`, `==`, `!=`, `>`, `<`
- **String**: `contains`, `startsWith`, `endsWith`
- **Logical**: `&&`, `||`, `!`

### Aggregation Operations
- **Numeric**: `sum`, `avg`, `min`, `max`
- **Collection**: `count`, `first`, `last`
- **String**: `join`

### Mathematical Functions
- **Basic**: `add`, `subtract`, `multiply`, `divide`
- **Advanced**: `power`, `sqrt`, `abs`, `round`

### Constant Values
- **Timestamps**: `now`, `utcnow`, `timestamp`
- **Identifiers**: `guid`, `newguid`
- **Literals**: `true`, `false`, `null`
- **Custom**: Any JSON value

### Dependencies
- **.NET 9.0**: Target framework
- **JsonPath.Net 1.0.5**: JSONPath expression evaluation
- **System.Text.Json**: Built-in JSON handling (no external JSON dependencies)

### Testing
- **xUnit**: Unit testing framework
- **FluentAssertions**: Readable test assertions
- **95%+ Code Coverage**: Comprehensive test suite
- **Performance Tests**: Benchmarking with BenchmarkDotNet

### Examples Included
- Simple field mapping
- Conditional transformations
- String concatenation
- Array aggregation
- Mathematical operations
- Complex multi-step transformations

### Performance Characteristics
- **Throughput**: 1000+ transformations/second for medium JSON (~5KB)
- **Memory**: Efficient memory usage with minimal allocations
- **Latency**: Sub-millisecond processing for simple transformations

## [0.1.0-alpha] - Development

### Added
- Initial project structure
- Core model classes
- Basic transformation engine
- JSONPath integration
- Unit test framework

---

## Legend

- **Added**: New features
- **Changed**: Changes in existing functionality  
- **Deprecated**: Soon-to-be removed features
- **Removed**: Removed features
- **Fixed**: Bug fixes
- **Security**: Vulnerability fixes
