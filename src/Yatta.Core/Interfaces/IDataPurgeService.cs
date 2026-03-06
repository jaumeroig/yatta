namespace Yatta.Core.Interfaces;

using Yatta.Core.Models;

/// <summary>
/// Service for purging old data based on retention policy.
/// </summary>
public interface IDataPurgeService
{
    /// <summary>
    /// Calculates the cutoff date based on the retention policy.
    /// Returns null if the policy is Forever (no purge needed).
    /// </summary>
    /// <param name="policy">The retention policy.</param>
    /// <param name="customDays">Custom retention days (used when policy is Custom).</param>
    /// <param name="referenceDate">The reference date for calculation (defaults to today).</param>
    /// <returns>The cutoff date, or null if no purge is needed.</returns>
    DateOnly? CalculateCutoffDate(RetentionPolicy policy, int customDays, DateOnly? referenceDate = null);

    /// <summary>
    /// Gets the count of records that would be affected by a purge.
    /// </summary>
    /// <param name="cutoffDate">Records before this date will be counted.</param>
    /// <returns>A tuple with the count of time records and workdays that would be purged.</returns>
    Task<(int TimeRecordCount, int WorkdayCount)> GetPurgeableCountAsync(DateOnly cutoffDate);

    /// <summary>
    /// Executes the purge, deleting records before the specified cutoff date.
    /// </summary>
    /// <param name="cutoffDate">Records before this date will be deleted.</param>
    /// <returns>A tuple with the count of time records and workdays that were deleted.</returns>
    Task<(int TimeRecordsDeleted, int WorkdaysDeleted)> ExecutePurgeAsync(DateOnly cutoffDate);
}
