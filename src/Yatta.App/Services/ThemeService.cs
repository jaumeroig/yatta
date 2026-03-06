namespace Yatta.App.Services;

using System.Windows;
using Wpf.Ui.Appearance;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;


/// <summary>
/// Implementation of the theme service for WPF-UI.
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
            // Get the current operating system theme and convert it
            applicationTheme = ConvertSystemThemeToApplicationTheme(ApplicationThemeManager.GetSystemTheme());
            
            // Subscribe to operating system theme changes if we're not already
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
    /// Initializes the system theme watcher if not already done.
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
    /// Converts a SystemTheme to ApplicationTheme.
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
    /// Handles operating system theme changes when using Theme.System.
    /// </summary>
    private void OnApplicationThemeChanged(ApplicationTheme currentTheme, System.Windows.Media.Color systemAccent)
    {
        // If the configured theme is System, re-apply when the system changes
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
