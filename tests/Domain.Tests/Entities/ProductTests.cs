using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Product_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        Assert.NotNull(product.Name);
        Assert.Empty(product.Name);
        Assert.NotNull(product.Description);
        Assert.Empty(product.Description);
        Assert.Equal(0, product.Price);
        Assert.NotNull(product.Category);
        Assert.Empty(product.Category);
        Assert.True(product.IsActive);
        Assert.NotNull(product.Id);
        Assert.NotEmpty(product.Id);
    }

    [Fact]
    public void Product_ShouldSetProperties_Correctly()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 1299.99m,
            Category = "Electronics",
            IsActive = true
        };

        // Assert
        Assert.Equal("Laptop", product.Name);
        Assert.Equal("High-performance laptop", product.Description);
        Assert.Equal(1299.99m, product.Price);
        Assert.Equal("Electronics", product.Category);
        Assert.True(product.IsActive);
    }

    [Theory]
    [InlineData("Electronics", 999.99)]
    [InlineData("Books", 29.99)]
    [InlineData("Clothing", 49.99)]
    public void Product_ShouldAcceptDifferentCategories(string category, decimal price)
    {
        // Arrange & Act
        var product = new Product
        {
            Category = category,
            Price = price
        };

        // Assert
        Assert.Equal(category, product.Category);
        Assert.Equal(price, product.Price);
    }

    [Fact]
    public void Product_IsActive_ShouldBeTrue_ByDefault()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Product_Id_ShouldBeUnique_ForMultipleInstances()
    {
        // Arrange & Act
        var product1 = new Product();
        var product2 = new Product();

        // Assert
        Assert.NotEqual(product1.Id, product2.Id);
    }
}
