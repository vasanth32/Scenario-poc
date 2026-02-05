namespace CQRS.Orders.Application.DTOs;

/// <summary>
/// Data Transfer Object for read operations (Queries)
/// DTOs are optimized for reading - only include what the API needs
/// This is different from Domain entities which contain business logic
/// 
/// CQRS Performance Benefit:
/// - DTOs can be flattened (no complex object graphs)
/// - Can include computed fields (like TotalPrice) without loading related entities
/// - Queries can select only needed columns from database
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; } // Computed field - no need to load related entities
    public DateTime CreatedAt { get; set; }
}

