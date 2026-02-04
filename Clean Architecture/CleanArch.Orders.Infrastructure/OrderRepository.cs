using Microsoft.EntityFrameworkCore;
using CleanArch.Orders.Domain;
using CleanArch.Orders.Application;

namespace CleanArch.Orders.Infrastructure;

/// <summary>
/// Repository Implementation
/// This is HOW data is persisted (using EF Core InMemory)
/// 
/// Dependency Flow:
/// - Infrastructure implements IOrderRepository (from Application layer)
/// - Infrastructure uses OrderDbContext (EF Core)
/// - Infrastructure uses Order entity (from Domain layer)
/// 
/// This is the ONLY place that knows about EF Core and database
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}

