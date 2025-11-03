using MediatR;

namespace CleanArchitecture.Demo.Application.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid CustomerId,
    decimal Total
) : IRequest<Guid>;

