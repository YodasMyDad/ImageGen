namespace ImageGen.Models;

/// <summary>
/// Request parameters for generating an image from a text prompt.
/// </summary>
public sealed record GenerateRequest(
    /// <summary>
    /// The text prompt describing the image to generate.
    /// Should be descriptive and specific for best results.
    /// </summary>
    string Prompt,

    /// <summary>
    /// The desired width of the generated image in pixels.
    /// If not specified, uses the model's default dimensions.
    /// Common values: 1024, 1536, 2048 (square).
    /// </summary>
    int? Width = null,

    /// <summary>
    /// The desired height of the generated image in pixels.
    /// If not specified, uses the model's default dimensions.
    /// Common values: 1024, 1536, 2048 (square).
    /// </summary>
    int? Height = null,

    /// <summary>
    /// The quality level for the generated image.
    /// Defaults to High quality for better results.
    /// </summary>
    ImageQuality Quality = ImageQuality.High,

    /// <summary>
    /// Whether to generate an image with a transparent background.
    /// Only supported by certain formats (PNG, WebP).
    /// Defaults to false (opaque background).
    /// </summary>
    bool TransparentBackground = false,

    /// <summary>
    /// The output format for the generated image.
    /// Defaults to JPEG for smaller file sizes and better web compatibility.
    /// </summary>
    ImageFormat Format = ImageFormat.Jpeg,

    /// <summary>
    /// An optional seed for reproducible generation.
    /// If provided, the same seed should produce similar results.
    /// </summary>
    string? Seed = null,

    /// <summary>
    /// Additional provider-specific parameters.
    /// Can be used to pass extra options not covered by the standard properties.
    /// </summary>
    IDictionary<string, string>? Extra = null);
