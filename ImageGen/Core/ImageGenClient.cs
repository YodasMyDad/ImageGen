using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ImageGen.Configuration;
using ImageGen.Exceptions;
using ImageGen.Models;

namespace ImageGen.Core;

/// <summary>
/// Client for image generation and editing operations using HTTP-based providers.
/// Implements resilience patterns and observability features.
/// </summary>
internal sealed class ImageGenClient : IImageGenClient
{
    private readonly HttpClient _httpClient;
    private readonly ImageGenOptions _options;
    private readonly ILogger<ImageGenClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured with Polly policies.</param>
    /// <param name="options">The ImageGen configuration options.</param>
    /// <param name="logger">The logger for observability.</param>
    public ImageGenClient(
        HttpClient httpClient,
        ImageGenOptions options,
        ILogger<ImageGenClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ImageResult> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var activity = Diagnostics.ImageGenActivitySource.StartActivity("ImageGen.Generate");
        activity?.SetTag("image.prompt_length", request.Prompt.Length);
        activity?.SetTag("image.width", request.Width);
        activity?.SetTag("image.height", request.Height);
        activity?.SetTag("image.quality", request.Quality.ToString());
        activity?.SetTag("image.format", request.Format.ToString());

        _logger.LogInformation("Generating image with prompt: {Prompt}", TruncatePrompt(request.Prompt));

        try
        {
            using var content = CreateGenerationContent(request);
            using var response = await SendRequestAsync("images/generations", content, cancellationToken);
            return await ParseImageResponseAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate image");
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ImageResult>> GenerateManyAsync(GenerateRequest request, int count = 1, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        using var activity = Diagnostics.ImageGenActivitySource.StartActivity("ImageGen.GenerateMany");
        activity?.SetTag("image.count", count);

        var results = new List<ImageResult>();
        for (int i = 0; i < count; i++)
        {
            var result = await GenerateAsync(request, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<ImageResult> EditAsync(EditRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var activity = Diagnostics.ImageGenActivitySource.StartActivity("ImageGen.Edit");
        activity?.SetTag("image.input_fidelity", request.InputFidelity.ToString());
        activity?.SetTag("image.has_mask", request.Mask is not null);
        activity?.SetTag("image.secondary_count", request.SecondaryImages?.Count ?? 0);

        _logger.LogInformation("Editing image with prompt: {Prompt}", TruncatePrompt(request.Prompt));

        try
        {
            using var content = CreateEditContent(request);
            using var response = await SendRequestAsync("images/edits", content, cancellationToken);
            return await ParseImageResponseAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to edit image");
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ImageResult>> VariationsAsync(
        Stream baseImage,
        string? prompt = null,
        int count = 4,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(baseImage);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        using var activity = Diagnostics.ImageGenActivitySource.StartActivity("ImageGen.Variations");
        activity?.SetTag("image.count", count);
        if (prompt is not null)
        {
            activity?.SetTag("image.prompt_length", prompt.Length);
        }

        _logger.LogInformation("Creating {Count} variations of image", count);

        try
        {
            using var content = CreateVariationsContent(baseImage, prompt, count);
            using var response = await SendRequestAsync("images/variations", content, cancellationToken);
            return await ParseImageResponseListAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create image variations");
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    private MultipartFormDataContent CreateGenerationContent(GenerateRequest request)
    {
        var content = new MultipartFormDataContent();

        // Add model
        content.Add(new StringContent(_options.Model), "model");

        // Add prompt
        content.Add(new StringContent(request.Prompt), "prompt");

        // Add dimensions
        if (request.Width.HasValue)
            content.Add(new StringContent(request.Width.Value.ToString()), "width");
        if (request.Height.HasValue)
            content.Add(new StringContent(request.Height.Value.ToString()), "height");

        // Add quality
        var quality = MapQuality(request.Quality);
        content.Add(new StringContent(quality), "quality");

        // Add format/background
        var format = MapFormat(request.Format);
        content.Add(new StringContent(format), "format");
        if (request.TransparentBackground)
            content.Add(new StringContent("transparent"), "background");

        // Add seed if provided
        if (!string.IsNullOrEmpty(request.Seed))
            content.Add(new StringContent(request.Seed), "seed");

        return content;
    }

    private MultipartFormDataContent CreateEditContent(EditRequest request)
    {
        var content = new MultipartFormDataContent();

        // Add model
        content.Add(new StringContent(_options.Model), "model");

        // Add prompt
        content.Add(new StringContent(request.Prompt), "prompt");

        // Add primary image
        var primaryContent = new StreamContent(request.PrimaryImage);
        primaryContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(primaryContent, "image");

        // Add secondary images
        if (request.SecondaryImages is not null)
        {
            for (int i = 0; i < request.SecondaryImages.Count; i++)
            {
                var secondaryContent = new StreamContent(request.SecondaryImages[i]);
                secondaryContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                content.Add(secondaryContent, $"secondary_image_{i}");
            }
        }

        // Add mask if provided
        if (request.Mask is not null)
        {
            var maskContent = new StreamContent(request.Mask);
            maskContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(maskContent, "mask");
        }

        // Add input fidelity
        var fidelity = MapInputFidelity(request.InputFidelity);
        content.Add(new StringContent(fidelity), "input_fidelity");

        // Add dimensions
        if (request.Width.HasValue)
            content.Add(new StringContent(request.Width.Value.ToString()), "width");
        if (request.Height.HasValue)
            content.Add(new StringContent(request.Height.Value.ToString()), "height");

        // Add quality
        var quality = MapQuality(request.Quality);
        content.Add(new StringContent(quality), "quality");

        // Add format/background
        var format = MapFormat(request.Format);
        content.Add(new StringContent(format), "output_format");
        if (request.TransparentBackground)
            content.Add(new StringContent("transparent"), "background");

        return content;
    }

    private MultipartFormDataContent CreateVariationsContent(Stream baseImage, string? prompt, int count)
    {
        var content = new MultipartFormDataContent();

        // Add model
        content.Add(new StringContent(_options.Model), "model");

        // Add base image
        var imageContent = new StreamContent(baseImage);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "image");

        // Add prompt if provided
        if (!string.IsNullOrEmpty(prompt))
            content.Add(new StringContent(prompt), "prompt");

        // Add count
        content.Add(new StringContent(count.ToString()), "n");

        return content;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(string endpoint, HttpContent content, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content,
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey) }
        };

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("API request failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

            // Create a more descriptive exception
            var message = $"API request failed with status {response.StatusCode}";
            if (!string.IsNullOrWhiteSpace(errorContent))
            {
                message += $": {errorContent}";
            }

            throw new ImageGenClientException(message, (int)response.StatusCode);
        }

        return response;
    }

    private async Task<ImageResult> ParseImageResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        // Extract metadata from response
        var requestId = response.Headers.TryGetValues("x-request-id", out var values)
            ? values.FirstOrDefault() ?? string.Empty
            : string.Empty;

        // For now, return basic result - in a real implementation you'd parse the actual response format
        // This would typically involve deserializing JSON response with image URLs or base64 data
        return new ImageResult(bytes, 1024, 1024, ImageFormat.Png, requestId);
    }

    private async Task<IReadOnlyList<ImageResult>> ParseImageResponseListAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        var requestId = response.Headers.TryGetValues("x-request-id", out var values)
            ? values.FirstOrDefault() ?? string.Empty
            : string.Empty;

        // For variations, return multiple results
        return [new ImageResult(bytes, 1024, 1024, ImageFormat.Png, requestId)];
    }

    private static string MapQuality(ImageQuality quality) => quality switch
    {
        ImageQuality.Standard => "standard",
        ImageQuality.High => "hd",
        _ => "standard"
    };

    private static string MapFormat(ImageFormat format) => format switch
    {
        ImageFormat.Png => "png",
        ImageFormat.Jpeg => "jpeg",
        ImageFormat.Webp => "webp",
        _ => "png"
    };

    private static string MapInputFidelity(InputFidelity fidelity) => fidelity switch
    {
        InputFidelity.Default => "default",
        InputFidelity.High => "high",
        _ => "default"
    };

    private static string TruncatePrompt(string prompt) =>
        prompt.Length > 100 ? prompt[..97] + "..." : prompt;
}

/// <summary>
/// Diagnostics and observability helpers.
/// </summary>
internal static class Diagnostics
{
    public static readonly System.Diagnostics.ActivitySource ImageGenActivitySource =
        new("ImageGen", "1.0.0");
}
