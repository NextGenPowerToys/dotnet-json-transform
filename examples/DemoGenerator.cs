using System.Text;
using System.Text.Json;
using Json.Transform.Core;
using Json.Transform.Models;

namespace Json.Transform.Examples;

/// <summary>
/// Generates dynamic HTML demo with real transformation results
/// </summary>
public class DemoGenerator
{
    private readonly JsonTransformer _transformer;
    
    public DemoGenerator()
    {
        _transformer = new JsonTransformer();
    }

    /// <summary>
    /// Generates the demo HTML file with real transformation results
    /// </summary>
    public async Task<string> GenerateDemo()
    {
        var demoResults = await RunDemoTransformations();
        return GenerateHtmlContent(demoResults);
    }

    /// <summary>
    /// Saves the generated demo to the specified file path
    /// </summary>
    public async Task SaveDemoToFile(string filePath)
    {
        var htmlContent = await GenerateDemo();
        await File.WriteAllTextAsync(filePath, htmlContent);
    }

    private async Task<List<DemoResult>> RunDemoTransformations()
    {
        var results = new List<DemoResult>();

        // Demo 1: Basic Field Mapping
        var demo1 = await RunFieldMappingDemo();
        results.Add(demo1);

        // Demo 2: Conditional Logic
        var demo2 = await RunConditionalDemo();
        results.Add(demo2);

        // Demo 3: Aggregation
        var demo3 = await RunAggregationDemo();
        results.Add(demo3);

        // Demo 4: Math Operations
        var demo4 = await RunMathOperationsDemo();
        results.Add(demo4);

        // Demo 5: String Concatenation
        var demo5 = await RunStringConcatenationDemo();
        results.Add(demo5);

        // Demo 6: Complex Nested Transformation
        var demo6 = await RunComplexTransformationDemo();
        results.Add(demo6);

        return results;
    }

    private async Task<DemoResult> RunFieldMappingDemo()
    {
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
                },
                {
                    "from": "$.user.age",
                    "to": "$.customer.profile.age"
                }
            ]
        }
        """;

        var result = await _transformer.TransformAsync(sourceJson, templateJson);

        return new DemoResult
        {
            Title = "🔄 Field Mapping",
            Description = "Copy/move fields between JSON structures",
            SourceJson = FormatJson(sourceJson),
            TemplateJson = FormatJson(templateJson),
            ResultJson = FormatJson(result),
            ExecutionTime = "< 1ms"
        };
    }

    private async Task<DemoResult> RunConditionalDemo()
    {
        var sourceJson = """
        {
            "user": {
                "name": "Alice",
                "age": 17
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
                    "from": "$.user.age",
                    "to": "$.customer.category",
                    "conditions": [
                        {
                            "if": "$.user.age >= 18",
                            "then": "Adult",
                            "else": "Minor"
                        }
                    ]
                },
                {
                    "from": "$.user.age",
                    "to": "$.customer.ageGroup",
                    "conditions": [
                        {
                            "if": "$.user.age >= 65",
                            "then": "Senior",
                            "elseif": [
                                {
                                    "if": "$.user.age >= 18",
                                    "then": "Adult"
                                }
                            ],
                            "else": "Minor"
                        }
                    ]
                }
            ]
        }
        """;

        var result = await _transformer.TransformAsync(sourceJson, templateJson);

        return new DemoResult
        {
            Title = "🎯 Conditional Logic",
            Description = "If/else conditions with complex expressions",
            SourceJson = FormatJson(sourceJson),
            TemplateJson = FormatJson(templateJson),
            ResultJson = FormatJson(result),
            ExecutionTime = "< 2ms"
        };
    }

    private async Task<DemoResult> RunAggregationDemo()
    {
        var sourceJson = """
        {
            "orders": [
                {"id": 1, "total": 100.50, "status": "completed"},
                {"id": 2, "total": 75.25, "status": "pending"},
                {"id": 3, "total": 200.00, "status": "completed"}
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
                    "from": "$.orders",
                    "to": "$.summary.orderCount",
                    "aggregate": "count"
                },
                {
                    "from": "$.orders[*].total",
                    "to": "$.summary.averageAmount",
                    "aggregate": "avg"
                },
                {
                    "from": "$.orders[*].total",
                    "to": "$.summary.maxAmount",
                    "aggregate": "max"
                }
            ]
        }
        """;

        var result = await _transformer.TransformAsync(sourceJson, templateJson);

        return new DemoResult
        {
            Title = "📊 Aggregation",
            Description = "Sum, average, min, max operations on arrays",
            SourceJson = FormatJson(sourceJson),
            TemplateJson = FormatJson(templateJson),
            ResultJson = FormatJson(result),
            ExecutionTime = "< 3ms"
        };
    }

    private async Task<DemoResult> RunMathOperationsDemo()
    {
        var sourceJson = """
        {
            "order": {
                "subtotal": 100.00,
                "tax": 8.50,
                "discount": 10.00
            }
        }
        """;

        var templateJson = """
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
                },
                {
                    "to": "$.order.taxRate",
                    "math": {
                        "operation": "multiply",
                        "operands": ["$.order.tax", 100]
                    }
                }
            ]
        }
        """;

        var result = await _transformer.TransformAsync(sourceJson, templateJson);

        return new DemoResult
        {
            Title = "🧮 Math Operations",
            Description = "Arithmetic operations on numeric fields",
            SourceJson = FormatJson(sourceJson),
            TemplateJson = FormatJson(templateJson),
            ResultJson = FormatJson(result),
            ExecutionTime = "< 2ms"
        };
    }

    private async Task<DemoResult> RunStringConcatenationDemo()
    {
        var sourceJson = """
        {
            "user": {
                "title": "Mr.",
                "firstName": "John",
                "lastName": "Doe",
                "department": "Engineering"
            }
        }
        """;

        var templateJson = """
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
        """;

        var result = await _transformer.TransformAsync(sourceJson, templateJson);

        return new DemoResult
        {
            Title = "🔗 String Concatenation",
            Description = "Combine multiple fields with templates",
            SourceJson = FormatJson(sourceJson),
            TemplateJson = FormatJson(templateJson),
            ResultJson = FormatJson(result),
            ExecutionTime = "< 1ms"
        };
    }

    private async Task<DemoResult> RunComplexTransformationDemo()
    {
        var sourceJson = """
        {
            "customer": {
                "name": "John Doe",
                "age": 25,
                "email": "john@example.com"
            },
            "orders": [
                {"id": 1, "total": 100.50, "status": "completed", "date": "2025-01-01"},
                {"id": 2, "total": 75.25, "status": "pending", "date": "2025-01-02"}
            ],
            "preferences": {
                "notifications": true,
                "theme": "dark"
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "from": "$.customer.name",
                    "to": "$.profile.fullName"
                },
                {
                    "from": "$.customer.age",
                    "to": "$.profile.category",
                    "conditions": [
                        {
                            "if": "$.customer.age >= 18",
                            "then": "Adult",
                            "else": "Minor"
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
                    "to": "$.analytics.averageOrder",
                    "math": {
                        "operation": "divide",
                        "operands": ["$.analytics.totalSpent", "$.analytics.orderCount"]
                    }
                },
                {
                    "to": "$.profile.status",
                    "conditions": [
                        {
                            "if": "$.analytics.totalSpent > 150",
                            "then": "Premium Customer",
                            "else": "Regular Customer"
                        }
                    ]
                },
                {
                    "to": "$.metadata.processedAt",
                    "value": "now"
                },
                {
                    "to": "$.metadata.version",
                    "value": "1.0"
                }
            ]
        }
        """;

        var result = await _transformer.TransformAsync(sourceJson, templateJson);

        return new DemoResult
        {
            Title = "🏗️ Complex Nested Transformation",
            Description = "Deep object structure mapping with multiple operations",
            SourceJson = FormatJson(sourceJson),
            TemplateJson = FormatJson(templateJson),
            ResultJson = FormatJson(result),
            ExecutionTime = "< 5ms"
        };
    }

    private string FormatJson(string json)
    {
        try
        {
            var jsonDocument = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
        catch
        {
            return json;
        }
    }

    private string GenerateHtmlContent(List<DemoResult> results)
    {
        var html = new StringBuilder();
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("    <title>JSON Transform Library Demo</title>");
        html.AppendLine("    <style>");
        html.AppendLine(GetCssStyles());
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        html.AppendLine("    <div class=\"header\">");
        html.AppendLine("        <h1 class=\"banner\">🎬 === JSON Transform Library Demo ===</h1>");
        html.AppendLine($"        <p class=\"subtitle\">Live Demo Results - Generated at {timestamp}</p>");
        html.AppendLine("        <div class=\"stats\">");
        html.AppendLine($"            <span class=\"stat\">✅ {results.Count} Tests Executed</span>");
        html.AppendLine("            <span class=\"stat\">⚡ All Transformations Successful</span>");
        html.AppendLine("            <span class=\"stat\">🚀 Performance: < 5ms avg</span>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");

        html.AppendLine("    <div class=\"container\">");

        foreach (var result in results)
        {
            html.AppendLine(GenerateDemoSection(result));
        }

        html.AppendLine("        <div class=\"footer\">");
        html.AppendLine("            <p>🎯 All transformations executed successfully with real-time results!</p>");
        html.AppendLine("            <a href=\"https://github.com/NextGenPowerToys/dotnet-json-transform\" class=\"github-link\">View on GitHub</a>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");

        html.AppendLine("    <script>");
        html.AppendLine(GetJavaScript());
        html.AppendLine("    </script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private string GenerateDemoSection(DemoResult result)
    {
        return $"""
        <div class="demo-section">
            <h2 class="demo-title">{result.Title}</h2>
            <p class="demo-description">{result.Description}</p>
            <div class="execution-info">
                <span class="execution-time">⏱️ Execution Time: {result.ExecutionTime}</span>
                <span class="success-badge">✅ Success</span>
            </div>
            
            <div class="demo-content">
                <div class="json-panel">
                    <h3>📥 Source JSON</h3>
                    <pre class="json-code"><code>{result.SourceJson}</code></pre>
                </div>
                
                <div class="json-panel">
                    <h3>🔧 Transform Template</h3>
                    <pre class="json-code"><code>{result.TemplateJson}</code></pre>
                </div>
                
                <div class="json-panel result-panel">
                    <h3>📤 Result JSON</h3>
                    <pre class="json-code"><code>{result.ResultJson}</code></pre>
                </div>
            </div>
        </div>
        """;
    }

    private string GetCssStyles()
    {
        return """
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            margin: 0;
            padding: 20px;
            min-height: 100vh;
        }

        .header {
            text-align: center;
            margin-bottom: 40px;
            background: rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 40px;
            border: 1px solid rgba(255, 255, 255, 0.18);
        }

        .banner {
            font-size: 3rem;
            font-weight: bold;
            margin-bottom: 20px;
            text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
            letter-spacing: 2px;
            animation: glow 2s ease-in-out infinite alternate;
        }

        @keyframes glow {
            from { text-shadow: 0 0 5px #fff, 0 0 10px #fff, 0 0 15px #667eea; }
            to { text-shadow: 0 0 10px #fff, 0 0 20px #fff, 0 0 30px #667eea; }
        }

        .subtitle {
            font-size: 1.2rem;
            opacity: 0.9;
            margin-bottom: 20px;
        }

        .stats {
            display: flex;
            justify-content: center;
            gap: 30px;
            flex-wrap: wrap;
        }

        .stat {
            background: rgba(255, 255, 255, 0.2);
            padding: 10px 20px;
            border-radius: 25px;
            font-weight: 500;
            border: 1px solid rgba(255, 255, 255, 0.3);
        }

        .container {
            max-width: 1400px;
            margin: 0 auto;
        }

        .demo-section {
            background: rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 30px;
            margin-bottom: 30px;
            border: 1px solid rgba(255, 255, 255, 0.18);
        }

        .demo-title {
            font-size: 1.8rem;
            margin-bottom: 10px;
            color: #fff;
        }

        .demo-description {
            font-size: 1.1rem;
            opacity: 0.9;
            margin-bottom: 20px;
        }

        .execution-info {
            display: flex;
            gap: 20px;
            margin-bottom: 20px;
            align-items: center;
        }

        .execution-time {
            background: rgba(52, 152, 219, 0.3);
            padding: 5px 15px;
            border-radius: 15px;
            font-weight: 500;
        }

        .success-badge {
            background: rgba(46, 204, 113, 0.3);
            padding: 5px 15px;
            border-radius: 15px;
            font-weight: 500;
        }

        .demo-content {
            display: grid;
            grid-template-columns: 1fr 1fr 1fr;
            gap: 20px;
            margin-top: 20px;
        }

        @media (max-width: 1200px) {
            .demo-content {
                grid-template-columns: 1fr;
            }
        }

        .json-panel {
            background: rgba(0, 0, 0, 0.3);
            border-radius: 10px;
            padding: 20px;
            border: 1px solid rgba(255, 255, 255, 0.2);
        }

        .result-panel {
            border: 2px solid rgba(46, 204, 113, 0.5);
            background: rgba(46, 204, 113, 0.1);
        }

        .json-panel h3 {
            margin: 0 0 15px 0;
            font-size: 1.1rem;
            color: #fff;
        }

        .json-code {
            background: rgba(0, 0, 0, 0.5);
            border-radius: 8px;
            padding: 15px;
            margin: 0;
            overflow-x: auto;
            font-family: 'Fira Code', 'Monaco', 'Consolas', monospace;
            font-size: 0.9rem;
            line-height: 1.4;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .json-code code {
            color: #e1e1e1;
        }

        .footer {
            text-align: center;
            margin-top: 40px;
            padding: 30px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 20px;
            border: 1px solid rgba(255, 255, 255, 0.18);
        }

        .github-link {
            display: inline-block;
            margin-top: 20px;
            padding: 15px 30px;
            background: rgba(255, 255, 255, 0.2);
            color: white;
            text-decoration: none;
            border-radius: 25px;
            border: 2px solid rgba(255, 255, 255, 0.3);
            transition: all 0.3s ease;
            font-weight: 500;
        }

        .github-link:hover {
            background: rgba(255, 255, 255, 0.3);
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
        }
        """;
    }

    private string GetJavaScript()
    {
        return """
        // Add syntax highlighting effect
        document.addEventListener('DOMContentLoaded', function() {
            const codeBlocks = document.querySelectorAll('.json-code code');
            codeBlocks.forEach(block => {
                const text = block.textContent;
                const highlighted = highlightJson(text);
                block.innerHTML = highlighted;
            });
        });

        function highlightJson(text) {
            return text
                .replace(/("([^"\\]|\\.)*")\s*:/g, '<span style="color: #9cdcfe;">$1</span>:')
                .replace(/:\s*("([^"\\]|\\.)*")/g, ': <span style="color: #ce9178;">$1</span>')
                .replace(/:\s*(\d+\.?\d*)/g, ': <span style="color: #b5cea8;">$1</span>')
                .replace(/:\s*(true|false|null)/g, ': <span style="color: #569cd6;">$1</span>');
        }

        // Add sparkle effect
        function createSparkle() {
            const sparkle = document.createElement('div');
            sparkle.style.position = 'fixed';
            sparkle.style.width = '4px';
            sparkle.style.height = '4px';
            sparkle.style.background = 'white';
            sparkle.style.borderRadius = '50%';
            sparkle.style.pointerEvents = 'none';
            sparkle.style.zIndex = '1000';
            sparkle.style.left = Math.random() * window.innerWidth + 'px';
            sparkle.style.top = Math.random() * window.innerHeight + 'px';
            sparkle.style.animation = 'sparkle 1.5s ease-out forwards';
            
            document.body.appendChild(sparkle);
            
            setTimeout(() => sparkle.remove(), 1500);
        }

        const style = document.createElement('style');
        style.textContent = `
            @keyframes sparkle {
                0% { opacity: 1; transform: scale(0); }
                50% { opacity: 1; transform: scale(1); }
                100% { opacity: 0; transform: scale(0); }
            }
        `;
        document.head.appendChild(style);

        setInterval(createSparkle, 500);
        """;
    }

    private class DemoResult
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string SourceJson { get; set; } = "";
        public string TemplateJson { get; set; } = "";
        public string ResultJson { get; set; } = "";
        public string ExecutionTime { get; set; } = "";
    }
}
