namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: LISKOV SUBSTITUTION
/// ====================================
/// This implements IPaymentProcessor and can be substituted
/// anywhere IPaymentProcessor is expected.
/// 
/// It honors the contract:
/// - Takes decimal amount
/// - Returns Task<bool>
/// - Doesn't throw unexpected exceptions
/// - Can replace IPaymentProcessor without breaking code
/// </summary>
public class CreditCardPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        // Simulate credit card payment processing
        await Task.Delay(100); // Simulate API call
        
        // Liskov: Returns valid bool, doesn't throw unexpected exceptions
        return amount > 0; // Simple validation for demo
    }
}

