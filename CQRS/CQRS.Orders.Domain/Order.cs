namespace CQRS.Orders.Domain;

/// <summary>
/// Domain Entity - Contains only business logic
/// No dependencies on EF Core, ASP.NET, or any external frameworks
/// This is the core of Clean Architecture - pure business logic
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Business logic: Calculate total price
    /// This is domain logic, not infrastructure or application logic
    /// </summary>
    public decimal GetTotalPrice()
    {
        return Quantity * Price;
    }

    /// <summary>
    /// Business rule: Order must have valid quantity
    /// </summary>
    public bool IsValid()
    {
        return Quantity > 0 && Price >= 0 && !string.IsNullOrWhiteSpace(ProductName);
    }
}

