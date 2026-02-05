using Microsoft.AspNetCore.Mvc;
using CQRS.Orders.Application.Commands;
using CQRS.Orders.Application.Queries;

namespace CQRS.Orders.API.Controllers;

/// <summary>
/// Order Controller - Demonstrates CQRS pattern
/// 
/// CQRS Separation:
/// - POST endpoint uses Command (write operation)
/// - GET endpoint uses Query (read operation)
/// - Commands and Queries are completely separated
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly CreateOrderCommandHandler _commandHandler;
    private readonly GetOrdersQueryHandler _queryHandler;

    public OrderController(
        CreateOrderCommandHandler commandHandler,
        GetOrdersQueryHandler queryHandler)
    {
        _commandHandler = commandHandler;
        _queryHandler = queryHandler;
    }

    /// <summary>
    /// Create Order - WRITE operation (Command)
    /// Uses CreateOrderCommand which goes through Domain + Repository
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            var orderId = await _commandHandler.HandleAsync(command);
            return Ok(new { OrderId = orderId, Message = "Order created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Get Orders - READ operation (Query)
    /// Uses GetOrdersQuery which directly reads data and returns DTOs
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var query = new GetOrdersQuery();
        var orders = await _queryHandler.HandleAsync(query);
        return Ok(orders);
    }
}

