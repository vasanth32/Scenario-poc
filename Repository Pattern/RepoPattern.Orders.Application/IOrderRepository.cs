using RepoPattern.Orders.Domain;

namespace RepoPattern.Orders.Application;

/// <summary>
/// Repository Interface - Defined in Application layer
/// 
/// WHY REPOSITORY EXISTS:
/// 1. Abstraction: Business logic doesn't need to know HOW data is stored
/// 2. Testability: Easy to create mock repositories for unit testing
/// 3. Flexibility: Can swap database implementations (SQL Server, MongoDB, InMemory, etc.) without changing business code
/// 4. Separation of Concerns: Data access logic is isolated from business logic
/// 
/// The interface is in Application layer because:
/// - Application layer defines WHAT it needs (the contract)
/// - Infrastructure layer implements HOW it's done (the implementation)
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Get all orders
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersAsync();

    /// <summary>
    /// Add a new order
    /// </summary>
    Task<Order> AddOrderAsync(Order order);
}

