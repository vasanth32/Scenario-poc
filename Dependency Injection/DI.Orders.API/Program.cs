using Microsoft.EntityFrameworkCore;
using DI.Orders.Application;
using DI.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================================
// DEPENDENCY INJECTION CONFIGURATION
// ============================================================================
//
// WHAT IS DEPENDENCY INJECTION?
// =============================
// Dependency Injection (DI) is a design pattern where objects receive their
// dependencies from an external source (DI container) rather than creating
// them internally.
//
// BENEFITS:
// 1. Loose Coupling - Classes depend on interfaces, not concrete implementations
// 2. Testability - Easy to inject mocks for testing
// 3. Maintainability - Changes to implementations don't affect dependent classes
// 4. Lifetime Management - DI container manages object creation and disposal
//
// WHAT HAPPENS WITHOUT DI?
// ========================
// Without DI, you would write code like this:
// ```csharp
// public class OrderController
// {
//     private readonly OrderService _service;
//     
//     public OrderController()
//     {
//         // Manual object creation - BAD!
//         var context = new OrderDbContext(...);
//         var repository = new OrderRepository(context);
//         _service = new OrderService(repository);
//     }
// }
// ```
// Problems:
// - Tight coupling (hard to change implementations)
// - Hard to test (can't inject mocks)
// - Manual lifetime management
// - Violates Single Responsibility Principle
//
// WITH DI:
// =======
// ```csharp
// public class OrderController
// {
//     private readonly OrderService _service;
//     
//     public OrderController(OrderService service)  // Injected!
//     {
//         _service = service;  // Framework provides it
//     }
// }
// ```
// Benefits:
// - Loose coupling
// - Easy to test
// - Automatic lifetime management
// - Clean separation of concerns
//
// ============================================================================
// SERVICE LIFETIMES
// ============================================================================
//
// ASP.NET Core provides three service lifetimes:
//
// 1. TRANSIENT
//    - New instance created EVERY TIME it's requested
//    - Use for: Lightweight, stateless services
//    - Example: Utility classes, calculators, validators
//
// 2. SCOPED
//    - One instance per HTTP request (or scope)
//    - Created at the start of request, disposed at the end
//    - Use for: DbContext, repositories, services that need request-scoped data
//    - Example: OrderDbContext, OrderRepository, OrderService
//
// 3. SINGLETON
//    - One instance for the entire application lifetime
//    - Created once when app starts, disposed when app shuts down
//    - Use for: Configuration, caching, logging services
//    - Example: Configuration service, cache manager
//
// ============================================================================
// LIFETIME EXAMPLES
// ============================================================================
//
// TRANSIENT EXAMPLE:
// =================
// ```csharp
// builder.Services.AddTransient<IEmailValidator, EmailValidator>();
// ```
// Scenario: EmailValidator
// - Request 1: Creates EmailValidator instance A
// - Request 1: Creates EmailValidator instance B (different instance)
// - Request 2: Creates EmailValidator instance C (different instance)
// - Each time you ask for IEmailValidator, you get a NEW instance
// - Use when: Service is stateless and lightweight
//
// SCOPED EXAMPLE:
// ===============
// ```csharp
// builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// ```
// Scenario: OrderRepository
// - Request 1: Creates OrderRepository instance A
// - Request 1: Same instance A is reused throughout the request
// - Request 2: Creates OrderRepository instance B (new instance)
// - Request 2: Same instance B is reused throughout the request
// - One instance per HTTP request
// - Use when: Service needs to maintain state during a request (like DbContext)
//
// SINGLETON EXAMPLE:
// =================
// ```csharp
// builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
// ```
// Scenario: ConfigurationService
// - App starts: Creates ConfigurationService instance A
// - Request 1: Uses instance A
// - Request 2: Uses instance A (same instance)
// - Request 3: Uses instance A (same instance)
// - App shuts down: Disposes instance A
// - One instance for entire application lifetime
// - Use when: Service is stateless and expensive to create
//
// ============================================================================
// REGISTERING SERVICES
// ============================================================================

// 1. Register DbContext with SCOPED lifetime
// ===========================================
// Why Scoped for DbContext?
// - One instance per HTTP request
// - All repositories in the same request share the same DbContext
// - Automatically disposed at the end of the request
// - Thread-safe within a single request
// - Prevents connection leaks
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrdersDb"),
    ServiceLifetime.Scoped);  // Explicitly set to Scoped (this is the default)

// 2. Register Repository with SCOPED lifetime
// =============================================
// Why Scoped for Repository?
// - One instance per HTTP request
// - Shares the same DbContext instance within the request
// - Automatically disposed at the end of the request
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// 3. Register Service with SCOPED lifetime
// =========================================
// Why Scoped for Service?
// - One instance per HTTP request
// - Shares the same repository instance within the request
// - Automatically disposed at the end of the request
builder.Services.AddScoped<OrderService>();

// ============================================================================
// LIFETIME SCENARIOS - WHEN TO USE WHICH?
// ============================================================================
//
// USE TRANSIENT WHEN:
// - Service is stateless
// - Service is lightweight
// - Service doesn't need to share state
// - Examples: Validators, Calculators, Mappers
//
// USE SCOPED WHEN:
// - Service needs to maintain state during a request
// - Service uses DbContext (MUST be Scoped)
// - Service is request-specific
// - Examples: DbContext, Repositories, Application Services
//
// USE SINGLETON WHEN:
// - Service is stateless
// - Service is expensive to create
// - Service needs to be shared across all requests
// - Examples: Configuration, Cache, Logging
//
// IMPORTANT RULES:
// ================
// 1. NEVER register DbContext as Singleton (causes connection leaks)
// 2. NEVER register Scoped service as dependency of Singleton (causes issues)
// 3. Scoped services can depend on Transient or Scoped
// 4. Singleton services can only depend on Singleton or Transient
//
// ============================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
