namespace Yatta.App;

using System.Globalization;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yatta.App.Services;
using Yatta.App.ViewModels;
using Yatta.App.Views.Pages;
using Yatta.Core.Interfaces;
using Yatta.Core.Services;
using Yatta.Data;
using Yatta.Data.Repositories;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // When launched automatically at Windows login (--autostart flag), the user profile
        // may not be fully initialized yet. A short delay ensures LocalApplicationData and
        // the database file are accessible before proceeding.
        if (e.Args.Contains("--autostart"))
        {
            Thread.Sleep(3000);
        }

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Apply database migrations and pending schema updates
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<YattaDbContext>();

                // Log pending migrations before applying
                var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Count != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Applying {pendingMigrations.Count} pending migrations:");
                    foreach (var migration in pendingMigrations)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {migration}");
                    }
                }

                dbContext.Database.Migrate();

                // Verify migration was applied
                var appliedMigrations = dbContext.Database.GetAppliedMigrations().ToList();
                System.Diagnostics.Debug.WriteLine($"Total applied migrations: {appliedMigrations.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Database migration failed:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Migration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        // IMPORTANT: Initialize localization BEFORE creating the MainWindow
        // This ensures that XAML resources are evaluated with the correct culture
        InitializeLocalization();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();


        // Load and apply theme after creating the main window
        // This ensures that the system theme is detected correctly
        var themeService = _serviceProvider.GetRequiredService<ThemeService>();
        mainWindow.Loaded += async (_, _) =>
        {
            await themeService.LoadThemeAsync();
            await CheckForUpdatesAsync();
        };

        // Initialize global hotkey service AFTER window handle is created
        mainWindow.SourceInitialized += (_, _) => InitializeGlobalHotkey(mainWindow);

        // Initialize notification service
        InitializeNotificationService();

        // Close stale activities from previous days (data operation only, before window shows)
        var staleResult = CloseStaleActivitiesSync();

        // Execute automatic purge if enabled
        ExecuteAutoPurge();

        // Start the timer automatically with the previous day's last activity if configured
        StartTimerOnStartup();

        mainWindow.Show();

        // Show stale activity notification after the window is visible
        if (staleResult != null)
        {
            mainWindow.Loaded += async (_, _) =>
            {
                var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = Yatta.App.Resources.Resources.TitleBar_Title,
                    Content = string.Format(Yatta.App.Resources.Resources.StaleActivity_AutoClosed, staleResult.Date, staleResult.EndTime)
                };
                _ = await uiMessageBox.ShowDialogAsync();
            };
        }
    }

    /// <summary>
    /// Initializes the global hotkey service with saved settings.
    /// </summary>
    private void InitializeGlobalHotkey(MainWindow mainWindow)
    {
        var hotkeyService = _serviceProvider!.GetRequiredService<IGlobalHotkeyService>();

        // Initialize the service with the main window handle
        if (hotkeyService is GlobalHotkeyService service)
        {
            service.Initialize(mainWindow);
        }

        // Connect event to show the change activity dialog
        hotkeyService.HotkeyPressed += OnGlobalHotkeyPressed;

        // Load settings and register the hotkey
        using var scope = _serviceProvider!.CreateScope();
        var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();

        try
        {
            var settings = settingsRepository.GetAsync().GetAwaiter().GetResult();
            var hotkeyString = settings.GlobalHotkey ?? hotkeyService.GetDefaultHotkey();
            hotkeyService.RegisterHotkey(hotkeyString);
        }
        catch
        {
            // If there's an error, try to register the default hotkey
            hotkeyService.RegisterHotkey(hotkeyService.GetDefaultHotkey());
        }
    }

    /// <summary>
    /// Handles the global hotkey pressed event.
    /// Shows the change activity dialog.
    /// </summary>
    private void OnGlobalHotkeyPressed(object? sender, EventArgs e)
    {
        Current.Dispatcher.Invoke(() =>
        {
            if (Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ShowChangeActivityDialog();
            }
        });
    }

    /// <summary>
    /// Initializes the notification service with saved settings.
    /// </summary>
    private void InitializeNotificationService()
    {
        var notificationService = _serviceProvider!.GetRequiredService<INotificationService>();

        // Connect event to bring window to foreground and navigate to edit record
        notificationService.OnChangeActivity += OnNotificationChangeActivity;

        // Load settings and enable if notifications are enabled
        using var scope = _serviceProvider!.CreateScope();
        var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();

        try
        {
            var settings = settingsRepository.GetAsync().GetAwaiter().GetResult();
            notificationService.IsEnabled = settings.Notifications;
        }
        catch
        {
            // If there's an error, leave notifications disabled
        }
    }

    /// <summary>
    /// Handles the notification change activity event.
    /// Brings window to foreground and shows the change activity dialog.
    /// </summary>
    private void OnNotificationChangeActivity(object? sender, Guid recordId)
    {
        Current.Dispatcher.Invoke(() =>
        {
            var mainWindow = Current.MainWindow as MainWindow;
            mainWindow?.ShowChangeActivityDialog();
        });
    }

    /// <summary>
    /// Closes stale activities from previous days.
    /// Returns the result if a stale record was found and closed, otherwise null.
    /// Uses Task.Run so that EF Core async operations run off the UI thread's
    /// SynchronizationContext, avoiding a potential deadlock from sync-over-async.
    /// </summary>
    private StaleActivityResult? CloseStaleActivitiesSync()
    {
        using var scope = _serviceProvider!.CreateScope();
        var staleActivityService = scope.ServiceProvider.GetRequiredService<IStaleActivityService>();
        return Task.Run(() => staleActivityService.CloseStaleActivitiesAsync()).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes automatic data purge if enabled in settings.
    /// </summary>
    private void ExecuteAutoPurge()
    {
        using var scope = _serviceProvider!.CreateScope();
        var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
        var dataPurgeService = scope.ServiceProvider.GetRequiredService<IDataPurgeService>();

        try
        {
            var settings = settingsRepository.GetAsync().GetAwaiter().GetResult();

            if (settings.RetentionPolicy == Core.Models.RetentionPolicy.Forever)
                return;

            var cutoffDate = dataPurgeService.CalculateCutoffDate(
                settings.RetentionPolicy, settings.CustomRetentionDays);

            if (!cutoffDate.HasValue)
                return;

            dataPurgeService.ExecutePurgeAsync(cutoffDate.Value).GetAwaiter().GetResult();
        }
        catch
        {
            // If there's an error during auto-purge, continue startup normally
        }
    }

    /// <summary>
    /// Starts the timer automatically on application startup when enabled in settings.
    /// </summary>
    private void StartTimerOnStartup()
    {
        using var scope = _serviceProvider!.CreateScope();
        var autoStartActivityService = scope.ServiceProvider.GetRequiredService<IAutoStartActivityService>();

        try
        {
            autoStartActivityService.TryStartPreviousDayActivityAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // If there's an error during auto-start, continue startup normally
        }
    }

    /// <summary>
    /// Checks for application updates in the background.
    /// Shows a dialog if a new version is available.
    /// </summary>
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateService = _serviceProvider!.GetRequiredService<IUpdateService>();
            if (!updateService.IsInstalled)
                return;

            var hasUpdate = await updateService.IsUpdateAvailableAsync();
            if (!hasUpdate)
                return;

            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = Yatta.App.Resources.Resources.TitleBar_Title,
                Content = Yatta.App.Resources.Resources.Update_Available,
                PrimaryButtonText = Yatta.App.Resources.Resources.Update_InstallAndRestart,
                SecondaryButtonText = Yatta.App.Resources.Resources.Update_Later,
            };

            var result = await messageBox.ShowDialogAsync();
            if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                await updateService.ApplyUpdateAndRestartAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Update check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Inicialitza la localització carregant l'idioma guardat de la base de dades.
    /// S'ha de cridar ABANS de crear qualsevol finestra o pàgina.
    /// </summary>
    private void InitializeLocalization()
    {
        using var scope = _serviceProvider!.CreateScope();
        var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();

        try
        {
            var settings = settingsRepository.GetAsync().GetAwaiter().GetResult();
            CultureInfo culture;

            if (settings != null && !string.IsNullOrEmpty(settings.Language))
            {
                // Usar l'idioma guardat
                culture = new CultureInfo(settings.Language);
            }
            else
            {
                // Usar idioma del sistema (si és es o ca), sinó espanyol per defecte
                var systemCulture = CultureInfo.CurrentUICulture;
                culture = (systemCulture.Name.StartsWith("es") || systemCulture.Name.StartsWith("ca"))
                    ? systemCulture
                    : new CultureInfo("es-ES");
            }

            // Aplicar la cultura a tot el sistema
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Yatta.App.Resources.Resources.Culture = culture;

            // Actualitzar el LocalizationService amb la cultura correcta
            if (_serviceProvider != null)
            {
                var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
                localizationService.SetCulture(culture.Name);
            }
        }
        catch
        {
            // Si hi ha error, usar espanyol per defecte
            var defaultCulture = new CultureInfo("es-ES");
            CultureInfo.CurrentUICulture = defaultCulture;
            CultureInfo.CurrentCulture = defaultCulture;
            Yatta.App.Resources.Resources.Culture = defaultCulture;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register DbContext
        services.AddDbContext<YattaDbContext>(options =>
            options.UseSqlite(DatabaseConfiguration.GetConnectionString()));

        // Register repositories
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ITimeRecordRepository, TimeRecordRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IWorkdayRepository, WorkdayRepository>();
        services.AddScoped<IAnnualQuotaRepository, AnnualQuotaRepository>();

        // Register services
        services.AddScoped<ITimeCalculatorService, TimeCalculatorService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IWorkdayService, WorkdayService>();
        services.AddScoped<IWorkdayConfigService, WorkdayConfigService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDataPurgeService, DataPurgeService>();
        services.AddScoped<IStaleActivityService, StaleActivityService>();
        services.AddScoped<IAutoStartActivityService, AutoStartActivityService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IBreadcrumbService, BreadcrumbService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<IPageStateService, PageStateService>();
        services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();
        services.AddSingleton<IUpdateService, UpdateService>();

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<TodayViewModel>();
        services.AddTransient<HistoricViewModel>();
        services.AddTransient<ActivitiesViewModel>();
        services.AddTransient<ActivityDetailViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<WhatsNewViewModel>();
        services.AddTransient<DashboardIndexViewModel>();
        services.AddTransient<DashboardDayViewModel>();
        services.AddTransient<DashboardWeekViewModel>();
        services.AddTransient<DashboardMonthViewModel>();
        services.AddTransient<DashboardYearViewModel>();
        services.AddTransient<TrayPanelViewModel>();

        // Register Pages
        services.AddTransient<TodayPage>();
        services.AddTransient<HistoricPage>();
        services.AddTransient<ActivitiesPage>();
        services.AddTransient<ActivityDetailPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<WhatsNewPage>();
        services.AddTransient<DashboardIndexPage>();
        services.AddTransient<DashboardDayPage>();
        services.AddTransient<DashboardWeekPage>();
        services.AddTransient<DashboardMonthPage>();
        services.AddTransient<DashboardYearPage>();

        // Register windows
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            CleanupNotificationService();
            CleanupGlobalHotkeyService();
        }
        catch
        {
            // Ignore cleanup errors
        }

        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// Cleans up the global hotkey service by unregistering hotkeys and disconnecting events.
    /// </summary>
    private void CleanupGlobalHotkeyService()
    {
        var hotkeyService = _serviceProvider?.GetService<IGlobalHotkeyService>();
        if (hotkeyService != null)
        {
            hotkeyService.HotkeyPressed -= OnGlobalHotkeyPressed;
            hotkeyService.UnregisterHotkey();
            if (hotkeyService is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Releases resources used by the notification service, unsubscribes from activity change events, and disposes of the service instance.
    /// </summary>
    private void CleanupNotificationService()
    {
        var notificationService = _serviceProvider?.GetService<INotificationService>();
        notificationService?.OnChangeActivity -= OnNotificationChangeActivity;
        notificationService?.Dispose();
    }
}
