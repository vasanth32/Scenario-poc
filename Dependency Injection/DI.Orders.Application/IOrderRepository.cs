using DI.Orders.Domain;

namespace DI.Orders.Application;

/// <summary>
/// Order Repository Interface
/// </summary>
public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
}

