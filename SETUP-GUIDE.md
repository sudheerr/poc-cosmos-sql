# Setup Guide - .NET 8 API with Cosmos DB & SQL Server

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, Express, or Full)
- Azure Cosmos DB account (or Cosmos DB Emulator for local development)
- Visual Studio 2022 / VS Code / Rider

## Step 1: Create Solution and Projects

```bash
# Navigate to your project directory
cd /mnt/c/git_projects/poc-cosmos-sql

# Create solution
dotnet new sln -n CosmosDbSqlApi

# Create Domain project (Class Library)
dotnet new classlib -n Domain -o src/Domain -f net8.0
rm src/Domain/Class1.cs

# Create Application project (Class Library)
dotnet new classlib -n Application -o src/Application -f net8.0
rm src/Application/Class1.cs

# Create Infrastructure project (Class Library)
dotnet new classlib -n Infrastructure -o src/Infrastructure -f net8.0
rm src/Infrastructure/Class1.cs

# Create API project (Web API)
dotnet new webapi -n API -o src/API -f net8.0

# Add projects to solution
dotnet sln add src/Domain/Domain.csproj
dotnet sln add src/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure.csproj
dotnet sln add src/API/API.csproj
```

## Step 2: Set Up Project References

```bash
# Application depends on Domain
cd src/Application
dotnet add reference ../Domain/Domain.csproj

# Infrastructure depends on Domain and Application
cd ../Infrastructure
dotnet add reference ../Domain/Domain.csproj
dotnet add reference ../Application/Application.csproj

# API depends on Application and Infrastructure
cd ../API
dotnet add reference ../Application/Application.csproj
dotnet add reference ../Infrastructure/Infrastructure.csproj

cd ../../..
```

## Step 3: Install NuGet Packages

### Infrastructure Project
```bash
cd src/Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.Azure.Cosmos
dotnet add package Microsoft.Extensions.Configuration.Abstractions
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions
cd ../..
```

### API Project
```bash
cd src/API
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
cd ../..
```

## Step 4: Copy Source Files

The following files have been created in your project:

### Domain Layer
- `src/Domain/Entities/BaseEntity.cs`
- `src/Domain/Entities/Product.cs`
- `src/Domain/Entities/Customer.cs`
- `src/Domain/Entities/Order.cs`

### Application Layer
- `src/Application/Interfaces/IDataRepository.cs`
- `src/Application/Interfaces/IUnitOfWork.cs`
- `src/Application/DTOs/ProductDto.cs`

### Infrastructure Layer
**SQL Server:**
- `src/Infrastructure/SqlServer/SqlServerDbContext.cs`
- `src/Infrastructure/SqlServer/SqlServerRepository.cs`
- `src/Infrastructure/SqlServer/SqlServerUnitOfWork.cs`

**Cosmos DB:**
- `src/Infrastructure/CosmosDB/CosmosDbSettings.cs`
- `src/Infrastructure/CosmosDB/CosmosDbService.cs` (Singleton pattern)
- `src/Infrastructure/CosmosDB/CosmosDbRepository.cs`

**Dependency Injection:**
- `src/Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`

### API Layer
- `src/API/Program.cs`
- `src/API/appsettings.json`
- `src/API/Controllers/ProductsController.cs`

## Step 5: Configure Connection Strings

### Option A: Using SQL Server LocalDB (for Development)

Update `src/API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "ProductsDb",
    "ContainerName": "Products",
    "PartitionKeyPath": "/id"
  }
}
```

### Option B: Using SQL Server Express/Full

```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=ProductsDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
  }
}
```

### Option C: Azure SQL Database

```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=tcp:yourserver.database.windows.net,1433;Database=ProductsDb;User ID=yourusername;Password=yourpassword;Encrypt=True;"
  }
}
```

### Cosmos DB Configuration

**Local Emulator:**
```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "ProductsDb",
    "ContainerName": "Products",
    "PartitionKeyPath": "/id"
  }
}
```

**Azure Cosmos DB:**
```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://your-account.documents.azure.com:443/;AccountKey=your-key-here;",
    "DatabaseName": "ProductsDb",
    "ContainerName": "Products",
    "PartitionKeyPath": "/id"
  }
}
```

## Step 6: Create and Apply EF Core Migrations

```bash
# Add initial migration
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

## Step 7: Run the Application

```bash
cd src/API
dotnet run
```

Or with hot reload:
```bash
dotnet watch run
```

The API will be available at:
- HTTPS: https://localhost:7xxx
- HTTP: http://localhost:5xxx
- Swagger UI: https://localhost:7xxx/swagger

## Step 8: Test the API

### Using Swagger UI
1. Navigate to `https://localhost:7xxx/swagger`
2. Try the endpoints

### Using curl

**Create a Product:**
```bash
curl -X POST https://localhost:7xxx/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "category": "Electronics"
  }'
```

**Get All Products:**
```bash
curl https://localhost:7xxx/api/products
```

**Get Product by ID:**
```bash
curl https://localhost:7xxx/api/products/{id}
```

**Search Products:**
```bash
curl "https://localhost:7xxx/api/products/search?name=Laptop&minPrice=1000&maxPrice=2000"
```

**Get Paged Products:**
```bash
curl "https://localhost:7xxx/api/products/paged?pageNumber=1&pageSize=10"
```

## Database Selection Strategies

### Strategy 1: SQL Server Only (Default)

In `Program.cs`:
```csharp
builder.Services.AddSqlServerInfrastructure(builder.Configuration);
```

### Strategy 2: Cosmos DB Only

In `Program.cs`, replace SQL Server configuration with:
```csharp
builder.Services.AddCosmosDbInfrastructure(builder.Configuration);

// Register Cosmos DB repositories
builder.Services.AddScoped<IDataRepository<Product>, CosmosDbRepository<Product>>();
builder.Services.AddScoped<IDataRepository<Customer>, CosmosDbRepository<Customer>>();
builder.Services.AddScoped<IDataRepository<Order>, CosmosDbRepository<Order>>();
```

### Strategy 3: Hybrid (Recommended for Production)

Use the provided `AddHybridInfrastructure` method:
```csharp
builder.Services.AddHybridInfrastructure(builder.Configuration);
```

This configures:
- **Products** → Cosmos DB (high-scale product catalog)
- **Customers, Orders** → SQL Server (transactional data)

### Strategy 4: Multiple SQL Server Databases

For multiple SQL Server databases, create separate DbContexts:

```csharp
// Products Database
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsDb")));

// Orders Database
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));
```

## Troubleshooting

### Issue: EF Core Migrations Error

**Error:** "Unable to create an object of type 'SqlServerDbContext'"

**Solution:** Ensure you specify both the project and startup-project:
```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API
```

### Issue: Cosmos DB Connection Error

**Error:** "The remote certificate is invalid"

**Solution:** For local emulator, add to Program.cs before creating CosmosClient:
```csharp
// Only for local development with Cosmos DB Emulator
if (builder.Environment.IsDevelopment())
{
    ServicePointManager.ServerCertificateValidationCallback =
        (sender, certificate, chain, errors) => true;
}
```

### Issue: SQL Server Connection Error

**Error:** "A connection was successfully established with the server, but then an error occurred"

**Solution:** Add `TrustServerCertificate=True` to your connection string:
```json
"SqlServerConnection": "Server=localhost;Database=ProductsDb;User Id=sa;Password=Pass@123;TrustServerCertificate=True;"
```

### Issue: Port Already in Use

**Solution:** Change the port in `src/API/Properties/launchSettings.json` or use:
```bash
dotnet run --urls "https://localhost:5001;http://localhost:5000"
```

## Production Considerations

### 1. Configuration Management
- Use Azure Key Vault for connection strings
- Use User Secrets for local development
- Never commit connection strings to source control

### 2. Cosmos DB Optimization
- Choose appropriate partition keys (consider using category or customer ID)
- Monitor RU consumption
- Use Cosmos DB Change Feed for real-time processing

### 3. SQL Server Optimization
- Enable connection pooling
- Use async methods everywhere
- Implement retry policies
- Add indexes for frequently queried columns

### 4. Security
- Enable authentication/authorization
- Use HTTPS in production
- Implement rate limiting
- Add CORS properly

### 5. Monitoring
- Add Application Insights
- Log all data operations
- Monitor query performance
- Set up alerts for failures

## Next Steps

1. Implement authentication (JWT, Azure AD)
2. Add validation using FluentValidation
3. Implement CQRS pattern with MediatR
4. Add caching with Redis
5. Implement health checks
6. Add API versioning
7. Create integration tests
8. Set up CI/CD pipeline

## Useful Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Create new migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/API

# Remove last migration
dotnet ef migrations remove --project src/Infrastructure --startup-project src/API

# Update database to specific migration
dotnet ef database update MigrationName --project src/Infrastructure --startup-project src/API

# Generate SQL script
dotnet ef migrations script --project src/Infrastructure --startup-project src/API --output migration.sql
```

## Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Cosmos DB .NET SDK](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-api-sdk-dotnet-standard)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
