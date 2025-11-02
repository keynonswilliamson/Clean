using CleanArchitecture.Demo.Application.DTOs;
using MediatR;

namespace CleanArchitecture.Demo.Application.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;

