
# JSQLAnalyzer (.NET 9 Library)

## 📘 Overview
**JSQLAnalyzer** is a .NET 9 library for analyzing structured, aligned JSQL queries and converting them into JSON-based **Transform Template** definitions. These templates can be used in ETL pipelines, validation engines, or integration middleware to standardize and automate data transformations.

## 🎯 Goals
- ✅ Parse aligned JSQL syntax (as per user's formatting rules)
- ✅ Extract mappings, CASE logic, math expressions, and aggregations
- ✅ Generate valid JSON transformation templates
- ✅ Support array iteration with `item` keyword
- ✅ Match MSSQL, Oracle, and DB2 string and math conventions

## ✅ Supported Features
| Feature                | Description                                                  |
|------------------------|--------------------------------------------------------------|
| Field Mapping          | `user.name AS 'result.name'`                                 |
| CASE Conditions        | Maps to conditional logic in transformation template         |
| Aggregations           | `SUM`, `COUNT`, `AVG` with conditions                        |
| Math Expressions       | Supports `+`, `-`, `*`, `/` with nested expressions          |
| String Concatenation   | Uses `CONCAT(...)` or ANSI `||` style                        |
| Constant Values        | Literal strings, `CURRENT_TIMESTAMP`                         |
| Array Element Handling | Uses `item` for looping unknown-named objects               |

## 🔧 Implementation Hints
- Build SQL AST from aligned JSQL using a parser (ANTLR or TSQL parser)
- Convert AST to intermediate operation graph (mappings, logic, etc.)
- Flatten graph into `TransformTemplate` JSON format

## 🧪 Examples (Input → Template → JSQL)

### Example 1: Age-based condition
#### Input JSON
```json
{ "user": { "name": "Alice Smith", "age": 17 } }
```
#### Template
```json
{
  "mappings": [
    { "from": "$.user.name", "to": "$.result.name" },
    {
      "from": "$.user.age",
      "to": "$.result.status",
      "conditions": [
        { "if": "$.user.age >= 18", "then": "Adult", "else": "Minor" }
      ]
    }
  ]
}
```
#### JSQL
```sql
SELECT
  user.name AS 'result.name',
  CASE
    WHEN user.age >= 18 THEN 'Adult'
    ELSE 'Minor'
  END AS 'result.status'
FROM user;
```

### Example 2: Full Name Concatenation
#### Input JSON
```json
{ "user": { "firstName": "Bob", "lastName": "Johnson", "title": "Mr." } }
```
#### Template
```json
{
  "mappings": [
    {
      "to": "$.user.displayName",
      "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
    }
  ]
}
```
#### JSQL
```sql
SELECT
  CONCAT(user.title, ' ', user.firstName, ' ', user.lastName) AS 'user.displayName'
FROM user;
```

### Example 3: Aggregated Order Analysis (SUM/COUNT/AVG)
#### Input JSON
```json
{
  "orders": [
    { "amount": 50.5, "status": "completed", "priority": "low" },
    { "amount": 150, "status": "completed", "priority": "high" },
    { "amount": 75, "status": "pending", "priority": "medium" },
    { "amount": 200, "status": "completed", "priority": "high" },
    { "amount": 125, "status": "completed", "priority": "medium" }
  ]
}
```
#### Template
```json
{
  "mappings": [
    {
      "to": "totalHighPriorityCompletedOrders",
      "from": "$.orders[*]",
      "aggregation": {
        "type": "sum",
        "field": "amount",
        "condition": "$.item.status == 'completed' && $.item.priority == 'high' && $.item.amount > 100"
      }
    },
    {
      "to": "completedOrdersOverTarget",
      "from": "$.orders[*]",
      "aggregation": {
        "type": "count",
        "condition": "$.item.status == 'completed' && $.item.amount >= 100"
      }
    },
    {
      "to": "averageCompletedOrderValue",
      "from": "$.orders[*]",
      "aggregation": {
        "type": "avg",
        "field": "amount",
        "condition": "$.item.status == 'completed'"
      }
    }
  ]
}
```
#### JSQL
```sql
SELECT
  SUM(CASE WHEN item.status = 'completed' AND item.priority = 'high' AND item.amount > 100 THEN item.amount ELSE 0 END) AS 'totalHighPriorityCompletedOrders',
  COUNT(CASE WHEN item.status = 'completed' AND item.amount >= 100 THEN 1 ELSE NULL END) AS 'completedOrdersOverTarget',
  AVG(CASE WHEN item.status = 'completed' THEN item.amount ELSE NULL END) AS 'averageCompletedOrderValue'
FROM orders;
```

### Example 4: Math Calculation (Subtotal & Tax)
#### Input JSON
```json
{ "product": { "price": 100, "taxRate": 0.08, "discount": 10 } }
```
#### Template
```json
{
  "mappings": [
    {
      "to": "$.result.subtotal",
      "math": { "operation": "subtract", "operands": ["$.product.price", "$.product.discount"] }
    },
    {
      "to": "$.result.tax",
      "math": { "operation": "multiply", "operands": ["$.result.subtotal", "$.product.taxRate"] }
    }
  ]
}
```
#### JSQL
```sql
SELECT
  product.price - product.discount AS 'result.subtotal',
  (product.price - product.discount) * product.taxRate AS 'result.tax'
FROM product;
```

### Example 5: High-Value Transaction Filters
#### Input JSON
```json
{
  "transactions": [
    { "amount": 50.5, "type": "expense" },
    { "amount": 150, "type": "income" },
    { "amount": 75, "type": "expense" },
    { "amount": 200, "type": "income" },
    { "amount": 25, "type": "expense" }
  ]
}
```
#### Template
```json
{
  "mappings": [
    {
      "to": "totalHighValueTransactions",
      "from": "$.transactions[*]",
      "aggregation": {
        "type": "sum",
        "field": "amount",
        "condition": "$.item.amount > 100"
      }
    },
    {
      "to": "highValueTransactionCount",
      "from": "$.transactions[*]",
      "aggregation": {
        "type": "count",
        "condition": "$.item.amount > 100"
      }
    }
  ]
}
```
#### JSQL
```sql
SELECT
  SUM(CASE WHEN item.amount > 100 THEN item.amount ELSE 0 END) AS 'totalHighValueTransactions',
  COUNT(CASE WHEN item.amount > 100 THEN 1 ELSE NULL END) AS 'highValueTransactionCount'
FROM transactions;
```

### Example 6: Complex Employee Access Level & Badge Formatting
(... Additional examples continue below ...)

### Example 6: Complex Employee Access Level & Badge Formatting
#### Input JSON
```json
{
  "employees": [
    {
      "name": "Alice Admin",
      "email": "alice.admin@company.com",
      "department": "IT",
      "files": ["report.pdf", "data.xlsx"]
    },
    {
      "name": "Bob Support",
      "email": "bob.support@company.com",
      "department": "Customer Service",
      "files": ["guide.pdf", "help.docx"]
    },
    {
      "name": "Charlie Dev",
      "email": "charlie@external.com",
      "department": "Engineering",
      "files": ["code.js", "README.md"]
    }
  ]
}
```
#### Template
```json
{
  "mappings": [
    {
      "from": "$.employees[*]",
      "to": "processedEmployees",
      "template": {
        "mappings": [
          { "from": "$.name", "to": "name" },
          {
            "from": "$.email",
            "to": "accessLevel",
            "conditions": [
              { "if": "$.email contains 'admin' || $.email startsWith 'alice'", "then": "Administrator" },
              { "if": "$.email contains 'support' && $.department == 'Customer Service'", "then": "Support Agent" },
              { "if": "$.email endsWith '@company.com'", "then": "Employee" },
              { "else": true, "then": "External" }
            ]
          },
          {
            "to": "badge",
            "concat": "{$.name} - {$.accessLevel} ({$.department})"
          },
          {
            "to": "pdfFileCount",
            "from": "$.files[*]",
            "aggregation": {
              "type": "count",
              "condition": "$.item endsWith '.pdf'"
            }
          }
        ]
      }
    }
  ]
}
```
#### JSQL
```sql
SELECT
  e.name AS 'processedEmployees.name',
  CASE
    WHEN CHARINDEX('admin', e.email) > 0 OR LEFT(e.email, 5) = 'alice' THEN 'Administrator'
    WHEN CHARINDEX('support', e.email) > 0 AND e.department = 'Customer Service' THEN 'Support Agent'
    WHEN RIGHT(e.email, 12) = '@company.com' THEN 'Employee'
    ELSE 'External'
  END AS 'processedEmployees.accessLevel',
  e.name || ' - ' ||
  CASE
    WHEN CHARINDEX('admin', e.email) > 0 OR LEFT(e.email, 5) = 'alice' THEN 'Administrator'
    WHEN CHARINDEX('support', e.email) > 0 AND e.department = 'Customer Service' THEN 'Support Agent'
    WHEN RIGHT(e.email, 12) = '@company.com' THEN 'Employee'
    ELSE 'External'
  END || ' (' || e.department || ')' AS 'processedEmployees.badge',
  (
    SELECT COUNT(*) FROM TABLE(e.files) f WHERE RIGHT(f, 4) = '.pdf'
  ) AS 'processedEmployees.pdfFileCount'
FROM employees e;
```

### Example 7: Summary Report With Metadata, Counts, and Timestamp
#### JSQL
```sql
SELECT
  metadata.company || ' Employee Report - ' || metadata.generated AS 'summary.reportTitle',
  (
    SELECT COUNT(*) FROM employees emp WHERE RIGHT(emp.email, 12) = '@company.com'
  ) AS 'summary.companyEmployeeCount',
  (
    SELECT COUNT(*) FROM employees emp WHERE CHARINDEX('admin', emp.email) > 0
  ) AS 'summary.adminCount',
  (
    SELECT COUNT(*) FROM employees emp WHERE RIGHT(emp.email, 12) != '@company.com'
  ) AS 'summary.externalCount'
FROM metadata;
```

### Example 8: Customer Profile and Order Summary
#### Template
```json
{
  "mappings": [
    { "from": "$.customer.name", "to": "$.profile.displayName" },
    { "from": "$.customer.email", "to": "$.profile.contact.email" },
    {
      "from": "$.customer.age",
      "to": "$.profile.ageGroup",
      "conditions": [
        { "if": "$.customer.age >= 18", "then": "Adult", "else": "Minor" }
      ]
    },
    {
      "from": "$.orders[*].total",
      "to": "$.orderSummary.totalSpent",
      "aggregate": "sum"
    },
    {
      "from": "$.orders",
      "to": "$.orderSummary.orderCount",
      "aggregate": "count"
    }
  ]
}
```
#### JSQL
```sql
SELECT
  customer.name AS 'profile.displayName',
  customer.email AS 'profile.contact.email',
  CASE
    WHEN customer.age >= 18 THEN 'Adult'
    ELSE 'Minor'
  END AS 'profile.ageGroup',
  SUM(item.total) AS 'orderSummary.totalSpent',
  COUNT(*) AS 'orderSummary.orderCount'
FROM customer, orders;
```

### Example 9: Static Version and Timestamp
#### Template
```json
{
  "mappings": [
    { "to": "$.metadata.generatedAt", "value": "now" },
    { "to": "$.metadata.version", "value": "1.0" }
  ]
}
```
#### JSQL
```sql
SELECT
  CURRENT_TIMESTAMP AS 'metadata.generatedAt',
  '1.0' AS 'metadata.version'
FROM metadata;
```
