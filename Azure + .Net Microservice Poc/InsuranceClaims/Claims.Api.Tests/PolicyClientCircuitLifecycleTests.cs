using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Claims.Api.Tests;

// Walks the full lifecycle: failures -> retries -> circuit opens -> fail fast -> half-open -> recovery,
// and asserts each step is logged.
public sealed class PolicyClientCircuitLifecycleTests
{
    [Fact]
    public async Task Retries_circuit_open_and_recovery_are_each_logged()
    {
        var failing = true;
        var handler = new FakeHttpMessageHandler((_, _) => Task.FromResult(failing
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":"11111111-1111-1111-1111-111111111111","isActive":true}""",
                    System.Text.Encoding.UTF8, "application/json")
            }));

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PolicyApi:BaseUrl"] = "http://policy.test/",
            ["PolicyApi:Resilience:AttemptTimeoutSeconds"] = "1",
            ["PolicyApi:Resilience:MaxRetryAttempts"] = "1",
            ["PolicyApi:Resilience:RetryBaseDelayMs"] = "10",
            ["PolicyApi:Resilience:CircuitMinimumThroughput"] = "2",
            ["PolicyApi:Resilience:CircuitBreakDurationSeconds"] = "1"
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddFakeLogging());
        services.AddPolicyClient(config).ConfigurePrimaryHttpMessageHandler(() => handler);
        using var sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<IPolicyClient>();
        var logs = sp.GetFakeLogCollector();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // 1 attempt + 1 retry, both 503 -> failure ratio reaches the threshold -> circuit opens.
        var first = await client.GetPolicyAsync(id, CancellationToken.None);
        // Circuit is open now: this call must not reach Policy.Api.
        var callsWhenOpen = handler.Requests.Count;
        var second = await client.GetPolicyAsync(id, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, first.Status);
        Assert.Equal(PolicyLookupStatus.Unavailable, second.Status);
        Assert.Equal(callsWhenOpen, handler.Requests.Count);

        // Policy.Api recovers; after the break duration the trial call goes through and closes the circuit.
        failing = false;
        await Task.Delay(TimeSpan.FromSeconds(1.3));
        var recovered = await client.GetPolicyAsync(id, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Found, recovered.Status);

        var messages = logs.GetSnapshot().Select(l => l.Message).ToList();
        Assert.Contains(messages, m => m.StartsWith("Retry 1/1 for Policy.Api") && m.Contains("HTTP 503"));
        Assert.Contains(messages, m => m.StartsWith("Circuit OPENED for Policy.Api"));
        Assert.Contains(messages, m => m.StartsWith("Circuit open for Policy.Api, failing fast"));
        Assert.Contains(messages, m => m.StartsWith("Circuit HALF-OPEN for Policy.Api"));
        Assert.Contains(messages, m => m.StartsWith("Circuit CLOSED for Policy.Api"));
    }
}
