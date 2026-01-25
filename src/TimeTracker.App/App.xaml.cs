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

        // IMPORTANT: Inicialitzar localització ABANS de crear la MainWindow
        // Això assegura que els recursos XAML s'avaluïn amb la cultura correcta
        InitializeLocalization();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        
        // Carregar i aplicar el tema després de crear la finestra principal
        // Això assegura que el tema del sistema es detecti correctament
        var themeService = _serviceProvider.GetRequiredService<ThemeService>();
        mainWindow.Loaded += async (_, _) => await themeService.LoadThemeAsync();
        
        mainWindow.Show();
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
        services.AddSingleton<INavigationService, NavigationService>();

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<RegistresViewModel>();
        services.AddTransient<JornadaViewModel>();
        services.AddTransient<ActivitatsViewModel>();
        services.AddTransient<ActivityDetailViewModel>();
        services.AddTransient<OpcionsViewModel>();

        // Register Pages
        services.AddTransient<RegistresPage>();
        services.AddTransient<JornadaPage>();
        services.AddTransient<ActivitatsPage>();
        services.AddTransient<ActivityDetailPage>();
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

