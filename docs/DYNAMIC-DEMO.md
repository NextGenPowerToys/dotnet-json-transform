# Dynamic Demo System

The Json.Transform library includes a sophisticated **dynamic demo generation system** that creates live, interactive HTML demonstrations with real transformation results.

## 🎯 Key Features

### ✅ Real-Time Execution
- Every demo runs **actual transformation code** with live data
- Results are generated fresh each time, not pre-recorded
- Shows real execution times and performance metrics

### 🔄 Dynamic Generation
- HTML is created programmatically with current test results
- No static files - everything is generated on demand
- Updates automatically reflect code changes

### 📊 Comprehensive Examples
The demo includes 6 complete transformation scenarios:

1. **🔄 Field Mapping** - Basic field copying and restructuring
2. **🎯 Conditional Logic** - Age-based categorization with if/else
3. **📊 Aggregation** - Sum, count, average, and max operations
4. **🧮 Math Operations** - Arithmetic calculations on order data
5. **🔗 String Concatenation** - Template-based string building
6. **🏗️ Complex Transformations** - Multi-step nested operations

### 🎨 Interactive Features
- **Syntax Highlighting** - Color-coded JSON with proper formatting
- **Execution Metrics** - Real timing data for each transformation
- **Success Indicators** - Visual confirmation of successful operations
- **Responsive Design** - Works on desktop and mobile devices

## 🚀 Usage

### Quick Demo Generation
```bash
# macOS/Linux
./open-demo.sh

# Windows  
.\open-demo.ps1
```

### With Tests
```bash
# Run tests first, then generate demo
./open-demo.sh --run-tests
.\open-demo.ps1 --run-tests
```

### Full Build Pipeline
```bash
# Build, test, and generate demo
./build-and-demo.sh

# Custom options
./build-and-demo.sh --no-tests    # Skip tests
./build-and-demo.sh --no-browser  # Don't auto-open browser
```

### Manual Generation
```bash
cd src/Json.Transform/DemoRunner
dotnet run -- --output ../../../demo.html --run-tests
```

## 🔧 Architecture

### Demo Generator (`DemoGenerator.cs`)
- Executes real transformations using the JsonTransformer library
- Generates HTML with embedded CSS and JavaScript
- Includes timing and performance metrics
- Handles all formatting and syntax highlighting

### Demo Runner (`Program.cs`)
- Command-line interface for demo generation
- Supports test execution before demo generation
- Cross-platform browser opening
- Flexible output path configuration

### Build Integration
- Integrated with the main solution
- Proper project references and dependencies
- Excludes conflicting files to prevent build issues

## 📈 Benefits

### For Development
- **Validates functionality** - Demo breaks if library is broken
- **Visual testing** - See transformation results immediately  
- **Performance monitoring** - Track execution time changes
- **Documentation sync** - Examples stay current with code

### For Users
- **Live demonstrations** - See exactly how the library works
- **Real examples** - Copy and paste working code
- **Visual learning** - Understand transformations through examples
- **Confidence building** - Proof that the library actually works

### For Documentation
- **Always current** - Regenerated with each build
- **Accurate examples** - Uses real library code, not samples
- **Performance data** - Shows actual execution characteristics
- **Error detection** - Will fail if library has issues

## 🎬 Output Format

The generated demo displays:

```
🎬 === JSON Transform Library Demo ===
Live Demo Results - Generated at 2025-01-14 10:30:45

✅ 6 Tests Executed  ⚡ All Transformations Successful  🚀 Performance: < 5ms avg
```

Each transformation section includes:
- **Title and description** of the transformation type
- **Execution metrics** (timing, success status)
- **Source JSON** - Input data with syntax highlighting
- **Transform Template** - Transformation rules
- **Result JSON** - Final output with highlighting

## 🔮 Future Enhancements

- **Interactive editing** - Modify templates and see results live
- **Performance graphs** - Visual representation of execution times
- **Export functionality** - Save individual examples
- **Template library** - Common transformation patterns
- **Comparison mode** - Before/after transformation views

---

This dynamic demo system ensures that documentation stays synchronized with the actual library functionality, providing users with confidence that the examples they see represent real, working code.
