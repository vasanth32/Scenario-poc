using CQRS.Orders.Domain;

namespace CQRS.Orders.Application;

/// <summary>
/// Repository interface for write operations (Commands)
/// In CQRS, repositories are used ONLY for write operations
/// Read operations use direct data access (see Queries)
/// </summary>
public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task SaveChangesAsync();
}

