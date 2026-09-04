using idempotency.Models;
using Microsoft.EntityFrameworkCore;

namespace idempotency.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WireRequest> WireRequests => Set<WireRequest>();

    public DbSet<IdempotencyRecord> IdempotencyRecords =>
        Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdempotencyRecord>()
            .HasIndex(x => x.Key)
            .IsUnique();
    }
}
