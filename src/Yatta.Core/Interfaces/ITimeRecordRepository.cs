namespace Yatta.Core.Interfaces;

using Yatta.Core.Models;

/// <summary>
/// Repository to manage time records.
/// </summary>
public interface ITimeRecordRepository
{
    /// <summary>
    /// Gets all time records.
    /// </summary>
    Task<IEnumerable<TimeRecord>> GetAllAsync();

    /// <summary>
    /// Gets a time record by identifier.
    /// </summary>
    Task<TimeRecord?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets time records for a specific date.
    /// </summary>
    Task<IEnumerable<TimeRecord>> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Gets time records for a date range.
    /// </summary>
    Task<IEnumerable<TimeRecord>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Gets time records for a specific activity.
    /// </summary>
    Task<IEnumerable<TimeRecord>> GetByActivityIdAsync(Guid activityId);

    /// <summary>
    /// Gets the currently active time record (EndTime is null).
    /// Returns the most recent active record if multiple exist.
    /// </summary>
    Task<TimeRecord?> GetActiveAsync();

    /// <summary>
    /// Adds a new time record.
    /// </summary>
    Task<TimeRecord> AddAsync(TimeRecord timeRecord);

    /// <summary>
    /// Updates an existing time record.
    /// </summary>
    Task UpdateAsync(TimeRecord timeRecord);

    /// <summary>
    /// Deletes a time record.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Counts time records before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date (exclusive).</param>
    /// <returns>The number of records before the date.</returns>
    Task<int> CountBeforeDateAsync(DateOnly date);

    /// <summary>
    /// Deletes all time records before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date (exclusive).</param>
    /// <returns>The number of deleted records.</returns>
    Task<int> DeleteBeforeDateAsync(DateOnly date);
}
