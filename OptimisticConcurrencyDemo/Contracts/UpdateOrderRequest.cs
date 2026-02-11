namespace OptimisticConcurrencyDemo.Contracts;

public class UpdateOrderRequest
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
}

