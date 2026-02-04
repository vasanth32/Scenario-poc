using Microsoft.EntityFrameworkCore;
using CleanArch.Orders.Domain;

namespace CleanArch.Orders.Infrastructure;

/// <summary>
/// Entity Framework DbContext
/// This is in Infrastructure layer because it's a data access concern
/// 
/// Dependency Flow:
/// - Infrastructure depends on Domain (uses Order entity)
/// - Infrastructure depends on Application (implements IOrderRepository)
/// - Infrastructure uses EF Core (external framework)
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

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Price).IsRequired().HasPrecision(18, 2);
        });
    }
}

