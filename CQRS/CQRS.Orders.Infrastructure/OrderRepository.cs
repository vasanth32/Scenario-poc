using Microsoft.EntityFrameworkCore;
using CQRS.Orders.Domain;
using CQRS.Orders.Application;

namespace CQRS.Orders.Infrastructure;

/// <summary>
/// Repository implementation for write operations (Commands)
/// 
/// CQRS Principle:
/// - Repository is used ONLY for write operations (Commands)
/// - Read operations (Queries) directly use DbContext for performance
/// 
/// Why separate?
/// - Commands need domain validation and business rules
/// - Queries need direct, optimized data access
/// - This separation allows independent optimization of read/write paths
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Add order (write operation)
    /// Uses domain entity with business logic
    /// Note: Does not save changes - caller must call SaveChangesAsync()
    /// This allows transaction control (Unit of Work pattern)
    /// The order ID will be set by EF Core after SaveChangesAsync() is called
    /// </summary>
    public async Task AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
        // Don't save here - let caller control when to save (for transactions)
    }

    /// <summary>
    /// Save changes (for transaction control)
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

