using System.Text.Json.Serialization;

namespace ImageGen.Models;

/// <summary>
/// Error response from OpenAI API.
/// </summary>
internal sealed class OpenAiErrorResponse
{
    /// <summary>
    /// Error details.
    /// </summary>
    [JsonPropertyName("error")]
    public OpenAiError? Error { get; set; }
}

/// <summary>
/// OpenAI error details.
/// </summary>
internal sealed class OpenAiError
{
    /// <summary>
    /// Error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Error type.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Error code.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

/// <summary>
/// Response from OpenAI's image generation/editing API.
/// </summary>
internal sealed class OpenAiImageResponse
{
    /// <summary>
    /// Timestamp when the image was created.
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// Array of generated images.
    /// </summary>
    [JsonPropertyName("data")]
    public OpenAiImageData[] Data { get; set; } = [];
}

/// <summary>
/// Individual image data from OpenAI response.
/// </summary>
internal sealed class OpenAiImageData
{
    /// <summary>
    /// URL to download the generated image (for URL-based responses).
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Base64-encoded image data (for base64 responses like gpt-image-1).
    /// </summary>
    [JsonPropertyName("b64_json")]
    public string? B64Json { get; set; }

    /// <summary>
    /// Revised prompt used for generation (may be null).
    /// </summary>
    [JsonPropertyName("revised_prompt")]
    public string? RevisedPrompt { get; set; }
}
