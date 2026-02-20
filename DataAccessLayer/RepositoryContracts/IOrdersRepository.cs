using eCommerce.OrdersMicroService.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroService.DataAccessLayer.RepositoryContracts;

public interface IOrdersRepository
{
    Task<IEnumerable<Order>> GetOrdersAsync();
    Task<IEnumerable<Order?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter);
    Task<Order?> GetOrderByConditionAsync(FilterDefinition<Order> filter);
    Task<Order?> AddOrderAsync(Order order);
    Task<Order?> UpdateOrderAsync(Order order);
    Task<bool> DeleteOrderAsync(Guid orderID);

}
