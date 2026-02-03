namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Service for workday configuration business logic.
/// </summary>
public interface IWorkdayConfigService
{
    /// <summary>
    /// Gets the effective workday configuration for a specific date.
    /// If no specific configuration exists, returns a default configuration
    /// based on the application settings.
    /// </summary>
    /// <param name="date">The date to get configuration for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The effective workday configuration.</returns>
    Task<Workday> GetEffectiveConfigurationAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the target duration for a specific date.
    /// Returns TimeSpan.Zero for non-working days (Holiday, FreeChoice, Vacation).
    /// </summary>
    /// <param name="date">The date to get target duration for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The target working duration.</returns>
    Task<TimeSpan> GetTargetDurationAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the day type for a specific date.
    /// </summary>
    /// <param name="date">The date to get day type for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The day type.</returns>
    Task<DayType> GetDayTypeAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a date is a working day (WorkDay or IntensiveDay).
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if it's a working day, false otherwise.</returns>
    Task<bool> IsWorkingDayAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the configuration for a specific date.
    /// </summary>
    /// <param name="date">The date to configure.</param>
    /// <param name="dayType">The type of day.</param>
    /// <param name="targetDuration">The target duration (optional, uses default if null).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved workday configuration.</returns>
    Task<Workday> SetConfigurationAsync(
        DateOnly date,
        DayType dayType,
        TimeSpan? targetDuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specific configuration for a date, reverting to default behavior.
    /// </summary>
    /// <param name="date">The date to reset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResetConfigurationAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of each day type within a date range.
    /// </summary>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary with day types and their counts.</returns>
    Task<Dictionary<DayType, int>> GetDayTypeCountsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the remaining work time for a date based on already worked hours.
    /// Returns TimeSpan.Zero for non-working days.
    /// </summary>
    /// <param name="date">The date to calculate remaining time for.</param>
    /// <param name="workedDuration">The duration already worked.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The remaining work time, or TimeSpan.Zero if target is reached or it's a non-working day.</returns>
    Task<TimeSpan> GetRemainingWorkTimeAsync(
        DateOnly date,
        TimeSpan workedDuration,
        CancellationToken cancellationToken = default);
}
