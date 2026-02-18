using System.ComponentModel.DataAnnotations;

namespace OrderApi.Models;

/// <summary>
/// Request DTO for creating a new order.
/// </summary>
public record CreateOrderRequest
{
    [Required(ErrorMessage = "Customer name is required.")]
    [MinLength(1, ErrorMessage = "Customer name cannot be empty.")]
    public string CustomerName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Total amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
    public decimal TotalAmount { get; init; }
}

