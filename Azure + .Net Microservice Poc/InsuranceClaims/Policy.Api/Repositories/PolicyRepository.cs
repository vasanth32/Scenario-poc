using Microsoft.EntityFrameworkCore;
using Policy.Api.Data;
using Policy.Api.Models;

namespace Policy.Api.Repositories;

public interface IPolicyRepository
{
    Task<List<PolicyEntity>> ListAsync(int skip, int take, CancellationToken ct);
    Task<PolicyEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> PolicyNumberExistsAsync(string policyNumber, Guid? excludeId, CancellationToken ct);
    Task AddAsync(PolicyEntity policy, CancellationToken ct);
    Task<bool> UpdateAsync(PolicyEntity policy, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class EfPolicyRepository(PolicyDbContext db) : IPolicyRepository
{
    public Task<List<PolicyEntity>> ListAsync(int skip, int take, CancellationToken ct) =>
        db.Policies.AsNoTracking()
            .OrderBy(p => p.PolicyNumber)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public Task<PolicyEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> PolicyNumberExistsAsync(string policyNumber, Guid? excludeId, CancellationToken ct) =>
        db.Policies.AnyAsync(p => p.PolicyNumber == policyNumber && p.Id != excludeId, ct);

    public async Task AddAsync(PolicyEntity policy, CancellationToken ct)
    {
        db.Policies.Add(policy);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(PolicyEntity policy, CancellationToken ct)
    {
        db.Policies.Update(policy);
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
        await db.Policies.Where(p => p.Id == id).ExecuteDeleteAsync(ct) > 0;
}
