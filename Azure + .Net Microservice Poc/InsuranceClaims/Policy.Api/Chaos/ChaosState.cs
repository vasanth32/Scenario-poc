using System.ComponentModel.DataAnnotations;

namespace Policy.Api.Chaos;

// Fault-injection settings, changed at runtime through /api/chaos. Development only.
public sealed record ChaosSettings(
    [Range(0, 60_000)] int LatencyMs = 0,
    [Range(0.0, 1.0)] double FailureRate = 0.0,
    bool Down = false);

public sealed class ChaosState
{
    private volatile ChaosSettings _current = new();

    public ChaosSettings Current => _current;

    public void Set(ChaosSettings settings) => _current = settings;

    public void Reset() => _current = new();
}
