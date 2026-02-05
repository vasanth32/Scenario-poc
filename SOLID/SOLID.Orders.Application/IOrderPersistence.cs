using SOLID.Orders.Domain;

namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: INTERFACE SEGREGATION (continued)
/// ===================================================
/// Interface focused ONLY on persistence operations.
/// 
/// Classes that only need to save/load orders don't need
/// to implement validation or calculation methods.
/// </summary>
public interface IOrderPersistence
{
    Task<Order> SaveAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
}

