using Json.Transform.Core;
using Json.Transform.Models;
using System.Text.Json;
using System.Diagnostics;

namespace Json.Transform.Examples;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== JSON Transform Library Examples ===\n");

        var generateDemo = args.Contains("--demo") || args.Contains("-d");
        var runTests = args.Contains("--tests") || args.Contains("-t");
        var openBrowser = !args.Contains("--no-browser");

        if (runTests)
        {
            await RunTests();
            Console.WriteLine();
        }

        if (generateDemo)
        {
            await GenerateHtmlDemo(openBrowser);
            Console.WriteLine();
        }

        // Run code examples
        Console.WriteLine("Running transformation examples...\n");

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

        Console.WriteLine("\n=== Examples Complete ===");
        Console.WriteLine("\nTip: Use --demo to generate HTML demo, --tests to run tests first");
    }

    static async Task RunTests()
    {
        Console.WriteLine("🧪 Running tests...");
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "test",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("✅ All tests passed!");
            }
            else
            {
                Console.WriteLine("❌ Some tests failed!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error running tests: {ex.Message}");
        }
    }

    static async Task GenerateHtmlDemo(bool openBrowser)
    {
        Console.WriteLine("🎬 Generating HTML demo...");
        try
        {
            var generator = new DemoGenerator();
            var outputPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "demo.html");
            
            var stopwatch = Stopwatch.StartNew();
            await generator.SaveDemoToFile(outputPath);
            stopwatch.Stop();

            Console.WriteLine($"✅ Demo generated in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"📄 Output: {outputPath}");

            if (openBrowser)
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        Process.Start("open", outputPath);
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        Process.Start("xdg-open", outputPath);
                    }
                    Console.WriteLine("🌐 Demo opened in browser");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Could not open browser: {ex.Message}");
                    Console.WriteLine($"Please open manually: {outputPath}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating demo: {ex.Message}");
        }
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
                    "to": "$.customer.contact.email"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine($"  Input: {FormatJson(sourceJson)}");
        Console.WriteLine($"  Output: {FormatJson(result)}");
        Console.WriteLine();
    }

    static void ConditionalLogicExample()
    {
        Console.WriteLine("2. Conditional Logic:");
        
        var sourceJson = """
        {
            "user": {
                "name": "Alice Smith",
                "age": 17,
                "country": "US"
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "from": "$.user.name",
                    "to": "$.result.name"
                },
                {
                    "from": "$.user.age",
                    "to": "$.result.status",
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
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine($"  Input: {FormatJson(sourceJson)}");
        Console.WriteLine($"  Output: {FormatJson(result)}");
        Console.WriteLine();
    }

    static void StringConcatenationExample()
    {
        Console.WriteLine("3. String Concatenation:");
        
        var sourceJson = """
        {
            "user": {
                "firstName": "Bob",
                "lastName": "Johnson",
                "title": "Mr."
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "to": "$.user.displayName",
                    "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine($"  Input: {FormatJson(sourceJson)}");
        Console.WriteLine($"  Output: {FormatJson(result)}");
        Console.WriteLine();
    }

    static void AggregationExample()
    {
        Console.WriteLine("4. Aggregation Operations:");
        
        var sourceJson = """
        {
            "orders": [
                {"id": 1, "amount": 100.50},
                {"id": 2, "amount": 75.25},
                {"id": 3, "amount": 200.00}
            ]
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "from": "$.orders[*].amount",
                    "to": "$.summary.totalAmount",
                    "aggregate": "sum"
                },
                {
                    "from": "$.orders",
                    "to": "$.summary.orderCount",
                    "aggregate": "count"
                },
                {
                    "from": "$.orders[*].amount",
                    "to": "$.summary.averageAmount",
                    "aggregate": "avg"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine($"  Input: {FormatJson(sourceJson)}");
        Console.WriteLine($"  Output: {FormatJson(result)}");
        Console.WriteLine();
    }

    static void MathOperationExample()
    {
        Console.WriteLine("5. Mathematical Operations:");
        
        var sourceJson = """
        {
            "product": {
                "price": 100.00,
                "taxRate": 0.08,
                "discount": 10.00
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "to": "$.result.subtotal",
                    "math": {
                        "operation": "subtract",
                        "operands": ["$.product.price", "$.product.discount"]
                    }
                },
                {
                    "to": "$.result.tax",
                    "math": {
                        "operation": "multiply",
                        "operands": [
                            {"operation": "subtract", "operands": ["$.product.price", "$.product.discount"]},
                            "$.product.taxRate"
                        ]
                    }
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine($"  Input: {FormatJson(sourceJson)}");
        Console.WriteLine($"  Output: {FormatJson(result)}");
        Console.WriteLine();
    }

    static void ComplexTransformationExample()
    {
        Console.WriteLine("6. Complex Transformation:");
        
        var sourceJson = """
        {
            "customer": {
                "name": "Emma Wilson",
                "age": 28,
                "email": "emma@example.com"
            },
            "orders": [
                {"id": 1, "total": 150.75, "status": "completed"},
                {"id": 2, "total": 89.99, "status": "pending"}
            ],
            "preferences": {
                "currency": "USD",
                "notifications": true
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "from": "$.customer.name",
                    "to": "$.profile.displayName"
                },
                {
                    "from": "$.customer.email",
                    "to": "$.profile.contact.email"
                },
                {
                    "from": "$.customer.age",
                    "to": "$.profile.ageGroup",
                    "conditions": [
                        {
                            "if": "$.customer.age < 25",
                            "then": "Young Adult",
                            "else": "Adult"
                        }
                    ]
                },
                {
                    "from": "$.orders[*].total",
                    "to": "$.orderSummary.totalSpent",
                    "aggregate": "sum"
                },
                {
                    "from": "$.orders",
                    "to": "$.orderSummary.orderCount",
                    "aggregate": "count"
                },
                {
                    "to": "$.metadata.generatedAt",
                    "value": "now"
                },
                {
                    "to": "$.metadata.version",
                    "value": "1.0"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();
        var result = transformer.Transform(sourceJson, templateJson);
        
        Console.WriteLine($"  Input: {FormatJson(sourceJson)}");
        Console.WriteLine($"  Output: {FormatJson(result)}");
        Console.WriteLine();
    }

    static string FormatJson(string json)
    {
        try
        {
            var jsonDocument = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }
}
