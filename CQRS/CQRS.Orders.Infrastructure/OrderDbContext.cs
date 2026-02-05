using Microsoft.EntityFrameworkCore;
using CQRS.Orders.Domain;

namespace CQRS.Orders.Infrastructure;

/// <summary>
/// Entity Framework DbContext
/// This is in Infrastructure layer because it's a data access concern
/// 
/// In CQRS:
/// - Commands use this through Repository (write path)
/// - Queries use this directly (read path) for performance
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
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}

