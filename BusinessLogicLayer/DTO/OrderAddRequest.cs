namespace eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;

public record OrderAddRequest(Guid UserID, DateTime OrderDate, List<OrderItemAddRequest> OrderItems)
{
    public OrderAddRequest(): this(default, default, default) // constructor chaining to call parameterized constructor of same record
    {

    } // creating parameterless constructor so that automapper can create object of this record // 88
}
