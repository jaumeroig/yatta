using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Services;
using TimeTracker.Data;
using TimeTracker.Data.Repositories;

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

        // Register windows
        services.AddSingleton<MainWindow>();

        // TODO: Register view models here
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

