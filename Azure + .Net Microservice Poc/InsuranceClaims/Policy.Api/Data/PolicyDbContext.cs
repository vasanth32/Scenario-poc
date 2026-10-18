using Microsoft.EntityFrameworkCore;
using Policy.Api.Models;

namespace Policy.Api.Data;

public class PolicyDbContext(DbContextOptions<PolicyDbContext> options) : DbContext(options)
{
    public DbSet<PolicyEntity> Policies => Set<PolicyEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PolicyEntity>(e =>
        {
            e.ToTable("Policies");
            e.HasKey(p => p.Id);
            e.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(50);
            e.Property(p => p.HolderName).IsRequired().HasMaxLength(200);
            e.Property(p => p.CoverageAmount).HasPrecision(18, 2);
            e.HasIndex(p => p.PolicyNumber).IsUnique();

            e.HasData(
                new PolicyEntity
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    PolicyNumber = "POL-1001",
                    HolderName = "Alice Johnson",
                    CoverageAmount = 250000m,
                    StartDate = new DateOnly(2025, 1, 1),
                    EndDate = new DateOnly(2026, 12, 31),
                    IsActive = true
                },
                new PolicyEntity
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    PolicyNumber = "POL-1002",
                    HolderName = "Bob Smith",
                    CoverageAmount = 100000m,
                    StartDate = new DateOnly(2023, 6, 1),
                    EndDate = new DateOnly(2024, 5, 31),
                    IsActive = false
                });
        });
    }
}
