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
}
