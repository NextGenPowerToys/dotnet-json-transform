# NextGenPowerToys.Json.Transform

⚠️ **ALPHA VERSION - FOR TESTING PURPOSES ONLY** ⚠️

A powerful .NET JSON transformation engine that maps source JSON data to target JSON structures using configurable transformation templates.

## 🚀 Quick Start

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
```

## ✨ Features

- 🔄 **Field Mapping**: Copy/move fields between JSON structures  
- 📊 **Aggregation**: Sum, average, min, max operations on arrays  
- 🎯 **Conditional Logic**: If/else conditions with complex expressions  
- 🧮 **Math Operations**: Arithmetic operations on numeric fields  
- 🔗 **String Concatenation**: Combine multiple fields with templates  
- 📝 **Constants**: Inject static values (timestamps, GUIDs, etc.)  
- 🏗️ **Nested Transformations**: Deep object structure mapping  
- ⚡ **High Performance**: Built on System.Text.Json for speed  

## 📦 Installation

```bash
dotnet add package NextGenPowerToys.Json.Transform --version 1.0.0-alpha
```

## 🎮 Interactive Playground

Try the live playground at: [https://github.com/NextGenPowerToys/dotnet-json-transform](https://github.com/NextGenPowerToys/dotnet-json-transform)

## ⚠️ Alpha Release Warning

This is an experimental alpha version for testing and feedback purposes. **Not recommended for production use.**

## 📚 Documentation

For complete documentation, examples, and API reference, visit:
[https://github.com/NextGenPowerToys/dotnet-json-transform](https://github.com/NextGenPowerToys/dotnet-json-transform)

## 🛠️ Target Framework

- .NET 9.0

## 📄 License

MIT License - see [LICENSE](https://github.com/NextGenPowerToys/dotnet-json-transform/blob/main/LICENSE) for details.

---

**Built with ❤️ by NextGenPowerToys**
