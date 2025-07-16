# Interactive JSON Transform Playground

The **JSON Transform Playground** is a modern, web-based interface that provides real-time JSON transformation testing and experimentation capabilities. It offers a complete interactive environment for learning and testing the Json.Transform library.

## 🚀 Quick Start

### Launch the Playground

```bash
cd examples
dotnet run -- --api --port 5260
# Open browser to: http://localhost:5260/playground
```

**Alternative ports:**
```bash
# If port 5260 is in use, try:
dotnet run -- --api --port 8080
dotnet run -- --api --port 3000
```

## ✨ Features Overview

### 🎨 Modern User Interface
- **Clean Design**: Professional blue-gray color scheme with excellent contrast
- **Responsive Layout**: Optimized for desktop, tablet, and mobile devices  
- **Glass Morphism**: Modern visual effects with backdrop blur and transparency
- **Compact Header**: Minimal header design for maximum workspace
- **Visual Hierarchy**: Clear separation between components and sections

### 📝 Dual Editor System
- **Source JSON Editor**: Left panel for input data with syntax highlighting
- **Transform Template Editor**: Right panel for transformation rules
- **Real-time Updates**: Changes are immediately reflected when transforming
- **Syntax Validation**: JSON parsing errors are highlighted inline
- **Auto-resize**: Editors automatically adjust to content size

### ⚡ Interactive Controls
- **🚀 Transform Button**: Execute transformations with visual feedback
- **📚 Example Library**: Nine pre-loaded transformation scenarios
- **🔧 Format JSON**: Beautify and format JSON with proper indentation
- **🗑️ Clear All**: Reset both editors to start fresh
- **📋 Copy Results**: One-click copying of transformation output

### 🎯 Real-time Transformation
- **Instant Results**: Transformations execute immediately on button click
- **Live API Integration**: Direct communication with the Json.Transform API
- **Error Handling**: Clear error messages with detailed explanations
- **Success Indicators**: Visual confirmation when transformations succeed
- **Performance Metrics**: Execution time display for performance monitoring

## 📚 Example Scenarios

The playground includes nine comprehensive examples that demonstrate all major features:

### 1. 🔄 Simple Field Mapping
**Purpose**: Basic property copying and renaming
```json
// Input
{
  "user": {
    "name": "John Doe",
    "age": 25,
    "email": "john@example.com"
  }
}

// Template
{
  "mappings": [
    { "from": "$.user.name", "to": "$.customer.fullName" },
    { "from": "$.user.email", "to": "$.customer.contact.email" }
  ]
}
```

### 2. 🎯 Conditional Logic
**Purpose**: Age-based status assignment with if/else conditions
```json
// Template
{
  "mappings": [
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
```

### 3. 🎯 Multi-Condition Logic
**Purpose**: Complex boolean expressions with AND/OR operators
```json
// Template
{
  "mappings": [
    {
      "to": "$.result.eligibleForPromotion",
      "conditions": [
        {
          "if": "$.employee.yearsOfExperience >= 3 && $.employee.performanceScore >= 9.0",
          "then": "Yes - Meets Experience and Performance Requirements"
        }
      ]
    }
  ]
}
```

### 4. 🔗 String Operations
**Purpose**: Template concatenation and string comparison operators
```json
// Template - Concatenation
{
  "mappings": [
    {
      "to": "$.user.displayName",
      "concat": "{$.user.title} {$.user.firstName} {$.user.lastName}"
    },
    {
      "from": "$.user.email", 
      "to": "$.user.accountType",
      "conditions": [
        {
          "if": "$.user.email contains '@company.com'",
          "then": "Employee"
        },
        {
          "if": "$.user.email startsWith 'admin'",
          "then": "Administrator"
        },
        {
          "else": true,
          "then": "External"
        }
      ]
    }
  ]
}
```

### 4b. 🔍 Complex String Operations
**Purpose**: Advanced string filtering with aggregation and complex logic
```json
// Template - Advanced String Processing
{
  "mappings": [
    {
      "from": "$.employees[*]",
      "to": "processedEmployees",
      "template": {
        "mappings": [
          {
            "from": "$.name",
            "to": "name"
          },
          {
            "from": "$.email",
            "to": "accessLevel",
            "conditions": [
              {
                "if": "$.email contains 'admin' || $.email startsWith 'alice'",
                "then": "Administrator"
              },
              {
                "if": "$.email contains 'support' && $.department == 'Customer Service'",
                "then": "Support Agent"
              },
              {
                "if": "$.email endsWith '@company.com'",
                "then": "Employee"
              },
              {
                "else": true,
                "then": "External"
              }
            ]
          },
          {
            "to": "badge",
            "concat": "{$.name} - {$.accessLevel}"
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
    },
    {
      "to": "companyEmployeeCount",
      "from": "$.employees[*]",
      "aggregation": {
        "type": "count",
        "condition": "$.item.email endsWith '@company.com'"
      }
    }
  ]
}
```

### 5. 📊 Aggregation Operations
**Purpose**: Sum, count, and average calculations on arrays
```json
// Template
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
    }
  ]
}
```

### 6. 📊 Conditional Aggregation - Simple
**Purpose**: Filter arrays before aggregation with basic conditions
```json
// Template
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
    }
  ]
}
```

### 7. 📊 Conditional Aggregation - Complex
**Purpose**: Multi-condition array filtering before aggregation
```json
// Template
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
    }
  ]
}
```

### 8. 🧮 Mathematical Operations
**Purpose**: Arithmetic operations on numeric fields
```json
// Template
{
  "mappings": [
    {
      "to": "$.result.subtotal",
      "math": {
        "operation": "subtract",
        "left": "$.product.price",
        "right": "$.product.discount"
      }
    }
  ]
}
```

### 9. 🏗️ Complex Transformation
**Purpose**: Multi-level mapping combining all features
- Field mapping + conditional logic
- Aggregation + mathematical operations  
- String concatenation + constant values
- Nested object creation

## 🎨 User Interface Components

### Header Section
- **Title**: "🎮 JSON Transform Playground"
- **Subtitle**: "Interactive JSON transformation editor - Edit, transform, and see results in real-time"
- **Compact Design**: Minimal height for maximum workspace

### Control Bar
- **Example Buttons**: 6 buttons for loading pre-defined scenarios
- **Action Buttons**: Clear All, Format JSON, Transform
- **Responsive Layout**: Adapts to screen size with proper wrapping

### Editor Panels
- **Left Panel**: "📝 Source JSON" editor with dark background
- **Right Panel**: "🔧 Transform Template" editor with matching styling
- **Headers**: Clear labels with icons for easy identification
- **Borders**: Strong visual separation between panels

### Output Section
- **Results Panel**: Expandable section showing transformation output
- **Status Indicators**: Success (green) or error (red) visual feedback
- **Copy Button**: Easy copying of results to clipboard
- **Error Messages**: Detailed error information when transformations fail

## 🔧 Technical Implementation

### Frontend Architecture
- **Pure HTML/CSS/JavaScript**: No external frameworks for maximum compatibility
- **Modern CSS**: CSS Grid, Flexbox, and CSS Variables for responsive design
- **Glass Morphism**: Backdrop filters and transparency effects
- **Syntax Highlighting**: Prism.js for JSON syntax coloring
- **Responsive Design**: Mobile-first approach with desktop enhancements

### API Integration
- **RESTful Communication**: Standard HTTP POST requests to `/api/transform`
- **JSON Protocol**: Clean request/response format
- **Error Handling**: Comprehensive error catching and user feedback
- **Real-time Updates**: Immediate API calls on transform button click

### Color Scheme
- **Primary Colors**: Steel blue and cornflower blue gradients
- **Background**: Deep blue-gray gradient for professional appearance
- **Text**: High contrast white text for excellent readability
- **Accents**: Success green, error red, and warning yellow
- **Panels**: Semi-transparent blue-gray for depth and clarity

## 🚀 Usage Workflow

### Getting Started
1. **Launch**: Start the API server with `dotnet run -- --api --port 5260`
2. **Navigate**: Open browser to `http://localhost:5260/playground`
3. **Explore**: Click example buttons to see pre-loaded scenarios

### Creating Transformations
1. **Load Example**: Click any example button for starting templates
2. **Edit Source**: Modify the left editor with your input JSON data
3. **Edit Template**: Update the right editor with transformation rules
4. **Transform**: Click "🚀 Transform" to execute the transformation
5. **Review Results**: Check the output panel for results or errors

### Best Practices
- **Start Simple**: Begin with basic field mapping before complex operations
- **Use Examples**: Study the provided examples to understand patterns
- **Validate JSON**: Use "Format JSON" to ensure proper JSON syntax
- **Test Iteratively**: Make small changes and test frequently
- **Copy Results**: Save successful transformations for later use

## 🔍 Troubleshooting

### Common Issues

**JSON Syntax Errors**
- Use the "Format JSON" button to validate syntax
- Check for missing commas, quotes, or brackets
- Ensure proper escape sequences in strings

**Template Format Issues**
- Verify the `mappings` array is properly structured
- Check JSONPath expressions use correct `$` notation
- Ensure condition syntax follows the documented format

**API Connection Issues**
- Verify the server is running with `dotnet run -- --api`
- Check the correct port is being used
- Look for CORS issues in browser developer tools

**Transformation Errors**
- Review error messages in the output panel
- Verify source paths exist in the input JSON
- Check data types match the expected transformation operations

### Browser Compatibility
- **Supported**: Chrome 80+, Firefox 75+, Safari 13+, Edge 80+
- **Features**: CSS Grid, Flexbox, Fetch API, ES6+ JavaScript
- **Mobile**: iOS Safari 13+, Android Chrome 80+

## 🔮 Future Enhancements

### Planned Features
- **Template Validation**: Real-time template syntax checking
- **Path Intellisense**: Auto-completion for JSONPath expressions
- **Save/Load**: Local storage for custom transformations
- **Export Options**: Download templates and results as files
- **Undo/Redo**: History tracking for editor changes
- **Dark/Light Themes**: User-selectable color schemes
- **Performance Profiling**: Detailed execution time breakdowns

### Advanced Features
- **Template Debugging**: Step-through transformation execution
- **Schema Validation**: JSON Schema integration for input validation
- **Batch Processing**: Multiple transformations in sequence
- **Custom Functions**: Plugin system for user-defined operations
- **Collaboration**: Shareable links for transformation templates

---

The JSON Transform Playground represents the future of interactive JSON transformation testing, providing a powerful yet intuitive interface for developers to experiment with and learn the Json.Transform library capabilities.
