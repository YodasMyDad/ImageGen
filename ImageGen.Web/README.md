# ImageGen Web Demo

A simple ASP.NET Core Razor Pages demo showing how to use the ImageGen library for AI-powered image editing with OpenAI's GPT-Image-1 model. Perfect for learning and getting started!

## What You Can Try

This demo includes 5 simple examples:

### 🧽 Remove Background
- Upload an image and remove its background
- Choose transparent or white background
- Great for product photos or isolating subjects

### 🎨 Change Background
- Replace backgrounds with new scenes
- Perfect for product photography
- See instant transformations

### 🏷️ Add Logo
- Add logos to your images
- Control position and size
- Professional-looking results

### ✨ Enhance Product
- Improve product photos automatically
- Better lighting and details
- Professional-quality enhancements

### 🔄 Create Variations
- Generate multiple versions of an image
- Different creative styles
- Explore artistic possibilities

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- OpenAI API key

### Setup

1. **Clone the repository** (if not already done):
   ```bash
   git clone <repository-url>
   cd ImageGen
   ```

2. **Configure API Key**:
   - Open `ImageGen.Web/appsettings.json`
   - Replace `"your-openai-api-key-here"` with your actual OpenAI API key:
   ```json
   {
     "ImageGen": {
       "ApiKey": "sk-your-actual-api-key-here"
     }
   }
   ```

3. **Build and run**:
   ```bash
   cd ImageGen.Web
   dotnet run
   ```

4. **Open your browser** to `http://localhost:5001` (or the URL shown in the console)

## Project Structure

```
ImageGen.Web/
├── Pages/
│   ├── Index.cshtml/cs         # Home page with demo overview
│   ├── RemoveBackground.cshtml/cs    # Background removal demo
│   ├── ChangeBackground.cshtml/cs    # Background replacement demo
│   ├── AddLogo.cshtml/cs             # Logo addition demo
│   ├── EnhanceProduct.cshtml/cs      # Product enhancement demo
│   ├── ProductVariations.cshtml/cs   # Image variations demo
│   └── Shared/
│       └── _Layout.cshtml            # Simple Bootstrap layout
├── wwwroot/
│   ├── images/                       # Uploaded and processed images
│   └── ...
├── appsettings.json                  # Configuration (API key)
├── Program.cs                        # Application startup
└── ImageGen.Web.csproj              # Project file
```

## Technology Stack

- **ASP.NET Core 9.0** - Web framework
- **Razor Pages** - Simple page-focused development
- **Bootstrap 5.3** - Clean, responsive UI
- **ImageGen Library** - AI-powered image processing
- **OpenAI GPT-Image-1** - High-quality image generation and editing

## How It Works

Each demo page follows this simple pattern:

1. **Upload**: Select image files using the file picker
2. **Configure**: Choose your options (like position, size, background type)
3. **Process**: Click the button to send to OpenAI's AI for processing
4. **See Results**: View the processed image and download it

**Best part**: Each example shows you the exact AI prompt being used, so you can learn and create your own variations!

Images are temporarily stored in `wwwroot/images/` for processing.

## API Usage

The demo uses the ImageGen library which provides:

- **High-fidelity image editing** with `InputFidelity.High`
- **Simple image composition** for adding logos and overlays
- **Image variations** for creative exploration
- **Clean error handling** with helpful messages
- **Easy-to-follow code examples** you can copy and modify

## Perfect for Learning

Each demo page shows you:
- **The exact AI prompt** being sent to OpenAI
- **Simple form inputs** you can easily replicate
- **Clear code examples** in C# that you can copy
- **Step-by-step process** that's easy to understand

**New to AI image editing?** This demo is perfect for you! See exactly how the prompts work and adapt them for your own projects.

## Security Notes

- API keys are stored in `appsettings.json` (configure appropriately for production)
- Images are temporarily stored on disk - implement cleanup for production use
- Rate limiting and authentication should be added for production deployment

## Contributing

This is a demo application for the ImageGen library. For library contributions, see the main ImageGen project.

## License

See the main ImageGen project for licensing information.
