using CleanArch.Orders.Domain;

namespace CleanArch.Orders.Application;

/// <summary>
/// Repository Interface - Defined in Application layer
/// Application layer defines WHAT it needs (interface), not HOW it's implemented
/// Infrastructure layer will implement this interface
/// 
/// Dependency Flow: Application -> Domain (Application depends on Domain)
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order> CreateAsync(Order order);
    Task<bool> DeleteAsync(int id);
}

