using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Claims.Api.Controllers;

namespace Claims.Api.Tests;

public sealed class JwtAuthenticationTests(ClaimsApiFactory factory) : IClassFixture<ClaimsApiFactory>
{
    [Fact]
    public async Task No_token_returns_401()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Expired_token_returns_401()
    {
        var token = ClaimsApiFactory.CreateToken("Adjuster", DateTime.UtcNow.AddMinutes(-2));
        using var client = CreateClientWithToken(token);

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_role_returns_403()
    {
        using var client = factory.CreateAuthenticatedClient("Customer");

        var response = await client.PatchAsync($"/api/claims/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Tampered_signature_returns_401()
    {
        var tokenParts = ClaimsApiFactory.CreateToken("Adjuster").Split('.');
        tokenParts[2] = (tokenParts[2][0] == 'A' ? 'B' : 'A') + tokenParts[2][1..];
        using var client = CreateClientWithToken(string.Join('.', tokenParts));

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Valid_token_returns_200_and_controller_reads_claims()
    {
        using var client = factory.CreateAuthenticatedClient("Adjuster");

        var response = await client.GetAsync("/api/auth/me");
        var currentUser = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("test-adjuster", currentUser!.Subject);
        Assert.Equal("Test Adjuster", currentUser.Name);
        Assert.Contains("Adjuster", currentUser.Roles);
    }

    private HttpClient CreateClientWithToken(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}