using ImageGen.Configuration;
using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

/// <summary>
/// Custom AI image processing page allowing users to upload images and enter any prompt.
/// </summary>
public class CustomModel(ImageGenOptions options, IImageGenClient imageClient, IWebHostEnvironment environment) : BasePageModel(options)
{
    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string? ImageUrl { get; set; }

    [BindProperty]
    public string? Prompt { get; set; }

    [BindProperty]
    public ImageQuality Quality { get; set; } = ImageQuality.High;

    [BindProperty]
    public ImageFormat Format { get; set; } = ImageFormat.Png;

    public string? ResultImageUrl { get; set; }
    public string? CurrentPrompt { get; set; }
    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(environment.WebRootPath, "images");

    /// <summary>
    /// Handle image upload and show preview.
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

        ImageUrl = $"/images/{fileName}";

        return Page();
    }

    /// <summary>
    /// Process image with custom AI prompt.
    /// </summary>
    public async Task<IActionResult> OnPostProcessAsync()
    {
        if (string.IsNullOrEmpty(ImageUrl))
        {
            ErrorMessage = "Please upload an image first.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Prompt))
        {
            ErrorMessage = "Please enter a prompt for the AI.";
            return Page();
        }

        try
        {
            // Store the current prompt for display
            CurrentPrompt = Prompt;

            // Process with AI
            var imagePath = Path.Combine(environment.WebRootPath, ImageUrl.TrimStart('/'));
            using var imageStream = new FileStream(imagePath, FileMode.Open);

            var request = new EditRequest(
                PrimaryImage: imageStream,
                Prompt: Prompt,
                Quality: Quality,
                Format: Format
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
