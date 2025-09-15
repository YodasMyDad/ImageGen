using ImageGen.Models;

namespace ImageGen.Core;

/// <summary>
/// Client interface for image generation and editing operations.
/// Provides methods for generating images from text, editing existing images,
/// and creating variations with high fidelity preservation.
/// </summary>
public interface IImageGenClient
{
    /// <summary>
    /// Generates an image from a text prompt.
    /// </summary>
    /// <param name="request">The generation request parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated image result.</returns>
    Task<ImageResult> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple images from a text prompt.
    /// </summary>
    /// <param name="request">The generation request parameters.</param>
    /// <param name="count">The number of images to generate (1-10, depending on provider limits).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of generated image results.</returns>
    Task<IReadOnlyList<ImageResult>> GenerateManyAsync(GenerateRequest request, int count = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits an existing image based on a text prompt.
    /// Supports primary image editing with optional secondary images and masks for high-fidelity results.
    /// </summary>
    /// <param name="request">The edit request parameters including the primary image and editing instructions.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the edited image result.</returns>
    Task<ImageResult> EditAsync(EditRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates variations of an existing image.
    /// </summary>
    /// <param name="baseImage">The base image to create variations of.</param>
    /// <param name="prompt">Optional prompt to guide the variations.</param>
    /// <param name="count">The number of variations to generate (1-10, depending on provider limits).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of image variations.</returns>
    Task<IReadOnlyList<ImageResult>> VariationsAsync(
        Stream baseImage,
        string? prompt = null,
        int count = 4,
        CancellationToken cancellationToken = default);
}
