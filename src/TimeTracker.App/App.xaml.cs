namespace TimeTracker.App;

using System.Globalization;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Services;
using TimeTracker.Data;
using TimeTracker.Data.Repositories;
using TimeTracker.App.ViewModels;
using TimeTracker.App.Views.Pages;
using TimeTracker.App.Services;
using TimeTracker.App.Models;
using Microsoft.Toolkit.Uwp.Notifications;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Apply database migrations and pending schema updates
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TimeTrackerDbContext>();

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
        mainWindow.Loaded += async (_, _) => await themeService.LoadThemeAsync();

        // Initialize global hotkey service AFTER window handle is created
        mainWindow.SourceInitialized += (_, _) => InitializeGlobalHotkey(mainWindow);

        // Initialize notification service
        InitializeNotificationService();

        // Close stale activities from previous days
        CloseStaleActivities().Wait();

        // Execute automatic purge if enabled
        ExecuteAutoPurge();

        mainWindow.Show();
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
            var mainWindow = Current.MainWindow as MainWindow;
            if (mainWindow != null)
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
    /// Brings window to foreground and navigates to edit the active record.
    /// </summary>
    private void OnNotificationChangeActivity(object? sender, Guid recordId)
    {
        Current.Dispatcher.Invoke(() =>
        {
            var mainWindow = Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowInTaskbar = true;
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            }

            var navigationService = _serviceProvider!.GetRequiredService<INavigationService>();
            var navParam = new HistoricNavigationParameter
            {
                RecordId = recordId,
                FromNotification = true
            };
            navigationService.Navigate<HistoricDetailPage>(navParam);
        });
    }

    /// <summary>
    /// Closes stale activities from previous days and shows a notification.
    /// </summary>
    private async Task CloseStaleActivities()
    {
        using var scope = _serviceProvider!.CreateScope();
        var staleActivityService = scope.ServiceProvider.GetRequiredService<IStaleActivityService>();

        var result = staleActivityService.CloseStaleActivitiesAsync().GetAwaiter().GetResult();

        if (result == null)
            return;

        var uiMessageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = TimeTracker.App.Resources.Resources.TitleBar_Title,
            Content = string.Format(TimeTracker.App.Resources.Resources.StaleActivity_AutoClosed, result.Date, result.EndTime)
        };

        _ = await uiMessageBox.ShowDialogAsync();
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
            TimeTracker.App.Resources.Resources.Culture = culture;

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
            TimeTracker.App.Resources.Resources.Culture = defaultCulture;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register DbContext
        services.AddDbContext<TimeTrackerDbContext>(options =>
            options.UseSqlite(DatabaseConfiguration.GetConnectionString()));

        // Register repositories
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ITimeRecordRepository, TimeRecordRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IWorkdayRepository, WorkdayRepository>();

        // Register services
        services.AddScoped<ITimeCalculatorService, TimeCalculatorService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IWorkdayService, WorkdayService>();
        services.AddScoped<IWorkdayConfigService, WorkdayConfigService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDataPurgeService, DataPurgeService>();
        services.AddScoped<IStaleActivityService, StaleActivityService>();
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

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<TodayViewModel>();
        services.AddTransient<HistoricViewModel>();
        services.AddTransient<HistoricDetailViewModel>();
        services.AddTransient<JornadaViewModel>();
        services.AddTransient<ActivitiesViewModel>();
        services.AddTransient<ActivityDetailViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<WhatsNewViewModel>();
        services.AddTransient<DashboardIndexViewModel>();
        services.AddTransient<DashboardDayViewModel>();
        services.AddTransient<DashboardWeekViewModel>();
        services.AddTransient<DashboardMonthViewModel>();
        services.AddTransient<DashboardYearViewModel>();

        // Register Pages
        services.AddTransient<TodayPage>();
        services.AddTransient<HistoricPage>();
        services.AddTransient<HistoricDetailPage>();
        services.AddTransient<JornadaPage>();
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
        // Cleanup notification service
        try
        {
            var notificationService = _serviceProvider?.GetService<INotificationService>();
            if (notificationService != null)
            {
                notificationService.OnChangeActivity -= OnNotificationChangeActivity;
            }
            notificationService?.Dispose();
        }
        catch
        {
            // Ignore cleanup errors
        }

        // Cleanup global hotkey service
        try
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
        catch
        {
            // Ignore cleanup errors
        }

        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

