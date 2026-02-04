using Microsoft.EntityFrameworkCore;
using UnitOfWork.Orders.Application;
using UnitOfWork.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Dependency Injection for Unit of Work Pattern

// 1. Register DbContext (Infrastructure layer)
// Using InMemory database for simplicity
// IMPORTANT: Use Scoped lifetime - same instance shared by all repositories in UnitOfWork
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrdersDb"),
    ServiceLifetime.Scoped);

// 2. Register UnitOfWork
// UnitOfWork manages the DbContext and all repositories
// Scoped lifetime ensures one UnitOfWork per HTTP request
builder.Services.AddScoped<IUnitOfWork, UnitOfWork.Orders.Infrastructure.UnitOfWork>();

// 3. Register Application Services
builder.Services.AddScoped<OrderPaymentService>();

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
