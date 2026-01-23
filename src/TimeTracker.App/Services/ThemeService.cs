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
        
        var applicationTheme = theme switch
        {
            Theme.Light => ApplicationTheme.Light,
            Theme.Dark => ApplicationTheme.Dark,
            _ => ApplicationTheme.Dark
        };

        ApplicationThemeManager.Apply(applicationTheme);
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
}
