using Microsoft.Extensions.DependencyInjection;
using ImageGen.Core;

namespace ImageGen.Configuration;

/// <summary>
/// Extension methods for configuring ImageGen services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add ImageGen client to dependency injection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for ImageGen options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddImageGenClient(
        this IServiceCollection services,
        Action<ImageGenOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        // Create options instance for injection
        var tempOptions = new ImageGenOptions();
        configure(tempOptions);
        services.AddSingleton(tempOptions);

        // Register HttpClient for the ImageGen client
        services.AddHttpClient<IImageGenClient, ImageGenClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<ImageGenOptions>();
            client.BaseAddress = options.BaseUrl;
            client.Timeout = options.RequestTimeout;
        });

        return services;
    }
}
