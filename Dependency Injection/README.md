# Dependency Injection - Orders API

A simple .NET 8 Web API demonstrating Dependency Injection (DI) pattern.

## What is Dependency Injection?

**Dependency Injection (DI)** is a design pattern where objects receive their dependencies from an external source (DI container) rather than creating them internally.

## Key Concept: What Happens Without DI?

### ❌ WITHOUT Dependency Injection (BAD):
```csharp
public class OrderController : ControllerBase
{
    private readonly OrderService _service;
    
    public OrderController()
    {
        // BAD: Manual object creation
        var context = new OrderDbContext(...);
        var repository = new OrderRepository(context);
        _service = new OrderService(repository);
        
        // Problems:
        // 1. Tight coupling - hard to change implementations
        // 2. Hard to test - can't inject mocks
        // 3. Manual lifetime management
        // 4. Violates Single Responsibility Principle
    }
}
```

### ✅ WITH Dependency Injection (GOOD):
```csharp
public class OrderController : ControllerBase
{
    private readonly OrderService _service;
    
    // GOOD: Dependencies injected via constructor
    public OrderController(OrderService service)
    {
        _service = service;  // Framework provides it!
        
        // Benefits:
        // 1. Loose coupling
        // 2. Easy to test (inject mock service)
        // 3. Automatic lifetime management
        // 4. Clean separation of concerns
    }
}
```

## Constructor Injection

We use **Constructor Injection** - dependencies are provided through constructor parameters.

**Benefits:**
- Dependencies are explicit and required
- Easy to test (can pass mocks in tests)
- Most common and recommended approach
- Makes dependencies clear

## Service Lifetimes

ASP.NET Core provides **three service lifetimes**:

### 1. TRANSIENT
- **New instance** created **EVERY TIME** it's requested
- **Use for:**
  - Lightweight, stateless services that don't maintain any state
  - Services that are cheap to create and don't need to share data
  - Utility classes and helper services
  - Services that process data without storing it
  - Formatters, converters, and transformers
  - Validation services that check input data
  - Calculation services that perform computations
  - Mapping services that transform objects
  - String manipulation utilities
  - Date/time helpers
  - Math utilities
  - File path helpers
  - Encoding/decoding services
- **Real-time Project Scenarios:**
  - **E-commerce:** `PriceCalculator`, `TaxCalculator`, `DiscountCalculator` - Calculate prices/taxes for each order independently
  - **Social Media:** `PostValidator`, `CommentValidator`, `UserInputSanitizer` - Validate user input without storing state
  - **Banking:** `AccountNumberValidator`, `IBANFormatter`, `CurrencyConverter` - Format/validate financial data
  - **Healthcare:** `BMI Calculator`, `DosageCalculator`, `AgeCalculator` - Calculate medical metrics
  - **E-learning:** `ScoreCalculator`, `GradeCalculator`, `TimeFormatter` - Calculate grades and format time
  - **Logistics:** `DistanceCalculator`, `ShippingCostCalculator`, `RouteOptimizer` - Calculate shipping costs
  - **Content Management:** `MarkdownConverter`, `HTMLSanitizer`, `ImageResizer` - Transform content
- **Examples:** `EmailValidator`, `PhoneValidator`, `PriceCalculator`, `TaxCalculator`, `OrderMapper`, `UserMapper`, `DateFormatter`, `StringHelper`

```csharp
builder.Services.AddTransient<IEmailValidator, EmailValidator>();
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();
builder.Services.AddTransient<IOrderMapper, OrderMapper>();

// Request 1: Creates instance A
// Request 1: Creates instance B (different)
// Request 2: Creates instance C (different)
```

### 2. SCOPED ⭐ (Most Common)
- **One instance per HTTP request**
- Created at the start of request, disposed at the end
- **Use for:**
  - Entity Framework DbContext (MUST be Scoped to prevent connection leaks)
  - Repository implementations that need to share the same DbContext
  - Application services and business logic services
  - Services that need to maintain state during a single request
  - Services that work with request-specific data
  - Services that need transaction consistency within a request
  - Unit of Work pattern implementations
  - Services that track user session data during a request
  - Services that need to share data between multiple operations in the same request
- **Real-time Project Scenarios:**
  - **E-commerce:** `ShoppingCartService`, `OrderService`, `PaymentService` - Manage cart/order state during checkout process
  - **Banking:** `TransactionService`, `AccountService`, `TransferService` - Handle financial transactions with database consistency
  - **Healthcare:** `PatientService`, `AppointmentService`, `PrescriptionService` - Manage patient data within a single request
  - **Social Media:** `PostService`, `CommentService`, `NotificationService` - Handle user interactions with shared DbContext
  - **E-learning:** `CourseService`, `EnrollmentService`, `ProgressService` - Track student progress during a session
  - **Logistics:** `ShipmentService`, `TrackingService`, `DeliveryService` - Manage shipment state during processing
  - **HR System:** `EmployeeService`, `PayrollService`, `LeaveService` - Handle employee operations with transaction safety
  - **Inventory Management:** `StockService`, `WarehouseService`, `InventoryService` - Manage inventory with consistent state
- **Examples:** `OrderDbContext`, `OrderRepository`, `OrderService`, `PaymentService`, `UserService`, `ShoppingCartService`, `UnitOfWork`

```csharp
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ShoppingCartService>();

// Request 1: Creates instance A, reuses A throughout request
// Request 2: Creates instance B, reuses B throughout request
```

### 3. SINGLETON
- **One instance** for the **entire application lifetime**
- Created once when app starts, disposed when app shuts down
- **Use for:**
  - Configuration services that load settings at startup
  - Caching services that need to be shared across all requests
  - Logging services that write to files or external systems
  - Services that are expensive to create (database connections, HTTP clients)
  - Services that maintain application-wide state
  - Background service coordinators
  - Service locators and factories (when appropriate)
  - Metrics collectors and monitoring services
  - Health check services
  - Feature flag services
  - Rate limiting services
  - API key management services
- **Real-time Project Scenarios:**
  - **E-commerce:** `ConfigurationService`, `CacheManager`, `RedisCacheService` - Share cache and config across all users
  - **Banking:** `RateLimiter`, `SecurityPolicyService`, `AuditLogger` - Enforce security policies application-wide
  - **Healthcare:** `SystemConfigService`, `HealthMonitorService`, `AlertService` - Monitor system health globally
  - **Social Media:** `FeatureFlagService`, `NotificationQueueService`, `AnalyticsService` - Track metrics across all users
  - **E-learning:** `CourseCatalogCache`, `SystemSettingsService`, `LicenseService` - Cache course data for all students
  - **Logistics:** `ShippingRateCache`, `RouteCacheService`, `WeatherService` - Cache shipping rates and routes
  - **HR System:** `CompanyPolicyService`, `HolidayCalendarService`, `SystemNotificationService` - Share company-wide policies
  - **Inventory Management:** `ProductCatalogCache`, `SupplierService`, `PriceUpdateService` - Cache product data globally
  - **Microservices:** `ServiceDiscoveryClient`, `CircuitBreaker`, `DistributedCache` - Manage inter-service communication
  - **API Gateway:** `ApiKeyValidator`, `RateLimiter`, `RequestLogger` - Enforce API policies across all requests
- **Examples:** `ConfigurationService`, `CacheManager`, `ILogger`, `ApplicationSettings`, `BackgroundJobService`, `MetricsCollector`, `HealthCheckService`, `FeatureFlagService`, `RateLimiter`

```csharp
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<ICacheManager, CacheManager>();
builder.Services.AddSingleton<IFeatureFlagService, FeatureFlagService>();

// App starts: Creates instance A
// All requests: Use instance A (same instance)
// App shuts down: Disposes instance A
```

## When to Use Which Lifetime?

### Use TRANSIENT When:
- ✅ Service is stateless (doesn't store any data)
- ✅ Service is lightweight (cheap to create)
- ✅ Service doesn't need to share state between calls
- ✅ Service processes data independently each time
- ✅ **Real-world examples:**
  - Calculating prices, taxes, discounts for each order
  - Validating user input without storing validation state
  - Converting/formatting data (JSON to XML, date formatting)
  - Performing calculations (BMI, distance, shipping cost)
- ✅ Examples: `EmailValidator`, `PriceCalculator`, `TaxCalculator`, `OrderMapper`, `DateFormatter`

### Use SCOPED When:
- ✅ Service needs to maintain state during a request
- ✅ Service uses DbContext (MUST be Scoped to prevent connection leaks)
- ✅ Service is request-specific (different data per user/request)
- ✅ Service needs transaction consistency within a request
- ✅ Multiple services need to share the same DbContext instance
- ✅ **Real-world examples:**
  - Managing shopping cart during checkout process
  - Processing a financial transaction with multiple database operations
  - Handling a user's order creation with payment processing
  - Tracking user session data during a single request
- ✅ Examples: `OrderDbContext`, `OrderRepository`, `OrderService`, `ShoppingCartService`, `PaymentService`, `UnitOfWork`

### Use SINGLETON When:
- ✅ Service is stateless but expensive to create
- ✅ Service needs to be shared across ALL requests
- ✅ Service maintains application-wide state or cache
- ✅ Service loads configuration or data at startup
- ✅ **Real-world examples:**
  - Caching product catalog that all users need
  - Storing application configuration loaded at startup
  - Managing feature flags that apply to all users
  - Rate limiting service that tracks requests globally
  - Health monitoring service that checks system status
- ✅ Examples: `ConfigurationService`, `CacheManager`, `FeatureFlagService`, `RateLimiter`, `HealthCheckService`, `ILogger`

## Important Rules

1. **NEVER** register DbContext as Singleton (causes connection leaks)
2. **NEVER** register Scoped service as dependency of Singleton (causes issues)
3. Scoped services can depend on Transient or Scoped
4. Singleton services can only depend on Singleton or Transient

## Dependency Flow in This Project

```
Controller (Scoped)
  ↓ injects
OrderService (Scoped)
  ↓ injects
IOrderRepository (Scoped)
  ↓ injects
OrderRepository (Scoped)
  ↓ injects
OrderDbContext (Scoped)
```

All services are **Scoped** - one instance per HTTP request, all sharing the same DbContext.

## Project Structure

```
DI.Orders/
├── DI.Orders.Domain/          # Order entity
├── DI.Orders.Application/     # IOrderRepository interface + OrderService
├── DI.Orders.Infrastructure/ # OrderRepository + OrderDbContext
└── DI.Orders.API/            # Controller (uses DI)
```

## API Endpoints

- `GET /api/order` - Get all orders
- `GET /api/order/{id}` - Get order by ID
- `POST /api/order` - Create a new order

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
cd DI.Orders.API
dotnet run
```

The API will be available at `https://localhost:5001` (or the port shown in the console).

## Key Learning

> **"Classes should not create their own dependencies"**

Dependency Injection makes code:
- **Testable** - Easy to inject mocks
- **Flexible** - Easy to swap implementations
- **Maintainable** - Clear dependencies
- **Clean** - Separation of concerns

## Benefits Summary

1. **Loose Coupling** - Depend on interfaces, not concrete classes
2. **Testability** - Inject mocks for unit testing
3. **Maintainability** - Changes don't cascade
4. **Lifetime Management** - Automatic creation and disposal
5. **Single Responsibility** - Classes don't create dependencies

