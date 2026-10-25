using System.Net;
using System.Net.Http.Json;
using Claims.Api.Data;
using Claims.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Api.Tests;

public sealed class ClaimsApiIntegrationTests(ClaimsApiFactory factory) : IClassFixture<ClaimsApiFactory>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();
    private readonly HttpClient _customerClient = factory.CreateAuthenticatedClient("Customer");

    private Guid NewPolicy(bool active = true)
    {
        var id = Guid.NewGuid();
        factory.Policies.Add(id, active);
        return id;
    }

    private async Task<ClaimDto> CreateClaimAsync(Guid policyId, string? notes = null)
    {
        var response = await _customerClient.PostAsJsonAsync("/api/claims", new CreateClaimRequest(policyId, "Water damage", 150m, notes));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ClaimDto>())!;
    }

    [Fact]
    public async Task Claims_require_a_valid_bearer_token()
    {
        using var anonymousClient = factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/claims");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Development_token_endpoint_issues_a_usable_customer_token()
    {
        using var client = factory.CreateClient();
        var tokenResponse = await client.PostAsJsonAsync("/api/auth/token", new { userName = "local-customer", role = "Customer" });
        var token = await tokenResponse.Content.ReadFromJsonAsync<Claims.Api.Controllers.TokenResponse>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var claimsResponse = await client.GetAsync("/api/claims");

        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, claimsResponse.StatusCode);
        Assert.Equal("Customer", token.Role);
    }

    [Fact]
    public async Task Only_adjusters_can_approve_claims()
    {
        var created = await CreateClaimAsync(NewPolicy());
        using var customerClient = factory.CreateAuthenticatedClient("Customer");

        var customerResponse = await customerClient.PatchAsync($"/api/claims/{created.Id}/approve", null);
        var adjusterResponse = await _client.PatchAsync($"/api/claims/{created.Id}/approve", null);
        var approved = await adjusterResponse.Content.ReadFromJsonAsync<ClaimDto>();

        Assert.Equal(HttpStatusCode.Forbidden, customerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adjusterResponse.StatusCode);
        Assert.Equal("Approved", approved!.Status);
    }

    [Fact]
    public async Task Migrations_create_schema_and_seed_data_in_the_fresh_database()
    {
        var seeded = await _client.GetAsync("/api/claims/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        Assert.Equal(HttpStatusCode.OK, seeded.StatusCode);
    }

    [Fact]
    public async Task Create_then_get_round_trips_including_the_column_added_by_the_AddClaimNotes_migration()
    {
        var created = await CreateClaimAsync(NewPolicy(), notes: "Called the adjuster");

        var fetched = await _client.GetFromJsonAsync<ClaimDto>($"/api/claims/{created.Id}");

        Assert.Equal("Called the adjuster", fetched!.Notes);
        Assert.Equal("Submitted", fetched.Status);
    }

    [Fact]
    public async Task Update_and_delete_work_against_the_database()
    {
        var created = await CreateClaimAsync(NewPolicy());

        var update = await _client.PutAsJsonAsync($"/api/claims/{created.Id}",
            new UpdateClaimRequest("Updated", 200m, ClaimStatus.Approved, "n"));
        var updated = await update.Content.ReadFromJsonAsync<ClaimDto>();
        var delete = await _client.DeleteAsync($"/api/claims/{created.Id}");
        var afterDelete = await _client.GetAsync($"/api/claims/{created.Id}");

        Assert.Equal("Approved", updated!.Status);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task Create_maps_policy_problems_to_422_and_503()
    {
        var inactive = await _customerClient.PostAsJsonAsync("/api/claims", new CreateClaimRequest(NewPolicy(active: false), "x", 1m));
        var missing = await _customerClient.PostAsJsonAsync("/api/claims", new CreateClaimRequest(Guid.NewGuid(), "x", 1m));

        factory.Policies.Down = true;
        try
        {
            var down = await _customerClient.PostAsJsonAsync("/api/claims", new CreateClaimRequest(NewPolicy(), "x", 1m));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, down.StatusCode);
        }
        finally { factory.Policies.Down = false; }

        Assert.Equal(HttpStatusCode.UnprocessableEntity, inactive.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missing.StatusCode);
    }

    [Fact]
    public async Task Claims_schema_has_no_foreign_key_to_the_policy_database()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        Assert.Empty(db.Model.FindEntityType(typeof(Claim))!.GetForeignKeys());
    }

    // The scenario from the prompt: a policy is deleted in Policy.Api after a claim was filed against it.
    [Fact]
    public async Task Claim_whose_policy_was_later_deleted_is_orphaned_but_still_readable_and_updatable()
    {
        var policyId = NewPolicy();
        var claim = await CreateClaimAsync(policyId);

        factory.Policies.Remove(policyId); // Policy.Api deleted it; Claims.Api is never told.

        // Nothing in ClaimsDb stops this: no FK, so the claim silently dangles.
        var get = await _client.GetAsync($"/api/claims/{claim.Id}");
        var update = await _client.PutAsJsonAsync($"/api/claims/{claim.Id}",
            new UpdateClaimRequest("Still processing", 150m, ClaimStatus.UnderReview));
        // New claims against the deleted policy are rejected, because creation re-validates with Policy.Api.
        var newClaim = await _customerClient.PostAsJsonAsync("/api/claims", new CreateClaimRequest(policyId, "Second claim", 10m));

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(policyId, (await get.Content.ReadFromJsonAsync<ClaimDto>())!.PolicyId);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, newClaim.StatusCode);
    }
}
