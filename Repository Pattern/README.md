# Repository Pattern - Orders API

A simple .NET 8 Web API demonstrating the Repository Pattern.

## What is Repository Pattern?

The Repository Pattern **abstracts data access logic** from business logic. Instead of controllers/services directly using `DbContext`, they use a repository interface.

## Project Structure

```
RepoPattern.Orders/
├── RepoPattern.Orders.Domain/          # Order entity
├── RepoPattern.Orders.Application/     # IOrderRepository interface
├── RepoPattern.Orders.Infrastructure/ # OrderRepository implementation + DbContext
└── RepoPattern.Orders.API/            # Controllers (uses repository, NOT DbContext)
```

## Key Principle

**Controller should NOT talk to DbContext directly!**

### ❌ WITHOUT Repository Pattern (BAD):
```csharp
public class OrderController : ControllerBase
{
    private readonly OrderDbContext _context;  // Direct database access
    
    public OrderController(OrderDbContext context)
    {
        _context = context;
    }
    
    public async Task<ActionResult> GetOrders()
    {
        return await _context.Orders.ToListAsync();  // Controller knows about EF!
    }
}
```

### ✅ WITH Repository Pattern (GOOD):
```csharp
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;  // Uses interface
    
    public OrderController(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    public async Task<ActionResult> GetOrders()
    {
        return await _orderRepository.GetOrdersAsync();  // Controller doesn't know about EF!
    }
}
```

## Why Repository Pattern Exists

### 1. **Testing** 🧪
Without repository, testing is hard:
- Need a real database connection
- Tests are slow
- Hard to isolate unit tests

With repository:
```csharp
// Easy to create a mock for testing
public class MockOrderRepository : IOrderRepository
{
    private List<Order> _orders = new();
    
    public Task<IEnumerable<Order>> GetOrdersAsync()
    {
        return Task.FromResult(_orders.AsEnumerable());
    }
    
    // ... other methods
}

// In your test:
var mockRepo = new MockOrderRepository();
var controller = new OrderController(mockRepo);
// Test without database!
```

### 2. **Database Replacement** 🔄
Want to switch from SQL Server to MongoDB? 

**Without Repository:**
- Change every controller
- Change every service
- Change all database code
- High risk of bugs

**With Repository:**
```csharp
// Just create a new implementation
public class MongoOrderRepository : IOrderRepository
{
    // MongoDB implementation
}

// In Program.cs, just change one line:
// OLD: builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// NEW: builder.Services.AddScoped<IOrderRepository, MongoOrderRepository>();
// Controllers don't change at all!
```

### 3. **Separation of Concerns** 🎯
- **Controller**: Handles HTTP requests/responses
- **Repository**: Handles data access
- **DbContext**: Handles database operations

Each layer has a single responsibility.

## API Endpoints

- `GET /api/order` - Get all orders
- `POST /api/order` - Add a new order

## Example Request

```json
POST /api/order
{
  "productName": "Laptop",
  "quantity": 2,
  "price": 999.99
}
```

## Running the Project

```bash
cd RepoPattern.Orders.API
dotnet run
```

The API will be available at `https://localhost:5001` (or the port shown in the console).

## Key Learning

> **"Business code should not know how data is stored"**

The Repository Pattern enforces this principle by creating an abstraction layer between your business logic and data access code.

