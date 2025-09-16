using ImageGen.Models;

namespace ImageGen.Core;

/// <summary>
/// Simple interface for OpenAI GPT-Image-1 image operations.
/// </summary>
public interface IImageGenClient
{
    /// <summary>
    /// Generate a new image from text prompt.
    /// </summary>
    Task<ImageResult> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate multiple images from the same prompt.
    /// </summary>
    Task<IReadOnlyList<ImageResult>> GenerateManyAsync(GenerateRequest request, int count = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edit an existing image with a prompt.
    /// </summary>
    Task<ImageResult> EditAsync(EditRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create variations of an existing image.
    /// </summary>
    Task<IReadOnlyList<ImageResult>> VariationsAsync(
        Stream baseImage,
        string? prompt = null,
        int count = 4,
        CancellationToken cancellationToken = default);
}
