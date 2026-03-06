namespace Yatta.App.Services;

using System.IO;
using System.Reflection;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
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
    private int _snoozeMinutes;
    private bool _isEnabled;
    private bool _isDisposed;

    public event EventHandler? OnContinueActivity;
    public event EventHandler<Guid>? OnChangeActivity;
    public event EventHandler<int>? OnSnooze;

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
        _snoozeMinutes = 0;
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
        _snoozeMinutes = 0;
    }

    public async Task CheckAndNotifyAsync()
    {
        if (!_isEnabled) return;

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
                _snoozeMinutes = 0;
                return;
            }

            var intervalMinutes = settings.NotificationIntervalMinutes;
            var timeSinceLastNotification = DateTime.Now - _lastNotificationTime;

            // Consider snooze time
            var effectiveInterval = intervalMinutes + _snoozeMinutes;

            if (timeSinceLastNotification.TotalMinutes >= effectiveInterval)
            {
                await ShowNotificationAsync(activeRecord);
                _lastNotificationTime = DateTime.Now;
                _snoozeMinutes = 0; // Reset snooze after showing
            }
        }
        catch
        {
            // Silently handle errors to avoid crashing the timer
        }
    }

    public async Task ForceShowNotificationAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var timeRecordRepository = scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>();

            var activeRecord = await timeRecordRepository.GetActiveAsync();
            if (activeRecord != null)
            {
                await ShowNotificationAsync(activeRecord);
            }
        }
        catch
        {
            // Silently handle errors
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
            var snoozeText = _localizationService.GetString("Notification_Snooze");

            // Get the logo path
            var logoPath = GetLogoPath();

            // Get settings to determine notification behavior
            var settings = await settingsRepository.GetAsync();
            var scenario = settings.KeepNotificationsVisible ? ToastScenario.Reminder : ToastScenario.Default;

            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(body)
                .SetToastScenario(scenario);

            // Add logo if available
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                builder.AddAppLogoOverride(new Uri(logoPath), ToastGenericAppLogoCrop.Circle);
            }

            builder.AddButton(new ToastButton()
                    .SetContent(continueText)
                    .AddArgument("action", "continue")
                    .AddArgument("recordId", record.Id.ToString()))
                .AddButton(new ToastButton()
                    .SetContent(changeText)
                    .AddArgument("action", "change")
                    .AddArgument("recordId", record.Id.ToString()))
                .AddComboBox("snoozeTime", snoozeText, "15",
                    ("15", _localizationService.GetString("Notification_15min")),
                    ("30", _localizationService.GetString("Notification_30min")),
                    ("60", _localizationService.GetString("Notification_1hour")),
                    ("120", _localizationService.GetString("Notification_2hours")))
                .AddButton(new ToastButton()
                    .SetContent(snoozeText)
                    .AddArgument("action", "snooze")
                    .AddArgument("recordId", record.Id.ToString()))
                .Show();
        }
        catch
        {
            // Silently handle notification errors
        }
    }

    /// <summary>
    /// Gets the absolute path to the application logo.
    /// </summary>
    private string? GetLogoPath()
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

    private void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        var args = ToastArguments.Parse(e.Argument);

        if (!args.TryGetValue("action", out string? action)) return;

        switch (action)
        {
            case "continue":
                _lastNotificationTime = DateTime.Now; // Reset timer
                _snoozeMinutes = 0;
                OnContinueActivity?.Invoke(this, EventArgs.Empty);
                break;

            case "change":
                if (args.TryGetValue("recordId", out string? recordIdStr) &&
                    Guid.TryParse(recordIdStr, out var recordId))
                {
                    OnChangeActivity?.Invoke(this, recordId);
                }
                break;

            case "snooze":
                if (e.UserInput.TryGetValue("snoozeTime", out var snoozeValue) &&
                    int.TryParse(snoozeValue?.ToString(), out var minutes))
                {
                    _snoozeMinutes = minutes;
                    _lastNotificationTime = DateTime.Now;
                    OnSnooze?.Invoke(this, minutes);
                }
                break;
        }
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
