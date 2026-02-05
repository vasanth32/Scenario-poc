using Microsoft.EntityFrameworkCore;
using CQRS.Orders.Application;
using CQRS.Orders.Application.DTOs;

namespace CQRS.Orders.Infrastructure;

/// <summary>
/// Read Repository implementation for query operations
/// 
/// CQRS Principle:
/// - This is separate from write repository
/// - Optimized for reading (direct queries, projections, etc.)
/// - Can be implemented with read replicas, caching, or denormalized views
/// 
/// Why separate read repository improves performance:
/// 1. Can use direct SQL queries (no domain entity loading)
/// 2. Can select only needed columns
/// 3. Can use database projections to compute fields
/// 4. Can use read replicas (separate database for reads)
/// 5. Can implement caching strategies
/// 6. Can use denormalized views optimized for reading
/// </summary>
public class OrderReadRepository : IOrderReadRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderReadRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get all orders - optimized for reading
    /// Directly queries database and returns DTOs (not domain entities)
    /// 
    /// Performance optimizations:
    /// - Uses projection (Select) to only get needed fields
    /// - Computes TotalPrice in database query (not in memory)
    /// - No domain entity instantiation overhead
    /// - Can be easily replaced with read replica or cached implementation
    /// </summary>
    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        // Direct database query with projection - optimized for reading
        // This generates efficient SQL: SELECT Id, ProductName, Quantity, Price, (Quantity * Price) as TotalPrice, CreatedAt FROM Orders
        var orders = await _dbContext.Orders
            .Select(o => new OrderDto
            {
                Id = o.Id,
                ProductName = o.ProductName,
                Quantity = o.Quantity,
                Price = o.Price,
                // Compute TotalPrice directly in query (database does the calculation)
                TotalPrice = o.Quantity * o.Price,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return orders;
    }
}

