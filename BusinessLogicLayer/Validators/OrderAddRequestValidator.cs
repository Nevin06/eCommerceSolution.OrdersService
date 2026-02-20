using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Validators;

public class OrderAddRequestValidator : AbstractValidator<OrderAddRequest>
{
    public OrderAddRequestValidator()
    {
        RuleFor(x => x.UserID).NotEmpty().WithErrorCode("User ID can't be empty");
        RuleFor(x => x.OrderDate).NotEmpty().WithErrorCode("Order Date can't be empty");
        RuleFor(x => x.OrderItems).NotEmpty().WithErrorCode("Order Items can't be empty");
    }
}
