using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OptimisticConcurrencyDemo.Contracts;
using OptimisticConcurrencyDemo.Data;
using OptimisticConcurrencyDemo.Models;

namespace OptimisticConcurrencyDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OptimisticConcurrencyDemoDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OptimisticConcurrencyDemoDbContext context,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateOrder(
        [FromBody] UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 3;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if (order is null)
            {
                return NotFound(new { message = $"OrderId {request.OrderId} not found" });
            }

            ApplyUpdate(request, order);

            try
            {
                // Test-only delay to increase overlap window for concurrent writers
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new
                {
                    message = "Order updated",
                    attempt,
                    order.OrderId,
                    order.ProductId,
                    order.Quantity,
                    order.Status
                });
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict detected for OrderId {OrderId}. Retry attempt {Attempt}/{MaxRetries}",
                    request.OrderId,
                    attempt,
                    maxRetries);

                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is not Order)
                    {
                        continue;
                    }

                    var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                    if (databaseValues is null)
                    {
                        return NotFound(new { message = $"OrderId {request.OrderId} was deleted by another transaction" });
                    }

                    entry.OriginalValues.SetValues(databaseValues);
                }

                // Small delay helps avoid immediate collision with the other writer.
                await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(
                    ex,
                    "Order update failed after retries due to concurrency conflicts for OrderId {OrderId}",
                    request.OrderId);

                return Conflict(new
                {
                    message = "Order update failed due to concurrent updates. Please retry.",
                    request.OrderId
                });
            }
        }

        return Conflict(new
        {
            message = "Order update failed due to concurrent updates. Please retry.",
            request.OrderId
        });
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrder(int orderId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        return order is null
            ? NotFound(new { message = $"OrderId {orderId} not found" })
            : Ok(order);
    }

    private static void ApplyUpdate(UpdateOrderRequest request, Order order)
    {
        order.ProductId = request.ProductId;
        order.Quantity = request.Quantity;
        order.Status = request.Status;
    }
}

