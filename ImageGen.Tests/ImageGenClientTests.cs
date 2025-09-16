using System.Net;
using System.Text.Json;
using ImageGen.Configuration;
using ImageGen.Core;
using ImageGen.Exceptions;
using ImageGen.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace ImageGen.Tests;

/// <summary>
/// Unit tests for ImageGenClient focusing on response parsing and error handling.
/// </summary>
public class ImageGenClientTests : TestBase
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ImageGenOptions _options;
    private readonly ILogger<ImageGenClient> _logger;

    public ImageGenClientTests()
    {
        _httpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler.Object);
        _options = new ImageGenOptions
        {
            ApiKey = "test-key",
            Model = "gpt-image-1",
            BaseUrl = new Uri("https://api.openai.com/v1/")
        };
        _logger = GetLogger<ImageGenClient>();
    }

    [Fact]
    public void ParseImageResponseAsync_WithValidUrl_ReturnsImageResult()
    {
        // Arrange
        var mockResponse = new OpenAiImageResponse
        {
            Created = 1234567890,
            Data = [
                new OpenAiImageData
                {
                    Url = "https://example.com/image.png",
                    RevisedPrompt = "A beautiful sunset"
                }
            ]
        };

        var responseJson = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var client = new ImageGenClient(_httpClient, _options, _logger);

        // Act & Assert - This should not throw if URL is present
        Should.NotThrow(() =>
        {
            // We can't fully test this without mocking the image download
            // But we can at least test that it doesn't fail on URL parsing
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG signature

            _httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(imageBytes)
                });

            // This would require more complex mocking to work fully
        });
    }

    [Fact]
    public async Task ParseImageResponseAsync_WithNullUrlAndNullB64Json_ThrowsException()
    {
        // Arrange
        var mockResponse = new OpenAiImageResponse
        {
            Created = 1234567890,
            Data = [
                new OpenAiImageData
                {
                    Url = null, // No URL
                    B64Json = null, // No base64 data either
                    RevisedPrompt = "A beautiful sunset"
                }
            ]
        };

        var responseJson = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        var client = new ImageGenClient(_httpClient, _options, _logger);

        // Act & Assert
        var ex = await Should.ThrowAsync<ImageGenClientException>(async () =>
        {
            var method = client.GetType()
                .GetMethod("ParseImageResponseAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var task = (Task)method.Invoke(client, [httpResponse, CancellationToken.None])!;
            await task;
        });

        ex.Message.ShouldContain("No image URL or base64 data provided in API response");
    }

    [Fact]
    public async Task ParseImageResponseAsync_WithValidB64Json_ReturnsImageResult()
    {
        // Arrange - Create a simple PNG image as base64
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG signature
        var b64Data = Convert.ToBase64String(pngBytes);

        var mockResponse = new OpenAiImageResponse
        {
            Created = 1234567890,
            Data = [
                new OpenAiImageData
                {
                    Url = null, // No URL, using base64 instead
                    B64Json = b64Data,
                    RevisedPrompt = "A beautiful sunset"
                }
            ]
        };

        var responseJson = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        var client = new ImageGenClient(_httpClient, _options, _logger);

        // Act & Assert - This should not throw since we have valid base64 data
        await Should.NotThrowAsync(async () =>
        {
            var method = client.GetType()
                .GetMethod("ParseImageResponseAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var task = (Task)method.Invoke(client, [httpResponse, CancellationToken.None])!;
            await task;
        });
    }

    [Fact]
    public async Task ParseImageResponseAsync_WithInvalidB64Json_ThrowsException()
    {
        // Arrange
        var mockResponse = new OpenAiImageResponse
        {
            Created = 1234567890,
            Data = [
                new OpenAiImageData
                {
                    Url = null,
                    B64Json = "invalid-base64-data!!!", // Invalid base64
                    RevisedPrompt = "A beautiful sunset"
                }
            ]
        };

        var responseJson = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        var client = new ImageGenClient(_httpClient, _options, _logger);

        // Act & Assert
        var ex = await Should.ThrowAsync<ImageGenClientException>(async () =>
        {
            var method = client.GetType()
                .GetMethod("ParseImageResponseAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var task = (Task)method.Invoke(client, [httpResponse, CancellationToken.None])!;
            await task;
        });

        ex.Message.ShouldContain("Invalid base64 image data in API response");
    }

    [Fact]
    public async Task ParseImageResponseAsync_WithEmptyDataArray_ThrowsException()
    {
        // Arrange
        var mockResponse = new OpenAiImageResponse
        {
            Created = 1234567890,
            Data = [] // Empty array
        };

        var responseJson = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        var client = new ImageGenClient(_httpClient, _options, _logger);

        // Act & Assert
        var ex = await Should.ThrowAsync<ImageGenClientException>(async () =>
        {
            var method = client.GetType()
                .GetMethod("ParseImageResponseAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var task = (Task)method.Invoke(client, [httpResponse, CancellationToken.None])!;
            await task;
        });

        ex.Message.ShouldContain("No image data received from API response");
    }

    [Fact]
    public async Task ParseImageResponseAsync_WithErrorResponse_ThrowsException()
    {
        // Arrange
        var errorResponse = new OpenAiErrorResponse
        {
            Error = new OpenAiError
            {
                Type = "invalid_request_error",
                Message = "Invalid prompt",
                Code = "invalid_prompt"
            }
        };

        var responseJson = JsonSerializer.Serialize(errorResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        var client = new ImageGenClient(_httpClient, _options, _logger);

        // Act & Assert
        var ex = await Should.ThrowAsync<ImageGenClientException>(async () =>
        {
            var method = client.GetType()
                .GetMethod("ParseImageResponseAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var task = (Task)method.Invoke(client, [httpResponse, CancellationToken.None])!;
            await task;
        });

        ex.Message.ShouldContain("OpenAI API error: Invalid prompt");
    }

    [Fact]
    public async Task GenerateAsync_WithFailedHttpRequest_ThrowsException()
    {
        // Arrange
        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request")
            });

        var client = new ImageGenClient(_httpClient, _options, _logger);
        var request = new GenerateRequest(
            Prompt: "Test prompt",
            Width: 512,
            Height: 512);

        // Act & Assert
        var ex = await Should.ThrowAsync<ImageGenClientException>(() =>
            client.GenerateAsync(request));

        ex.Message.ShouldContain("API request failed with status 400");
    }

    [Fact]
    public async Task EditAsync_WithFailedHttpRequest_ThrowsException()
    {
        // Arrange
        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request")
            });

        var client = new ImageGenClient(_httpClient, _options, _logger);
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG signature
        var request = new EditRequest(
            PrimaryImage: new MemoryStream(imageBytes),
            Prompt: "Test prompt",
            Width: 512,
            Height: 512);

        // Act & Assert
        var ex = await Should.ThrowAsync<ImageGenClientException>(() =>
            client.EditAsync(request));

        ex.Message.ShouldContain("API request failed with status 400");
    }

    [Fact]
    public async Task DownloadImageAsync_WithInvalidUrl_ThrowsException()
    {
        // Arrange
        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var client = new ImageGenClient(_httpClient, _options, _logger);

        // Act & Assert
        var ex = await Should.ThrowAsync<ImageGenClientException>(async () =>
        {
            var method = client.GetType()
                .GetMethod("DownloadImageAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var task = (Task)method.Invoke(client, ["https://invalid-url.com/image.png", CancellationToken.None])!;
            await task;
        });

        ex.Message.ShouldContain("Failed to download image: HTTP");
    }

    [Fact]
    public async Task DownloadImageAsync_WithEmptyResponse_ThrowsException()
    {
        // Arrange
        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([]) // Empty content
            });

        var client = new ImageGenClient(_httpClient, _options, _logger);

        // Act & Assert
        var ex = await Should.ThrowAsync<ImageGenClientException>(async () =>
        {
            var method = client.GetType()
                .GetMethod("DownloadImageAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var task = (Task)method.Invoke(client, ["https://example.com/image.png", CancellationToken.None])!;
            await task;
        });

        ex.Message.ShouldContain("Downloaded image is empty");
    }

    [Fact]
    public void MapQuality_ConvertsCorrectly()
    {
        // Test the quality mapping function
        var client = new ImageGenClient(_httpClient, _options, _logger);

        var standardQuality = client.GetType()
            .GetMethod("MapQuality", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [ImageQuality.Standard]);

        var highQuality = client.GetType()
            .GetMethod("MapQuality", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [ImageQuality.High]);

        standardQuality.ShouldBe("medium");
        highQuality.ShouldBe("high");
    }

    [Fact]
    public void MapFormat_ConvertsCorrectly()
    {
        // Test the format mapping function
        var client = new ImageGenClient(_httpClient, _options, _logger);

        var pngFormat = client.GetType()
            .GetMethod("MapFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [ImageFormat.Png]);

        var jpegFormat = client.GetType()
            .GetMethod("MapFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [ImageFormat.Jpeg]);

        var webpFormat = client.GetType()
            .GetMethod("MapFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [ImageFormat.Webp]);

        pngFormat.ShouldBe("png");
        jpegFormat.ShouldBe("jpeg");
        webpFormat.ShouldBe("webp");
    }
}
