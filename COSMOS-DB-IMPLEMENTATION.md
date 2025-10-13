# Cosmos DB Implementation Guide

## Overview

This solution implements Cosmos DB with:
- ✅ **Singleton Pattern** using Dependency Injection
- ✅ **Generic Repository** with full LINQ support
- ✅ **Multiple Database Support**
- ✅ **Service-based Architecture**
- ✅ **Entity Framework Core** option available

## Architecture

### Service-Based Approach (Implemented)

```
ICosmosDbService (Interface)
    ↓
CosmosDbService (Singleton Implementation)
    ↓
CosmosDbRepository<T> (Generic Repository)
    ↓
ProductRepository, CustomerRepository, OrderRepository (Specific Implementations)
```

## Key Components

### 1. ICosmosDbService Interface

Located: `src/Application/Interfaces/ICosmosDbService.cs`

Defines contract for Cosmos DB operations:
```csharp
public interface ICosmosDbService
{
    CosmosClient Client { get; }
    string DatabaseName { get; }
    Database GetDatabase();
    Container GetContainer(string containerName);
    Task<Database> CreateDatabaseIfNotExistsAsync(int? throughput = null);
    Task<Container> CreateContainerIfNotExistsAsync(...);
}
```

### 2. CosmosDbService (Singleton)

Located: `src/Infrastructure/Services/CosmosDbService.cs`

**Registered as Singleton** in DI container for connection reuse.

```csharp
public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _client;  // Singleton instance
    private readonly string _databaseName;

    // Injected via DI
    public CosmosDbService(CosmosClient client, CosmosDbSettings settings)
}
```

### 3. Generic Repository with LINQ

Located: `src/Infrastructure/Repositories/CosmosDbRepository.cs`

Features:
- Full LINQ support via `IQueryable<T>`
- Async operations
- Pagination support
- Expression-based filtering

```csharp
public class CosmosDbRepository<T> : IRepository<T> where T : BaseEntity
{
    // LINQ Query support
    public IQueryable<T> Query()
    {
        return _container.GetItemLinqQueryable<T>();
    }

    // Complex async queries
    public async Task<IEnumerable<T>> QueryAsync(
        Func<IQueryable<T>, IQueryable<T>> queryBuilder,
        CancellationToken cancellationToken = default)
    {
        var queryable = _container.GetItemLinqQueryable<T>();
        var query = queryBuilder(queryable);
        var iterator = query.ToFeedIterator();
        // ... execute query
    }
}
```

### 4. Specific Repositories

Located: `src/Infrastructure/Repositories/`

Each repository inherits generic functionality and adds entity-specific methods:

**ProductRepository.cs**
```csharp
public class ProductRepository : CosmosDbRepository<Product>
{
    public ProductRepository(ICosmosDbService cosmosDbService)
        : base(cosmosDbService, "Products")
    {
    }

    // Entity-specific methods using LINQ
    public async Task<IEnumerable<Product>> SearchProductsAsync(
        string? searchTerm, decimal? minPrice, decimal? maxPrice)
    {
        return await QueryAsync(query =>
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            return query.OrderBy(p => p.Name);
        });
    }
}
```

## Configuration

### appsettings.Development.json

```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=...",
    "DatabaseName": "CleanArchitectureDB",
    "Throughput": 400,

    "ProductsDatabase": {
      "ConnectionString": "...",
      "DatabaseName": "ProductsDB",
      "Throughput": 400
    },
    "CustomersDatabase": {
      "ConnectionString": "...",
      "DatabaseName": "CustomersDB",
      "Throughput": 400
    },
    "OrdersDatabase": {
      "ConnectionString": "...",
      "DatabaseName": "OrdersDB",
      "Throughput": 400
    }
  }
}
```

## Dependency Injection Setup

### Single Database Approach

Located: `src/API/Program.cs`

```csharp
// Register Cosmos DB with single database
builder.Services.AddCosmosDb(builder.Configuration);
```

This registers:
- `CosmosClient` as Singleton
- `ICosmosDbService` → `CosmosDbService` as Singleton
- `IRepository<Product>` → `ProductRepository` as Scoped
- `IRepository<Customer>` → `CustomerRepository` as Scoped
- `IRepository<Order>` → `OrderRepository` as Scoped

### Multiple Databases Approach

```csharp
// Register Cosmos DB with multiple databases
builder.Services.AddMultipleCosmosDbDatabases(builder.Configuration);
```

Each entity gets its own database and service instance.

### Extension Methods

Located: `src/Infrastructure/Configuration/CosmosDbServiceCollectionExtensions.cs`

```csharp
public static class CosmosDbServiceCollectionExtensions
{
    // Single database registration
    public static IServiceCollection AddCosmosDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "CosmosDb")

    // Multiple databases registration
    public static IServiceCollection AddMultipleCosmosDbDatabases(
        this IServiceCollection services,
        IConfiguration configuration)

    // Initialize databases and containers
    public static async Task InitializeCosmosDbAsync(
        this IServiceProvider serviceProvider,
        params (string containerName, string partitionKeyPath)[] containers)
}
```

## Usage Examples

### 1. Basic CRUD Operations

```csharp
public class ProductsController : ControllerBase
{
    private readonly ProductRepository _repository;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _repository.GetAllAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        var created = await _repository.AddAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
```

### 2. LINQ Queries

```csharp
// Simple LINQ query
var products = _repository.Query()
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name)
    .ToList();

// Complex async LINQ query
var products = await _repository.QueryAsync(query =>
    query.Where(p => p.Category == "Electronics")
         .Where(p => p.Stock > 0)
         .OrderByDescending(p => p.Price)
         .Take(10)
);
```

### 3. Expression-Based Filtering

```csharp
// Find products by category
var electronics = await _repository.FindAsync(
    p => p.Category == "Electronics" && p.IsActive
);

// Complex filtering
var filtered = await _repository.FindAsync(
    p => p.Price >= 50 &&
         p.Price <= 200 &&
         p.Stock > 0
);
```

### 4. Entity-Specific Methods

```csharp
// ProductRepository specific methods
var inStock = await _productRepository.GetProductsInStockAsync();
var byCategory = await _productRepository.GetProductsByCategoryAsync("Electronics");

// CustomerRepository specific methods
var customer = await _customerRepository.GetByEmailAsync("user@example.com");
var usCustomers = await _customerRepository.GetByCountryAsync("USA");

// OrderRepository specific methods
var customerOrders = await _orderRepository.GetByCustomerIdAsync(customerId);
var pending = await _orderRepository.GetPendingOrdersAsync();
var revenue = await _orderRepository.GetTotalRevenueAsync(startDate, endDate);
```

### 5. Pagination

```csharp
// Get paginated results
var (items, continuationToken) = await _repository.GetPagedAsync(
    pageSize: 20,
    continuationToken: null
);

// Next page
var (nextItems, nextToken) = await _repository.GetPagedAsync(
    pageSize: 20,
    continuationToken: continuationToken
);
```

## Partition Keys

Configured per entity type:

| Entity | Container | Partition Key |
|--------|-----------|---------------|
| Product | Products | /category |
| Customer | Customers | /country |
| Order | Orders | /customerId |

## Benefits of This Approach

### 1. **Singleton Pattern via DI**
- ✅ Single `CosmosClient` instance (recommended by Microsoft)
- ✅ Managed by DI container lifecycle
- ✅ Automatic disposal
- ✅ Thread-safe

### 2. **Service-Based Architecture**
- ✅ Clean separation of concerns
- ✅ Testable with interfaces
- ✅ Easy to mock in unit tests
- ✅ No factory pattern needed

### 3. **Generic Repository**
- ✅ Reusable code
- ✅ Type-safe operations
- ✅ Consistent API across entities

### 4. **Full LINQ Support**
- ✅ Familiar syntax
- ✅ Complex queries
- ✅ Expression trees
- ✅ Both sync and async

### 5. **Multiple Database Support**
- ✅ Separate databases per entity
- ✅ Independent scaling
- ✅ Better organization

## Testing

### Unit Testing with Mocked Service

```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnProduct()
{
    // Arrange
    var mockService = new Mock<ICosmosDbService>();
    var mockContainer = new Mock<Container>();

    mockService.Setup(s => s.GetContainer("Products"))
        .Returns(mockContainer.Object);

    var repository = new ProductRepository(mockService.Object);

    // Act
    var result = await repository.GetByIdAsync("123");

    // Assert
    Assert.NotNull(result);
}
```

## Running the Application

### 1. Start Cosmos DB Emulator

Download from: https://aka.ms/cosmosdb-emulator

Or use Docker:
```bash
docker run -p 8081:8081 -p 10251:10251 -p 10252:10252 -p 10253:10253 -p 10254:10254 \
    -m 3g --cpus=2.0 \
    -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 \
    -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true \
    mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
```

### 2. Run the API

```bash
cd src/API
dotnet run
```

### 3. Access Swagger

Open: https://localhost:7xxx/swagger

### 4. Test Endpoints

```bash
# Create a product
curl -X POST https://localhost:7xxx/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "category": "Electronics",
    "stock": 10
  }'

# Get all products
curl https://localhost:7xxx/api/products

# Search products
curl "https://localhost:7xxx/api/products/search?searchTerm=Laptop&minPrice=1000"
```

## Performance Tips

1. **Use Partition Keys Wisely**: Always include partition key in queries
2. **Limit Cross-Partition Queries**: They are expensive
3. **Use Pagination**: For large result sets
4. **Enable Query Metrics**: Monitor RU consumption
5. **Use Singleton Client**: One client per application (✅ Already implemented)

## Troubleshooting

### Emulator Certificate Issues
```bash
# Trust the certificate (Windows)
dotnet dev-certs https --trust

# For Cosmos DB Emulator
# Export certificate from emulator and install
```

### Connection Refused
- Ensure Cosmos DB Emulator is running
- Check port 8081 is available
- Verify connection string in appsettings

### High RU Consumption
- Check partition key usage
- Review query patterns
- Add indexes if needed
- Use pagination

## Next Steps

1. ✅ Implement custom LINQ queries
2. ✅ Add more entity-specific methods
3. ✅ Set up proper partition strategies
4. ✅ Add caching layer (Redis)
5. ✅ Implement change feed processing
6. ✅ Add monitoring and logging
7. ✅ Write comprehensive tests

---

**All implemented with Clean Architecture, Singleton pattern via DI, and full LINQ support!** 🚀
