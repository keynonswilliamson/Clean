using CleanArchitecture.Demo.Application.DTOs;
using CleanArchitecture.Demo.Application.Interfaces;
using CleanArchitecture.Demo.Application.Queries.GetCustomerById;
using CleanArchitecture.Demo.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.Demo.Tests.Queries;

public class GetCustomerByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<GetCustomerByIdQueryHandler>> _loggerMock;
    private readonly GetCustomerByIdQueryHandler _handler;

    public GetCustomerByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<GetCustomerByIdQueryHandler>>();
        _handler = new GetCustomerByIdQueryHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomerWithOrders_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(customerId);
        
        var customer = new Customer("John Doe", "john.doe@example.com");
        customer.GetType().GetProperty("Id")!.SetValue(customer, customerId);
        
        var order1 = new Order(customerId, 100.50m);
        var order2 = new Order(customerId, 250.75m);
        customer.AddOrder(order1);
        customer.AddOrder(order2);
        
        _contextMock
            .Setup(c => c.GetCustomerByIdAsync(It.Is<Guid>(id => id == customerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull("because the customer exists");
        result.Should().BeOfType<CustomerDto>("because the handler should return a CustomerDto");
        
        result!.Id.Should().Be(customerId, "because the customer ID should match");
        result.Name.Should().Be("John Doe", "because the customer name should match");
        result.Email.Should().Be("john.doe@example.com", "because the customer email should match");
        
        result.Orders.Should().NotBeNull("because orders collection should be initialized");
        result.Orders.Should().HaveCount(2, "because the customer has two orders");
        
        result.Orders[0].CustomerId.Should().Be(customerId, "because the order should belong to the customer");
        result.Orders[0].Total.Should().Be(100.50m, "because the first order total should match");
        
        result.Orders[1].CustomerId.Should().Be(customerId, "because the order should belong to the customer");
        result.Orders[1].Total.Should().Be(250.75m, "because the second order total should match");
        
        _contextMock.Verify(
            c => c.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()), 
            Times.Once, 
            "because we need to retrieve the customer from the context");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(customerId);
        
        _contextMock
            .Setup(c => c.GetCustomerByIdAsync(It.Is<Guid>(id => id == customerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull("because the customer does not exist");
        
        _contextMock.Verify(
            c => c.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()), 
            Times.Once, 
            "because we need to check if the customer exists in the context");
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomerWithoutOrders_WhenCustomerHasNoOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(customerId);
        
        var customer = new Customer("John Doe", "john.doe@example.com");
        customer.GetType().GetProperty("Id")!.SetValue(customer, customerId);
        
        _contextMock
            .Setup(c => c.GetCustomerByIdAsync(It.Is<Guid>(id => id == customerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull("because the customer exists");
        result!.Id.Should().Be(customerId, "because the customer ID should match");
        result.Name.Should().Be("John Doe", "because the customer name should match");
        result.Email.Should().Be("john.doe@example.com", "because the customer email should match");
        
        result.Orders.Should().NotBeNull("because orders collection should be initialized");
        result.Orders.Should().BeEmpty("because the customer has no orders");
        
        _contextMock.Verify(
            c => c.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()), 
            Times.Once, 
            "because we need to retrieve the customer from the context");
    }
}

