namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: OPEN/CLOSED
/// ============================
/// "Software entities should be open for extension, but closed for modification"
/// 
/// This interface allows us to add new discount types WITHOUT modifying
/// existing code. We can create new implementations (PercentageDiscount,
/// FixedAmountDiscount, SeasonalDiscount) without changing OrderService.
/// 
/// Example:
/// - Existing: NoDiscountCalculator
/// - New: PercentageDiscountCalculator (extends functionality, doesn't modify existing)
/// - New: FixedAmountDiscountCalculator (extends functionality, doesn't modify existing)
/// 
/// Benefits:
/// - Add new discount types without touching existing code
/// - Existing code remains stable and tested
/// - Follows Open/Closed Principle
/// </summary>
public interface IDiscountCalculator
{
    decimal CalculateDiscount(decimal amount);
}

