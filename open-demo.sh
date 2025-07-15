#!/bin/bash

# Generate and open dynamic JSON Transform Library Demo
echo "🎬 JSON Transform Library Demo Generator"
echo "========================================"

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
EXAMPLES_DIR="$SCRIPT_DIR/examples"

# Check if we should run tests first
RUN_TESTS=""
if [ "$1" = "--run-tests" ] || [ "$1" = "-t" ]; then
    RUN_TESTS="--tests"
    echo "🧪 Will run tests before generating demo"
fi

# Build and run the examples project with demo generation
echo "🔧 Building examples project..."
cd "$SCRIPT_DIR"
if ! dotnet build examples/Json.Transform.Examples.csproj --configuration Release > /dev/null 2>&1; then
    echo "❌ Error: Failed to build examples project"
    exit 1
fi

echo "🔄 Generating dynamic demo with real transformation results..."
dotnet run --project examples/Json.Transform.Examples.csproj --configuration Release -- $RUN_TESTS --demo

if [ $? -eq 0 ]; then
    echo "✅ Demo generated and opened successfully!"
else
    echo "❌ Error generating demo"
    exit 1
fi
