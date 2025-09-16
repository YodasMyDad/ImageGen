using ImageGen.Models;
using ImageGenApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

namespace ImageGenApp.Dialogs;

public sealed partial class SettingsDialog : ContentDialog
{
    private readonly SettingsService _settingsService;
    private readonly ILogger _logger;

    public SettingsDialog(SettingsService settingsService, ILogger logger, Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        this.InitializeComponent();
        _settingsService = settingsService;
        _logger = logger;
        this.XamlRoot = xamlRoot;

        // Load current settings asynchronously
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await LoadSettingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings in dialog");
            await ShowErrorDialog("Load Error", $"Failed to load settings: {ex.Message}");
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();

            // Load API key
            ApiKeyTextBox.Text = settings.ApiKey ?? string.Empty;

            // Load quality setting
            QualityComboBox.SelectedIndex = settings.DefaultQuality switch
            {
                ImageQuality.Standard => 0,
                ImageQuality.High => 1,
                _ => 0
            };

            // Load format setting
            FormatComboBox.SelectedIndex = settings.DefaultFormat switch
            {
                ImageFormat.Jpeg => 0,
                ImageFormat.Png => 1,
                ImageFormat.Webp => 2,
                _ => 0
            };

            // Load fidelity setting
            FidelityComboBox.SelectedIndex = settings.DefaultFidelity switch
            {
                InputFidelity.Default => 0,
                InputFidelity.High => 1,
                _ => 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings");
            await ShowErrorDialog("Load Error", $"Failed to load settings: {ex.Message}");
        }
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();

            // Save API key
            settings.ApiKey = ApiKeyTextBox.Text.Trim();

            // Save quality setting
            settings.DefaultQuality = QualityComboBox.SelectedIndex switch
            {
                0 => ImageQuality.Standard,
                1 => ImageQuality.High,
                _ => ImageQuality.High
            };

            // Save format setting
            settings.DefaultFormat = FormatComboBox.SelectedIndex switch
            {
                0 => ImageFormat.Jpeg,
                1 => ImageFormat.Png,
                2 => ImageFormat.Webp,
                _ => ImageFormat.Jpeg
            };

            // Save fidelity setting
            settings.DefaultFidelity = FidelityComboBox.SelectedIndex switch
            {
                0 => InputFidelity.Default,
                1 => InputFidelity.High,
                _ => InputFidelity.High
            };

            await _settingsService.SaveSettingsAsync(settings);

            _logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            args.Cancel = true; // Prevent dialog from closing
            await ShowErrorDialog("Save Error", $"Failed to save settings: {ex.Message}");
        }
    }

    private async Task ShowErrorDialog(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
