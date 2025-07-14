using Json.Transform.Core;
using Json.Transform.Models;
using System.Text.Json;

namespace Json.Transform.Examples;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== JSON Transform Library Demo ===\n");

        // Example 1: Simple field mapping
        SimpleFieldMappingExample();

        // Example 2: Conditional logic
        ConditionalLogicExample();

        // Example 3: String concatenation
        StringConcatenationExample();

        // Example 4: Aggregation operations
        AggregationExample();

        // Example 5: Mathematical operations
        MathOperationExample();

        // Example 6: Complex transformation
        ComplexTransformationExample();

        Console.WriteLine("\n=== Demo Complete ===");
    }

    static void SimpleFieldMappingExample()
    {
        Console.WriteLine("1. Simple Field Mapping:");
        
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
        
        Console.WriteLine("Source JSON:");
        Console.WriteLine(FormatJson(sourceJson));
        Console.WriteLine("\nTransformed JSON:");
        Console.WriteLine(FormatJson(result));
        Console.WriteLine();
    }

    static void ConditionalLogicExample()
    {
        Console.WriteLine("2. Conditional Logic:");
        
        var sourceJson = """
        {
            "user": {
                "age": 25,
                "isVip": true
            }
        }
        """;

        var templateJson = """
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
                },
                {
                    "from": "$.user.isVip",
                    "to": "$.customer.tier",
                    "conditions": [
                        {
                            "if": "$.user.isVip == true",
                            "then": "Premium",
                            "else": "Standard"
                        }
                    ]
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine("Source JSON:");
        Console.WriteLine(FormatJson(sourceJson));
        Console.WriteLine("\nTransformed JSON:");
        Console.WriteLine(FormatJson(result));
        Console.WriteLine();
    }

    static void StringConcatenationExample()
    {
        Console.WriteLine("3. String Concatenation:");
        
        var sourceJson = """
        {
            "user": {
                "firstName": "John",
                "lastName": "Doe",
                "title": "Mr."
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "to": "$.customer.displayName",
                    "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
                },
                {
                    "to": "$.customer.initials",
                    "concat": "{$.user.firstName}.{$.user.lastName}"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine("Source JSON:");
        Console.WriteLine(FormatJson(sourceJson));
        Console.WriteLine("\nTransformed JSON:");
        Console.WriteLine(FormatJson(result));
        Console.WriteLine();
    }

    static void AggregationExample()
    {
        Console.WriteLine("4. Aggregation Operations:");
        
        var sourceJson = """
        {
            "orders": [
                {"id": 1, "total": 100.50, "items": 3},
                {"id": 2, "total": 75.25, "items": 2},
                {"id": 3, "total": 200.00, "items": 5}
            ]
        }
        """;

        var templateJson = """
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
                },
                {
                    "from": "$.orders[*].total",
                    "to": "$.summary.maxOrder",
                    "aggregate": "max"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine("Source JSON:");
        Console.WriteLine(FormatJson(sourceJson));
        Console.WriteLine("\nTransformed JSON:");
        Console.WriteLine(FormatJson(result));
        Console.WriteLine();
    }

    static void MathOperationExample()
    {
        Console.WriteLine("5. Mathematical Operations:");
        
        var sourceJson = """
        {
            "order": {
                "subtotal": 100.00,
                "taxRate": 0.085,
                "shippingFee": 12.50
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "to": "$.order.tax",
                    "math": {
                        "operation": "multiply",
                        "operands": ["$.order.subtotal", "$.order.taxRate"]
                    }
                },
                {
                    "to": "$.order.total",
                    "math": {
                        "operation": "add",
                        "operands": ["$.order.subtotal", "$.order.tax", "$.order.shippingFee"]
                    }
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine("Source JSON:");
        Console.WriteLine(FormatJson(sourceJson));
        Console.WriteLine("\nTransformed JSON:");
        Console.WriteLine(FormatJson(result));
        Console.WriteLine();
    }

    static void ComplexTransformationExample()
    {
        Console.WriteLine("6. Complex Transformation:");
        
        var sourceJson = """
        {
            "customer": {
                "id": 12345,
                "name": "Jane Smith",
                "email": "jane@example.com",
                "registrationDate": "2023-01-15"
            },
            "orders": [
                {"id": 1, "date": "2023-02-01", "total": 150.00, "status": "completed"},
                {"id": 2, "date": "2023-02-15", "total": 89.99, "status": "completed"},
                {"id": 3, "date": "2023-03-01", "total": 200.00, "status": "pending"}
            ]
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "from": "$.customer.id",
                    "to": "$.profile.customerId"
                },
                {
                    "from": "$.customer.name",
                    "to": "$.profile.fullName"
                },
                {
                    "to": "$.profile.accountStatus",
                    "conditions": [
                        {
                            "if": "$.orders[*].status contains 'completed'",
                            "then": "Active",
                            "else": "Inactive"
                        }
                    ]
                },
                {
                    "from": "$.orders[*].total",
                    "to": "$.analytics.totalSpent",
                    "aggregate": "sum"
                },
                {
                    "from": "$.orders",
                    "to": "$.analytics.orderCount",
                    "aggregate": "count"
                },
                {
                    "from": "$.orders[*].total",
                    "to": "$.analytics.averageOrderValue",
                    "aggregate": "avg"
                },
                {
                    "to": "$.metadata.processedAt",
                    "value": "now"
                },
                {
                    "to": "$.metadata.version",
                    "value": "2.0"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine("Source JSON:");
        Console.WriteLine(FormatJson(sourceJson));
        Console.WriteLine("\nTransformed JSON:");
        Console.WriteLine(FormatJson(result));
        Console.WriteLine();
    }

    static string FormatJson(string json)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        catch
        {
            return json;
        }
    }
}
