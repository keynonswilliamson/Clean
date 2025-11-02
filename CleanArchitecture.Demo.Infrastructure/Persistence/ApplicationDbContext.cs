using CleanArchitecture.Demo.Domain.Entities;
using CleanArchitecture.Demo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanArchitecture.Demo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
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
}

