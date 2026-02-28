namespace eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;

//public record OrderItemResponse(Guid ProductID, decimal UnitPrice, int Quantity, decimal TotalPrice)
public record OrderItemResponse(Guid ProductID, decimal UnitPrice, int Quantity, decimal TotalPrice, string ProductName, string Category) //111
{
    public OrderItemResponse() : this(default, default, default, default, default, default)
    {

    }
}