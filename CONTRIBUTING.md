# Contributing to Json.Transform

We love your input! We want to make contributing to Json.Transform as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features
- Becoming a maintainer

## Development Process

We use GitHub to host code, to track issues and feature requests, as well as accept pull requests.

### Pull Requests

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code lints.
6. Issue that pull request!

### Branch Naming Convention

- `feature/your-feature-name` - For new features
- `bugfix/issue-description` - For bug fixes
- `docs/documentation-update` - For documentation changes
- `refactor/component-name` - For refactoring existing code

### Commit Message Format

```
type(scope): brief description

Detailed explanation if needed

Fixes #issue-number
```

**Types:**
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Changes that do not affect the meaning of the code
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools

## Code Style

We use EditorConfig to maintain consistent coding styles. Make sure your editor supports EditorConfig.

### C# Style Guidelines

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation for all public APIs
- Keep methods focused and small (ideally under 20 lines)
- Use dependency injection where appropriate

### Code Quality

- Maintain minimum 80% code coverage
- All public APIs must have XML documentation
- Use nullable reference types appropriately
- Handle edge cases and provide meaningful error messages

## Testing

### Unit Tests

- Write unit tests for all new functionality
- Use xUnit as the testing framework
- Use FluentAssertions for readable assertions
- Mock dependencies using Moq when necessary

### Test Structure

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = "test data";
    var expected = "expected result";

    // Act
    var result = methodUnderTest(input);

    // Assert
    result.Should().Be(expected);
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Json.Transform.Tests/
```

## Performance

### Benchmarking

We use BenchmarkDotNet for performance testing. Run benchmarks with:

```bash
cd benchmarks/Json.Transform.Benchmarks
dotnet run -c Release
```

### Performance Goals

- **Throughput**: 1000+ transformations/sec for medium JSON (~5KB)
- **Memory**: < 50MB for 1000 transformations
- **Latency**: < 10ms per transformation (P95)

## Documentation

### API Documentation

- All public APIs must have XML documentation
- Include usage examples in documentation
- Document parameter validation and exceptions

### README Updates

- Update feature lists when adding new capabilities
- Include examples for new features
- Update performance benchmarks when relevant

## Reporting Issues

### Bug Reports

Please include:

1. **Clear title and description**
2. **Steps to reproduce** the issue
3. **Expected behavior**
4. **Actual behavior**
5. **Sample JSON data** that demonstrates the issue
6. **Environment details** (.NET version, OS, etc.)

Use this template:

```markdown
## Bug Description
Brief description of the issue.

## Steps to Reproduce
1. Step one
2. Step two
3. Step three

## Expected Behavior
What should happen.

## Actual Behavior
What actually happens.

## Sample Data
```json
{
  "input": "sample data that causes the issue"
}
```

## Environment
- .NET Version: 9.0
- OS: Windows/macOS/Linux
- Json.Transform Version: 1.0.0
```

### Feature Requests

Please include:

1. **Clear use case** for the feature
2. **Proposed API design** (if applicable)
3. **Examples** of how it would be used
4. **Benefits** over existing approaches

## Development Setup

### Prerequisites

- .NET 9.0 SDK or later
- Git
- Visual Studio 2022, VS Code, or JetBrains Rider

### Local Development

```bash
# Clone your fork
git clone https://github.com/your-username/Json.Transform.git
cd Json.Transform

# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run examples
cd src/Json.Transform/Examples
dotnet run
```

### Project Structure

```
Json.Transform/
├── src/
│   └── Json.Transform/           # Main library
├── tests/
│   └── Json.Transform.Tests/     # Unit tests
├── benchmarks/
│   └── Json.Transform.Benchmarks/ # Performance tests
├── docs/                         # Documentation
├── examples/                     # Example projects
└── tools/                        # Build tools
```

## Release Process

### Versioning

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR**: Incompatible API changes
- **MINOR**: Backwards-compatible functionality additions
- **PATCH**: Backwards-compatible bug fixes

### Release Checklist

- [ ] Update version in project file
- [ ] Update CHANGELOG.md
- [ ] Update README.md if needed
- [ ] Ensure all tests pass
- [ ] Run performance benchmarks
- [ ] Update documentation
- [ ] Create release notes

## Community

### Code of Conduct

Be respectful, inclusive, and constructive in all interactions. We're here to build something great together!

### Getting Help

- **GitHub Issues**: For bugs and feature requests
- **Discussions**: For questions and general discussion

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to Json.Transform! 🎉
