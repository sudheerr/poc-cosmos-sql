# Testing Guide

This project includes comprehensive test coverage using xUnit, Moq, and InMemory database providers.

## Test Projects Structure

```
tests/
├── Domain.Tests/              # Domain entity tests
├── API.Tests/                 # Controller unit tests & integration tests
└── Infrastructure.Tests/      # Repository integration tests
```

## Setting Up Test Projects

### 1. Create Test Projects

```bash
# Create test projects
dotnet new xunit -n Domain.Tests -o tests/Domain.Tests
dotnet new xunit -n API.Tests -o tests/API.Tests
dotnet new xunit -n Infrastructure.Tests -o tests/Infrastructure.Tests

# Add to solution
dotnet sln add tests/Domain.Tests/Domain.Tests.csproj
dotnet sln add tests/API.Tests/API.Tests.csproj
dotnet sln add tests/Infrastructure.Tests/Infrastructure.Tests.csproj
```

### 2. Add Project References

```bash
# Domain.Tests references Domain
cd tests/Domain.Tests
dotnet add reference ../../src/Domain/Domain.csproj

# API.Tests references API, Application, Domain, Infrastructure
cd ../API.Tests
dotnet add reference ../../src/API/API.csproj
dotnet add reference ../../src/Application/Application.csproj
dotnet add reference ../../src/Domain/Domain.csproj
dotnet add reference ../../src/Infrastructure/Infrastructure.csproj

# Infrastructure.Tests references Infrastructure, Application, Domain
cd ../Infrastructure.Tests
dotnet add reference ../../src/Infrastructure/Infrastructure.csproj
dotnet add reference ../../src/Application/Application.csproj
dotnet add reference ../../src/Domain/Domain.csproj
```

### 3. Install Required NuGet Packages

#### Domain.Tests
```bash
cd tests/Domain.Tests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
```

#### API.Tests
```bash
cd tests/API.Tests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

#### Infrastructure.Tests
```bash
cd tests/Infrastructure.Tests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

## Test Categories

### 1. Domain Tests

**Purpose:** Test domain entities and business rules

**Location:** `tests/Domain.Tests/Entities/`

**Examples:**
- `ProductTests.cs` - Product entity validation
- `CustomerTests.cs` - Customer entity validation
- `OrderTests.cs` - Order entity and OrderItem tests

**Run:**
```bash
dotnet test tests/Domain.Tests
```

### 2. Unit Tests (Controllers with Moq)

**Purpose:** Test controller logic with mocked dependencies

**Location:** `tests/API.Tests/Controllers/`

**Key Features:**
- Uses **Moq** to mock `IDataRepository<T>`
- Tests controller actions in isolation
- Verifies correct HTTP status codes
- Tests error handling

**Example Test:**
```csharp
[Fact]
public async Task GetById_ShouldReturnOkResult_WhenProductExists()
{
    // Arrange
    var product = new Product { Id = "1", Name = "Test Product" };
    _mockRepository.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(product);

    // Act
    var result = await _controller.GetById("1", CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedProduct = Assert.IsType<Product>(okResult.Value);
    Assert.Equal("1", returnedProduct.Id);
}
```

**Run:**
```bash
dotnet test tests/API.Tests --filter "FullyQualifiedName~Controllers"
```

### 3. Integration Tests (Repositories)

**Purpose:** Test repository implementations with InMemory database

**Location:** `tests/Infrastructure.Tests/SqlServer/`

**Key Features:**
- Uses EF Core InMemory database
- Tests actual repository logic
- Tests LINQ queries
- Tests transactions with Unit of Work

**Example Test:**
```csharp
[Fact]
public async Task AddAsync_ShouldAddEntity_AndSetCreatedAt()
{
    // Arrange
    var product = new Product { Name = "Test Product", Price = 99.99m };

    // Act
    var result = await _repository.AddAsync(product);

    // Assert
    Assert.NotNull(result);
    Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
}
```

**Run:**
```bash
dotnet test tests/Infrastructure.Tests
```

### 4. Integration Tests (API Endpoints)

**Purpose:** End-to-end testing of API endpoints

**Location:** `tests/API.Tests/Integration/`

**Key Features:**
- Uses `WebApplicationFactory` to create test server
- Tests complete HTTP request/response cycle
- Tests with InMemory database
- Validates status codes, headers, and response bodies

**Example Test:**
```csharp
[Fact]
public async Task Create_ShouldCreateProduct_AndReturnCreated()
{
    // Arrange
    var createDto = new CreateProductDto
    {
        Name = "New Product",
        Price = 199.99m,
        Category = "Electronics"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/products", createDto);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
    Assert.NotNull(createdProduct);
}
```

**Run:**
```bash
dotnet test tests/API.Tests --filter "FullyQualifiedName~Integration"
```

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/API.Tests
dotnet test tests/Domain.Tests
dotnet test tests/Infrastructure.Tests
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~ProductsControllerTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "GetById_ShouldReturnOkResult_WhenProductExists"
```

### Run Tests with Detailed Output
```bash
dotnet test --verbosity detailed
```

### Run Tests with Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Tests in Watch Mode
```bash
dotnet watch test
```

## Test Coverage Report

### Install Coverage Tools
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Generate Coverage Report
```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open the report
open coveragereport/index.html  # macOS
start coveragereport/index.html # Windows
xdg-open coveragereport/index.html # Linux
```

## Test Organization

### Naming Conventions

**Test Classes:**
- `{ClassName}Tests` - e.g., `ProductsControllerTests`

**Test Methods:**
- `{MethodName}_{Scenario}_{ExpectedResult}`
- Examples:
  - `GetById_ShouldReturnOkResult_WhenProductExists`
  - `Create_ShouldReturnBadRequest_WhenInvalidData`
  - `Delete_ShouldReturnNotFound_WhenProductDoesNotExist`

**Test Structure (AAA Pattern):**
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and mocks
    var product = new Product { Name = "Test" };

    // Act - Execute the method being tested
    var result = await _controller.GetById("1");

    // Assert - Verify the expected outcome
    Assert.NotNull(result);
}
```

## Moq Usage Examples

### Basic Setup
```csharp
var mockRepository = new Mock<IDataRepository<Product>>();
```

### Setup Method Return Value
```csharp
mockRepository
    .Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Product { Id = "1", Name = "Test" });
```

### Setup Method with Predicate
```csharp
mockRepository
    .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new List<Product> { /* ... */ });
```

### Setup Method to Throw Exception
```csharp
mockRepository
    .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
    .ThrowsAsync(new Exception("Database error"));
```

### Verify Method Was Called
```csharp
mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
```

### Setup with Callback
```csharp
mockRepository
    .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((Product p, CancellationToken ct) => p);
```

## Best Practices

### 1. Test Isolation
- Each test should be independent
- Use fresh database context for each test
- Dispose resources properly

### 2. Use Theory for Similar Tests
```csharp
[Theory]
[InlineData("Electronics", 999.99)]
[InlineData("Books", 29.99)]
public void Product_ShouldAcceptDifferentCategories(string category, decimal price)
{
    var product = new Product { Category = category, Price = price };
    Assert.Equal(category, product.Category);
}
```

### 3. Mock Only External Dependencies
- Mock repositories and external services
- Use InMemory database for repository tests
- Don't mock what you're testing

### 4. Test Edge Cases
- Null values
- Empty collections
- Invalid input
- Boundary conditions

### 5. Keep Tests Simple
- One logical assertion per test
- Clear test names
- Follow AAA pattern

## Continuous Integration

### GitHub Actions Example
```yaml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

## Test Data Builders (Optional)

Create builders for complex test data:

```csharp
public class ProductBuilder
{
    private string _name = "Default Product";
    private decimal _price = 99.99m;
    private string _category = "Electronics";

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public Product Build()
    {
        return new Product
        {
            Name = _name,
            Price = _price,
            Category = _category
        };
    }
}

// Usage:
var product = new ProductBuilder()
    .WithName("Laptop")
    .WithPrice(1299.99m)
    .Build();
```

## Common Issues and Solutions

### Issue: InMemory Database Not Cleaning Up
**Solution:** Use unique database names per test
```csharp
var options = new DbContextOptionsBuilder<SqlServerDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

### Issue: Async Tests Not Working
**Solution:** Use `async Task` and `await`
```csharp
[Fact]
public async Task MyTest()
{
    var result = await _repository.GetByIdAsync("1");
    Assert.NotNull(result);
}
```

### Issue: Moq Setup Not Working
**Solution:** Ensure parameter matchers are correct
```csharp
// Use It.IsAny<T>() for flexible matching
.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
```

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [EF Core Testing](https://learn.microsoft.com/en-us/ef/core/testing/)
- [ASP.NET Core Integration Tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
