using System.Net;
using System.Text;

namespace Claims.Api.Tests;

// Fake HttpMessageHandler: returns a canned response or throws, and records requests.
public sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Requests.Add(request);
        return handler(request, ct);
    }

    public static FakeHttpMessageHandler Responding(HttpStatusCode status, string? json = null) =>
        new((_, _) => Task.FromResult(new HttpResponseMessage(status)
        {
            Content = json is null ? null : new StringContent(json, Encoding.UTF8, "application/json")
        }));

    public static FakeHttpMessageHandler Throwing(Exception ex) => new((_, _) => throw ex);
}
