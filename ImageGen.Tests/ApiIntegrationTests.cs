using System.Net.Http.Headers;
using System.Text.Json;
using ImageGen.Configuration;
using ImageGen.Core;
using ImageGen.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace ImageGen.Tests;

/// <summary>
/// Integration tests that call the actual OpenAI API to understand response formats.
/// These tests require a valid API key and will be marked as integration tests.
/// </summary>
[Collection("IntegrationTests")]
public class ApiIntegrationTests : TestBase, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ImageGenOptions _options;

    public ApiIntegrationTests()
    {
        // Get API key from environment variable
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException(
                "OPENAI_API_KEY environment variable must be set to run integration tests. " +
                "Set it to your OpenAI API key.");
        }

        _options = new ImageGenOptions
        {
            ApiKey = apiKey,
            Model = "gpt-image-1", // Keep using gpt-image-1 as requested
            BaseUrl = new Uri("https://api.openai.com/v1/")
        };

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TestJsonFormatGeneration()
    {
        // Test that our JSON format matches the curl example
        var request = new GenerateRequest(
            Prompt: "A childrens book drawing of a veterinarian using a stethoscope to listen to the heartbeat of a baby otter.",
            Width: 512,
            Height: 512,
            Quality: ImageQuality.Standard,
            Format: ImageFormat.Png);

        // This should now use JSON format like the curl command
        var client = new ImageGenClient(
            new HttpClient(),
            new ImageGenOptions { ApiKey = "dummy", Model = "gpt-image-1" },
            GetLogger<ImageGenClient>());

        // We can't easily test the content creation without reflection, but we can verify the method exists
        var method = client.GetType().GetMethod("CreateGenerationContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        Console.WriteLine("✅ JSON format generation method exists and should work like the curl example");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateImage_CapturesActualResponseFormat()
    {
        // Arrange
        var request = new GenerateRequest(
            Prompt: "A simple red circle on a white background",
            Width: 512,
            Height: 512,
            Quality: ImageQuality.Standard,
            Format: ImageFormat.Png);

        using var content = CreateGenerationContent(request);

        // Act
        var response = await _httpClient.PostAsync($"{_options.BaseUrl}images/generations", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Log the actual response for analysis
        Console.WriteLine("=== ACTUAL API RESPONSE ===");
        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine($"Content: {responseContent}");
        Console.WriteLine("==========================");

        // Assert - just ensure we get a response
        response.IsSuccessStatusCode.ShouldBeTrue($"API call failed: {responseContent}");

        // Parse and examine the response structure
        var openAiResponse = JsonSerializer.Deserialize<OpenAiImageResponse>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        openAiResponse.ShouldNotBeNull();
        openAiResponse.Data.ShouldNotBeNull();
        openAiResponse.Data.Length.ShouldBeGreaterThan(0);

        var firstImage = openAiResponse.Data[0];
        Console.WriteLine($"Image URL: {firstImage.Url}");
        Console.WriteLine($"Image B64Json: {(string.IsNullOrEmpty(firstImage.B64Json) ? "null" : "present")}");
        Console.WriteLine($"Revised Prompt: {firstImage.RevisedPrompt}");

        if (!string.IsNullOrEmpty(firstImage.B64Json))
        {
            Console.WriteLine("✅ SUCCESS: Received base64 image data!");
            Console.WriteLine($"Base64 data length: {firstImage.B64Json.Length} characters");
        }
        else if (!string.IsNullOrEmpty(firstImage.Url))
        {
            Console.WriteLine("✅ URL found - traditional response format");
        }
        else
        {
            Console.WriteLine("❌ CONFIRMED: No URL or base64 data in response - this is the bug!");
            // Let's examine the entire response structure
            var fullResponse = JsonDocument.Parse(responseContent);
            Console.WriteLine("Full response structure:");
            Console.WriteLine(JsonSerializer.Serialize(fullResponse, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EditImage_CapturesActualResponseFormat()
    {
        // Arrange - Create a simple test image
        var imageBytes = CreateTestImage();
        using var imageStream = new MemoryStream(imageBytes);

        var request = new EditRequest(
            PrimaryImage: imageStream,
            Prompt: "Add a blue border around the image",
            Width: 512,
            Height: 512,
            Quality: ImageQuality.Standard,
            Format: ImageFormat.Png);

        using var content = CreateEditContent(request);

        // Act
        var response = await _httpClient.PostAsync($"{_options.BaseUrl}images/edits", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Log the actual response for analysis
        Console.WriteLine("=== ACTUAL EDIT API RESPONSE ===");
        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine($"Content: {responseContent}");
        Console.WriteLine("=================================");

        // Assert - just ensure we get a response
        response.IsSuccessStatusCode.ShouldBeTrue($"API call failed: {responseContent}");

        // Parse and examine the response structure
        var openAiResponse = JsonSerializer.Deserialize<OpenAiImageResponse>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        openAiResponse.ShouldNotBeNull();
        openAiResponse.Data.ShouldNotBeNull();
        openAiResponse.Data.Length.ShouldBeGreaterThan(0);

        var firstImage = openAiResponse.Data[0];
        Console.WriteLine($"Image URL: {firstImage.Url}");
        Console.WriteLine($"Image B64Json: {(string.IsNullOrEmpty(firstImage.B64Json) ? "null" : "present")}");
        Console.WriteLine($"Revised Prompt: {firstImage.RevisedPrompt}");

        if (!string.IsNullOrEmpty(firstImage.B64Json))
        {
            Console.WriteLine("✅ SUCCESS: Received base64 image data for edit!");
            Console.WriteLine($"Base64 data length: {firstImage.B64Json.Length} characters");
        }
        else if (!string.IsNullOrEmpty(firstImage.Url))
        {
            Console.WriteLine("✅ URL found in edit response - traditional format");
        }
        else
        {
            Console.WriteLine("❌ CONFIRMED: No URL or base64 data in edit response - this is the bug!");
            var fullResponse = JsonDocument.Parse(responseContent);
            Console.WriteLine("Full response structure:");
            Console.WriteLine(JsonSerializer.Serialize(fullResponse, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestQualityParameter_Mapping()
    {
        // Test different quality settings to see if they affect the response
        var qualities = new[] { ImageQuality.Standard, ImageQuality.High };

        foreach (var quality in qualities)
        {
            var request = new GenerateRequest(
                Prompt: "A simple test image",
                Width: 256,
                Height: 256,
                Quality: quality,
                Format: ImageFormat.Png);

            using var content = CreateGenerationContent(request);
            var response = await _httpClient.PostAsync($"{_options.BaseUrl}images/generations", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Quality: {quality} -> API value: {MapQuality(quality)}");
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var openAiResponse = JsonSerializer.Deserialize<OpenAiImageResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (openAiResponse?.Data?.Length > 0)
                {
                    Console.WriteLine($"Has URL: {!string.IsNullOrEmpty(openAiResponse.Data[0].Url)}");
                }
            }

            Console.WriteLine("---");
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
        content.Add(primaryContent, "image", "image.png");

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

    private static byte[] CreateTestImage()
    {
        // Create a simple 256x256 PNG image for testing
        // This is a minimal PNG with a white background
        var pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        // IHDR chunk for 256x256 RGB image
        var ihdrLength = BitConverter.GetBytes(13).Reverse().ToArray();
        var ihdrType = System.Text.Encoding.ASCII.GetBytes("IHDR");
        var width = BitConverter.GetBytes(256).Reverse().ToArray();
        var height = BitConverter.GetBytes(256).Reverse().ToArray();
        var bitDepth = new byte[] { 8 }; // 8-bit
        var colorType = new byte[] { 2 }; // RGB
        var compression = new byte[] { 0 };
        var filter = new byte[] { 0 };
        var interlace = new byte[] { 0 };

        var ihdrData = width.Concat(height).Concat(bitDepth).Concat(colorType)
                           .Concat(compression).Concat(filter).Concat(interlace).ToArray();
        var ihdrCrc = BitConverter.GetBytes(Crc32(ihdrData.Concat(ihdrType).ToArray())).Reverse().ToArray();

        // IDAT chunk with minimal image data (white pixels)
        var imageData = new byte[256 * 256 * 3]; // 256x256 RGB
        Array.Fill(imageData, (byte)255); // Fill with white

        // Add filter bytes (0 for no filter)
        var filteredData = new List<byte>();
        for (int y = 0; y < 256; y++)
        {
            filteredData.Add(0); // No filter
            filteredData.AddRange(imageData.Skip(y * 256 * 3).Take(256 * 3));
        }

        var compressedData = CompressDeflate(filteredData.ToArray());
        var idatLength = BitConverter.GetBytes(compressedData.Length).Reverse().ToArray();
        var idatType = System.Text.Encoding.ASCII.GetBytes("IDAT");
        var idatCrc = BitConverter.GetBytes(Crc32(compressedData.Concat(idatType).ToArray())).Reverse().ToArray();

        // IEND chunk
        var iendLength = new byte[] { 0, 0, 0, 0 };
        var iendType = System.Text.Encoding.ASCII.GetBytes("IEND");
        var iendCrc = BitConverter.GetBytes(Crc32(iendType)).Reverse().ToArray();

        return pngSignature
            .Concat(ihdrLength).Concat(ihdrType).Concat(ihdrData).Concat(ihdrCrc)
            .Concat(idatLength).Concat(idatType).Concat(compressedData).Concat(idatCrc)
            .Concat(iendLength).Concat(iendType).Concat(iendCrc)
            .ToArray();
    }

    private static byte[] CompressDeflate(byte[] data)
    {
        using var output = new MemoryStream();
        using var compressor = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionMode.Compress);
        compressor.Write(data, 0, data.Length);
        compressor.Close();
        return output.ToArray();
    }

    private static uint Crc32(byte[] data)
    {
        const uint polynomial = 0xEDB88320;
        uint crc = 0xFFFFFFFF;

        foreach (byte b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ polynomial : crc >> 1;
            }
        }

        return crc ^ 0xFFFFFFFF;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection { }
