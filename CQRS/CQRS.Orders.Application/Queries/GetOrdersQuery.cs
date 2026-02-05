using CQRS.Orders.Application.DTOs;

namespace CQRS.Orders.Application.Queries;

/// <summary>
/// Query: Represents a read operation (SELECT)
/// 
/// CQRS Principle: Queries read data, they don't change state
/// Queries directly access data and return DTOs (NOT domain entities)
/// 
/// Why CQRS improves performance:
/// 1. Queries can directly read from database without going through domain layer
/// 2. Queries can use optimized SQL (SELECT only needed columns)
/// 3. Queries can use read replicas (separate database for reads)
/// 4. Queries can use different data models optimized for reading (denormalized views)
/// 5. No need to load full domain entities - just return what's needed
/// 6. Can use caching strategies specific to read operations
/// 7. Read and write operations can be scaled independently
/// </summary>
public class GetOrdersQuery
{
    // Query parameters can be added here if needed
    // For example: public int? CustomerId { get; set; }
}

/// <summary>
/// Query Handler - Processes the query
/// Uses read repository to get DTOs (read path)
/// 
/// IMPORTANT: This does NOT use write Repository or Domain entities
/// It uses a separate read interface optimized for queries
/// </summary>
public class GetOrdersQueryHandler
{
    private readonly IOrderReadRepository _readRepository;

    public GetOrdersQueryHandler(IOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    /// <summary>
    /// Execute the query
    /// This is the READ path - uses read repository to get DTOs
    /// 
    /// Performance benefits:
    /// - Separate read interface allows optimized implementations
    /// - Can use read replicas, caching, or denormalized views
    /// - Returns DTOs directly (no domain entity overhead)
    /// - Can select only needed columns
    /// </summary>
    public async Task<List<OrderDto>> HandleAsync(GetOrdersQuery query)
    {
        // Use read repository - optimized for reading
        // Implementation can use direct SQL, read replicas, caching, etc.
        return await _readRepository.GetAllOrdersAsync();
    }
}

