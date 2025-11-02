namespace CleanArchitecture.Demo.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

