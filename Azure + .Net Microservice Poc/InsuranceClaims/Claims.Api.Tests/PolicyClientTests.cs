using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Claims.Api.Tests;

public class PolicyClientTests
{
    private static readonly Guid PolicyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static (PolicyClient Client, FakeLogger<PolicyClient> Logger) Create(
        FakeHttpMessageHandler handler, TimeSpan? timeout = null)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://policy.test/"),
            Timeout = timeout ?? TimeSpan.FromSeconds(5)
        };
        var logger = new FakeLogger<PolicyClient>();
        return (new PolicyClient(http, logger), logger);
    }

    [Fact]
    public async Task Returns_Found_for_200_and_calls_expected_url()
    {
        var handler = FakeHttpMessageHandler.Responding(HttpStatusCode.OK, $$"""{"id":"{{PolicyId}}","isActive":true,"holderName":"x"}""");
        var (client, _) = Create(handler);

        var result = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Found, result.Status);
        Assert.True(result.Policy!.IsActive);
        Assert.Equal($"http://policy.test/api/policies/{PolicyId}", handler.Requests.Single().RequestUri!.ToString());
    }

    [Fact]
    public async Task Returns_NotFound_for_404_and_logs_information()
    {
        var (client, logger) = Create(FakeHttpMessageHandler.Responding(HttpStatusCode.NotFound));

        var result = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.NotFound, result.Status);
        Assert.Equal(LogLevel.Information, logger.LatestRecord.Level);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task Returns_Unavailable_for_non_success_and_logs_warning(HttpStatusCode status)
    {
        var (client, logger) = Create(FakeHttpMessageHandler.Responding(status));

        var result = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, result.Status);
        Assert.Equal(LogLevel.Warning, logger.LatestRecord.Level);
        Assert.Contains(((int)status).ToString(), logger.LatestRecord.Message);
    }

    [Fact]
    public async Task Returns_Unavailable_when_service_is_down_and_logs_error()
    {
        var (client, logger) = Create(FakeHttpMessageHandler.Throwing(new HttpRequestException("Connection refused")));

        var result = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, result.Status);
        Assert.Equal(LogLevel.Error, logger.LatestRecord.Level);
        Assert.Contains("unreachable", logger.LatestRecord.Message);
    }

    [Fact]
    public async Task Returns_Unavailable_on_timeout_and_logs_error()
    {
        var (client, logger) = Create(FakeHttpMessageHandler.Throwing(new TaskCanceledException("timeout")));

        var result = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, result.Status);
        Assert.Equal(LogLevel.Error, logger.LatestRecord.Level);
        Assert.Contains("Timed out", logger.LatestRecord.Message);
    }

    [Fact]
    public async Task Real_HttpClient_timeout_on_slow_handler_returns_Unavailable()
    {
        var slow = new FakeHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var (client, logger) = Create(slow, TimeSpan.FromMilliseconds(100));

        var result = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, result.Status);
        Assert.Contains("Timed out", logger.LatestRecord.Message);
    }

    [Fact]
    public async Task Returns_Unavailable_for_invalid_json_and_logs_error()
    {
        var (client, logger) = Create(FakeHttpMessageHandler.Responding(HttpStatusCode.OK, "not json"));

        var result = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, result.Status);
        Assert.Equal(LogLevel.Error, logger.LatestRecord.Level);
    }

    [Fact]
    public async Task Returns_Unavailable_for_200_with_non_json_content_type()
    {
        var handler = new FakeHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html>proxy error</html>", System.Text.Encoding.UTF8, "text/html")
        }));
        var (client, logger) = Create(handler);

        var result = await client.GetPolicyAsync(PolicyId, CancellationToken.None);

        Assert.Equal(PolicyLookupStatus.Unavailable, result.Status);
        Assert.Equal(LogLevel.Error, logger.LatestRecord.Level);
    }

    [Fact]
    public async Task Caller_cancellation_is_propagated_not_swallowed()
    {
        var (client, _) = Create(FakeHttpMessageHandler.Throwing(new TaskCanceledException()));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetPolicyAsync(PolicyId, cts.Token));
    }
}
