using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

public class RemoveBackgroundModel : PageModel
{
    private readonly IImageGenClient _imageClient;
    private readonly IWebHostEnvironment _environment;

    public RemoveBackgroundModel(IImageGenClient imageClient, IWebHostEnvironment environment)
    {
        _imageClient = imageClient;
        _environment = environment;
    }

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string? BackgroundType { get; set; }

    public string? OriginalImageUrl { get; set; }
    public string? ProcessedImageUrl { get; set; }
    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(_environment.WebRootPath, "images");

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (ImageFile == null || ImageFile.Length == 0)
        {
            ErrorMessage = "Please select an image file.";
            return Page();
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            ErrorMessage = "Only image files (.jpg, .png, .webp) are allowed.";
            return Page();
        }

        // Ensure images directory exists
        Directory.CreateDirectory(ImagesPath);

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(ImagesPath, fileName);

        // Save uploaded file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await ImageFile.CopyToAsync(stream);
        }

        OriginalImageUrl = $"/images/{fileName}";
        BackgroundType ??= "transparent";

        return Page();
    }

    public async Task<IActionResult> OnPostProcessAsync()
    {
        if (string.IsNullOrEmpty(OriginalImageUrl))
        {
            ErrorMessage = "No image uploaded.";
            return Page();
        }

        try
        {
            var imagePath = Path.Combine(_environment.WebRootPath, OriginalImageUrl.TrimStart('/'));
            using var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            var backgroundType = BackgroundType ?? "transparent";
            var prompt = backgroundType == "transparent"
                ? "Remove the background completely, make it transparent, keep the subject intact with high detail"
                : "Remove the background and replace it with a clean white background, keep the subject intact with high detail";

            var editRequest = new EditRequest(
                PrimaryImage: imageStream,
                Prompt: prompt,
                InputFidelity: InputFidelity.High,
                Quality: ImageQuality.High,
                Format: backgroundType == "transparent" ? ImageFormat.Png : ImageFormat.Jpeg,
                TransparentBackground: backgroundType == "transparent"
            );

            var result = await _imageClient.EditAsync(editRequest);

            // Save processed image
            var processedFileName = $"processed_{Guid.NewGuid()}.{result.Format.ToString().ToLower()}";
            var processedPath = Path.Combine(ImagesPath, processedFileName);

            await using var fileStream = new FileStream(processedPath, FileMode.Create);
            await fileStream.WriteAsync(result.Bytes.ToArray());

            ProcessedImageUrl = $"/images/{processedFileName}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error processing image: {ex.Message}";
        }

        return Page();
    }
}
