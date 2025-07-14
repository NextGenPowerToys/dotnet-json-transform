using System;
using System.Threading.Tasks;
using Json.Transform.Core;

class Program
{
    static async Task Main(string[] args)
    {
        var transformer = new JsonTransformer();

        var sourceJson = """
        {
            "users": [
                {"name": "Alice", "age": 17},
                {"name": "Bob", "age": 25},
                {"name": "Charlie", "age": 16}
            ]
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "from": "$.users[*]",
                    "to": "$.customers[*]",
                    "mappings": [
                        {
                            "from": "$.name",
                            "to": "$.fullName"
                        },
                        {
                            "from": "$.age",
                            "to": "$.category",
                            "conditions": [
                                {
                                    "if": "$.age >= 18",
                                    "then": "Adult",
                                    "else": "Minor"
                                }
                            ]
                        }
                    ]
                }
            ]
        }
        """;

        Console.WriteLine("=== TESTING CONDITIONAL TRANSFORMATION ===");
        Console.WriteLine();
        
        Console.WriteLine("Source JSON:");
        Console.WriteLine(sourceJson);
        Console.WriteLine();

        Console.WriteLine("Template JSON:");
        Console.WriteLine(templateJson);
        Console.WriteLine();

        try
        {
            var result = await transformer.TransformAsync(sourceJson, templateJson);
            Console.WriteLine("Result JSON:");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
