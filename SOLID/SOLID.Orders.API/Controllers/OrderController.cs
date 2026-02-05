using Microsoft.AspNetCore.Mvc;
using SOLID.Orders.Application;
using SOLID.Orders.Domain;

namespace SOLID.Orders.API.Controllers;

/// <summary>
/// Order Controller
/// 
/// SOLID Principle: DEPENDENCY INVERSION
/// =====================================
/// Controller depends on OrderService (abstraction through concrete class),
/// which in turn depends on interfaces (IOrderRepository, IOrderValidator, etc.).
/// 
/// We depend on abstractions, not concrete implementations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly IPaymentProcessor _paymentProcessor;

    public OrderController(OrderService orderService, IPaymentProcessor paymentProcessor)
    {
        _orderService = orderService;
        _paymentProcessor = paymentProcessor;
    }

    /// <summary>
    /// GET: api/order
    /// Get all orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
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
    /// Demonstrates all SOLID principles in action
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            // OrderService orchestrates using injected dependencies
            // Each dependency follows Single Responsibility Principle
            var order = await _orderService.CreateOrderAsync(
                request.ProductName,
                request.Quantity,
                request.Price
            );

            // Liskov Substitution: Can use any IPaymentProcessor implementation
            var paymentSuccess = await _paymentProcessor.ProcessPaymentAsync(order.TotalAmount);

            if (!paymentSuccess)
            {
                return BadRequest("Payment processing failed");
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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

