using UnitOfWork.Orders.Domain;

namespace UnitOfWork.Orders.Application;

/// <summary>
/// Payment Repository Interface
/// 
/// Same principle: No SaveChanges() here!
/// All saves are controlled by UnitOfWork.
/// </summary>
public interface IPaymentRepository
{
    Task<Payment> AddAsync(Payment payment);
    Task<Payment?> GetByIdAsync(int id);
    Task<IEnumerable<Payment>> GetAllAsync();
}

