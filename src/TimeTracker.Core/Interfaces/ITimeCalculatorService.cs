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
    /// Calculates the telework percentage over total hours from time records.
    /// </summary>
    /// <param name="records">List of time records.</param>
    /// <returns>Telework percentage (0-100).</returns>
    double CalculateTeleworkPercentage(IEnumerable<TimeRecord> records);

    /// <summary>
    /// Calculates the total telework hours from time records.
    /// </summary>
    /// <param name="records">List of time records.</param>
    /// <returns>Total telework hours.</returns>
    double CalculateTeleworkHours(IEnumerable<TimeRecord> records);

    /// <summary>
    /// Calculates the total office work hours from time records.
    /// </summary>
    /// <param name="records">List of time records.</param>
    /// <returns>Total office hours.</returns>
    double CalculateOfficeHours(IEnumerable<TimeRecord> records);
}
