namespace Yatta.App.Models;

using Yatta.Core.Models;

/// <summary>
/// Represents the result of the custom reminder dialog.
/// </summary>
public class ReminderDialogResult
{
    /// <summary>
    /// Gets or sets whether notifications are enabled.
    /// </summary>
    public bool NotificationsEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether notifications should remain visible until manually dismissed.
    /// </summary>
    public bool KeepNotificationsVisible { get; set; }

    /// <summary>
    /// Gets or sets the default reminder interval preset.
    /// </summary>
    public ReminderInterval DefaultReminderInterval { get; set; }

    /// <summary>
    /// Gets or sets the custom default reminder interval in minutes.
    /// </summary>
    public int DefaultReminderMinutes { get; set; }

    /// <summary>
    /// Gets or sets the custom minutes for the next reminder, or null if no
    /// custom time was selected (the standard interval should be used).
    /// </summary>
    public int? CustomMinutes { get; set; }
}
