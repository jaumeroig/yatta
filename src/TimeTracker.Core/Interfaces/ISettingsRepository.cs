namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Repositori per gestionar la configuració de l'aplicació.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Obté la configuració de l'aplicació.
    /// </summary>
    Task<AppSettings> GetAsync();

    /// <summary>
    /// Actualitza la configuració de l'aplicació.
    /// </summary>
    Task UpdateAsync(AppSettings settings);
}
