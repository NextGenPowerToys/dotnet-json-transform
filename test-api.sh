#!/bin/bash

# Simple test script for the JSON Transform API
API_URL="http://localhost:5001"

echo "🧪 Testing JSON Transform API..."
echo ""
echo "💡 Pro Tip: For visual testing, visit the playground:"
echo "   👉 http://localhost:5001/playground"
echo ""

# Test health endpoint
echo "1. Testing health endpoint..."
curl -s "$API_URL/api/health" | python3 -m json.tool
echo ""

# Test examples endpoint
echo "2. Testing examples endpoint..."
curl -s "$API_URL/api/examples" | python3 -m json.tool | head -20
echo ""

# Test transform endpoint
echo "3. Testing transform endpoint..."
curl -s -X POST "$API_URL/api/transform" \
  -H "Content-Type: application/json" \
  -d '{
    "sourceJson": "{\"user\": {\"name\": \"Test User\", \"age\": 30}}",
    "templateJson": "{\"mappings\": [{\"from\": \"$.user.name\", \"to\": \"$.customer.fullName\"}]}"
  }' | python3 -m json.tool
echo ""

echo "✅ API tests complete!"
