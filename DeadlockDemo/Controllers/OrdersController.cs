using DeadlockDemo.Data;
using DeadlockDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeadlockDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly DeadlockDemoDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(DeadlockDemoDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/orders/place
    /// Deadlock POC - This endpoint updates Inventory first, waits 5 seconds,
    /// then updates Orders in a single database transaction.
    /// When run concurrently with a "cancel" endpoint that updates tables
    /// in the opposite order, SQL Server can detect a deadlock.
    /// </summary>
    [HttpPost("place")]
    public async Task<IActionResult> PlaceOrder(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PlaceOrder starting - loading seed order and inventory");

        // For the POC we always use the first order in the table (seeded at startup)
        var order = await _context.Orders
            .OrderBy(o => o.OrderId)
            .FirstOrDefaultAsync(cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Seed order not found" });
        }

        var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.ProductId == order.ProductId, cancellationToken);
        if (inventory is null)
        {
            return NotFound(new { message = $"Inventory for ProductId {order.ProductId} not found" });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _logger.LogInformation("PlaceOrder - Step 1: updating Inventory (ReservedQty += Quantity)");

            // Step 1: update Inventory first
            inventory.ReservedQty += order.Quantity;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("PlaceOrder - Step 1 completed, simulating 5 second delay before updating Orders");

            // Simulate long-running work while holding locks
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            _logger.LogInformation("PlaceOrder - Step 2: updating Orders status to 'PlacedAgain'");

            // Step 2: update Orders
            order.Status = "PlacedAgain";
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("PlaceOrder completed successfully");

            return Ok(new
            {
                message = "PlaceOrder completed",
                orderId = order.OrderId,
                productId = order.ProductId,
                quantity = order.Quantity,
                reservedQty = inventory.ReservedQty,
                status = order.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PlaceOrder transaction. Rolling back.");
            await transaction.RollbackAsync(cancellationToken);
            return StatusCode(500, new { message = "Error placing order", error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/orders/cancel
    /// Deadlock POC - This endpoint updates Orders first, waits 5 seconds,
    /// then updates Inventory in the same transaction.
    /// When run concurrently with PlaceOrder (Inventory -> Orders),
    /// SQL Server can detect a real deadlock (error 1205).
    /// </summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelOrder(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CancelOrder starting - loading seed order and inventory");

        // Use the same seed order to guarantee both APIs touch the same rows
        var order = await _context.Orders
            .OrderBy(o => o.OrderId)
            .FirstOrDefaultAsync(cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Seed order not found" });
        }

        var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.ProductId == order.ProductId, cancellationToken);
        if (inventory is null)
        {
            return NotFound(new { message = $"Inventory for ProductId {order.ProductId} not found" });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _logger.LogInformation("CancelOrder - Step 1: updating Orders status to 'Cancelled'");

            // Step 1: update Orders first (opposite order from PlaceOrder)
            order.Status = "Cancelled";
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CancelOrder - Step 1 completed, simulating 5 second delay before updating Inventory");

            // Hold locks for a while
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            _logger.LogInformation("CancelOrder - Step 2: updating Inventory (ReservedQty -= Quantity)");

            // Step 2: update Inventory
            inventory.ReservedQty -= order.Quantity;
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("CancelOrder completed successfully");

            return Ok(new
            {
                message = "CancelOrder completed",
                orderId = order.OrderId,
                productId = order.ProductId,
                quantity = order.Quantity,
                reservedQty = inventory.ReservedQty,
                status = order.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CancelOrder transaction. Rolling back.");
            await transaction.RollbackAsync(cancellationToken);
            return StatusCode(500, new { message = "Error cancelling order", error = ex.Message });
        }
    }
}


