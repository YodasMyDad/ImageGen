# ImageGen Web Demo

A comprehensive ASP.NET Core Razor Pages demo showcasing the ImageGen library's AI-powered image editing capabilities using OpenAI's GPT-Image-1 model.

## Features

This demo includes 5 interactive examples:

### 🧽 Remove Background
- Automatically remove image backgrounds with high precision
- Choose between transparent or white backgrounds
- Perfect for product photography and isolated subjects

### 🎨 Change Background
- Replace product backgrounds with various environments
- Options include studio, gradient, nature, urban, luxury, and minimalist settings
- Transform product photos instantly

### 🏷️ Add Logo
- Seamlessly integrate logos and watermarks into images
- Control logo position and size
- Professional branding and watermarking

### ✨ Enhance Product
- Improve product photos with professional lighting and details
- Various enhancement types: lighting, sharpness, studio quality, colors, textures
- Complete professional retouching

### 🔄 Create Variations
- Generate multiple creative variations of product images
- Different artistic styles: artistic, minimalist, vibrant, luxury, vintage, modern
- Create 2-4 variations at once

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
   dotnet build
   dotnet run
   ```

4. **Open your browser** to `https://localhost:5001` (or the URL shown in the console)

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
│       └── _Layout.cshtml            # Bootstrap 5 layout
├── wwwroot/
│   ├── images/                       # Uploaded and processed images
│   └── ...
├── appsettings.json                  # Configuration (API key)
├── Program.cs                        # Application startup
└── ImageGen.Web.csproj              # Project file
```

## Technology Stack

- **ASP.NET Core 9.0** - Web framework
- **Razor Pages** - Page-focused web development
- **Bootstrap 5.3** - Responsive UI framework
- **ImageGen Library** - AI-powered image processing
- **OpenAI GPT-Image-1** - High-fidelity image generation and editing

## How It Works

Each demo page follows this pattern:

1. **Upload**: Drag & drop or select image files
2. **Configure**: Choose processing options (style, position, size, etc.)
3. **Process**: Click to send to OpenAI's API for AI processing
4. **Download**: Save the processed results

Images are temporarily stored in `wwwroot/images/` for processing and display.

## API Usage

The demo uses the ImageGen library which provides:

- **High-fidelity image editing** with `InputFidelity.High`
- **Multiple image composition** for logo addition
- **Image variations** for creative exploration
- **Robust error handling** and retry logic
- **OpenTelemetry observability**

## Security Notes

- API keys are stored in `appsettings.json` (configure appropriately for production)
- Images are temporarily stored on disk - implement cleanup for production use
- Rate limiting and authentication should be added for production deployment

## Contributing

This is a demo application for the ImageGen library. For library contributions, see the main ImageGen project.

## License

See the main ImageGen project for licensing information.
