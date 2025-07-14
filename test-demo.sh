#!/bin/bash

# Quick test script for the dynamic demo generator
echo "🧪 Testing Dynamic Demo Generation"
echo "================================="

cd "$(dirname "$0")"

# Test just the demo generation (no tests, no browser)
echo "📝 Testing demo generation only..."
cd src/Json.Transform/DemoRunner
if dotnet run --configuration Release -- --output ../../../demo-test.html --no-browser; then
    echo "✅ Demo generation successful!"
    
    # Check if the file contains the expected content
    if grep -q "=== JSON Transform Library Demo ===" ../../../demo-test.html; then
        echo "✅ Demo contains correct title!"
    else
        echo "❌ Demo title not found!"
        exit 1
    fi
    
    # Check if it contains real transformation results
    if grep -q "Result JSON" ../../../demo-test.html; then
        echo "✅ Demo contains transformation results!"
    else
        echo "❌ Demo doesn't contain transformation results!"
        exit 1
    fi
    
    # Clean up test file
    rm -f ../../../demo-test.html
    echo "✅ All tests passed! Dynamic demo generation is working correctly."
else
    echo "❌ Demo generation failed!"
    exit 1
fi
