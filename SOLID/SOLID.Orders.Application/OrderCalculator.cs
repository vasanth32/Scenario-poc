namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: SINGLE RESPONSIBILITY
/// ======================================
/// This class has ONE responsibility: Calculate order totals.
/// 
/// It doesn't validate, persist, or process payments.
/// Only calculates.
/// </summary>
public class OrderCalculator : IOrderCalculator
{
    public decimal CalculateTotal(decimal price, int quantity, decimal discount)
    {
        // Single Responsibility: Only calculation logic
        var subtotal = price * quantity;
        return subtotal - discount;
    }
}

