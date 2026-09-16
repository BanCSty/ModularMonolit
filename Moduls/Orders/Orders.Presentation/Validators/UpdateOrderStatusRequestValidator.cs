using FluentValidation;
using Orders.Domain.Entities;
using Orders.Presentation.Contracts.Requests;

namespace Orders.Presentation.Validators;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidOrderStatus).WithMessage("Invalid order status");
    }

    private bool BeValidOrderStatus(string status)
    {
        return Enum.TryParse<OrderStatus>(status, true, out _);
    }
}