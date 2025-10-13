using Domain.Entities;
using Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.SqlServer;

public class SqlServerUnitOfWorkTests : IDisposable
{
    private readonly SqlServerDbContext _context;
    private readonly SqlServerUnitOfWork _unitOfWork;
    private readonly SqlServerRepository<Product> _productRepository;
    private readonly SqlServerRepository<Customer> _customerRepository;

    public SqlServerUnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SqlServerDbContext(options);
        _unitOfWork = new SqlServerUnitOfWork(_context);
        _productRepository = new SqlServerRepository<Product>(_context);
        _customerRepository = new SqlServerRepository<Customer>(_context);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var product = new Product { Name = "Test Product", Price = 100 };
        await _context.Products.AddAsync(product);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        var savedProduct = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(savedProduct);
    }

    [Fact]
    public async Task Transaction_ShouldCommit_WhenSuccessful()
    {
        // Arrange
        var product = new Product { Name = "Product 1", Price = 100 };
        var customer = new Customer { FirstName = "John", LastName = "Doe", Email = "john@test.com" };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _productRepository.AddAsync(product);
        await _customerRepository.AddAsync(customer);
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var savedProduct = await _context.Products.FindAsync(product.Id);
        var savedCustomer = await _context.Customers.FindAsync(customer.Id);
        Assert.NotNull(savedProduct);
        Assert.NotNull(savedCustomer);
    }

    [Fact]
    public async Task Transaction_ShouldRollback_WhenFailed()
    {
        // Arrange
        var product = new Product { Name = "Product 1", Price = 100 };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _productRepository.AddAsync(product);
        await _unitOfWork.RollbackTransactionAsync();

        // Assert - Changes should not be persisted after rollback
        // Since we're using InMemory database, the behavior might differ slightly
        // In a real database, this would definitely roll back
        var savedProduct = await _context.Products.FindAsync(product.Id);
        Assert.Null(savedProduct);
    }

    [Fact]
    public async Task Transaction_MultipleOperations_ShouldCommitAll()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Name = "Product 1", Price = 100 },
            new Product { Name = "Product 2", Price = 200 }
        };

        var customers = new List<Customer>
        {
            new Customer { FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new Customer { FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _productRepository.AddRangeAsync(products);
        await _customerRepository.AddRangeAsync(customers);
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var productCount = await _productRepository.CountAsync();
        var customerCount = await _customerRepository.CountAsync();
        Assert.Equal(2, productCount);
        Assert.Equal(2, customerCount);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
