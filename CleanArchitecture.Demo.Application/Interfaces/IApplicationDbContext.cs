using CleanArchitecture.Demo.Domain.Entities;

namespace CleanArchitecture.Demo.Application.Interfaces;

public interface IApplicationDbContext
{
    Task<Customer?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetCustomersPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountCustomersAsync(CancellationToken cancellationToken = default);
    Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

