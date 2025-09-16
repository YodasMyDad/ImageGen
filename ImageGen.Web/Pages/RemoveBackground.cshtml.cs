using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

/// <summary>
/// Simple demo page for removing image backgrounds using AI.
/// </summary>
public class RemoveBackgroundModel(IImageGenClient imageClient, IWebHostEnvironment environment) : PageModel
{
    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string? BackgroundType { get; set; } = "transparent";

    [BindProperty]
    public string? OriginalImageUrl { get; set; }

    public string? ResultImageUrl { get; set; }
    public string? CurrentPrompt { get; set; }
    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(environment.WebRootPath, "images");

    /// <summary>
    /// Upload and validate the image.
    /// </summary>
    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (ImageFile == null || ImageFile.Length == 0)
        {
            ErrorMessage = "Please select an image file.";
            return Page();
        }

        // Basic validation
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            ErrorMessage = "Only image files are allowed.";
            return Page();
        }

        // Save uploaded image
        Directory.CreateDirectory(ImagesPath);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(ImagesPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await ImageFile.CopyToAsync(stream);

        OriginalImageUrl = $"/images/{fileName}";
        return Page();
    }

    /// <summary>
    /// Process the image to remove background.
    /// </summary>
    public async Task<IActionResult> OnPostProcessAsync()
    {
        if (string.IsNullOrEmpty(OriginalImageUrl))
        {
            ErrorMessage = "Please upload an image first.";
            return Page();
        }

        try
        {
            // Create the AI prompt
            CurrentPrompt = BackgroundType == "transparent"
                ? "Remove the background completely and make it transparent. Keep the subject intact with high detail."
                : "Remove the background and replace it with a clean white background. Keep the subject intact with high detail.";

            // Process with AI
            var imagePath = Path.Combine(environment.WebRootPath, OriginalImageUrl.TrimStart('/'));
            using var imageStream = new FileStream(imagePath, FileMode.Open);

            var request = new EditRequest(
                PrimaryImage: imageStream,
                Prompt: CurrentPrompt,
                Quality: ImageQuality.High,
                Format: BackgroundType == "transparent" ? ImageFormat.Png : ImageFormat.Jpeg,
                TransparentBackground: BackgroundType == "transparent"
            );

            var result = await imageClient.EditAsync(request);

            // Save result
            var resultFileName = $"result_{Guid.NewGuid()}.{result.Format.ToString().ToLower()}";
            var resultPath = Path.Combine(ImagesPath, resultFileName);

            await using var fileStream = new FileStream(resultPath, FileMode.Create);
            await fileStream.WriteAsync(result.Bytes.ToArray());

            ResultImageUrl = $"/images/{resultFileName}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }

        return Page();
    }
}
