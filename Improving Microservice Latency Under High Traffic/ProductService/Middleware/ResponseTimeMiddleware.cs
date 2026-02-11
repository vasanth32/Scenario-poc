using Microsoft.ApplicationInsights;

namespace ProductService.Middleware;

/// <summary>
/// Middleware to log slow requests (>500ms) for performance monitoring.
/// Helps identify performance bottlenecks in the application.
/// </summary>
public class ResponseTimeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseTimeMiddleware> _logger;
    private readonly TelemetryClient? _telemetryClient;
    private const long SlowRequestThresholdMs = 500;

    public ResponseTimeMiddleware(
        RequestDelegate next,
        ILogger<ResponseTimeMiddleware> logger,
        TelemetryClient? telemetryClient = null)
    {
        _next = next;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Process the request
        await _next(context);

        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        // Track request duration as a custom metric in Application Insights
        _telemetryClient?.TrackMetric("RequestDurationMs", elapsedMilliseconds);

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

        // Add response time header for monitoring (only if headers are still writable)
        if (!context.Response.HasStarted)
        {
            context.Response.Headers.Append("X-Response-Time", $"{elapsedMilliseconds}ms");
        }
        else
        {
            _logger.LogDebug("Response has already started, skipping X-Response-Time header.");
        }
    }
}

