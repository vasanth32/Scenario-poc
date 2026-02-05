using Microsoft.EntityFrameworkCore;
using SOLID.Orders.Domain;

namespace SOLID.Orders.Infrastructure;

/// <summary>
/// Entity Framework DbContext
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
            entity.Property(e => e.TotalAmount).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).IsRequired().HasPrecision(18, 2);
        });
    }
}

