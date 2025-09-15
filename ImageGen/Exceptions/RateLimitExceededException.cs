namespace ImageGen.Exceptions;

/// <summary>
/// Exception thrown when the rate limit for the image generation service has been exceeded.
/// </summary>
public class RateLimitExceededException : ImageGenClientException
{
    /// <summary>
    /// Gets the time to wait before retrying the request, if specified by the service.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitExceededException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="retryAfter">The time to wait before retrying.</param>
    /// <param name="requestId">The request ID from the service response.</param>
    public RateLimitExceededException(string message, TimeSpan? retryAfter = null, string? requestId = null)
        : base(message, 429, requestId)
    {
        RetryAfter = retryAfter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitExceededException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="retryAfter">The time to wait before retrying.</param>
    /// <param name="requestId">The request ID from the service response.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public RateLimitExceededException(string message, TimeSpan? retryAfter, string? requestId, Exception innerException)
        : base(message, 429, requestId, innerException)
    {
        RetryAfter = retryAfter;
    }
}
