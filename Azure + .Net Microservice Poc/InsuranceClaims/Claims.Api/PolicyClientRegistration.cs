using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

public static class PolicyClientRegistration
{
    // Typed client + options + resilience pipeline. Returns the builder so tests can swap the primary handler.
    public static IHttpClientBuilder AddPolicyClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PolicyApiOptions>()
            .Bind(configuration.GetSection(PolicyApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var builder = services.AddHttpClient<IPolicyClient, PolicyClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<PolicyApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
                // Backstop only: the resilience pipeline enforces the real total and per-attempt timeouts.
                client.Timeout = TimeSpan.FromSeconds(options.Resilience.TotalTimeoutSeconds + 5);
            });

        builder.AddResilienceHandler("policy-api", (pipeline, context) =>
            {
                var r = context.ServiceProvider.GetRequiredService<IOptions<PolicyApiOptions>>().Value.Resilience;

                var log = context.ServiceProvider.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Claims.Api.PolicyClient.Resilience");

                pipeline.AddTimeout(TimeSpan.FromSeconds(r.TotalTimeoutSeconds));

                var retry = new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = r.MaxRetryAttempts,
                    Delay = TimeSpan.FromMilliseconds(r.RetryBaseDelayMs),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldRetryAfterHeader = true,
                    OnRetry = args =>
                    {
                        log.LogWarning(
                            "Retry {Attempt}/{Max} for Policy.Api in {DelayMs:F0} ms. Reason: {Reason}",
                            args.AttemptNumber + 1, r.MaxRetryAttempts, args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Exception?.GetType().Name ?? $"HTTP {(int?)args.Outcome.Result?.StatusCode}");
                        return default;
                    }
                };
                // Default ShouldHandle is transient only (5xx, 408, 429, network errors, timeouts); never 4xx like 404 or 422.
                // Also skip POST/PATCH/etc: retrying a non-idempotent call can duplicate it.
                retry.DisableForUnsafeHttpMethods();
                pipeline.AddRetry(retry);

                pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = r.CircuitFailureRatio,
                    MinimumThroughput = r.CircuitMinimumThroughput,
                    SamplingDuration = TimeSpan.FromSeconds(r.CircuitSamplingSeconds),
                    BreakDuration = TimeSpan.FromSeconds(r.CircuitBreakDurationSeconds),
                    OnOpened = args =>
                    {
                        log.LogError("Circuit OPENED for Policy.Api: failing fast for {BreakSeconds:F0}s",
                            args.BreakDuration.TotalSeconds);
                        return default;
                    },
                    OnHalfOpened = _ =>
                    {
                        log.LogWarning("Circuit HALF-OPEN for Policy.Api: allowing a trial call");
                        return default;
                    },
                    OnClosed = _ =>
                    {
                        log.LogInformation("Circuit CLOSED for Policy.Api: recovered");
                        return default;
                    }
                });

                pipeline.AddTimeout(TimeSpan.FromSeconds(r.AttemptTimeoutSeconds));
            });

        return builder;
    }
}
