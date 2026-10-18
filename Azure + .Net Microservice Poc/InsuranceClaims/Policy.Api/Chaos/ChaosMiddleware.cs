namespace Policy.Api.Chaos;

// Applies the current chaos settings to every request except the chaos endpoint itself,
// so the toggle can always be used to turn the chaos off again.
public sealed class ChaosMiddleware(RequestDelegate next, ChaosState state, ILogger<ChaosMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/chaos"))
        {
            await next(context);
            return;
        }

        var chaos = state.Current;

        if (chaos.Down)
        {
            logger.LogWarning("Chaos: simulating downtime for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        if (chaos.LatencyMs > 0)
        {
            logger.LogWarning("Chaos: adding {LatencyMs} ms latency to {Path}", chaos.LatencyMs, context.Request.Path);
            await Task.Delay(chaos.LatencyMs, context.RequestAborted);
        }

        if (chaos.FailureRate > 0 && Random.Shared.NextDouble() < chaos.FailureRate)
        {
            logger.LogWarning("Chaos: injecting HTTP 500 for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        await next(context);
    }
}
