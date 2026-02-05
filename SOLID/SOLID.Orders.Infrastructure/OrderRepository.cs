using Microsoft.EntityFrameworkCore;
using SOLID.Orders.Domain;
using SOLID.Orders.Application;

namespace SOLID.Orders.Infrastructure;

/// <summary>
/// SOLID Principle: DEPENDENCY INVERSION
/// =====================================
/// This is a concrete implementation of IOrderRepository.
/// 
/// High-level modules (OrderService) depend on IOrderRepository (abstraction),
/// not on this concrete class.
/// 
/// We can swap this with MongoDBOrderRepository, FileOrderRepository, etc.
/// without changing OrderService.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

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

