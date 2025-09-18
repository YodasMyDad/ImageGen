using ImageGenApp.Models;
using ImageGenApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Navigation;
using System.IO;
using System.Linq;

namespace ImageGenApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = default!;
        private readonly IHost _host;
        public static Window? Window { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                Console.WriteLine("Starting App constructor...");
                this.InitializeComponent();
                Console.WriteLine("InitializeComponent completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InitializeComponent failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
#if DEBUG
                        builder.SetMinimumLevel(LogLevel.Debug);
#else
                        builder.SetMinimumLevel(LogLevel.Information);
#endif
                    });

                    services.AddHttpClient();

                    var dbPath = GetDatabasePath();
                    services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

                    services.AddSingleton<SettingsService>();
                    services.AddSingleton<IThemeService, ThemeService>();
                })
                .Build();

            Services = _host.Services;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Console.WriteLine("OnLaunched called");
            
            try
            {
                // Apply EF Core migrations / create DB
                Console.WriteLine("Starting EF Core migrations...");
                using (var scope = Services.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                using var dbContext = factory.CreateDbContext();
                var hasMigrations = dbContext.Database.GetPendingMigrations().Any();
                if (hasMigrations)
                {
                    dbContext.Database.Migrate();
                }
            }

            // Initialize theme service
            var themeService = Services.GetRequiredService<IThemeService>();
            _ = Task.Run(async () => await themeService.InitializeAsync());

            Window ??= new Window();
            Window.Title = "ImageGen - AI Image Editing";
            Console.WriteLine("Window created");

            if (Window.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Content = rootFrame;
                Console.WriteLine("Frame created");
            }

            Console.WriteLine("Navigating to MainPage...");
            _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);
            Console.WriteLine("Navigation completed");

            Console.WriteLine("Activating window...");
            Window.Activate();
            Console.WriteLine("Window activated - OnLaunched finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnLaunched failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private static string GetDatabasePath()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ImageGenApp", "settings.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            return dbPath;
        }
    }
}
