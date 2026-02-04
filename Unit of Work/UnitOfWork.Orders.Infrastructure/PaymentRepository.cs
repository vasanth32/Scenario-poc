using Microsoft.EntityFrameworkCore;
using UnitOfWork.Orders.Domain;
using UnitOfWork.Orders.Application;

namespace UnitOfWork.Orders.Infrastructure;

/// <summary>
/// Payment Repository Implementation
/// 
/// Same principle: No SaveChanges() here!
/// All saves are controlled by UnitOfWork.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly OrderDbContext _context;

    public PaymentRepository(OrderDbContext context)
    {
        _context = context;
    }

    public Task<Payment> AddAsync(Payment payment)
    {
        // Add to context, but DON'T save yet
        // SaveChanges() will be called by UnitOfWork.CommitAsync()
        _context.Payments.Add(payment);
        // NO await _context.SaveChangesAsync() here!
        return Task.FromResult(payment);
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.ToListAsync();
    }
}

