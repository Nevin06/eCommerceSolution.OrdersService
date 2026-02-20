namespace eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;

public record OrderResponse(Guid OrderID, Guid UserID, DateTime OrderDate, decimal TotalBill, List<OrderItemResponse> OrderItems)
{
    public OrderResponse() : this(default, default, default, default, default) // constructor chaining to call parameterized constructor of same record
    {

    } // creating parameterless constructor so that automapper can create object of this record // 88
}
