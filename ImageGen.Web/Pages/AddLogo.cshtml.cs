using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

public class AddLogoModel : PageModel
{
    private readonly IImageGenClient _imageClient;
    private readonly IWebHostEnvironment _environment;

    public AddLogoModel(IImageGenClient imageClient, IWebHostEnvironment environment)
    {
        _imageClient = imageClient;
        _environment = environment;
    }

    [BindProperty]
    public IFormFile? MainImage { get; set; }

    [BindProperty]
    public IFormFile? LogoImage { get; set; }

    [BindProperty]
    public string? Position { get; set; }

    [BindProperty]
    public string? Size { get; set; }

    [BindProperty]
    public string? MainImageUrl { get; set; }

    [BindProperty]
    public string? LogoImageUrl { get; set; }

    [BindProperty]
    public string? ProcessedImageUrl { get; set; }

    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(_environment.WebRootPath, "images");

    private static readonly Dictionary<string, string> PositionPrompts = new()
    {
        ["bottom-right"] = "Add the logo in the bottom right corner with appropriate spacing from the edges",
        ["bottom-left"] = "Add the logo in the bottom left corner with appropriate spacing from the edges",
        ["top-right"] = "Add the logo in the top right corner with appropriate spacing from the edges",
        ["top-left"] = "Add the logo in the top left corner with appropriate spacing from the edges",
        ["center"] = "Add the logo in the center of the image with subtle transparency"
    };

    private static readonly Dictionary<string, string> SizePrompts = new()
    {
        ["small"] = "Make the logo small (about 10% of the image width)",
        ["medium"] = "Make the logo medium-sized (about 15% of the image width)",
        ["large"] = "Make the logo large (about 20% of the image width)",
        ["xlarge"] = "Make the logo extra large (about 25% of the image width)"
    };

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (MainImage == null || MainImage.Length == 0)
        {
            ErrorMessage = "Please select a main image file.";
            return Page();
        }

        if (LogoImage == null || LogoImage.Length == 0)
        {
            ErrorMessage = "Please select a logo image file.";
            return Page();
        }

        // Validate file types
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var mainExtension = Path.GetExtension(MainImage.FileName).ToLowerInvariant();
        var logoExtension = Path.GetExtension(LogoImage.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(mainExtension) || !allowedExtensions.Contains(logoExtension))
        {
            ErrorMessage = "Only image files (.jpg, .png, .webp) are allowed.";
            return Page();
        }

        // Ensure images directory exists
        Directory.CreateDirectory(ImagesPath);

        // Save main image
        var mainFileName = $"main_{Guid.NewGuid()}{mainExtension}";
        var mainFilePath = Path.Combine(ImagesPath, mainFileName);
        using (var stream = new FileStream(mainFilePath, FileMode.Create))
        {
            await MainImage.CopyToAsync(stream);
        }

        // Save logo image
        var logoFileName = $"logo_{Guid.NewGuid()}{logoExtension}";
        var logoFilePath = Path.Combine(ImagesPath, logoFileName);
        using (var stream = new FileStream(logoFilePath, FileMode.Create))
        {
            await LogoImage.CopyToAsync(stream);
        }

        MainImageUrl = $"/images/{mainFileName}";
        LogoImageUrl = $"/images/{logoFileName}";
        Position ??= "bottom-right";
        Size ??= "medium";

        return Page();
    }

    public async Task<IActionResult> OnPostProcessAsync()
    {
        if (string.IsNullOrEmpty(MainImageUrl) || string.IsNullOrEmpty(LogoImageUrl))
        {
            ErrorMessage = "Images not uploaded.";
            return Page();
        }

        try
        {
            // Open both image streams
            var mainImagePath = Path.Combine(_environment.WebRootPath, MainImageUrl.TrimStart('/'));
            var logoImagePath = Path.Combine(_environment.WebRootPath, LogoImageUrl.TrimStart('/'));

            using var mainImageStream = new FileStream(mainImagePath, FileMode.Open, FileAccess.Read);
            using var logoImageStream = new FileStream(logoImagePath, FileMode.Open, FileAccess.Read);

            var position = Position ?? "bottom-right";
            var size = Size ?? "medium";

            var positionPrompt = PositionPrompts.GetValueOrDefault(position, "Add the logo in the bottom right corner");
            var sizePrompt = SizePrompts.GetValueOrDefault(size, "Make the logo medium-sized");

            var prompt = $"{positionPrompt}. {sizePrompt}. Blend the logo naturally with the main image, ensuring proper transparency and positioning. The logo should look professional and integrated.";

            var editRequest = new EditRequest(
                PrimaryImage: mainImageStream,
                SecondaryImages: new[] { logoImageStream },
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
            ErrorMessage = $"Error processing images: {ex.Message}";
        }

        return Page();
    }
}
