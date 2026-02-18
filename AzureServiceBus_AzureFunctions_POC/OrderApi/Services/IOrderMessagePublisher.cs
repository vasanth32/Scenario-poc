using Shared.Contracts.Messages;

namespace OrderApi.Services;

/// <summary>
/// Service for publishing order events to Azure Service Bus.
/// </summary>
public interface IOrderMessagePublisher
{
    /// <summary>
    /// Publishes an order created event to the Service Bus queue.
    /// </summary>
    Task PublishAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default);
}

