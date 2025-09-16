namespace ImageGen.Configuration;

/// <summary>
/// Configuration options for the ImageGen client.
/// </summary>
public sealed class ImageGenOptions
{
    /// <summary>
    /// Gets or sets the API key for authentication with the image generation service.
    /// This is required and should be kept secure.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the image generation service API.
    /// Defaults to the OpenAI images API endpoint.
    /// </summary>
    public Uri BaseUrl { get; set; } = new("https://api.openai.com/v1/");

    /// <summary>
    /// Gets or sets the model to use for image generation and editing.
    /// Defaults to "gpt-image-1" for high-fidelity image generation.
    /// </summary>
    public string Model { get; set; } = "gpt-image-1";

    /// <summary>
    /// Gets or sets the timeout for individual requests.
    /// Defaults to 5 minutes to allow for complex operations like background removal.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// Defaults to 4 retries with exponential backoff.
    /// </summary>
    public int MaxRetries { get; set; } = 4;

    /// <summary>
    /// Gets or sets the handler lifetime for the HttpClient.
    /// Defaults to 5 minutes to balance connection reuse and DNS updates.
    /// </summary>
    public TimeSpan HandlerLifetime { get; set; } = TimeSpan.FromMinutes(5);
}
