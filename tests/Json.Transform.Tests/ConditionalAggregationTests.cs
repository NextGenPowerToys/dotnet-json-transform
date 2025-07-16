using Xunit;
using Json.Transform.Core;
using System.Text.Json;

namespace Json.Transform.Tests
{
    public class ConditionalAggregationTests
    {
        [Fact]
        public void TestConditionalAggregationWithAmountFilter()
        {
            // Arrange
            var sourceData = """
            {
                "transactions": [
                    { "amount": 50.5, "type": "expense" },
                    { "amount": 150.0, "type": "income" },
                    { "amount": 75.0, "type": "expense" },
                    { "amount": 200.0, "type": "income" },
                    { "amount": 25.0, "type": "expense" }
                ]
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "totalAmount",
                        "from": "$.transactions[*]",
                        "aggregation": {
                            "type": "sum",
                            "field": "amount",
                            "condition": "$.item.amount > 100"
                        }
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            Assert.True(resultDoc.RootElement.TryGetProperty("totalAmount", out var totalAmountElement), 
                $"Property 'totalAmount' not found in result: {result}");
            var totalAmount = totalAmountElement.GetDecimal();
            
            // Should sum only amounts > 100: 150.0 + 200.0 = 350.0
            Assert.Equal(350.0m, totalAmount);
        }

        [Fact]
        public void TestConditionalAggregationWithComplexCondition()
        {
            // Arrange
            var sourceData = """
            {
                "transactions": [
                    { "amount": 50.5, "type": "expense", "status": "completed" },
                    { "amount": 150.0, "type": "income", "status": "completed" },
                    { "amount": 75.0, "type": "expense", "status": "pending" },
                    { "amount": 200.0, "type": "income", "status": "completed" },
                    { "amount": 125.0, "type": "expense", "status": "completed" }
                ]
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "totalCompletedExpenses",
                        "from": "$.transactions[*]",
                        "aggregation": {
                            "type": "sum",
                            "field": "amount",
                            "condition": "$.item.type == 'expense' && $.item.status == 'completed' && $.item.amount > 100"
                        }
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            Assert.True(resultDoc.RootElement.TryGetProperty("totalCompletedExpenses", out var totalElement));
            var total = totalElement.GetDecimal();
            
            // Should sum only expense transactions with status=completed and amount>100: 125.0
            Assert.Equal(125.0m, total);
        }

        [Fact]
        public void TestConditionalAggregationCount()
        {
            // Arrange
            var sourceData = """
            {
                "orders": [
                    { "amount": 50.5, "status": "shipped" },
                    { "amount": 150.0, "status": "pending" },
                    { "amount": 75.0, "status": "shipped" },
                    { "amount": 200.0, "status": "delivered" }
                ]
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "shippedOrderCount",
                        "from": "$.orders[*]",
                        "aggregation": {
                            "type": "count",
                            "condition": "$.item.status == 'shipped'"
                        }
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            Assert.True(resultDoc.RootElement.TryGetProperty("shippedOrderCount", out var countElement));
            var count = countElement.GetInt32();
            
            // Should count only orders with status=shipped: 2
            Assert.Equal(2, count);
        }
    }
}
