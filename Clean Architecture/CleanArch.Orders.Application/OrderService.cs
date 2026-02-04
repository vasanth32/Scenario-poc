using CleanArch.Orders.Domain;

namespace CleanArch.Orders.Application;

/// <summary>
/// Use Case / Application Service
/// This is where business use cases are implemented
/// 
/// Dependency Flow:
/// - Application depends on Domain (uses Order entity)
/// - Application depends on IOrderRepository (interface, not implementation)
/// - Infrastructure will implement IOrderRepository
/// - API will use this service
/// 
/// This layer knows WHAT to do, not HOW to do it
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;

    // Dependency Injection: We depend on abstraction (interface), not concrete implementation
    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Use Case: Get all orders
    /// </summary>
    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _orderRepository.GetAllAsync();
    }

    /// <summary>
    /// Use Case: Get order by ID
    /// </summary>
    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _orderRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Use Case: Create a new order
    /// Contains business logic validation
    /// </summary>
    public async Task<Order> CreateOrderAsync(string productName, int quantity, decimal price)
    {
        var order = new Order
        {
            ProductName = productName,
            Quantity = quantity,
            Price = price
        };

        // Business rule validation
        if (!order.IsValid())
        {
            throw new ArgumentException("Invalid order data");
        }

        return await _orderRepository.CreateAsync(order);
    }

    /// <summary>
    /// Use Case: Delete an order
    /// </summary>
    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await _orderRepository.DeleteAsync(id);
    }
}

