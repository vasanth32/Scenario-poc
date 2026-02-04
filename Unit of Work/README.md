# Unit of Work Pattern - Orders API

A simple .NET 8 Web API demonstrating the Unit of Work pattern.

## What is Unit of Work Pattern?

The Unit of Work pattern **manages a single database transaction** across multiple repository operations. It ensures that all changes are saved together (atomicity) - either everything succeeds or everything fails.

## Project Structure

```
UnitOfWork.Orders/
├── UnitOfWork.Orders.Domain/          # Order and Payment entities
├── UnitOfWork.Orders.Application/     # Repository interfaces + IUnitOfWork
├── UnitOfWork.Orders.Infrastructure/ # Repository implementations + UnitOfWork
└── UnitOfWork.Orders.API/            # Controllers
```

## Key Concept: Why SaveChanges Should NOT Be in Repository

### ❌ WITHOUT Unit of Work (BAD):
```csharp
// Each repository calls SaveChanges() separately
orderRepository.Add(order);
orderRepository.SaveChanges();  // Transaction 1 - Order saved ✅

paymentRepository.Add(payment);
paymentRepository.SaveChanges();  // Transaction 2 - Payment saved ✅

// Problem: If payment fails, order is already saved! ❌
// Result: Inconsistent state - order exists but payment doesn't
```

### ✅ WITH Unit of Work (GOOD):
```csharp
// Repositories add to context, but DON'T save
orderRepository.Add(order);      // Added to context (not saved yet)
paymentRepository.Add(payment);  // Added to context (not saved yet)

// UnitOfWork saves BOTH in ONE transaction
unitOfWork.CommitAsync();        // ONE transaction saves both ✅

// If payment fails, order is also rolled back! ✅
// Result: Consistent state - either both saved or neither saved
```

## How It Works

1. **Repositories don't call SaveChanges()**
   - They only add entities to the DbContext
   - All repositories share the same DbContext instance

2. **UnitOfWork manages the transaction**
   - Provides access to all repositories
   - `CommitAsync()` is the ONLY place where `SaveChanges()` is called
   - All pending changes are saved in ONE database transaction

3. **Transaction consistency**
   - Entity Framework automatically wraps `SaveChanges()` in a transaction
   - If `CommitAsync()` fails, ALL changes are rolled back
   - Ensures "All or Nothing" - either all succeed or all fail

## Example: Order + Payment Transaction

```csharp
// Create order and payment
var order = new Order { ProductName = "Laptop", Quantity = 1, Price = 999.99 };
var payment = new Payment { Amount = 999.99, PaymentMethod = "Credit Card" };

// Add both to repositories (not saved yet)
await unitOfWork.Orders.AddAsync(order);
await unitOfWork.Payments.AddAsync(payment);

// Save BOTH in ONE transaction
await unitOfWork.CommitAsync();

// If payment validation fails, BOTH are rolled back!
```

## Transaction Consistency Explained

### Scenario 1: Success
```
1. Order added to context ✅
2. Payment added to context ✅
3. CommitAsync() called ✅
4. Both saved in database ✅
```

### Scenario 2: Failure
```
1. Order added to context ✅
2. Payment validation fails ❌
3. Exception thrown
4. CommitAsync() never called
5. Both rolled back ✅ (nothing saved)
```

### Scenario 3: Database Error
```
1. Order added to context ✅
2. Payment added to context ✅
3. CommitAsync() called
4. Database error occurs ❌
5. Entity Framework automatically rolls back transaction ✅
6. Both rolled back ✅ (nothing saved)
```

## API Endpoints

- `POST /api/order/create-with-payment` - Create Order and Payment in one transaction
- `GET /api/order` - Get all orders
- `GET /api/order/payments` - Get all payments

## Example Request

```json
POST /api/order/create-with-payment
{
  "productName": "Laptop",
  "quantity": 1,
  "price": 999.99,
  "paymentMethod": "Credit Card"
}
```

## Running the Project

```bash
cd UnitOfWork.Orders.API
dotnet run
```

The API will be available at `https://localhost:5001` (or the port shown in the console).

## Key Learning

> **"Either everything succeeds or everything fails"**

The Unit of Work pattern ensures transaction consistency by managing all database operations through a single transaction boundary.

## Why This Matters

- **Data Integrity**: Prevents partial saves that leave data in inconsistent state
- **Atomicity**: All related operations succeed or fail together
- **Simplified Error Handling**: One rollback handles all changes
- **Performance**: One database transaction is more efficient than multiple

