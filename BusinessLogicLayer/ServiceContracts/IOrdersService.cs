using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.ServiceContracts;

public interface IOrdersService
{
    Task<List<OrderResponse>> GetOrdersAsync();
    Task<List<OrderResponse?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter);
    Task<OrderResponse?> GetOrderByConditionAsync(FilterDefinition<Order> filter);
    Task<OrderResponse?> AddOrderAsync(OrderAddRequest orderAddRequest);
    Task<OrderResponse?> UpdateOrderAsync(OrderUpdateRequest orderUpdateRequest);
    Task<bool> DeleteOrderAsync(Guid OrderID);
}
