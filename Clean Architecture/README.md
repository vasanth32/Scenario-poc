# Clean Architecture - Orders API

A simple .NET 8 Web API demonstrating Clean Architecture principles.

## Project Structure

```
CleanArch.Orders/
├── CleanArch.Orders.Domain/          # Core business logic (no dependencies)
├── CleanArch.Orders.Application/     # Use cases and interfaces
├── CleanArch.Orders.Infrastructure/ # Data access implementation
└── CleanArch.Orders.API/            # HTTP endpoints (controllers only)
```

## Dependency Flow (The Key Concept)

```
API Layer
  ↓ depends on
Application Layer
  ↓ depends on
Domain Layer
  ↑ implemented by
Infrastructure Layer
```

### Detailed Explanation:

1. **Domain Layer** (Innermost)
   - Contains `Order` entity with business logic
   - **NO dependencies** on EF Core, ASP.NET, or any external frameworks
   - Pure business logic only
   - This is the core of your application

2. **Application Layer**
   - Contains use cases (`OrderService`)
   - Defines interfaces (`IOrderRepository`)
   - **Depends on:** Domain layer only
   - Knows WHAT to do, not HOW to do it

3. **Infrastructure Layer**
   - Implements data persistence (`OrderRepository`, `OrderDbContext`)
   - Uses EF Core InMemory database
   - **Depends on:** Domain and Application layers
   - Implements interfaces defined in Application layer

4. **API Layer** (Outermost)
   - Contains controllers only
   - Handles HTTP requests/responses
   - **Depends on:** Application and Infrastructure layers
   - Does NOT directly access Domain or database

## Key Principles

- **Dependency Rule**: Inner layers don't know about outer layers
- **Separation of Concerns**: Each layer has a single responsibility
- **Dependency Inversion**: Depend on abstractions (interfaces), not concrete implementations

## Running the Project

```bash
cd CleanArch.Orders.API
dotnet run
```

The API will be available at `https://localhost:5001` (or the port shown in the console).

## API Endpoints

- `GET /api/order` - Get all orders
- `GET /api/order/{id}` - Get order by ID
- `POST /api/order` - Create a new order
- `DELETE /api/order/{id}` - Delete an order

## Example Request

```json
POST /api/order
{
  "productName": "Laptop",
  "quantity": 2,
  "price": 999.99
}
```

