using Policy.Api.Models;
using Policy.Api.Repositories;

namespace Policy.Api.Services;

public enum PolicyWriteError { NotFound, DuplicatePolicyNumber }

public sealed record PolicyWriteResult(PolicyDto? Policy, PolicyWriteError? Error)
{
    public static PolicyWriteResult Success(PolicyDto policy) => new(policy, null);
    public static PolicyWriteResult Failure(PolicyWriteError error) => new(null, error);
}

public interface IPolicyService
{
    Task<IReadOnlyList<PolicyDto>> ListAsync(int page, int pageSize, CancellationToken ct);
    Task<PolicyDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PolicyWriteResult> CreateAsync(CreatePolicyRequest request, CancellationToken ct);
    Task<PolicyWriteResult> UpdateAsync(Guid id, UpdatePolicyRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class PolicyService(IPolicyRepository repository) : IPolicyService
{
    public async Task<IReadOnlyList<PolicyDto>> ListAsync(int page, int pageSize, CancellationToken ct) =>
        (await repository.ListAsync((page - 1) * pageSize, pageSize, ct)).Select(PolicyDto.From).ToList();

    public async Task<PolicyDto?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct) is { } policy ? PolicyDto.From(policy) : null;

    public async Task<PolicyWriteResult> CreateAsync(CreatePolicyRequest request, CancellationToken ct)
    {
        var number = request.PolicyNumber.Trim();
        if (await repository.PolicyNumberExistsAsync(number, null, ct))
            return PolicyWriteResult.Failure(PolicyWriteError.DuplicatePolicyNumber);

        var policy = new PolicyEntity
        {
            Id = Guid.NewGuid(),
            PolicyNumber = number,
            HolderName = request.HolderName.Trim(),
            CoverageAmount = request.CoverageAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true
        };
        await repository.AddAsync(policy, ct);
        return PolicyWriteResult.Success(PolicyDto.From(policy));
    }

    public async Task<PolicyWriteResult> UpdateAsync(Guid id, UpdatePolicyRequest request, CancellationToken ct)
    {
        var policy = await repository.GetByIdAsync(id, ct);
        if (policy is null) return PolicyWriteResult.Failure(PolicyWriteError.NotFound);

        var number = request.PolicyNumber.Trim();
        if (await repository.PolicyNumberExistsAsync(number, id, ct))
            return PolicyWriteResult.Failure(PolicyWriteError.DuplicatePolicyNumber);

        policy.PolicyNumber = number;
        policy.HolderName = request.HolderName.Trim();
        policy.CoverageAmount = request.CoverageAmount;
        policy.StartDate = request.StartDate;
        policy.EndDate = request.EndDate;
        policy.IsActive = request.IsActive;

        return await repository.UpdateAsync(policy, ct)
            ? PolicyWriteResult.Success(PolicyDto.From(policy))
            : PolicyWriteResult.Failure(PolicyWriteError.NotFound);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => repository.DeleteAsync(id, ct);
}
