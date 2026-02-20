namespace eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;

public record OrderUpdateRequest(Guid OrderID, Guid UserID, DateTime OrderDate, List<OrderItemUpdateRequest> OrderItems)
{
    public OrderUpdateRequest(): this(default, default, default, default) // constructor chaining to call parameterized constructor of same record
    {

    } // creating parameterless constructor so that automapper can create object of this record // 88
}
