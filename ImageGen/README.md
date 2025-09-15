# ImageGen 🖼️

**AI-Powered Image Editing for .NET Developers**

Transform images with OpenAI's GPT-Image-1 model. Generate, edit, and enhance images using simple C# code. Perfect for web apps, APIs, and automation tools.

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
dotnet add package ImageGen --version 1.0.0-beta.1
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

### 3. Use It!

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

    // Optional settings (model defaults to "gpt-image-1")
    options.RequestTimeout = TimeSpan.FromMinutes(3);
    options.MaxRetries = 3; // Auto-retry failed requests
});
```

**Add to appsettings.json:**
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-api-key-here"
  }
}
```

## 🎯 Key Features

- **High-Fidelity Editing**: `InputFidelity.High` preserves faces, logos, and details
- **Smart Retries**: Automatic retry with exponential backoff
- **Async First**: Built for modern .NET with async/await
- **Type Safe**: Strong typing prevents runtime errors
- **Observable**: Built-in logging and tracing
- **Testable**: Easy to mock and test

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

Want to see it in action? Check out the **interactive web demo**:

```bash
cd ImageGen.Web
dotnet run
```

Visit `http://localhost:5243` to try:
- Remove backgrounds
- Change backgrounds
- Add logos
- Enhance products
- Generate variations

All examples are interactive with drag-and-drop uploads!

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
