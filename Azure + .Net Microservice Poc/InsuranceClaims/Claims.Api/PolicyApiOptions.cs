using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

public sealed class PolicyApiOptions
{
    public const string SectionName = "PolicyApi";

    [Required, Url]
    public string BaseUrl { get; set; } = "";

    [Required, ValidateObjectMembers]
    public PolicyApiResilienceOptions Resilience { get; set; } = new();
}

// Pipeline order (outer to inner): total timeout -> retry -> circuit breaker -> per-attempt timeout.
public sealed class PolicyApiResilienceOptions
{
    // Budget for the whole call including every retry.
    [Range(1, 120)]
    public int TotalTimeoutSeconds { get; set; } = 10;

    // Budget for a single attempt.
    [Range(1, 60)]
    public int AttemptTimeoutSeconds { get; set; } = 2;

    [Range(1, 10)]
    public int MaxRetryAttempts { get; set; } = 2;

    // First delay; later delays double, with random jitter applied.
    [Range(10, 10_000)]
    public int RetryBaseDelayMs { get; set; } = 200;

    // Open the circuit when this share of calls fail within the sampling window.
    [Range(0.01, 1.0)]
    public double CircuitFailureRatio { get; set; } = 0.5;

    [Range(2, 1_000)]
    public int CircuitMinimumThroughput { get; set; } = 10;

    // Must be at least twice AttemptTimeoutSeconds (enforced by the resilience library).
    [Range(1, 600)]
    public int CircuitSamplingSeconds { get; set; } = 30;

    // How long calls fail fast before a trial call is allowed.
    [Range(1, 600)]
    public int CircuitBreakDurationSeconds { get; set; } = 15;
}
