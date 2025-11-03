using CleanArchitecture.Demo.Application.DTOs;
using CleanArchitecture.Demo.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Demo.Application.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedResult<CustomerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetCustomersQueryHandler> _logger;

    public GetCustomersQueryHandler(
        IApplicationDbContext context,
        ILogger<GetCustomersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting customers page {PageNumber} with page size {PageSize}", 
            request.PageNumber, request.PageSize);

        var customers = await _context.GetCustomersPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _context.CountCustomersAsync(cancellationToken);

        var customerDtos = customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            CreatedOn = c.CreatedOn,
            Orders = c.Orders.Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                Total = o.Total
            }).ToList()
        }).ToList();

        return new PaginatedResult<CustomerDto>
        {
            Items = customerDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

