using ImageGen.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure ImageGen client
builder.Services.AddImageGenClient(options =>
{
    var apiKey = builder.Configuration["ImageGen:ApiKey"] ??
        throw new InvalidOperationException("ImageGen API key not found in configuration");

    if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "your-openai-api-key-here")
    {
        throw new InvalidOperationException("ImageGen API key is not configured. Please set a valid OpenAI API key in appsettings.json or appsettings.development.json");
    }

    options.ApiKey = apiKey;
    options.RequestTimeout = TimeSpan.FromMinutes(3);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files to serve dynamically generated images
app.UseStaticFiles();

// Enable directory browsing for images folder (optional, for debugging)
if (app.Environment.IsDevelopment())
{
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
            Path.Combine(app.Environment.WebRootPath, "images")),
        RequestPath = "/images"
    });
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
