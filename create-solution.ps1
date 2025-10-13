# PowerShell script to create .NET 8 Solution with Clean Architecture

Write-Host "Creating solution and projects..." -ForegroundColor Green

# Create solution
dotnet new sln -n CosmosDbSqlApi

# Create projects
Write-Host "Creating Domain project..." -ForegroundColor Yellow
dotnet new classlib -n Domain -o src/Domain -f net8.0
Remove-Item src/Domain/Class1.cs -ErrorAction SilentlyContinue

Write-Host "Creating Application project..." -ForegroundColor Yellow
dotnet new classlib -n Application -o src/Application -f net8.0
Remove-Item src/Application/Class1.cs -ErrorAction SilentlyContinue

Write-Host "Creating Infrastructure project..." -ForegroundColor Yellow
dotnet new classlib -n Infrastructure -o src/Infrastructure -f net8.0
Remove-Item src/Infrastructure/Class1.cs -ErrorAction SilentlyContinue

Write-Host "Creating API project..." -ForegroundColor Yellow
dotnet new webapi -n API -o src/API -f net8.0 --no-openapi false

# Create test projects
Write-Host "Creating test projects..." -ForegroundColor Yellow
dotnet new xunit -n Domain.Tests -o tests/Domain.Tests -f net8.0
dotnet new xunit -n API.Tests -o tests/API.Tests -f net8.0
dotnet new xunit -n Infrastructure.Tests -o tests/Infrastructure.Tests -f net8.0

# Remove default test files
Remove-Item tests/Domain.Tests/UnitTest1.cs -ErrorAction SilentlyContinue
Remove-Item tests/API.Tests/UnitTest1.cs -ErrorAction SilentlyContinue
Remove-Item tests/Infrastructure.Tests/UnitTest1.cs -ErrorAction SilentlyContinue

# Add projects to solution
Write-Host "Adding projects to solution..." -ForegroundColor Yellow
dotnet sln add src/Domain/Domain.csproj
dotnet sln add src/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure.csproj
dotnet sln add src/API/API.csproj
dotnet sln add tests/Domain.Tests/Domain.Tests.csproj
dotnet sln add tests/API.Tests/API.Tests.csproj
dotnet sln add tests/Infrastructure.Tests/Infrastructure.Tests.csproj

# Set up project references
Write-Host "Setting up project references..." -ForegroundColor Yellow

# Application depends on Domain
Push-Location src/Application
dotnet add reference ../Domain/Domain.csproj
Pop-Location

# Infrastructure depends on Domain and Application
Push-Location src/Infrastructure
dotnet add reference ../Domain/Domain.csproj
dotnet add reference ../Application/Application.csproj
Pop-Location

# API depends on Application and Infrastructure
Push-Location src/API
dotnet add reference ../Application/Application.csproj
dotnet add reference ../Infrastructure/Infrastructure.csproj
Pop-Location

# Domain.Tests depends on Domain
Push-Location tests/Domain.Tests
dotnet add reference ../../src/Domain/Domain.csproj
Pop-Location

# API.Tests depends on API, Application, Infrastructure, Domain
Push-Location tests/API.Tests
dotnet add reference ../../src/API/API.csproj
dotnet add reference ../../src/Application/Application.csproj
dotnet add reference ../../src/Infrastructure/Infrastructure.csproj
dotnet add reference ../../src/Domain/Domain.csproj
Pop-Location

# Infrastructure.Tests depends on Infrastructure, Application, Domain
Push-Location tests/Infrastructure.Tests
dotnet add reference ../../src/Infrastructure/Infrastructure.csproj
dotnet add reference ../../src/Application/Application.csproj
dotnet add reference ../../src/Domain/Domain.csproj
Pop-Location

# Install NuGet packages
Write-Host "Installing NuGet packages..." -ForegroundColor Yellow

# Infrastructure packages
Push-Location src/Infrastructure
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.Azure.Cosmos --version 3.38.1
dotnet add package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0
Pop-Location

# API packages
Push-Location src/API
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
Pop-Location

# Test packages - Domain.Tests
Push-Location tests/Domain.Tests
dotnet add package xunit --version 2.6.2
dotnet add package xunit.runner.visualstudio --version 2.5.4
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
Pop-Location

# Test packages - API.Tests
Push-Location tests/API.Tests
dotnet add package xunit --version 2.6.2
dotnet add package xunit.runner.visualstudio --version 2.5.4
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add package Moq --version 4.20.70
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
Pop-Location

# Test packages - Infrastructure.Tests
Push-Location tests/Infrastructure.Tests
dotnet add package xunit --version 2.6.2
dotnet add package xunit.runner.visualstudio --version 2.5.4
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
Pop-Location

Write-Host ""
Write-Host "✅ Solution created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Copy the generated source files to their respective directories" -ForegroundColor White
Write-Host "2. Build the solution: dotnet build" -ForegroundColor White
Write-Host "3. Run tests: dotnet test" -ForegroundColor White
Write-Host "4. Run the API: cd src/API && dotnet run" -ForegroundColor White
Write-Host ""
