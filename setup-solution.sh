#!/bin/bash

echo "Creating .NET 8 Clean Architecture Solution..."

# Create solution
dotnet new sln -n CleanArchitecture

# Create Domain layer (Class Library)
dotnet new classlib -n Domain -o src/Domain -f net8.0
rm src/Domain/Class1.cs 2>/dev/null || true

# Create Application layer (Class Library)
dotnet new classlib -n Application -o src/Application -f net8.0
rm src/Application/Class1.cs 2>/dev/null || true

# Create Infrastructure layer (Class Library)
dotnet new classlib -n Infrastructure -o src/Infrastructure -f net8.0
rm src/Infrastructure/Class1.cs 2>/dev/null || true

# Create API layer (Web API)
dotnet new webapi -n API -o src/API -f net8.0

# Add projects to solution
dotnet sln add src/Domain/Domain.csproj
dotnet sln add src/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure.csproj
dotnet sln add src/API/API.csproj

# Set up project references
cd src/Application && dotnet add reference ../Domain/Domain.csproj && cd ../..
cd src/Infrastructure && dotnet add reference ../Domain/Domain.csproj ../Application/Application.csproj && cd ../..
cd src/API && dotnet add reference ../Application/Application.csproj ../Infrastructure/Infrastructure.csproj && cd ../..

echo "✅ Solution created successfully!"
echo "Run 'dotnet build' to build the solution"
