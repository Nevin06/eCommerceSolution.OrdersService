using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;
using eCommerce.OrdersMicroService.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Services;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;

    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator,
        IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
        IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _mapper = mapper;
        _ordersRepository = ordersRepository;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
    }
    public async Task<OrderResponse?> AddOrderAsync(OrderAddRequest orderAddRequest)
    {
        if (orderAddRequest == null)
        {
            throw new ArgumentNullException(nameof(orderAddRequest));
        }

        // Validation
        var validationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!validationResult.IsValid)
        {
            string errors = string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }

        //Validate order items using Fluent Validation
        foreach (var orderItem in orderAddRequest.OrderItems)
        {
            validationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItem);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }
        }

        //TO DO: Add logic for checking if UserID exists in Users microservice


        Order order = _mapper.Map<Order>(orderAddRequest);
        //Generate values
        foreach (var orderItem in order.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }
        order.TotalBill = order.OrderItems.Sum(x => x.TotalPrice);

        Order? addedOrder = await _ordersRepository.AddOrderAsync(order);
        if (addedOrder == null)
            return null;
        return _mapper.Map<OrderResponse>(addedOrder);
    }

    public async Task<bool> DeleteOrderAsync(Guid OrderID)
    {
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.Eq(temp => temp.OrderID, OrderID);
        Order? existingOrder = await _ordersRepository.GetOrderByConditionAsync(filterDefinition);
        if (existingOrder == null)
        {
            return false;
        }
        return await _ordersRepository.DeleteOrderAsync(OrderID);
    }

    public async Task<OrderResponse?> GetOrderByConditionAsync(FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByConditionAsync(filter);
        if (order == null) //93
            return null;
        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<List<OrderResponse>> GetOrdersAsync()
    {
        IEnumerable<Order> orders = await _ordersRepository.GetOrdersAsync();
        List<Order> ordersList = orders.ToList();
        return _mapper.Map<List<OrderResponse>>(ordersList);
    }

    public async Task<List<OrderResponse?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByConditionAsync(filter);
        List<Order?> ordersList = orders.ToList();
        return _mapper.Map<List<OrderResponse?>>(ordersList);
    }

    public async Task<OrderResponse?> UpdateOrderAsync(OrderUpdateRequest orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }

        // Validation
        var validationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!validationResult.IsValid)
        {
            string errors = string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }

        //Validate order items using Fluent Validation
        foreach (var orderItem in orderUpdateRequest.OrderItems)
        {
            validationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItem);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }
        }

        //TO DO: Add logic for checking if UserID exists in Users microservice

        Order order = _mapper.Map<Order>(orderUpdateRequest);
        //Generate values
        foreach (var orderItem in order.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }
        order.TotalBill = order.OrderItems.Sum(x => x.TotalPrice);
        Order? updatedOrder = await _ordersRepository.UpdateOrderAsync(order);
        if (updatedOrder == null)
            return null;
        return _mapper.Map<OrderResponse>(updatedOrder);
    }
}
