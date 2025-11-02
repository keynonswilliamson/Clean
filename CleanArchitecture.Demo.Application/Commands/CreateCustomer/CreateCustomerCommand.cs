using MediatR;

namespace CleanArchitecture.Demo.Application.Commands.CreateCustomer;

public record CreateCustomerCommand(string Name, string Email) : IRequest<Guid>;

