# Clean Architecture Implementation Guide

## Overview

This project implements Clean Architecture with a generic repository pattern (`IDataRepository<T>`) that works seamlessly with both Cosmos DB and SQL Server using Entity Framework Core and LINQ.

## Architecture Layers

### 1. Domain Layer (`src/Domain`)
- **Purpose**: Core business entities and domain logic
- **Dependencies**: None (pure domain logic)
- **Contents**:
  - `Entities/`: Domain entities (Product, Customer, Order)
  - `BaseEntity.cs`: Base class with common properties (Id, CreatedAt, UpdatedAt)

### 2. Application Layer (`src/Application`)
- **Purpose**: Business logic, interfaces, and DTOs
- **Dependencies**: Domain layer only
- **Contents**:
  - `Interfaces/IDataRepository.cs`: Generic repository interface for all data operations
  - `Interfaces/IUnitOfWork.cs`: Unit of Work pattern for transactions
  - `DTOs/`: Data Transfer Objects for API communication

### 3. Infrastructure Layer (`src/Infrastructure`)
- **Purpose**: Data access implementations
- **Dependencies**: Domain, Application layers
- **Contents**:
  - `SqlServer/`: SQL Server implementation using EF Core
    - `SqlServerDbContext.cs`: DbContext with entity configurations
    - `SqlServerRepository.cs`: Generic repository implementation
    - `SqlServerUnitOfWork.cs`: Transaction management
  - `CosmosDB/`: Cosmos DB implementation (Singleton pattern)
    - `CosmosDbService.cs`: Singleton service for Cosmos client
    - `CosmosDbRepository.cs`: Generic repository implementation
    - `CosmosDbSettings.cs`: Configuration model
  - `DependencyInjection/`: Service registration extensions

### 4. API Layer (`src/API`)
- **Purpose**: RESTful API endpoints
- **Dependencies**: Application, Infrastructure layers
- **Contents**:
  - `Controllers/`: API controllers
  - `Program.cs`: Application startup and DI configuration
  - `appsettings.json`: Configuration

## Key Features

### Generic Repository Pattern (`IDataRepository<T>`)

The `IDataRepository<T>` interface provides a unified API for data access across both Cosmos DB and SQL Server:

```csharp
// CRUD operations
Task<T> AddAsync(T entity);
Task<T?> GetByIdAsync(string id);
Task<T> UpdateAsync(T entity);
Task<bool> DeleteAsync(string id);

// Querying with LINQ
IQueryable<T> Query();
Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

// Pagination
Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

// Aggregation
Task<int> CountAsync();
Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
```

### Singleton Cosmos DB Client

The Cosmos DB implementation uses a singleton pattern to ensure a single client instance across the application:

```csharp
public class CosmosDbService
{
    private static CosmosClient? _cosmosClient;
    private static readonly object _lock = new object();

    public CosmosClient GetClient()
    {
        if (_cosmosClient == null)
        {
            lock (_lock)
            {
                if (_cosmosClient == null)
                {
                    _cosmosClient = new CosmosClient(connectionString, options);
                }
            }
        }
        return _cosmosClient;
    }
}
```

### Multiple SQL Server Databases

The architecture supports connecting to multiple SQL Server databases:

```csharp
// appsettings.json
"ConnectionStrings": {
  "SqlServerConnection": "Server=...;Database=ProductsDb;...",
  "SqlServerConnection_OrdersDb": "Server=...;Database=OrdersDb;...",
  "SqlServerConnection_CustomersDb": "Server=...;Database=CustomersDb;..."
}
```

## Configuration Strategies

### Strategy 1: SQL Server Only

```csharp
// Program.cs
builder.Services.AddSqlServerInfrastructure(builder.Configuration);
```

### Strategy 2: Cosmos DB Only

```csharp
// Program.cs
builder.Services.AddCosmosDbInfrastructure(builder.Configuration);
```

### Strategy 3: Hybrid Approach (Recommended)

Use Cosmos DB for high-scale read operations and SQL Server for transactional data:

```csharp
// Program.cs
builder.Services.AddHybridInfrastructure(builder.Configuration);

// This configures:
// - Products → Cosmos DB (high-scale product catalog)
// - Customers, Orders → SQL Server (transactional data with ACID guarantees)
```

### Strategy 4: Custom Configuration

```csharp
// Register specific entities to specific databases
builder.Services.AddScoped<IDataRepository<Product>, CosmosDbRepository<Product>>();
builder.Services.AddScoped<IDataRepository<Customer>, SqlServerRepository<Customer>>();
```

## Usage Examples

### Example 1: Using LINQ Queries

```csharp
// Works with both Cosmos DB and SQL Server
var expensiveProducts = _productRepository
    .Query()
    .Where(p => p.Price > 100)
    .OrderByDescending(p => p.Price)
    .Take(10)
    .ToList();
```

### Example 2: Complex Queries with Expressions

```csharp
var activeProducts = await _productRepository.FindAsync(
    p => p.IsActive && p.Category == "Electronics",
    cancellationToken
);
```

### Example 3: Pagination

```csharp
var (items, totalCount) = await _productRepository.GetPagedAsync(
    pageNumber: 1,
    pageSize: 20,
    predicate: p => p.IsActive
);
```

### Example 4: Transactions (SQL Server Only)

```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    await _customerRepository.AddAsync(customer);
    await _orderRepository.AddAsync(order);
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

## Benefits

1. **Database Agnostic**: Switch between Cosmos DB and SQL Server without changing business logic
2. **Clean Separation**: Clear boundaries between layers with no leaky abstractions
3. **LINQ Support**: Use familiar LINQ queries across different data stores
4. **Performance**: Singleton Cosmos DB client, EF Core optimizations
5. **Flexibility**: Mix and match databases per entity based on requirements
6. **Testability**: Easy to mock repositories for unit testing
7. **Scalability**: Cosmos DB for high-scale scenarios, SQL Server for transactions

## Testing

### Unit Testing Repositories

```csharp
// Mock the repository
var mockRepository = new Mock<IDataRepository<Product>>();
mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default))
    .ReturnsAsync(new Product { Id = "1", Name = "Test" });
```

### Integration Testing

```csharp
// Use in-memory database for SQL Server
services.AddDbContext<SqlServerDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
```

## Migration Guide

### From SQL Server to Cosmos DB

1. Change the repository registration:
```csharp
// Before
services.AddScoped<IDataRepository<Product>, SqlServerRepository<Product>>();

// After
services.AddScoped<IDataRepository<Product>, CosmosDbRepository<Product>>();
```

2. No changes needed in controllers or business logic!

### Adding a New Entity

1. Create entity in `Domain/Entities/`
2. Add DbSet to `SqlServerDbContext` (if using SQL Server)
3. Register repository in `Program.cs`
4. Create controller if needed

## Best Practices

1. **Use DTOs**: Always use DTOs for API communication, never expose entities directly
2. **Async/Await**: Always use async methods for data operations
3. **Cancellation Tokens**: Pass cancellation tokens for long-running operations
4. **Error Handling**: Wrap repository calls in try-catch blocks
5. **Logging**: Log all data access operations
6. **Connection Strings**: Store connection strings in user secrets or Azure Key Vault
7. **Cosmos DB Partitioning**: Choose appropriate partition keys for Cosmos DB
8. **EF Core Migrations**: Use migrations for SQL Server schema changes

## Next Steps

1. Run the setup commands in README.md
2. Update connection strings in appsettings.json
3. Run EF Core migrations for SQL Server
4. Test endpoints using Swagger UI
5. Implement authentication and authorization
6. Add caching layer (Redis)
7. Add health checks
8. Configure monitoring and logging
