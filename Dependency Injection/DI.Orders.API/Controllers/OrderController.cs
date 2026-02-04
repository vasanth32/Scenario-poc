using Microsoft.AspNetCore.Mvc;
using DI.Orders.Application;
using DI.Orders.Domain;

namespace DI.Orders.API.Controllers;

/// <summary>
/// Order Controller - Demonstrates Dependency Injection
/// 
/// DEPENDENCY INJECTION IN ACTION:
/// ===============================
/// This controller receives OrderService through constructor injection.
/// The DI container creates the service and passes it here.
/// 
/// WITHOUT DEPENDENCY INJECTION (BAD):
/// ====================================
/// ```csharp
/// public class OrderController : ControllerBase
/// {
///     private readonly OrderService _service;
///     
///     public OrderController()
///     {
///         // BAD: Controller creates its own dependencies
///         var repository = new OrderRepository(new OrderDbContext(...));
///         _service = new OrderService(repository);
///         // Problems:
///         // 1. Tight coupling
///         // 2. Hard to test
///         // 3. Can't swap implementations
///         // 4. Manual object creation and disposal
///     }
/// }
/// ```
/// 
/// WITH DEPENDENCY INJECTION (GOOD):
/// ==================================
/// ```csharp
/// public class OrderController : ControllerBase
/// {
///     private readonly OrderService _service;
///     
///     // GOOD: Service is injected via constructor
///     public OrderController(OrderService service)
///     {
///         _service = service;
///         // Benefits:
///         // 1. Loose coupling
///         // 2. Easy to test (inject mock service)
///         // 3. Lifetime managed by DI container
///         // 4. Automatic disposal
///     }
/// }
/// ```
/// 
/// CONSTRUCTOR INJECTION:
/// ======================
/// - Dependencies are provided through constructor parameters
/// - ASP.NET Core automatically resolves and injects dependencies
/// - No need to manually create objects
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    /// <summary>
    /// Constructor Injection
    /// 
    /// The DI container will automatically:
    /// 1. Create OrderService (which needs IOrderRepository)
    /// 2. Create OrderRepository (which needs OrderDbContext)
    /// 3. Create OrderDbContext (Scoped lifetime)
    /// 4. Wire them all together
    /// 5. Pass OrderService to this constructor
    /// 
    /// We don't write any "new" statements!
    /// The framework handles everything!
    /// </summary>
    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
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
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = await _orderService.CreateOrderAsync(
            request.ProductName,
            request.Quantity,
            request.Price
        );

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
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

