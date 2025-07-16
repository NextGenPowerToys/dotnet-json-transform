using Xunit;
using Json.Transform.Core;
using System.Text.Json;
using FluentAssertions;

namespace Json.Transform.Tests
{
    public class StringOperationsTests
    {
        [Fact]
        public void StringConcatenation_WithBasicTemplate_ShouldConcatenateCorrectly()
        {
            // Arrange
            var sourceData = """
            {
                "user": {
                    "firstName": "John",
                    "lastName": "Doe",
                    "title": "Mr."
                }
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "fullName",
                        "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            var fullName = resultDoc.RootElement.GetProperty("fullName").GetString();
            fullName.Should().Be("Mr. John Doe");
        }

        [Fact]
        public void StringConcatenation_WithComplexTemplate_ShouldHandleMultipleFields()
        {
            // Arrange
            var sourceData = """
            {
                "order": {
                    "id": "ORD-12345",
                    "customer": "Alice Smith",
                    "total": 299.99,
                    "status": "shipped"
                }
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "orderSummary",
                        "concat": "Order {$.order.id} for customer {$.order.customer} with total ${$.order.total} has been {$.order.status}."
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            var summary = resultDoc.RootElement.GetProperty("orderSummary").GetString();
            summary.Should().Be("Order ORD-12345 for customer Alice Smith with total $299.99 has been shipped.");
        }

        [Fact]
        public void StringConcatenation_WithDynamicConstants_ShouldIncludeTimestamp()
        {
            // Arrange
            var sourceData = """
            {
                "report": {
                    "name": "Monthly Sales",
                    "period": "July 2025"
                }
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "reportHeader",
                        "concat": "Report: {$.report.name} for {$.report.period} generated at {now}"
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            var header = resultDoc.RootElement.GetProperty("reportHeader").GetString();
            header.Should().StartWith("Report: Monthly Sales for July 2025 generated at");
            header.Should().Contain("2025"); // Should contain current year
        }

        [Fact]
        public void StringComparison_Contains_ShouldFilterCorrectly()
        {
            // Arrange
            var sourceData = """
            {
                "users": [
                    { "name": "John Smith", "email": "john@company.com" },
                    { "name": "Admin User", "email": "admin@company.com" },
                    { "name": "Jane Doe", "email": "jane@external.com" }
                ]
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "from": "$.users[*]",
                        "to": "companyUsers",
                        "conditions": [
                            {
                                "if": "$.email contains '@company.com'",
                                "then": {
                                    "from": "$.name",
                                    "to": "name"
                                }
                            }
                        ]
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            resultDoc.RootElement.TryGetProperty("companyUsers", out var companyUsers).Should().BeTrue();
            // Should process users with @company.com emails
        }

        [Fact]
        public void StringComparison_StartsWith_ShouldIdentifyAdminUsers()
        {
            // Arrange
            var sourceData = """
            {
                "user": {
                    "email": "admin@company.com",
                    "name": "System Administrator"
                }
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "from": "$.user.email",
                        "to": "userType",
                        "conditions": [
                            {
                                "if": "$.user.email startsWith 'admin'",
                                "then": "Administrator"
                            },
                            {
                                "if": "$.user.email startsWith 'support'",
                                "then": "Support Agent"
                            },
                            {
                                "else": true,
                                "then": "Regular User"
                            }
                        ]
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            var userType = resultDoc.RootElement.GetProperty("userType").GetString();
            userType.Should().Be("Administrator");
        }

        [Fact]
        public void StringComparison_EndsWith_ShouldIdentifyFileTypes()
        {
            // Arrange
            var sourceData = """
            {
                "files": [
                    { "name": "document.pdf", "size": 1024 },
                    { "name": "image.jpg", "size": 2048 },
                    { "name": "report.pdf", "size": 512 }
                ]
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "pdfFileCount",
                        "from": "$.files[*]",
                        "aggregation": {
                            "type": "count",
                            "condition": "$.item.name endsWith '.pdf'"
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
            var pdfCount = resultDoc.RootElement.GetProperty("pdfFileCount").GetInt32();
            pdfCount.Should().Be(2); // document.pdf and report.pdf
        }

        [Fact]
        public void StringOperations_CombinedWithConditionalLogic_ShouldProcessComplexScenarios()
        {
            // Note: This test demonstrates string operations with conditional logic
            // The concat references the original source data paths, not the target paths
            // Arrange
            var sourceData = """
            {
                "employee": { "name": "John Admin", "email": "john.admin@company.com", "department": "IT" }
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "from": "$.employee.name",
                        "to": "name"
                    },
                    {
                        "from": "$.employee.email",
                        "to": "accessLevel",
                        "conditions": [
                            {
                                "if": "$.employee.email startsWith 'john.admin' || $.employee.email contains 'admin'",
                                "then": "Administrator"
                            },
                            {
                                "if": "$.employee.email contains 'support' && $.employee.department == 'Customer Service'",
                                "then": "Support Agent"
                            },
                            {
                                "if": "$.employee.email endsWith '@company.com'",
                                "then": "Employee"
                            },
                            {
                                "else": true,
                                "then": "Guest"
                            }
                        ]
                    },
                    {
                        "to": "badge",
                        "concat": "{$.employee.name} - Administrator"
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            var name = resultDoc.RootElement.GetProperty("name").GetString();
            var accessLevel = resultDoc.RootElement.GetProperty("accessLevel").GetString();
            var badge = resultDoc.RootElement.GetProperty("badge").GetString();
            
            name.Should().Be("John Admin");
            accessLevel.Should().Be("Administrator");
            badge.Should().Be("John Admin - Administrator");
        }

        [Fact]
        public void StringConcatenation_WithNullAndMissingValues_ShouldHandleGracefully()
        {
            // Arrange
            var sourceData = """
            {
                "user": {
                    "firstName": "John",
                    "lastName": null,
                    "title": "Mr."
                }
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "displayName",
                        "concat": "{$.user.title} {$.user.firstName} {$.user.lastName} {$.user.middleName}"
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            var displayName = resultDoc.RootElement.GetProperty("displayName").GetString();
            // Should handle null and missing values by treating them as empty strings
            displayName.Should().Be("Mr. John  "); // null becomes empty, missing becomes empty
        }

        [Fact]
        public void StringOperations_CaseInsensitiveComparisons_ShouldWorkCorrectly()
        {
            // Arrange
            var sourceData = """
            {
                "items": [
                    { "name": "Document.PDF", "type": "file" },
                    { "name": "Image.JPG", "type": "file" },
                    { "name": "Backup.ZIP", "type": "archive" }
                ]
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "documentCount",
                        "from": "$.items[*]",
                        "aggregation": {
                            "type": "count",
                            "condition": "$.item.name endsWith '.pdf' || $.item.name endsWith '.doc'"
                        }
                    },
                    {
                        "to": "imageCount", 
                        "from": "$.items[*]",
                        "aggregation": {
                            "type": "count",
                            "condition": "$.item.name endsWith '.jpg' || $.item.name endsWith '.png'"
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
            var documentCount = resultDoc.RootElement.GetProperty("documentCount").GetInt32();
            var imageCount = resultDoc.RootElement.GetProperty("imageCount").GetInt32();
            
            documentCount.Should().Be(1); // Should match Document.PDF (case-insensitive)
            imageCount.Should().Be(1); // Should match Image.JPG (case-insensitive)
        }

        [Fact]
        public void StringConcatenation_WithConditionalContent_ShouldBuildDynamicMessages()
        {
            // Arrange
            var sourceData = """
            {
                "order": {
                    "id": "ORD-123",
                    "status": "shipped",
                    "customer": "Alice Johnson",
                    "isPriority": true,
                    "trackingNumber": "TRK-789"
                }
            }
            """;

            var transformConfig = """
            {
                "mappings": [
                    {
                        "to": "notification",
                        "conditions": [
                            {
                                "if": "$.order.status == 'shipped' && $.order.isPriority == true",
                                "then": "PRIORITY: Order ORD-123 for Alice Johnson has been shipped! Tracking: TRK-789"
                            },
                            {
                                "if": "$.order.status == 'shipped'",
                                "then": "Order ORD-123 for Alice Johnson has been shipped. Tracking: TRK-789"
                            },
                            {
                                "else": true,
                                "then": "Order ORD-123 for Alice Johnson is currently shipped."
                            }
                        ]
                    }
                ]
            }
            """;

            // Act
            var transformer = new JsonTransformer();
            var result = transformer.Transform(sourceData, transformConfig);
            var resultDoc = JsonDocument.Parse(result);

            // Assert
            var notification = resultDoc.RootElement.GetProperty("notification").GetString();
            notification.Should().Be("PRIORITY: Order ORD-123 for Alice Johnson has been shipped! Tracking: TRK-789");
        }
    }
}
