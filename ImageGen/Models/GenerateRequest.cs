namespace ImageGen.Models;

/// <summary>
/// Parameters for generating a new image from text.
/// </summary>
public sealed record GenerateRequest(
    /// <summary>
    /// Text description of the image you want to generate.
    /// Be specific and descriptive for best results.
    /// </summary>
    string Prompt,

    /// <summary>
    /// Image width in pixels (optional).
    /// </summary>
    int? Width = null,

    /// <summary>
    /// Image height in pixels (optional).
    /// </summary>
    int? Height = null,

    /// <summary>
    /// Image quality level.
    /// </summary>
    ImageQuality Quality = ImageQuality.High,

    /// <summary>
    /// Whether to make the background transparent.
    /// </summary>
    bool TransparentBackground = false,

    /// <summary>
    /// Output image format.
    /// </summary>
    ImageFormat Format = ImageFormat.Jpeg,

    /// <summary>
    /// Optional seed for reproducible results.
    /// </summary>
    string? Seed = null,

    /// <summary>
    /// Extra parameters if needed.
    /// </summary>
    IDictionary<string, string>? Extra = null);
