namespace ProductService.Services;

/// <summary>
/// Service to track database query performance.
/// Logs slow queries for monitoring and optimization.
/// </summary>
public class QueryPerformanceService
{
    private readonly ILogger<QueryPerformanceService> _logger;
    private readonly long _slowQueryThresholdMs;

    public QueryPerformanceService(ILogger<QueryPerformanceService> logger)
    {
        _logger = logger;
        _slowQueryThresholdMs = 200; // Log queries slower than 200ms
    }

    /// <summary>
    /// Logs query execution time if it exceeds the threshold.
    /// </summary>
    public void LogQueryPerformance(string operation, long elapsedMilliseconds, string? additionalInfo = null)
    {
        if (elapsedMilliseconds > _slowQueryThresholdMs)
        {
            _logger.LogWarning(
                "Slow query detected - Operation: {Operation}, Duration: {Duration}ms, Info: {Info}",
                operation,
                elapsedMilliseconds,
                additionalInfo ?? "N/A");
        }
        else
        {
            _logger.LogDebug(
                "Query executed - Operation: {Operation}, Duration: {Duration}ms",
                operation,
                elapsedMilliseconds);
        }
    }
}

