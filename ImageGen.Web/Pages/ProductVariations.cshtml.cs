using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

public class ProductVariationsModel : PageModel
{
    private readonly IImageGenClient _imageClient;
    private readonly IWebHostEnvironment _environment;

    public ProductVariationsModel(IImageGenClient imageClient, IWebHostEnvironment environment)
    {
        _imageClient = imageClient;
        _environment = environment;
    }

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string? VariationStyle { get; set; }

    [BindProperty]
    public int Count { get; set; } = 4;

    [BindProperty]
    public string? OriginalImageUrl { get; set; }

    public List<string>? VariationUrls { get; set; }
    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(_environment.WebRootPath, "images");

    private static readonly Dictionary<string, string> StylePrompts = new()
    {
        ["artistic"] = "Create artistic and creative variations with abstract elements, artistic compositions, and creative styling",
        ["minimalist"] = "Create minimalist variations with clean designs, simple compositions, and elegant minimalism",
        ["vibrant"] = "Create vibrant and colorful variations with bold colors, energetic compositions, and vivid styling",
        ["luxury"] = "Create luxury and elegant variations with sophisticated styling, premium aesthetics, and elegant compositions",
        ["vintage"] = "Create vintage-style variations with retro elements, nostalgic colors, and classic design treatments",
        ["modern"] = "Create modern and contemporary variations with current design trends, geometric elements, and contemporary styling"
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
        VariationStyle ??= "artistic";

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

            var variationStyle = VariationStyle ?? "artistic";
            var prompt = StylePrompts.GetValueOrDefault(variationStyle,
                "Create creative variations of this product with different artistic styles");

            var variations = await _imageClient.VariationsAsync(
                baseImage: imageStream,
                prompt: prompt,
                count: Math.Clamp(Count, 1, 4)
            );

            VariationUrls = new List<string>();

            foreach (var variation in variations)
            {
                var variationFileName = $"variation_{Guid.NewGuid()}.{variation.Format.ToString().ToLower()}";
                var variationPath = Path.Combine(ImagesPath, variationFileName);

                await using var fileStream = new FileStream(variationPath, FileMode.Create);
                await fileStream.WriteAsync(variation.Bytes.ToArray());

                VariationUrls.Add($"/images/{variationFileName}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error generating variations: {ex.Message}";
        }

        return Page();
    }
}
