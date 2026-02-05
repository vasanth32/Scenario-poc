namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: OPEN/CLOSED (continued)
/// ========================================
/// Another NEW discount calculator added WITHOUT modifying existing code.
/// 
/// Multiple discount types can coexist without affecting each other.
/// </summary>
public class FixedAmountDiscountCalculator : IDiscountCalculator
{
    private readonly decimal _fixedDiscount;

    public FixedAmountDiscountCalculator(decimal fixedDiscount)
    {
        _fixedDiscount = fixedDiscount;
    }

    public decimal CalculateDiscount(decimal amount)
    {
        // Another new discount type - added without modifying existing code
        return Math.Min(_fixedDiscount, amount); // Don't discount more than the amount
    }
}

