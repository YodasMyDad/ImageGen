using ImageGenApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;

namespace ImageGenApp.Services;

public interface IThemeService
{
    ApplicationTheme CurrentTheme { get; }
    Task InitializeAsync();
    Task SetThemeAsync(string theme);
    event EventHandler<ApplicationTheme>? ThemeChanged;
}

public class ThemeService(IDbContextFactory<AppDbContext> dbContextFactory) : IThemeService
{
    private ApplicationTheme _currentTheme = ApplicationTheme.Light;
    
    public ApplicationTheme CurrentTheme => _currentTheme;
    
    public event EventHandler<ApplicationTheme>? ThemeChanged;

    public async Task InitializeAsync()
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var settings = await dbContext.Settings.FirstOrDefaultAsync();
        
        if (settings?.Theme != null)
        {
            await SetThemeInternalAsync(settings.Theme);
        }
        else
        {
            // Default to system theme if no setting exists
            await SetThemeInternalAsync("Default");
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        await SetThemeInternalAsync(theme);
        
        // Save to database
        using var dbContext = dbContextFactory.CreateDbContext();
        var settings = await dbContext.Settings.FirstOrDefaultAsync() ?? new AppSettings();
        settings.Theme = theme;
        settings.UpdatedAt = DateTime.UtcNow;
        
        if (settings.Id == 0)
        {
            dbContext.Settings.Add(settings);
        }
        else
        {
            dbContext.Settings.Update(settings);
        }
        
        await dbContext.SaveChangesAsync();
    }

    private async Task SetThemeInternalAsync(string theme)
    {
        var elementTheme = theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default // Use system theme for "Default"
        };

        var applicationTheme = theme switch
        {
            "Light" => ApplicationTheme.Light,
            "Dark" => ApplicationTheme.Dark,
            _ => ApplicationTheme.Light // Fallback for tracking
        };

        _currentTheme = applicationTheme;
        
        // Apply theme to the current window content
        if (App.Window?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = elementTheme;
        }
        
        ThemeChanged?.Invoke(this, applicationTheme);
        
        await Task.CompletedTask;
    }
}
