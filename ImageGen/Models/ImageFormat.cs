namespace ImageGen.Models;

/// <summary>
/// Specifies the output format for generated images.
/// Different formats offer different trade-offs between file size, quality, and transparency support.
/// </summary>
public enum ImageFormat
{
    /// <summary>
    /// PNG format - supports transparency, lossless compression.
    /// Good for images that need to be composited or have transparent backgrounds.
    /// </summary>
    Png,

    /// <summary>
    /// JPEG format - lossy compression, smaller file sizes.
    /// Best for photographs and images where file size is important.
    /// Does not support transparency.
    /// </summary>
    Jpeg,

    /// <summary>
    /// WebP format - modern format with good compression and transparency support.
    /// Good balance between file size and quality, with broad browser support.
    /// </summary>
    Webp
}
