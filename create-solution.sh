#!/bin/bash

# Create .NET 8 Solution with Clean Architecture
# This script creates the complete solution structure

echo "Creating solution and projects..."

# Create solution
dotnet new sln -n CosmosDbSqlApi

# Create projects
echo "Creating Domain project..."
dotnet new classlib -n Domain -o src/Domain -f net8.0
rm src/Domain/Class1.cs

echo "Creating Application project..."
dotnet new classlib -n Application -o src/Application -f net8.0
rm src/Application/Class1.cs

echo "Creating Infrastructure project..."
dotnet new classlib -n Infrastructure -o src/Infrastructure -f net8.0
rm src/Infrastructure/Class1.cs

echo "Creating API project..."
dotnet new webapi -n API -o src/API -f net8.0 --no-openapi false

# Create test projects
echo "Creating test projects..."
dotnet new xunit -n Domain.Tests -o tests/Domain.Tests -f net8.0
dotnet new xunit -n API.Tests -o tests/API.Tests -f net8.0
dotnet new xunit -n Infrastructure.Tests -o tests/Infrastructure.Tests -f net8.0

# Remove default test files
rm tests/Domain.Tests/UnitTest1.cs 2>/dev/null || true
rm tests/API.Tests/UnitTest1.cs 2>/dev/null || true
rm tests/Infrastructure.Tests/UnitTest1.cs 2>/dev/null || true

# Add projects to solution
echo "Adding projects to solution..."
dotnet sln add src/Domain/Domain.csproj
dotnet sln add src/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure.csproj
dotnet sln add src/API/API.csproj
dotnet sln add tests/Domain.Tests/Domain.Tests.csproj
dotnet sln add tests/API.Tests/API.Tests.csproj
dotnet sln add tests/Infrastructure.Tests/Infrastructure.Tests.csproj

# Set up project references
echo "Setting up project references..."

# Application depends on Domain
cd src/Application
dotnet add reference ../Domain/Domain.csproj
cd ../..

# Infrastructure depends on Domain and Application
cd src/Infrastructure
dotnet add reference ../Domain/Domain.csproj
dotnet add reference ../Application/Application.csproj
cd ../..

# API depends on Application and Infrastructure
cd src/API
dotnet add reference ../Application/Application.csproj
dotnet add reference ../Infrastructure/Infrastructure.csproj
cd ../..

# Domain.Tests depends on Domain
cd tests/Domain.Tests
dotnet add reference ../../src/Domain/Domain.csproj
cd ../..

# API.Tests depends on API, Application, Infrastructure, Domain
cd tests/API.Tests
dotnet add reference ../../src/API/API.csproj
dotnet add reference ../../src/Application/Application.csproj
dotnet add reference ../../src/Infrastructure/Infrastructure.csproj
dotnet add reference ../../src/Domain/Domain.csproj
cd ../..

# Infrastructure.Tests depends on Infrastructure, Application, Domain
cd tests/Infrastructure.Tests
dotnet add reference ../../src/Infrastructure/Infrastructure.csproj
dotnet add reference ../../src/Application/Application.csproj
dotnet add reference ../../src/Domain/Domain.csproj
cd ../..

# Install NuGet packages
echo "Installing NuGet packages..."

# Infrastructure packages
cd src/Infrastructure
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.Azure.Cosmos --version 3.38.1
dotnet add package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0
cd ../..

# API packages
cd src/API
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
cd ../..

# Test packages - Domain.Tests
cd tests/Domain.Tests
dotnet add package xunit --version 2.6.2
dotnet add package xunit.runner.visualstudio --version 2.5.4
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
cd ../..

# Test packages - API.Tests
cd tests/API.Tests
dotnet add package xunit --version 2.6.2
dotnet add package xunit.runner.visualstudio --version 2.5.4
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add package Moq --version 4.20.70
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
cd ../..

# Test packages - Infrastructure.Tests
cd tests/Infrastructure.Tests
dotnet add package xunit --version 2.6.2
dotnet add package xunit.runner.visualstudio --version 2.5.4
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
cd ../..

echo ""
echo "✅ Solution created successfully!"
echo ""
echo "Next steps:"
echo "1. Copy the generated source files to their respective directories"
echo "2. Build the solution: dotnet build"
echo "3. Run tests: dotnet test"
echo "4. Run the API: cd src/API && dotnet run"
echo ""
