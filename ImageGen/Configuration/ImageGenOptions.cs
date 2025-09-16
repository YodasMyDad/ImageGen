namespace ImageGen.Configuration;

/// <summary>
/// Configuration for the ImageGen client.
/// </summary>
public sealed class ImageGenOptions
{
    /// <summary>
    /// Your OpenAI API key (required).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI API base URL.
    /// </summary>
    public Uri BaseUrl { get; set; } = new("https://api.openai.com/v1/");

    /// <summary>
    /// AI model to use (don't change this).
    /// </summary>
    public string Model { get; set; } = "gpt-image-1";

    /// <summary>
    /// How long to wait for API responses.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(3);
}
