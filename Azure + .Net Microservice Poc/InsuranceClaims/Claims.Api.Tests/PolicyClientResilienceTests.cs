using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Claims.Api.Tests;

// Exercises the real resilience pipeline from AddPolicyClient, with a fake handler as the "network".
public sealed class PolicyClientResilienceTests
{
    private static readonly Guid PolicyId = Guid.NewGuid();
    private static readonly string ActivePolicyJson = $$"""{"id":"{{PolicyId}}","isActive":true}""";

    private static ServiceProvider Build(FakeHttpMessageHandler handler, int maxRetries = 2, int minThroughput = 10)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PolicyApi:BaseUrl"] = "http://policy.test/",
            ["PolicyApi:Resilience:TotalTimeoutSeconds"] = "10",
            ["PolicyApi:Resilience:AttemptTimeoutSeconds"] = "1",
            ["PolicyApi:Resilience:MaxRetryAttempts"] = maxRetries.ToString(),
            ["PolicyApi:Resilience:RetryBaseDelayMs"] = "10",
            ["PolicyApi:Resilience:CircuitMinimumThroughput"] = minThroughput.ToString(),
            ["PolicyApi:Resilience:CircuitBreakDurationSeconds"] = "30"
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddDebug());
        services.AddPolicyClient(config).ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }

    private static Task<HttpResponseMessage> Ok() =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ActivePolicyJson, System.Text.Encoding.UTF8, "application/json")
        });

    private static Task<HttpResponseMessage> Status(HttpStatusCode code) =>
        Task.FromResult(new HttpResponseMessage(code));

    [Fact]
    public async Task Transient_503_is_retried_until_it_succeeds()
    {
        var calls = 0;
        var handler = new FakeHttpMessageHandler((_, _) => ++calls < 3 ? Status(HttpStatusCode.ServiceUnavailable) : Ok());
        using var sp = Build(handler);

        var result = await sp.GetRequiredService<IPolicyClient>().GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Found, result.Status);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task Persistent_503_stops_after_the_configured_retries_and_is_reported_unavailable()
    {
        var handler = new FakeHttpMessageHandler((_, _) => Status(HttpStatusCode.ServiceUnavailable));
        using var sp = Build(handler, maxRetries: 2);

        var result = await sp.GetRequiredService<IPolicyClient>().GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, result.Status);
        Assert.Equal(3, handler.Requests.Count); // 1 attempt + 2 retries
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task Non_transient_4xx_is_never_retried(HttpStatusCode code)
    {
        var handler = new FakeHttpMessageHandler((_, _) => Status(code));
        using var sp = Build(handler);

        await sp.GetRequiredService<IPolicyClient>().GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Attempt_that_hangs_is_cut_off_by_the_per_attempt_timeout_and_retried()
    {
        var calls = 0;
        var handler = new FakeHttpMessageHandler(async (_, ct) =>
        {
            if (++calls == 1) await Task.Delay(Timeout.Infinite, ct); // hangs until the attempt timeout cancels it
            return await Ok();
        });
        using var sp = Build(handler);

        var result = await sp.GetRequiredService<IPolicyClient>().GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Found, result.Status);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Network_failure_is_retried_then_reported_unavailable()
    {
        var handler = new FakeHttpMessageHandler((_, _) => throw new HttpRequestException("connection refused"));
        using var sp = Build(handler, maxRetries: 1);

        var result = await sp.GetRequiredService<IPolicyClient>().GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, result.Status);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Circuit_opens_after_repeated_failures_and_then_fails_fast_without_calling_Policy_Api()
    {
        var handler = new FakeHttpMessageHandler((_, _) => Status(HttpStatusCode.ServiceUnavailable));
        using var sp = Build(handler, maxRetries: 1, minThroughput: 2);
        var client = sp.GetRequiredService<IPolicyClient>();

        await client.GetPolicyAsync(PolicyId, CancellationToken.None); // 1 attempt + 1 retry = 2 failures
        var callsBeforeOpen = handler.Requests.Count;
        var failFast = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(2, callsBeforeOpen);
        Assert.Equal(PolicyLookupStatus.Unavailable, failFast.Status);
        Assert.Equal(callsBeforeOpen, handler.Requests.Count); // third call never reached the handler
    }

    [Fact]
    public async Task Caller_cancellation_is_not_retried_and_is_propagated()
    {
        var handler = new FakeHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            return await Ok();
        });
        using var sp = Build(handler);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sp.GetRequiredService<IPolicyClient>().GetPolicyAsync(PolicyId, cts.Token));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Unsafe_methods_such_as_POST_are_not_retried()
    {
        var handler = new FakeHttpMessageHandler((_, _) => Status(HttpStatusCode.ServiceUnavailable));
        using var sp = Build(handler);
        var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(IPolicyClient));

        using var response = await http.PostAsync("api/anything", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Single(handler.Requests);
    }
}
