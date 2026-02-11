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

    // Deadlock demo locks: two in-memory "resources" to simulate contention
    private static readonly object ResourceALock = new();
    private static readonly object ResourceBLock = new();

    // Fixed-version locks using SemaphoreSlim with timeouts (deadlock-avoidance demo)
    private static readonly System.Threading.SemaphoreSlim ResourceASemaphore = new(1, 1);
    private static readonly System.Threading.SemaphoreSlim ResourceBSemaphore = new(1, 1);

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

    /// <summary>
    /// POST /api/orders/deadlock/a
    /// Intentionally creates a deadlock by locking Resource A then Resource B.
    /// When called concurrently with DeadlockScenarioB, both requests can hang.
    /// </summary>
    [HttpPost("deadlock/a")]
    public IActionResult DeadlockScenarioA()
    {
        _logger.LogInformation("DeadlockScenarioA starting - attempting to lock Resource A then Resource B");

        lock (ResourceALock)
        {
            _logger.LogInformation("DeadlockScenarioA acquired Resource A, simulating work before locking Resource B");

            // Simulate some work while holding Resource A
            Thread.Sleep(4000);

            _logger.LogInformation("DeadlockScenarioA attempting to acquire Resource B");

            lock (ResourceBLock)
            {
                _logger.LogInformation("DeadlockScenarioA acquired Resource B");
                return Ok(new { message = "DeadlockScenarioA completed (this should rarely be seen when B runs concurrently)" });
            }
        }
    }

    /// <summary>
    /// POST /api/orders/deadlock/b
    /// Intentionally creates a deadlock by locking Resource B then Resource A.
    /// When called concurrently with DeadlockScenarioA, both requests can hang.
    /// </summary>
    [HttpPost("deadlock/b")]
    public IActionResult DeadlockScenarioB()
    {
        _logger.LogInformation("DeadlockScenarioB starting - attempting to lock Resource B then Resource A");

        lock (ResourceBLock)
        {
            _logger.LogInformation("DeadlockScenarioB acquired Resource B, simulating work before locking Resource A");

            // Simulate some work while holding Resource B
            Thread.Sleep(4000);

            _logger.LogInformation("DeadlockScenarioB attempting to acquire Resource A");

            lock (ResourceALock)
            {
                _logger.LogInformation("DeadlockScenarioB acquired Resource A");
                return Ok(new { message = "DeadlockScenarioB completed (this should rarely be seen when A runs concurrently)" });
            }
        }
    }

    /// <summary>
    /// POST /api/orders/deadlock/fixed-a
    /// Deadlock-safe version: always acquires locks in the same order with timeouts.
    /// Demonstrates how to avoid deadlocks in real systems.
    /// </summary>
    [HttpPost("deadlock/fixed-a")]
    public async Task<IActionResult> DeadlockScenarioFixedA(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeadlockScenarioFixedA starting - attempting to acquire A then B with timeouts");

        var acquiredA = false;
        var acquiredB = false;

        try
        {
            // Always acquire Resource A first
            acquiredA = await ResourceASemaphore.WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);
            if (!acquiredA)
            {
                _logger.LogWarning("DeadlockScenarioFixedA: could not acquire Resource A within timeout");
                return StatusCode(409, new { message = "Could not acquire Resource A, please retry later" });
            }

            _logger.LogInformation("DeadlockScenarioFixedA acquired Resource A, now attempting Resource B");

            acquiredB = await ResourceBSemaphore.WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);
            if (!acquiredB)
            {
                _logger.LogWarning("DeadlockScenarioFixedA: could not acquire Resource B within timeout");
                return StatusCode(409, new { message = "Could not acquire Resource B, please retry later" });
            }

            _logger.LogInformation("DeadlockScenarioFixedA acquired Resource B, simulating work");
            await Task.Delay(500, cancellationToken);

            return Ok(new { message = "DeadlockScenarioFixedA completed without deadlock" });
        }
        finally
        {
            if (acquiredB)
            {
                ResourceBSemaphore.Release();
            }

            if (acquiredA)
            {
                ResourceASemaphore.Release();
            }
        }
    }

    /// <summary>
    /// POST /api/orders/deadlock/fixed-b
    /// Also uses deadlock-safe approach: still acquires A then B to keep global lock ordering consistent.
    /// </summary>
    [HttpPost("deadlock/fixed-b")]
    public async Task<IActionResult> DeadlockScenarioFixedB(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeadlockScenarioFixedB starting - attempting to acquire A then B with timeouts");

        var acquiredA = false;
        var acquiredB = false;

        try
        {
            // Even though this is the "B" path, we still acquire A then B to avoid deadlock
            acquiredA = await ResourceASemaphore.WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);
            if (!acquiredA)
            {
                _logger.LogWarning("DeadlockScenarioFixedB: could not acquire Resource A within timeout");
                return StatusCode(409, new { message = "Could not acquire Resource A, please retry later" });
            }

            _logger.LogInformation("DeadlockScenarioFixedB acquired Resource A, now attempting Resource B");

            acquiredB = await ResourceBSemaphore.WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);
            if (!acquiredB)
            {
                _logger.LogWarning("DeadlockScenarioFixedB: could not acquire Resource B within timeout");
                return StatusCode(409, new { message = "Could not acquire Resource B, please retry later" });
            }

            _logger.LogInformation("DeadlockScenarioFixedB acquired Resource B, simulating work");
            await Task.Delay(500, cancellationToken);

            return Ok(new { message = "DeadlockScenarioFixedB completed without deadlock" });
        }
        finally
        {
            if (acquiredB)
            {
                ResourceBSemaphore.Release();
            }

            if (acquiredA)
            {
                ResourceASemaphore.Release();
            }
        }
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

