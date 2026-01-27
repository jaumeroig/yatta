namespace TimeTracker.Core.Models;

/// <summary>
/// Defines a work slot of a workday.
/// </summary>
public class WorkdaySlot
{
    /// <summary>
    /// Unique identifier of the work slot.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Date of the work slot.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Start time of the work slot.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// End time of the work slot.
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Indicates if the work slot is telework.
    /// </summary>
    public bool Telework { get; set; } = false;
}
