namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: LISKOV SUBSTITUTION
/// ====================================
/// "Objects of a superclass should be replaceable with objects of its subclasses
/// without breaking the application"
/// 
/// This is the base interface. Any implementation (CreditCardPaymentProcessor,
/// PayPalPaymentProcessor, BankTransferProcessor) should be substitutable
/// without breaking the code that uses IPaymentProcessor.
/// 
/// Liskov Substitution means:
/// - All implementations must honor the contract
/// - Can't throw unexpected exceptions
/// - Can't return invalid values
/// - Must be truly substitutable
/// 
/// Example:
/// - CreditCardPaymentProcessor : IPaymentProcessor ✅ (can replace base)
/// - PayPalPaymentProcessor : IPaymentProcessor ✅ (can replace base)
/// - All work the same way, just different implementations
/// </summary>
public interface IPaymentProcessor
{
    Task<bool> ProcessPaymentAsync(decimal amount);
}

