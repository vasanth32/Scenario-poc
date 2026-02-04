using Microsoft.EntityFrameworkCore;
using UnitOfWork.Orders.Application;

namespace UnitOfWork.Orders.Infrastructure;

/// <summary>
/// Unit of Work Implementation
/// 
/// This is the ONLY place where SaveChanges() is called!
/// 
/// HOW IT WORKS:
/// 1. All repositories share the same DbContext instance
/// 2. Repositories add entities to context (but don't save)
/// 3. UnitOfWork.CommitAsync() saves ALL pending changes in ONE transaction
/// 4. If CommitAsync() fails, ALL changes are automatically rolled back
/// 
/// TRANSACTION CONSISTENCY:
/// - Entity Framework automatically wraps CommitAsync() in a database transaction
/// - If SaveChanges() fails, the transaction is rolled back
/// - This ensures "All or Nothing" - either all changes succeed or all fail
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;
    private IOrderRepository? _orders;
    private IPaymentRepository? _payments;

    public UnitOfWork(OrderDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Order Repository - Lazy initialization
    /// All repositories share the same DbContext instance
    /// </summary>
    public IOrderRepository Orders
    {
        get
        {
            _orders ??= new OrderRepository(_context);
            return _orders;
        }
    }

    /// <summary>
    /// Payment Repository - Lazy initialization
    /// All repositories share the same DbContext instance
    /// </summary>
    public IPaymentRepository Payments
    {
        get
        {
            _payments ??= new PaymentRepository(_context);
            return _payments;
        }
    }

    /// <summary>
    /// Save all pending changes in a single transaction
    /// 
    /// THIS IS THE ONLY PLACE WHERE SaveChanges() IS CALLED!
    /// 
    /// Transaction behavior:
    /// - All changes from all repositories are saved together
    /// - If ANY change fails, ALL changes are rolled back
    /// - Returns the number of affected rows
    /// </summary>
    public async Task<int> CommitAsync()
    {
        try
        {
            // SaveChanges() automatically creates a database transaction
            // If this fails, Entity Framework rolls back the transaction
            return await _context.SaveChangesAsync();
        }
        catch
        {
            // If SaveChanges() fails, the transaction is automatically rolled back
            // by Entity Framework. We can also explicitly rollback if needed.
            await RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Rollback all pending changes
    /// </summary>
    public async Task RollbackAsync()
    {
        // Entity Framework automatically handles rollback on exception
        // But we can also explicitly clear the change tracker
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();

        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

