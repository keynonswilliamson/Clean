using CleanArchitecture.Demo.Application.Interfaces;
using CleanArchitecture.Demo.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Demo.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for customer ID: {CustomerId} with total: {Total}", 
            request.CustomerId, request.Total);

        // Verify customer exists
        var customer = await _context.GetCustomerByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID '{request.CustomerId}' not found.");
        }

        var order = new Order(request.CustomerId, request.Total);
        
        // Add order to customer's collection
        customer.AddOrder(order);
        
        // Save changes
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order created with ID: {OrderId}", order.Id);

        return order.Id;
    }
}

