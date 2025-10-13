using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class CustomerTests
{
    [Fact]
    public void Customer_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        Assert.NotNull(customer.FirstName);
        Assert.Empty(customer.FirstName);
        Assert.NotNull(customer.LastName);
        Assert.Empty(customer.LastName);
        Assert.NotNull(customer.Email);
        Assert.Empty(customer.Email);
        Assert.NotNull(customer.Id);
        Assert.NotEmpty(customer.Id);
    }

    [Fact]
    public void Customer_ShouldSetProperties_Correctly()
    {
        // Arrange & Act
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1234567890",
            Address = "123 Main St"
        };

        // Assert
        Assert.Equal("John", customer.FirstName);
        Assert.Equal("Doe", customer.LastName);
        Assert.Equal("john.doe@example.com", customer.Email);
        Assert.Equal("+1234567890", customer.PhoneNumber);
        Assert.Equal("123 Main St", customer.Address);
    }

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("jane.doe@company.com")]
    [InlineData("user+tag@domain.co.uk")]
    public void Customer_ShouldAcceptValidEmails(string email)
    {
        // Arrange & Act
        var customer = new Customer { Email = email };

        // Assert
        Assert.Equal(email, customer.Email);
    }
}
