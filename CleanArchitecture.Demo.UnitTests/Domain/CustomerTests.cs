using CleanArchitecture.Demo.Domain.Entities;
using CleanArchitecture.Demo.Domain.Events;
using Xunit;

namespace CleanArchitecture.Demo.UnitTests.Domain;

public class CustomerTests
{
    [Fact]
    public void Constructor_ShouldCreateCustomer_WithValidData()
    {
        // Arrange
        var name = "John Doe";
        var email = "john.doe@example.com";

        // Act
        var customer = new Customer(name, email);

        // Assert
        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal(name, customer.Name);
        Assert.Equal(email, customer.Email);
        Assert.True(customer.CreatedOn <= DateTime.UtcNow);
        Assert.NotNull(customer.Orders);
        Assert.Contains(customer.DomainEvents, e => e is CustomerCreatedEvent);
    }

    [Fact]
    public void AddOrder_ShouldAddOrder_ToCustomer()
    {
        // Arrange
        var customer = new Customer("John Doe", "john.doe@example.com");
        var order = new Order(customer.Id, 100.00m);

        // Act
        customer.AddOrder(order);

        // Assert
        Assert.Single(customer.Orders);
        Assert.Equal(order, customer.Orders.First());
    }
}

