namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Service for workday-specific logic.
/// </summary>
public interface IWorkdayService
{
    /// <summary>
    /// Gets the daily summary of a workday.
    /// </summary>
    /// <param name="date">Date of the workday.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Daily summary with total hours, telework and office.</returns>
    Task<WorkdaySummary> GetDailySummaryAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a new work slot can be added to a workday.
    /// </summary>
    /// <param name="slot">Work slot to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if it can be added, false otherwise.</returns>
    Task<bool> CanAddWorkdaySlotAsync(WorkdaySlot slot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a new work slot can be added to a workday with error message.
    /// </summary>
    /// <param name="slot">Work slot to add.</param>
    /// <param name="errorMessage">Error message if it cannot be added.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if it can be added, false otherwise.</returns>
    Task<(bool IsValid, string ErrorMessage)> ValidateWorkdaySlotAsync(WorkdaySlot slot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total hours worked in a date range.
    /// </summary>
    /// <param name="startDate">Start date.</param>
    /// <param name="endDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total hours worked.</returns>
    Task<double> GetTotalHoursAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the telework percentage in a date range.
    /// </summary>
    /// <param name="startDate">Start date.</param>
    /// <param name="endDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Telework percentage (0-100).</returns>
    Task<double> GetTeleworkPercentageAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Daily summary of a workday.
/// </summary>
public class WorkdaySummary
{
    /// <summary>
    /// Date of the workday.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Total hours worked.
    /// </summary>
    public double TotalHours { get; set; }

    /// <summary>
    /// Telework hours.
    /// </summary>
    public double TeleworkHours { get; set; }

    /// <summary>
    /// Office hours.
    /// </summary>
    public double OfficeHours { get; set; }

    /// <summary>
    /// Telework percentage.
    /// </summary>
    public double TeleworkPercentage { get; set; }

    /// <summary>
    /// Number of work slots.
    /// </summary>
    public int SlotCount { get; set; }
}
