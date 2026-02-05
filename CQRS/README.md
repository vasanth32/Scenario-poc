# CQRS (Command Query Responsibility Segregation) - Orders API

A simple .NET 8 Web API demonstrating the CQRS (Command Query Responsibility Segregation) pattern.

## What is CQRS?

**CQRS** is a pattern that separates read operations (Queries) from write operations (Commands). The key principle is:

> **"Write for correctness, Read for speed"**

## Key Concept: Why Separate Commands and Queries?

### ❌ WITHOUT CQRS (Traditional Approach):

```csharp
// Same service handles both read and write
public class OrderService
{
    public async Task<int> CreateOrder(Order order) { ... }  // Write
    public async Task<List<Order>> GetOrders() { ... }        // Read
    
    // Problems:
    // 1. Same model for read/write (overhead)
    // 2. Can't optimize reads independently
    // 3. Can't scale read/write separately
    // 4. Domain entities loaded even for simple reads
}
```

### ✅ WITH CQRS (Separated Approach):

```csharp
// Commands (Write) - Use Domain + Repository
public class CreateOrderCommandHandler
{
    public async Task<int> HandleAsync(CreateOrderCommand command)
    {
        var order = new Order { ... };  // Domain entity
        await _repository.AddAsync(order);  // Repository
    }
}

// Queries (Read) - Direct data access + DTOs
public class GetOrdersQueryHandler
{
    public async Task<List<OrderDto>> HandleAsync(GetOrdersQuery query)
    {
        // Direct query, returns DTOs (not domain entities)
        return await _readRepository.GetAllOrdersAsync();
    }
}
```

## Project Structure

```
CQRS.Orders.API/
├── Controllers/
│   └── OrderController.cs          # API endpoints
│
CQRS.Orders.Application/
├── Commands/                        # Write operations
│   └── CreateOrderCommand.cs       # Command + Handler
├── Queries/                         # Read operations
│   └── GetOrdersQuery.cs           # Query + Handler
├── DTOs/                            # Data Transfer Objects
│   └── OrderDto.cs                 # Read-only DTOs
├── IOrderRepository.cs             # Write repository interface
└── IOrderReadRepository.cs         # Read repository interface
│
CQRS.Orders.Domain/
└── Order.cs                         # Domain entity (business logic)
│
CQRS.Orders.Infrastructure/
├── OrderDbContext.cs                # EF Core DbContext
├── OrderRepository.cs              # Write repository implementation
└── OrderReadRepository.cs          # Read repository implementation
```

## CQRS Principles

### 1. Commands (Write Operations)

**Location:** `Application/Commands/`

**Characteristics:**
- Change system state
- Use Domain entities + Repository pattern
- Focus on **correctness** (validation, business rules)
- Return minimal data (usually just success/failure)

**Example:**
```csharp
public class CreateOrderCommandHandler
{
    private readonly IOrderRepository _repository;

    public async Task<int> HandleAsync(CreateOrderCommand command)
    {
        // 1. Create domain entity (with business logic)
        var order = new Order
        {
            ProductName = command.ProductName,
            Quantity = command.Quantity,
            Price = command.Price
        };

        // 2. Validate using domain logic
        if (!order.IsValid())
            throw new InvalidOperationException("Order is not valid");

        // 3. Use repository to persist
        var orderId = await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        return orderId;
    }
}
```

### 2. Queries (Read Operations)

**Location:** `Application/Queries/`

**Characteristics:**
- Read data only (no state changes)
- Directly access data (bypass domain layer)
- Return DTOs (not domain entities)
- Focus on **performance** (optimized queries)

**Example:**
```csharp
public class GetOrdersQueryHandler
{
    private readonly IOrderReadRepository _readRepository;

    public async Task<List<OrderDto>> HandleAsync(GetOrdersQuery query)
    {
        // Direct read - returns DTOs, not domain entities
        return await _readRepository.GetAllOrdersAsync();
    }
}
```

### 3. DTOs (Data Transfer Objects)

**Location:** `Application/DTOs/`

**Purpose:**
- Optimized for reading
- Can include computed fields
- Flattened structure (no complex object graphs)
- Only contains what the API needs

**Example:**
```csharp
public class OrderDto
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }  // Computed field
    public DateTime CreatedAt { get; set; }
}
```

## Why CQRS Improves Performance

### 1. **Independent Optimization**
- **Commands** can use complex domain models with validation
- **Queries** can use optimized SQL (SELECT only needed columns)
- Each path can be optimized independently

### 2. **Direct Database Queries**
```csharp
// Query uses projection - only selects needed columns
var orders = await _dbContext.Orders
    .Select(o => new OrderDto
    {
        Id = o.Id,
        ProductName = o.ProductName,
        TotalPrice = o.Quantity * o.Price  // Computed in database
    })
    .ToListAsync();

// Generates efficient SQL:
// SELECT Id, ProductName, (Quantity * Price) as TotalPrice FROM Orders
```

### 3. **No Domain Entity Overhead**
- Queries don't load full domain entities
- No business logic execution during reads
- Faster data retrieval

### 4. **Scalability**
- Read and write operations can be scaled independently
- Can use read replicas (separate database for reads)
- Can implement caching strategies for reads

### 5. **Different Data Models**
- Commands use normalized domain model
- Queries can use denormalized views optimized for reading
- Can use different databases for read/write

## API Endpoints

### POST /api/order
**Command** - Creates a new order

**Request:**
```json
{
  "productName": "Laptop",
  "quantity": 2,
  "price": 999.99
}
```

**Response:**
```json
{
  "orderId": 1,
  "message": "Order created successfully"
}
```

### GET /api/order
**Query** - Gets all orders

**Response:**
```json
[
  {
    "id": 1,
    "productName": "Laptop",
    "quantity": 2,
    "price": 999.99,
    "totalPrice": 1999.98,
    "createdAt": "2026-02-05T10:00:00Z"
  }
]
```

## Key Differences: Commands vs Queries

| Aspect | Commands (Write) | Queries (Read) |
|--------|------------------|----------------|
| **Purpose** | Change state | Read data |
| **Uses** | Domain entities + Repository | Direct data access + DTOs |
| **Returns** | Minimal data (ID, success) | Full data (DTOs) |
| **Optimization** | Correctness, validation | Performance, speed |
| **Can modify data?** | Yes | No |
| **Uses business logic?** | Yes | No |

## Running the Project

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Build:**
   ```bash
   dotnet build
   ```

3. **Run:**
   ```bash
   cd CQRS.Orders.API
   dotnet run
   ```

4. **Access Swagger UI:**
   ```
   https://localhost:5001/swagger
   ```

## Testing the API

### Create Order (Command)
```bash
POST https://localhost:5001/api/order
Content-Type: application/json

{
  "productName": "Laptop",
  "quantity": 2,
  "price": 999.99
}
```

### Get Orders (Query)
```bash
GET https://localhost:5001/api/order
```

## Important Notes

1. **No MediatR**: This example doesn't use MediatR to keep it simple and focus on CQRS concepts
2. **Separation is Key**: Commands and Queries are completely separated - they don't share logic
3. **Read Repository**: Queries use a separate read repository interface, allowing different implementations (read replicas, caching, etc.)
4. **Clean Architecture**: Still follows clean architecture principles with proper layer separation

## When to Use CQRS

**Use CQRS when:**
- Read and write workloads are different
- You need to optimize reads independently
- You have complex domain models but simple read requirements
- You need to scale read/write operations separately

**Don't use CQRS when:**
- Application is simple (CRUD operations)
- Read/write patterns are similar
- Overhead of separation isn't justified

## Next Steps

After understanding CQRS, you can:
1. Add MediatR for command/query handling
2. Implement read replicas
3. Add caching for queries
4. Use event sourcing with CQRS
5. Implement different databases for read/write

---

**Key Learning:**
> "Write for correctness, Read for speed"

