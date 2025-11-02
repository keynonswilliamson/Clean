using CleanArchitecture.Demo.Domain.Common;

namespace CleanArchitecture.Demo.Domain.Entities;

public class Customer : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedOn { get; private set; }
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    private Customer() { } // EF Core

    public Customer(string name, string email)
    {
        Name = name;
        Email = email;
        CreatedOn = DateTime.UtcNow;
        
        // Raise domain event
        AddDomainEvent(new Domain.Events.CustomerCreatedEvent(Id, Name, Email));
    }

    public void AddOrder(Order order)
    {
        Orders.Add(order);
    }
}

