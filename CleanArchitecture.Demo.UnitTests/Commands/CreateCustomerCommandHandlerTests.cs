using CleanArchitecture.Demo.Application.Commands.CreateCustomer;
using CleanArchitecture.Demo.Application.Interfaces;
using CleanArchitecture.Demo.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.Demo.UnitTests.Commands;

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
    public async Task Handle_ShouldCreateCustomer_WhenEmailIsUnique()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john.doe@example.com");
        _contextMock.Setup(c => c.GetCustomerByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);
        _contextMock.Setup(c => c.AddCustomerAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var customerId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, customerId);
        _contextMock.Verify(c => c.GetCustomerByEmailAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(c => c.AddCustomerAsync(It.Is<Customer>(c => c.Email == command.Email), It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john.doe@example.com");
        var existingCustomer = new Customer("Jane Doe", "john.doe@example.com");
        _contextMock.Setup(c => c.GetCustomerByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _contextMock.Verify(c => c.AddCustomerAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

