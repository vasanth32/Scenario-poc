using DeadlockDemo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeadlockDemo.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TestController> _logger;

    public TestController(IServiceScopeFactory scopeFactory, ILogger<TestController> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/test/deadlock
    /// Runs PlaceOrder and CancelOrder concurrently to try to trigger a SQL deadlock.
    /// Returns information about whether a deadlock / transient exception occurred.
    /// </summary>
    [HttpPost("deadlock")]
    public async Task<IActionResult> TriggerDeadlock(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting deadlock simulation: running PlaceOrder and CancelOrder concurrently");

        var exceptions = new List<Exception>();

        var placeTask = Task.Run(() => RunPlaceAsync(cancellationToken), cancellationToken);
        var cancelTask = Task.Run(() => RunCancelAsync(cancellationToken), cancellationToken);

        try
        {
            await Task.WhenAll(placeTask, cancelTask);
        }
        catch
        {
            // Collect exceptions from both tasks
            if (placeTask.Exception is not null)
            {
                exceptions.AddRange(placeTask.Exception.InnerExceptions);
            }

            if (cancelTask.Exception is not null)
            {
                exceptions.AddRange(cancelTask.Exception.InnerExceptions);
            }
        }

        if (exceptions.Count == 0)
        {
            _logger.LogInformation("Deadlock simulation completed: both operations finished without exception");
            return Ok(new
            {
                message = "Both PlaceOrder and CancelOrder completed without exception",
                hadDeadlock = false
            });
        }

        // Look for SQL deadlock / transient errors
        var messages = exceptions.Select(e => e.Message).ToArray();

        _logger.LogWarning("Deadlock simulation encountered exceptions: {Messages}", messages);

        return Ok(new
        {
            message = "Deadlock simulation completed with exceptions",
            hadDeadlock = true,
            errors = messages
        });
    }

    private async Task RunPlaceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeadlockDemoDbContext>();

        // Use a fresh DbContext and explicit transaction as in PlaceOrder
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = await context.Orders
                .OrderBy(o => o.OrderId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Seed order not found for Place in test");

            var inventory = await context.Inventory
                .OrderBy(i => i.ProductId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Inventory not found for Place in test");

            // Inventory -> delay -> Orders (same as /place)
            inventory.ReservedQty += order.Quantity;
            await context.SaveChangesAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            order.Status = "PlacedAgain";
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task RunCancelAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeadlockDemoDbContext>();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = await context.Orders
                .OrderBy(o => o.OrderId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Seed order not found for Cancel in test");

            var inventory = await context.Inventory
                .OrderBy(i => i.ProductId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Inventory not found for Cancel in test");

            // Orders -> delay -> Inventory (same as /cancel)
            order.Status = "Cancelled";
            await context.SaveChangesAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            inventory.ReservedQty -= order.Quantity;
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}


