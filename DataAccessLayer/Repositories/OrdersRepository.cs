using eCommerce.OrdersMicroService.DataAccessLayer.Entities;
using eCommerce.OrdersMicroService.DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroService.DataAccessLayer.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _ordersCollection;
    private readonly string collectionName = "orders";
    public OrdersRepository(IMongoDatabase database)
    {
        _ordersCollection = database.GetCollection<Order>(collectionName);
    }
    public async Task<Order?> AddOrderAsync(Order order)
    {
        order.OrderID = Guid.NewGuid();
        order._id = order.OrderID;
        foreach (var orderItem in order.OrderItems)
        {
            orderItem._id = Guid.NewGuid();
        }
        await _ordersCollection.InsertOneAsync(order);
        return order;
    }

    public async Task<bool> DeleteOrderAsync(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder = (await _ordersCollection.FindAsync(filter)).FirstOrDefault();
        if (existingOrder == null) { 
            return false;
        }

        DeleteResult result = await _ordersCollection.DeleteOneAsync(filter);
        //DeleteResult result = await _ordersCollection.DeleteOneAsync(o => o.OrderId == orderID);
        return result.DeletedCount > 0;
    }

    public async Task<Order?> GetOrderByConditionAsync(FilterDefinition<Order> filter)
    {
        return (await _ordersCollection.FindAsync(filter)).FirstOrDefault();
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        return await (await _ordersCollection.FindAsync(Builders<Order>.Filter.Empty)).ToListAsync();
    }

    public async Task<IEnumerable<Order?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter)
    {
        return await (await _ordersCollection.FindAsync(filter)).ToListAsync();
    }

    public async Task<Order?> UpdateOrderAsync(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, order.OrderID);
        Order? existingOrder = (await _ordersCollection.FindAsync(filter)).FirstOrDefault();
        if (existingOrder == null)
        {
            return null;
        }

        // Preserve the _id from the existing order (MongoDB _id is immutable)
        order._id = existingOrder._id;

        // Also preserve _id for OrderItems if they exist
        if (order.OrderItems != null && existingOrder.OrderItems != null)
        {
            for (int i = 0; i < order.OrderItems.Count && i < existingOrder.OrderItems.Count; i++)
            {
                order.OrderItems[i]._id = existingOrder.OrderItems[i]._id;
            }
        }

        ReplaceOneResult replaceOneResult = await _ordersCollection.ReplaceOneAsync(filter, order);
        return replaceOneResult.ModifiedCount > 0 ? existingOrder : null;
    }
}
