namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: LISKOV SUBSTITUTION (continued)
/// ================================================
/// Another implementation of IPaymentProcessor.
/// 
/// This can be substituted for IPaymentProcessor anywhere,
/// and the code will work the same way.
/// 
/// Both CreditCardPaymentProcessor and PayPalPaymentProcessor
/// are substitutable - this is Liskov Substitution Principle.
/// </summary>
public class PayPalPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        // Simulate PayPal payment processing
        await Task.Delay(100); // Simulate API call
        
        // Liskov: Same contract, different implementation
        // Can replace IPaymentProcessor without breaking code
        return amount > 0 && amount <= 10000; // PayPal has limit
    }
}

