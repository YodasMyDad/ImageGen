using ImageGen.Core;
using ImageGen.Models;
using ImageGen.Configuration;
using ImageGenApp.Models;
using ImageGenApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Media;

namespace ImageGenApp.Views
{
    /// <summary>
    /// Modern main page for AI Image Generation with WinUI 3.
    /// Implements theme-aware UI with simplified, maintainable code.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IImageGenClient? _imageGenClient;
        private ILogger<ImageGenClient>? _logger;
        private HttpClient? _httpClient;
        private SettingsService? _settingsService;
        private PromptTemplateService? _promptTemplateService;

        // UI State
        private string? _primaryImagePath;
        private ImageResult? _currentResult;
        private readonly ObservableCollection<AdditionalImage> _additionalImages = new();

        // Panel state
        private AppSettings? _originalSettings;

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
                _promptTemplateService = new PromptTemplateService();

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
                if (_settingsService == null)
                {
                    await ShowErrorDialog("Error", "Application is not fully initialized. Please restart the application.");
                    return;
                }

                // Load current settings
                var settings = await _settingsService.GetSettingsAsync();
                _originalSettings = new AppSettings
                {
                    ApiKey = settings.ApiKey,
                    DefaultQuality = settings.DefaultQuality,
                    DefaultFormat = settings.DefaultFormat,
                    DefaultFidelity = settings.DefaultFidelity
                };

                // Populate UI
                ApiKeyTextBox.Text = settings.ApiKey ?? string.Empty;
                
                // Set combo box selections
                SetComboBoxSelection(QualityComboBox, settings.DefaultQuality.ToString());
                SetComboBoxSelection(FormatComboBox, settings.DefaultFormat.ToString());
                SetComboBoxSelection(FidelityComboBox, settings.DefaultFidelity.ToString());

                // Show panel
                SettingsPanelOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Settings Error", $"Failed to open settings: {ex.Message}");
                _logger?.LogError(ex, "Error opening settings panel");
            }
        }

        private async void OnPromptTemplatesClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_promptTemplateService == null)
                {
                    await ShowErrorDialog("Error", "Application is not fully initialized. Please restart the application.");
                    return;
                }

                // Populate templates
                _ = PopulatePromptTemplates();

                // Show panel
                PromptTemplatesPanelOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Templates Error", $"Failed to open prompt templates: {ex.Message}");
                _logger?.LogError(ex, "Error opening prompt templates panel");
            }
        }

        private void OnSettingsPanelOverlayTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            SettingsPanelOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnPromptTemplatesPanelOverlayTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            PromptTemplatesPanelOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnCloseSettingsPanelClicked(object sender, RoutedEventArgs e)
        {
            SettingsPanelOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnClosePromptTemplatesPanelClicked(object sender, RoutedEventArgs e)
        {
            PromptTemplatesPanelOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnCancelSettingsClicked(object sender, RoutedEventArgs e)
        {
            SettingsPanelOverlay.Visibility = Visibility.Collapsed;
        }

        private async void OnSaveSettingsClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_settingsService == null)
                {
                    await ShowErrorDialog("Error", "Settings service is not available.");
                    return;
                }

                // Get values from UI
                var apiKey = ApiKeyTextBox.Text.Trim();
                var quality = GetComboBoxSelection<ImageQuality>(QualityComboBox);
                var format = GetComboBoxSelection<ImageFormat>(FormatComboBox);
                var fidelity = GetComboBoxSelection<InputFidelity>(FidelityComboBox);

                // Save settings
                await _settingsService.SaveApiKeyAsync(apiKey);
                var settings = await _settingsService.GetSettingsAsync();
                settings.DefaultQuality = quality;
                settings.DefaultFormat = format;
                settings.DefaultFidelity = fidelity;
                await _settingsService.SaveSettingsAsync(settings);

                // Reinitialize client with new settings
                await InitializeImageGenClient();

                // Hide API key warning if key is now set
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    ApiKeyWarning.Visibility = Visibility.Collapsed;
                }

                // Close panel
                SettingsPanelOverlay.Visibility = Visibility.Collapsed;

                await ShowInfoDialog("Settings Saved", "Your settings have been saved successfully!");
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Save Error", $"Failed to save settings: {ex.Message}");
                _logger?.LogError(ex, "Error saving settings");
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
                await ShowErrorDialog("Input Error", "Please add a primary image first.");
                return;
            }

            await GenerateImageAsync();
        }

        private async Task GenerateImageAsync()
        {
            SetLoadingState(true);

            try
            {
                var request = await CreateEditRequest();
                var result = await _imageGenClient!.EditAsync(request, CancellationToken.None);
                
                UpdateResultUI(result);
                DisposeRequest(request);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Generation Error", $"Failed to generate image: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            GenerateButton.IsEnabled = !isLoading;
            
            if (isLoading)
            {
                LoadingText.Text = "Please wait, this could take 60+ seconds";
            }
        }

        private async Task<EditRequest> CreateEditRequest()
        {
            var settings = await _settingsService!.GetSettingsAsync();
            var primaryStream = File.OpenRead(_primaryImagePath!);

            return new EditRequest(primaryStream, PromptTextBox.Text.Trim())
            {
                Quality = settings.DefaultQuality,
                Format = settings.DefaultFormat,
                InputFidelity = settings.DefaultFidelity,
                SecondaryImages = _additionalImages.Any()
                    ? _additionalImages.Select(img => File.OpenRead(img.FilePath)).ToArray()
                    : null
            };
        }

        private void UpdateResultUI(ImageResult result)
        {
            _currentResult = result;

            DispatcherQueue.TryEnqueue(() =>
            {
                ResultPlaceholder.Visibility = Visibility.Collapsed;
                
                var bitmapImage = new BitmapImage();
                using var stream = new MemoryStream(result.Bytes.ToArray());
                bitmapImage.SetSource(stream.AsRandomAccessStream());
                ResultImage.Source = bitmapImage;

                DownloadButton.IsEnabled = true;
            });
        }

        private static void DisposeRequest(EditRequest request)
        {
            request.PrimaryImage?.Dispose();
            if (request.SecondaryImages != null)
            {
                foreach (var stream in request.SecondaryImages)
                {
                    stream?.Dispose();
                }
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
            await ShowDialog(title, message, "❌");
        }

        private async Task ShowInfoDialog(string title, string message)
        {
            await ShowDialog(title, message, "✅");
        }
        
        private async Task ShowDialog(string title, string message, string icon)
        {
            var dialog = new ContentDialog
            {
                Title = $"{icon} {title}",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
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
            
            _additionalImages.Clear();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupResources();
        }

        // Helper methods for slide-out panels
        private void SetComboBoxSelection(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Tag?.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private T GetComboBoxSelection<T>(ComboBox comboBox) where T : struct, Enum
        {
            if (comboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                if (Enum.TryParse<T>(tag, out var result))
                {
                    return result;
                }
            }
            return default;
        }

        private Task PopulatePromptTemplates()
        {
            if (_promptTemplateService == null) return Task.CompletedTask;

            TemplatesContainer.Children.Clear();

            var searchTerm = TemplateSearchBox.Text?.Trim();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                PopulateCategorizedTemplates();
            }
            else
            {
                PopulateSearchResults(searchTerm);
            }

            return Task.CompletedTask;
        }

        private void PopulateCategorizedTemplates()
        {
            var categories = _promptTemplateService!.GetCategories();
            foreach (var category in categories.Where(c => c.Templates.Any()))
            {
                AddCategoryHeader(category.Name);
                foreach (var template in category.Templates)
                {
                    TemplatesContainer.Children.Add(CreateTemplateControl(template));
                }
            }
        }

        private void PopulateSearchResults(string searchTerm)
        {
            var templates = _promptTemplateService!.SearchTemplates(searchTerm).ToList();
            
            if (templates.Any())
            {
                AddSearchHeader(templates.Count);
                foreach (var template in templates)
                {
                    TemplatesContainer.Children.Add(CreateTemplateControl(template));
                }
            }
            else
            {
                AddNoResultsMessage();
            }
        }

        private void AddCategoryHeader(string categoryName)
        {
            var categoryHeader = new TextBlock
            {
                Text = categoryName,
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 5),
                Foreground = GetThemeResource<Brush>("TextFillColorPrimaryBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.Black)
            };
            TemplatesContainer.Children.Add(categoryHeader);
        }

        private void AddSearchHeader(int count)
        {
            var searchHeader = new TextBlock
            {
                Text = $"Search Results ({count})",
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = GetThemeResource<Brush>("TextFillColorPrimaryBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.Black)
            };
            TemplatesContainer.Children.Add(searchHeader);
        }

        private void AddNoResultsMessage()
        {
            var noResults = new TextBlock
            {
                Text = "No templates found matching your search.",
                FontSize = 12,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0),
                Foreground = GetThemeResource<Brush>("TextFillColorSecondaryBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
            TemplatesContainer.Children.Add(noResults);
        }

        private Border CreateTemplateControl(PromptTemplate template)
        {
            var border = new Border
            {
                BorderBrush = GetThemeResource<Brush>("CardStrokeColorDefaultBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.Gray),
                BorderThickness = new Thickness(1),
                Background = GetThemeResource<Brush>("CardBackgroundFillColorDefaultBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.White),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 8, 0, 0)
            };
            
            border.Tapped += async (s, e) => await OnTemplateClicked(template);

            var stackPanel = new StackPanel { Spacing = 10 };

            // Title
            var titleBlock = new TextBlock 
            { 
                Text = template.Title,
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = GetThemeResource<Brush>("TextFillColorPrimaryBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.Black)
            };
            stackPanel.Children.Add(titleBlock);

            // Description
            if (!string.IsNullOrEmpty(template.Description))
            {
                var descriptionBlock = new TextBlock
                {
                    Text = template.Description,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, -5, 0, 0),
                    Foreground = GetThemeResource<Brush>("TextFillColorSecondaryBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
                stackPanel.Children.Add(descriptionBlock);
            }

            // Full prompt text with background
            var promptBorder = new Border
            {
                Background = GetThemeResource<Brush>("SystemControlAcrylicElementBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8)
            };
            
            var promptBlock = new TextBlock
            {
                Text = template.Prompt,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18,
                Foreground = GetThemeResource<Brush>("TextFillColorSecondaryBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
            promptBorder.Child = promptBlock;
            stackPanel.Children.Add(promptBorder);

            // Click instruction
            var instructionBlock = new TextBlock
            {
                Text = "💡 Click to use this prompt",
                FontSize = 11,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, -3, 0, 0),
                Foreground = GetThemeResource<Brush>("TextFillColorSecondaryBrush") ?? new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
            stackPanel.Children.Add(instructionBlock);

            border.Child = stackPanel;
            return border;
        }
        
        private T? GetThemeResource<T>(string resourceKey) where T : class
        {
            try
            {
                return Application.Current.Resources[resourceKey] as T;
            }
            catch
            {
                return null;
            }
        }

        private async Task OnTemplateClicked(PromptTemplate template)
        {
            try
            {
                CopyToClipboard(template.Prompt);
                PromptTextBox.Text = template.Prompt;
                PromptTemplatesPanelOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Template Error", $"Failed to apply template: {ex.Message}");
            }
        }
        
        private static void CopyToClipboard(string text)
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }

        private void OnTemplateSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _ = PopulatePromptTemplates();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching prompt templates");
            }
        }
    }
}
