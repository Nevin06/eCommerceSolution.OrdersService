using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.BusinessLogicLayer.HttpClients;
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
    private readonly UsersMicroserviceClient _usersMicroserviceClient;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;

    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator,
        IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
        IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
        UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _mapper = mapper;
        _ordersRepository = ordersRepository;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
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

        //111
        List<ProductDTO> productDTOs = new List<ProductDTO>();
        //TO DO: Add logic for checking if ProductID exists in Products microservice
        foreach (var orderItem in orderAddRequest.OrderItems)
        {
            ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItem.ProductID);
            if (productDTO == null)
            {
                throw new ArgumentException($"Invalid Product ID: {orderItem.ProductID}");
            }
            productDTOs.Add(productDTO);
        }

        //TO DO: Add logic for checking if UserID exists in Users microservice // 104
        UserDTO? userDto = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);
        if (userDto == null)
        {
            throw new ArgumentException("Invalid User ID");
        }

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
        OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);
        //111
        if (addedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in addedOrderResponse.OrderItems)
            {
                //ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                ProductDTO? productDTO = productDTOs.Where(product => product.ProductID == orderItemResponse.ProductID).FirstOrDefault();
                if (productDTO == null)
                    continue;
                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }
        //112
        _mapper.Map<UserDTO, OrderResponse?>(userDto, addedOrderResponse);
        //addedOrderResponse = new OrderResponse(
        //                            addedOrderResponse.OrderID,
        //                            addedOrderResponse.UserID,
        //                            addedOrderResponse.OrderDate,
        //                            addedOrderResponse.TotalBill,
        //                            addedOrderResponse.OrderItems,
        //                            userDto.PersonName ?? "",
        //                            userDto.Email ?? ""
        //                        );

        return addedOrderResponse;
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
        OrderResponse orderResponse =  _mapper.Map<OrderResponse>(order);
        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                if (productDTO == null)
                    continue;
                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }

        UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
        if (userDTO != null)
            _mapper.Map<UserDTO, OrderResponse?>(userDTO, orderResponse);
        return orderResponse;
    }

    public async Task<List<OrderResponse>> GetOrdersAsync()
    {
        IEnumerable<Order> orders = await _ordersRepository.GetOrdersAsync();
        List<Order> ordersList = orders.ToList();
        List<OrderResponse> orderResponses = _mapper.Map<List<OrderResponse>>(ordersList);

        // TO DO: Load ProductName and Category in each OrderItem //111
        foreach (OrderResponse orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                if (productDTO == null)
                    continue;
                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }

            //112
            UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (userDTO != null)
                _mapper.Map<UserDTO, OrderResponse?>(userDTO, orderResponse);
        }

        return orderResponses;
    }

    public async Task<List<OrderResponse?>> GetOrdersByConditionAsync(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByConditionAsync(filter);
        List<Order?> ordersList = orders.ToList();
        List<OrderResponse?> orderResponses = _mapper.Map<List<OrderResponse?>>(ordersList);

        // TO DO: Load ProductName and Category in each OrderItem //111
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                if (productDTO == null)
                    continue;
                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
            //112
            UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (userDTO != null)
                _mapper.Map<UserDTO, OrderResponse?>(userDTO, orderResponse);
        }

        return orderResponses;
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

        List<ProductDTO> productDTOs = new List<ProductDTO>();
        //TO DO: Add logic for checking if ProductID exists in Products microservice
        foreach (var orderItem in orderUpdateRequest.OrderItems)
        {
            ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItem.ProductID);
            if (productDTO == null)
            {
                throw new ArgumentException($"Invalid Product ID: {orderItem.ProductID}");
            }
            productDTOs.Add(productDTO);
        }

        //TO DO: Add logic for checking if UserID exists in Users microservice //104
        UserDTO? userDto = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);
        if (userDto == null)
        {
            throw new ArgumentException("Invalid User ID");
        }        

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
        OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder);
        //111
        if (updatedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
            {
                //ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                ProductDTO? productDTO = productDTOs.Where(product => product.ProductID == orderItemResponse.ProductID).FirstOrDefault();
                if (productDTO == null)
                    continue;
                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }
        //112
        _mapper.Map<UserDTO, OrderResponse?>(userDto, updatedOrderResponse);
        //updatedOrderResponse = new OrderResponse(
        //                            updatedOrderResponse.OrderID,
        //                            updatedOrderResponse.UserID,
        //                            updatedOrderResponse.OrderDate,
        //                            updatedOrderResponse.TotalBill,
        //                            updatedOrderResponse.OrderItems,
        //                            userDto.PersonName ?? "",
        //                            userDto.Email ?? ""
        //                        );


        return updatedOrderResponse;
    }
}
