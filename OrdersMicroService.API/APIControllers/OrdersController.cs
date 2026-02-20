using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroService.API.APIControllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;
    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    //GET: /api/Orders
    [HttpGet]
    //public async Task<ActionResult<IEnumerable<OrderResponse?>>> Get()
    // gives you explicit control over status codes while keeping type safety.
    // This becomes important when you need to handle error scenarios //94
    public async Task<IEnumerable<OrderResponse?>> GetOrdersAsync()
    {
        List<OrderResponse> orderResponses = await _ordersService.GetOrdersAsync();
        return orderResponses;
        //if (orderResponses == null) return NotFound();
        //return Ok(orderResponses);
    }

    //GET: /api/Orders/search/orderid/{orderID}
    [HttpGet("search/orderid/{orderID}")]
    public async Task<OrderResponse?> GetOrderByOrderIdConditionAsync(Guid orderID)
    {
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        OrderResponse? orderResponse = await _ordersService.GetOrderByConditionAsync(filterDefinition);
        return orderResponse;
    }

    //GET: /api/Orders/search/productid/{productID}
    [HttpGet("search/productid/{productID}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductIdConditionAsync(Guid productID)
    {
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.ElemMatch(temp => temp.OrderItems, Builders<OrderItem>.Filter.Eq(temp => temp.ProductID, productID));
        List<OrderResponse?> orderResponses = await _ordersService.GetOrdersByConditionAsync(filterDefinition);
        return orderResponses;
    }

    //GET: /api/Orders/search/orderdate/{OrderDate}
    [HttpGet("search/orderdate/{OrderDate}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByOrderDateConditionAsync(DateTime OrderDate)
    {
        // Get the start of the day (00:00:00)
        DateTime startOfDay = OrderDate.Date;
        // Get the start of the next day (exclusive upper bound)
        DateTime startOfNextDay = startOfDay.AddDays(1);
        // Filter orders where OrderDate is >= start of day AND < start of next day
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Gte(temp => temp.OrderDate, startOfDay),
            Builders<Order>.Filter.Lt(temp => temp.OrderDate, startOfNextDay)
        );
        //FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.Eq(temp => temp.OrderDate.ToString("yyy-MM-dd"), OrderDate.ToString("yyy-MM-dd"));
        List<OrderResponse?> orderResponses = await _ordersService.GetOrdersByConditionAsync(filterDefinition);
        return orderResponses;
    }

    // POST: /api/Orders
    [HttpPost]
    public async Task<ActionResult<OrderResponse?>> AddOrderAsync([FromBody] OrderAddRequest orderAddRequest)
    {
        if (orderAddRequest == null)
            return BadRequest(new { Message = "Order request body is required." });

        try
        {
            OrderResponse? created = await _ordersService.AddOrderAsync(orderAddRequest);
            if (created == null)
                //return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Order creation failed." });
                return Problem("Error in adding Product");

            //return CreatedAtAction(
            //    nameof(GetOrderByOrderIdConditionAsync),
            //    new { orderID = created.OrderID },
            //    created);
            return Created($"api/Orders/search/orderid/{created.OrderID}",  created);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // PUT: /api/Orders/{orderID}
    [HttpPut("{orderID}")]
    //public async Task<ActionResult<OrderResponse?>> UpdateOrderAsync([FromBody] OrderUpdateRequest orderUpdateRequest)
    public async Task<ActionResult<OrderResponse?>> UpdateOrderAsync([FromBody] OrderUpdateRequest orderUpdateRequest, Guid orderID)
    {
        if (orderUpdateRequest == null)
            return BadRequest(new { Message = "Order update request body is required." });

        if (orderID != orderUpdateRequest.OrderID)
            return BadRequest(new { Message = $"{orderID} in the URL doesn't match with the {orderUpdateRequest.OrderID} " +
                $"in the request body." });

        try
        {
            OrderResponse? updated = await _ordersService.UpdateOrderAsync(orderUpdateRequest);
            if (updated == null)
                return NotFound(new { Message = $"Order with ID {orderUpdateRequest.OrderID} not found or update failed." });

            return Ok(updated);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // DELETE: /api/Orders/{orderID}
    [HttpDelete("{orderID}")]
    public async Task<ActionResult<OrderResponse?>> DeleteOrderAsync(Guid orderID)
    {
        try
        {
            //if (string.IsNullOrEmpty(orderID.ToString()))
            if (orderID == Guid.Empty)
                return BadRequest(new { Message = "OrderID is required." });

            bool deleted = await _ordersService.DeleteOrderAsync(orderID);

            if (deleted)
            {
                //return NoContent();
                return Ok(true);
            }
            else
            {
                return NotFound($"Order with ID {orderID} not found");
            }
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    //GET: /api/Orders/search/userid/{userID}
    [HttpGet("search/userid/{userID}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByUserIDConditionAsync(Guid userID)
    {
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.Eq(temp => temp.UserID, userID.ToString());
        List<OrderResponse?> orderResponses = await _ordersService.GetOrdersByConditionAsync(filterDefinition);
        return orderResponses;
    }
}
