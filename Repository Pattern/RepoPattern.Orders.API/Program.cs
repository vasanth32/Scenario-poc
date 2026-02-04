using Microsoft.EntityFrameworkCore;
using RepoPattern.Orders.Application;
using RepoPattern.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Dependency Injection for Repository Pattern

// 1. Register DbContext (Infrastructure layer)
// Using InMemory database for simplicity
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrdersDb"));

// 2. Register Repository Implementation
// This is where the magic happens:
// - We register OrderRepository (concrete class) for IOrderRepository (interface)
// - When controller asks for IOrderRepository, DI container provides OrderRepository
// - If we want to swap databases, we just change this line to register a different implementation
//   Example: builder.Services.AddScoped<IOrderRepository, MongoOrderRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

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
