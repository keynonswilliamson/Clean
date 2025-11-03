using FluentValidation;

namespace CleanArchitecture.Demo.Application.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Total)
            .GreaterThan(0).WithMessage("Total must be greater than zero.")
            .LessThanOrEqualTo(999999.99m).WithMessage("Total must not exceed 999,999.99.");
    }
}

