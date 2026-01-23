namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Repositori per gestionar activitats.
/// </summary>
public interface IActivityRepository
{
    /// <summary>
    /// Obté totes les activitats.
    /// </summary>
    Task<IEnumerable<Activity>> GetAllAsync();

    /// <summary>
    /// Obté una activitat per identificador.
    /// </summary>
    Task<Activity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obté totes les activitats actives.
    /// </summary>
    Task<IEnumerable<Activity>> GetActiveAsync();

    /// <summary>
    /// Afegeix una nova activitat.
    /// </summary>
    Task<Activity> AddAsync(Activity activity);

    /// <summary>
    /// Actualitza una activitat existent.
    /// </summary>
    Task UpdateAsync(Activity activity);

    /// <summary>
    /// Elimina una activitat.
    /// </summary>
    Task DeleteAsync(Guid id);
}
