using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace API.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IDataRepository<Product>> _mockRepository;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockRepository = new Mock<IDataRepository<Product>>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResult_WithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = "1", Name = "Product 1", Price = 100 },
            new Product { Id = "2", Name = "Product 2", Price = 200 }
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
    public async Task GetById_ShouldReturnOkResult_WhenProductExists()
    {
        // Arrange
        var product = new Product { Id = "1", Name = "Test Product", Price = 100 };
        _mockRepository.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetById("1", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("1", returnedProduct.Id);
        Assert.Equal("Test Product", returnedProduct.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetById("999", CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetByCategory_ShouldReturnProducts_MatchingCategory()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = "1", Name = "Product 1", Category = "Electronics", Price = 100 },
            new Product { Id = "2", Name = "Product 2", Category = "Electronics", Price = 200 }
        };
        _mockRepository.Setup(r => r.FindAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetByCategory("Electronics", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WithProduct()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Test Description",
            Price = 150,
            Category = "Electronics"
        };

        var createdProduct = new Product
        {
            Id = "1",
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            Category = createDto.Category
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(createDto, CancellationToken.None);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedProduct = Assert.IsType<Product>(createdAtActionResult.Value);
        Assert.Equal("New Product", returnedProduct.Name);
        Assert.Equal(150, returnedProduct.Price);
    }

    [Fact]
    public async Task Update_ShouldReturnOkResult_WhenProductExists()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = "1",
            Name = "Old Name",
            Price = 100
        };

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 200,
            Category = "Electronics",
            IsActive = true
        };

        _mockRepository.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken ct) => p);

        // Act
        var result = await _controller.Update("1", updateDto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("Updated Name", updatedProduct.Name);
        Assert.Equal(200, updatedProduct.Price);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateProductDto { Name = "Test", Price = 100 };
        _mockRepository.Setup(r => r.GetByIdAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Update("999", updateDto, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenProductExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete("1", CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete("999", CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetCount_ShouldReturnOkResult_WithCount()
    {
        // Arrange
        _mockRepository.Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        // Act
        var result = await _controller.GetCount(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOkResult_WithPagedData()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = "1", Name = "Product 1", Price = 100, IsActive = true },
            new Product { Id = "2", Name = "Product 2", Price = 200, IsActive = true }
        };

        _mockRepository.Setup(r => r.GetPagedAsync(
            1, 10,
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _controller.GetPaged(1, 10, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAll_ShouldReturnInternalServerError_WhenExceptionThrown()
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
}
