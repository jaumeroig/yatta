namespace Yatta.Core.Interfaces;

using Yatta.Core.Models;

/// <summary>
/// Repository to manage workday configurations.
/// </summary>
public interface IWorkdayRepository
{
    /// <summary>
    /// Gets the workday configuration for a specific date.
    /// </summary>
    /// <param name="date">The date to get configuration for.</param>
    /// <returns>The workday configuration if exists, null otherwise.</returns>
    Task<Workday?> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Gets workday configurations for a date range.
    /// </summary>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <returns>Collection of workday configurations within the range.</returns>
    Task<IEnumerable<Workday>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Gets workday configurations filtered by day type within a date range.
    /// </summary>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <param name="dayType">The day type to filter by.</param>
    /// <returns>Collection of workday configurations matching the criteria.</returns>
    Task<IEnumerable<Workday>> GetByDayTypeAsync(DateOnly startDate, DateOnly endDate, DayType dayType);

    /// <summary>
    /// Counts the number of days of each type within a date range.
    /// </summary>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <returns>Dictionary with day types and their counts.</returns>
    Task<Dictionary<DayType, int>> GetDayTypeCountsAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Adds or updates a workday configuration.
    /// If a configuration for the date already exists, it will be updated.
    /// </summary>
    /// <param name="workday">The workday configuration to save.</param>
    /// <returns>The saved workday configuration.</returns>
    Task<Workday> SaveAsync(Workday workday);

    /// <summary>
    /// Deletes the workday configuration for a specific date.
    /// </summary>
    /// <param name="date">The date to delete configuration for.</param>
    Task DeleteAsync(DateOnly date);

    /// <summary>
    /// Counts workday configurations before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date (exclusive).</param>
    /// <returns>The number of workdays before the date.</returns>
    Task<int> CountBeforeDateAsync(DateOnly date);

    /// <summary>
    /// Deletes all workday configurations before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date (exclusive).</param>
    /// <returns>The number of deleted workdays.</returns>
    Task<int> DeleteBeforeDateAsync(DateOnly date);
}
