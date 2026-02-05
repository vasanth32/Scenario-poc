namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: INTERFACE SEGREGATION
/// ======================================
/// "Clients should not be forced to depend on interfaces they do not use"
/// 
/// Instead of one large IOrderService interface with all methods,
/// we split it into smaller, focused interfaces.
/// 
/// This interface is focused ONLY on validation.
/// If a class only needs validation, it doesn't need to implement
/// other methods like calculation or persistence.
/// 
/// Benefits:
/// - Classes only implement what they need
/// - No forced implementation of unused methods
/// - Better separation of concerns
/// </summary>
public interface IOrderValidator
{
    bool Validate(decimal price, int quantity);
}

