# JSON Template Transformer Library (.NET) - Developer Guide

> **Contributing to Json.Transform** - A comprehensive guide for developers working on this JSON transformation library

## Project Overview

The Json.Transform library is a powerful .NET JSON transformation engine that maps source JSON data to target JSON structures using configurable transformation templates. This guide provides detailed implementation instructions for contributors and maintainers.

### Key Features
- 🔄 **Field Mapping**: Copy/move fields between JSON structures
- 📊 **Aggregation**: Sum, average, min, max operations on arrays
- 🎯 **Conditional Logic**: If/else conditions with complex expressions
- 🧮 **Math Operations**: Arithmetic operations on numeric fields
- 🔗 **String Concatenation**: Combine multiple fields with templates
- 📝 **Constants**: Inject static values (timestamps, GUIDs, etc.)
- 🏗️ **Nested Transformations**: Deep object structure mapping

## Development Workflow

### Getting Started
1. Fork the repository
2. Clone your fork locally
3. Create a feature branch from `main`
4. Follow the implementation phases below
5. Submit a pull request

### Branch Naming Convention
- `feature/your-feature-name` - For new features
- `bugfix/issue-description` - For bug fixes
- `docs/documentation-update` - For documentation changes

### Commit Message Format
```
type(scope): brief description

Detailed explanation if needed

Fixes #issue-number
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

## System Architecture

### Core Components

1. **JsonTransformer** - Main entry point for transformations
2. **TransformTemplate** - Model representing transformation rules
3. **TransformEngine** - Core transformation logic processor
4. **PathResolver** - JSONPath expression evaluator
5. **ConditionEvaluator** - Conditional logic processor
6. **AggregationProcessor** - Data aggregation handler
7. **MathProcessor** - Mathematical operations handler

### Project Structure

```
src/
├── Json.Transform/
│   ├── Models/
│   │   ├── TransformTemplate.cs
│   │   ├── Mapping.cs
│   │   ├── Condition.cs
│   │   └── AggregationRule.cs
│   ├── Core/
│   │   ├── JsonTransformer.cs
│   │   ├── TransformEngine.cs
│   │   ├── PathResolver.cs
│   │   ├── ConditionEvaluator.cs
│   │   ├── AggregationProcessor.cs
│   │   └── MathProcessor.cs
│   ├── Extensions/
│   │   └── JsonExtensions.cs
│   └── Exceptions/
│       └── TransformException.cs
tests/
├── Json.Transform.Tests/
│   ├── Unit/
│   │   ├── JsonTransformerTests.cs
│   │   ├── PathResolverTests.cs
│   │   ├── ConditionEvaluatorTests.cs
│   │   └── AggregationProcessorTests.cs
│   ├── Integration/
│   │   └── EndToEndTests.cs
│   └── TestData/
│       ├── input-sample.json
│       ├── output-sample.json
│       └── transform-sample.json
```

## Development Phases

> **Note**: Follow these phases sequentially for systematic development. Each phase builds upon the previous one.

### Phase 1: Project Setup & Core Models 🏗️

**Estimated Time**: 1-2 days  
**Priority**: High  
**Dependencies**: None

#### 1.1 Environment Setup

**Prerequisites:**
- .NET 8.0 SDK or later
- Visual Studio 2022 / VS Code / JetBrains Rider
- Git

**Setup Steps:**

1. Clone the repository
2. Restore NuGet packages: `dotnet restore`
3. Build the solution: `dotnet build`
4. Run tests: `dotnet test`

#### 1.1 Project Configuration

**File**: `src/Json.Transform/Json.Transform.csproj`

> ⚠️ **Important**: Update target framework to `net8.0` for latest features and performance improvements

**Required Dependencies:**
- **JsonPath.Net**: For JSONPath expression evaluation
- **System.Text.Json**: Built-in .NET JSON handling (no external dependencies)

**Configuration Requirements:**
- Target Framework: .NET 8.0
- Enable implicit usings and nullable reference types
- Package metadata for NuGet publishing
- XML documentation generation

#### 1.2 Core Model Classes

> 📋 **Task**: Create the foundational data models for transformation rules

**Location**: `src/Json.Transform/Models/`

**TransformTemplate.cs** - Main container for transformation rules:

**Key Requirements:**
- Use `System.Text.Json.Serialization` attributes
- List of mapping rules with JsonPropertyName attributes
- Optional settings configuration
- Support for strict mode and null value handling

**Mapping.cs** - Individual transformation mapping rules:

**Key Components:**
- Source path (`from`) and destination path (`to`)
- Support for constant values, aggregation operations
- Conditional logic with if/then/else structures
- Mathematical operations and string concatenation
- All properties should use JsonPropertyName attributes
- Support for nested operations and complex transformations

**Condition.cs** - Conditional logic model:

**Requirements:**
- Support for if/then/else conditional logic
- Nested conditions with elseif support
- Use JsonPropertyName attributes for serialization
- Handle complex conditional expressions

#### 1.3 Exception Handling

> 🚨 **Best Practice**: Implement comprehensive error handling from the start

**Location**: `src/Json.Transform/Exceptions/`

**TransformException.cs** - Custom exception types:

**Exception Hierarchy:**
- Base `TransformException` class inheriting from `Exception`
- `PathNotFoundException` for JSONPath resolution errors
- `InvalidConditionException` for conditional logic errors
- Include detailed error messages with context
- Support for inner exceptions

### Phase 2: Core Processing Engine 🚀

**Estimated Time**: 3-4 days  
**Priority**: High  
**Dependencies**: Phase 1 complete

> 💡 **Tip**: Implement components in the order listed below for optimal dependency management

#### 2.1 JSONPath Resolution

> 🎯 **Purpose**: Handle dynamic path resolution and value extraction from JSON

**Location**: `src/Json.Transform/Core/`

**PathResolver.cs** - JSONPath processing engine:

**Core Functionality:**
- Use `System.Text.Json.Nodes` instead of Newtonsoft.Json
- Implement JSONPath resolution using JsonPath.Net library
- Support for single value and array resolution
- Path setting capabilities for nested object creation
- Proper error handling with custom exceptions

#### 2.2 Conditional Logic Engine

> 🔍 **Focus**: Support complex conditional expressions like `$.user.age >= 18`

**ConditionEvaluator.cs** - Handles if/else logic processing:

**Key Features:**
- Support for comparison operators (>=, <=, ==, !=, >, <)
- String operations (contains, startsWith, endsWith)
- Integration with PathResolver for dynamic value extraction
- Handle string literals, numbers, and boolean values
- Nested condition evaluation with elseif support

#### 2.3 Data Aggregation

> 📊 **Capability**: Process arrays with sum, avg, min, max, count operations

**AggregationProcessor.cs** - Array data processing:

**Supported Operations:**
- Numeric aggregations: sum, average, min, max
- Collection operations: count, first, last
- String operations: join
- Work with System.Text.Json.Nodes types
- Proper numeric value extraction and type handling

#### 2.4 Mathematical Operations

> 🧮 **Feature**: Support arithmetic operations on numeric fields

**MathProcessor.cs** - Mathematical computations:

**Supported Operations:**
- Basic arithmetic: add, subtract, multiply, divide
- Advanced functions: power, sqrt, abs, round
- Support for both JSONPath references and direct numeric values
- Integration with PathResolver for dynamic value extraction
- Proper numeric type handling with System.Text.Json

### Phase 3: Transformation Engine Integration 🎛️

**Estimated Time**: 2-3 days  
**Priority**: Critical  
**Dependencies**: Phase 2 complete

> ⚙️ **Goal**: Integrate all processing components into a unified transformation engine

#### 3.1 Core Transform Engine

**TransformEngine.cs** - Main processing orchestrator:

**Core Responsibilities:**
- Orchestrate all transformation operations
- Process mappings in sequence
- Handle different mapping types (constant, aggregation, math, concatenation)
- Apply conditional logic when present
- Use System.Text.Json.Nodes for all JSON operations
- Implement proper error handling and logging

#### 3.2 Public API Interface

> 🌐 **Interface**: The main entry point for library consumers

**JsonTransformer.cs** - Public API class:

**Main Features:**
- Primary entry point for transformations
- Support for string-based and strongly-typed APIs
- Async transformation support
- Use System.Text.Json for parsing and serialization
- Comprehensive error handling with meaningful messages

### Phase 4: Utilities & Extensions 🔧

**Estimated Time**: 1 day  
**Priority**: Medium  
**Dependencies**: Phase 3 complete

#### 4.1 Helper Extensions

> 🛠️ **Enhancement**: Useful extension methods for JSON manipulation

**Location**: `src/Json.Transform/Extensions/`

**JsonExtensions.cs** - JSON utility extensions:

**Extension Methods:**
- Helper methods for System.Text.Json.Nodes
- Safe value extraction with default values
- Path checking and validation utilities
- Null and empty value checking
- Type conversion helpers

### Phase 5: Comprehensive Testing 🧪

**Estimated Time**: 2-3 days  
**Priority**: Critical  
**Dependencies**: Phase 4 complete

> ✅ **Quality Gate**: Minimum 80% code coverage required before merging

#### 5.1 Test Project Setup

**Location**: `tests/Json.Transform.Tests/`

**Json.Transform.Tests.csproj** - Test project configuration:

**Required Test Dependencies:**
- Microsoft.NET.Test.Sdk
- xunit and xunit.runner.visualstudio
- FluentAssertions for readable assertions
- Moq for mocking dependencies
- Project reference to main library

#### 5.2 Test Data Samples

> 📋 **Testing**: Standardized test data for consistent validation

**Location**: `tests/Json.Transform.Tests/TestData/`

**input-sample.json** - Sample input data:
```json
{
  "user": {
    "name": "John Doe",
    "age": 25,
    "email": "john@example.com"
  },
  "orders": [
    {"id": 1, "total": 100.50, "status": "completed"},
    {"id": 2, "total": 75.25, "status": "pending"}
  ],
  "preferences": {
    "notifications": true,
    "theme": "dark"
  }
}
```

**output-sample.json** - Expected output template:
```json
{
  "customer": {
    "fullName": "",
    "status": "",
    "contactInfo": {
      "email": ""
    }
  },
  "summary": {
    "totalSpent": 0,
    "orderCount": 0
  },
  "metadata": {
    "timestamp": "",
    "version": "1.0"
  }
}
```

**transform-sample.json** - Transformation rules:
```json
{
  "mappings": [
    {
      "from": "$.user.name",
      "to": "$.customer.fullName"
    },
    {
      "from": "$.user.email",
      "to": "$.customer.contactInfo.email"
    },
    {
      "from": "$.orders[*].total",
      "to": "$.summary.totalSpent",
      "aggregate": "sum"
    },
    {
      "from": "$.orders",
      "to": "$.summary.orderCount",
      "aggregate": "count"
    },
    {
      "from": "$.user.age",
      "to": "$.customer.status",
      "conditions": [
        {
          "if": "$.user.age >= 18",
          "then": "Adult",
          "else": "Minor"
        }
      ]
    },
    {
      "to": "$.metadata.timestamp",
      "value": "now"
    }
  ]
}
```

### Phase 6: Documentation & Examples 📚

**Estimated Time**: 1-2 days  
**Priority**: High  
**Dependencies**: Phase 5 complete

## Code Examples & Usage

### Basic Implementation

**Key Usage Patterns:**
- Simple string-based API for ease of use
- Strongly-typed API for advanced scenarios
- Async support for non-blocking operations
- Comprehensive error handling

### Advanced Transformation Examples

> 🎯 **Real-world Scenarios**: Complex transformation patterns

**Complex transformation template:**

**Advanced Features:**
- String concatenation with templates
- Mathematical operations with mixed operands
- Multi-level aggregation
- Complex conditional logic with multiple conditions

## Project Timeline & Milestones

### Development Schedule
| Phase | Duration | Key Deliverables | Assigned To |
|-------|----------|------------------|-------------|
| 1 | 1-2 days | Core models, project setup | - |
| 2 | 3-4 days | Processing components | - |
| 3 | 2-3 days | Transform engine | - |
| 4 | 1 day | Extensions & utilities | - |
| 5 | 2-3 days | Testing & validation | - |
| 6 | 1-2 days | Documentation | - |

**Total Estimated Time**: 10-15 days

### Release Milestones
- 🎯 **v0.1.0-alpha**: Core transformation functionality
- 🎯 **v0.2.0-beta**: Complete feature set with testing
- 🎯 **v1.0.0**: Production-ready release

## Technical Dependencies

### Required Packages
| Package | Version | Purpose |
|---------|---------|---------|
| **System.Text.Json** | Built-in | JSON parsing and manipulation |
| **JsonPath.Net** | 1.0.5 | JSONPath expression evaluation |

### Development Dependencies
| Package | Version | Purpose |
|---------|---------|---------|
| **xunit** | 2.6.3 | Unit testing framework |
| **FluentAssertions** | 6.12.0 | Assertion library |
| **Moq** | 4.20.69 | Mocking framework |

### Optional Enhancements
- **BenchmarkDotNet**: Performance benchmarking
- **Microsoft.Extensions.Logging**: Structured logging support

## Performance & Quality Guidelines

### Performance Targets
| Metric | Target | Measurement |
|--------|--------|-------------|
| **Throughput** | 1000+ transformations/sec | Medium JSON (~5KB) |
| **Memory** | < 50MB for 1000 transformations | Peak memory usage |
| **Latency** | < 10ms per transformation | P95 latency |

### Optimization Strategies
1. **JSONPath Caching** 🏎️
   - Cache compiled JSONPath expressions
   - Implement LRU cache for frequently used paths

2. **Streaming Support** 📡
   - Process large JSON files without loading entirely into memory
   - Support for IAsyncEnumerable patterns

3. **Parallel Processing** ⚡
   - Process independent mappings concurrently
   - Use Task.WhenAll for async operations

4. **Memory Management** 🧹
   - Efficient JsonNode handling and disposal
   - Minimize object allocations in hot paths
   - Use object pooling for frequently created objects

### Code Quality Standards
- **Code Coverage**: Minimum 80%
- **Cyclomatic Complexity**: Maximum 10 per method
- **Documentation**: XML docs for all public APIs
- **Static Analysis**: Enable all analyzer rules

## Error Handling & Resilience

### Error Handling Strategy
| Error Type | Handling Approach | User Impact |
|------------|-------------------|-------------|
| **Invalid JSON** | Throw `JsonException` with details | Immediate failure |
| **Path Not Found** | Configurable (fail/skip/default) | Graceful degradation |
| **Type Mismatch** | Attempt conversion, fallback to string | Continue processing |
| **Math Errors** | Return null/zero based on operation | Partial results |

### Resilience Patterns
1. **Circuit Breaker** 🔧
   - Stop processing after consecutive failures
   - Configurable failure threshold

2. **Retry Logic** 🔄
   - Retry transient errors (network, I/O)
   - Exponential backoff strategy

3. **Validation Pipeline** ✅
   - Pre-validate JSON schemas
   - Validate transformation templates

4. **Audit Logging** 📝
   - Detailed error context
   - Performance metrics
   - Transformation history

## Roadmap & Future Enhancements

### Version 1.x Features
- [ ] **Custom Functions** 🔌
  - Plugin system for user-defined transformation functions
  - Built-in function library (date formatting, string manipulation)

- [ ] **Schema Validation** 📋
  - JSON Schema validation for inputs and outputs  
  - Template validation with IntelliSense support

- [ ] **Performance Monitoring** 📊
  - Built-in metrics collection
  - Performance dashboards
  - Bottleneck identification

### Version 2.x Features  
- [ ] **Visual Designer** 🎨
  - Drag-and-drop transformation designer
  - Template debugging tools
  - Live preview capabilities

- [ ] **Template Composition** 🏗️
  - Template inheritance and composition
  - Modular transformation components
  - Template marketplace/library

### Community Contributions
- [ ] **Language Bindings** 🌐
  - Python wrapper
  - JavaScript/Node.js binding
  - Go implementation

## Contributing Guidelines

### Code Style
- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use EditorConfig settings provided in the repository
- Run `dotnet format` before committing

### Pull Request Process
1. ✅ Ensure all tests pass
2. ✅ Add tests for new functionality  
3. ✅ Update documentation
4. ✅ Follow semantic versioning
5. ✅ Request review from maintainers

### Issue Reporting
- Use provided issue templates
- Include reproduction steps
- Provide sample JSON files when relevant
- Label issues appropriately

---

This comprehensive guide provides the foundation for implementing a robust, production-ready JSON transformation library with enterprise-grade features and community collaboration support.
