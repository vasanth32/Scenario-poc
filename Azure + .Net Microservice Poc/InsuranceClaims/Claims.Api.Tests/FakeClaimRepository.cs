using Claims.Api.Models;
using Claims.Api.Repositories;

namespace Claims.Api.Tests;

// In-memory stand-in for the EF repository so service tests need no database.
public sealed class FakeClaimRepository : IClaimRepository
{
    private readonly List<Claim> _claims = [];

    public Task<List<Claim>> ListAsync(int skip, int take, CancellationToken ct) =>
        Task.FromResult(_claims.OrderByDescending(c => c.CreatedAtUtc).Skip(skip).Take(take).ToList());

    public Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_claims.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(Claim claim, CancellationToken ct)
    {
        _claims.Add(claim);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Claim claim, CancellationToken ct) =>
        Task.FromResult(_claims.Any(c => c.Id == claim.Id));

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_claims.RemoveAll(c => c.Id == id) > 0);
}
