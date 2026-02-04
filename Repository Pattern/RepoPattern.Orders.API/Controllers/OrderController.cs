using Microsoft.AspNetCore.Mvc;
using RepoPattern.Orders.Application;
using RepoPattern.Orders.Domain;

namespace RepoPattern.Orders.API.Controllers;

/// <summary>
/// Order Controller - Demonstrates Repository Pattern
/// 
/// KEY POINT: Controller uses IOrderRepository, NOT DbContext directly!
/// 
/// This is the main benefit of Repository Pattern:
/// - Controller doesn't know about Entity Framework
/// - Controller doesn't know about database details
/// - Controller only knows about the repository interface
/// 
/// If we wanted to switch from SQL Server to MongoDB tomorrow,
/// we would only change the Infrastructure layer, NOT this controller!
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;

    // Dependency Injection: We receive IOrderRepository (interface), not concrete implementation
    // This is Dependency Inversion Principle in action
    public OrderController(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// GET: api/order
    /// Get all orders
    /// Notice: We use _orderRepository, NOT _context.Orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        var orders = await _orderRepository.GetOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// POST: api/order
    /// Add a new order
    /// Notice: We use _orderRepository.AddOrderAsync, NOT _context.Orders.Add()
    /// Controller doesn't need to call SaveChanges() - repository handles it!
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> AddOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Price = request.Price
        };

        var createdOrder = await _orderRepository.AddOrderAsync(order);
        return CreatedAtAction(nameof(GetOrders), createdOrder);
    }
}

/// <summary>
/// DTO for creating orders
/// </summary>
public class CreateOrderRequest
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

