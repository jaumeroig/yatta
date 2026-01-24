namespace TimeTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel per a la pàgina d'opcions i configuració.
/// </summary>
public partial class OpcionsViewModel : ObservableObject
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IThemeService _themeService;
    private AppSettings? _currentSettings;

    public OpcionsViewModel(
        ISettingsRepository settingsRepository,
        IThemeService themeService)
    {
        _settingsRepository = settingsRepository;
        _themeService = themeService;
        
        // Inicialitzar valors per defecte
        IsDarkTheme = true;
        IsLightTheme = false;
        IsSystemTheme = false;
        NotificationsEnabled = false;
        WorkdayHours = 8;
        WorkdayMinutes = 0;
    }

    #region Observable Properties

    /// <summary>
    /// Indica si el tema fosc està seleccionat.
    /// </summary>
    [ObservableProperty]
    private bool _isDarkTheme;

    /// <summary>
    /// Indica si el tema clar està seleccionat.
    /// </summary>
    [ObservableProperty]
    private bool _isLightTheme;

    /// <summary>
    /// Indica si el tema del sistema està seleccionat.
    /// </summary>
    [ObservableProperty]
    private bool _isSystemTheme;

    /// <summary>
    /// Indica si les notificacions estan activades.
    /// </summary>
    [ObservableProperty]
    private bool _notificationsEnabled;

    /// <summary>
    /// Hores de la jornada laboral total.
    /// </summary>
    [ObservableProperty]
    private int _workdayHours;

    /// <summary>
    /// Minuts de la jornada laboral total.
    /// </summary>
    [ObservableProperty]
    private int _workdayMinutes;

    /// <summary>
    /// Versió de l'aplicació.
    /// </summary>
    [ObservableProperty]
    private string _appVersion = string.Empty;

    #endregion

    #region Commands

    /// <summary>
    /// Carrega les dades inicials.
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await LoadSettingsAsync();
        LoadAppVersion();
    }

    /// <summary>
    /// Aplica el tema fosc.
    /// </summary>
    [RelayCommand]
    private async Task SelectDarkThemeAsync()
    {
        IsDarkTheme = true;
        IsLightTheme = false;
        IsSystemTheme = false;
        await SaveThemeAsync(Theme.Dark);
    }

    /// <summary>
    /// Aplica el tema clar.
    /// </summary>
    [RelayCommand]
    private async Task SelectLightThemeAsync()
    {
        IsDarkTheme = false;
        IsLightTheme = true;
        IsSystemTheme = false;
        await SaveThemeAsync(Theme.Light);
    }

    /// <summary>
    /// Aplica el tema del sistema.
    /// </summary>
    [RelayCommand]
    private async Task SelectSystemThemeAsync()
    {
        IsDarkTheme = false;
        IsLightTheme = false;
        IsSystemTheme = true;
        await SaveThemeAsync(Theme.System);
    }

    /// <summary>
    /// Activa o desactiva les notificacions.
    /// </summary>
    [RelayCommand]
    private async Task ToggleNotificationsAsync()
    {
        await SaveNotificationsAsync(NotificationsEnabled);
    }

    /// <summary>
    /// Desa el temps total de la jornada.
    /// </summary>
    [RelayCommand]
    private async Task SaveWorkdayTimeAsync()
    {
        // Validar que les hores i minuts són vàlids
        if (WorkdayHours < 0 || WorkdayHours > 23)
        {
            WorkdayHours = 8;
        }
        
        if (WorkdayMinutes < 0 || WorkdayMinutes > 59)
        {
            WorkdayMinutes = 0;
        }

        var totalTime = new TimeSpan(WorkdayHours, WorkdayMinutes, 0);
        await SaveWorkdayTotalTimeAsync(totalTime);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Carrega la configuració des de la base de dades.
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        _currentSettings = await _settingsRepository.GetAsync();

        // Actualitzar les propietats del tema
        IsDarkTheme = _currentSettings.Theme == Theme.Dark;
        IsLightTheme = _currentSettings.Theme == Theme.Light;
        IsSystemTheme = _currentSettings.Theme == Theme.System;

        // Actualitzar notificacions
        NotificationsEnabled = _currentSettings.Notifications;

        // Actualitzar temps de jornada
        WorkdayHours = _currentSettings.WorkdayTotalTime.Hours;
        WorkdayMinutes = _currentSettings.WorkdayTotalTime.Minutes;
    }

    /// <summary>
    /// Desa el tema seleccionat.
    /// </summary>
    private async Task SaveThemeAsync(Theme theme)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.Theme = theme;
        await _settingsRepository.UpdateAsync(_currentSettings);
        
        // Aplicar el tema immediatament
        _themeService.ApplyTheme(theme);
    }

    /// <summary>
    /// Desa la configuració de notificacions.
    /// </summary>
    private async Task SaveNotificationsAsync(bool enabled)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.Notifications = enabled;
        await _settingsRepository.UpdateAsync(_currentSettings);
    }

    /// <summary>
    /// Desa el temps total de la jornada.
    /// </summary>
    private async Task SaveWorkdayTotalTimeAsync(TimeSpan totalTime)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.WorkdayTotalTime = totalTime;
        await _settingsRepository.UpdateAsync(_currentSettings);
    }

    /// <summary>
    /// Carrega la versió de l'aplicació.
    /// </summary>
    private void LoadAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version != null)
        {
            AppVersion = $"v{version.Major}.{version.Minor}.{version.Build}";
        }
        else
        {
            AppVersion = "v1.0.0";
        }
    }

    #endregion
}
