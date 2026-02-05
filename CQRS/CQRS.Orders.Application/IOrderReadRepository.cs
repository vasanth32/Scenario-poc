using CQRS.Orders.Application.DTOs;

namespace CQRS.Orders.Application;

/// <summary>
/// Read Repository interface for query operations
/// 
/// CQRS Principle:
/// - Separate read interface from write interface
/// - Queries use this interface (not the write repository)
/// - This allows different implementations for read/write (read replicas, caching, etc.)
/// </summary>
public interface IOrderReadRepository
{
    Task<List<OrderDto>> GetAllOrdersAsync();
}

