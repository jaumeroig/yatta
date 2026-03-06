namespace Yatta.Core.Interfaces;

using Yatta.Core.Models;

/// <summary>
/// Repository to manage activities.
/// </summary>
public interface IActivityRepository
{
    /// <summary>
    /// Gets all activities.
    /// </summary>
    Task<IEnumerable<Activity>> GetAllAsync();

    /// <summary>
    /// Gets an activity by identifier.
    /// </summary>
    Task<Activity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all active activities.
    /// </summary>
    Task<IEnumerable<Activity>> GetActiveAsync();

    /// <summary>
    /// Gets an activity by its name (case-insensitive search).
    /// </summary>
    /// <param name="name">Name of the activity to search for.</param>
    /// <returns>The activity if it exists, null otherwise.</returns>
    Task<Activity?> GetByNameAsync(string name);

    /// <summary>
    /// Adds a new activity.
    /// </summary>
    Task<Activity> AddAsync(Activity activity);

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    Task UpdateAsync(Activity activity);

    /// <summary>
    /// Deletes an activity.
    /// </summary>
    Task DeleteAsync(Guid id);
}
