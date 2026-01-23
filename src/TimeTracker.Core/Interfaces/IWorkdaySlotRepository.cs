namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Repositori per gestionar franges de jornada.
/// </summary>
public interface IWorkdaySlotRepository
{
    /// <summary>
    /// Obté totes les franges de jornada.
    /// </summary>
    Task<IEnumerable<WorkdaySlot>> GetAllAsync();

    /// <summary>
    /// Obté una franja de jornada per identificador.
    /// </summary>
    Task<WorkdaySlot?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obté les franges de jornada d'una data específica.
    /// </summary>
    Task<IEnumerable<WorkdaySlot>> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Obté les franges de jornada d'un rang de dates.
    /// </summary>
    Task<IEnumerable<WorkdaySlot>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Afegeix una nova franja de jornada.
    /// </summary>
    Task<WorkdaySlot> AddAsync(WorkdaySlot workdaySlot);

    /// <summary>
    /// Actualitza una franja de jornada existent.
    /// </summary>
    Task UpdateAsync(WorkdaySlot workdaySlot);

    /// <summary>
    /// Elimina una franja de jornada.
    /// </summary>
    Task DeleteAsync(Guid id);
}
