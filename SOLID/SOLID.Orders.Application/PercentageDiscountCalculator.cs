namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: OPEN/CLOSED
/// ============================
/// This is a NEW discount calculator added WITHOUT modifying existing code.
/// 
/// We didn't change NoDiscountCalculator or OrderService.
/// We just added this new implementation.
/// 
/// This demonstrates Open/Closed Principle:
/// - Open for extension (we can add new discount types)
/// - Closed for modification (existing code unchanged)
/// </summary>
public class PercentageDiscountCalculator : IDiscountCalculator
{
    private readonly decimal _discountPercentage;

    public PercentageDiscountCalculator(decimal discountPercentage)
    {
        _discountPercentage = discountPercentage;
    }

    public decimal CalculateDiscount(decimal amount)
    {
        // New discount logic - added without modifying existing code
        return amount * (_discountPercentage / 100);
    }
}

