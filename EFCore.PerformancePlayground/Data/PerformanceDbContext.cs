using Microsoft.EntityFrameworkCore;

namespace EFCore.PerformancePlayground.Data;

public class PerformanceDbContext : DbContext
{
    public PerformanceDbContext(DbContextOptions<PerformanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Allow explicit ID values for seeding
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasIndex(e => e.Category);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Allow explicit ID values for seeding
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.OrderDate).IsRequired();
            entity.HasIndex(e => e.OrderDate);
            entity.HasMany(e => e.OrderItems)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Allow explicit ID values for seeding
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }

    public void SeedData()
    {
        if (Products.Any())
        {
            return; // Already seeded
        }

        // Seed Products (~500)
        var categories = new[] { "Electronics", "Clothing", "Books", "Home", "Sports", "Toys", "Food", "Automotive" };
        var random = new Random(42); // Fixed seed for reproducibility

        var products = new List<Product>();
        for (int i = 1; i <= 500; i++)
        {
            products.Add(new Product
            {
                Id = i,
                Name = $"Product {i}",
                Category = categories[random.Next(categories.Length)],
                Price = (decimal)(random.NextDouble() * 1000 + 10),
                IsActive = random.Next(10) > 0 // 90% active
            });
        }
        Products.AddRange(products);

        // Seed Orders (~2,000 with items)
        var customerNames = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace", "Henry" };
        var orders = new List<Order>();
        var orderItems = new List<OrderItem>();
        int orderItemId = 1;

        for (int i = 1; i <= 2000; i++)
        {
            var orderDate = DateTime.Now.AddDays(-random.Next(365));
            var order = new Order
            {
                Id = i,
                OrderDate = orderDate,
                CustomerName = customerNames[random.Next(customerNames.Length)]
            };
            orders.Add(order);

            // Each order has 2-10 items
            int itemCount = random.Next(2, 11);
            var orderProducts = products.OrderBy(x => random.Next()).Take(itemCount).ToList();

            foreach (var product in orderProducts)
            {
                orderItems.Add(new OrderItem
                {
                    Id = orderItemId++,
                    OrderId = i,
                    ProductId = product.Id,
                    Quantity = random.Next(1, 5),
                    UnitPrice = product.Price
                });
            }
        }

        Orders.AddRange(orders);
        OrderItems.AddRange(orderItems);

        SaveChanges();
    }
}

