using Microsoft.EntityFrameworkCore;
using CQRS.Orders.Application;
using CQRS.Orders.Application.Commands;
using CQRS.Orders.Application.Queries;
using CQRS.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Dependency Injection for CQRS

// 1. Register DbContext (Infrastructure layer)
// Using InMemory database for simplicity
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("CQRSOrdersDb"));

// 2. Register Repository for Commands (write operations)
// Commands use Domain + Repository pattern
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// 3. Register Read Repository for Queries (read operations)
// Queries use separate read repository optimized for reading
builder.Services.AddScoped<IOrderReadRepository, OrderReadRepository>();

// 4. Register Command Handlers (write path)
// Commands go through Domain entities and Repository
builder.Services.AddScoped<CreateOrderCommandHandler>();

// 5. Register Query Handlers (read path)
// Queries use read repository to get DTOs directly
builder.Services.AddScoped<GetOrdersQueryHandler>();

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
