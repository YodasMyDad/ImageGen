using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ImageGen.Configuration;
using ImageGen.Exceptions;
using ImageGen.Models;

namespace ImageGen.Core;

/// <summary>
/// Client for image generation and editing operations using HTTP-based providers.
/// Implements resilience patterns and observability features.
/// </summary>
public sealed class ImageGenClient : IImageGenClient
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

    private HttpContent CreateGenerationContent(GenerateRequest request)
    {
        // Use JSON format for gpt-image-1 (like the curl example)
        var requestBody = new
        {
            model = _options.Model,
            prompt = request.Prompt,
            width = request.Width,
            height = request.Height,
            quality = MapQuality(request.Quality),
            format = MapFormat(request.Format),
            background = request.TransparentBackground ? "transparent" : null,
            seed = request.Seed
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return content;
    }

    private HttpContent CreateEditContent(EditRequest request)
    {
        var content = new MultipartFormDataContent();

        // Add model
        content.Add(new StringContent(_options.Model), "model");

        // Add prompt
        content.Add(new StringContent(request.Prompt), "prompt");

        // Add primary image (as a real file part with filename)
        var primaryContent = new StreamContent(request.PrimaryImage);
        var primaryContentType = (request.Extra is not null && request.Extra.TryGetValue("content_type", out var ct) && !string.IsNullOrWhiteSpace(ct))
            ? ct
            : "image/png";
        var primaryFileName = (request.Extra is not null && request.Extra.TryGetValue("filename", out var fn) && !string.IsNullOrWhiteSpace(fn))
            ? fn
            : "image.png";
        primaryContent.Headers.ContentType = new MediaTypeHeaderValue(primaryContentType);
        content.Add(primaryContent, "image", primaryFileName);

        // Add secondary images
        if (request.SecondaryImages is not null)
        {
            for (int i = 0; i < request.SecondaryImages.Count; i++)
            {
                var secondaryContent = new StreamContent(request.SecondaryImages[i]);
                secondaryContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                content.Add(secondaryContent, $"secondary_image_{i}", $"secondary_{i}.png");
            }
        }

        // Add mask if provided
        if (request.Mask is not null)
        {
            var maskContent = new StreamContent(request.Mask);
            maskContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(maskContent, "mask", "mask.png");
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

    private HttpContent CreateVariationsContent(Stream baseImage, string? prompt, int count)
    {
        // For variations, we still need multipart because we have to send the image file
        var content = new MultipartFormDataContent();

        // Add model
        content.Add(new StringContent(_options.Model), "model");

        // Add base image
        var imageContent = new StreamContent(baseImage);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "image", "image.png");

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
        // Extract metadata from response
        var requestId = response.Headers.TryGetValues("x-request-id", out var values)
            ? values.FirstOrDefault() ?? string.Empty
            : string.Empty;

        // Read the JSON response
        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

        // First try to parse as an error response
        var errorResponse = JsonSerializer.Deserialize<OpenAiErrorResponse>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (errorResponse?.Error != null)
        {
            var errorMessage = errorResponse.Error.Message ?? "Unknown API error";
            _logger.LogError("OpenAI API returned error: {Type} - {Message}", errorResponse.Error.Type, errorMessage);
            throw new ImageGenClientException($"OpenAI API error: {errorMessage}", 400);
        }

        // Parse the OpenAI response
        var openAiResponse = JsonSerializer.Deserialize<OpenAiImageResponse>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Log response summary without potentially huge base64 data
        _logger.LogInformation("Received API response with {DataCount} image(s), created: {Created}",
            openAiResponse?.Data?.Length ?? 0,
            openAiResponse?.Created ?? 0);

        if (openAiResponse?.Data == null || openAiResponse.Data.Length == 0)
        {
            _logger.LogError("Parsed response has no data array. Full response: {Response}", jsonContent);
            throw new ImageGenClientException("No image data received from API response", 0);
        }

        var imageData = openAiResponse.Data[0];

        // Handle both URL-based and base64-based responses
        ReadOnlyMemory<byte> imageBytes;
        if (!string.IsNullOrEmpty(imageData.Url))
        {
            // Download the actual image from the URL
            imageBytes = await DownloadImageAsync(imageData.Url, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(imageData.B64Json))
        {
            // Decode the base64 image data directly
            _logger.LogInformation("Received base64-encoded image data from API");
            try
            {
                imageBytes = Convert.FromBase64String(imageData.B64Json);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Failed to decode base64 image data");
                throw new ImageGenClientException("Invalid base64 image data in API response", 0);
            }
        }
        else
        {
            _logger.LogError("First image data has no URL or base64 data. Image data: {@ImageData}", imageData);
            throw new ImageGenClientException("No image URL or base64 data provided in API response", 0);
        }

        // Get image dimensions (we'll need to parse the actual image format)
        var format = DetermineImageFormat(imageBytes);
        var (width, height) = GetImageDimensions(imageBytes, format);

        return new ImageResult(imageBytes, width, height, format, requestId);
    }

    private async Task<IReadOnlyList<ImageResult>> ParseImageResponseListAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        // Extract metadata from response
        var requestId = response.Headers.TryGetValues("x-request-id", out var values)
            ? values.FirstOrDefault() ?? string.Empty
            : string.Empty;

        // Read the JSON response
        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

        // Parse the OpenAI response
        var openAiResponse = JsonSerializer.Deserialize<OpenAiImageResponse>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (openAiResponse?.Data == null || openAiResponse.Data.Length == 0)
        {
            throw new ImageGenClientException("No image data received from API response", 0);
        }

        var results = new List<ImageResult>();

        foreach (var imageData in openAiResponse.Data)
        {
            // Handle both URL-based and base64-based responses
            ReadOnlyMemory<byte> imageBytes;
            if (!string.IsNullOrEmpty(imageData.Url))
            {
                // Download the actual image from the URL
                imageBytes = await DownloadImageAsync(imageData.Url, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(imageData.B64Json))
            {
                // Decode the base64 image data directly
                _logger.LogInformation("Received base64-encoded image data for variation from API");
                try
                {
                    imageBytes = Convert.FromBase64String(imageData.B64Json);
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Failed to decode base64 image data for variation");
                    continue; // Skip this image and continue with others
                }
            }
            else
            {
                _logger.LogWarning("Skipping image with missing URL and base64 data in response");
                continue;
            }

            try
            {

                // Get image dimensions
                var format = DetermineImageFormat(imageBytes);
                var (width, height) = GetImageDimensions(imageBytes, format);

                results.Add(new ImageResult(imageBytes, width, height, format, requestId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download image from {Url}", imageData.Url);
                // Continue with other images even if one fails
            }
        }

        if (results.Count == 0)
        {
            throw new ImageGenClientException("Failed to download any images from the API response", 0);
        }

        return results;
    }

    private static string MapQuality(ImageQuality quality) => quality switch
    {
        ImageQuality.Standard => "medium",
        ImageQuality.High => "high",
        _ => "medium"
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

    private async Task<ReadOnlyMemory<byte>> DownloadImageAsync(string imageUrl, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to download image from {Url} with status {StatusCode}", imageUrl, response.StatusCode);
            throw new ImageGenClientException($"Failed to download image: HTTP {response.StatusCode}", (int)response.StatusCode);
        }

        var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (imageBytes.Length == 0)
        {
            throw new ImageGenClientException("Downloaded image is empty", 0);
        }

        _logger.LogInformation("Successfully downloaded image of {Size} bytes from {Url}", imageBytes.Length, imageUrl);
        return imageBytes;
    }

    private static ImageFormat DetermineImageFormat(ReadOnlyMemory<byte> imageBytes)
    {
        var span = imageBytes.Span;

        // Check PNG signature: 89 50 4E 47 0D 0A 1A 0A
        if (span.Length >= 8 &&
            span[0] == 0x89 && span[1] == 0x50 && span[2] == 0x4E && span[3] == 0x47 &&
            span[4] == 0x0D && span[5] == 0x0A && span[6] == 0x1A && span[7] == 0x0A)
        {
            return ImageFormat.Png;
        }

        // Check JPEG signature: FF D8
        if (span.Length >= 2 && span[0] == 0xFF && span[1] == 0xD8)
        {
            return ImageFormat.Jpeg;
        }

        // Check WebP signature: 52 49 46 46 ... 57 45 42 50
        if (span.Length >= 12 &&
            span[0] == 0x52 && span[1] == 0x49 && span[2] == 0x46 && span[3] == 0x46 &&
            span[8] == 0x57 && span[9] == 0x45 && span[10] == 0x42 && span[11] == 0x50)
        {
            return ImageFormat.Webp;
        }

        // Default to PNG if we can't determine the format
        return ImageFormat.Png;
    }

    private static (int Width, int Height) GetImageDimensions(ReadOnlyMemory<byte> imageBytes, ImageFormat format)
    {
        var span = imageBytes.Span;

        try
        {
            switch (format)
            {
                case ImageFormat.Png:
                    // PNG dimensions are stored at bytes 16-23 (width: 16-19, height: 20-23, big-endian)
                    if (span.Length >= 24)
                    {
                        var width = (span[16] << 24) | (span[17] << 16) | (span[18] << 8) | span[19];
                        var height = (span[20] << 24) | (span[21] << 16) | (span[22] << 8) | span[23];
                        return (width, height);
                    }
                    break;

                case ImageFormat.Jpeg:
                    // JPEG dimensions require parsing the SOF marker
                    return ParseJpegDimensions(span);

                case ImageFormat.Webp:
                    // WebP dimensions are stored at bytes 26-29 (width: 26-27, height: 28-29, little-endian)
                    if (span.Length >= 30)
                    {
                        var width = span[26] | (span[27] << 8);
                        var height = span[28] | (span[29] << 8);
                        return (width, height);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log the error and fall back to default dimensions
            Console.WriteLine($"Error parsing image dimensions: {ex.Message}");
        }

        // Fallback dimensions if we can't parse them
        return (1024, 1024);
    }

    private static (int Width, int Height) ParseJpegDimensions(ReadOnlySpan<byte> jpegData)
    {
        // Simple JPEG SOF marker parsing
        int i = 2; // Skip SOI marker

        while (i < jpegData.Length - 1)
        {
            if (jpegData[i] == 0xFF)
            {
                var marker = jpegData[i + 1];

                // SOF markers (Start of Frame) contain dimensions
                if (marker >= 0xC0 && marker <= 0xC3)
                {
                    if (i + 9 < jpegData.Length)
                    {
                        var height = (jpegData[i + 5] << 8) | jpegData[i + 6];
                        var width = (jpegData[i + 7] << 8) | jpegData[i + 8];
                        return (width, height);
                    }
                }

                // Skip to next marker
                if (i + 3 < jpegData.Length)
                {
                    var length = (jpegData[i + 2] << 8) | jpegData[i + 3];
                    i += 2 + length;
                }
                else
                {
                    break;
                }
            }
            else
            {
                i++;
            }
        }

        return (1024, 1024); // Fallback
    }
}

/// <summary>
/// Diagnostics and observability helpers.
/// </summary>
internal static class Diagnostics
{
    public static readonly System.Diagnostics.ActivitySource ImageGenActivitySource =
        new("ImageGen", "1.0.0");
}
