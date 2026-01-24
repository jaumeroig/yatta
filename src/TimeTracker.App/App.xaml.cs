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

namespace TimeTracker.App;

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

        // Apply database migrations
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TimeTrackerDbContext>();
            dbContext.Database.Migrate();
        }

        // Initialize localization service (loads saved language preference)
        var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();

        // Load and apply theme
        var themeService = _serviceProvider.GetRequiredService<ThemeService>();
        _ = themeService.LoadThemeAsync();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register DbContext
        services.AddDbContext<TimeTrackerDbContext>(options =>
            options.UseSqlite(DatabaseConfiguration.GetConnectionString()));

        // Register repositories
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ITimeRecordRepository, TimeRecordRepository>();
        services.AddScoped<IWorkdaySlotRepository, WorkdaySlotRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        // Register services
        services.AddScoped<ITimeCalculatorService, TimeCalculatorService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IWorkdayService, WorkdayService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<RegistresViewModel>();
        services.AddTransient<JornadaViewModel>();
        services.AddTransient<ActivitatsViewModel>();
        services.AddTransient<OpcionsViewModel>();

        // Register Pages
        services.AddTransient<RegistresPage>();
        services.AddTransient<JornadaPage>();
        services.AddTransient<ActivitatsPage>();
        services.AddTransient<OpcionsPage>();

        // Register windows
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

