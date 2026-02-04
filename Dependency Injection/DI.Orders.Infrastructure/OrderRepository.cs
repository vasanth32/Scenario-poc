using Microsoft.EntityFrameworkCore;
using DI.Orders.Domain;
using DI.Orders.Application;

namespace DI.Orders.Infrastructure;

/// <summary>
/// Order Repository Implementation
/// 
/// DEPENDENCY INJECTION IN ACTION:
/// ===============================
/// This repository receives OrderDbContext through constructor injection.
/// The DI container creates the DbContext and passes it here.
/// 
/// We don't write: new OrderDbContext(...)
/// The framework handles it!
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    /// <summary>
    /// Constructor Injection
    /// 
    /// The DI container will:
    /// 1. Create OrderDbContext (Scoped lifetime)
    /// 2. Pass it to this constructor
    /// 3. Manage both lifetimes
    /// </summary>
    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }
}

