#!/bin/bash

# HYDRA Energy Intelligence API - Start Script

echo "Starting HYDRA Energy Intelligence API..."
echo "=========================================="
echo ""

# Navigate to the Backend directory
cd "$(dirname "$0")"

# Check if .NET is installed
if ! command -v dotnet &> /dev/null
if [ ! -f "$HOME/.dotnet/dotnet" ]; then
    echo "Error: .NET 9 SDK is not installed."
    echo "Please install .NET 9 SDK from: https://dotnet.microsoft.com/download/dotnet/9.0"
    echo "Or run: wget https://dot.net/v1/dotnet-install.sh && bash dotnet-install.sh --channel 9.0"
    exit 1
fi

# Set PATH to include .NET
export PATH="$PATH:$HOME/.dotnet"

# Restore dependencies
echo "Restoring NuGet packages..."
$HOME/.dotnet/dotnet restore

if [ $? -ne 0 ]; then
    echo "Error: Failed to restore NuGet packages."
    exit 1
fi

# Build the project
echo ""
echo "Building the project..."
$HOME/.dotnet/dotnet build

if [ $? -ne 0 ]; then
    echo "Error: Build failed."
    exit 1
fi

# Run the API
echo ""
echo "Starting the API..."
echo "=========================================="
echo "API will be available at:"
echo "  - HTTP:  http://localhost:5000"
echo "  - HTTPS: https://localhost:5001"
echo "  - Swagger UI: http://localhost:5000/swagger"
echo "=========================================="
echo ""

$HOME/.dotnet/dotnet run --launch-profile http
