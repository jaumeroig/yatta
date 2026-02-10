namespace TimeTracker.Core.Models;

/// <summary>
/// Defines the application configuration.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Configuration identifier. Will always be 1 (singleton).
    /// </summary>
    public int Id { get; set; } = 1;

    /// <summary>
    /// Application theme.
    /// </summary>
    public Theme Theme { get; set; } = Theme.System;

    /// <summary>
    /// Indicates if notifications are enabled.
    /// </summary>
    public bool Notifications { get; set; }

    /// <summary>
    /// Total work time of a workday (default 8 hours).
    /// </summary>
    public TimeSpan WorkdayTotalTime { get; set; } = TimeSpan.FromHours(8);

    /// <summary>
    /// Application language (culture code like "es-ES", "ca-ES"). 
    /// If null, the system language is used.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Indicates if the application should minimize to tray when closing the window.
    /// If false, closing the window will exit the application.
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Interval in minutes between notification reminders when there is an active time record.
    /// Default is 120 minutes (2 hours).
    /// </summary>
    public int NotificationIntervalMinutes { get; set; } = 120;

    /// <summary>
    /// Indicates if the application should start automatically when Windows starts.
    /// </summary>
    public bool StartWithWindows { get; set; } = false;

    /// <summary>
    /// Data retention policy.
    /// </summary>
    public RetentionPolicy RetentionPolicy { get; set; } = RetentionPolicy.Forever;

    /// <summary>
    /// Custom retention period in days. Only used when RetentionPolicy is Custom.
    /// </summary>
    public int CustomRetentionDays { get; set; } = 365;

    /// <summary>
    /// Global hotkey combination to open the change activity dialog.
    /// Format: "Modifiers+Key" (e.g., "Control+Alt+A").
    /// If null or empty, the default hotkey is used.
    /// </summary>
    public string? GlobalHotkey { get; set; }

    /// <summary>
    /// Indicates if the historic records should be sorted in ascending order (oldest first).
    /// If false (default), records are sorted in descending order (newest first).
    /// </summary>
    public bool HistoricSortAscending { get; set; } = false;
}
