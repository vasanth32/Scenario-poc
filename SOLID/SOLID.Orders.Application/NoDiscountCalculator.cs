namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: OPEN/CLOSED
/// ============================
/// This is the original discount calculator (no discount).
/// 
/// When we need to add discount logic, we DON'T modify this class.
/// Instead, we create NEW classes that implement IDiscountCalculator.
/// 
/// This class remains unchanged (closed for modification).
/// New discount types extend functionality (open for extension).
/// </summary>
public class NoDiscountCalculator : IDiscountCalculator
{
    public decimal CalculateDiscount(decimal amount)
    {
        // No discount - original implementation
        return 0;
    }
}

