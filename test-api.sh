#!/bin/bash

# HYDRA Energy Intelligence API - Test Script

API_URL="http://localhost:5000"

echo "Testing HYDRA Energy Intelligence API"
echo "====================================="
echo ""

# Function to test an endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    
    echo "Testing: $description"
    echo "Endpoint: $method $endpoint"
    
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X $method "$API_URL$endpoint")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo "✅ Success (HTTP $http_code)"
    else
        echo "❌ Failed (HTTP $http_code)"
    fi
    
    echo "Response: ${body:0:200}..."
    echo ""
}

# Check if API is running
echo "Checking if API is running..."
if ! curl -s "$API_URL/health" > /dev/null; then
    echo "❌ Error: API is not running at $API_URL"
    echo "Please start the API first using: ./start-api.sh"
    exit 1
fi

echo "✅ API is running"
echo ""

# Test endpoints
test_endpoint "GET" "/health" "Health Check"
test_endpoint "POST" "/api/auth/token" "Authentication - Get Token"
test_endpoint "GET" "/api/auth/validate" "Authentication - Validate Token"
test_endpoint "GET" "/api/energy/data?fromDate=2025-03-01&toDate=2025-03-31" "Energy Data Retrieval"
test_endpoint "GET" "/api/weather/data?fromDate=2025-03-01&toDate=2025-03-31" "Weather Data Retrieval"
test_endpoint "GET" "/api/analytics/summary?fromDate=2025-03-01&toDate=2025-03-31" "Analytics Summary"
test_endpoint "GET" "/api/forecast?days=3" "Energy Forecast"
test_endpoint "GET" "/api/insights?fromDate=2025-03-01&toDate=2025-03-31" "Insights Generation"

echo "====================================="
echo "Testing completed!"
echo "View full API documentation at: $API_URL/swagger"
