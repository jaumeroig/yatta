namespace TimeTracker.Core.Models;

/// <summary>
/// Defines each of the time tracker records.
/// </summary>
public class TimeRecord
{
    /// <summary>
    /// Unique identifier of the record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Date of the record.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Start time of the record.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// End time of the record (optional).
    /// </summary>
    public TimeOnly? EndTime { get; set; }

    /// <summary>
    /// Identifier of the associated activity.
    /// </summary>
    public Guid ActivityId { get; set; }

    /// <summary>
    /// Additional notes for the record (optional).
    /// </summary>
    public string? Notes { get; set; }
}
