using ImageGenApp.Models;
using ImageGen.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageGenApp.Services;

public class SettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public SettingsService(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<SettingsService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var settings = await context.Settings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new AppSettings { Id = 1 };
                context.Settings.Add(settings);
                await context.SaveChangesAsync();
                _logger.LogInformation("Created default settings record");
            }
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings, returning default settings");
            return new AppSettings { Id = 1 };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        await _semaphore.WaitAsync();
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            settings.UpdatedAt = DateTime.UtcNow;
            context.Settings.Update(settings);
            await context.SaveChangesAsync();
            _logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            throw;
        }
        finally
        {
            _semaphore.Release();
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
