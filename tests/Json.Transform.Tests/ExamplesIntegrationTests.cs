using Xunit;
using System.Threading.Tasks;
using System.Linq;
using Json.Transform.Core;
using System.Text.Json;

namespace Json.Transform.Tests
{
    public class ExamplesIntegrationTests
    {
        [Fact]
        public void Examples_MultiConditionLogic_ShouldProduceExpectedResult()
        {
            // Arrange
            var transformer = new JsonTransformer();
            var sourceJson = """
            {
                "employee": {
                    "name": "Sarah Johnson",
                    "age": 28,
                    "department": "Engineering",
                    "yearsOfExperience": 5,
                    "performanceScore": 8.5,
                    "isManager": false,
                    "location": "Remote",
                    "salary": 75000
                }
            }
            """;

            var templateJson = """
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
                                "then": "Senior"
                            },
                            {
                                "if": "$.employee.age >= 40",
                                "then": "Mid-Career"
                            },
                            {
                                "if": "$.employee.age >= 25",
                                "then": "Early Career"
                            },
                            {
                                "if": "$.employee.age < 25",
                                "then": "Entry Level"
                            }
                        ]
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
                    }
                ]
            }
            """;

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("ageCategory", result);
            Assert.Contains("eligibleForPromotion", result);
            
            var resultJson = JsonDocument.Parse(result);
            var ageCategory = resultJson.RootElement.GetProperty("result").GetProperty("ageCategory").GetString();
            var promotionEligible = resultJson.RootElement.GetProperty("result").GetProperty("eligibleForPromotion").GetString();
            
            Assert.Equal("Early Career", ageCategory);
            Assert.Equal("Conditional - Meets Experience but Performance Below Threshold", promotionEligible);
        }

        [Fact]
        public void Examples_FieldMapping_ShouldMapFieldsCorrectly()
        {
            // Arrange
            var transformer = new JsonTransformer();
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

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("customer", result);
            Assert.Contains("fullName", result);
            Assert.Contains("John Doe", result);
        }

        [Fact]
        public void Examples_ConditionalLogic_ShouldApplyConditionsCorrectly()
        {
            // Arrange
            var transformer = new JsonTransformer();
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

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("result", result);
            Assert.Contains("Alice Smith", result);
            Assert.Contains("Minor", result);
        }

        [Fact]
        public void Examples_StringConcatenation_ShouldConcatenateCorrectly()
        {
            // Arrange
            var transformer = new JsonTransformer();
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

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("displayName", result);
            Assert.Contains("Mr. Bob Johnson", result);
        }

        [Fact]
        public void Examples_AggregationOperations_ShouldAggregateCorrectly()
        {
            // Arrange
            var transformer = new JsonTransformer();
            var sourceJson = """
            {
                "orders": [
                    { "id": 1, "amount": 100.50 },
                    { "id": 2, "amount": 75.25 },
                    { "id": 3, "amount": 200.00 }
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

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("totalAmount", result);
            Assert.Contains("orderCount", result);
            Assert.Contains("averageAmount", result);
            Assert.Contains("375.75", result);
        }
    }
}
