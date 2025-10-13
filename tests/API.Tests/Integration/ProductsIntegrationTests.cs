using Application.DTOs;
using Domain.Entities;
using Infrastructure.SqlServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace API.Tests.Integration;

/// <summary>
/// Integration tests for Products API endpoints
/// Uses WebApplicationFactory to create a test server
/// </summary>
public class ProductsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private readonly SqlServerDbContext _dbContext;

    public ProductsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<SqlServerDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<SqlServerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<SqlServerDbContext>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task GetAll_ShouldReturnProducts_WhenProductsExist()
    {
        // Arrange
        await SeedProducts();

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetById_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        var product = await SeedSingleProduct();

        // Act
        var response = await _client.GetAsync($"/api/products/{product.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var returnedProduct = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(returnedProduct);
        Assert.Equal(product.Id, returnedProduct.Id);
        Assert.Equal(product.Name, returnedProduct.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/products/non-existent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldCreateProduct_AndReturnCreated()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Test Description",
            Price = 199.99m,
            Category = "Electronics"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(createdProduct);
        Assert.Equal(createDto.Name, createdProduct.Name);
        Assert.Equal(createDto.Price, createdProduct.Price);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Update_ShouldUpdateProduct_WhenExists()
    {
        // Arrange
        var product = await SeedSingleProduct();
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 299.99m,
            Category = "Updated Category",
            IsActive = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedProduct = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(updatedProduct);
        Assert.Equal(updateDto.Name, updatedProduct.Name);
        Assert.Equal(updateDto.Price, updatedProduct.Price);
        Assert.False(updatedProduct.IsActive);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateProductDto
        {
            Name = "Test",
            Price = 100
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/products/non-existent-id", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldDeleteProduct_WhenExists()
    {
        // Arrange
        var product = await SeedSingleProduct();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/products/{product.Id}");
        var getResponse = await _client.GetAsync($"/api/products/{product.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/products/non-existent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByCategory_ShouldReturnFilteredProducts()
    {
        // Arrange
        await SeedProducts();

        // Act
        var response = await _client.GetAsync("/api/products/category/Electronics");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.All(products, p => Assert.Equal("Electronics", p.Category));
    }

    [Fact]
    public async Task Search_ShouldReturnFilteredProducts_ByName()
    {
        // Arrange
        await SeedProducts();

        // Act
        var response = await _client.GetAsync("/api/products/search?name=Laptop");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.All(products, p => Assert.Contains("Laptop", p.Name));
    }

    [Fact]
    public async Task Search_ShouldReturnFilteredProducts_ByPriceRange()
    {
        // Arrange
        await SeedProducts();

        // Act
        var response = await _client.GetAsync("/api/products/search?minPrice=50&maxPrice=150");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.All(products, p =>
        {
            Assert.True(p.Price >= 50);
            Assert.True(p.Price <= 150);
        });
    }

    [Fact]
    public async Task GetPaged_ShouldReturnPagedResults()
    {
        // Arrange
        await SeedManyProducts(25);

        // Act
        var response = await _client.GetAsync("/api/products/paged?pageNumber=2&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult>();
        Assert.NotNull(result);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetCount_ShouldReturnCorrectCount()
    {
        // Arrange
        await SeedManyProducts(15);

        // Act
        var response = await _client.GetAsync("/api/products/count");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CountResult>();
        Assert.NotNull(result);
        Assert.Equal(15, result.Count);
    }

    // Helper methods
    private async Task<Product> SeedSingleProduct()
    {
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test",
            IsActive = true
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    private async Task SeedProducts()
    {
        var products = new List<Product>
        {
            new Product { Name = "Laptop", Price = 999.99m, Category = "Electronics" },
            new Product { Name = "Mouse", Price = 29.99m, Category = "Electronics" },
            new Product { Name = "Keyboard", Price = 79.99m, Category = "Electronics" },
            new Product { Name = "Book", Price = 19.99m, Category = "Books" }
        };
        _dbContext.Products.AddRange(products);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedManyProducts(int count)
    {
        var products = Enumerable.Range(1, count).Select(i => new Product
        {
            Name = $"Product {i}",
            Price = i * 10,
            Category = i % 2 == 0 ? "Electronics" : "Books",
            IsActive = true
        }).ToList();

        _dbContext.Products.AddRange(products);
        await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _scope.Dispose();
        _client.Dispose();
    }

    // Helper classes for deserializing responses
    private class PagedResult
    {
        public List<Product>? Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    private class CountResult
    {
        public int Count { get; set; }
    }
}
