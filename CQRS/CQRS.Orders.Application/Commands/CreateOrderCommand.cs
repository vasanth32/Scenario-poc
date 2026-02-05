using CQRS.Orders.Domain;

namespace CQRS.Orders.Application.Commands;

/// <summary>
/// Command: Represents a write operation (CREATE, UPDATE, DELETE)
/// 
/// CQRS Principle: Commands change state, they don't return data
/// Commands use Domain entities + Repository pattern
/// 
/// Why CQRS improves performance:
/// 1. Commands can use complex domain models with validation and business rules
/// 2. Commands don't need to be optimized for reading - they focus on correctness
/// 3. Write operations can use different database optimizations (indexes, constraints)
/// 4. Separation allows independent scaling of read/write workloads
/// </summary>
public class CreateOrderCommand
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Command Handler - Processes the command
/// Uses Domain entity and Repository (write path)
/// </summary>
public class CreateOrderCommandHandler
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Execute the command
    /// This is the WRITE path - uses Domain + Repository
    /// </summary>
    public async Task<int> HandleAsync(CreateOrderCommand command)
    {
        // Create domain entity with business logic
        var order = new Order
        {
            ProductName = command.ProductName,
            Quantity = command.Quantity,
            Price = command.Price,
            CreatedAt = DateTime.UtcNow
        };

        // Validate using domain logic
        if (!order.IsValid())
        {
            throw new InvalidOperationException("Order is not valid");
        }

        // Use repository to persist (write operation)
        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();
        
        // Order ID is set by EF Core after SaveChanges
        return order.Id;
    }
}

