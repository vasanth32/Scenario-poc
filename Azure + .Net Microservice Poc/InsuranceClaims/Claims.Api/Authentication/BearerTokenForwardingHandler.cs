using System.Net.Http.Headers;

namespace Claims.Api.Authentication;

public sealed class BearerTokenForwardingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (AuthenticationHeaderValue.TryParse(authorization, out var header) &&
            header.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Authorization = header;
        }

        return base.SendAsync(request, cancellationToken);
    }
}