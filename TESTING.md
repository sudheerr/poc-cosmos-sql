# Unit Testing Guide with Moq

## Overview

This project includes comprehensive unit tests using:
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **.NET 8** - Latest .NET version

## Test Project Structure

```
tests/
├── Infrastructure.Tests/
│   ├── Services/
│   │   └── CosmosDbServiceTests.cs          (12 tests)
│   └── Repositories/
│       ├── CosmosDbRepositoryTests.cs       (11 tests)
│       └── ProductRepositoryTests.cs        (3 tests)
└── API.Tests/
    └── Controllers/
        └── ProductsControllerTests.cs       (13 tests)
```

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/Infrastructure.Tests
dotnet test tests/API.Tests
```

### Run with Detailed Output
```bash
dotnet test --verbosity detailed
```

### Run with Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Coverage

### Infrastructure.Tests (26 tests)

#### CosmosDbServiceTests.cs (12 tests)
Tests for `CosmosDbService` singleton service:

✅ Constructor validation
- `Constructor_WithNullClient_ThrowsArgumentNullException`
- `Constructor_WithNullSettings_ThrowsArgumentNullException`

✅ Property access
- `Client_ShouldReturnCosmosClient`
- `DatabaseName_ShouldReturnConfiguredName`

✅ Database operations
- `GetDatabase_ShouldReturnDatabase`
- `CreateDatabaseIfNotExistsAsync_ShouldCallCosmosClient`

✅ Container operations
- `GetContainer_WithValidName_ShouldReturnContainer`
- `GetContainer_WithNullName_ThrowsArgumentException`
- `GetContainer_WithEmptyName_ThrowsArgumentException`
- `CreateContainerIfNotExistsAsync_WithValidParameters_ShouldCreateContainer`
- `CreateContainerIfNotExistsAsync_WithNullContainerName_ThrowsArgumentException`
- `CreateContainerIfNotExistsAsync_WithNullPartitionKey_ThrowsArgumentException`

#### CosmosDbRepositoryTests.cs (11 tests)
Tests for generic repository:

✅ Constructor validation
- `Constructor_WithNullService_ThrowsArgumentNullException`
- `Constructor_WithNullContainerName_ThrowsArgumentException`
- `Constructor_WithEmptyContainerName_ThrowsArgumentException`

✅ CRUD operations
- `GetByIdAsync_WhenItemExists_ReturnsItem`
- `GetByIdAsync_WhenItemNotFound_ReturnsNull`
- `AddAsync_WithValidEntity_SetsCreatedAtAndReturnsEntity`
- `AddAsync_WithNullEntity_ThrowsArgumentNullException`
- `UpdateAsync_WithValidEntity_SetsUpdatedAtAndReturnsEntity`
- `UpdateAsync_WithNullEntity_ThrowsArgumentNullException`
- `DeleteAsync_WhenItemExists_ReturnsTrue`
- `DeleteAsync_WhenItemNotFound_ReturnsFalse`

✅ Query support
- `Query_ReturnsQueryable`

#### ProductRepositoryTests.cs (3 tests)
Tests for product-specific repository:

✅ Constructor validation
- `Constructor_WithNullService_ThrowsArgumentNullException`

✅ Entity-specific methods
- `GetProductsByCategoryAsync_ShouldCallFindAsync`
- `GetProductsInStockAsync_ShouldFilterActiveProductsWithStock`
- `SearchProductsAsync_WithAllParameters_ShouldBuildComplexQuery`

### API.Tests (13 tests)

#### ProductsControllerTests.cs (13 tests)
Tests for Products API controller:

✅ GetAll endpoint
- `GetAll_ReturnsOkResult_WithListOfProducts`
- `GetAll_WhenExceptionThrown_ReturnsInternalServerError`

✅ GetById endpoint
- `GetById_WithExistingId_ReturnsOkResult`
- `GetById_WithNonExistingId_ReturnsNotFound`

✅ GetByCategory endpoint
- `GetByCategory_ReturnsOkResult_WithFilteredProducts`

✅ GetInStock endpoint
- `GetInStock_ReturnsOkResult_WithInStockProducts`

✅ Search endpoint
- `Search_WithParameters_ReturnsFilteredProducts`

✅ Create endpoint
- `Create_WithValidProduct_ReturnsCreatedAtAction`

✅ Update endpoint
- `Update_WithExistingProduct_ReturnsOkResult`
- `Update_WithNonExistingProduct_ReturnsNotFound`

✅ Delete endpoint
- `Delete_WithExistingId_ReturnsNoContent`
- `Delete_WithNonExistingId_ReturnsNotFound`

## Moq Usage Examples

### 1. Mocking Services

```csharp
// Mock ICosmosDbService
var mockService = new Mock<ICosmosDbService>();
mockService.Setup(s => s.GetContainer("Products"))
    .Returns(Mock.Of<Container>());
```

### 2. Mocking Repository Methods

```csharp
// Mock GetAllAsync
_mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(new List<Product> { /* ... */ });

// Mock GetByIdAsync with return value
_mockRepository.Setup(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Product { Id = "123" });

// Mock GetByIdAsync with null return
_mockRepository.Setup(r => r.GetByIdAsync("999", It.IsAny<CancellationToken>()))
    .ReturnsAsync((Product?)null);
```

### 3. Mocking Cosmos DB Responses

```csharp
// Mock successful ItemResponse
var mockResponse = new Mock<ItemResponse<Product>>();
mockResponse.Setup(r => r.Resource).Returns(product);

_mockContainer.Setup(c => c.ReadItemAsync<Product>(
        It.IsAny<string>(),
        It.IsAny<PartitionKey>(),
        It.IsAny<ItemRequestOptions>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(mockResponse.Object);
```

### 4. Mocking Exceptions

```csharp
// Mock CosmosException for NotFound
var cosmosException = new CosmosException(
    "Not found",
    HttpStatusCode.NotFound,
    0, "", 0);

_mockContainer.Setup(c => c.ReadItemAsync<Product>(/* ... */))
    .ThrowsAsync(cosmosException);
```

### 5. Verifying Method Calls

```csharp
// Verify method was called once
_mockRepository.Verify(
    r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
    Times.Once);

// Verify with specific parameters
_mockService.Verify(
    s => s.GetContainer("Products"),
    Times.Once);
```

## Test Patterns

### Arrange-Act-Assert (AAA)

```csharp
[Fact]
public async Task GetById_WithExistingId_ReturnsOkResult()
{
    // Arrange
    var productId = "123";
    var product = new Product { Id = productId, Name = "Test" };
    _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(product);

    // Act
    var result = await _controller.GetById(productId, CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedProduct = Assert.IsType<Product>(okResult.Value);
    Assert.Equal(productId, returnedProduct.Id);
}
```

### Testing Constructor Validation

```csharp
[Fact]
public void Constructor_WithNullService_ThrowsArgumentNullException()
{
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
        new CosmosDbService(null!, settings));
}
```

### Testing Async Methods

```csharp
[Fact]
public async Task AddAsync_WithValidEntity_SetsCreatedAtAndReturnsEntity()
{
    // Arrange
    var product = new Product { Name = "New Product" };

    // Act
    var result = await _repository.AddAsync(product);

    // Assert
    Assert.NotEqual(DateTime.MinValue, product.CreatedAt);
}
```

## Best Practices

### 1. Use Descriptive Test Names
```csharp
// Good
GetById_WithExistingId_ReturnsOkResult()

// Bad
TestGetById()
```

### 2. One Assertion Per Test (when possible)
```csharp
[Fact]
public async Task Create_WithValidProduct_ReturnsCreatedAtAction()
{
    // Focus on testing one behavior
    var result = await _controller.Create(product);
    Assert.IsType<CreatedAtActionResult>(result.Result);
}
```

### 3. Use It.IsAny<T>() for Flexible Matching
```csharp
_mockRepository.Setup(r => r.GetByIdAsync(
    It.IsAny<string>(),
    It.IsAny<CancellationToken>()))
    .ReturnsAsync(product);
```

### 4. Test Both Success and Failure Cases
```csharp
// Success case
GetById_WithExistingId_ReturnsOkResult()

// Failure case
GetById_WithNonExistingId_ReturnsNotFound()
```

### 5. Mock External Dependencies Only
- ✅ Mock: ICosmosDbService, IRepository<T>
- ❌ Don't mock: Domain entities, DTOs

## Running Specific Tests

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~GetById_WithExistingId_ReturnsOkResult"
```

### Run Tests by Class
```bash
dotnet test --filter "FullyQualifiedName~ProductsControllerTests"
```

### Run Tests by Category
```bash
dotnet test --filter "Category=Integration"
```

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

## Test Statistics

- **Total Test Projects**: 2
- **Total Test Classes**: 4
- **Total Test Methods**: 39+
- **Test Coverage**: Services, Repositories, Controllers
- **Mocking Framework**: Moq 4.20.70
- **Test Framework**: xUnit 2.6.2

## Troubleshooting

### Issue: Tests not discovered
**Solution**: Rebuild the solution
```bash
dotnet clean
dotnet build
dotnet test
```

### Issue: Moq setup not working
**Solution**: Ensure correct method signature
```csharp
// Correct
_mock.Setup(m => m.MethodAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(result);
```

### Issue: Async tests timing out
**Solution**: Use proper async/await
```csharp
[Fact]
public async Task MyTest()  // async Task, not void
{
    await _repository.GetAllAsync();
}
```

## Next Steps

1. ✅ Add integration tests with Cosmos DB Emulator
2. ✅ Add performance tests
3. ✅ Increase code coverage to 80%+
4. ✅ Add mutation testing
5. ✅ Add contract testing

---

**All tests use Moq for mocking and follow xUnit best practices!** ✅
