using Domain.Entities;
using Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.SqlServer;

/// <summary>
/// Integration tests for SqlServerRepository using InMemory database
/// </summary>
public class SqlServerRepositoryTests : IDisposable
{
    private readonly SqlServerDbContext _context;
    private readonly SqlServerRepository<Product> _repository;

    public SqlServerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SqlServerDbContext(options);
        _repository = new SqlServerRepository<Product>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity_AndSetCreatedAt()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test"
        };

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Name, result.Name);
        Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Name = "Product 1", Price = 10 },
            new Product { Name = "Product 2", Price = 20 },
            new Product { Name = "Product 3", Price = 30 }
        };

        // Act
        var results = await _repository.AddRangeAsync(products);

        // Assert
        Assert.Equal(3, results.Count());
        Assert.All(results, p => Assert.NotEqual(DateTime.MinValue, p.CreatedAt));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var product = new Product { Name = "Test Product", Price = 99.99m };
        await _repository.AddAsync(product);

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync("non-existent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        await _repository.AddAsync(new Product { Name = "Product 1", Price = 10 });
        await _repository.AddAsync(new Product { Name = "Product 2", Price = 20 });
        await _repository.AddAsync(new Product { Name = "Product 3", Price = 30 });

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, results.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        await _repository.AddAsync(new Product { Name = "Electronics 1", Category = "Electronics", Price = 100 });
        await _repository.AddAsync(new Product { Name = "Book 1", Category = "Books", Price = 20 });
        await _repository.AddAsync(new Product { Name = "Electronics 2", Category = "Electronics", Price = 200 });

        // Act
        var results = await _repository.FindAsync(p => p.Category == "Electronics");

        // Assert
        Assert.Equal(2, results.Count());
        Assert.All(results, p => Assert.Equal("Electronics", p.Category));
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatch()
    {
        // Arrange
        await _repository.AddAsync(new Product { Name = "Product 1", Category = "Electronics", Price = 100 });
        await _repository.AddAsync(new Product { Name = "Product 2", Category = "Electronics", Price = 200 });

        // Act
        var result = await _repository.FirstOrDefaultAsync(p => p.Category == "Electronics");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Category);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnNull_WhenNoMatch()
    {
        // Act
        var result = await _repository.FirstOrDefaultAsync(p => p.Name == "NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Query_ShouldSupportLinqOperations()
    {
        // Arrange
        await _repository.AddAsync(new Product { Name = "Product A", Price = 100, Category = "Electronics" });
        await _repository.AddAsync(new Product { Name = "Product B", Price = 200, Category = "Books" });
        await _repository.AddAsync(new Product { Name = "Product C", Price = 300, Category = "Electronics" });

        // Act
        var results = _repository.Query()
            .Where(p => p.Category == "Electronics")
            .Where(p => p.Price > 150)
            .OrderByDescending(p => p.Price)
            .ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("Product C", results[0].Name);
        Assert.Equal(300, results[0].Price);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity_AndSetUpdatedAt()
    {
        // Arrange
        var product = await _repository.AddAsync(new Product { Name = "Original Name", Price = 100 });
        product.Name = "Updated Name";
        product.Price = 150;

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(150, result.Price);
        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task UpdateRangeAsync_ShouldUpdateMultipleEntities()
    {
        // Arrange
        var products = new List<Product>
        {
            await _repository.AddAsync(new Product { Name = "Product 1", Price = 10 }),
            await _repository.AddAsync(new Product { Name = "Product 2", Price = 20 })
        };

        foreach (var product in products)
        {
            product.Price += 10;
        }

        // Act
        var results = await _repository.UpdateRangeAsync(products);

        // Assert
        Assert.Equal(2, results.Count());
        Assert.All(results, p => Assert.NotNull(p.UpdatedAt));
    }

    [Fact]
    public async Task DeleteAsync_ById_ShouldRemoveEntity()
    {
        // Arrange
        var product = await _repository.AddAsync(new Product { Name = "To Delete", Price = 100 });

        // Act
        var deleteResult = await _repository.DeleteAsync(product.Id);
        var getResult = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.True(deleteResult);
        Assert.Null(getResult);
    }

    [Fact]
    public async Task DeleteAsync_ById_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _repository.DeleteAsync("non-existent-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ByEntity_ShouldRemoveEntity()
    {
        // Arrange
        var product = await _repository.AddAsync(new Product { Name = "To Delete", Price = 100 });

        // Act
        var deleteResult = await _repository.DeleteAsync(product);
        var getResult = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.True(deleteResult);
        Assert.Null(getResult);
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldRemoveMultipleEntities()
    {
        // Arrange
        var products = new List<Product>
        {
            await _repository.AddAsync(new Product { Name = "Product 1", Price = 10 }),
            await _repository.AddAsync(new Product { Name = "Product 2", Price = 20 }),
            await _repository.AddAsync(new Product { Name = "Product 3", Price = 30 })
        };

        // Act
        var deletedCount = await _repository.DeleteRangeAsync(products);
        var remainingCount = await _repository.CountAsync();

        // Assert
        Assert.Equal(3, deletedCount);
        Assert.Equal(0, remainingCount);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        await _repository.AddAsync(new Product { Name = "Product 1", Price = 10 });
        await _repository.AddAsync(new Product { Name = "Product 2", Price = 20 });

        // Act
        var count = await _repository.CountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        // Arrange
        await _repository.AddAsync(new Product { Name = "Product 1", Price = 100, IsActive = true });
        await _repository.AddAsync(new Product { Name = "Product 2", Price = 200, IsActive = false });
        await _repository.AddAsync(new Product { Name = "Product 3", Price = 300, IsActive = true });

        // Act
        var count = await _repository.CountAsync(p => p.IsActive);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        // Arrange
        await _repository.AddAsync(new Product { Name = "Existing Product", Price = 100 });

        // Act
        var exists = await _repository.ExistsAsync(p => p.Name == "Existing Product");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        // Act
        var exists = await _repository.ExistsAsync(p => p.Name == "Non-Existent Product");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            await _repository.AddAsync(new Product { Name = $"Product {i}", Price = i * 10 });
        }

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(2, 10);

        // Assert
        Assert.Equal(25, totalCount);
        Assert.Equal(10, items.Count());
    }

    [Fact]
    public async Task GetPagedAsync_WithPredicate_ShouldReturnFilteredPage()
    {
        // Arrange
        for (int i = 1; i <= 20; i++)
        {
            await _repository.AddAsync(new Product
            {
                Name = $"Product {i}",
                Price = i * 10,
                IsActive = i % 2 == 0
            });
        }

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(1, 5, p => p.IsActive);

        // Assert
        Assert.Equal(10, totalCount); // Only 10 active products
        Assert.Equal(5, items.Count());
        Assert.All(items, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task GetPagedAsync_LastPage_ShouldReturnRemainingItems()
    {
        // Arrange
        for (int i = 1; i <= 23; i++)
        {
            await _repository.AddAsync(new Product { Name = $"Product {i}", Price = i * 10 });
        }

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(3, 10);

        // Assert
        Assert.Equal(23, totalCount);
        Assert.Equal(3, items.Count()); // Only 3 items on last page
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
