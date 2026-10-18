using Claims.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Data;

public class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(e =>
        {
            e.ToTable("Claims");
            e.HasKey(c => c.Id);
            e.Property(c => c.Description).IsRequired().HasMaxLength(500);
            e.Property(c => c.Notes).HasMaxLength(1000);
            e.Property(c => c.Amount).HasPrecision(18, 2);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

            // Index only: PolicyId references another service's database, so no foreign key.
            e.HasIndex(c => c.PolicyId);

            // Supports the paged list: ORDER BY CreatedAtUtc DESC, Id.
            e.HasIndex(c => new { c.CreatedAtUtc, c.Id }).IsDescending(true, false);

            e.HasData(new Claim
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PolicyId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Description = "Rear-end collision damage",
                Amount = 4200m,
                Status = ClaimStatus.UnderReview,
                CreatedAtUtc = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc)
            });
        });
    }
}
