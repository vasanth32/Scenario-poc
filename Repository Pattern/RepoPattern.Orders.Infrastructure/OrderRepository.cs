using Microsoft.EntityFrameworkCore;
using RepoPattern.Orders.Domain;
using RepoPattern.Orders.Application;

namespace RepoPattern.Orders.Infrastructure;

/// <summary>
/// Repository Implementation - Concrete implementation of IOrderRepository
/// 
/// HOW REPOSITORY HELPS:
/// 
/// 1. TESTING:
///    - Can create a MockOrderRepository that implements IOrderRepository
///    - Test business logic without hitting a real database
///    - Example: InMemoryOrderRepository for fast unit tests
/// 
/// 2. DATABASE REPLACEMENT:
///    - Want to switch from SQL Server to MongoDB? Just create MongoOrderRepository
///    - Want to use a file-based storage? Create FileOrderRepository
///    - Business code (controllers, services) doesn't change at all!
///    - Only need to change the DI registration in Program.cs
/// 
/// 3. SINGLE RESPONSIBILITY:
///    - This class ONLY handles data persistence
///    - Controllers don't need to know about DbContext, SaveChanges, etc.
///    - All database-specific code is isolated here
/// 
/// WITHOUT REPOSITORY (BAD):
///    Controller -> DbContext (direct access)
///    - Hard to test (need real database)
///    - Hard to swap databases
///    - Business logic mixed with data access
/// 
/// WITH REPOSITORY (GOOD):
///    Controller -> IOrderRepository -> OrderRepository -> DbContext
///    - Easy to test (mock the interface)
///    - Easy to swap databases (implement interface differently)
///    - Clear separation of concerns
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        // All database access logic is here
        // Controller doesn't need to know about DbContext, ToListAsync, etc.
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order> AddOrderAsync(Order order)
    {
        // All database save logic is here
        // Controller doesn't need to know about Add, SaveChanges, etc.
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }
}

