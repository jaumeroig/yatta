namespace TimeTracker.Core.Interfaces;

/// <summary>
/// Service to manage toast notifications for time tracking reminders.
/// </summary>
public interface INotificationService : IDisposable
{
    /// <summary>
    /// Starts the notification service with periodic checks.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the notification service.
    /// </summary>
    void Stop();

    /// <summary>
    /// Gets or sets whether notifications are enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Triggers a notification check immediately.
    /// </summary>
    Task CheckAndNotifyAsync();

    /// <summary>
    /// Forces showing a notification immediately (for testing purposes).
    /// </summary>
    Task ForceShowNotificationAsync();

    /// <summary>
    /// Event raised when user responds to continue with current activity.
    /// </summary>
    event EventHandler? OnContinueActivity;

    /// <summary>
    /// Event raised when user wants to change activity (should open app and navigate to edit).
    /// The Guid parameter is the ID of the active time record.
    /// </summary>
    event EventHandler<Guid>? OnChangeActivity;

    /// <summary>
    /// Event raised when user snoozes the notification.
    /// The int parameter is the number of minutes to snooze.
    /// </summary>
    event EventHandler<int>? OnSnooze;
}
