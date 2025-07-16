# Json.Transform Query Templates Reference

A comprehensive guide to all transformation patterns, query expressions, and template structures supported by the Json.Transform library.

## Table of Contents

1. [Template Structure](#template-structure)
2. [Basic Field Mappings](#basic-field-mappings)
3. [JSONPath Expressions](#jsonpath-expressions)
4. [Conditional Logic](#conditional-logic)
5. [Mathematical Operations](#mathematical-operations)
6. [Aggregation Operations](#aggregation-operations)
   - [Conditional Aggregation](#conditional-aggregation)
7. [String Operations](#string-operations)
8. [Constant Values](#constant-values)
9. [Complex Transformations](#complex-transformations)
10. [Advanced Patterns](#advanced-patterns)
11. [Error Handling](#error-handling)
12. [Performance Optimization](#performance-optimization)

## Template Structure

### Basic Template Format

```json
{
  "version": "1.0",
  "description": "Optional description of transformation",
  "settings": {
    "strictMode": false,
    "preserveNulls": true,
    "createPaths": true,
    "maxDepth": 10,
    "enableTracing": false
  },
  "mappings": [
    // Array of mapping rules
  ]
}
```

### Mapping Rule Structure

```json
{
  "from": "$.source.path",           // Source JSONPath (optional)
  "to": "$.target.path",             // Target JSONPath (required)
  "value": "constant_value",         // Constant value (optional)
  "aggregate": "operation",          // Aggregation operation (optional)
  "aggregation": { /* aggregation config */ }, // Conditional aggregation (optional)
  "math": { /* math config */ },     // Mathematical operation (optional)
  "concat": "template_string",       // String concatenation (optional)
  "conditions": [ /* conditions */ ], // Conditional logic (optional)
  "default": "fallback_value",       // Default value (optional)
  "enabled": true,                   // Enable/disable mapping (optional)
  "template": { /* nested template */ } // Nested transformation (optional)
}
```

## Basic Field Mappings

### Simple Field Copy

```json
{
  "mappings": [
    {
      "from": "$.user.name",
      "to": "$.customer.fullName"
    }
  ]
}
```

### Multiple Field Mappings

```json
{
  "mappings": [
    {
      "from": "$.user.firstName",
      "to": "$.profile.name.first"
    },
    {
      "from": "$.user.lastName",
      "to": "$.profile.name.last"
    },
    {
      "from": "$.user.email",
      "to": "$.profile.contact.email"
    }
  ]
}
```

### Nested Object Creation

```json
{
  "mappings": [
    {
      "from": "$.user.address.street",
      "to": "$.customer.billing.address.street"
    },
    {
      "from": "$.user.address.city",
      "to": "$.customer.billing.address.city"
    }
  ]
}
```

## JSONPath Expressions

### Basic Path Patterns

| Pattern | Description | Example |
|---------|-------------|---------|
| `$.field` | Root level field | `$.name` |
| `$.object.field` | Nested field | `$.user.email` |
| `$.array[0]` | Array element by index | `$.orders[0]` |
| `$.array[*]` | All array elements | `$.orders[*]` |
| `$.array[*].field` | Field from all array elements | `$.orders[*].total` |
| `$..field` | Recursive descent | `$..email` |

### Advanced Path Expressions

```json
{
  "mappings": [
    {
      "from": "$.orders[?(@.status == 'completed')].total",
      "to": "$.summary.completedOrdersTotal",
      "aggregate": "sum"
    },
    {
      "from": "$.products[?(@.price > 100)]",
      "to": "$.expensiveProducts"
    }
  ]
}
```

## Conditional Logic

### Simple Conditions

```json
{
  "mappings": [
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
    }
  ]
}
```

### Multiple Conditions (If-ElseIf-Else)

```json
{
  "mappings": [
    {
      "from": "$.user.age",
      "to": "$.customer.ageGroup",
      "conditions": [
        {
          "if": "$.user.age < 18",
          "then": "Minor"
        },
        {
          "if": "$.user.age >= 18 && $.user.age < 65",
          "then": "Adult"
        },
        {
          "if": "$.user.age >= 65",
          "then": "Senior"
        }
      ]
    }
  ]
}
```

### Complex Boolean Logic

#### AND Conditions

```json
{
  "conditions": [
    {
      "if": "$.employee.yearsOfExperience >= 3 && $.employee.performanceScore >= 9.0",
      "then": "Promotion Eligible"
    }
  ]
}
```

#### OR Conditions

```json
{
  "conditions": [
    {
      "if": "$.employee.department == 'Engineering' || $.employee.department == 'Sales'",
      "then": "Core Team"
    }
  ]
}
```

#### Mixed AND/OR with Parentheses

```json
{
  "conditions": [
    {
      "if": "$.user.age >= 30 && $.user.isManager == true && ($.user.department == 'Engineering' || $.user.department == 'Product')",
      "then": "Executive Track Candidate"
    }
  ]
}
```

### Comparison Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `==` | Equal | `$.status == 'active'` |
| `!=` | Not equal | `$.status != 'inactive'` |
| `>` | Greater than | `$.age > 18` |
| `>=` | Greater than or equal | `$.score >= 8.0` |
| `<` | Less than | `$.price < 100` |
| `<=` | Less than or equal | `$.discount <= 0.2` |
| `contains` | String contains | `$.name contains 'John'` |
| `startsWith` | String starts with | `$.email startsWith 'admin'` |
| `endsWith` | String ends with | `$.file endsWith '.pdf'` |

### Logical Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `&&` | AND | `$.age >= 18 && $.verified == true` |
| `\|\|` | OR | `$.type == 'premium' \|\| $.score > 90` |
| `()` | Grouping | `($.a == 1 \|\| $.b == 2) && $.c == 3` |

## Mathematical Operations

### Basic Math Operations

```json
{
  "mappings": [
    {
      "to": "$.order.total",
      "math": {
        "operation": "add",
        "operands": ["$.order.subtotal", "$.order.tax", "$.order.shipping"]
      }
    }
  ]
}
```

### Supported Math Operations

| Operation | Description | Operands | Example |
|-----------|-------------|----------|---------|
| `add` / `+` | Addition | 2+ | `[100, 25, 8.5]` |
| `subtract` / `-` | Subtraction | 2+ | `[100, 15]` |
| `multiply` / `*` | Multiplication | 2+ | `[25, 1.08]` |
| `divide` / `/` | Division | 2+ | `[100, 4]` |
| `power` / `^` | Exponentiation | 2 | `[2, 3]` |
| `sqrt` | Square root | 1 | `[16]` |
| `abs` | Absolute value | 1 | `[-5]` |
| `round` | Round to precision | 1 | `[3.14159]` |
| `ceil` | Ceiling | 1 | `[3.2]` |
| `floor` | Floor | 1 | `[3.8]` |
| `mod` / `%` | Modulo | 2 | `[10, 3]` |

### Complex Math Examples

```json
{
  "mappings": [
    {
      "to": "$.pricing.discountedPrice",
      "math": {
        "operation": "multiply",
        "operands": [
          "$.product.price",
          {
            "operation": "subtract",
            "operands": [1, "$.discount.percentage"]
          }
        ]
      }
    },
    {
      "to": "$.analytics.compound",
      "math": {
        "operation": "power",
        "operands": [
          {
            "operation": "add",
            "operands": [1, "$.rate"]
          },
          "$.periods"
        ]
      }
    }
  ]
}
```

### Math with Precision

```json
{
  "to": "$.result.rounded",
  "math": {
    "operation": "round",
    "operands": ["$.value"],
    "precision": 2
  }
}
```

## Aggregation Operations

### Basic Aggregations

```json
{
  "mappings": [
    {
      "from": "$.orders[*].total",
      "to": "$.summary.totalRevenue",
      "aggregate": "sum"
    },
    {
      "from": "$.orders[*].total",
      "to": "$.summary.averageOrder",
      "aggregate": "avg"
    },
    {
      "from": "$.orders",
      "to": "$.summary.orderCount",
      "aggregate": "count"
    }
  ]
}
```

### Supported Aggregation Operations

| Operation | Description | Example |
|-----------|-------------|---------|
| `sum` | Sum of numeric values | Total revenue |
| `avg` / `average` | Average of numeric values | Average order value |
| `min` | Minimum value | Lowest price |
| `max` | Maximum value | Highest score |
| `count` | Count of items | Number of orders |
| `first` | First item | First order |
| `last` | Last item | Most recent order |
| `join` | Join strings | Comma-separated list |

### Advanced Aggregation Examples

```json
{
  "mappings": [
    {
      "from": "$.products[*].categories[*]",
      "to": "$.summary.allCategories",
      "aggregate": "join"
    },
    {
      "from": "$.employees[*].skills[*]",
      "to": "$.company.skillsList",
      "aggregate": "join"
    },
    {
      "from": "$.transactions[?(@.status == 'completed')].amount",
      "to": "$.summary.completedTotal",
      "aggregate": "sum"
    }
  ]
}
```

### Conditional Aggregation

Filter array elements before performing aggregation operations using the new `aggregation` property:

```json
{
  "mappings": [
    {
      "to": "$.summary.highValueTransactions",
      "from": "$.transactions[*]",
      "aggregation": {
        "type": "sum",
        "field": "amount",
        "condition": "$.item.amount > 100"
      }
    },
    {
      "to": "$.summary.premiumCustomerCount",
      "from": "$.customers[*]",
      "aggregation": {
        "type": "count",
        "condition": "$.item.type == 'premium' && $.item.status == 'active'"
      }
    },
    {
      "to": "$.analytics.averageHighValueOrder",
      "from": "$.orders[*]",
      "aggregation": {
        "type": "avg",
        "field": "total",
        "condition": "$.item.total >= 500 && $.item.status == 'completed'"
      }
    }
  ]
}
```

#### Conditional Aggregation Properties

| Property | Required | Description | Example |
|----------|----------|-------------|---------|
| `type` | Yes | Aggregation operation | `"sum"`, `"count"`, `"avg"`, `"min"`, `"max"` |
| `field` | No | Field to aggregate (omit for count) | `"amount"`, `"total"`, `"score"` |
| `condition` | Yes | Boolean expression to filter elements | `"$.item.amount > 100"` |

#### Advanced Conditional Aggregation Examples

```json
{
  "mappings": [
    {
      "to": "$.report.completedHighPriorityRevenue",
      "from": "$.orders[*]",
      "aggregation": {
        "type": "sum",
        "field": "revenue",
        "condition": "$.item.status == 'completed' && $.item.priority == 'high' && $.item.revenue > 1000"
      }
    },
    {
      "to": "$.metrics.eligibleEmployeeCount",
      "from": "$.employees[*]",
      "aggregation": {
        "type": "count",
        "condition": "$.item.yearsOfExperience >= 3 && $.item.performanceScore >= 8.0 && ($.item.department == 'Engineering' || $.item.department == 'Product')"
      }
    },
    {
      "to": "$.dashboard.averageCompletedProjectDuration",
      "from": "$.projects[*]",
      "aggregation": {
        "type": "avg",
        "field": "durationDays",
        "condition": "$.item.status == 'completed' && $.item.durationDays != null"
      }
    }
  ]
}
```

#### Condition Syntax

Conditional aggregation supports the same operators as conditional logic:

- **Comparison**: `>=`, `<=`, `==`, `!=`, `>`, `<`
- **String**: `contains`, `startsWith`, `endsWith`
- **Logical**: `&&` (AND), `||` (OR)
- **Grouping**: `()` for complex expressions
- **Context**: Use `$.item.field` to reference the current array element being filtered

## String Operations

### String Concatenation

```json
{
  "mappings": [
    {
      "to": "$.customer.displayName",
      "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
    }
  ]
}
```

### Complex String Templates

```json
{
  "mappings": [
    {
      "to": "$.notification.message",
      "concat": "Hello {$.user.name}, your order #{$.order.id} for ${$.order.total} has been {$.order.status}."
    },
    {
      "to": "$.report.summary",
      "concat": "Report generated on {now} for {$.report.period} with {$.data.recordCount} records."
    }
  ]
}
```

### String with Conditional Logic

```json
{
  "mappings": [
    {
      "to": "$.greeting.message",
      "conditions": [
        {
          "if": "$.user.isPremium == true",
          "then": {
            "concat": "Welcome back, Premium Member {$.user.name}!"
          }
        },
        {
          "else": true,
          "then": {
            "concat": "Hello {$.user.name}, consider upgrading to Premium!"
          }
        }
      ]
    }
  ]
}
```

## Constant Values

### Static Constants

```json
{
  "mappings": [
    {
      "to": "$.metadata.version",
      "value": "1.0"
    },
    {
      "to": "$.config.environment",
      "value": "production"
    }
  ]
}
```

### Dynamic Constants

| Constant | Description | Output |
|----------|-------------|--------|
| `now` | Current timestamp | `2024-01-15T10:30:00Z` |
| `utcnow` | UTC timestamp | `2024-01-15T10:30:00Z` |
| `guid` / `newguid` | New GUID | `550e8400-e29b-41d4-a716-446655440000` |
| `timestamp` | Unix timestamp | `1705315800` |
| `true` | Boolean true | `true` |
| `false` | Boolean false | `false` |
| `null` | Null value | `null` |

```json
{
  "mappings": [
    {
      "to": "$.audit.createdAt",
      "value": "now"
    },
    {
      "to": "$.record.id",
      "value": "guid"
    },
    {
      "to": "$.flags.isActive",
      "value": true
    }
  ]
}
```

## Complex Transformations

### Nested Templates

```json
{
  "mappings": [
    {
      "from": "$.orders",
      "to": "$.processedOrders",
      "template": {
        "mappings": [
          {
            "from": "$.id",
            "to": "$.orderId"
          },
          {
            "from": "$.total",
            "to": "$.amount"
          },
          {
            "to": "$.processedAt",
            "value": "now"
          }
        ]
      }
    }
  ]
}
```

### Multi-Step Transformations

```json
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
      "to": "$.profile.demographics.ageGroup",
      "conditions": [
        {
          "if": "$.customer.age < 25",
          "then": "Young Adult"
        },
        {
          "if": "$.customer.age >= 25 && $.customer.age < 65",
          "then": "Adult"
        },
        {
          "else": true,
          "then": "Senior"
        }
      ]
    },
    {
      "from": "$.orders[*].total",
      "to": "$.profile.spending.totalAmount",
      "aggregate": "sum"
    },
    {
      "from": "$.orders",
      "to": "$.profile.spending.orderCount",
      "aggregate": "count"
    },
    {
      "to": "$.profile.spending.averageOrder",
      "math": {
        "operation": "divide",
        "operands": [
          "$.profile.spending.totalAmount",
          "$.profile.spending.orderCount"
        ]
      }
    },
    {
      "to": "$.metadata.processedAt",
      "value": "now"
    },
    {
      "to": "$.metadata.version",
      "value": "2.0"
    }
  ]
}
```

## Advanced Patterns

### Conditional Field Creation

```json
{
  "mappings": [
    {
      "to": "$.customer.loyaltyTier",
      "conditions": [
        {
          "if": "$.orders[*].total sum >= 10000",
          "then": "Platinum"
        },
        {
          "if": "$.orders[*].total sum >= 5000",
          "then": "Gold"
        },
        {
          "if": "$.orders[*].total sum >= 1000",
          "then": "Silver"
        },
        {
          "else": true,
          "then": "Bronze"
        }
      ]
    }
  ]
}
```

### Dynamic Field Names

```json
{
  "mappings": [
    {
      "from": "$.user.preferences.theme",
      "to": "$.settings['{$.user.id}'].theme"
    }
  ]
}
```

### Array Transformations

```json
{
  "mappings": [
    {
      "from": "$.products",
      "to": "$.catalog.items",
      "template": {
        "mappings": [
          {
            "from": "$.name",
            "to": "$.title"
          },
          {
            "from": "$.price",
            "to": "$.cost"
          },
          {
            "to": "$.discountedPrice",
            "math": {
              "operation": "multiply",
              "operands": ["$.price", 0.9]
            }
          }
        ]
      }
    }
  ]
}
```

### Conditional Array Processing

```json
{
  "mappings": [
    {
      "from": "$.orders[?(@.status == 'completed')]",
      "to": "$.completedOrders",
      "template": {
        "mappings": [
          {
            "from": "$.id",
            "to": "$.orderId"
          },
          {
            "from": "$.total",
            "to": "$.amount"
          },
          {
            "to": "$.completedAt",
            "value": "now"
          }
        ]
      }
    }
  ]
}
```

## Error Handling

### Default Values

```json
{
  "mappings": [
    {
      "from": "$.user.phone",
      "to": "$.contact.phone",
      "default": "Not provided"
    }
  ]
}
```

### Conditional Defaults

```json
{
  "mappings": [
    {
      "from": "$.user.email",
      "to": "$.contact.email",
      "conditions": [
        {
          "if": "$.user.email != null && $.user.email != ''",
          "then": "$.user.email"
        }
      ],
      "default": "no-email@example.com"
    }
  ]
}
```

### Strict Mode Handling

```json
{
  "settings": {
    "strictMode": true,
    "preserveNulls": false
  },
  "mappings": [
    {
      "from": "$.user.optionalField",
      "to": "$.result.field",
      "default": "N/A"
    }
  ]
}
```

## Performance Optimization

### Efficient Path Resolution

```json
{
  "mappings": [
    // Prefer specific paths over recursive descent
    {
      "from": "$.user.profile.name",
      "to": "$.customer.name"
    }
    // Instead of: "from": "$..name"
  ]
}
```

### Batch Operations

```json
{
  "mappings": [
    // Group related transformations
    {
      "from": "$.user",
      "to": "$.customer",
      "template": {
        "mappings": [
          {"from": "$.name", "to": "$.fullName"},
          {"from": "$.email", "to": "$.contactEmail"},
          {"from": "$.phone", "to": "$.contactPhone"}
        ]
      }
    }
  ]
}
```

### Conditional Processing

```json
{
  "mappings": [
    {
      "enabled": true,
      "from": "$.expensiveOperation",
      "to": "$.result",
      "conditions": [
        {
          "if": "$.shouldProcess == true",
          "then": "$.expensiveOperation"
        }
      ]
    }
  ]
}
```

## Template Validation

### Required Fields Validation

```json
{
  "mappings": [
    {
      "from": "$.user.email",
      "to": "$.customer.email",
      "conditions": [
        {
          "if": "$.user.email != null && $.user.email != ''",
          "then": "$.user.email",
          "else": {
            "error": "Email is required"
          }
        }
      ]
    }
  ]
}
```

### Data Type Validation

```json
{
  "mappings": [
    {
      "from": "$.user.age",
      "to": "$.customer.age",
      "conditions": [
        {
          "if": "$.user.age >= 0 && $.user.age <= 150",
          "then": "$.user.age",
          "else": 0
        }
      ]
    }
  ]
}
```

## Common Use Cases

### E-commerce Order Processing

```json
{
  "description": "Transform order data for processing",
  "mappings": [
    {
      "from": "$.order.customer.name",
      "to": "$.processedOrder.customerName"
    },
    {
      "from": "$.order.items[*].price",
      "to": "$.processedOrder.subtotal",
      "aggregate": "sum"
    },
    {
      "to": "$.processedOrder.tax",
      "math": {
        "operation": "multiply",
        "operands": ["$.processedOrder.subtotal", 0.08]
      }
    },
    {
      "to": "$.processedOrder.total",
      "math": {
        "operation": "add",
        "operands": ["$.processedOrder.subtotal", "$.processedOrder.tax"]
      }
    },
    {
      "to": "$.processedOrder.status",
      "conditions": [
        {
          "if": "$.order.paymentStatus == 'paid'",
          "then": "confirmed"
        },
        {
          "else": true,
          "then": "pending"
        }
      ]
    }
  ]
}
```

### User Profile Transformation

```json
{
  "description": "Transform user data for profile display",
  "mappings": [
    {
      "to": "$.profile.displayName",
      "concat": "{$.user.firstName} {$.user.lastName}"
    },
    {
      "from": "$.user.birthDate",
      "to": "$.profile.ageGroup",
      "conditions": [
        {
          "if": "$.user.age < 18",
          "then": "Minor"
        },
        {
          "if": "$.user.age >= 18 && $.user.age < 65",
          "then": "Adult"
        },
        {
          "else": true,
          "then": "Senior"
        }
      ]
    },
    {
      "from": "$.user.orders[*].total",
      "to": "$.profile.totalSpent",
      "aggregate": "sum"
    },
    {
      "to": "$.profile.membershipLevel",
      "conditions": [
        {
          "if": "$.profile.totalSpent >= 10000",
          "then": "Platinum"
        },
        {
          "if": "$.profile.totalSpent >= 5000",
          "then": "Gold"
        },
        {
          "if": "$.profile.totalSpent >= 1000",
          "then": "Silver"
        },
        {
          "else": true,
          "then": "Bronze"
        }
      ]
    }
  ]
}
```

### Analytics Data Aggregation

```json
{
  "description": "Aggregate analytics data for reporting",
  "mappings": [
    {
      "from": "$.events[*].revenue",
      "to": "$.analytics.totalRevenue",
      "aggregate": "sum"
    },
    {
      "from": "$.events[*].users",
      "to": "$.analytics.totalUsers",
      "aggregate": "sum"
    },
    {
      "from": "$.events",
      "to": "$.analytics.eventCount",
      "aggregate": "count"
    },
    {
      "to": "$.analytics.averageRevenuePerEvent",
      "math": {
        "operation": "divide",
        "operands": [
          "$.analytics.totalRevenue",
          "$.analytics.eventCount"
        ]
      }
    },
    {
      "to": "$.analytics.conversionRate",
      "math": {
        "operation": "divide",
        "operands": [
          "$.events[?(@.type == 'conversion')] count",
          "$.analytics.totalUsers"
        ]
      }
    }
  ]
}
```

This comprehensive reference covers all the major transformation patterns and query expressions supported by the Json.Transform library. Use these templates as starting points for your own transformations, combining and modifying them as needed for your specific use cases.