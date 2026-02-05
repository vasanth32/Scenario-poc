namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: SINGLE RESPONSIBILITY
/// ======================================
/// This class has ONE responsibility: Validate order data.
/// 
/// It doesn't calculate, persist, or process payments.
/// Only validates.
/// </summary>
public class OrderValidator : IOrderValidator
{
    public bool Validate(decimal price, int quantity)
    {
        // Single Responsibility: Only validation logic
        return price > 0 && quantity > 0 && quantity <= 1000;
    }
}

