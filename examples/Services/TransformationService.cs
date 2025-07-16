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
                Name = "String Concatenation - Basic",
                Description = "Simple template-based string building",
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
            },
            new ExampleScenario
            {
                Name = "String Operations",
                Description = "Advanced string concatenation and comparison operators (contains, startsWith, endsWith)",
                SourceJson = """
                {
                    "employees": [
                        { "name": "Alice Admin", "email": "alice.admin@company.com", "department": "IT", "files": ["report.pdf", "data.xlsx"] },
                        { "name": "Bob Support", "email": "bob.support@company.com", "department": "Customer Service", "files": ["guide.pdf", "help.docx"] },
                        { "name": "Charlie Dev", "email": "charlie@external.com", "department": "Engineering", "files": ["code.js", "README.md"] }
                    ],
                    "metadata": {
                        "company": "TechCorp",
                        "generated": "2025-07-16"
                    }
                }
                """,
                TemplateJson = """
                {
                    "mappings": [
                        {
                            "from": "$.employees[*]",
                            "to": "processedEmployees",
                            "template": {
                                "mappings": [
                                    {
                                        "from": "$.name",
                                        "to": "name"
                                    },
                                    {
                                        "from": "$.email",
                                        "to": "accessLevel",
                                        "conditions": [
                                            {
                                                "if": "$.email contains 'admin' || $.email startsWith 'alice'",
                                                "then": "Administrator"
                                            },
                                            {
                                                "if": "$.email contains 'support' && $.department == 'Customer Service'",
                                                "then": "Support Agent"
                                            },
                                            {
                                                "if": "$.email endsWith '@company.com'",
                                                "then": "Employee"
                                            },
                                            {
                                                "else": true,
                                                "then": "External"
                                            }
                                        ]
                                    },
                                    {
                                        "to": "badge",
                                        "concat": "{$.name} - {$.accessLevel} ({$.department})"
                                    },
                                    {
                                        "to": "pdfFileCount",
                                        "from": "$.files[*]",
                                        "aggregation": {
                                            "type": "count",
                                            "condition": "$.item endsWith '.pdf'"
                                        }
                                    }
                                ]
                            }
                        },
                        {
                            "to": "summary",
                            "template": {
                                "mappings": [
                                    {
                                        "to": "reportTitle",
                                        "concat": "{$.metadata.company} Employee Report - {$.metadata.generated}"
                                    },
                                    {
                                        "to": "companyEmployeeCount",
                                        "from": "$.employees[*]",
                                        "aggregation": {
                                            "type": "count",
                                            "condition": "$.item.email endsWith '@company.com'"
                                        }
                                    },
                                    {
                                        "to": "adminCount",
                                        "from": "$.employees[*]",
                                        "aggregation": {
                                            "type": "count",
                                            "condition": "$.item.email contains 'admin'"
                                        }
                                    },
                                    {
                                        "to": "externalCount",
                                        "from": "$.employees[*]",
                                        "aggregation": {
                                            "type": "count",
                                            "condition": "!$.item.email endsWith '@company.com'"
                                        }
                                    }
                                ]
                            }
                        }
                    ]
                }
                """
            },
            new ExampleScenario
            {
                Name = "Conditional Aggregation - Simple",
                Description = "Filter array elements before aggregation with simple conditions",
                SourceJson = """
                {
                    "transactions": [
                        { "amount": 50.5, "type": "expense" },
                        { "amount": 150.0, "type": "income" },
                        { "amount": 75.0, "type": "expense" },
                        { "amount": 200.0, "type": "income" },
                        { "amount": 25.0, "type": "expense" }
                    ]
                }
                """,
                TemplateJson = """
                {
                    "mappings": [
                        {
                            "to": "totalHighValueTransactions",
                            "from": "$.transactions[*]",
                            "aggregation": {
                                "type": "sum",
                                "field": "amount",
                                "condition": "$.item.amount > 100"
                            }
                        },
                        {
                            "to": "highValueTransactionCount",
                            "from": "$.transactions[*]",
                            "aggregation": {
                                "type": "count",
                                "condition": "$.item.amount > 100"
                            }
                        }
                    ]
                }
                """
            },
            new ExampleScenario
            {
                Name = "Conditional Aggregation - Complex",
                Description = "Filter array elements with complex boolean conditions before aggregation",
                SourceJson = """
                {
                    "orders": [
                        { "amount": 50.5, "status": "completed", "priority": "low" },
                        { "amount": 150.0, "status": "completed", "priority": "high" },
                        { "amount": 75.0, "status": "pending", "priority": "medium" },
                        { "amount": 200.0, "status": "completed", "priority": "high" },
                        { "amount": 125.0, "status": "completed", "priority": "medium" }
                    ]
                }
                """,
                TemplateJson = """
                {
                    "mappings": [
                        {
                            "to": "totalHighPriorityCompletedOrders",
                            "from": "$.orders[*]",
                            "aggregation": {
                                "type": "sum",
                                "field": "amount",
                                "condition": "$.item.status == 'completed' && $.item.priority == 'high' && $.item.amount > 100"
                            }
                        },
                        {
                            "to": "completedOrdersOverTarget",
                            "from": "$.orders[*]",
                            "aggregation": {
                                "type": "count",
                                "condition": "$.item.status == 'completed' && $.item.amount >= 100"
                            }
                        },
                        {
                            "to": "averageCompletedOrderValue",
                            "from": "$.orders[*]",
                            "aggregation": {
                                "type": "avg",
                                "field": "amount",
                                "condition": "$.item.status == 'completed'"
                            }
                        }
                    ]
                }
                """
            },
            new ExampleScenario
            {
                Name = "Multi-Condition Logic",
                Description = "Complex conditional logic with multiple criteria and nested conditions",
                SourceJson = """
                {
                    "employee": {
                        "name": "Sarah Johnson",
                        "age": 28,
                        "department": "Engineering", 
                        "yearsOfExperience": 5,
                        "performanceScore": 8.5,
                        "isManager": false,
                        "location": "Remote"
                    }
                }
                """,
                TemplateJson = """
                {
                    "mappings": [
                        {
                            "from": "$.employee.name",
                            "to": "$.result.employeeName"
                        },
                        {
                            "from": "$.employee.age",
                            "to": "$.result.ageCategory",
                            "conditions": [
                                {
                                    "if": "$.employee.age >= 60",
                                    "then": "Senior",
                                    "elseif": [
                                        {
                                            "if": "$.employee.age >= 40",
                                            "then": "Mid-Career"
                                        },
                                        {
                                            "if": "$.employee.age >= 25",
                                            "then": "Early Career"
                                        }
                                    ],
                                    "else": "Entry Level"
                                }
                            ]
                        },
                        {
                            "from": "$.employee.performanceScore",
                            "to": "$.result.performanceLevel",
                            "conditions": [
                                {
                                    "if": "$.employee.performanceScore >= 9.0",
                                    "then": "Exceptional",
                                    "elseif": [
                                        {
                                            "if": "$.employee.performanceScore >= 7.5",
                                            "then": "High Performer"
                                        },
                                        {
                                            "if": "$.employee.performanceScore >= 6.0",
                                            "then": "Good Performer"
                                        }
                                    ],
                                    "else": "Needs Improvement"
                                }
                            ]
                        },
                        {
                            "from": "$.employee.yearsOfExperience",
                            "to": "$.result.eligibleForPromotion",
                            "conditions": [
                                {
                                    "if": "$.employee.yearsOfExperience >= 3",
                                    "then": "Yes",
                                    "else": "No"
                                }
                            ]
                        },
                        {
                            "from": "$.employee.department",
                            "to": "$.result.workStyle",
                            "conditions": [
                                {
                                    "if": "$.employee.location == \"Remote\"",
                                    "then": "Remote Worker",
                                    "elseif": [
                                        {
                                            "if": "$.employee.department contains \"Engineering\"",
                                            "then": "Technical Team"
                                        },
                                        {
                                            "if": "$.employee.department contains \"Sales\"", 
                                            "then": "Client Facing"
                                        }
                                    ],
                                    "else": "Office Based"
                                }
                            ]
                        },
                        {
                            "to": "$.result.bonusEligible",
                            "conditions": [
                                {
                                    "if": "$.employee.performanceScore >= 8.0",
                                    "then": "Eligible for Performance Bonus",
                                    "else": "Standard Compensation"
                                }
                            ]
                        },
                        {
                            "to": "$.result.summary",
                            "concat": "{$.employee.name} is a {$.result.performanceLevel} in the {$.result.ageCategory} category"
                        }
                    ]
                }
                """
            }
        };
    }
}
