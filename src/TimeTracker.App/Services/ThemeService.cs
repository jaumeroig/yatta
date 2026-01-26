using System.Windows;
using Wpf.Ui.Appearance;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

namespace TimeTracker.App.Services;

/// <summary>
/// Implementació del servei de tema per WPF-UI.
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ISettingsRepository _settingsRepository;
    private Theme _currentTheme = Theme.Dark;
    private bool _isWatchingSystemTheme;

    public ThemeService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public Theme GetCurrentTheme()
    {
        return _currentTheme;
    }

    public void ApplyTheme(Theme theme)
    {
        _currentTheme = theme;
        
        ApplicationTheme applicationTheme;
        
        if (theme == Theme.System)
        {
            // Obtenir el tema actual del sistema operatiu i convertir-lo
            applicationTheme = ConvertSystemThemeToApplicationTheme(ApplicationThemeManager.GetSystemTheme());
            
            // Subscriure's als canvis de tema del sistema operatiu si encara no ho estem
            EnsureSystemThemeWatcherInitialized();
        }
        else
        {
            applicationTheme = theme switch
            {
                Theme.Light => ApplicationTheme.Light,
                Theme.Dark => ApplicationTheme.Dark,
                _ => ApplicationTheme.Dark
            };
        }

        ApplicationThemeManager.Apply(applicationTheme);
    }

    /// <summary>
    /// Inicialitza el watcher del tema del sistema si encara no s'ha fet.
    /// </summary>
    private void EnsureSystemThemeWatcherInitialized()
    {
        if (_isWatchingSystemTheme)
            return;

        var mainWindow = Application.Current?.MainWindow;
        if (mainWindow != null)
        {
            SystemThemeWatcher.Watch(mainWindow);
            ApplicationThemeManager.Changed += OnApplicationThemeChanged;
            _isWatchingSystemTheme = true;
        }
    }

    /// <summary>
    /// Converteix un SystemTheme a ApplicationTheme.
    /// </summary>
    private static ApplicationTheme ConvertSystemThemeToApplicationTheme(SystemTheme systemTheme)
    {
        return systemTheme switch
        {
            SystemTheme.Light => ApplicationTheme.Light,
            SystemTheme.Dark => ApplicationTheme.Dark,
            SystemTheme.HC1 => ApplicationTheme.HighContrast,
            SystemTheme.HC2 => ApplicationTheme.HighContrast,
            SystemTheme.HCBlack => ApplicationTheme.HighContrast,
            SystemTheme.HCWhite => ApplicationTheme.HighContrast,
            _ => ApplicationTheme.Dark
        };
    }

    public async Task LoadThemeAsync()
    {
        var settings = await _settingsRepository.GetAsync();
        if (settings != null)
        {
            ApplyTheme(settings.Theme);
        }
    }

    public async Task SaveThemeAsync(Theme theme)
    {
        var settings = await _settingsRepository.GetAsync();
        if (settings != null)
        {
            settings.Theme = theme;
            await _settingsRepository.UpdateAsync(settings);
        }
        ApplyTheme(theme);
    }

    /// <summary>
    /// Gestiona els canvis de tema del sistema operatiu quan s'utilitza Theme.System.
    /// </summary>
    private void OnApplicationThemeChanged(ApplicationTheme currentTheme, System.Windows.Media.Color systemAccent)
    {
        // Si el tema configurat és System, re-aplicar quan el sistema canvia
        if (_currentTheme == Theme.System)
        {
            var systemTheme = ApplicationThemeManager.GetSystemTheme();
            var expectedTheme = ConvertSystemThemeToApplicationTheme(systemTheme);
            if (currentTheme != expectedTheme)
            {
                ApplicationThemeManager.Apply(expectedTheme);
            }
        }
    }
}
