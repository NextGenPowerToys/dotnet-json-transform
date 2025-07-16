# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.3.0] - 2025-07-16

### 🚀 Added
- **String Operations**: Advanced string comparison operators for conditional logic
  - `contains`: Check if string contains substring (case-sensitive)
  - `startsWith`: Check if string starts with specific prefix
  - `endsWith`: Check if string ends with specific suffix
  - Full integration with conditional logic and aggregation filters
- **Complex String Examples**: Enhanced playground and API with sophisticated string operations
  - Employee email filtering using string comparison operators
  - Badge generation with conditional string concatenation
  - File type counting using `endsWith` filtering
  - Dynamic access level assignment based on email patterns
- **Enhanced Template Engine**: String comparison operators in JSONPath expressions
- **Real-world Scenarios**: Employee processing, file classification, domain filtering

### 🧪 Testing Enhancements
- **String Operations Tests**: 10 comprehensive test scenarios covering all string operators
  - Basic string comparison tests for contains, startsWith, endsWith
  - Integration with conditional logic and aggregation
  - Complex boolean expressions with string operators
  - Template concatenation with dynamic values
- **Test Suite Expansion**: Increased from 26 to 36 total passing tests
- **Production Scenarios**: Email validation, file processing, domain filtering

### 📚 Documentation Updates
- **README Updates**: Updated test counts from 26 to 36 passing tests
- **Playground Examples**: Added "String Operations" with complex employee processing
- **Console Examples**: Enhanced string concatenation examples with 3 sub-scenarios
- **API Documentation**: New "String Operations" example in transformation service
- **Demo Generation**: Added complex string operations HTML demo

### 🔧 Technical Implementation
- **StringComparison Methods**: Efficient string comparison implementation
- **JSONPath Integration**: String operators work seamlessly with path expressions
- **Template Processing**: Enhanced concatenation with conditional string operations
- **Performance Optimization**: Fast string comparison operations

### Changed
- **Test Count**: Updated from 26 to 36 passing tests across all documentation
- **Example Count**: Maintained 9 playground scenarios with enhanced string operations
- **Demo Coverage**: Added 7th demo scenario for string operations

## [2.2.0] - 2025-07-16

### 🚀 Added
- **Conditional Aggregation**: Filter array elements before performing aggregation operations
  - New `aggregation` property in mapping templates with `type`, `field`, and `condition` support
  - Support for complex boolean conditions with `&&` and `||` operators in aggregation filters
  - Array element filtering before sum, count, average, min, max operations
  - JSONPath-based condition evaluation for array element selection
- **Enhanced Examples**: Added conditional aggregation examples to both API and playground
  - "Conditional Aggregation - Simple": Basic filtering with amount thresholds
  - "Conditional Aggregation - Complex": Multi-condition filtering with business logic
  - Interactive examples in playground UI with working demonstrations
- **AggregationRule Model**: New model for advanced aggregation configuration
- **ProcessAdvancedAggregation**: Comprehensive aggregation engine with condition support

### 🧪 Testing Enhancements
- **Conditional Aggregation Tests**: 3 new comprehensive test scenarios
  - Simple amount filtering with sum and count operations
  - Complex multi-condition filtering with AND operators
  - Count aggregation with status-based filtering
- **Test Suite Expansion**: Increased from 23 to 26 total passing tests
- **Real-world Scenarios**: Transaction filtering, order processing, inventory management

### 📚 Documentation Updates
- **README Enhancements**: Added conditional aggregation section with examples
- **API Documentation**: Updated with new aggregation examples and endpoints
- **Playground Examples**: Interactive demonstrations of conditional aggregation
- **Feature Coverage**: Complete documentation of all transformation capabilities

### 🔧 Technical Implementation
- **JsonNode Cloning**: Proper handling of parent-child relationships in aggregation
- **Context Creation**: Enhanced condition evaluation for array element processing
- **Error Handling**: Robust validation for aggregation conditions and field extraction
- **Performance Optimization**: Efficient filtering and aggregation operations

### Changed
- **Test Count**: Updated from 23 to 26 passing tests across all documentation
- **Example Count**: Increased playground examples from 7 to 9 scenarios
- **API Examples**: Enhanced transformation service with conditional aggregation scenarios

## [2.1.0] - 2025-07-15

### 🚀 Added
- **Advanced Multi-Condition Support**: Complex conditional expressions with `&&` (AND) and `||` (OR) operators
- **Parentheses Grouping**: Support for complex expressions like `(A || B) && C`
- **Comprehensive Test Suite**: Expanded from 8 to 23 test cases with detailed coverage
- **Performance Benchmark Suite**: 8 comprehensive benchmark scenarios covering all operation types
- **Complex Condition Tests**: Dedicated test class for multi-criteria validation scenarios
- **Integration Tests**: End-to-end transformation workflow validation
- **HTML Test Reports**: Detailed execution analysis with timing and memory usage
- **HTML Benchmark Reports**: Performance metrics with histograms and statistical analysis

### ⚡ Performance
- **Outstanding Math Operations**: 6.35μs mean time (131K operations tested)
- **Excellent Conditional Logic**: 5.63μs mean time (131K operations tested)  
- **Efficient Multi-Conditions**: 39.17μs for high-complexity scenarios (16K operations tested)
- **Optimized Memory Usage**: Reasonable allocation scaling with complexity
- **Sub-microsecond Operations**: For simple conditional logic

### 🧪 Testing Enhancements
- **Employee Eligibility Tests**: Multi-criteria promotion and bonus logic
- **Work Style Categorization**: Location and department-based classification
- **Executive Track Assessment**: Complex nested conditions with OR operators
- **Real-world Scenarios**: Production-ready validation cases
- **Error Handling Tests**: Comprehensive edge case coverage
- **Performance Validation**: Benchmark tests for all operation types

### 📚 Documentation
- **Updated README**: Comprehensive feature overview with performance metrics
- **Benchmark Results**: Detailed performance analysis with comparisons
- **Test Documentation**: Coverage explanation and example scenarios
- **Version History**: Changelog tracking major releases
- **Contributing Guidelines**: Development setup and testing requirements

### 🔧 Developer Experience
- **Enhanced CLI**: Multiple execution modes (`--tests`, `--demo`, `--api`)
- **Improved Error Messages**: More descriptive validation feedback
- **Better Examples**: Real-world transformation scenarios
- **Performance Guidelines**: Optimization recommendations and targets

### Changed
- **Package Version**: Updated to 2.1.0 (production ready)
- **Package Description**: Enhanced with performance metrics and feature highlights
- **Project Status**: Moved from alpha to production-ready status

## [Unreleased - Previous]

### Changed
- 🏢 **Repository Ownership**: Transferred repository ownership to NextGenPowerToys organization
  - Updated all GitHub repository URLs and references
  - Updated package author and company information
  - Updated copyright notices in LICENSE file

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
