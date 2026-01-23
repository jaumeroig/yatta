namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Repositori per gestionar registres de temps.
/// </summary>
public interface ITimeRecordRepository
{
    /// <summary>
    /// Obté tots els registres de temps.
    /// </summary>
    Task<IEnumerable<TimeRecord>> GetAllAsync();

    /// <summary>
    /// Obté un registre de temps per identificador.
    /// </summary>
    Task<TimeRecord?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obté els registres de temps d'una data específica.
    /// </summary>
    Task<IEnumerable<TimeRecord>> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Obté els registres de temps d'un rang de dates.
    /// </summary>
    Task<IEnumerable<TimeRecord>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Obté els registres de temps d'una activitat específica.
    /// </summary>
    Task<IEnumerable<TimeRecord>> GetByActivityIdAsync(Guid activityId);

    /// <summary>
    /// Afegeix un nou registre de temps.
    /// </summary>
    Task<TimeRecord> AddAsync(TimeRecord timeRecord);

    /// <summary>
    /// Actualitza un registre de temps existent.
    /// </summary>
    Task UpdateAsync(TimeRecord timeRecord);

    /// <summary>
    /// Elimina un registre de temps.
    /// </summary>
    Task DeleteAsync(Guid id);
}
