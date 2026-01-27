namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Service for calculating durations, daily totals, and percentages.
/// </summary>
public interface ITimeCalculatorService
{
    /// <summary>
    /// Calculates the duration in hours between two times.
    /// </summary>
    /// <param name="startTime">Start time.</param>
    /// <param name="endTime">End time.</param>
    /// <returns>Duration in hours.</returns>
    double CalculateDuration(TimeOnly startTime, TimeOnly endTime);

    /// <summary>
    /// Calculates the total hours from a list of records.
    /// </summary>
    /// <param name="records">List of time records.</param>
    /// <returns>Total hours.</returns>
    double CalculateTotalHours(IEnumerable<TimeRecord> records);

    /// <summary>
    /// Calculates the total hours from a list of work slots.
    /// </summary>
    /// <param name="slots">List of work slots.</param>
    /// <returns>Total hours.</returns>
    double CalculateTotalHours(IEnumerable<WorkdaySlot> slots);

    /// <summary>
    /// Calculates the telework percentage over total hours.
    /// </summary>
    /// <param name="slots">List of work slots.</param>
    /// <returns>Telework percentage (0-100).</returns>
    double CalculateTeleworkPercentage(IEnumerable<WorkdaySlot> slots);

    /// <summary>
    /// Calculates the total telework hours.
    /// </summary>
    /// <param name="slots">List of work slots.</param>
    /// <returns>Total telework hours.</returns>
    double CalculateTeleworkHours(IEnumerable<WorkdaySlot> slots);

    /// <summary>
    /// Calculates the total office work hours.
    /// </summary>
    /// <param name="slots">List of work slots.</param>
    /// <returns>Total office hours.</returns>
    double CalculateOfficeHours(IEnumerable<WorkdaySlot> slots);
}
