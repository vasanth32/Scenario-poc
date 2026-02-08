namespace ProductService.Middleware;

/// <summary>
/// Middleware to log slow requests (>500ms) for performance monitoring.
/// Helps identify performance bottlenecks in the application.
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

        // Process the request
        await _next(context);

        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        // Log slow requests
        if (elapsedMilliseconds > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow request detected - Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsedMilliseconds);
        }
        else
        {
            _logger.LogDebug(
                "Request completed - Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsedMilliseconds);
        }

        // Add response time header for monitoring
        context.Response.Headers.Append("X-Response-Time", $"{elapsedMilliseconds}ms");
    }
}

