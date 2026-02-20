namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Service for generating dashboard reports aggregating time records,
/// activities, and workday configuration for different periods.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Generates a detailed report for a single day.
    /// </summary>
    /// <param name="date">The date to report on.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Day report with all metrics and breakdowns.</returns>
    Task<DayReport> GetDayReportAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an aggregated report for the week containing the given date.
    /// The week runs from Monday to Sunday (ISO 8601).
    /// </summary>
    /// <param name="anyDayInWeek">Any day within the desired week.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Period report for the week.</returns>
    Task<PeriodReport> GetWeekReportAsync(DateOnly anyDayInWeek, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an aggregated report for a specific month.
    /// </summary>
    /// <param name="year">Year.</param>
    /// <param name="month">Month (1-12).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Period report for the month.</returns>
    Task<PeriodReport> GetMonthReportAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an aggregated report for a full year.
    /// </summary>
    /// <param name="year">Year.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Period report for the year.</returns>
    Task<PeriodReport> GetYearReportAsync(int year, CancellationToken cancellationToken = default);
}
