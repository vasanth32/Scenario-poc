namespace OrderService.Middleware;

/// <summary>
/// Middleware to log slow requests (>500ms) for performance monitoring.
/// </summary>
public class ResponseTimeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseTimeMiddleware> _logger;
    private const long SlowRequestThresholdMs = 500;

    public ResponseTimeMiddleware(RequestDelegate next, ILogger<ResponseTimeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (elapsedMilliseconds > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow request detected - Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsedMilliseconds);
        }

        context.Response.Headers.Append("X-Response-Time", $"{elapsedMilliseconds}ms");
    }
}

