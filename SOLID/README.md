# SOLID Principles - Orders API

A simple .NET 8 Web API demonstrating all five SOLID principles with practical examples.

## What are SOLID Principles?

SOLID is an acronym for five object-oriented design principles that make software more maintainable, flexible, and understandable.

## The Five Principles

### 1. **S** - Single Responsibility Principle
### 2. **O** - Open/Closed Principle
### 3. **L** - Liskov Substitution Principle
### 4. **I** - Interface Segregation Principle
### 5. **D** - Dependency Inversion Principle

---

## 1. Single Responsibility Principle (SRP)

> **"A class should have only one reason to change"**

### What It Means
Each class should have only one job or responsibility. If a class does multiple things, it becomes harder to maintain and test.

### Example in This Project

#### ❌ WITHOUT SRP (BAD):
```csharp
public class OrderService
{
    // BAD: This class does too many things
    public void CreateOrder(...)
    {
        // Validates order
        // Calculates total
        // Calculates discount
        // Saves to database
        // Sends email
        // Processes payment
    }
}
// Problem: If validation logic changes, calculation changes, or database changes,
// we have to modify this class. Too many reasons to change!
```

#### ✅ WITH SRP (GOOD):
```csharp
// Each class has ONE responsibility
public class OrderValidator      // Only validates
public class OrderCalculator     // Only calculates
public class DiscountCalculator  // Only calculates discounts
public class OrderRepository     // Only persists data
public class OrderService        // Only orchestrates (delegates to others)
```

### Implementation in This Project

- **`OrderValidator`** - Only validates order data
- **`OrderCalculator`** - Only calculates order totals
- **`OrderService`** - Only orchestrates order creation (delegates to others)
- **`OrderRepository`** - Only persists orders to database

**Benefits:**
- Easy to test each component independently
- Easy to modify one part without affecting others
- Clear separation of concerns

---

## 2. Open/Closed Principle (OCP)

> **"Software entities should be open for extension, but closed for modification"**

### What It Means
You should be able to add new functionality without modifying existing code. Extend functionality through inheritance, interfaces, or composition.

### Example in This Project

#### ❌ WITHOUT OCP (BAD):
```csharp
public class OrderService
{
    public decimal CalculateDiscount(string discountType, decimal amount)
    {
        // BAD: Adding new discount types requires modifying this method
        if (discountType == "Percentage")
            return amount * 0.1;
        else if (discountType == "Fixed")
            return 50;
        else if (discountType == "Seasonal")  // New type - must modify existing code!
            return amount * 0.2;
    }
}
// Problem: Every new discount type requires changing this method
```

#### ✅ WITH OCP (GOOD):
```csharp
// Original interface - closed for modification
public interface IDiscountCalculator
{
    decimal CalculateDiscount(decimal amount);
}

// Original implementation - closed for modification
public class NoDiscountCalculator : IDiscountCalculator { ... }

// New implementations - open for extension
public class PercentageDiscountCalculator : IDiscountCalculator { ... }
public class FixedAmountDiscountCalculator : IDiscountCalculator { ... }
// Can add more without modifying existing code!
```

### Implementation in This Project

- **`IDiscountCalculator`** - Interface (closed for modification)
- **`NoDiscountCalculator`** - Original implementation (unchanged)
- **`PercentageDiscountCalculator`** - New discount type (added without modifying existing code)
- **`FixedAmountDiscountCalculator`** - Another new discount type (added without modifying existing code)

**Benefits:**
- Existing code remains stable and tested
- New features don't break existing functionality
- Easy to add new discount types

---

## 3. Liskov Substitution Principle (LSP)

> **"Objects of a superclass should be replaceable with objects of its subclasses without breaking the application"**

### What It Means
LSP is about **behavioral substitutability** - derived classes must honor the **complete contract** of the base class/interface, not just return types and exceptions. The derived class must be truly substitutable in all ways.

### The Complete LSP Contract Includes:

1. **✅ Same Return Types** - Must return the same type (or compatible subtype)
2. **✅ No Unexpected Exceptions** - Can't throw exceptions not in the contract
3. **✅ Same Preconditions** - Can't require MORE than the base class
4. **✅ Same/Stronger Postconditions** - Must guarantee AT LEAST what base class guarantees
5. **✅ Same Invariants** - Must maintain the same properties/state
6. **✅ Behavioral Compatibility** - Must behave in a way that's compatible with base class

### Example in This Project

#### ❌ WITHOUT LSP (BAD):
```csharp
public interface IPaymentProcessor
{
    bool ProcessPayment(decimal amount);
}

public class CreditCardProcessor : IPaymentProcessor
{
    public bool ProcessPayment(decimal amount)
    {
        // ❌ VIOLATION 1: Unexpected exception
        if (amount < 0) throw new Exception();
        
        // ❌ VIOLATION 2: Stronger precondition (requires amount > 100)
        if (amount <= 100) 
            throw new InvalidOperationException("Minimum $100 required");
        
        // ❌ VIOLATION 3: Different behavior (returns null instead of bool)
        // (This would be a compile error, but shows the concept)
        
        return true;
    }
}

// Problem: Can't substitute CreditCardProcessor for IPaymentProcessor
// because it has different preconditions and throws unexpected exceptions
```

#### ✅ WITH LSP (GOOD):
```csharp
public interface IPaymentProcessor
{
    // Contract: Returns true if payment succeeds, false if it fails
    // Contract: Accepts any positive amount
    // Contract: No exceptions thrown (returns false on failure)
    Task<bool> ProcessPaymentAsync(decimal amount);
}

public class CreditCardPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        // ✅ Honors contract:
        // - Returns bool (same type)
        // - No unexpected exceptions (handles invalid input gracefully)
        // - Same preconditions (accepts any amount, doesn't require more)
        // - Same postconditions (returns true/false as expected)
        if (amount <= 0) return false; // Handle invalid input, don't throw
        
        // Process payment...
        return true;
    }
}

public class PayPalPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        // ✅ Honors contract:
        // - Returns bool (same type)
        // - No unexpected exceptions
        // - Same preconditions (accepts any amount)
        // - Same postconditions (returns true/false)
        // - Can have different business logic (amount limit) but same contract
        if (amount <= 0) return false;
        if (amount > 10000) return false; // Business rule, but doesn't change contract
        
        // Process payment...
        return true;
    }
}

// Both can be used interchangeably!
// The caller doesn't need to know which one is being used
```

### More LSP Examples

#### Example 1: Preconditions (Can't Require More)
```csharp
// ❌ BAD - Violates LSP
public interface IShape
{
    void Draw(int x, int y); // Base: accepts any x, y
}

public class Circle : IShape
{
    public void Draw(int x, int y)
    {
        if (x < 0 || y < 0) // ❌ Stronger precondition!
            throw new ArgumentException("Must be positive");
        // ...
    }
}

// ✅ GOOD - Honors LSP
public class Circle : IShape
{
    public void Draw(int x, int y)
    {
        // Accepts any x, y (same precondition as base)
        // Can validate internally but handle gracefully
        if (x < 0 || y < 0) return; // Handle gracefully, don't throw
        // ...
    }
}
```

#### Example 2: Postconditions (Must Guarantee At Least Same)
```csharp
// ✅ GOOD - Honors LSP
public interface IRepository
{
    // Contract: Returns item if found, null if not found
    Task<Order?> GetByIdAsync(int id);
}

public class OrderRepository : IRepository
{
    public async Task<Order?> GetByIdAsync(int id)
    {
        // ✅ Honors contract: Returns Order or null
        return await _context.Orders.FindAsync(id);
    }
}

public class CachedOrderRepository : IRepository
{
    public async Task<Order?> GetByIdAsync(int id)
    {
        // ✅ Same postcondition: Returns Order or null
        // Can add caching (stronger guarantee) but same contract
        var cached = _cache.Get<Order>(id);
        if (cached != null) return cached;
        
        var order = await _baseRepository.GetByIdAsync(id);
        if (order != null) _cache.Set(id, order);
        return order;
    }
}
```

### Implementation in This Project

- **`IPaymentProcessor`** - Base interface with clear contract
- **`CreditCardPaymentProcessor`** - Honors contract completely
- **`PayPalPaymentProcessor`** - Honors contract completely

**Key Points:**
- Both return `Task<bool>` (same type) ✅
- Both handle invalid input gracefully (no unexpected exceptions) ✅
- Both have same preconditions (accept any amount) ✅
- Both have same postconditions (return true/false) ✅
- Both are truly substitutable ✅

**Benefits:**
- Can swap implementations without breaking code
- Polymorphism works correctly
- Easy to test with mocks
- Caller doesn't need to know which implementation is used

### Understanding "Without Modification" - Practical Example

This is the **core concept** of LSP. Let me show you with a real example:

#### The Scenario:
You write code that uses `IPaymentProcessor`. Later, you want to switch from CreditCard to PayPal. **Your code should work without any changes!**

```csharp
// ============================================
// THIS IS YOUR CODE (the caller)
// ============================================
public class OrderController
{
    private readonly IPaymentProcessor _paymentProcessor;
    
    public OrderController(IPaymentProcessor paymentProcessor)
    {
        _paymentProcessor = paymentProcessor;
    }
    
    public async Task<ActionResult> ProcessOrder(decimal amount)
    {
        // This code works with IPaymentProcessor
        // It should work with ANY implementation!
        
        var success = await _paymentProcessor.ProcessPaymentAsync(amount);
        
        if (success)
        {
            return Ok("Payment successful");
        }
        else
        {
            return BadRequest("Payment failed");
        }
    }
}
```

#### ✅ When LSP is Followed (GOOD):

```csharp
// Implementation 1: CreditCard
public class CreditCardPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        if (amount <= 0) return false;
        // Process credit card...
        return true;
    }
}

// Implementation 2: PayPal
public class PayPalPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        if (amount <= 0) return false;
        if (amount > 10000) return false; // PayPal limit
        // Process PayPal...
        return true;
    }
}

// ============================================
// YOUR CODE WORKS WITH BOTH - NO CHANGES NEEDED!
// ============================================

// In Program.cs, you can switch implementations:
builder.Services.AddScoped<IPaymentProcessor, CreditCardPaymentProcessor>();
// OR
builder.Services.AddScoped<IPaymentProcessor, PayPalPaymentProcessor>();

// OrderController code doesn't need to change!
// It works with either implementation!
```

#### ❌ When LSP is Violated (BAD):

```csharp
// Implementation 1: CreditCard (follows LSP)
public class CreditCardPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        if (amount <= 0) return false;
        return true;
    }
}

// Implementation 2: PayPal (VIOLATES LSP)
public class PayPalPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        // ❌ VIOLATION: Throws exception instead of returning false
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        
        // ❌ VIOLATION: Requires minimum amount (stronger precondition)
        if (amount < 50) throw new InvalidOperationException("Minimum $50 required");
        
        return true;
    }
}

// ============================================
// YOUR CODE BREAKS! You must modify it!
// ============================================

public async Task<ActionResult> ProcessOrder(decimal amount)
{
    // ❌ Now you MUST add try-catch because PayPal throws exceptions!
    try
    {
        var success = await _paymentProcessor.ProcessPaymentAsync(amount);
        // ...
    }
    catch (ArgumentException ex)  // ❌ Must handle PayPal's exceptions
    {
        return BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)  // ❌ Must handle PayPal's exceptions
    {
        return BadRequest(ex.Message);
    }
    
    // ❌ Your code is now tightly coupled to PayPal's implementation!
    // ❌ If you switch back to CreditCard, the try-catch is unnecessary
    // ❌ You had to MODIFY your code because of the implementation!
}
```

### The Key Insight:

**When LSP is followed:**
- ✅ You write code once using the interface
- ✅ You can swap implementations in `Program.cs` (DI configuration)
- ✅ **Your business logic code NEVER needs to change**
- ✅ The caller doesn't know or care which implementation is used

**When LSP is violated:**
- ❌ You write code using the interface
- ❌ You try to swap implementations
- ❌ **Your business logic code BREAKS and must be modified**
- ❌ The caller must know which implementation is used (tight coupling)

### Real-World Analogy:

Think of it like electrical outlets:
- **LSP Followed:** All devices (CreditCard, PayPal) use the same plug (interface)
  - You can plug any device into the outlet
  - The outlet doesn't need to change
  - The device works the same way

- **LSP Violated:** Different devices need different plugs
  - CreditCard uses standard plug
  - PayPal needs special adapter
  - You must modify the outlet to support PayPal
  - The outlet code must know about each device type

### LSP Summary

**LSP is NOT just about:**
- ❌ Return types (that's just one part)
- ❌ No exceptions (that's just one part)

**LSP IS about:**
- ✅ **Complete behavioral substitutability**
- ✅ **Honoring the full contract** (preconditions, postconditions, invariants)
- ✅ **Being truly interchangeable** without the caller knowing which implementation is used
- ✅ **Same expectations** - if base class accepts X, derived must accept X (or more)
- ✅ **Same guarantees** - if base class guarantees Y, derived must guarantee Y (or more)

**The Golden Rule:** If code works with the base class/interface, it must work with any derived class/implementation **without modification**. You should only change the DI configuration, not your business logic!

---

## 4. Interface Segregation Principle (ISP)

> **"Clients should not be forced to depend on interfaces they do not use"**

### What It Means
Instead of one large interface, create smaller, focused interfaces. Classes should only implement what they need.

### Example in This Project

#### ❌ WITHOUT ISP (BAD):
```csharp
public interface IOrderService
{
    // BAD: One large interface with everything
    bool Validate(decimal price, int quantity);
    decimal CalculateTotal(decimal price, int quantity, decimal discount);
    Task<Order> SaveAsync(Order order);
    Task SendEmailAsync(Order order);
    Task ProcessPaymentAsync(Order order);
}

// Problem: A class that only needs validation must implement ALL methods
public class OrderValidator : IOrderService
{
    // Forced to implement methods it doesn't need!
    public decimal CalculateTotal(...) { throw new NotImplementedException(); }
    public Task<Order> SaveAsync(...) { throw new NotImplementedException(); }
    // etc.
}
```

#### ✅ WITH ISP (GOOD):
```csharp
// Small, focused interfaces
public interface IOrderValidator
{
    bool Validate(decimal price, int quantity);
}

public interface IOrderCalculator
{
    decimal CalculateTotal(decimal price, int quantity, decimal discount);
}

public interface IOrderPersistence
{
    Task<Order> SaveAsync(Order order);
}

// Classes only implement what they need
public class OrderValidator : IOrderValidator { ... }  // Only validation
public class OrderCalculator : IOrderCalculator { ... } // Only calculation
```

### Implementation in This Project

- **`IOrderValidator`** - Only validation methods
- **`IOrderCalculator`** - Only calculation methods
- **`IOrderPersistence`** - Only persistence methods
- **`IPaymentProcessor`** - Only payment processing

**Benefits:**
- Classes only implement what they need
- No forced implementation of unused methods
- Better separation of concerns

---

## 5. Dependency Inversion Principle (DIP)

> **"Depend on abstractions, not concrete classes"**

### What It Means
High-level modules should not depend on low-level modules. Both should depend on abstractions (interfaces).

### Example in This Project

#### ❌ WITHOUT DIP (BAD):
```csharp
// BAD: High-level module depends on concrete class
public class OrderService
{
    private readonly OrderRepository _repository; // Concrete class!
    
    public OrderService()
    {
        _repository = new OrderRepository(); // Direct dependency
    }
}
// Problem: Tightly coupled, hard to test, can't swap implementations
```

#### ✅ WITH DIP (GOOD):
```csharp
// GOOD: High-level module depends on abstraction
public class OrderService
{
    private readonly IOrderRepository _repository; // Interface!
    
    public OrderService(IOrderRepository repository) // Injected abstraction
    {
        _repository = repository;
    }
}

// Both high-level (OrderService) and low-level (OrderRepository)
// depend on abstraction (IOrderRepository)
```

### Implementation in This Project

- **`OrderService`** depends on `IOrderRepository` (abstraction)
- **`OrderRepository`** implements `IOrderRepository` (abstraction)
- Can swap `OrderRepository` with `MongoDBOrderRepository` without changing `OrderService`

**Benefits:**
- Loose coupling
- Easy to test (inject mocks)
- Easy to swap implementations

---

## Project Structure

```
SOLID.Orders/
├── SOLID.Orders.Domain/          # Order entity
├── SOLID.Orders.Application/     # Interfaces and services (SOLID principles)
├── SOLID.Orders.Infrastructure/ # Repository implementation
└── SOLID.Orders.API/            # Controllers
```

## SOLID Principles Summary

| Principle | What It Means | Example in Project |
|-----------|---------------|-------------------|
| **S**ingle Responsibility | One class, one job | `OrderValidator` only validates, `OrderCalculator` only calculates |
| **O**pen/Closed | Open for extension, closed for modification | Add new discount types without modifying existing code |
| **L**iskov Substitution | Subclasses must be substitutable | `CreditCardPaymentProcessor` and `PayPalPaymentProcessor` are interchangeable |
| **I**nterface Segregation | Small, focused interfaces | `IOrderValidator`, `IOrderCalculator`, `IOrderPersistence` instead of one large interface |
| **D**ependency Inversion | Depend on abstractions | `OrderService` depends on `IOrderRepository`, not `OrderRepository` |

## API Endpoints

- `GET /api/order` - Get all orders
- `GET /api/order/{id}` - Get order by ID
- `POST /api/order` - Create a new order (demonstrates all SOLID principles)

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
cd SOLID.Orders.API
dotnet run
```

The API will be available at `https://localhost:5001` (or the port shown in the console).

## Key Learning

> **"SOLID principles prevent future pain"**

Following SOLID principles makes code:
- **Maintainable** - Easy to understand and modify
- **Testable** - Easy to unit test
- **Flexible** - Easy to extend and change
- **Reusable** - Components can be reused
- **Robust** - Less prone to bugs

## Benefits Summary

1. **Single Responsibility** - Each class has one clear purpose
2. **Open/Closed** - Add features without breaking existing code
3. **Liskov Substitution** - Swap implementations safely
4. **Interface Segregation** - Use only what you need
5. **Dependency Inversion** - Depend on abstractions for flexibility

