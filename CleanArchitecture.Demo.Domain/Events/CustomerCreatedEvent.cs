using CleanArchitecture.Demo.Domain.Common;

namespace CleanArchitecture.Demo.Domain.Events;

public class CustomerCreatedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public string Name { get; }
    public string Email { get; }
    public DateTime OccurredOn { get; }

    public CustomerCreatedEvent(Guid customerId, string name, string email)
    {
        CustomerId = customerId;
        Name = name;
        Email = email;
        OccurredOn = DateTime.UtcNow;
    }
}

