#!/bin/bash

# Generate and open dynamic JSON Transform Library Demo
echo "🎬 JSON Transform Library Demo Generator"
echo "========================================"

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEMO_RUNNER_DIR="$SCRIPT_DIR/src/Json.Transform/DemoRunner"

# Check if we should run tests first
RUN_TESTS=""
if [ "$1" = "--run-tests" ] || [ "$1" = "-t" ]; then
    RUN_TESTS="--run-tests"
    echo "🧪 Will run tests before generating demo"
fi

# Build and run the demo generator
echo "🔧 Building demo generator..."
cd "$DEMO_RUNNER_DIR"
if ! dotnet build --configuration Release > /dev/null 2>&1; then
    echo "❌ Error: Failed to build demo generator"
    exit 1
fi

echo "🔄 Generating dynamic demo with real transformation results..."
dotnet run --configuration Release -- $RUN_TESTS --output "$SCRIPT_DIR/demo.html"

if [ $? -eq 0 ]; then
    echo "✅ Demo generated and opened successfully!"
else
    echo "❌ Error generating demo"
    exit 1
fi
