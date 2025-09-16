using ImageGen.Core;
using ImageGen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace ImageGen.Web.Pages;

public class RemoveBackgroundModel(IImageGenClient imageClient, IWebHostEnvironment environment)
    : PageModel
{
    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string? BackgroundType { get; set; }

    [BindProperty]
    public string? OriginalImageUrl { get; set; }

    [BindProperty]
    public string? ProcessedImageUrl { get; set; }

    public string? ErrorMessage { get; set; }

    private string ImagesPath => Path.Combine(environment.WebRootPath, "images");

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

        // Validate file size (4MB limit for DALL-E API)
        const long maxFileSize = 4 * 1024 * 1024; // 4MB
        if (ImageFile.Length > maxFileSize)
        {
            ErrorMessage = $"Image file size must be less than 4MB. Your file is {ImageFile.Length / 1024 / 1024:F1}MB.";
            return Page();
        }

        // Validate image dimensions
        try
        {
            using var imageStream = ImageFile.OpenReadStream();
            using var image = SixLabors.ImageSharp.Image.Load(imageStream);

            const int maxDimension = 4096; // DALL-E maximum dimension
            if (image.Width > maxDimension || image.Height > maxDimension)
            {
                ErrorMessage = $"Image dimensions must be less than {maxDimension}x{maxDimension} pixels. Your image is {image.Width}x{image.Height} pixels.";
                return Page();
            }

            // Also check minimum dimensions
            const int minDimension = 64;
            if (image.Width < minDimension || image.Height < minDimension)
            {
                ErrorMessage = $"Image dimensions must be at least {minDimension}x{minDimension} pixels. Your image is {image.Width}x{image.Height} pixels.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to validate image: {ex.Message}. Please ensure the file is a valid image.";
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
            var imagePath = Path.Combine(environment.WebRootPath, OriginalImageUrl.TrimStart('/'));
            using var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            var backgroundType = BackgroundType ?? "transparent";
            var prompt = backgroundType == "transparent"
                ? "Remove the background completely, make it transparent, keep the subject intact with high detail"
                : "Remove the background and replace it with a clean white background, keep the subject intact with high detail";

            var extension = Path.GetExtension(imagePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
            var originalFileName = Path.GetFileName(imagePath);
            var extra = new Dictionary<string, string>
            {
                ["filename"] = originalFileName,
                ["content_type"] = contentType
            };

            var editRequest = new EditRequest(
                PrimaryImage: imageStream,
                Prompt: prompt,
                InputFidelity: InputFidelity.High,
                Quality: ImageQuality.High,
                Format: backgroundType == "transparent" ? ImageFormat.Png : ImageFormat.Jpeg,
                TransparentBackground: backgroundType == "transparent",
                Extra: extra
            );

            var result = await imageClient.EditAsync(editRequest);

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
