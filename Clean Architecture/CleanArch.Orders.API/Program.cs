using Microsoft.EntityFrameworkCore;
using CleanArch.Orders.Application;
using CleanArch.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Dependency Injection
// This is where we wire up all the layers

// 1. Register DbContext (Infrastructure layer)
// Using InMemory database for simplicity
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrdersDb"));

// 2. Register Repository Implementation (Infrastructure) for Interface (Application)
// Dependency Flow: API -> Application (interface) <- Infrastructure (implementation)
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// 3. Register Application Service (Application layer)
// API depends on Application layer
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
