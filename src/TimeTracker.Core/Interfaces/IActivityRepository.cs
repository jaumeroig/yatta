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
    /// Obté una activitat pel seu nom (cerca case-insensitive).
    /// </summary>
    /// <param name="name">Nom de l'activitat a cercar.</param>
    /// <returns>L'activitat si existeix, null en cas contrari.</returns>
    Task<Activity?> GetByNameAsync(string name);

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
