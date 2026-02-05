namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: INTERFACE SEGREGATION (continued)
/// ===================================================
/// Another small, focused interface for calculations only.
/// 
/// A class that only needs to calculate totals doesn't need
/// to implement validation or persistence methods.
/// </summary>
public interface IOrderCalculator
{
    decimal CalculateTotal(decimal price, int quantity, decimal discount);
}

