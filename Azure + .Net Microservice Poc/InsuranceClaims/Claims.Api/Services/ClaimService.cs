using Claims.Api.Models;
using Claims.Api.Repositories;

namespace Claims.Api.Services;

public enum CreateClaimError { PolicyNotFound, PolicyInactive, PolicyServiceUnavailable }

public sealed record CreateClaimResult(ClaimDto? Claim, CreateClaimError? Error)
{
    public static CreateClaimResult Success(ClaimDto claim) => new(claim, null);
    public static CreateClaimResult Failure(CreateClaimError error) => new(null, error);
}

public interface IClaimService
{
    Task<IReadOnlyList<ClaimDto>> ListAsync(int page, int pageSize, CancellationToken ct);
    Task<ClaimDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<CreateClaimResult> CreateAsync(CreateClaimRequest request, CancellationToken ct);
    Task<ClaimDto?> UpdateAsync(Guid id, UpdateClaimRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class ClaimService(IClaimRepository repository, IPolicyClient policies) : IClaimService
{
    public async Task<IReadOnlyList<ClaimDto>> ListAsync(int page, int pageSize, CancellationToken ct) =>
        (await repository.ListAsync((page - 1) * pageSize, pageSize, ct)).Select(ClaimDto.From).ToList();

    public async Task<ClaimDto?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct) is { } claim ? ClaimDto.From(claim) : null;

    public async Task<CreateClaimResult> CreateAsync(CreateClaimRequest request, CancellationToken ct)
    {
        var lookup = await policies.GetPolicyAsync(request.PolicyId, ct);

        switch (lookup)
        {
            case { Status: PolicyLookupStatus.NotFound }:
                return CreateClaimResult.Failure(CreateClaimError.PolicyNotFound);
            case { Status: PolicyLookupStatus.Unavailable }:
                return CreateClaimResult.Failure(CreateClaimError.PolicyServiceUnavailable);
            case { Policy.IsActive: false } or { Policy: null }:
                return CreateClaimResult.Failure(CreateClaimError.PolicyInactive);
        }

        var claim = new Claim
        {
            Id = Guid.NewGuid(),
            PolicyId = request.PolicyId,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Status = ClaimStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow,
            Notes = request.Notes?.Trim()
        };
        await repository.AddAsync(claim, ct);
        return CreateClaimResult.Success(ClaimDto.From(claim));
    }

    // PolicyId is immutable after creation, so updates don't need to call Policy.Api.
    public async Task<ClaimDto?> UpdateAsync(Guid id, UpdateClaimRequest request, CancellationToken ct)
    {
        var claim = await repository.GetByIdAsync(id, ct);
        if (claim is null) return null;

        claim.Description = request.Description.Trim();
        claim.Amount = request.Amount;
        claim.Status = request.Status;
        claim.Notes = request.Notes?.Trim();

        return await repository.UpdateAsync(claim, ct) ? ClaimDto.From(claim) : null;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => repository.DeleteAsync(id, ct);
}
