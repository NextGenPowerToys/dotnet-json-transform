using Xunit;
using Json.Transform.Core;
using Json.Transform.Tests;
using System.Text.Json;

namespace Json.Transform.Tests
{
    public class ComplexExpressionTests
    {
        [Fact]
        public void Transform_ComplexAndExpression_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "employee": {
                    "yearsOfExperience": 5,
                    "performanceScore": 8.5
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.promotionEligible",
                        "conditions": [
                            {
                                "if": "$.employee.yearsOfExperience >= 3 && $.employee.performanceScore >= 9.0",
                                "then": "Yes"
                            },
                            {
                                "if": "$.employee.yearsOfExperience >= 3 && $.employee.performanceScore < 9.0",
                                "then": "Conditional"
                            },
                            {
                                "else": true,
                                "then": "No"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var promotionEligible = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("promotionEligible")
                .GetString();

            Assert.Equal("Conditional", promotionEligible);
        }

        [Fact]
        public void Transform_ComplexOrExpression_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "employee": {
                    "department": "Sales",
                    "location": "Office"
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.workType",
                        "conditions": [
                            {
                                "if": "$.employee.department == 'Engineering' || $.employee.department == 'Sales'",
                                "then": "Core Team"
                            },
                            {
                                "else": true,
                                "then": "Support Team"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var workType = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("workType")
                .GetString();

            Assert.Equal("Core Team", workType);
        }

        [Fact]
        public void Transform_MixedAndOrExpression_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "employee": {
                    "department": "Engineering",
                    "location": "Remote",
                    "performanceScore": 8.5,
                    "yearsOfExperience": 3
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.specialStatus",
                        "conditions": [
                            {
                                "if": "$.employee.location == 'Remote' && $.employee.department == 'Engineering' || $.employee.performanceScore >= 9.0",
                                "then": "Special Employee"
                            },
                            {
                                "else": true,
                                "then": "Regular Employee"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var specialStatus = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("specialStatus")
                .GetString();

            Assert.Equal("Special Employee", specialStatus);
        }

        [Fact]
        public void Transform_ComplexExpressionWithNumbers_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "employee": {
                    "age": 28,
                    "salary": 75000,
                    "performanceScore": 8.5
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.bonusEligible",
                        "conditions": [
                            {
                                "if": "$.employee.age >= 25 && $.employee.salary >= 70000 && $.employee.performanceScore >= 8.0",
                                "then": "Eligible for Bonus"
                            },
                            {
                                "else": true,
                                "then": "Not Eligible"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var bonusEligible = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("bonusEligible")
                .GetString();

            Assert.Equal("Eligible for Bonus", bonusEligible);
        }

        [Fact]
        public void Transform_FalseComplexExpression_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "employee": {
                    "yearsOfExperience": 2,
                    "performanceScore": 9.5
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.promotionEligible",
                        "conditions": [
                            {
                                "if": "$.employee.yearsOfExperience >= 3 && $.employee.performanceScore >= 9.0",
                                "then": "Yes"
                            },
                            {
                                "else": true,
                                "then": "No - Insufficient Experience"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var promotionEligible = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("promotionEligible")
                .GetString();

            Assert.Equal("No - Insufficient Experience", promotionEligible);
        }

        [Fact]
        public void Transform_NestedComplexExpression_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "user": {
                    "age": 35,
                    "isManager": true,
                    "department": "Engineering",
                    "yearsOfExperience": 8,
                    "performanceScore": 9.2,
                    "location": "Remote"
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.executiveTrack",
                        "conditions": [
                            {
                                "if": "$.user.age >= 30 && $.user.isManager == true && ($.user.department == 'Engineering' || $.user.department == 'Product') && $.user.performanceScore >= 9.0",
                                "then": "Executive Track Candidate"
                            },
                            {
                                "if": "$.user.age >= 25 && $.user.yearsOfExperience >= 5 && $.user.performanceScore >= 8.0",
                                "then": "Senior Professional"
                            },
                            {
                                "else": true,
                                "then": "Standard Track"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var executiveTrack = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("executiveTrack")
                .GetString();

            Assert.Equal("Executive Track Candidate", executiveTrack);
        }

        [Fact]
        public void Transform_MultipleOrConditions_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "employee": {
                    "department": "Marketing",
                    "skills": ["Social Media", "Analytics"],
                    "certifications": 2
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.eligibleForMarketing",
                        "conditions": [
                            {
                                "if": "$.employee.department == 'Marketing' || $.employee.department == 'Sales' || $.employee.department == 'Business Development'",
                                "then": "Marketing Track Eligible"
                            },
                            {
                                "else": true,
                                "then": "Not Marketing Track"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var eligibleForMarketing = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("eligibleForMarketing")
                .GetString();

            Assert.Equal("Marketing Track Eligible", eligibleForMarketing);
        }

        [Fact]
        public void Transform_ComplexNumericalConditions_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "metrics": {
                    "revenue": 125000,
                    "growthRate": 0.15,
                    "customerSatisfaction": 4.2,
                    "teamSize": 12
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.performanceCategory",
                        "conditions": [
                            {
                                "if": "$.metrics.revenue >= 100000 && $.metrics.growthRate >= 0.12 && $.metrics.customerSatisfaction >= 4.0 && $.metrics.teamSize >= 10",
                                "then": "High Performer"
                            },
                            {
                                "if": "$.metrics.revenue >= 75000 && $.metrics.growthRate >= 0.08 && $.metrics.customerSatisfaction >= 3.5",
                                "then": "Good Performer"
                            },
                            {
                                "if": "$.metrics.revenue >= 50000 || $.metrics.growthRate >= 0.05",
                                "then": "Average Performer"
                            },
                            {
                                "else": true,
                                "then": "Needs Improvement"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var performanceCategory = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("performanceCategory")
                .GetString();

            Assert.Equal("High Performer", performanceCategory);
        }

        [Fact]
        public void Transform_MixedStringAndNumericalConditions_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "candidate": {
                    "name": "Alice Johnson",
                    "education": "Masters",
                    "experience": 6,
                    "skills": "Python,JavaScript,SQL",
                    "location": "San Francisco",
                    "expectedSalary": 110000
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.hiringDecision",
                        "conditions": [
                            {
                                "if": "$.candidate.education == 'Masters' && $.candidate.experience >= 5 && $.candidate.expectedSalary <= 120000",
                                "then": "Strong Candidate - Proceed to Final Round"
                            },
                            {
                                "if": "$.candidate.education == 'Bachelors' && $.candidate.experience >= 3 || $.candidate.experience >= 7",
                                "then": "Good Candidate - Technical Interview"
                            },
                            {
                                "if": "$.candidate.location == 'San Francisco' || $.candidate.location == 'New York'",
                                "then": "Location Match - Phone Screen"
                            },
                            {
                                "else": true,
                                "then": "Not a Match"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var hiringDecision = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("hiringDecision")
                .GetString();

            Assert.Equal("Strong Candidate - Proceed to Final Round", hiringDecision);
        }

        [Fact]
        public void Transform_ComplexBooleanLogicEdgeCases_ShouldEvaluateCorrectly()
        {
            // Arrange
            var sourceJson = """
            {
                "project": {
                    "isUrgent": false,
                    "budget": 75000,
                    "teamSize": 8,
                    "complexity": "Medium",
                    "clientTier": "Premium"
                }
            }
            """;

            var templateJson = """
            {
                "mappings": [
                    {
                        "to": "$.result.projectPriority",
                        "conditions": [
                            {
                                "if": "$.project.isUrgent == true || $.project.clientTier == 'Premium' && $.project.budget >= 50000",
                                "then": "High Priority"
                            },
                            {
                                "if": "$.project.complexity == 'High' && $.project.teamSize >= 6 || $.project.budget >= 100000",
                                "then": "Medium Priority"
                            },
                            {
                                "if": "$.project.teamSize >= 5 && $.project.complexity == 'Medium'",
                                "then": "Standard Priority"
                            },
                            {
                                "else": true,
                                "then": "Low Priority"
                            }
                        ]
                    }
                ]
            }
            """;

            var transformer = new JsonTransformer();

            // Act
            var result = transformer.Transform(sourceJson, templateJson);

            // Assert
            var resultJson = JsonDocument.Parse(result);
            var projectPriority = resultJson.RootElement
                .GetProperty("result")
                .GetProperty("projectPriority")
                .GetString();

            Assert.Equal("High Priority", projectPriority);
        }
    }
}
