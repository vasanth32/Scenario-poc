using Microsoft.EntityFrameworkCore;
using OptimisticConcurrencyDemo.Models;

namespace OptimisticConcurrencyDemo.Data;

public class OptimisticConcurrencyDemoDbContext : DbContext
{
    public OptimisticConcurrencyDemoDbContext(DbContextOptions<OptimisticConcurrencyDemoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.OrderId);
            entity.Property(o => o.Status).HasMaxLength(50);
            entity.Property(o => o.RowVersion).IsRowVersion();
        });
    }
}

