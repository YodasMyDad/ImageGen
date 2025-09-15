namespace ImageGen.Exceptions;

/// <summary>
/// Base exception for all ImageGen-related errors.
/// </summary>
public class ImageGenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenException"/> class.
    /// </summary>
    public ImageGenException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ImageGenException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ImageGenException(string message, Exception innerException) : base(message, innerException) { }
}
