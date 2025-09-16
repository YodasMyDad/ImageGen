namespace ImageGen.Models;

/// <summary>
/// Request parameters for editing an existing image.
/// Supports primary image editing with optional secondary images and masks.
/// </summary>
public sealed record EditRequest(
    /// <summary>
    /// The primary image to edit.
    /// This image will have the highest fidelity preservation during editing.
    /// Must be a readable stream (file upload, memory stream, etc.).
    /// </summary>
    Stream PrimaryImage,

    /// <summary>
    /// The text prompt describing how to edit the image.
    /// Should describe the desired changes clearly.
    /// </summary>
    string Prompt,

    /// <summary>
    /// Optional secondary images to composite with the primary image.
    /// These can include overlays, logos, textures, or other assets.
    /// The primary image takes precedence in terms of fidelity preservation.
    /// </summary>
    IReadOnlyList<Stream>? SecondaryImages = null,

    /// <summary>
    /// An optional mask image that defines which parts of the primary image to edit.
    /// The mask should be the same dimensions as the primary image.
    /// White areas indicate regions to edit, black areas are preserved.
    /// </summary>
    Stream? Mask = null,

    /// <summary>
    /// The input fidelity level for preserving details from the primary image.
    /// High fidelity is recommended for preserving faces, logos, and distinctive features.
    /// </summary>
    InputFidelity InputFidelity = InputFidelity.High,

    /// <summary>
    /// The desired width of the edited image in pixels.
    /// If not specified, matches the primary image dimensions.
    /// </summary>
    int? Width = null,

    /// <summary>
    /// The desired height of the edited image in pixels.
    /// If not specified, matches the primary image dimensions.
    /// </summary>
    int? Height = null,

    /// <summary>
    /// The quality level for the edited image.
    /// Defaults to High quality for best results.
    /// </summary>
    ImageQuality Quality = ImageQuality.High,

    /// <summary>
    /// Whether to generate an image with a transparent background.
    /// Only supported by certain formats (PNG, WebP).
    /// Defaults to false (opaque background).
    /// </summary>
    bool TransparentBackground = false,

    /// <summary>
    /// The output format for the edited image.
    /// Defaults to JPEG for smaller file sizes and better web compatibility.
    /// </summary>
    ImageFormat Format = ImageFormat.Jpeg,

    /// <summary>
    /// Additional provider-specific parameters.
    /// Can be used to pass extra options not covered by the standard properties.
    /// </summary>
    IDictionary<string, string>? Extra = null);
