using DI.Orders.Domain;

namespace DI.Orders.Application;

/// <summary>
/// Order Service - Demonstrates Dependency Injection
/// 
/// WHAT IS DEPENDENCY INJECTION (DI)?
/// ====================================
/// Dependency Injection is a design pattern where objects receive their dependencies
/// from an external source (DI container) rather than creating them internally.
/// 
/// WITHOUT DEPENDENCY INJECTION (BAD):
/// ====================================
/// ```csharp
/// public class OrderService
/// {
///     private readonly IOrderRepository _repository;
///     
///     public OrderService()
///     {
///         // BAD: Service creates its own dependency
///         _repository = new OrderRepository(new OrderDbContext(...));
///         // Problems:
///         // 1. Tight coupling - can't easily swap implementations
///         // 2. Hard to test - can't inject a mock repository
///         // 3. Violates Single Responsibility Principle
///         // 4. Can't control object lifetime
///     }
/// }
/// ```
/// 
/// WITH DEPENDENCY INJECTION (GOOD):
/// ==================================
/// ```csharp
/// public class OrderService
/// {
///     private readonly IOrderRepository _repository;
///     
///     // GOOD: Dependencies are injected via constructor
///     public OrderService(IOrderRepository repository)
///     {
///         _repository = repository;
///         // Benefits:
///         // 1. Loose coupling - depends on interface, not concrete class
///         // 2. Easy to test - can inject mock repository
///         // 3. Single Responsibility - service doesn't create dependencies
///         // 4. Lifetime managed by DI container
///     }
/// }
/// ```
/// 
/// CONSTRUCTOR INJECTION (What we're using):
/// ==========================================
/// - Dependencies are provided through constructor parameters
/// - Most common and recommended approach
/// - Makes dependencies explicit and required
/// - Easy to test (can pass mocks in tests)
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _repository;

    /// <summary>
    /// Constructor Injection
    /// 
    /// The DI container will automatically:
    /// 1. Create an instance of IOrderRepository (OrderRepository)
    /// 2. Pass it to this constructor
    /// 3. Manage the lifetime of both objects
    /// 
    /// We don't need to write: new OrderRepository(...)
    /// The framework handles it!
    /// </summary>
    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Order> CreateOrderAsync(string productName, int quantity, decimal price)
    {
        var order = new Order
        {
            ProductName = productName,
            Quantity = quantity,
            Price = price
        };

        return await _repository.AddAsync(order);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

