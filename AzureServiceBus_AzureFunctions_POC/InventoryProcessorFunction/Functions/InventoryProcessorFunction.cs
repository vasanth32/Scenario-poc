using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Messages;

namespace InventoryProcessorFunction.Functions;

/// <summary>
/// Azure Function that processes order messages from the Service Bus queue.
/// </summary>
public class InventoryProcessorFunction
{
    private readonly ILogger<InventoryProcessorFunction> _logger;

    public InventoryProcessorFunction(ILogger<InventoryProcessorFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Processes order created events from the Service Bus queue.
    /// Triggered automatically when a new message arrives in the 'orders-queue'.
    /// </summary>
    [Function(nameof(ProcessOrderCreated))]
    public async Task ProcessOrderCreated(
        [ServiceBusTrigger("orders-queue", Connection = "ServiceBusConnection")]
        string messageBody,
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Received message from Service Bus queue. Message body: {MessageBody}", messageBody);

            // Deserialize the message body to OrderCreatedEvent
            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(messageBody);

            if (orderEvent == null)
            {
                _logger.LogWarning("Failed to deserialize message. Message body was null or invalid.");
                return;
            }

            // Process the order event
            _logger.LogInformation(
                "Processing order {OrderId} for customer {CustomerName} with total amount {TotalAmount}. Decreasing inventory.",
                orderEvent.OrderId,
                orderEvent.CustomerName,
                orderEvent.TotalAmount);

            // Simulate inventory update logic
            await SimulateInventoryUpdate(orderEvent);

            _logger.LogInformation(
                "Successfully processed order {OrderId}. Inventory updated.",
                orderEvent.OrderId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message from Service Bus queue. Message body: {MessageBody}", messageBody);
            throw; // Re-throw to trigger dead-letter queue if configured
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order message from Service Bus queue.");
            throw; // Re-throw to trigger retry or dead-letter queue
        }
    }

    /// <summary>
    /// Simulates an inventory update operation.
    /// In a real scenario, this would update a database or call another service.
    /// </summary>
    private async Task SimulateInventoryUpdate(OrderCreatedEvent orderEvent)
    {
        // Simulate async work (e.g., database update, API call)
        await Task.Delay(100);

        _logger.LogInformation(
            "Inventory updated for order {OrderId}. Reserved items for customer {CustomerName}.",
            orderEvent.OrderId,
            orderEvent.CustomerName);
    }
}

