namespace RepoPattern.Orders.Domain;

/// <summary>
/// Order Entity - Simple domain model
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

