#!/bin/bash

echo "====================================="
echo "Time Tracker Application - Quick Start"
echo "====================================="
echo ""

# Check if database is running
echo "Checking PostgreSQL..."
if docker ps | grep -q postgres_container; then
    echo "✓ PostgreSQL is running"
else
    echo "✗ PostgreSQL not running. Starting..."
    cd pgDockerCompose && docker-compose up -d
    cd ..
    echo "✓ PostgreSQL started"
fi

echo ""
echo "Building solution..."
dotnet build --verbosity quiet

if [ $? -eq 0 ]; then
    echo "✓ Build successful"
else
    echo "✗ Build failed"
    exit 1
fi

echo ""
echo "Running tests..."
dotnet test --verbosity quiet --nologo

echo ""
echo "====================================="
echo "Setup Complete!"
echo "====================================="
echo ""
echo "To run the application:"
echo "  cd src/TimeTracker.Web"
echo "  dotnet run"
echo ""
echo "Then navigate to: https://localhost:5001"
echo ""
echo "Database: PostgreSQL on localhost:5432"
echo "pgAdmin: http://localhost:5050"
echo ""
