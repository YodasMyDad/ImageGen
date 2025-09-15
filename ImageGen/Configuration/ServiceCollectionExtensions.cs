using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using ImageGen.Core;

namespace ImageGen.Configuration;

/// <summary>
/// Extension methods for configuring ImageGen services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the ImageGen client to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configure">An action to configure the ImageGen options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddImageGenClient(
        this IServiceCollection services,
        Action<ImageGenOptions> configure)
    {
        // Configure options using the standard pattern
        services.Configure(configure);

        // Create options instance for direct injection
        var tempOptions = new ImageGenOptions();
        configure(tempOptions);
        services.AddSingleton(tempOptions);

        // Register HttpClient with Polly policies
        services.AddHttpClient<IImageGenClient, ImageGenClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<ImageGenOptions>();
                client.BaseAddress = options.BaseUrl;
                client.Timeout = options.RequestTimeout;
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Reuse connections for efficiency

        // Note: Polly policies will be applied in the client implementation itself

        return services;
    }

    /// <summary>
    /// Gets the retry policy for handling transient failures.
    /// Implements exponential backoff with jitter and respects Retry-After headers.
    /// </summary>
    /// <returns>The retry policy.</returns>
    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode >= System.Net.HttpStatusCode.InternalServerError ||
                            msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                            msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            .WaitAndRetryAsync(
                retryCount: 4,
                sleepDurationProvider: (retryAttempt, response, context) =>
                {
                    // Check for Retry-After header first
                    if (response?.Result?.Headers?.RetryAfter is { } retryAfter)
                    {
                        if (retryAfter.Delta.HasValue)
                        {
                            return retryAfter.Delta.Value;
                        }
                        if (retryAfter.Date.HasValue)
                        {
                            var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                            return delay > TimeSpan.Zero ? delay : TimeSpan.FromSeconds(1);
                        }
                    }

                    // Exponential backoff with jitter
                    var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1));
                    var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                    return baseDelay + jitter;
                },
                onRetryAsync: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempts (will be handled by the client)
                    return Task.CompletedTask;
                });
    }

    /// <summary>
    /// Gets the circuit breaker policy for preventing cascading failures.
    /// </summary>
    /// <returns>The circuit breaker policy.</returns>
    private static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (response, breakDelay) =>
                {
                    // Log circuit breaker activation (will be handled by the client)
                },
                onReset: () =>
                {
                    // Log circuit breaker reset (will be handled by the client)
                },
                onHalfOpen: () =>
                {
                    // Log circuit breaker half-open (will be handled by the client)
                });
    }
}
