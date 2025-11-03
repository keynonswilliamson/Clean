using CleanArchitecture.Demo.Application.DTOs;
using CleanArchitecture.Demo.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Demo.Application.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;

    public GetCustomerByIdQueryHandler(
        IApplicationDbContext context,
        ILogger<GetCustomerByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting customer by ID: {CustomerId}", request.Id);

        var customer = await _context.GetCustomerByIdAsync(request.Id, cancellationToken);
        
        if (customer == null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", request.Id);
            return null;
        }

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            CreatedOn = customer.CreatedOn,
            Orders = customer.Orders.Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                Total = o.Total
            }).ToList()
        };
    }
}

