using Json.Transform.Core;
using Json.Transform.Examples.Api;
using System.Diagnostics;

namespace Json.Transform.Examples.Services;

/// <summary>
/// Service for handling JSON transformations with performance tracking
/// </summary>
public class TransformationService
{
    private readonly JsonTransformer _transformer;

    public TransformationService()
    {
        _transformer = new JsonTransformer();
    }

    /// <summary>
    /// Performs a JSON transformation with timing and error handling
    /// </summary>
    public async Task<TransformResponse> TransformAsync(TransformRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _transformer.TransformAsync(request.SourceJson, request.TemplateJson);
            
            stopwatch.Stop();
            
            return new TransformResponse
            {
                ResultJson = result,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Success = true
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            return new TransformResponse
            {
                ResultJson = "",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Gets predefined example scenarios for testing
    /// </summary>
    public List<ExampleScenario> GetExampleScenarios()
    {
        return new List<ExampleScenario>
        {
            new ExampleScenario
            {
                Name = "Field Mapping",
                Description = "Basic field copying and restructuring",
                SourceJson = """
                {
                    "user": {
                        "name": "John Doe",
                        "age": 25,
                        "email": "john@example.com"
                    }
                }
                """,
                TemplateJson = """
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
                            "from": "$.user.age",
                            "to": "$.customer.profile.age"
                        }
                    ]
                }
                """
            },
            new ExampleScenario
            {
                Name = "Conditional Logic",
                Description = "Age-based categorization with if/else conditions",
                SourceJson = """
                {
                    "user": {
                        "name": "Alice",
                        "age": 17
                    }
                }
                """,
                TemplateJson = """
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
                                    "if": "$.user.age >= 18",
                                    "then": "Adult",
                                    "else": "Minor"
                                }
                            ]
                        }
                    ]
                }
                """
            },
            new ExampleScenario
            {
                Name = "Aggregation",
                Description = "Sum, average, count operations on arrays",
                SourceJson = """
                {
                    "orders": [
                        {"id": 1, "total": 100.50, "status": "completed"},
                        {"id": 2, "total": 75.25, "status": "pending"},
                        {"id": 3, "total": 200.00, "status": "completed"}
                    ]
                }
                """,
                TemplateJson = """
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
                        },
                        {
                            "from": "$.orders[*].total",
                            "to": "$.summary.averageAmount",
                            "aggregate": "avg"
                        }
                    ]
                }
                """
            },
            new ExampleScenario
            {
                Name = "Math Operations",
                Description = "Arithmetic operations on numeric fields",
                SourceJson = """
                {
                    "order": {
                        "subtotal": 100.00,
                        "tax": 8.50,
                        "discount": 10.00
                    }
                }
                """,
                TemplateJson = """
                {
                    "mappings": [
                        {
                            "to": "$.order.total",
                            "math": {
                                "operation": "add",
                                "operands": ["$.order.subtotal", "$.order.tax"]
                            }
                        },
                        {
                            "to": "$.order.finalTotal",
                            "math": {
                                "operation": "subtract",
                                "operands": ["$.order.total", "$.order.discount"]
                            }
                        }
                    ]
                }
                """
            },
            new ExampleScenario
            {
                Name = "String Concatenation",
                Description = "Template-based string building",
                SourceJson = """
                {
                    "user": {
                        "title": "Mr.",
                        "firstName": "John",
                        "lastName": "Doe",
                        "department": "Engineering"
                    }
                }
                """,
                TemplateJson = """
                {
                    "mappings": [
                        {
                            "to": "$.user.fullName",
                            "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
                        },
                        {
                            "to": "$.user.displayName",
                            "concat": "{$.user.firstName} from {$.user.department}"
                        },
                        {
                            "to": "$.metadata.timestamp",
                            "value": "now"
                        }
                    ]
                }
                """
            }
        };
    }
}
