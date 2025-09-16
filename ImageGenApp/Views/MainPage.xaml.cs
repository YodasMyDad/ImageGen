using ImageGen.Core;
using ImageGen.Models;
using ImageGen.Configuration;
using ImageGenApp.Models;
using ImageGenApp.Services;
using ImageGenApp.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ImageGenApp.Views
{
    /// <summary>
    /// Modern main page for AI Image Generation with WinUI 3.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IImageGenClient? _imageGenClient;
        private ILogger<ImageGenClient>? _logger;
        private HttpClient? _httpClient;
        private SettingsService? _settingsService;

        // UI State
        private string? _primaryImagePath;
        private ImageResult? _currentResult;
        private readonly ObservableCollection<AdditionalImage> _additionalImages = new();

        public MainPage()
        {
            this.InitializeComponent();
            AdditionalImagesList.ItemsSource = _additionalImages;

            // Initialize everything asynchronously
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Resolve dependencies from DI
                var services = App.Services;
                var httpFactory = services.GetRequiredService<IHttpClientFactory>();
                _httpClient = httpFactory.CreateClient();

                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                _logger = loggerFactory.CreateLogger<ImageGenClient>();

                _settingsService = services.GetRequiredService<SettingsService>();

                // Ensure default settings exist
                await _settingsService.GetSettingsAsync();

                // Initialize the API client
                await InitializeImageGenClient();

                // Check API key and show warning if needed
                var apiKey = await _settingsService.GetApiKeyAsync();
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    DispatcherQueue.TryEnqueue(() => ApiKeyWarning.Visibility = Visibility.Visible);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Initialization Error", $"Failed to initialize the application: {ex.Message}");
            }
        }

        private async Task InitializeImageGenClient()
        {
            var apiKey = await _settingsService!.GetApiKeyAsync();
            if (!string.IsNullOrEmpty(apiKey))
            {
                var options = new ImageGenOptions
                {
                    ApiKey = apiKey,
                    RequestTimeout = TimeSpan.FromMinutes(5) // Longer timeout for image generation
                };

                // Ensure HttpClient is configured for relative API endpoints
                _httpClient!.BaseAddress = options.BaseUrl;
                _httpClient.Timeout = options.RequestTimeout;

                _imageGenClient = new ImageGenClient(_httpClient, options, _logger!);
            }
        }

        private async void OnSettingsClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_settingsService == null || _logger == null)
                {
                    await ShowErrorDialog("Error", "Application is not fully initialized. Please restart the application.");
                    return;
                }

                var settingsDialog = new SettingsDialog(_settingsService, _logger, this.XamlRoot);
                var result = await settingsDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // Reinitialize client with new settings
                    await InitializeImageGenClient();

                    // Hide API key warning if key is now set
                    var apiKey = await _settingsService.GetApiKeyAsync();
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        ApiKeyWarning.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Settings Error", $"Failed to open settings: {ex.Message}");
                _logger?.LogError(ex, "Error opening settings dialog");
            }
        }

        private async void OnUploadPrimaryClicked(object sender, RoutedEventArgs e)
        {
            var file = await PickImageFileAsync();
            if (file != null)
            {
                _primaryImagePath = file.Path;
                await LoadImageToUI(PrimaryImage, file.Path);
                UploadPrimaryButton.Visibility = Visibility.Collapsed;
                RemovePrimaryButton.Visibility = Visibility.Visible;
            }
        }

        private void OnRemovePrimaryClicked(object sender, RoutedEventArgs e)
        {
            _primaryImagePath = null;
            PrimaryImage.Source = null;
            UploadPrimaryButton.Visibility = Visibility.Visible;
            RemovePrimaryButton.Visibility = Visibility.Collapsed;
        }

        private async void OnAddImageClicked(object sender, RoutedEventArgs e)
        {
            var file = await PickImageFileAsync();
            if (file != null)
            {
                var additionalImage = new AdditionalImage { FilePath = file.Path };
                await LoadImageToUI(additionalImage, file.Path);
                _additionalImages.Add(additionalImage);
            }
        }

        private void OnRemoveAdditionalClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is AdditionalImage image)
            {
                _additionalImages.Remove(image);
            }
        }

        private async void OnGenerateClicked(object sender, RoutedEventArgs e)
        {
            if (_imageGenClient == null)
            {
                await ShowErrorDialog("API Client Error", "Please configure your API key in settings first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PromptTextBox.Text))
            {
                await ShowErrorDialog("Input Error", "Please enter a prompt for the image generation.");
                return;
            }

            if (string.IsNullOrEmpty(_primaryImagePath))
            {
                await ShowErrorDialog("Input Error", "Please upload a primary image first.");
                return;
            }

            await GenerateImageAsync();
        }

        private async Task GenerateImageAsync()
        {
            try
            {
                // Show loading overlay
                LoadingOverlay.Visibility = Visibility.Visible;
                LoadingText.Text = "Generating image...";
                GenerateButton.IsEnabled = false;

                // Prepare the edit request
                var settings = await _settingsService!.GetSettingsAsync();
                var primaryStream = File.OpenRead(_primaryImagePath!);

                var request = new EditRequest(primaryStream, PromptTextBox.Text.Trim())
                {
                    Quality = settings.DefaultQuality,
                    Format = settings.DefaultFormat,
                    InputFidelity = settings.DefaultFidelity,
                    SecondaryImages = _additionalImages.Any()
                        ? _additionalImages.Select(img => File.OpenRead(img.FilePath)).ToArray()
                        : null
                };

                // Generate the image
                var result = await _imageGenClient!.EditAsync(request, CancellationToken.None);
                _currentResult = result;

                // Update UI with result
                DispatcherQueue.TryEnqueue(() =>
                {
                    // Hide the placeholder overlay
                    ResultPlaceholder.Visibility = Visibility.Collapsed;

                    var bitmapImage = new BitmapImage();
                    using (var stream = new MemoryStream(result.Bytes.ToArray()))
                    {
                        bitmapImage.SetSource(stream.AsRandomAccessStream());
                    }
                    ResultImage.Source = bitmapImage;

                    // Enable action buttons
                    DownloadButton.IsEnabled = true;
                    CopyPromptButton.IsEnabled = true;
                });

                primaryStream.Dispose();
                foreach (var stream in request.SecondaryImages ?? [])
                {
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Generation Error", $"Failed to generate image: {ex.Message}");
            }
            finally
            {
                // Hide loading overlay
                LoadingOverlay.Visibility = Visibility.Collapsed;
                GenerateButton.IsEnabled = true;
            }
        }

        private async void OnDownloadClicked(object sender, RoutedEventArgs e)
        {
            if (_currentResult == null) return;

            try
            {
                var savePicker = new FileSavePicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var extension = _currentResult.Format switch
                {
                    ImageFormat.Jpeg => ".jpg",
                    ImageFormat.Png => ".png",
                    ImageFormat.Webp => ".webp",
                    _ => ".jpg"
                };

                savePicker.SuggestedFileName = $"generated_image_{DateTime.Now:yyyyMMdd_HHmmss}";
                savePicker.FileTypeChoices.Add("Image Files", new[] { extension });

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteBytesAsync(file, _currentResult.Bytes.ToArray());
                    await ShowInfoDialog("Success", "Image saved successfully!");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Save Error", $"Failed to save image: {ex.Message}");
            }
        }

        private async void OnCopyPromptClicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PromptTextBox.Text))
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(PromptTextBox.Text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                await ShowInfoDialog("Success", "Prompt copied to clipboard!");
            }
        }

        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            // Clear all inputs and results
            _primaryImagePath = null;
            _currentResult = null;
            _additionalImages.Clear();

            // Reset UI
            PrimaryImage.Source = null;
            ResultImage.Source = null;
            PromptTextBox.Text = string.Empty;

            UploadPrimaryButton.Visibility = Visibility.Visible;
            RemovePrimaryButton.Visibility = Visibility.Collapsed;
            DownloadButton.IsEnabled = false;
            CopyPromptButton.IsEnabled = false;

            // Show the placeholder overlay again
            ResultPlaceholder.Visibility = Visibility.Visible;
        }

        private async Task<StorageFile?> PickImageFileAsync()
        {
            var picker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");
            picker.FileTypeFilter.Add(".bmp");

            return await picker.PickSingleFileAsync();
        }

        private async Task LoadImageToUI(Image imageControl, string filePath)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);
                using var stream = await file.OpenReadAsync();
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                imageControl.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Image Load Error", $"Failed to load image: {ex.Message}");
            }
        }

        private async Task LoadImageToUI(AdditionalImage additionalImage, string filePath)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);
                using var stream = await file.OpenReadAsync();
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                additionalImage.ImageSource = bitmapImage;
                additionalImage.ImageStream = File.OpenRead(filePath);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Image Load Error", $"Failed to load image: {ex.Message}");
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

        private async Task ShowInfoDialog(string title, string message)
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

        private void CleanupResources()
        {
            _imageGenClient = null;

            foreach (var image in _additionalImages)
            {
                image.ImageStream?.Dispose();
            }
        }

        ~MainPage()
        {
            CleanupResources();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupResources();
        }
    }
}
