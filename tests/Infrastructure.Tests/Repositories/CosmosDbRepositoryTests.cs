using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.Azure.Cosmos;
using Moq;
using System.Net;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class CosmosDbRepositoryTests
{
    private readonly Mock<ICosmosDbService> _mockCosmosDbService;
    private readonly Mock<Container> _mockContainer;
    private readonly CosmosDbRepository<Product> _repository;

    public CosmosDbRepositoryTests()
    {
        _mockCosmosDbService = new Mock<ICosmosDbService>();
        _mockContainer = new Mock<Container>();

        _mockCosmosDbService.Setup(s => s.GetContainer(It.IsAny<string>()))
            .Returns(_mockContainer.Object);

        _repository = new CosmosDbRepository<Product>(_mockCosmosDbService.Object, "Products");
    }

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CosmosDbRepository<Product>(null!, "Products"));
    }

    [Fact]
    public void Constructor_WithNullContainerName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CosmosDbRepository<Product>(_mockCosmosDbService.Object, null!));
    }

    [Fact]
    public void Constructor_WithEmptyContainerName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CosmosDbRepository<Product>(_mockCosmosDbService.Object, string.Empty));
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemExists_ReturnsItem()
    {
        // Arrange
        var productId = "123";
        var product = new Product { Id = productId, Name = "Test Product" };

        var mockResponse = new Mock<ItemResponse<Product>>();
        mockResponse.Setup(r => r.Resource).Returns(product);

        _mockContainer.Setup(c => c.ReadItemAsync<Product>(
                productId,
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _repository.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemNotFound_ReturnsNull()
    {
        // Arrange
        var productId = "nonexistent";
        var cosmosException = new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0);

        _mockContainer.Setup(c => c.ReadItemAsync<Product>(
                productId,
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(cosmosException);

        // Act
        var result = await _repository.GetByIdAsync(productId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_SetsCreatedAtAndReturnsEntity()
    {
        // Arrange
        var product = new Product { Name = "New Product", Price = 99.99m };
        var mockResponse = new Mock<ItemResponse<Product>>();
        mockResponse.Setup(r => r.Resource).Returns(product);

        _mockContainer.Setup(c => c.CreateItemAsync(
                It.IsAny<Product>(),
                It.IsAny<PartitionKey?>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(DateTime.MinValue, product.CreatedAt);
        _mockContainer.Verify(c => c.CreateItemAsync(
            It.Is<Product>(p => p.CreatedAt != DateTime.MinValue),
            It.IsAny<PartitionKey?>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_SetsUpdatedAtAndReturnsEntity()
    {
        // Arrange
        var product = new Product
        {
            Id = "123",
            Name = "Updated Product",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var mockResponse = new Mock<ItemResponse<Product>>();
        mockResponse.Setup(r => r.Resource).Returns(product);

        _mockContainer.Setup(c => c.UpsertItemAsync(
                It.IsAny<Product>(),
                It.IsAny<PartitionKey?>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(product.UpdatedAt);
        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.Is<Product>(p => p.UpdatedAt != null),
            It.IsAny<PartitionKey?>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.UpdateAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_ReturnsTrue()
    {
        // Arrange
        var productId = "123";
        var mockResponse = new Mock<ItemResponse<Product>>();

        _mockContainer.Setup(c => c.DeleteItemAsync<Product>(
                productId,
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _repository.DeleteAsync(productId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFound_ReturnsFalse()
    {
        // Arrange
        var productId = "nonexistent";
        var cosmosException = new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0);

        _mockContainer.Setup(c => c.DeleteItemAsync<Product>(
                productId,
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(cosmosException);

        // Act
        var result = await _repository.DeleteAsync(productId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Query_ReturnsQueryable()
    {
        // Arrange
        _mockContainer.Setup(c => c.GetItemLinqQueryable<Product>(
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>(),
                It.IsAny<CosmosLinqSerializerOptions>()))
            .Returns((IQueryable<Product>)new List<Product>().AsQueryable());

        // Act
        var queryable = _repository.Query();

        // Assert
        Assert.NotNull(queryable);
    }
}
