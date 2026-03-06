namespace Yatta.Core.Models;

/// <summary>
/// Detailed report for a single day, used in the Dashboard Day view.
/// </summary>
public class DayReport
{
    /// <summary>
    /// The date of the report.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The type of day (WorkDay, IntensiveDay, Holiday, etc.).
    /// </summary>
    public DayType DayType { get; set; }

    /// <summary>
    /// Start time of the first record of the day, or null if no records.
    /// </summary>
    public TimeOnly? StartTime { get; set; }

    /// <summary>
    /// Target working duration for the day.
    /// </summary>
    public TimeSpan TargetDuration { get; set; }

    /// <summary>
    /// Total time actually worked.
    /// </summary>
    public TimeSpan WorkedDuration { get; set; }

    /// <summary>
    /// Difference between worked and target duration.
    /// Positive means overtime, negative means undertime.
    /// </summary>
    public TimeSpan Differential { get; set; }

    /// <summary>
    /// Time worked in the office.
    /// </summary>
    public TimeSpan OfficeTime { get; set; }

    /// <summary>
    /// Time worked remotely.
    /// </summary>
    public TimeSpan TeleworkTime { get; set; }

    /// <summary>
    /// Telework percentage (0-100).
    /// </summary>
    public double TeleworkPercentage { get; set; }

    /// <summary>
    /// Breakdown of time by activity.
    /// </summary>
    public List<ActivityBreakdownItem> Activities { get; set; } = [];

    /// <summary>
    /// All time records for the day, ordered by start time.
    /// </summary>
    public List<TimeRecord> Records { get; set; } = [];
}
