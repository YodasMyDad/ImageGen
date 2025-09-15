namespace ImageGen.Exceptions;

/// <summary>
/// Exception thrown when the image generation service returns an error.
/// </summary>
public class ImageGenClientException : ImageGenException
{
    /// <summary>
    /// Gets the HTTP status code returned by the service.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the request ID from the service response, if available.
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenClientException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="requestId">The request ID from the service response.</param>
    public ImageGenClientException(string message, int statusCode, string? requestId = null)
        : base(message)
    {
        StatusCode = statusCode;
        RequestId = requestId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenClientException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="requestId">The request ID from the service response.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public ImageGenClientException(string message, int statusCode, string? requestId, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        RequestId = requestId;
    }
}
