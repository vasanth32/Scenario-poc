using System.Net;
using Claims.Api.Messaging;
using Claims.Api.Models;
using Claims.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Claims.Api.Tests;

// Exercises ClaimService end to end over a real PolicyClient backed by a fake handler.
public class ClaimServiceTests
{
    private static readonly Guid PolicyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly CreateClaimRequest Request = new(PolicyId, " Broken window ", 100m);

    private static (ClaimService Service, FakeClaimRepository Repo) Create(FakeHttpMessageHandler handler)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://policy.test/") };
        var repo = new FakeClaimRepository();
        return (new ClaimService(
            repo,
            new PolicyClient(http, NullLogger<PolicyClient>.Instance),
            new InMemoryEventPublisher()), repo);
    }

    [Fact]
    public async Task Active_policy_creates_claim()
    {
        var (service, repo) = Create(FakeHttpMessageHandler.Responding(HttpStatusCode.OK, $$"""{"id":"{{PolicyId}}","isActive":true}"""));

        var result = await service.CreateAsync(Request, CancellationToken.None);

        Assert.Null(result.Error);
        Assert.Equal("Broken window", result.Claim!.Description);
        Assert.NotNull(await repo.GetByIdAsync(result.Claim.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Inactive_policy_is_rejected()
    {
        var (service, _) = Create(FakeHttpMessageHandler.Responding(HttpStatusCode.OK, $$"""{"id":"{{PolicyId}}","isActive":false}"""));

        var result = await service.CreateAsync(Request, CancellationToken.None);

        Assert.Equal(CreateClaimError.PolicyInactive, result.Error);
    }

    [Fact]
    public async Task Missing_policy_is_rejected()
    {
        var (service, _) = Create(FakeHttpMessageHandler.Responding(HttpStatusCode.NotFound));

        var result = await service.CreateAsync(Request, CancellationToken.None);

        Assert.Equal(CreateClaimError.PolicyNotFound, result.Error);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Policy_server_errors_map_to_unavailable_and_no_claim_is_stored(HttpStatusCode status)
    {
        var (service, repo) = Create(FakeHttpMessageHandler.Responding(status));

        var result = await service.CreateAsync(Request, CancellationToken.None);

        Assert.Equal(CreateClaimError.PolicyServiceUnavailable, result.Error);
        Assert.Null(result.Claim);
    }

    [Fact]
    public async Task Policy_down_maps_to_unavailable()
    {
        var (service, _) = Create(FakeHttpMessageHandler.Throwing(new HttpRequestException("refused")));

        var result = await service.CreateAsync(Request, CancellationToken.None);

        Assert.Equal(CreateClaimError.PolicyServiceUnavailable, result.Error);
    }

    [Fact]
    public async Task Policy_timeout_maps_to_unavailable()
    {
        var (service, _) = Create(FakeHttpMessageHandler.Throwing(new TaskCanceledException()));

        var result = await service.CreateAsync(Request, CancellationToken.None);

        Assert.Equal(CreateClaimError.PolicyServiceUnavailable, result.Error);
    }

    [Fact]
    public async Task Update_changes_fields_and_does_not_call_Policy_api()
    {
        var handler = FakeHttpMessageHandler.Responding(HttpStatusCode.OK, $$"""{"id":"{{PolicyId}}","isActive":true}""");
        var (service, _) = Create(handler);
        var created = (await service.CreateAsync(Request, CancellationToken.None)).Claim!;
        handler.Requests.Clear();

        var updated = await service.UpdateAsync(created.Id,
            new UpdateClaimRequest(" Updated ", 250m, ClaimStatus.Approved), CancellationToken.None);

        Assert.Equal("Updated", updated!.Description);
        Assert.Equal(250m, updated.Amount);
        Assert.Equal("Approved", updated.Status);
        Assert.Equal(created.PolicyId, updated.PolicyId);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Update_unknown_claim_returns_null()
    {
        var (service, _) = Create(FakeHttpMessageHandler.Responding(HttpStatusCode.OK));

        var updated = await service.UpdateAsync(Guid.NewGuid(),
            new UpdateClaimRequest("x", 1m, ClaimStatus.Approved), CancellationToken.None);

        Assert.Null(updated);
    }

    [Fact]
    public async Task Delete_removes_claim_once()
    {
        var (service, _) = Create(FakeHttpMessageHandler.Responding(HttpStatusCode.OK, $$"""{"id":"{{PolicyId}}","isActive":true}"""));
        var created = (await service.CreateAsync(Request, CancellationToken.None)).Claim!;

        Assert.True(await service.DeleteAsync(created.Id, CancellationToken.None));
        Assert.False(await service.DeleteAsync(created.Id, CancellationToken.None));
        Assert.Null(await service.GetByIdAsync(created.Id, CancellationToken.None));
    }
}
