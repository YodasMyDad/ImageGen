using ImageGenApp.Models;
using ImageGen.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageGenApp.Services;

public class SettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly AppDbContext _context;

    public SettingsService(AppDbContext context, ILogger<SettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        try
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new AppSettings();
                _context.Settings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings");
            return new AppSettings();
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            settings.UpdatedAt = DateTime.UtcNow;
            _context.Settings.Update(settings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            throw;
        }
    }

    public async Task<string?> GetApiKeyAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.ApiKey;
    }

    public async Task SaveApiKeyAsync(string apiKey)
    {
        var settings = await GetSettingsAsync();
        settings.ApiKey = apiKey;
        await SaveSettingsAsync(settings);
    }

    public async Task<ImageQuality> GetDefaultQualityAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.DefaultQuality;
    }

    public async Task<ImageFormat> GetDefaultFormatAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.DefaultFormat;
    }

    public async Task<InputFidelity> GetDefaultFidelityAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.DefaultFidelity;
    }

    public async Task UpdateDefaultSettingsAsync(ImageQuality quality, ImageFormat format, InputFidelity fidelity)
    {
        var settings = await GetSettingsAsync();
        settings.DefaultQuality = quality;
        settings.DefaultFormat = format;
        settings.DefaultFidelity = fidelity;
        await SaveSettingsAsync(settings);
    }
}
