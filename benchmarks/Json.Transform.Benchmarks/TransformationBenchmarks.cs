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

    [Benchmark]
    public string ComplexMultiCondition()
    {
        var complexConditionSource = """
        {
            "employee": {
                "name": "Sarah Johnson",
                "age": 28,
                "department": "Engineering",
                "yearsOfExperience": 5,
                "performanceScore": 8.5,
                "isManager": true,
                "location": "Remote",
                "salary": 75000
            }
        }
        """;

        var complexConditionTemplate = """
        {
            "mappings": [
                {
                    "from": "$.employee.name",
                    "to": "$.result.employeeName"
                },
                {
                    "to": "$.result.eligibleForPromotion",
                    "conditions": [
                        {
                            "if": "$.employee.yearsOfExperience >= 3 && $.employee.performanceScore >= 9.0",
                            "then": "Yes - Meets Experience and Performance Requirements"
                        },
                        {
                            "if": "$.employee.yearsOfExperience >= 3 && $.employee.performanceScore < 9.0",
                            "then": "Conditional - Meets Experience but Performance Below Threshold"
                        },
                        {
                            "if": "$.employee.yearsOfExperience < 3",
                            "then": "No - Insufficient Experience"
                        }
                    ]
                },
                {
                    "to": "$.result.workStyle",
                    "conditions": [
                        {
                            "if": "$.employee.location == 'Remote' && $.employee.department == 'Engineering'",
                            "then": "Remote Technical Worker"
                        },
                        {
                            "if": "$.employee.location == 'Remote'",
                            "then": "Remote Worker"
                        },
                        {
                            "if": "$.employee.department == 'Engineering'",
                            "then": "On-site Technical Team"
                        },
                        {
                            "else": true,
                            "then": "Office Based"
                        }
                    ]
                },
                {
                    "to": "$.result.bonusEligible",
                    "conditions": [
                        {
                            "if": "$.employee.performanceScore >= 8.0 && $.employee.yearsOfExperience >= 2",
                            "then": "Eligible for Performance Bonus"
                        },
                        {
                            "if": "$.employee.performanceScore >= 7.0",
                            "then": "Eligible for Standard Bonus"
                        },
                        {
                            "else": true,
                            "then": "Standard Compensation"
                        }
                    ]
                }
            ]
        }
        """;

        return _transformer.Transform(complexConditionSource, complexConditionTemplate);
    }

    [Benchmark]
    public string HighComplexityMultiCondition()
    {
        var highComplexitySource = """
        {
            "user": {
                "age": 35,
                "isManager": true,
                "department": "Engineering",
                "yearsOfExperience": 8,
                "performanceScore": 9.2,
                "location": "Remote",
                "salary": 125000,
                "certifications": 5,
                "teamSize": 12
            }
        }
        """;

        var highComplexityTemplate = """
        {
            "mappings": [
                {
                    "to": "$.result.executiveTrack",
                    "conditions": [
                        {
                            "if": "$.user.age >= 30 && $.user.isManager == true && ($.user.department == 'Engineering' || $.user.department == 'Product') && $.user.performanceScore >= 9.0 && $.user.salary >= 100000",
                            "then": "Executive Track Candidate"
                        },
                        {
                            "if": "$.user.age >= 25 && $.user.yearsOfExperience >= 5 && $.user.performanceScore >= 8.0 && $.user.certifications >= 3",
                            "then": "Senior Professional Track"
                        },
                        {
                            "if": "$.user.yearsOfExperience >= 3 && $.user.performanceScore >= 7.0",
                            "then": "Professional Track"
                        },
                        {
                            "else": true,
                            "then": "Standard Track"
                        }
                    ]
                },
                {
                    "to": "$.result.compensationTier",
                    "conditions": [
                        {
                            "if": "$.user.salary >= 120000 && $.user.performanceScore >= 9.0 && $.user.isManager == true",
                            "then": "Tier 1 - Executive Compensation"
                        },
                        {
                            "if": "$.user.salary >= 100000 && $.user.performanceScore >= 8.5",
                            "then": "Tier 2 - Senior Compensation"
                        },
                        {
                            "if": "$.user.salary >= 80000 && $.user.performanceScore >= 7.5",
                            "then": "Tier 3 - Professional Compensation"
                        },
                        {
                            "else": true,
                            "then": "Standard Compensation"
                        }
                    ]
                },
                {
                    "to": "$.result.leadershipPotential",
                    "conditions": [
                        {
                            "if": "$.user.isManager == true && $.user.teamSize >= 10 && $.user.performanceScore >= 8.5 && $.user.yearsOfExperience >= 6",
                            "then": "High Leadership Potential"
                        },
                        {
                            "if": "$.user.isManager == true && $.user.teamSize >= 5 && $.user.performanceScore >= 8.0",
                            "then": "Moderate Leadership Potential"
                        },
                        {
                            "if": "$.user.yearsOfExperience >= 5 && $.user.performanceScore >= 8.5",
                            "then": "Individual Contributor Excellence"
                        },
                        {
                            "else": true,
                            "then": "Standard Performance"
                        }
                    ]
                }
            ]
        }
        """;

        return _transformer.Transform(highComplexitySource, highComplexityTemplate);
    }
}
