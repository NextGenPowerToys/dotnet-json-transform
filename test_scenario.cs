using Json.Transform.Core;
using System.Text.Json;

var sourceJson = """
{
  "orders": [
    {
      "id": 1,
      "amount": 100.5
    },
    {
      "id": 2,
      "amount": 75.25
    },
    {
      "id": 3,
      "amount": 200
    }
  ]
}
""";

var templateJson = """
{
  "mappings": [
    {
      "from": "$.orders[*].amount",
      "to": "$.summary.totalAmount",
      "aggregate": "sum",
      "conditions": [
        {
          "if": "$.orders[*].amount >= 75 && $.orders[*].amount<=120",
          "then": "$.orders[*].amount"
        },
        {
          "if": "$.orders[*].amount >= 120",
          "then": "0" 
        }
      ]
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

Console.WriteLine("Source JSON:");
Console.WriteLine(JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(sourceJson), new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine("\nTemplate JSON:");
Console.WriteLine(JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(templateJson), new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine("\nResult:");
Console.WriteLine(JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(result), new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine("\n=== ANALYSIS ===");
Console.WriteLine("Orders analysis:");
Console.WriteLine("- Order 1: amount=100.5 (75 <= 100.5 <= 120) ✓ Include");
Console.WriteLine("- Order 2: amount=75.25 (75 <= 75.25 <= 120) ✓ Include"); 
Console.WriteLine("- Order 3: amount=200 (200 >= 120) ✗ Replace with 0");
Console.WriteLine("\nExpected totalAmount with conditions: 100.5 + 75.25 + 0 = 175.75");
Console.WriteLine("Actual totalAmount: depends on implementation behavior");
