using SOLID.Orders.Domain;

namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: DEPENDENCY INVERSION
/// =====================================
/// "Depend on abstractions, not concrete classes"
/// 
/// This interface is an abstraction. Both high-level modules (services)
/// and low-level modules (repositories) depend on this abstraction,
/// not on concrete implementations.
/// 
/// Benefits:
/// - Services don't know about database implementation
/// - Easy to swap implementations (SQL Server, MongoDB, InMemory)
/// - Easy to test (can inject mock repository)
/// </summary>
public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
}

