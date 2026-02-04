using UnitOfWork.Orders.Domain;

namespace UnitOfWork.Orders.Application;

/// <summary>
/// Order Repository Interface
/// 
/// IMPORTANT: Notice there's NO SaveChanges() method here!
/// Repositories should NOT call SaveChanges() directly.
/// 
/// WHY SaveChanges should NOT be in repository:
/// 1. Transaction Control: When you need to save Order + Payment together,
///    each repository calling SaveChanges() would create separate transactions.
///    If Payment fails, Order would already be saved (inconsistent state!)
/// 
/// 2. Unit of Work Pattern: All changes should be saved in ONE transaction.
///    UnitOfWork.CommitAsync() handles this.
/// 
/// 3. Multiple Operations: If you need to save 3 orders and 2 payments,
///    you don't want 5 separate database transactions - you want 1!
/// </summary>
public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
}

