using Microsoft.AspNetCore.Mvc;
using CleanArch.Orders.Application;
using CleanArch.Orders.Domain;

namespace CleanArch.Orders.API.Controllers;

/// <summary>
/// API Controller - Only contains HTTP concerns
/// 
/// Dependency Flow:
/// - API depends on Application layer (uses OrderService)
/// - API does NOT depend on Infrastructure directly
/// - API does NOT depend on Domain directly (though Application uses Domain)
/// 
/// This is the outermost layer - it only handles HTTP requests/responses
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    // Dependency Injection: Controller receives OrderService from Application layer
    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// GET: api/order
    /// Get all orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// GET: api/order/5
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    /// <summary>
    /// POST: api/order
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(
                request.ProductName,
                request.Quantity,
                request.Price
            );

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// DELETE: api/order/5
    /// Delete an order
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var deleted = await _orderService.DeleteOrderAsync(id);
        
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

/// <summary>
/// DTO (Data Transfer Object) for creating orders
/// API layer can have DTOs to separate API contracts from Domain entities
/// </summary>
public class CreateOrderRequest
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

