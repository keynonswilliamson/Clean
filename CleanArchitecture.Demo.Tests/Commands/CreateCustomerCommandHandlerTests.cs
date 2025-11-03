using CleanArchitecture.Demo.Application.Commands.CreateCustomer;
using CleanArchitecture.Demo.Application.Interfaces;
using CleanArchitecture.Demo.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.Demo.Tests.Commands;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<CreateCustomerCommandHandler>> _loggerMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<CreateCustomerCommandHandler>>();
        _handler = new CreateCustomerCommandHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomerId_WhenEmailIsUnique()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john.doe@example.com");
        
        _contextMock
            .Setup(c => c.GetCustomerByEmailAsync(It.Is<string>(e => e == command.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);
        
        _contextMock
            .Setup(c => c.AddCustomerAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty("because a new customer should have a valid ID");
        result.Should().NotBe(Guid.Empty, "because the ID should not be empty");
        
        _contextMock.Verify(
            c => c.GetCustomerByEmailAsync(command.Email, It.IsAny<CancellationToken>()), 
            Times.Once, 
            "because we need to check if the email already exists");
        
        _contextMock.Verify(
            c => c.AddCustomerAsync(
                It.Is<Customer>(c => c.Email == command.Email && c.Name == command.Name), 
                It.IsAny<CancellationToken>()), 
            Times.Once, 
            "because the customer should be added to the context");
        
        _contextMock.Verify(
            c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once, 
            "because changes should be persisted");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john.doe@example.com");
        var existingCustomer = new Customer("Jane Doe", "john.doe@example.com");
        
        _contextMock
            .Setup(c => c.GetCustomerByEmailAsync(It.Is<string>(e => e == command.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"A customer with email '{command.Email}' already exists.");
        
        _contextMock.Verify(
            c => c.GetCustomerByEmailAsync(command.Email, It.IsAny<CancellationToken>()), 
            Times.Once, 
            "because we need to check if the email already exists");
        
        _contextMock.Verify(
            c => c.AddCustomerAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), 
            Times.Never, 
            "because a customer should not be added when email already exists");
        
        _contextMock.Verify(
            c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never, 
            "because no changes should be saved when email already exists");
    }
}

