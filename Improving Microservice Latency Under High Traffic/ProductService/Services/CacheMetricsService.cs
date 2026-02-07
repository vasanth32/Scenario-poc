namespace ProductService.Services;

/// <summary>
/// Service to track cache metrics (hits and misses)
/// Helps monitor cache effectiveness
/// </summary>
public class CacheMetricsService
{
    private long _cacheHits = 0;
    private long _cacheMisses = 0;
    private readonly ILogger<CacheMetricsService> _logger;

    public CacheMetricsService(ILogger<CacheMetricsService> logger)
    {
        _logger = logger;
    }

    public void RecordCacheHit(string cacheKey)
    {
        Interlocked.Increment(ref _cacheHits);
        _logger.LogDebug("Cache HIT for key: {CacheKey}", cacheKey);
    }

    public void RecordCacheMiss(string cacheKey)
    {
        Interlocked.Increment(ref _cacheMisses);
        _logger.LogDebug("Cache MISS for key: {CacheKey}", cacheKey);
    }

    public CacheMetrics GetMetrics()
    {
        var total = _cacheHits + _cacheMisses;
        var hitRate = total > 0 ? (double)_cacheHits / total * 100 : 0;

        return new CacheMetrics
        {
            Hits = _cacheHits,
            Misses = _cacheMisses,
            Total = total,
            HitRate = hitRate
        };
    }

    public void LogMetrics()
    {
        var metrics = GetMetrics();
        _logger.LogInformation(
            "Cache Metrics - Hits: {Hits}, Misses: {Misses}, Total: {Total}, Hit Rate: {HitRate:F2}%",
            metrics.Hits, metrics.Misses, metrics.Total, metrics.HitRate);
    }
}

public class CacheMetrics
{
    public long Hits { get; set; }
    public long Misses { get; set; }
    public long Total { get; set; }
    public double HitRate { get; set; }
}

