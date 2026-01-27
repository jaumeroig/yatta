namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Represents a theme option for the dropdown.
/// </summary>
public class ThemeOption
{
    /// <summary>
    /// Theme value.
    /// </summary>
    public Theme Value { get; set; }

    /// <summary>
    /// Name to display in the UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a language option for the dropdown.
/// </summary>
public class LanguageOption
{
    /// <summary>
    /// Culture code (null for system).
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Name to display in the UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for the settings and configuration page.
/// </summary>
public partial class SettingsViewModel : ObservableObject, IDisposable
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private AppSettings? _currentSettings;
    private CancellationTokenSource? _languageSaveCts;
    private CancellationTokenSource? _themeSaveCts;

    public SettingsViewModel(
        ISettingsRepository settingsRepository,
        IThemeService themeService,
        ILocalizationService localizationService)
    {
        _settingsRepository = settingsRepository;
        _themeService = themeService;
        _localizationService = localizationService;


        // Initialize theme options
        ThemeOptions =
        [
            new ThemeOption { Value = Theme.System, DisplayName = Resources.Resources.RadioButton_SystemTheme },
            new ThemeOption { Value = Theme.Dark, DisplayName = Resources.Resources.RadioButton_DarkTheme },
            new ThemeOption { Value = Theme.Light, DisplayName = Resources.Resources.RadioButton_LightTheme }
        ];

        // Initialize language options
        LanguageOptions =
        [
            new LanguageOption { Value = null, DisplayName = Resources.Resources.RadioButton_SystemLanguage },
            new LanguageOption { Value = "es-ES", DisplayName = Resources.Resources.RadioButton_Spanish },
            new LanguageOption { Value = "ca-ES", DisplayName = Resources.Resources.RadioButton_Catalan }
        ];

        // Default values
        _selectedTheme = ThemeOptions[0];
        _selectedLanguage = LanguageOptions[0];
        NotificationsEnabled = false;
        WorkdayHours = 8;
        WorkdayMinutes = 0;
    }

    #region Observable Properties

    /// <summary>
    /// Available theme options.
    /// </summary>
    public ObservableCollection<ThemeOption> ThemeOptions { get; }

    /// <summary>
    /// Available language options.
    /// </summary>
    public ObservableCollection<LanguageOption> LanguageOptions { get; }

    /// <summary>
    /// Selected theme.
    /// </summary>
    [ObservableProperty]
    private ThemeOption _selectedTheme;

    /// <summary>
    /// Selected language.
    /// </summary>
    [ObservableProperty]
    private LanguageOption _selectedLanguage;

    /// <summary>
    /// Indicates if notifications are enabled.
    /// </summary>
    [ObservableProperty]
    private bool _notificationsEnabled;

    /// <summary>
    /// Hours of the total workday.
    /// </summary>
    [ObservableProperty]
    private int _workdayHours;

    /// <summary>
    /// Minutes of the total workday.
    /// </summary>
    [ObservableProperty]
    private int _workdayMinutes;

    /// <summary>
    /// Application version.
    /// </summary>
    [ObservableProperty]
    private string _appVersion = string.Empty;

    #endregion

    #region Property Changed Handlers

    /// <summary>
    /// Executes when the selected theme changes.
    /// </summary>
    partial void OnSelectedThemeChanged(ThemeOption value)
    {
        if (_currentSettings != null && value != null)
        {
            ResetCancellationTokenSource(ref _themeSaveCts);
            var token = _themeSaveCts!.Token;
            _ = SaveThemeAsync(value.Value, token);
        }
    }

    /// <summary>
    /// Executes when the selected language changes.
    /// </summary>
    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (_currentSettings != null && value != null)
        {
            ResetCancellationTokenSource(ref _languageSaveCts);
            var token = _languageSaveCts!.Token;
            _ = SaveLanguageAsync(value.Value, token);
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Loads initial data.
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await LoadSettingsAsync();
        LoadAppVersion();
    }

    /// <summary>
    /// Enables or disables notifications.
    /// </summary>
    [RelayCommand]
    private async Task ToggleNotificationsAsync()
    {
        await SaveNotificationsAsync(NotificationsEnabled);
    }

    /// <summary>
    /// Saves the total workday time.
    /// </summary>
    [RelayCommand]
    private async Task SaveWorkdayTimeAsync()
    {
        // Validate that hours and minutes are valid
        if (WorkdayHours < 0 || WorkdayHours > 23)
        {
            WorkdayHours = 8;
        }

        if (WorkdayMinutes is < 0 or > 59)
        {
            WorkdayMinutes = 0;
        }

        var totalTime = new TimeSpan(WorkdayHours, WorkdayMinutes, 0);
        await SaveWorkdayTotalTimeAsync(totalTime);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads configuration from the database.
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        _currentSettings = await _settingsRepository.GetAsync();

        // Update selected theme
        SelectedTheme = ThemeOptions.FirstOrDefault(t => t.Value == _currentSettings.Theme) ?? ThemeOptions[0];

        // Update selected language
        SelectedLanguage = LanguageOptions.FirstOrDefault(l => l.Value == _currentSettings.Language) ?? LanguageOptions[0];

        // Update notifications
        NotificationsEnabled = _currentSettings.Notifications;

        // Update workday time
        WorkdayHours = _currentSettings.WorkdayTotalTime.Hours;
        WorkdayMinutes = _currentSettings.WorkdayTotalTime.Minutes;
    }

    /// <summary>
    /// Saves the selected theme.
    /// </summary>
    private async Task SaveThemeAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        if (_currentSettings == null)
        {
            return;
        }

        try
        {
            // Check if the operation has been canceled before saving
            cancellationToken.ThrowIfCancellationRequested();

            _currentSettings.Theme = theme;
            await _settingsRepository.UpdateAsync(_currentSettings);

            // Check if the operation has been canceled before applying the theme
            cancellationToken.ThrowIfCancellationRequested();

            // Apply the theme immediately
            _themeService.ApplyTheme(theme);
        }
        catch (OperationCanceledException)
        {
            // The operation has been canceled, do nothing
        }
    }

    /// <summary>
    /// Saves the notifications configuration.
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
    /// Saves the total workday time.
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
    /// Saves the selected language and applies it immediately.
    /// </summary>
    /// <param name="language">Culture code (es-ES, ca-ES) or null for system.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    private async Task SaveLanguageAsync(string? language, CancellationToken cancellationToken = default)
    {
        if (_currentSettings == null)
        {
            return;
        }

        try
        {
            // Check if the operation has been canceled before saving
            cancellationToken.ThrowIfCancellationRequested();

            _currentSettings.Language = language;
            await _settingsRepository.UpdateAsync(_currentSettings);

            // Check if the operation has been canceled before applying the language
            cancellationToken.ThrowIfCancellationRequested();

            // Apply the language immediately
            _localizationService.SetCulture(language);
        }
        catch (OperationCanceledException)
        {
            // The operation has been canceled, do nothing
        }
    }

    /// <summary>
    /// Loads the application version.
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
    /// Resets a CancellationTokenSource, canceling and disposing the previous one.
    /// </summary>
    /// <param name="cts">Reference to the CancellationTokenSource to reset.</param>
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
