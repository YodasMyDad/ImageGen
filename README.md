# ImageGenAi 🖼️

**AI-Powered Image Editing for .NET Developers**

Transform and create images with OpenAI's new gpt-image-1 model. Generate, edit, and enhance images using simple C# code. Perfect for web apps, APIs, and automation tools.

*When creating high quality and high fidelity images the API can take up to 60+ seconds to respond*

[![NuGet](https://img.shields.io/nuget/v/ImageGenAi.svg)](https://www.nuget.org/packages/ImageGenAi/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)


## ✨ Examples In The Demo Web Project

- **Remove backgrounds** from product photos instantly
- **Change backgrounds** to any scene or environment
- **Add logos/watermarks** with perfect positioning
- **Enhance product photos** with professional lighting and details
- **Create images from text** descriptions

Your imagination is the limit!

## 🚀 Quick Start

### 1. Install the Package

```bash
dotnet add package ImageGenAi
```

### 2. Configure Your App

**Important:** You need to make sure your API key has access to the gpt-image-1 model or this will fail. Get your API key from https://platform.openai.com/api-keys

```csharp
// In Program.cs
using ImageGen.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddImageGenClient(options =>
{
    options.ApiKey = builder.Configuration["OPENAI_API_KEY"]!;
    // Api key from appsettings.json
});

var app = builder.Build();
```

### 3. Quick Stream Examples

Some examples of how to get an image stream into ImageGen.

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

## 💾 Saving Images to Disk

To save images to disk, simply write the bytes to a file:

```csharp
// Save any result to disk
await File.WriteAllBytesAsync("output.png", result.Bytes.ToArray());

// Or with automatic file extension based on format
var extension = result.Format switch {
    ImageFormat.Png => "png",
    ImageFormat.Jpeg => "jpg",
    ImageFormat.Webp => "webp"
};
await File.WriteAllBytesAsync($"output.{extension}", result.Bytes.ToArray());
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

## 🖥️ Try the Demo

Want to see it in action? Check out the **simple web demo**:

- **Remove backgrounds** - Upload an image and make the background transparent or white
- **Add logos** - Place logos on images with perfect positioning
- **Change backgrounds** - Replace image backgrounds with new scenes
- **Enhance products** - Improve product photos with better details

Each example shows you the exact AI prompt being used, so you can learn and adapt them for your own projects!

## 📚 Learn More

- **Full API Documentation**: Check the XML comments in the code
- **Error Handling**: See `ImageGenException`, `RateLimitExceededException`
- **Best Practices**: Always use `InputFidelity.High` for important edits

## 📄 License

MIT License - see LICENSE file for details.

---

**Ready to supercharge your .NET apps with AI image editing?** 🚀

Start with the demo app, then integrate ImageGenAI into your project today!
