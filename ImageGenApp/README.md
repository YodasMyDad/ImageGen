# 🎨 AI Image Generator

A modern, clean desktop application for AI-powered image generation and editing built with WinUI 3 and .NET 9.

## Features

### ✨ Core Functionality
- **Primary Image Upload**: Upload a primary image that serves as the base for AI editing
- **Additional Images**: Add multiple secondary images for compositing
- **AI Prompt Input**: Large textarea for detailed prompts describing desired changes
- **Real-time Generation**: Async processing with loading indicators for long API calls
- **Image Download**: Save generated images with proper file dialogs
- **Clear Function**: Reset all inputs and start fresh

### ⚙️ Settings Management
- **API Key Configuration**: Secure local storage of your ImageGen API key
- **Default Quality Settings**: Choose between Standard and High quality
- **Output Format Options**: JPEG, PNG, or WebP formats
- **Input Fidelity Levels**: Default or High fidelity for preserving image details

### 🎨 Modern UI Design
- **Clean Interface**: Modern WinUI 3 design with acrylic effects
- **Responsive Layout**: Adaptive layout that works on different screen sizes
- **Loading States**: Professional loading spinners and progress indicators
- **Error Handling**: Comprehensive error dialogs and user feedback
- **Accessibility**: Proper keyboard navigation and screen reader support

## Getting Started

### Prerequisites
- Windows 10 version 1903 (19H1) or later
- .NET 9.0 Runtime
- ImageGen API key

### Installation
1. Clone the repository
2. Open `ImageGenApp.sln` in Visual Studio
3. Restore NuGet packages
4. Build and run the application

### First Time Setup
1. **Launch the Application**: The app will show an API key warning if no key is configured
2. **Open Settings**: Click the ⚙️ Settings button in the top-right corner
3. **Configure API Key**: Enter your ImageGen API key in the settings panel
4. **Set Preferences**: Choose your default quality, format, and fidelity settings
5. **Save Settings**: Click "Save" to store your configuration

## How to Use

### Basic Image Editing
1. **Upload Primary Image**: Click "📁 Upload Primary Image" to select your base image
2. **Add Additional Images** (Optional): Click "➕ Add Image" to include secondary images
3. **Write Prompt**: Enter a detailed description of what you want the AI to do
4. **Generate**: Click "🚀 Generate" to start the AI processing
5. **Wait**: The app will show a loading indicator during processing (may take up to 60+ seconds)
6. **Download**: Once complete, click "💾 Download" to save your result

### Managing Images
- **Primary Image**: The main image that will be edited by the AI
- **Additional Images**: Secondary images that can be composited with the primary
- **Remove Images**: Click the ❌ button on any image to remove it
- **Clear All**: Use the "🗑️ Clear" button to reset everything

### Settings Options
- **API Key**: Your ImageGen service authentication key
- **Quality**: Standard (faster) vs High (better quality)
- **Format**: JPEG (smaller), PNG (transparent), WebP (modern)
- **Fidelity**: How well input image details are preserved

## Technical Details

### Architecture
- **Frontend**: WinUI 3 with XAML and C#
- **Backend**: .NET 9.0 with Entity Framework Core
- **Database**: SQLite for local settings storage
- **API Integration**: ImageGen NuGet package for AI services

### Key Components
- `MainPage.xaml/cs`: Main application interface with inline settings panel
- `SettingsService.cs`: Database operations and settings management
- `AppDbContext.cs`: SQLite database context
- `AppSettings.cs`: Settings data model

### Dependencies
- Microsoft.WindowsAppSDK
- ImageGenAi
- Microsoft.EntityFrameworkCore.Sqlite
- CommunityToolkit.WinUI.UI
- CommunityToolkit.Mvvm

## Troubleshooting

### Common Issues
- **"API Client Error"**: Ensure your API key is correctly configured in settings
- **"Input Error"**: Make sure you've uploaded a primary image and entered a prompt
- **Long Loading Times**: AI processing can take 30-120 seconds depending on complexity
- **Image Not Loading**: Ensure your image files are valid and not corrupted

### API Key Setup
1. Obtain an API key from your ImageGen service provider
2. Open the settings panel (⚙️ button)
3. Enter your API key in the "API Configuration" section
4. Click "Save" to store the key securely

## Contributing

This application follows modern .NET development practices:
- Async/await for all I/O operations
- Dependency injection for services
- Proper error handling and logging
- MVVM pattern with data binding
- Clean architecture principles

## License

This project is part of the ImageGen ecosystem. See the main project license for details.

---

**Built with ❤️ using WinUI 3 and .NET 9**
