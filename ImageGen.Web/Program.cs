using ImageGen.Configuration;
using ImageResize.Core.Extensions;
using Microsoft.Extensions.FileProviders;

// Create web application builder
var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages for the web UI
builder.Services.AddRazorPages();

// Configure ImageResize for image processing and caching
builder.Services.AddImageResize(builder.Environment);

// Configure ImageGen client - this is the main service for AI image operations
builder.Services.AddImageGenClient(options =>
{
    // Get OpenAI API key from configuration
    var apiKey = builder.Configuration["ImageGen:ApiKey"] ??
        throw new InvalidOperationException("ImageGen API key not found in configuration");

    // Validate the API key is configured
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "your-openai-api-key-here")
    {
        throw new InvalidOperationException("Please set a valid OpenAI API key in appsettings.json");
    }

    options.ApiKey = apiKey;
    options.RequestTimeout = TimeSpan.FromMinutes(3); // Allow time for AI processing
});

var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure ImageResize middleware for dynamic image resizing
app.UseImageResize();

// Serve static files (including generated images)
app.UseStaticFiles();

// In development, enable browsing of generated images
if (app.Environment.IsDevelopment())
{
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.WebRootPath, "images")),
        RequestPath = "/images"
    });
}

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
