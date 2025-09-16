using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ImageGen.Configuration;
using ImageGen.Exceptions;
using ImageGen.Models;

namespace ImageGen.Core;

/// <summary>
/// Simple client for OpenAI GPT-Image-1 image generation and editing.
/// </summary>
public sealed class ImageGenClient : IImageGenClient
{
    private readonly HttpClient _httpClient;
    private readonly ImageGenOptions _options;
    private readonly ILogger<ImageGenClient> _logger;

    /// <summary>
    /// Creates a new ImageGen client.
    /// </summary>
    public ImageGenClient(HttpClient httpClient, ImageGenOptions options, ILogger<ImageGenClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generate a new image from text prompt.
    /// </summary>
    public async Task<ImageResult> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Generating image: {Prompt}", TruncatePrompt(request.Prompt));

        try
        {
            using var content = CreateGenerationContent(request);
            using var response = await SendRequestAsync("images/generations", content, cancellationToken);
            return await ParseImageResponseAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate image");
            throw;
        }
    }

    /// <summary>
    /// Generate multiple images from the same prompt.
    /// </summary>
    public async Task<IReadOnlyList<ImageResult>> GenerateManyAsync(GenerateRequest request, int count = 1, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        var results = new List<ImageResult>();
        for (int i = 0; i < count; i++)
        {
            var result = await GenerateAsync(request, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Edit an existing image with a prompt.
    /// </summary>
    public async Task<ImageResult> EditAsync(EditRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Editing image: {Prompt}", TruncatePrompt(request.Prompt));

        try
        {
            using var content = CreateEditContent(request);
            using var response = await SendRequestAsync("images/edits", content, cancellationToken);
            return await ParseImageResponseAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to edit image");
            throw;
        }
    }


    // Create HTTP content for image generation
    private HttpContent CreateGenerationContent(GenerateRequest request)
    {
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

        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    // Create HTTP content for image editing
    private HttpContent CreateEditContent(EditRequest request)
    {
        var content = new MultipartFormDataContent();

        content.Add(new StringContent(_options.Model), "model");
        content.Add(new StringContent(request.Prompt), "prompt");

        // Add primary image
        var primaryContent = new StreamContent(request.PrimaryImage);
        primaryContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(primaryContent, "image[]", "image.png");

        // Add secondary images if provided (OpenAI supports multiple images for gpt-image-1)
        if (request.SecondaryImages is not null)
        {
            for (int i = 0; i < request.SecondaryImages.Count; i++)
            {
                var secondaryContent = new StreamContent(request.SecondaryImages[i]);
                secondaryContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                // Use array syntax for multiple images
                content.Add(secondaryContent, "image[]", $"secondary_{i}.png");
            }
        }
        // Add mask if provided
        if (request.Mask is not null)
        {
            var maskContent = new StreamContent(request.Mask);
            maskContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(maskContent, "mask", "mask.png");
        }

        // Add other parameters
        content.Add(new StringContent(MapInputFidelity(request.InputFidelity)), "input_fidelity");

        if (request.Width.HasValue)
            content.Add(new StringContent(request.Width.Value.ToString()), "width");
        if (request.Height.HasValue)
            content.Add(new StringContent(request.Height.Value.ToString()), "height");

        content.Add(new StringContent(MapQuality(request.Quality)), "quality");
        content.Add(new StringContent(MapFormat(request.Format)), "output_format");

        if (request.TransparentBackground)
            content.Add(new StringContent("transparent"), "background");

        return content;
    }


    // Send HTTP request to OpenAI API
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
            _logger.LogError("API request failed: {StatusCode} - {Error}", response.StatusCode, errorContent);

            throw new ImageGenClientException($"API request failed: {response.StatusCode} - {errorContent}", (int)response.StatusCode);
        }

        return response;
    }

    // Parse API response into ImageResult
    private async Task<ImageResult> ParseImageResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

        // Check for API errors
        var errorResponse = JsonSerializer.Deserialize<OpenAiErrorResponse>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (errorResponse?.Error != null)
        {
            throw new ImageGenClientException($"API error: {errorResponse.Error.Message}", 400);
        }

        // Parse successful response
        var openAiResponse = JsonSerializer.Deserialize<OpenAiImageResponse>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (openAiResponse?.Data == null || openAiResponse.Data.Length == 0)
        {
            throw new ImageGenClientException("No image data received", 0);
        }

        var imageData = openAiResponse.Data[0];
        ReadOnlyMemory<byte> imageBytes;

        // Get image data from URL or base64
        if (!string.IsNullOrEmpty(imageData.Url))
        {
            imageBytes = await DownloadImageAsync(imageData.Url, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(imageData.B64Json))
        {
            imageBytes = Convert.FromBase64String(imageData.B64Json);
        }
        else
        {
            throw new ImageGenClientException("No image data in response", 0);
        }

        var format = DetermineImageFormat(imageBytes);
        var (width, height) = GetImageDimensions(imageBytes, format);

        return new ImageResult(imageBytes, width, height, format, "");
    }

    // Parse API response for multiple images
    private async Task<IReadOnlyList<ImageResult>> ParseImageResponseListAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var openAiResponse = JsonSerializer.Deserialize<OpenAiImageResponse>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (openAiResponse?.Data == null || openAiResponse.Data.Length == 0)
        {
            throw new ImageGenClientException("No image data received", 0);
        }

        var results = new List<ImageResult>();

        foreach (var imageData in openAiResponse.Data)
        {
            ReadOnlyMemory<byte> imageBytes;

            if (!string.IsNullOrEmpty(imageData.Url))
            {
                imageBytes = await DownloadImageAsync(imageData.Url, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(imageData.B64Json))
            {
                imageBytes = Convert.FromBase64String(imageData.B64Json);
            }
            else
            {
                continue; // Skip images without data
            }

            var format = DetermineImageFormat(imageBytes);
            var (width, height) = GetImageDimensions(imageBytes, format);
            results.Add(new ImageResult(imageBytes, width, height, format, ""));
        }

        return results;
    }

    // Map our quality enum to API values
    private static string MapQuality(ImageQuality quality) => quality switch
    {
        ImageQuality.Standard => "medium",
        ImageQuality.High => "high",
        _ => "medium"
    };

    // Map our format enum to API values
    private static string MapFormat(ImageFormat format) => format switch
    {
        ImageFormat.Png => "png",
        ImageFormat.Jpeg => "jpeg",
        ImageFormat.Webp => "webp",
        _ => "png"
    };

    // Map our fidelity enum to API values
    private static string MapInputFidelity(InputFidelity fidelity) => fidelity switch
    {
        InputFidelity.Default => "default",
        InputFidelity.High => "high",
        _ => "default"
    };

    // Truncate long prompts for logging
    private static string TruncatePrompt(string prompt) =>
        prompt.Length > 100 ? prompt[..97] + "..." : prompt;

    // Download image from URL
    private async Task<ReadOnlyMemory<byte>> DownloadImageAsync(string imageUrl, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(imageUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ImageGenClientException($"Failed to download image: HTTP {response.StatusCode}", (int)response.StatusCode);
        }

        var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (imageBytes.Length == 0)
        {
            throw new ImageGenClientException("Downloaded image is empty", 0);
        }

        return imageBytes;
    }

    // Determine image format from bytes
    private static ImageFormat DetermineImageFormat(ReadOnlyMemory<byte> imageBytes)
    {
        var span = imageBytes.Span;

        if (span.Length >= 8 &&
            span[0] == 0x89 && span[1] == 0x50 && span[2] == 0x4E && span[3] == 0x47)
        {
            return ImageFormat.Png;
        }

        if (span.Length >= 2 && span[0] == 0xFF && span[1] == 0xD8)
        {
            return ImageFormat.Jpeg;
        }

        if (span.Length >= 12 &&
            span[0] == 0x52 && span[1] == 0x49 && span[2] == 0x46 && span[3] == 0x46 &&
            span[8] == 0x57 && span[9] == 0x45 && span[10] == 0x42 && span[11] == 0x50)
        {
            return ImageFormat.Webp;
        }

        return ImageFormat.Png; // Default
    }

    // Get image dimensions from bytes
    private static (int Width, int Height) GetImageDimensions(ReadOnlyMemory<byte> imageBytes, ImageFormat format)
    {
        var span = imageBytes.Span;

        try
        {
            switch (format)
            {
                case ImageFormat.Png when span.Length >= 24:
                    var pngWidth = (span[16] << 24) | (span[17] << 16) | (span[18] << 8) | span[19];
                    var pngHeight = (span[20] << 24) | (span[21] << 16) | (span[22] << 8) | span[23];
                    return (pngWidth, pngHeight);

                case ImageFormat.Jpeg:
                    return ParseJpegDimensions(span);

                case ImageFormat.Webp when span.Length >= 30:
                    var webpWidth = span[26] | (span[27] << 8);
                    var webpHeight = span[28] | (span[29] << 8);
                    return (webpWidth, webpHeight);
            }
        }
        catch
        {
            // Fall back to default dimensions on error
        }

        return (1024, 1024); // Default fallback
    }

    // Parse JPEG dimensions from SOF marker
    private static (int Width, int Height) ParseJpegDimensions(ReadOnlySpan<byte> jpegData)
    {
        int i = 2; // Skip SOI marker

        while (i < jpegData.Length - 1)
        {
            if (jpegData[i] == 0xFF)
            {
                var marker = jpegData[i + 1];

                if (marker >= 0xC0 && marker <= 0xC3 && i + 9 < jpegData.Length)
                {
                    var height = (jpegData[i + 5] << 8) | jpegData[i + 6];
                    var width = (jpegData[i + 7] << 8) | jpegData[i + 8];
                    return (width, height);
                }

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
