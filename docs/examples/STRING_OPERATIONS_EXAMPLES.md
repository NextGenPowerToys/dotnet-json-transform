# String Operations Examples

This document provides comprehensive examples of string operations in Json.Transform, including string concatenation templates and string comparison operators.

## String Concatenation

### Basic Template Concatenation

Combine multiple fields into a single string using template syntax with `{$.path}` placeholders:

```json
{
  "mappings": [
    {
      "to": "fullName",
      "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
    }
  ]
}
```

**Input:**
```json
{
  "user": {
    "firstName": "John",
    "lastName": "Doe", 
    "title": "Mr."
  }
}
```

**Output:**
```json
{
  "fullName": "Mr. John Doe"
}
```

### Complex Template with Multiple Data Types

Templates can include strings, numbers, and other data types:

```json
{
  "mappings": [
    {
      "to": "orderSummary",
      "concat": "Order {$.order.id} for customer {$.order.customer} with total ${$.order.total} has been {$.order.status}."
    }
  ]
}
```

**Input:**
```json
{
  "order": {
    "id": "ORD-12345",
    "customer": "Alice Smith",
    "total": 299.99,
    "status": "shipped"
  }
}
```

**Output:**
```json
{
  "orderSummary": "Order ORD-12345 for customer Alice Smith with total $299.99 has been shipped."
}
```

### Dynamic Constants in Templates

Use special constants like `{now}` for timestamps:

```json
{
  "mappings": [
    {
      "to": "reportHeader",
      "concat": "Report: {$.report.name} for {$.report.period} generated at {now}"
    }
  ]
}
```

**Input:**
```json
{
  "report": {
    "name": "Monthly Sales",
    "period": "July 2025"
  }
}
```

**Output:**
```json
{
  "reportHeader": "Report: Monthly Sales for July 2025 generated at 2025-01-22T10:30:00.000Z"
}
```

## String Comparison Operators

### Contains Operator

Filter data based on whether a string contains a substring (case-insensitive):

```json
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
```

**Input:**
```json
{
  "users": [
    { "name": "John Smith", "email": "john@company.com" },
    { "name": "Admin User", "email": "admin@company.com" },
    { "name": "Jane Doe", "email": "jane@external.com" }
  ]
}
```

**Output:** (Only processes users with company email)

### StartsWith Operator  

Identify items based on string prefixes:

```json
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
```

**Input:**
```json
{
  "user": {
    "email": "admin@company.com",
    "name": "System Administrator"
  }
}
```

**Output:**
```json
{
  "userType": "Administrator"
}
```

### EndsWith Operator

Filter based on string suffixes, useful for file extensions:

```json
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
```

**Input:**
```json
{
  "files": [
    { "name": "document.pdf", "size": 1024 },
    { "name": "image.jpg", "size": 2048 },
    { "name": "report.pdf", "size": 512 }
  ]
}
```

**Output:**
```json
{
  "pdfFileCount": 2
}
```

## Advanced String Operations

### Conditional String Building

Build different strings based on complex conditions:

```json
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
```

### Case-Insensitive String Operations

All string comparison operators work case-insensitively:

```json
{
  "mappings": [
    {
      "to": "documentCount",
      "from": "$.items[*]",
      "aggregation": {
        "type": "count",
        "condition": "$.item.name endsWith '.pdf' || $.item.name endsWith '.doc'"
      }
    }
  ]
}
```

This will match "Document.PDF", "file.Doc", etc.

### Complex String Logic

Combine multiple string operations with boolean logic:

```json
{
  "mappings": [
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
    }
  ]
}
```

## Handling Edge Cases

### Null and Missing Values

String templates handle null and missing values gracefully by treating them as empty strings:

```json
{
  "mappings": [
    {
      "to": "displayName",
      "concat": "{$.user.title} {$.user.firstName} {$.user.lastName} {$.user.middleName}"
    }
  ]
}
```

**Input:**
```json
{
  "user": {
    "firstName": "John",
    "lastName": null,
    "title": "Mr."
  }
}
```

**Output:**
```json
{
  "displayName": "Mr. John  "
}
```

## Best Practices

1. **Use Descriptive Template Variables**: Make template placeholders clear and readable
2. **Handle Missing Data**: Consider null/missing values in your templates  
3. **Case-Insensitive Comparisons**: Remember that string operators are case-insensitive
4. **Combine with Conditional Logic**: Use string operations with conditions for powerful data transformation
5. **Order Matters**: Concat operations should reference source data paths, not target paths within the same transformation

## Performance Considerations

- String operations are performed in-memory and are generally fast
- Complex regex patterns in string comparisons may impact performance
- For large datasets, consider the order of conditions (put most common matches first)
