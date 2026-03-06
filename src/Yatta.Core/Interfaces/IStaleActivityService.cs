namespace Yatta.Core.Interfaces;

using Yatta.Core.Models;

/// <summary>
/// Service for detecting and closing stale activities from previous days.
/// </summary>
public interface IStaleActivityService
{
    /// <summary>
    /// Detects active time records from previous days and closes them automatically.
    /// The end time is calculated based on the target duration for that day.
    /// </summary>
    /// <returns>The result of the operation, or null if no stale activity was found.</returns>
    Task<StaleActivityResult?> CloseStaleActivitiesAsync();
}

/// <summary>
/// Result of closing a stale activity.
/// </summary>
public class StaleActivityResult
{
    /// <summary>
    /// The date of the closed record.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The calculated end time assigned to the record.
    /// </summary>
    public TimeOnly EndTime { get; set; }
}
