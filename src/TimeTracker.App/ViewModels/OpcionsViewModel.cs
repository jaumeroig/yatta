namespace TimeTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Reflection;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Representa una opció de tema per al dropdown.
/// </summary>
public class ThemeOption
{
    /// <summary>
    /// Valor del tema.
    /// </summary>
    public Theme Value { get; set; }

    /// <summary>
    /// Nom per mostrar a la UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// Representa una opció d'idioma per al dropdown.
/// </summary>
public class LanguageOption
{
    /// <summary>
    /// Codi de cultura (null per sistema).
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Nom per mostrar a la UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel per a la pàgina d'opcions i configuració.
/// </summary>
public partial class OpcionsViewModel : ObservableObject, IDisposable
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private AppSettings? _currentSettings;
    private CancellationTokenSource? _languageSaveCts;
    private CancellationTokenSource? _themeSaveCts;

    public OpcionsViewModel(
        ISettingsRepository settingsRepository,
        IThemeService themeService,
        ILocalizationService localizationService)
    {
        _settingsRepository = settingsRepository;
        _themeService = themeService;
        _localizationService = localizationService;
        
        // Inicialitzar opcions de tema
        ThemeOptions =
        [
            new ThemeOption { Value = Theme.System, DisplayName = "Predeterminat (Sistema)" },
            new ThemeOption { Value = Theme.Dark, DisplayName = "Fosc" },
            new ThemeOption { Value = Theme.Light, DisplayName = "Clar" }
        ];

        // Inicialitzar opcions d'idioma
        LanguageOptions =
        [
            new LanguageOption { Value = null, DisplayName = "Predeterminat (Sistema)" },
            new LanguageOption { Value = "es-ES", DisplayName = "Español" },
            new LanguageOption { Value = "ca-ES", DisplayName = "Català" }
        ];

        // Valors per defecte
        _selectedTheme = ThemeOptions[0];
        _selectedLanguage = LanguageOptions[0];
        NotificationsEnabled = false;
        WorkdayHours = 8;
        WorkdayMinutes = 0;
    }

    #region Observable Properties

    /// <summary>
    /// Opcions de tema disponibles.
    /// </summary>
    public ObservableCollection<ThemeOption> ThemeOptions { get; }

    /// <summary>
    /// Opcions d'idioma disponibles.
    /// </summary>
    public ObservableCollection<LanguageOption> LanguageOptions { get; }

    /// <summary>
    /// Tema seleccionat.
    /// </summary>
    [ObservableProperty]
    private ThemeOption _selectedTheme;

    /// <summary>
    /// Idioma seleccionat.
    /// </summary>
    [ObservableProperty]
    private LanguageOption _selectedLanguage;

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

    #region Property Changed Handlers

    /// <summary>
    /// S'executa quan canvia el tema seleccionat.
    /// </summary>
    partial void OnSelectedThemeChanged(ThemeOption value)
    {
        if (_currentSettings != null && value != null)
        {
            ResetCancellationTokenSource(ref _themeSaveCts);
            _ = SaveThemeAsync(value.Value, _themeSaveCts!.Token);
        }
    }

    /// <summary>
    /// S'executa quan canvia l'idioma seleccionat.
    /// </summary>
    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (_currentSettings != null && value != null)
        {
            ResetCancellationTokenSource(ref _languageSaveCts);
            _ = SaveLanguageAsync(value.Value, _languageSaveCts!.Token);
        }
    }

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

        // Actualitzar el tema seleccionat
        SelectedTheme = ThemeOptions.FirstOrDefault(t => t.Value == _currentSettings.Theme) ?? ThemeOptions[0];

        // Actualitzar l'idioma seleccionat
        SelectedLanguage = LanguageOptions.FirstOrDefault(l => l.Value == _currentSettings.Language) ?? LanguageOptions[0];

        // Actualitzar notificacions
        NotificationsEnabled = _currentSettings.Notifications;

        // Actualitzar temps de jornada
        WorkdayHours = _currentSettings.WorkdayTotalTime.Hours;
        WorkdayMinutes = _currentSettings.WorkdayTotalTime.Minutes;
    }

    /// <summary>
    /// Desa el tema seleccionat.
    /// </summary>
    private async Task SaveThemeAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        if (_currentSettings == null)
        {
            return;
        }

        try
        {
            // Comprovar si l'operació ha estat cancel·lada abans de desar
            cancellationToken.ThrowIfCancellationRequested();
            
            _currentSettings.Theme = theme;
            await _settingsRepository.UpdateAsync(_currentSettings);
            
            // Comprovar si l'operació ha estat cancel·lada abans d'aplicar el tema
            cancellationToken.ThrowIfCancellationRequested();
            
            // Aplicar el tema immediatament
            _themeService.ApplyTheme(theme);
        }
        catch (OperationCanceledException)
        {
            // L'operació ha estat cancel·lada, no fer res
        }
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
    /// Desa l'idioma seleccionat i l'aplica immediatament.
    /// </summary>
    /// <param name="language">Codi de cultura (es-ES, ca-ES) o null per sistema.</param>
    /// <param name="cancellationToken">Token per cancel·lar l'operació.</param>
    private async Task SaveLanguageAsync(string? language, CancellationToken cancellationToken = default)
    {
        if (_currentSettings == null)
        {
            return;
        }

        try
        {
            // Comprovar si l'operació ha estat cancel·lada abans de desar
            cancellationToken.ThrowIfCancellationRequested();
            
            _currentSettings.Language = language;
            await _settingsRepository.UpdateAsync(_currentSettings);
            
            // Comprovar si l'operació ha estat cancel·lada abans d'aplicar l'idioma
            cancellationToken.ThrowIfCancellationRequested();
            
            // Aplicar l'idioma immediatament
            _localizationService.SetCulture(language);
        }
        catch (OperationCanceledException)
        {
            // L'operació ha estat cancel·lada, no fer res
        }
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

    /// <summary>
    /// Reinicia un CancellationTokenSource, cancel·lant i disposant l'anterior.
    /// </summary>
    /// <param name="cts">Referència al CancellationTokenSource a reiniciar.</param>
    private static void ResetCancellationTokenSource(ref CancellationTokenSource? cts)
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }

    #endregion

    #region IDisposable

    private bool _disposed = false;

    /// <summary>
    /// Allibera els recursos utilitzats pel ViewModel.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Allibera els recursos utilitzats pel ViewModel.
    /// </summary>
    /// <param name="disposing">Indica si s'estan disposant els recursos gerenciats.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Alliberar recursos gerenciats
                _languageSaveCts?.Cancel();
                _languageSaveCts?.Dispose();
                _languageSaveCts = null;
                
                _themeSaveCts?.Cancel();
                _themeSaveCts?.Dispose();
                _themeSaveCts = null;
            }

            _disposed = true;
        }
    }

    #endregion
}
