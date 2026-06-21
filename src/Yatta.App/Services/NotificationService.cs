namespace Yatta.App.Services;

using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using Yatta.App.Models;
using Yatta.App.Views.Dialogs;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Service to manage Windows toast notifications for time tracking reminders.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalizationService _localizationService;

    private Timer? _timer;
    private DateTime _lastNotificationTime;
    private int _nextReminderMinutes;
    private DateTime? _snoozeUntil;
    private bool _isEnabled;
    private bool _isDisposed;
    private volatile bool _isCustomReminderDialogOpen;

    public event EventHandler? OnContinueActivity;
    public event EventHandler<Guid>? OnChangeActivity;
    public event EventHandler<int>? OnSnooze;
    public event EventHandler? OnStopActivity;
    public event EventHandler? StateChanged;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                if (value)
                    Start();
                else
                    Stop();
                OnStateChanged();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether a custom (non-default) reminder is
    /// currently scheduled. Returns false when notifications are disabled or
    /// when the next reminder matches the standard interval from settings.
    /// </summary>
    public bool IsCustomReminderActive
    {
        get
        {
            if (!_isEnabled || _nextReminderMinutes <= 0)
                return false;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
                var settings = settingsRepository.GetAsync().GetAwaiter().GetResult();
                var defaultMinutes = GetEffectiveIntervalMinutes(settings);
                return _nextReminderMinutes != defaultMinutes;
            }
            catch
            {
                return false;
            }
        }
    }

    public NotificationService(IServiceProvider serviceProvider, ILocalizationService localizationService)
    {
        _serviceProvider = serviceProvider;
        _localizationService = localizationService;

        // Register for toast activation
        ToastNotificationManagerCompat.OnActivated += OnToastActivated;

        // Clear old notification history to ensure the new app name is used
        try
        {
            ToastNotificationManagerCompat.History.Clear();
        }
        catch
        {
            // Ignore if clearing fails
        }
    }

    public void Start()
    {
        if (_timer != null) return;

        _timer = new Timer(60000); // Check every minute
        _timer.Elapsed += async (s, e) => await CheckAndNotifyAsync();
        _timer.AutoReset = true;
        _timer.Start();
        _lastNotificationTime = DateTime.Now;
        _nextReminderMinutes = 0;
        _snoozeUntil = null;
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    public void ResetTimer()
    {
        _lastNotificationTime = DateTime.Now;
        _nextReminderMinutes = 0;
        _snoozeUntil = null;
        OnStateChanged();
    }

    /// <summary>
    /// Raises the <see cref="StateChanged"/> event so listeners can refresh
    /// their notification-related UI (e.g. bell icon state).
    /// </summary>
    protected virtual void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Computes the effective reminder interval in minutes for the given settings preset.
    /// </summary>
    private static int GetPresetMinutes(ReminderInterval interval)
    {
        return interval switch
        {
            ReminderInterval.Minutes5 => 15,
            ReminderInterval.Minutes30 => 30,
            ReminderInterval.Hour1 => 60,
            ReminderInterval.Hours2 => 120,
            _ => 0
        };
    }

    /// <summary>
    /// Formats a minute value as a localized display string (e.g. "15 minuts" or "1 hora").
    /// </summary>
    private string FormatMinutes(int minutes)
    {
        return minutes switch
        {
            15 => _localizationService.GetString("Notification_15min"),
            30 => _localizationService.GetString("Notification_30min"),
            60 => _localizationService.GetString("Notification_1hour"),
            120 => _localizationService.GetString("Notification_2hours"),
            _ => $"{minutes} {_localizationService.GetString("Label_Minutes")}"
        };
    }

    public async Task CheckAndNotifyAsync()
    {
        if (!_isEnabled) return;
        if (_isCustomReminderDialogOpen) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
            var timeRecordRepository = scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>();

            var settings = await settingsRepository.GetAsync();
            if (!settings.Notifications) return;

            var activeRecord = await timeRecordRepository.GetActiveAsync();
            if (activeRecord == null)
            {
                // No active record, reset timer for when one starts
                _lastNotificationTime = DateTime.Now;
                var wasCustom = _nextReminderMinutes > 0 || _snoozeUntil.HasValue;
                _nextReminderMinutes = 0;
                _snoozeUntil = null;
                if (wasCustom) OnStateChanged();
                return;
            }

            // Snooze-until-tomorrow path: do not notify until the target time has passed
            if (_snoozeUntil.HasValue)
            {
                if (DateTime.Now < _snoozeUntil.Value)
                    return;

                // Target reached: show immediately and reset
                await ShowNotificationAsync(activeRecord);
                _lastNotificationTime = DateTime.Now;
                _nextReminderMinutes = 0;
                _snoozeUntil = null;
                OnStateChanged();
                return;
            }

            // Standard interval path
            var threshold = _nextReminderMinutes > 0
                ? _nextReminderMinutes
                : GetEffectiveIntervalMinutes(settings);

            var timeSinceLastNotification = DateTime.Now - _lastNotificationTime;

            if (timeSinceLastNotification.TotalMinutes >= threshold)
            {
                await ShowNotificationAsync(activeRecord);
                _lastNotificationTime = DateTime.Now;
                var wasCustom = _nextReminderMinutes > 0;
                _nextReminderMinutes = 0;
                _snoozeUntil = null;
                if (wasCustom) OnStateChanged();
            }
        }
        catch
        {
            // Silently handle errors to avoid crashing the timer
        }
    }

    /// <summary>
    /// Computes the effective interval in minutes from the current settings preset.
    /// </summary>
    private static int GetEffectiveIntervalMinutes(AppSettings settings)
    {
        var presetMinutes = GetPresetMinutes(settings.ReminderInterval);
        return presetMinutes > 0 ? presetMinutes : settings.NotificationIntervalMinutes;
    }

    public async Task ForceShowNotificationAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var timeRecordRepository = scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>();

            var activeRecord = await timeRecordRepository.GetActiveAsync();
            System.Diagnostics.Debug.WriteLine($"[NotificationService] ForceShow: activeRecord={(activeRecord != null ? activeRecord.Id.ToString() : "null")}");
            if (activeRecord != null)
            {
                await ShowNotificationAsync(activeRecord);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NotificationService] ForceShow error: {ex}");
        }
    }

    private async Task ShowNotificationAsync(TimeRecord record)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var activityRepository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();
            var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();

            var activity = await activityRepository.GetByIdAsync(record.ActivityId);
            var activityName = activity?.Name ?? _localizationService.GetString("Notification_UnknownActivity");

            var startDateTime = record.Date.ToDateTime(record.StartTime);
            var duration = DateTime.Now - startDateTime;
            var durationText = $"{(int)duration.TotalHours}h {duration.Minutes}m";

            var title = _localizationService.GetString("Notification_StillWorking", activityName);
            var body = _localizationService.GetString("Notification_Duration", durationText);

            var continueText = _localizationService.GetString("Notification_Continue");
            var changeText = _localizationService.GetString("Notification_ChangeActivity");
            var stopText = _localizationService.GetString("Notification_Stop");
            var customizeText = _localizationService.GetString("Notification_Customize");

            // Get the logo path
            var logoPath = GetLogoPath();

            // Get settings to determine notification behavior and default snooze selection
            var settings = await settingsRepository.GetAsync();
            var scenario = settings.KeepNotificationsVisible ? ToastScenario.Reminder : ToastScenario.Default;

            // Build dynamic combo values: 15, 30, 60 plus either the custom value or 120
            var comboValues = new List<int> { 15, 30, 60 };
            if (settings.ReminderInterval == ReminderInterval.Custom && !comboValues.Contains(settings.NotificationIntervalMinutes))
            {
                comboValues.Add(settings.NotificationIntervalMinutes);
            }
            else
            {
                comboValues.Add(120);
            }
            comboValues.Sort();

            // Determine default selection (the configured interval, or closest preset)
            var defaultValue = settings.ReminderInterval == ReminderInterval.Custom
                ? settings.NotificationIntervalMinutes
                : GetPresetMinutes(settings.ReminderInterval);
            var defaultSelectionId = comboValues.Contains(defaultValue)
                ? defaultValue.ToString()
                : comboValues.OrderBy(v => Math.Abs(v - defaultValue)).First().ToString();

            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(body)
                .SetToastScenario(scenario);

            // Add logo if available
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                builder.AddAppLogoOverride(new Uri(logoPath), ToastGenericAppLogoCrop.Circle);
            }

            // Build combo box dynamically with 4 numeric values + "Personalitzar..."
            var comboChoices = comboValues.Select(v => (v.ToString(), FormatMinutes(v))).ToList();
            comboChoices.Add(("custom", customizeText));

            builder.AddComboBox("reminderTime", _localizationService.GetString("Notification_ReminderPlaceholder"), defaultSelectionId, comboChoices.ToArray())
                .AddButton(CreateButton(continueText, "continue", record.Id, "check"))
                .AddButton(CreateButton(changeText, "change", record.Id, "arrowleftright"))
                .AddButton(CreateButton(stopText, "stop", record.Id, "x"))
                .Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NotificationService] ShowNotificationAsync error: {ex}");
            // Silently handle notification errors
        }
    }

    /// <summary>
    /// Gets the absolute path to the application logo.
    /// </summary>
    private static string? GetLogoPath()
    {
        try
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var appDirectory = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(appDirectory)) return null;

            var logoPath = Path.Combine(appDirectory, "Resources", "Logo.ico");
            return File.Exists(logoPath) ? logoPath : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a toast button with the specified content, action arguments, and optional icon.
    /// </summary>
    private static ToastButton CreateButton(string content, string action, Guid recordId, string iconName)
    {
        var button = new ToastButton()
            .SetContent(content)
            .AddArgument("action", action)
            .AddArgument("recordId", recordId.ToString());

        var iconPath = GetIconPath(iconName);
        if (!string.IsNullOrEmpty(iconPath))
        {
            button.SetImageUri(new Uri(iconPath));
        }

        return button;
    }

    /// <summary>
    /// Gets the absolute path to a notification button icon.
    /// </summary>
    private static string? GetIconPath(string iconName)
    {
        try
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var appDirectory = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(appDirectory)) return null;

            var iconPath = Path.Combine(appDirectory, "Resources", "Notification", $"{iconName}.png");
            return File.Exists(iconPath) ? iconPath : null;
        }
        catch
        {
            return null;
        }
    }

    private void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        var args = ToastArguments.Parse(e.Argument);

        if (!args.TryGetValue("action", out string? action)) return;

        switch (action)
        {
            case "continue":
                HandleContinue(e);
                break;

            case "change":
                if (args.TryGetValue("recordId", out string? recordIdStr) &&
                    Guid.TryParse(recordIdStr, out var recordId))
                {
                    OnChangeActivity?.Invoke(this, recordId);
                }
                break;

            case "stop":
                OnStopActivity?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    /// <summary>
    /// Handles the "Continue" action, reading the dropdown selection to determine
    /// the next reminder time in minutes. When "Personalitzar..." is selected,
    /// opens the custom dialog.
    /// </summary>
    private void HandleContinue(ToastNotificationActivatedEventArgsCompat e)
    {
        if (e.UserInput.TryGetValue("reminderTime", out var selection) &&
            selection != null &&
            selection.ToString() == "custom")
        {
            HandleCustom();
            return;
        }

        int minutes = 0;

        if (selection != null && int.TryParse(selection.ToString(), out var parsed))
        {
            minutes = parsed;
        }

        _nextReminderMinutes = minutes;
        _snoozeUntil = null;
        _lastNotificationTime = DateTime.Now;
        OnStateChanged();

        if (minutes > 0)
            OnSnooze?.Invoke(this, minutes);

        OnContinueActivity?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Handles the "Customize" action by opening the reminder management dialog.
    /// Notifications are suppressed while the dialog is open so the reminder does
    /// not pop up again until the user has finished.
    /// </summary>
    private void HandleCustom()
    {
        ShowCustomReminderDialog();
    }

    /// <summary>
    /// Opens the custom reminder dialog so the user can manage notification
    /// settings and schedule the next reminder. Applies any changes made.
    /// </summary>
    public void ShowCustomReminderDialog()
    {
        if (_isCustomReminderDialogOpen)
            return;

        var currentSettings = ReadCurrentSettings();
        int defaultMinutes = currentSettings?.NotificationIntervalMinutes ?? 120;
        bool notificationsEnabled = currentSettings?.Notifications ?? false;
        bool keepVisible = currentSettings?.KeepNotificationsVisible ?? false;

        // Reset the reference time before showing the dialog so the timer does not
        // treat the time spent in the dialog as elapsed reminder time.
        _lastNotificationTime = DateTime.Now;
        _isCustomReminderDialogOpen = true;

        ReminderDialogResult? result;
        try
        {
            result = ShowReminderDialog(defaultMinutes, notificationsEnabled, keepVisible);
        }
        finally
        {
            _isCustomReminderDialogOpen = false;
        }

        if (result == null)
        {
            // Cancelled: fall back to settings default
            _nextReminderMinutes = 0;
            _snoozeUntil = null;
            _lastNotificationTime = DateTime.Now;
            return;
        }

        ApplyReminderDialogResult(result);
    }

    /// <summary>
    /// Applies the dialog result to the notification settings and internal state.
    /// </summary>
    private void ApplyReminderDialogResult(ReminderDialogResult result)
    {
        // Persist the keep-visible setting when it changed.
        if (TryUpdateKeepNotificationsVisible(result.KeepNotificationsVisible))
        {
            OnStateChanged();
        }

        // Sync the notification enabled state.
        if (_isEnabled != result.NotificationsEnabled)
        {
            IsEnabled = result.NotificationsEnabled;
        }

        if (!result.NotificationsEnabled)
        {
            _nextReminderMinutes = 0;
            _snoozeUntil = null;
            _lastNotificationTime = DateTime.Now;
            return;
        }

        if (result.CustomMinutes.HasValue)
        {
            _nextReminderMinutes = result.CustomMinutes.Value;
            _snoozeUntil = null;
            _lastNotificationTime = DateTime.Now;
            OnSnooze?.Invoke(this, result.CustomMinutes.Value);
            OnContinueActivity?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _nextReminderMinutes = 0;
            _snoozeUntil = null;
            _lastNotificationTime = DateTime.Now;
        }

        OnStateChanged();
    }

    /// <summary>
    /// Updates the KeepNotificationsVisible setting in the database if it changed.
    /// Returns true if the value was updated; otherwise false.
    /// </summary>
    private bool TryUpdateKeepNotificationsVisible(bool keepVisible)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
            var settings = settingsRepository.GetAsync().GetAwaiter().GetResult();
            if (settings.KeepNotificationsVisible == keepVisible)
                return false;

            settings.KeepNotificationsVisible = keepVisible;
            settingsRepository.UpdateAsync(settings).GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads the current application settings from the database.
    /// </summary>
    private AppSettings? ReadCurrentSettings()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
            return settingsRepository.GetAsync().GetAwaiter().GetResult();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Opens a modal WPF dialog to collect the reminder configuration.
    /// Returns null if the user cancels.
    /// </summary>
    private ReminderDialogResult? ShowReminderDialog(int defaultMinutes, bool notificationsEnabled, bool keepVisible)
    {
        ReminderDialogResult? result = null;

        void ShowDialog()
        {
            var dialog = new ReminderCustomDialogWindow(
                _serviceProvider,
                defaultMinutes,
                notificationsEnabled,
                keepVisible)
            {
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };
            dialog.ShowDialog();
            result = dialog.Result;
        }

        if (Application.Current?.Dispatcher?.CheckAccess() == false)
            Application.Current.Dispatcher.Invoke(ShowDialog);
        else
            ShowDialog();

        return result;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        Stop();

        try
        {
            ToastNotificationManagerCompat.OnActivated -= OnToastActivated;
            ToastNotificationManagerCompat.Uninstall();
        }
        catch
        {
            // Ignore errors during cleanup
        }

        GC.SuppressFinalize(this);
    }
}
