namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Repository to manage application configuration.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    Task<AppSettings> GetAsync();

    /// <summary>
    /// Updates the application configuration.
    /// </summary>
    Task UpdateAsync(AppSettings settings);
}
