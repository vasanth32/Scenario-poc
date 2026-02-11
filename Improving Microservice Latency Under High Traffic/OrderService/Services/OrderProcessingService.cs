using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

/// <summary>
/// Background service for processing orders asynchronously.
/// Handles long-running order processing tasks without blocking the API.
/// </summary>
public class OrderProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProcessingService> _logger;
    private readonly TelemetryClient? _telemetryClient;

    public OrderProcessingService(
        IServiceProvider serviceProvider,
        ILogger<OrderProcessingService> logger,
        TelemetryClient? telemetryClient = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderProcessingService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing orders");
            }

            // Wait 5 seconds before checking again
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("OrderProcessingService stopped");
    }

    private async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        // Get pending orders
        var pendingOrders = await context.Orders
            .Where(o => o.Status == "Pending")
            .OrderBy(o => o.CreatedAt)
            .Take(10) // Process 10 at a time
            .ToListAsync(cancellationToken);

        var queueDepth = pendingOrders.Count;

        // Track queue depth as a custom metric for monitoring
        _telemetryClient?.TrackMetric("OrdersQueueDepth", queueDepth);

        if (queueDepth == 0)
        {
            return; // No pending orders
        }

        _logger.LogInformation("Processing {Count} pending orders", queueDepth);

        foreach (var order in pendingOrders)
        {
            try
            {
                // Simulate order processing (in real app: validate inventory, charge payment, etc.)
                await Task.Delay(100, cancellationToken); // Simulate processing time

                order.Status = "Processing";
                await context.SaveChangesAsync(cancellationToken);

                // Simulate more processing
                await Task.Delay(200, cancellationToken);

                order.Status = "Completed";
                order.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
                order.Status = "Failed";
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

