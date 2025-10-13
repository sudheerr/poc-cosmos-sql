using API.Controllers;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace API.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<ProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        // Note: ProductRepository requires ICosmosDbService, so we mock that too
        var mockCosmosDbService = new Mock<Application.Interfaces.ICosmosDbService>();
        mockCosmosDbService.Setup(s => s.GetContainer(It.IsAny<string>()))
            .Returns(Mock.Of<Microsoft.Azure.Cosmos.Container>());

        _mockRepository = new Mock<ProductRepository>(mockCosmosDbService.Object);
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = "1", Name = "Product 1", Price = 10 },
            new Product { Id = "2", Name = "Product 2", Price = 20 }
        };

        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
    }

    [Fact]
    public async Task GetAll_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsOkResult()
    {
        // Arrange
        var productId = "123";
        var product = new Product { Id = productId, Name = "Test Product", Price = 99.99m };

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(productId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal(productId, returnedProduct.Id);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = "nonexistent";

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetById(productId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains(productId, notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetByCategory_ReturnsOkResult_WithFilteredProducts()
    {
        // Arrange
        var category = "Electronics";
        var products = new List<Product>
        {
            new Product { Id = "1", Name = "Laptop", Category = category },
            new Product { Id = "2", Name = "Phone", Category = category }
        };

        _mockRepository.Setup(r => r.GetProductsByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetByCategory(category, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
        Assert.All(returnedProducts, p => Assert.Equal(category, p.Category));
    }

    [Fact]
    public async Task GetInStock_ReturnsOkResult_WithInStockProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = "1", Name = "Product 1", Stock = 10, IsActive = true },
            new Product { Id = "2", Name = "Product 2", Stock = 5, IsActive = true }
        };

        _mockRepository.Setup(r => r.GetProductsInStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetInStock(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
    }

    [Fact]
    public async Task Search_WithParameters_ReturnsFilteredProducts()
    {
        // Arrange
        var searchTerm = "laptop";
        decimal minPrice = 500;
        decimal maxPrice = 2000;
        var products = new List<Product>
        {
            new Product { Id = "1", Name = "Gaming Laptop", Price = 1500 }
        };

        _mockRepository.Setup(r => r.SearchProductsAsync(
                searchTerm, minPrice, maxPrice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.Search(searchTerm, minPrice, maxPrice, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Single(returnedProducts);
    }

    [Fact]
    public async Task Create_WithValidProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var product = new Product { Name = "New Product", Price = 99.99m };
        var createdProduct = new Product { Id = "123", Name = "New Product", Price = 99.99m };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(product, CancellationToken.None);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedProduct = Assert.IsType<Product>(createdAtActionResult.Value);
        Assert.Equal("123", returnedProduct.Id);
        Assert.Equal("New Product", returnedProduct.Name);
    }

    [Fact]
    public async Task Update_WithExistingProduct_ReturnsOkResult()
    {
        // Arrange
        var productId = "123";
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Old Name",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedProduct = new Product
        {
            Id = productId,
            Name = "New Name",
            Price = 150
        };

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.Update(productId, updatedProduct, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("New Name", returnedProduct.Name);
    }

    [Fact]
    public async Task Update_WithNonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = "nonexistent";
        var product = new Product { Name = "Test" };

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Update(productId, product, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var productId = "123";

        _mockRepository.Setup(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(productId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = "nonexistent";

        _mockRepository.Setup(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(productId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(productId, notFoundResult.Value?.ToString());
    }
}
