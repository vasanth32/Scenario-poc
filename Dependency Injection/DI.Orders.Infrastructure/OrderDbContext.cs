using Microsoft.EntityFrameworkCore;
using DI.Orders.Domain;

namespace DI.Orders.Infrastructure;

/// <summary>
/// Entity Framework DbContext
/// 
/// LIFETIME: SCOPED (configured in Program.cs)
/// 
/// Why Scoped for DbContext?
/// - One instance per HTTP request
/// - All repositories in the same request share the same DbContext
/// - Automatically disposed at the end of the request
/// - Thread-safe within a single request
/// </summary>
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Price).IsRequired().HasPrecision(18, 2);
        });
    }
}

