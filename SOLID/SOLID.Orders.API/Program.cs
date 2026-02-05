using Microsoft.EntityFrameworkCore;
using SOLID.Orders.Application;
using SOLID.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================================
// SOLID PRINCIPLES DEMONSTRATION - Dependency Injection Configuration
// ============================================================================

// Register DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrdersDb"),
    ServiceLifetime.Scoped);

// ============================================================================
// SOLID: DEPENDENCY INVERSION
// ==========================
// We register interfaces (abstractions), not concrete classes.
// High-level modules depend on abstractions.
// ============================================================================
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// ============================================================================
// SOLID: SINGLE RESPONSIBILITY
// ============================
// Each service has one responsibility:
// - OrderValidator: Only validates
// - OrderCalculator: Only calculates
// - DiscountCalculator: Only calculates discounts
// ============================================================================
builder.Services.AddScoped<IOrderValidator, OrderValidator>();
builder.Services.AddScoped<IOrderCalculator, OrderCalculator>();

// ============================================================================
// SOLID: OPEN/CLOSED
// ==================
// We can add new discount calculators WITHOUT modifying existing code.
// Just register a different implementation here.
// 
// Options:
// - NoDiscountCalculator (no discount)
// - PercentageDiscountCalculator (percentage discount)
// - FixedAmountDiscountCalculator (fixed amount discount)
// 
// To add a new discount type, just create a new class implementing
// IDiscountCalculator and register it here. No need to modify existing code!
// ============================================================================
builder.Services.AddScoped<IDiscountCalculator>(provider =>
{
    // Example: Use percentage discount (10% off)
    // Can easily switch to NoDiscountCalculator or FixedAmountDiscountCalculator
    return new PercentageDiscountCalculator(10); // 10% discount
});

// ============================================================================
// SOLID: LISKOV SUBSTITUTION
// ==========================
// Both CreditCardPaymentProcessor and PayPalPaymentProcessor implement
// IPaymentProcessor and can be substituted for each other.
// 
// We can switch implementations here without breaking the code.
// ============================================================================
builder.Services.AddScoped<IPaymentProcessor, CreditCardPaymentProcessor>();
// Uncomment to use PayPal instead:
// builder.Services.AddScoped<IPaymentProcessor, PayPalPaymentProcessor>();

// ============================================================================
// SOLID: SINGLE RESPONSIBILITY (continued)
// ========================================
// OrderService orchestrates but delegates to specialized services.
// Each service injected has a single responsibility.
// ============================================================================
builder.Services.AddScoped<OrderService>();

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
