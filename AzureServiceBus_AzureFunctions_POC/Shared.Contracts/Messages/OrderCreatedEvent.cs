using System.Text.Json.Serialization;

namespace Shared.Contracts.Messages;

/// <summary>
/// Represents an event that is published when a new order is created.
/// This message is sent to Azure Service Bus and consumed by the InventoryProcessorFunction.
/// </summary>
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    
    [JsonPropertyName("CreatedAt")]
    public DateTime CreatedAt { get; init; }
}

