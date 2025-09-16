using ImageGen.Configuration;
using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

public class EnhanceProductModel(ImageGenOptions options, IImageGenClient imageClient, IWebHostEnvironment environment) : BasePageModel(options)
{
    private readonly IImageGenClient _imageClient = imageClient;
    private readonly IWebHostEnvironment _environment = environment;

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string? EnhancementType { get; set; } = "lighting";

    [BindProperty]
    public string? OriginalImageUrl { get; set; }

    [BindProperty]
    public string? ProcessedImageUrl { get; set; }

    [BindProperty]
    public string? Prompt { get; set; }

    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(_environment.WebRootPath, "images");

    private static readonly Dictionary<string, string> EnhancementPrompts = new()
    {
        ["lighting"] = "Improve the lighting to be professional and studio-quality. Add proper shadows, highlights, and even illumination. Make the product look professionally photographed.",
        ["details"] = "Enhance sharpness and details. Make fine textures and product features more visible and crisp. Improve overall image clarity and definition.",
        ["studio"] = "Apply complete professional studio retouching. Fix any imperfections, improve colors, enhance lighting, and make the product look like a high-end catalog photo.",
        ["colors"] = "Enhance colors and contrast. Make colors more vivid and appealing. Improve color balance and saturation for better visual impact.",
        ["texture"] = "Enhance material textures and surface details. Make fabrics, metals, woods, or other materials look more realistic and tactile.",
        ["all"] = "Apply complete professional enhancement: improve lighting, enhance details and sharpness, boost colors, enhance textures, and apply studio-quality retouching. Make this look like a premium product photograph."
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
        EnhancementType ??= "lighting";

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
            var prompt = Prompt ?? "Enhance this product photo with professional quality improvements";

            var editRequest = new EditRequest(
                PrimaryImage: imageStream,
                Prompt: prompt,
                InputFidelity: InputFidelity.High,
                Quality: ImageQuality.High,
                Format: ImageFormat.Jpeg
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
