using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

/// <summary>
/// Simple demo page for adding logos to images using AI.
/// </summary>
public class AddLogoModel(IImageGenClient imageClient, IWebHostEnvironment environment) : PageModel
{
    [BindProperty]
    public IFormFile? MainImage { get; set; }

    [BindProperty]
    public IFormFile? LogoImage { get; set; }

    [BindProperty]
    public string? Position { get; set; } = "bottom-right";

    [BindProperty]
    public string? Size { get; set; } = "medium";

    [BindProperty]
    public string? MainImageUrl { get; set; }

    [BindProperty]
    public string? LogoImageUrl { get; set; }

    public string? ResultImageUrl { get; set; }
    public string? CurrentPrompt { get; set; }
    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(environment.WebRootPath, "images");

    /// <summary>
    /// Handle image upload and show preview.
    /// </summary>
    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (MainImage == null || LogoImage == null)
        {
            ErrorMessage = "Please select both images.";
            return Page();
        }

        // Save uploaded images
        Directory.CreateDirectory(ImagesPath);

        var mainFileName = $"main_{Guid.NewGuid()}{Path.GetExtension(MainImage.FileName)}";
        var logoFileName = $"logo_{Guid.NewGuid()}{Path.GetExtension(LogoImage.FileName)}";

        var mainPath = Path.Combine(ImagesPath, mainFileName);
        var logoPath = Path.Combine(ImagesPath, logoFileName);

        await using (var stream = new FileStream(mainPath, FileMode.Create))
            await MainImage.CopyToAsync(stream);

        await using (var stream = new FileStream(logoPath, FileMode.Create))
            await LogoImage.CopyToAsync(stream);

        MainImageUrl = $"/images/{mainFileName}";
        LogoImageUrl = $"/images/{logoFileName}";

        return Page();
    }

    /// <summary>
    /// Process images with AI to add logo.
    /// </summary>
    public async Task<IActionResult> OnPostProcessAsync()
    {
        if (string.IsNullOrEmpty(MainImageUrl) || string.IsNullOrEmpty(LogoImageUrl))
        {
            ErrorMessage = "Please upload images first.";
            return Page();
        }

        try
        {
            // Build the prompt for the AI
            var positionText = Position switch
            {
                "bottom-right" => "bottom right corner",
                "bottom-left" => "bottom left corner",
                "top-right" => "top right corner",
                "top-left" => "top left corner",
                "center" => "center",
                _ => "bottom right corner"
            };

            var sizeText = Size switch
            {
                "small" => "small (10% of image width)",
                "medium" => "medium (15% of image width)",
                "large" => "large (20% of image width)",
                "xlarge" => "extra large (25% of image width)",
                _ => "medium (15% of image width)"
            };

            CurrentPrompt = $"Add a logo to the {positionText} of the image. Make the logo {sizeText}. Blend it naturally and ensure it looks professional.";

            // Process with AI
            var mainPath = Path.Combine(environment.WebRootPath, MainImageUrl.TrimStart('/'));
            var logoPath = Path.Combine(environment.WebRootPath, LogoImageUrl.TrimStart('/'));

            using var mainStream = new FileStream(mainPath, FileMode.Open);
            using var logoStream = new FileStream(logoPath, FileMode.Open);

            var request = new EditRequest(
                PrimaryImage: mainStream,
                SecondaryImages: [logoStream],
                Prompt: CurrentPrompt,
                Quality: ImageQuality.High,
                Format: ImageFormat.Png
            );

            var result = await imageClient.EditAsync(request);

            // Save result
            var resultFileName = $"result_{Guid.NewGuid()}.png";
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
