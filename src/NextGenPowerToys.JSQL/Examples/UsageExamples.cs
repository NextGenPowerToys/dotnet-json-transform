using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NextGenPowerToys.JSQL;
using NextGenPowerToys.JSQL.Core;

namespace NextGenPowerToys.JSQL.Examples
{
    /// <summary>
    /// Example usage of the NextGenPowerToys.JSQL library
    /// </summary>
    public static class UsageExamples
    {
        /// <summary>
        /// Basic usage example showing how to analyze a simple SQL query against JSON data
        /// </summary>
        public static async Task BasicUsageExample()
        {
            // Setup dependency injection
            var services = new ServiceCollection();
            services.AddNextGenPowerToysJSQL();
            
            var serviceProvider = services.BuildServiceProvider();
            var analyzer = serviceProvider.GetRequiredService<ITransformationAnalyzer>();
            
            // Example JSON data
            var jsonExample = JsonNode.Parse("""
            {
              "users": [
                {
                  "id": 1,
                  "name": "John Doe",
                  "email": "john@example.com",
                  "age": 30,
                  "active": true
                },
                {
                  "id": 2,
                  "name": "Jane Smith",
                  "email": "jane@example.com",
                  "age": 25,
                  "active": false
                }
              ]
            }
            """);
            
            // Example SQL query
            var sqlQuery = "SELECT name, email, age FROM users WHERE active = true ORDER BY age DESC";
            
            // Analyze the transformation
            var result = await analyzer.AnalyzeTransformation(jsonExample.ToString(), sqlQuery);
            
            // Display results
            Console.WriteLine($"Analysis Success: {result.IsSuccess}");
            
            if (result.IsSuccess && result.Template != null)
            {
                Console.WriteLine("Generated Template:");
                Console.WriteLine(JsonSerializer.Serialize(result.Template, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine("Errors:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"- {error.Message}");
                    if (!string.IsNullOrEmpty(error.Suggestion))
                        Console.WriteLine($"  Suggestion: {error.Suggestion}");
                }
            }
        }
        
        /// <summary>
        /// Advanced usage example with custom configuration
        /// </summary>
        public static async Task AdvancedUsageExample()
        {
            // Setup with custom configuration
            var services = new ServiceCollection();
            services.AddNextGenPowerToysJSQL(options =>
            {
                options.MaxComplexity = 5.0;
                options.DetailedErrors = true;
                options.EnableOptimizations = true;
                options.FunctionCompatibilityOverrides["CUSTOM_FUNC"] = true;
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var analyzer = serviceProvider.GetRequiredService<ITransformationAnalyzer>();
            var logger = serviceProvider.GetRequiredService<ILogger<TransformationAnalyzer>>();
            
            // Complex JSON with nested structures
            var complexJson = JsonNode.Parse("""
            {
              "sales": [
                {
                  "id": 1,
                  "amount": 100.50,
                  "date": "2023-01-15",
                  "customer": {
                    "name": "Alice Johnson",
                    "region": "North"
                  },
                  "items": [
                    { "product": "Widget A", "quantity": 2, "price": 25.00 },
                    { "product": "Widget B", "quantity": 3, "price": 16.83 }
                  ]
                }
              ]
            }
            """);
            
            // Complex SQL with aggregations
            var complexSql = """
                SELECT 
                    customer.region,
                    COUNT(*) as order_count,
                    SUM(amount) as total_sales,
                    AVG(amount) as avg_order_value
                FROM sales 
                WHERE amount > 50 
                GROUP BY customer.region 
                ORDER BY total_sales DESC
                """;
            
            logger.LogInformation("Starting complex analysis...");
            
            // Analyze with batch processing
            var batchResults = await analyzer.BatchAnalyze(complexJson.ToJsonString(), new[]
            {
                "SELECT customer.name, SUM(amount) as total_sales FROM sales GROUP BY customer.region ORDER BY total_sales DESC",
                "SELECT customer.name, amount FROM sales WHERE amount > 75"
            });
            
            // Display batch results
            int counter = 1;
            foreach (var kvp in batchResults)
            {
                var result = kvp.Value;
                Console.WriteLine($"\n=== Batch Analysis {counter} ===");
                Console.WriteLine($"Query: {kvp.Key}");
                Console.WriteLine($"Success: {result.IsSuccess}");
                Console.WriteLine($"Compatibility Score: {result.Compatibility?.Score:F2}");
                
                if (result.Performance != null)
                {
                    Console.WriteLine($"Estimated Execution Time: {result.Performance.EstimatedExecutionTimeMs:F1}ms");
                    Console.WriteLine($"Complexity Factor: {result.Performance.ComplexityFactor:F2}");
                }
                
                if (result.Warnings.Count > 0)
                {
                    Console.WriteLine("Warnings:");
                    foreach (var warning in result.Warnings)
                    {
                        Console.WriteLine($"- {warning.Message}");
                    }
                }
                counter++;
            }
        }
        
        /// <summary>
        /// Example showing error handling and alternative suggestions
        /// </summary>
        public static async Task ErrorHandlingExample()
        {
            var services = new ServiceCollection();
            services.AddNextGenPowerToysJSQL();
            
            var serviceProvider = services.BuildServiceProvider();
            var analyzer = serviceProvider.GetRequiredService<ITransformationAnalyzer>();
            
            // JSON that doesn't match the SQL expectations
            var jsonExample = JsonNode.Parse("""
            {
              "products": [
                {
                  "name": "Product A",
                  "description": "A great product",
                  "in_stock": true
                }
              ]
            }
            """);
            
            // SQL that references fields not in JSON
            var incompatibleSql = """
                SELECT 
                    product_id,
                    price,
                    quantity_available,
                    last_updated
                FROM inventory 
                WHERE price > 100 
                AND category = 'electronics'
                """;
            
            var result = await analyzer.AnalyzeTransformation(jsonExample.ToJsonString(), incompatibleSql);
            
            Console.WriteLine("=== Error Handling Example ===");
            Console.WriteLine($"Success: {result.IsSuccess}");
            
            if (!result.IsSuccess)
            {
                Console.WriteLine("\nDetailed Errors:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Type: {error.Type}");
                    Console.WriteLine($"Message: {error.Message}");
                    Console.WriteLine($"SQL Part: {error.SqlPart}");
                    Console.WriteLine($"Suggestion: {error.Suggestion}");
                    Console.WriteLine($"Severity: {error.Severity}");
                    Console.WriteLine();
                }
                
                if (result.Alternatives != null)
                {
                    Console.WriteLine("Alternative Suggestions:");
                    foreach (var suggestion in result.Alternatives.Suggestions)
                    {
                        Console.WriteLine($"- {suggestion}");
                    }
                    
                    if (!string.IsNullOrEmpty(result.Alternatives.SimplifiedQuery))
                    {
                        Console.WriteLine($"\nSimplified Query: {result.Alternatives.SimplifiedQuery}");
                    }
                    
                    if (result.Alternatives.RequiredJsonChanges.Count > 0)
                    {
                        Console.WriteLine("\nRequired JSON Changes:");
                        foreach (var change in result.Alternatives.RequiredJsonChanges)
                        {
                            Console.WriteLine($"- {change}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Example of compatibility validation
        /// </summary>
        public static async Task CompatibilityValidationExample()
        {
            var services = new ServiceCollection();
            services.AddNextGenPowerToysJSQL();
            
            var serviceProvider = services.BuildServiceProvider();
            var analyzer = serviceProvider.GetRequiredService<ITransformationAnalyzer>();
            
            var jsonExample = JsonNode.Parse("""
            {
              "transactions": [
                {
                  "id": "tx123",
                  "amount": 250.75,
                  "timestamp": "2023-12-01T10:30:00Z",
                  "status": "completed"
                }
              ]
            }
            """);
            
            var sqlQuery = "SELECT id, amount, status FROM transactions WHERE amount > 100";
            
            var compatibility = await analyzer.ValidateCompatibility(jsonExample.ToJsonString(), sqlQuery);
            
            Console.WriteLine("=== Compatibility Validation ===");
            Console.WriteLine($"Overall Score: {compatibility.Score:F2}");
            Console.WriteLine($"Is Valid: {compatibility.IsValid}");
            
            Console.WriteLine("\nField Mappings:");
            foreach (var mapping in compatibility.FieldMappings)
            {
                Console.WriteLine($"- {mapping.SqlField} -> {mapping.JsonPath} (Compatible: {mapping.IsCompatible})");
                if (mapping.TypeInfo != null)
                {
                    Console.WriteLine($"  SQL Type: {mapping.TypeInfo.SqlType}, JSON Type: {mapping.TypeInfo.JsonType}");
                    Console.WriteLine($"  Confidence: {mapping.TypeInfo.Confidence:F2}");
                }
            }
        }
    }
}
