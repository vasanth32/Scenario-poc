using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/orders
    /// Get all orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        _logger.LogInformation("Getting all orders");

        var orders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// GET /api/orders/{id}
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        _logger.LogInformation("Getting order with ID: {OrderId}", id);

        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", id);
            return NotFound();
        }

        return Ok(order);
    }

    /// <summary>
    /// POST /api/orders
    /// Create a new order (asynchronous processing)
    /// Returns 202 Accepted - order is queued for background processing
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("Creating new order for customer: {CustomerName}", request.CustomerName);

        var order = new Order
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            TotalAmount = request.TotalAmount,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            Status = "Pending" // Will be processed asynchronously by OrderProcessingService
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order created with ID: {OrderId} - Status: Pending (will be processed asynchronously)", order.Id);

        // Return 202 Accepted - order accepted for processing
        // Client can poll /api/orders/{id} to check status
        return AcceptedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            new
            {
                message = "Order accepted for processing",
                orderId = order.Id,
                status = order.Status,
                statusUrl = $"/api/orders/{order.Id}"
            });
    }
}

public class CreateOrderRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}

