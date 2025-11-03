using CleanArchitecture.Demo.Application.DTOs;
using MediatR;

namespace CleanArchitecture.Demo.Application.Queries.GetCustomers;

public record GetCustomersQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<CustomerDto>>;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

