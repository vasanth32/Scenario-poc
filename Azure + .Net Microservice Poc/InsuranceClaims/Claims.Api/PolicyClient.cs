using System.Net;
using System.Text.Json;
using Polly.CircuitBreaker;
using Polly.Timeout;

public enum PolicyLookupStatus { Found, NotFound, Unavailable }

public sealed record PolicyLookupResult(PolicyLookupStatus Status, PolicySummary? Policy = null);

// Claims' own view of a policy: only the fields it needs, not shared with Policy.Api.
public sealed record PolicySummary(Guid Id, bool IsActive);

public interface IPolicyClient
{
    Task<PolicyLookupResult> GetPolicyAsync(Guid id, CancellationToken ct);
}

public sealed class PolicyClient(HttpClient http, ILogger<PolicyClient> logger) : IPolicyClient
{
    private static readonly PolicyLookupResult Unavailable = new(PolicyLookupStatus.Unavailable);

    public async Task<PolicyLookupResult> GetPolicyAsync(Guid id, CancellationToken ct)
    {
        try
        {
            using var response = await http.GetAsync($"api/policies/{id}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation("Policy {PolicyId} not found in Policy.Api", id);
                return new PolicyLookupResult(PolicyLookupStatus.NotFound);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Policy.Api returned {StatusCode} for policy {PolicyId}", (int)response.StatusCode, id);
                return Unavailable;
            }

            var policy = await response.Content.ReadFromJsonAsync<PolicySummary>(ct);
            if (policy is null)
            {
                logger.LogError("Policy.Api returned an empty body for policy {PolicyId}", id);
                return Unavailable;
            }

            return new PolicyLookupResult(PolicyLookupStatus.Found, policy);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (BrokenCircuitException ex)
        {
            logger.LogWarning(ex, "Circuit open for Policy.Api, failing fast for policy {PolicyId}", id);
            return Unavailable;
        }
        catch (TimeoutRejectedException ex)
        {
            logger.LogError(ex, "Timed out calling Policy.Api for policy {PolicyId}", id);
            return Unavailable;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Timed out calling Policy.Api for policy {PolicyId}", id);
            return Unavailable;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Policy.Api is unreachable for policy {PolicyId}", id);
            return Unavailable;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            logger.LogError(ex, "Policy.Api returned an invalid response for policy {PolicyId}", id);
            return Unavailable;
        }
    }
}
