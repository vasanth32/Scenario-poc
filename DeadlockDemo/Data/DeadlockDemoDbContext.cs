using DeadlockDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace DeadlockDemo.Data;

public class DeadlockDemoDbContext : DbContext
{
    public DeadlockDemoDbContext(DbContextOptions<DeadlockDemoDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Inventory> Inventory => Set<Inventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.OrderId);
            entity.Property(o => o.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(i => i.ProductId);
        });
    }
}


