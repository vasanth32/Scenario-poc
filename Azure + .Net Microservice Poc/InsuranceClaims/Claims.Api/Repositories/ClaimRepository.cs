using Claims.Api.Data;
using Claims.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Repositories;

public interface IClaimRepository
{
    Task<List<Claim>> ListAsync(int skip, int take, CancellationToken ct);
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Claim claim, CancellationToken ct);
    Task<bool> UpdateAsync(Claim claim, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class EfClaimRepository(ClaimsDbContext db) : IClaimRepository
{
    public Task<List<Claim>> ListAsync(int skip, int take, CancellationToken ct) =>
        db.Claims.AsNoTracking()
            .OrderByDescending(c => c.CreatedAtUtc).ThenBy(c => c.Id)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Claims.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Claim claim, CancellationToken ct)
    {
        db.Claims.Add(claim);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(Claim claim, CancellationToken ct)
    {
        db.Claims.Update(claim);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false; // row was deleted between read and write
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.Claims.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0;
}
