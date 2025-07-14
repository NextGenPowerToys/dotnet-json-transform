using FluentAssertions;
using Json.Transform.Core;
using Json.Transform.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Json.Transform.Tests;

public class JsonTransformerTests
{
    [Fact]
    public void Transform_SimpleFieldMapping_ShouldMapCorrectly()
    {
        // Arrange
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

        // Act
        var result = transformer.Transform(sourceJson, templateJson);
        var resultNode = JsonNode.Parse(result);

        // Assert
        resultNode.Should().NotBeNull();
        resultNode!["customer"]!["fullName"]!.GetValue<string>().Should().Be("John Doe");
        resultNode["customer"]!["contactInfo"]!["email"]!.GetValue<string>().Should().Be("john@example.com");
    }

    [Fact]
    public void Transform_ConstantValue_ShouldSetConstant()
    {
        // Arrange
        var sourceJson = "{}";
        var templateJson = """
        {
            "mappings": [
                {
                    "to": "$.metadata.version",
                    "value": "1.0"
                },
                {
                    "to": "$.metadata.timestamp",
                    "value": "now"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();

        // Act
        var result = transformer.Transform(sourceJson, templateJson);
        var resultNode = JsonNode.Parse(result);

        // Assert
        resultNode.Should().NotBeNull();
        resultNode!["metadata"]!["version"]!.GetValue<string>().Should().Be("1.0");
        resultNode["metadata"]!["timestamp"]!.GetValue<string>().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Transform_ConditionalLogic_ShouldApplyConditions()
    {
        // Arrange
        var sourceJson = """
        {
            "user": {
                "age": 25
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "from": "$.user.age",
                    "to": "$.customer.status",
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

        // Act
        var result = transformer.Transform(sourceJson, templateJson);
        var resultNode = JsonNode.Parse(result);

        // Assert
        resultNode.Should().NotBeNull();
        resultNode!["customer"]!["status"]!.GetValue<string>().Should().Be("Adult");
    }

    [Fact]
    public void Transform_StringConcatenation_ShouldCombineFields()
    {
        // Arrange
        var sourceJson = """
        {
            "user": {
                "firstName": "John",
                "lastName": "Doe"
            }
        }
        """;

        var templateJson = """
        {
            "mappings": [
                {
                    "to": "$.customer.fullName",
                    "concat": "{$.user.firstName} {$.user.lastName}"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();

        // Act
        var result = transformer.Transform(sourceJson, templateJson);
        var resultNode = JsonNode.Parse(result);

        // Assert
        resultNode.Should().NotBeNull();
        resultNode!["customer"]!["fullName"]!.GetValue<string>().Should().Be("John Doe");
    }

    [Fact]
    public void Transform_Aggregation_ShouldCalculateSum()
    {
        // Arrange
        var sourceJson = """
        {
            "orders": [
                {"total": 100.50},
                {"total": 75.25},
                {"total": 200.00}
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
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();

        // Act
        var result = transformer.Transform(sourceJson, templateJson);
        var resultNode = JsonNode.Parse(result);

        // Assert
        resultNode.Should().NotBeNull();
        resultNode!["summary"]!["totalAmount"]!.GetValue<double>().Should().Be(375.75);
        resultNode["summary"]!["orderCount"]!.GetValue<int>().Should().Be(3);
    }

    [Fact]
    public void Transform_MathOperation_ShouldCalculateResult()
    {
        // Arrange
        var sourceJson = """
        {
            "order": {
                "subtotal": 100.00,
                "tax": 8.50
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
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();

        // Act
        var result = transformer.Transform(sourceJson, templateJson);
        var resultNode = JsonNode.Parse(result);

        // Assert
        resultNode.Should().NotBeNull();
        resultNode!["order"]!["total"]!.GetValue<double>().Should().Be(108.50);
    }

    [Fact]
    public void ValidateTemplate_ValidTemplate_ShouldReturnNoErrors()
    {
        // Arrange
        var templateJson = """
        {
            "mappings": [
                {
                    "from": "$.user.name",
                    "to": "$.customer.fullName"
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();

        // Act
        var errors = transformer.ValidateTemplate(templateJson);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateTemplate_InvalidTemplate_ShouldReturnErrors()
    {
        // Arrange
        var templateJson = """
        {
            "mappings": [
                {
                    "to": ""
                }
            ]
        }
        """;

        var transformer = new JsonTransformer();

        // Act
        var errors = transformer.ValidateTemplate(templateJson);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(error => error.Contains("'to' property is required"));
    }
}
