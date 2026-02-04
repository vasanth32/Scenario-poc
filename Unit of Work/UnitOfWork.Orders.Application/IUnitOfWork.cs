namespace UnitOfWork.Orders.Application;

/// <summary>
/// Unit of Work Interface
/// 
/// WHAT IS UNIT OF WORK PATTERN?
/// - Manages a single database transaction across multiple repository operations
/// - Ensures all changes are saved together (atomicity)
/// - If ANY operation fails, ALL changes are rolled back
/// 
/// WHY DO WE NEED IT?
/// 
/// Problem without UnitOfWork:
/// ```csharp
/// orderRepository.Add(order);
/// orderRepository.SaveChanges();  // Transaction 1 - Order saved
/// paymentRepository.Add(payment);
/// paymentRepository.SaveChanges();  // Transaction 2 - Payment saved
/// // If payment fails, order is already saved! ❌ Inconsistent state!
/// ```
/// 
/// Solution with UnitOfWork:
/// ```csharp
/// orderRepository.Add(order);      // Added to context (not saved yet)
/// paymentRepository.Add(payment);  // Added to context (not saved yet)
/// unitOfWork.CommitAsync();        // ONE transaction saves both
/// // If payment fails, order is also rolled back! ✅ Consistent state!
/// ```
/// 
/// TRANSACTION CONSISTENCY:
/// - All repository operations share the same DbContext
/// - CommitAsync() saves ALL pending changes in ONE transaction
/// - If CommitAsync() fails, ALL changes are rolled back
/// - This ensures "All or Nothing" - either everything succeeds or everything fails
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }

    /// <summary>
    /// Save all pending changes in a single transaction
    /// 
    /// This is the ONLY place where SaveChanges() is called!
    /// All repositories add entities to the context, but don't save them.
    /// Only UnitOfWork commits everything together.
    /// </summary>
    Task<int> CommitAsync();

    /// <summary>
    /// Rollback all pending changes (if needed)
    /// </summary>
    Task RollbackAsync();
}

