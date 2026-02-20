namespace TimeTracker.Core.Models;

/// <summary>
/// Aggregated report for a period (week, month, or year), used in Dashboard views.
/// </summary>
public class PeriodReport
{
    /// <summary>
    /// Start date of the period.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// End date of the period (inclusive).
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Accumulated target duration for the period (sum of all days' targets).
    /// </summary>
    public TimeSpan TotalTarget { get; set; }

    /// <summary>
    /// Total time actually worked in the period.
    /// </summary>
    public TimeSpan TotalWorked { get; set; }

    /// <summary>
    /// Difference between total worked and total target.
    /// Positive means overtime, negative means undertime.
    /// </summary>
    public TimeSpan Differential { get; set; }

    /// <summary>
    /// Total time worked in the office.
    /// </summary>
    public TimeSpan OfficeTime { get; set; }

    /// <summary>
    /// Total time worked remotely.
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
    /// Count of days by type in the period.
    /// </summary>
    public Dictionary<DayType, int> DayTypeCounts { get; set; } = [];

    /// <summary>
    /// Daily breakdown of hours for chart rendering.
    /// </summary>
    public List<DailyHoursSummary> DailyBreakdown { get; set; } = [];
}
