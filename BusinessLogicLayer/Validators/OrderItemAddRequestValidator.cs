using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Validators;

public class OrderItemAddRequestValidator : AbstractValidator<OrderItemAddRequest>
{
    public OrderItemAddRequestValidator()
    {
        RuleFor(x => x.ProductID).NotEmpty().WithErrorCode("ProductID cannot be empty");
        RuleFor(x => x.UnitPrice).NotEmpty().WithErrorCode("Unit Price cannot be empty")
            .GreaterThan(0).WithErrorCode("Unit Price cannot be less than or equal to zero");
        RuleFor(x => x.Quantity).NotEmpty().WithErrorCode("Quantity cannot be empty")
            .GreaterThan(0).WithErrorCode("Quantity cannot be less than or equal to zero");
    }
}
