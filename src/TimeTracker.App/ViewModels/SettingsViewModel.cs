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
/// Represents a retention policy option for the dropdown.
/// </summary>
public class RetentionPolicyOption
{
    /// <summary>
    /// Retention policy value.
    /// </summary>
    public RetentionPolicy Value { get; set; }

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
    private readonly INotificationService _notificationService;
    private readonly IStartupService _startupService;
    private readonly IDataPurgeService _dataPurgeService;
    private AppSettings? _currentSettings;
    private CancellationTokenSource? _languageSaveCts;
    private CancellationTokenSource? _themeSaveCts;

    public SettingsViewModel(
        ISettingsRepository settingsRepository,
        IThemeService themeService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IStartupService startupService,
        IDataPurgeService dataPurgeService)
    {
        _settingsRepository = settingsRepository;
        _themeService = themeService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _startupService = startupService;
        _dataPurgeService = dataPurgeService;


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

        // Initialize retention policy options
        RetentionPolicyOptions =
        [
            new RetentionPolicyOption { Value = RetentionPolicy.Forever, DisplayName = Resources.Resources.RetentionPolicy_Forever },
            new RetentionPolicyOption { Value = RetentionPolicy.OneYear, DisplayName = Resources.Resources.RetentionPolicy_OneYear },
            new RetentionPolicyOption { Value = RetentionPolicy.TwoYears, DisplayName = Resources.Resources.RetentionPolicy_TwoYears },
            new RetentionPolicyOption { Value = RetentionPolicy.ThreeYears, DisplayName = Resources.Resources.RetentionPolicy_ThreeYears },
            new RetentionPolicyOption { Value = RetentionPolicy.Custom, DisplayName = Resources.Resources.RetentionPolicy_Custom }
        ];

        // Default values
        _selectedTheme = ThemeOptions[0];
        _selectedLanguage = LanguageOptions[0];
        _selectedRetentionPolicy = RetentionPolicyOptions[0];
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
    /// Available retention policy options.
    /// </summary>
    public ObservableCollection<RetentionPolicyOption> RetentionPolicyOptions { get; }

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
    /// Indicates if the application should minimize to tray when closing.
    /// </summary>
    [ObservableProperty]
    private bool _minimizeToTrayEnabled;

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

    /// <summary>
    /// Interval in minutes between notification reminders.
    /// </summary>
    [ObservableProperty]
    private int _notificationIntervalMinutes;

    /// <summary>
    /// Indicates if the application should start with Windows.
    /// </summary>
    [ObservableProperty]
    private bool _startWithWindowsEnabled;

    /// <summary>
    /// Selected retention policy.
    /// </summary>
    [ObservableProperty]
    private RetentionPolicyOption _selectedRetentionPolicy;

    /// <summary>
    /// Custom retention days.
    /// </summary>
    [ObservableProperty]
    private int _customRetentionDays;

    /// <summary>
    /// Indicates if automatic purge is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _autoPurgeEnabled;

    /// <summary>
    /// Indicates if the custom retention days field should be visible.
    /// </summary>
    [ObservableProperty]
    private bool _isCustomRetentionVisible;

    /// <summary>
    /// Message to display after a purge operation.
    /// </summary>
    [ObservableProperty]
    private string _purgeResultMessage = string.Empty;

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

    /// <summary>
    /// Executes when the selected retention policy changes.
    /// </summary>
    partial void OnSelectedRetentionPolicyChanged(RetentionPolicyOption value)
    {
        if (_currentSettings != null && value != null)
        {
            IsCustomRetentionVisible = value.Value == RetentionPolicy.Custom;
            _ = SaveRetentionPolicyAsync(value.Value);
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
    /// Enables or disables minimize to tray.
    /// </summary>
    [RelayCommand]
    private async Task ToggleMinimizeToTrayAsync()
    {
        await SaveMinimizeToTrayAsync(MinimizeToTrayEnabled);
    }

    /// <summary>
    /// Enables or disables start with Windows.
    /// </summary>
    [RelayCommand]
    private async Task ToggleStartWithWindowsAsync()
    {
        await SaveStartWithWindowsAsync(StartWithWindowsEnabled);
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

    /// <summary>
    /// Saves the notification interval.
    /// </summary>
    [RelayCommand]
    private async Task SaveNotificationIntervalAsync()
    {
        // Validate that interval is valid (between 15 and 480 minutes = 8 hours)
        if (NotificationIntervalMinutes < 15)
        {
            NotificationIntervalMinutes = 15;
        }
        else if (NotificationIntervalMinutes > 480)
        {
            NotificationIntervalMinutes = 480;
        }

        await SaveNotificationIntervalMinutesAsync(NotificationIntervalMinutes);
    }

    /// <summary>
    /// Saves the custom retention days.
    /// </summary>
    [RelayCommand]
    private async Task SaveCustomRetentionDaysAsync()
    {
        if (CustomRetentionDays < 1)
        {
            CustomRetentionDays = 1;
        }

        await SaveCustomRetentionDaysValueAsync(CustomRetentionDays);
    }

    /// <summary>
    /// Calculates purge preview information for the confirmation dialog.
    /// Returns the cutoff date, time record count and workday count.
    /// </summary>
    public async Task<(DateOnly? CutoffDate, int TimeRecordCount, int WorkdayCount)> GetPurgePreviewAsync()
    {
        if (_currentSettings == null)
        {
            return (null, 0, 0);
        }

        var cutoffDate = _dataPurgeService.CalculateCutoffDate(
            _currentSettings.RetentionPolicy, _currentSettings.CustomRetentionDays);

        if (!cutoffDate.HasValue)
        {
            return (null, 0, 0);
        }

        var (timeRecordCount, workdayCount) = await _dataPurgeService.GetPurgeableCountAsync(cutoffDate.Value);
        return (cutoffDate, timeRecordCount, workdayCount);
    }

    /// <summary>
    /// Executes the purge operation.
    /// </summary>
    public async Task<(int TimeRecordsDeleted, int WorkdaysDeleted)> ExecutePurgeAsync()
    {
        if (_currentSettings == null)
        {
            return (0, 0);
        }

        var cutoffDate = _dataPurgeService.CalculateCutoffDate(
            _currentSettings.RetentionPolicy, _currentSettings.CustomRetentionDays);

        if (!cutoffDate.HasValue)
        {
            return (0, 0);
        }

        return await _dataPurgeService.ExecutePurgeAsync(cutoffDate.Value);
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

        // Update minimize to tray
        MinimizeToTrayEnabled = _currentSettings.MinimizeToTray;

        // Update start with Windows
        StartWithWindowsEnabled = _currentSettings.StartWithWindows;

        // Update workday time
        WorkdayHours = _currentSettings.WorkdayTotalTime.Hours;
        WorkdayMinutes = _currentSettings.WorkdayTotalTime.Minutes;

        // Update notification interval
        NotificationIntervalMinutes = _currentSettings.NotificationIntervalMinutes;

        // Update retention policy
        SelectedRetentionPolicy = RetentionPolicyOptions.FirstOrDefault(r => r.Value == _currentSettings.RetentionPolicy) ?? RetentionPolicyOptions[0];
        CustomRetentionDays = _currentSettings.CustomRetentionDays;
        IsCustomRetentionVisible = _currentSettings.RetentionPolicy == RetentionPolicy.Custom;
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
    /// Saves the notifications configuration and syncs with the notification service.
    /// </summary>
    private async Task SaveNotificationsAsync(bool enabled)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.Notifications = enabled;
        await _settingsRepository.UpdateAsync(_currentSettings);

        // Sync with notification service
        _notificationService.IsEnabled = enabled;
    }

    /// <summary>
    /// Saves the minimize to tray configuration.
    /// </summary>
    private async Task SaveMinimizeToTrayAsync(bool enabled)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.MinimizeToTray = enabled;
        await _settingsRepository.UpdateAsync(_currentSettings);
    }

    /// <summary>
    /// Saves the start with Windows configuration and updates the registry.
    /// </summary>
    private async Task SaveStartWithWindowsAsync(bool enabled)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.StartWithWindows = enabled;
        await _settingsRepository.UpdateAsync(_currentSettings);

        // Update Windows registry
        if (enabled)
        {
            _startupService.EnableStartup();
        }
        else
        {
            _startupService.DisableStartup();
        }
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
    /// Saves the notification interval in minutes.
    /// </summary>
    private async Task SaveNotificationIntervalMinutesAsync(int intervalMinutes)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.NotificationIntervalMinutes = intervalMinutes;
        await _settingsRepository.UpdateAsync(_currentSettings);
    }

    /// <summary>
    /// Saves the retention policy.
    /// </summary>
    private async Task SaveRetentionPolicyAsync(RetentionPolicy policy)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.RetentionPolicy = policy;
        await _settingsRepository.UpdateAsync(_currentSettings);
    }

    /// <summary>
    /// Saves the custom retention days value.
    /// </summary>
    private async Task SaveCustomRetentionDaysValueAsync(int days)
    {
        if (_currentSettings == null)
        {
            return;
        }

        _currentSettings.CustomRetentionDays = days;
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
