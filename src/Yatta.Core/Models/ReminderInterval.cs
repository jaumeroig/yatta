namespace Yatta.Core.Models;

/// <summary>
/// Defines the available reminder intervals for notification snooze.
/// </summary>
public enum ReminderInterval
{
    /// <summary>
    /// 5 minutes.
    /// </summary>
    Minutes5,

    /// <summary>
    /// 10 minutes.
    /// </summary>
    Minutes10,

    /// <summary>
    /// 30 minutes.
    /// </summary>
    Minutes30,

    /// <summary>
    /// 1 hour.
    /// </summary>
    Hour1,

    /// <summary>
    /// 2 hours.
    /// </summary>
    Hours2,

    /// <summary>
    /// Until tomorrow at midnight (00:00).
    /// </summary>
    UntilTomorrow,

    /// <summary>
    /// Custom interval defined by the user in minutes.
    /// </summary>
    Custom
}
