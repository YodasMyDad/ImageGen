# ImageGen 🖼️

**AI-Powered Image Editing for .NET Developers**

Transform images with OpenAI's GPT-Image-1 model. Generate, edit, and enhance images using simple C# code. Perfect for web apps, APIs, and automation tools.

[![NuGet](https://img.shields.io/nuget/v/ImageGen.svg)](https://www.nuget.org/packages/ImageGen/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)


## ✨ Cool Things You Can Do

- **Remove backgrounds** from product photos instantly
- **Change backgrounds** to any scene or environment
- **Add logos/watermarks** with perfect positioning
- **Enhance product photos** with professional lighting and details
- **Generate image variations** with different artistic styles
- **Create images from text** descriptions
- **Batch process** multiple images at once

## 🚀 Quick Start

### 1. Install the Package

```bash
dotnet add package ImageGen
```

### 2. Configure Your App

```csharp
// In Program.cs
using ImageGen.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add ImageGen with your OpenAI API key
builder.Services.AddImageGenClient(options =>
{
    options.ApiKey = builder.Configuration["OPENAI_API_KEY"]!;
});

var app = builder.Build();
```

### 3. Quick Stream Examples

**From a file on disk:**
```csharp
using var fileStream = new FileStream("path/to/image.jpg", FileMode.Open);
```

**From an uploaded file (ASP.NET Core):**
```csharp
// In your controller or page handler
public async Task<IActionResult> UploadImage(IFormFile uploadedFile)
{
    using var stream = uploadedFile.OpenReadStream();
    // Now use the stream with ImageGen
}
```

**From a URL:**
```csharp
using var httpClient = new HttpClient();
using var stream = await httpClient.GetStreamAsync("https://example.com/image.jpg");
```

**From a byte array:**
```csharp
var imageBytes = File.ReadAllBytes("path/to/image.jpg");
using var stream = new MemoryStream(imageBytes);
```

### 4. Use It!

```csharp
using ImageGen.Core;
using ImageGen.Models;

// Inject the client
public class ImageService(IImageGenClient client)
{
    public async Task<byte[]> RemoveBackground(Stream imageStream)
    {
        var result = await client.EditAsync(new EditRequest(
            PrimaryImage: imageStream,
            Prompt: "Remove the background completely",
            InputFidelity: InputFidelity.High
        ));

        return result.Bytes.ToArray();
    }
}
```

## 📝 Code Examples

### Remove Background
```csharp
public async Task<byte[]> RemoveBackground(Stream imageStream)
{
    var result = await client.EditAsync(new EditRequest(
        PrimaryImage: imageStream,
        Prompt: "Remove the background, make it transparent",
        InputFidelity: InputFidelity.High, // Keeps details crisp
        Format: ImageFormat.Png
    ));

    return result.Bytes.ToArray();
}
```

### Change Product Background
```csharp
public async Task<byte[]> ChangeBackground(Stream productImage)
{
    var result = await client.EditAsync(new EditRequest(
        PrimaryImage: productImage,
        Prompt: "Place this product on a luxury marble background",
        InputFidelity: InputFidelity.High,
        Quality: ImageQuality.High
    ));

    return result.Bytes.ToArray();
}
```

### Add Logo to Image
```csharp
public async Task<byte[]> AddLogo(Stream mainImage, Stream logoImage)
{
    var result = await client.EditAsync(new EditRequest(
        PrimaryImage: mainImage,
        SecondaryImages: new[] { logoImage },
        Prompt: "Add the logo in the bottom right corner, blend naturally",
        InputFidelity: InputFidelity.High
    ));

    return result.Bytes.ToArray();
}
```

### Generate Images from Text
```csharp
public async Task<byte[]> GenerateImage(string description)
{
    var result = await client.GenerateAsync(new GenerateRequest(
        Prompt: $"Professional product photo: {description}",
        Width: 1024,
        Height: 1024,
        Quality: ImageQuality.High,
        Format: ImageFormat.Png
    ));

    return result.Bytes.ToArray();
}
```

### Create Multiple Variations
```csharp
public async Task<List<byte[]>> CreateVariations(Stream baseImage)
{
    var variations = await client.VariationsAsync(
        baseImage: baseImage,
        prompt: "Create artistic variations with different lighting",
        count: 4
    );

    return variations.Select(v => v.Bytes.ToArray()).ToList();
}
```

## ⚙️ Configuration

```csharp
builder.Services.AddImageGenClient(options =>
{
    // Required: Your OpenAI API key
    options.ApiKey = builder.Configuration["OPENAI_API_KEY"]!;

    // Optional: Timeout for API calls (default: 3 minutes)
    options.RequestTimeout = TimeSpan.FromMinutes(3);
});
```

**Add to appsettings.json:**
```json
{
  "ImageGen": {
    "ApiKey": "sk-your-api-key-here"
  }
}
```

## 🎯 Key Features

- **High-Fidelity Editing**: `InputFidelity.High` preserves faces, logos, and details
- **Simple & Clean**: Easy-to-understand API designed for developers
- **Async First**: Built for modern .NET with async/await
- **Type Safe**: Strong typing prevents runtime errors
- **Production Ready**: Handles errors gracefully and logs important events

## 🛠️ API Overview

### Main Methods
- `GenerateAsync()` - Create images from text prompts
- `EditAsync()` - Edit existing images with prompts
- `VariationsAsync()` - Create multiple variations
- `GenerateManyAsync()` - Batch generate multiple images

### Key Classes
- `EditRequest` - Configure image editing
- `GenerateRequest` - Configure image generation
- `ImageResult` - Contains the processed image data

## 🖥️ Try the Demo

Want to see it in action? Check out the **simple web demo**:

```bash
cd ImageGen.Web
dotnet run
```

Visit `http://localhost:5001` to try:
- **Remove backgrounds** - Upload an image and make the background transparent or white
- **Add logos** - Place logos on images with perfect positioning
- **Change backgrounds** - Replace image backgrounds with new scenes
- **Enhance products** - Improve product photos with better details
- **Create variations** - Generate multiple creative versions

Each example shows you the exact AI prompt being used, so you can learn and adapt them for your own projects!

## 📚 Learn More

- **Full API Documentation**: Check the XML comments in the code
- **Error Handling**: See `ImageGenException`, `RateLimitExceededException`
- **Best Practices**: Always use `InputFidelity.High` for important edits

## 🤝 Contributing

Found a bug or want to add a feature? We'd love your help!

1. Fork the repo
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 📄 License

MIT License - see LICENSE file for details.

---

**Ready to supercharge your .NET apps with AI image editing?** 🚀

Start with the demo app, then integrate ImageGen into your project today!
