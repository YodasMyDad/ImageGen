namespace ImageGen.Exceptions;

/// <summary>
/// Exception thrown when an image exceeds the maximum allowed size for processing.
/// </summary>
public class ImageTooLargeException : ImageGenException
{
    /// <summary>
    /// Gets the size of the image in bytes.
    /// </summary>
    public long ImageSize { get; }

    /// <summary>
    /// Gets the maximum allowed size in bytes.
    /// </summary>
    public long MaxAllowedSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageTooLargeException"/> class.
    /// </summary>
    /// <param name="imageSize">The size of the image in bytes.</param>
    /// <param name="maxAllowedSize">The maximum allowed size in bytes.</param>
    public ImageTooLargeException(long imageSize, long maxAllowedSize)
        : base($"Image size {imageSize} bytes exceeds the maximum allowed size of {maxAllowedSize} bytes.")
    {
        ImageSize = imageSize;
        MaxAllowedSize = maxAllowedSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageTooLargeException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="imageSize">The size of the image in bytes.</param>
    /// <param name="maxAllowedSize">The maximum allowed size in bytes.</param>
    public ImageTooLargeException(string message, long imageSize, long maxAllowedSize)
        : base(message)
    {
        ImageSize = imageSize;
        MaxAllowedSize = maxAllowedSize;
    }
}
