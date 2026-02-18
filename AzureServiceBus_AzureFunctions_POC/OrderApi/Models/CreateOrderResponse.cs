namespace OrderApi.Models;

/// <summary>
/// Response DTO returned after creating an order.
/// </summary>
public record CreateOrderResponse
{
    public Guid OrderId { get; init; }
    public string Message { get; init; } = string.Empty;
}

