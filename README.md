# CosmosDB and SQL Server API with Clean Architecture

## Architecture Overview

This project implements Clean Architecture with:
- **Domain Layer**: Core entities and domain logic
- **Application Layer**: Generic interfaces (IDataRepository) and business logic
- **Infrastructure Layer**: Concrete implementations for Cosmos DB and SQL Server
- **API Layer**: RESTful endpoints

## Project Structure

```
src/
├── Domain/                     # Core entities, no dependencies
├── Application/                # Interfaces, DTOs, business logic
├── Infrastructure/             # Data access implementations
│   ├── CosmosDB/              # Cosmos DB repository
│   └── SqlServer/             # SQL Server repository with EF Core
└── API/                       # Web API controllers
```

## Prerequisites

- .NET 8 SDK
- Azure Cosmos DB account
- SQL Server instance

## Setup Instructions

1. Create the solution and projects:
```bash
dotnet new sln -n CosmosDbSqlApi

# Create projects
dotnet new classlib -n Domain -o src/Domain
dotnet new classlib -n Application -o src/Application
dotnet new classlib -n Infrastructure -o src/Infrastructure
dotnet new webapi -n API -o src/API

# Add projects to solution
dotnet sln add src/Domain/Domain.csproj
dotnet sln add src/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure.csproj
dotnet sln add src/API/API.csproj
```

2. Set up project references:
```bash
cd src/Application && dotnet add reference ../Domain/Domain.csproj
cd ../Infrastructure && dotnet add reference ../Domain/Domain.csproj ../Application/Application.csproj
cd ../API && dotnet add reference ../Application/Application.csproj ../Infrastructure/Infrastructure.csproj
```

3. Install required NuGet packages:
```bash
# Infrastructure project
cd src/Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.Azure.Cosmos

# API project
cd ../API
dotnet add package Microsoft.EntityFrameworkCore.Design
```

4. Update appsettings.json with your connection strings

5. Run migrations for SQL Server:
```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

## Configuration

See appsettings.json for connection string configuration.
