namespace Yatta.Core.Interfaces;

using Yatta.Core.Models;

/// <summary>
/// Repository for managing annual quota configurations.
/// </summary>
public interface IAnnualQuotaRepository
{
    /// <summary>
    /// Gets the annual quota configuration for a specific year.
    /// </summary>
    /// <param name="year">The year to get the quota for.</param>
    /// <returns>The annual quota if it exists; otherwise null.</returns>
    Task<AnnualQuota?> GetByYearAsync(int year);

    /// <summary>
    /// Saves an annual quota configuration (insert or update).
    /// </summary>
    /// <param name="quota">The quota to save.</param>
    /// <returns>The saved quota.</returns>
    Task<AnnualQuota> SaveAsync(AnnualQuota quota);

    /// <summary>
    /// Deletes the annual quota configuration for a specific year.
    /// </summary>
    /// <param name="year">The year to delete the quota for.</param>
    Task DeleteAsync(int year);
}
