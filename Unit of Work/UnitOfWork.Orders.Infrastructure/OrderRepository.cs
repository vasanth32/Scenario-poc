using Microsoft.EntityFrameworkCore;
using UnitOfWork.Orders.Domain;
using UnitOfWork.Orders.Application;

namespace UnitOfWork.Orders.Infrastructure;

/// <summary>
/// Order Repository Implementation
/// 
/// KEY POINT: This repository does NOT call SaveChanges()!
/// It only adds entities to the DbContext.
/// UnitOfWork is responsible for calling SaveChanges().
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public Task<Order> AddAsync(Order order)
    {
        // Add to context, but DON'T save yet
        // SaveChanges() will be called by UnitOfWork.CommitAsync()
        _context.Orders.Add(order);
        // NO await _context.SaveChangesAsync() here!
        return Task.FromResult(order);
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }
}

