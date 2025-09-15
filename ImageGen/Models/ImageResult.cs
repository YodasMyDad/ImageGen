namespace ImageGen.Models;

/// <summary>
/// Represents the result of an image generation or editing operation.
/// Contains the generated image data and associated metadata.
/// </summary>
public sealed record ImageResult(
    /// <summary>
    /// The raw bytes of the generated or edited image.
    /// Use this to save to disk, stream to clients, or further process the image.
    /// </summary>
    ReadOnlyMemory<byte> Bytes,

    /// <summary>
    /// The width of the image in pixels.
    /// </summary>
    int Width,

    /// <summary>
    /// The height of the image in pixels.
    /// </summary>
    int Height,

    /// <summary>
    /// The format of the image (PNG, JPEG, WebP).
    /// </summary>
    ImageFormat Format,

    /// <summary>
    /// The provider's unique identifier for this request.
    /// Useful for debugging, support requests, and tracking usage.
    /// </summary>
    string ProviderRequestId,

    /// <summary>
    /// The number of input tokens used by the request (if available from provider).
    /// Can be used for cost tracking and usage monitoring.
    /// </summary>
    long? InputTokens = null,

    /// <summary>
    /// The number of output tokens used by the request (if available from provider).
    /// Can be used for cost tracking and usage monitoring.
    /// </summary>
    long? OutputTokens = null);
