using CleanArchitecture.Demo.Domain.Common;

namespace CleanArchitecture.Demo.Domain.Entities;

public class Order : EntityBase
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public DateTime OrderDate { get; private set; }
    public decimal Total { get; private set; }

    private Order() { } // EF Core

    public Order(Guid customerId, decimal total)
    {
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        Total = total;
    }

    public void UpdateTotal(decimal total)
    {
        Total = total;
    }
}

