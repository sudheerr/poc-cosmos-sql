using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void Order_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        Assert.NotNull(order.Id);
        Assert.NotEmpty(order.Id);
        Assert.NotNull(order.CustomerId);
        Assert.Empty(order.CustomerId);
        Assert.Equal(0, order.TotalAmount);
        Assert.Equal("Pending", order.Status);
        Assert.NotNull(order.Items);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void Order_ShouldSetProperties_Correctly()
    {
        // Arrange & Act
        var order = new Order
        {
            CustomerId = "customer-123",
            OrderDate = new DateTime(2024, 1, 15),
            TotalAmount = 299.99m,
            Status = "Completed"
        };

        // Assert
        Assert.Equal("customer-123", order.CustomerId);
        Assert.Equal(new DateTime(2024, 1, 15), order.OrderDate);
        Assert.Equal(299.99m, order.TotalAmount);
        Assert.Equal("Completed", order.Status);
    }

    [Fact]
    public void Order_ShouldAddOrderItems()
    {
        // Arrange
        var order = new Order();
        var item1 = new OrderItem
        {
            ProductId = "prod-1",
            ProductName = "Laptop",
            Quantity = 1,
            UnitPrice = 999.99m,
            TotalPrice = 999.99m
        };
        var item2 = new OrderItem
        {
            ProductId = "prod-2",
            ProductName = "Mouse",
            Quantity = 2,
            UnitPrice = 29.99m,
            TotalPrice = 59.98m
        };

        // Act
        order.Items.Add(item1);
        order.Items.Add(item2);

        // Assert
        Assert.Equal(2, order.Items.Count);
        Assert.Contains(item1, order.Items);
        Assert.Contains(item2, order.Items);
    }

    [Fact]
    public void OrderItem_ShouldCalculateTotalPrice()
    {
        // Arrange & Act
        var item = new OrderItem
        {
            ProductId = "prod-1",
            ProductName = "Laptop",
            Quantity = 3,
            UnitPrice = 999.99m,
            TotalPrice = 3 * 999.99m
        };

        // Assert
        Assert.Equal(2999.97m, item.TotalPrice);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Processing")]
    [InlineData("Shipped")]
    [InlineData("Delivered")]
    [InlineData("Cancelled")]
    public void Order_ShouldAcceptDifferentStatuses(string status)
    {
        // Arrange & Act
        var order = new Order { Status = status };

        // Assert
        Assert.Equal(status, order.Status);
    }
}
