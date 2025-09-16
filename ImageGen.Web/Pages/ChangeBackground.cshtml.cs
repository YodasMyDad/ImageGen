using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

public class ChangeBackgroundModel : PageModel
{
    private readonly IImageGenClient _imageClient;
    private readonly IWebHostEnvironment _environment;

    public ChangeBackgroundModel(IImageGenClient imageClient, IWebHostEnvironment environment)
    {
        _imageClient = imageClient;
        _environment = environment;
    }

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string? BackgroundStyle { get; set; } = "studio";

    [BindProperty]
    public string? OriginalImageUrl { get; set; }

    [BindProperty]
    public string? ProcessedImageUrl { get; set; }

    [BindProperty]
    public string? Prompt { get; set; }

    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(_environment.WebRootPath, "images");

    private static readonly Dictionary<string, string> BackgroundPrompts = new()
    {
        ["studio"] = "Place this product on a clean professional studio background with perfect white lighting and subtle shadows",
        ["gradient"] = "Place this product on a modern gradient background with soft blue to purple colors, elegant and contemporary",
        ["nature"] = "Place this product in a beautiful natural outdoor setting with soft greenery and natural lighting",
        ["urban"] = "Place this product in a modern urban environment with city architecture and contemporary design",
        ["luxury"] = "Place this product in an elegant luxury setting with marble surfaces, gold accents, and sophisticated lighting",
        ["minimal"] = "Place this product on a clean minimalist background with geometric shapes and neutral colors"
    };

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
        BackgroundStyle ??= "studio";

        // Don't set prompt here - it will be set in Step 2 based on dropdown selection

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

            // Use the user-provided prompt
            var prompt = Prompt ?? "Place this product on a clean professional background";

            var editRequest = new EditRequest(
                PrimaryImage: imageStream,
                Prompt: prompt,
                InputFidelity: InputFidelity.High,
                Quality: ImageQuality.High,
                Format: ImageFormat.Png
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
