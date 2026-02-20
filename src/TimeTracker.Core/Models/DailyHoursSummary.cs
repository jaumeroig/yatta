namespace TimeTracker.Core.Models;

/// <summary>
/// Summary of hours worked for a single day, split by location.
/// Used in period reports (week/month/year) for daily breakdown charts.
/// </summary>
public class DailyHoursSummary
{
    /// <summary>
    /// The date of this summary.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Total office hours worked.
    /// </summary>
    public double OfficeHours { get; set; }

    /// <summary>
    /// Total telework hours worked.
    /// </summary>
    public double TeleworkHours { get; set; }

    /// <summary>
    /// Total hours worked (office + telework).
    /// </summary>
    public double TotalHours { get; set; }
}
