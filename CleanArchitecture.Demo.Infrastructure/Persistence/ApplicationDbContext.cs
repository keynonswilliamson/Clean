using CleanArchitecture.Demo.Application.Interfaces;
using CleanArchitecture.Demo.Domain.Entities;
using CleanArchitecture.Demo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanArchitecture.Demo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publish domain events before saving (if you add an event dispatcher)
        // This is a placeholder for domain event dispatching
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    // IApplicationDbContext implementation
    public async Task<Customer?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Customers
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    public async Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await Customers.AddAsync(customer, cancellationToken);
    }

    public async Task<List<Customer>> GetCustomersPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await Customers
            .Include(c => c.Orders)
            .OrderBy(c => c.CreatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await Customers.CountAsync(cancellationToken);
    }
}

