using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Json.Transform.Core;

namespace Json.Transform.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<TransformationBenchmarks>();
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class TransformationBenchmarks
{
    private JsonTransformer _transformer = null!;
    private string _sourceJson = null!;
    private string _simpleTemplate = null!;
    private string _complexTemplate = null!;
    private string _largeSourceJson = null!;

    [GlobalSetup]
    public void Setup()
    {
        _transformer = new JsonTransformer();

        // Simple transformation data
        _sourceJson = """
        {
            "user": {
                "name": "John Doe",
                "age": 25,
                "email": "john@example.com"
            }
        }
        """;

        _simpleTemplate = """
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

        // Complex transformation data
        _complexTemplate = """
        {
            "mappings": [
                {
                    "from": "$.user.name",
                    "to": "$.customer.fullName"
                },
                {
                    "from": "$.user.age",
                    "to": "$.customer.category",
                    "conditions": [
                        {
                            "if": "$.user.age >= 65",
                            "then": "Senior",
                            "else": {
                                "if": "$.user.age >= 18",
                                "then": "Adult",
                                "else": "Minor"
                            }
                        }
                    ]
                },
                {
                    "to": "$.customer.displayName",
                    "concat": "Customer: {$.user.name}"
                },
                {
                    "to": "$.metadata.timestamp",
                    "value": "now"
                }
            ]
        }
        """;

        // Large dataset for performance testing
        var orders = string.Join(",", Enumerable.Range(1, 100).Select(i => 
            $@"{{""id"": {i}, ""total"": {100 + i * 10}, ""status"": ""completed""}}"));

        _largeSourceJson = $$"""
        {
            "user": {
                "name": "John Doe",
                "age": 25,
                "email": "john@example.com"
            },
            "orders": [{{orders}}]
        }
        """;
    }

    [Benchmark(Baseline = true)]
    public string SimpleFieldMapping()
    {
        return _transformer.Transform(_sourceJson, _simpleTemplate);
    }

    [Benchmark]
    public string ComplexTransformation()
    {
        return _transformer.Transform(_sourceJson, _complexTemplate);
    }

    [Benchmark]
    public string LargeDataAggregation()
    {
        var aggregationTemplate = """
        {
            "mappings": [
                {
                    "from": "$.orders[*].total",
                    "to": "$.summary.totalAmount",
                    "aggregate": "sum"
                },
                {
                    "from": "$.orders[*].total",
                    "to": "$.summary.averageOrder",
                    "aggregate": "avg"
                },
                {
                    "from": "$.orders",
                    "to": "$.summary.orderCount",
                    "aggregate": "count"
                }
            ]
        }
        """;

        return _transformer.Transform(_largeSourceJson, aggregationTemplate);
    }

    [Benchmark]
    public string MathOperations()
    {
        var mathTemplate = """
        {
            "mappings": [
                {
                    "to": "$.calculated.ageSquared",
                    "math": {
                        "operation": "power",
                        "operands": ["$.user.age", 2]
                    }
                },
                {
                    "to": "$.calculated.agePlusConstant",
                    "math": {
                        "operation": "add",
                        "operands": ["$.user.age", 10]
                    }
                }
            ]
        }
        """;

        return _transformer.Transform(_sourceJson, mathTemplate);
    }

    [Benchmark]
    public string StringConcatenation()
    {
        var concatTemplate = """
        {
            "mappings": [
                {
                    "to": "$.customer.greeting",
                    "concat": "Hello {$.user.name}, you are {$.user.age} years old!"
                }
            ]
        }
        """;

        return _transformer.Transform(_sourceJson, concatTemplate);
    }

    [Benchmark]
    public string ConditionalLogic()
    {
        var conditionalTemplate = """
        {
            "mappings": [
                {
                    "from": "$.user.age",
                    "to": "$.customer.category",
                    "conditions": [
                        {
                            "if": "$.user.age >= 65",
                            "then": "Senior",
                            "else": {
                                "if": "$.user.age >= 18",
                                "then": "Adult",
                                "else": "Minor"
                            }
                        }
                    ]
                }
            ]
        }
        """;

        return _transformer.Transform(_sourceJson, conditionalTemplate);
    }
}
