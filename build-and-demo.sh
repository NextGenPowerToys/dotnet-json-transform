#!/bin/bash

# Build, test, and generate demo script
echo "🚀 JSON Transform Library - Build, Test & Demo"
echo "=============================================="

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Parse arguments
RUN_TESTS=true
GENERATE_DEMO=true
OPEN_BROWSER=true

for arg in "$@"; do
    case $arg in
        --no-tests)
            RUN_TESTS=false
            shift
            ;;
        --no-demo)
            GENERATE_DEMO=false
            shift
            ;;
        --no-browser)
            OPEN_BROWSER=false
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [options]"
            echo "Options:"
            echo "  --no-tests     Skip running tests"
            echo "  --no-demo      Skip generating demo"
            echo "  --no-browser   Don't open browser automatically"
            echo "  --help, -h     Show this help message"
            exit 0
            ;;
    esac
done

# Build solution
echo "🔧 Building solution..."
if ! dotnet build --configuration Release; then
    echo "❌ Build failed!"
    exit 1
fi
echo "✅ Build successful!"
echo

# Run tests
if [ "$RUN_TESTS" = true ]; then
    echo "🧪 Running tests..."
    if ! dotnet test --configuration Release --logger:console; then
        echo "❌ Tests failed!"
        echo "⚠️  Continuing with demo generation anyway..."
    else
        echo "✅ All tests passed!"
    fi
    echo
fi

# Generate demo
if [ "$GENERATE_DEMO" = true ]; then
    echo "🎬 Generating dynamic demo..."
    cd "$SCRIPT_DIR/src/Json.Transform/DemoRunner"
    
    DEMO_ARGS=""
    if [ "$OPEN_BROWSER" = false ]; then
        DEMO_ARGS="--no-browser"
    fi
    
    if ! dotnet run --configuration Release -- --output "$SCRIPT_DIR/demo.html" $DEMO_ARGS; then
        echo "❌ Demo generation failed!"
        exit 1
    fi
    
    echo "✅ Demo generated successfully!"
    echo "📄 Output: $SCRIPT_DIR/demo.html"
    
    if [ "$OPEN_BROWSER" = true ]; then
        echo "🌐 Demo should have opened in your browser automatically"
    else
        echo "💡 To view the demo, open: $SCRIPT_DIR/demo.html"
    fi
fi

echo
echo "🎉 All tasks completed successfully!"
