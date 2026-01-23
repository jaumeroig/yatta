namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Servei per al càlcul de durades, totals diaris i percentatges.
/// </summary>
public interface ITimeCalculatorService
{
    /// <summary>
    /// Calcula la durada en hores entre dues hores.
    /// </summary>
    /// <param name="startTime">Hora d'inici.</param>
    /// <param name="endTime">Hora de fi.</param>
    /// <returns>Durada en hores.</returns>
    double CalculateDuration(TimeOnly startTime, TimeOnly endTime);

    /// <summary>
    /// Calcula el total d'hores d'una llista de registres.
    /// </summary>
    /// <param name="records">Llista de registres de temps.</param>
    /// <returns>Total d'hores.</returns>
    double CalculateTotalHours(IEnumerable<TimeRecord> records);

    /// <summary>
    /// Calcula el total d'hores d'una llista de franges de treball.
    /// </summary>
    /// <param name="slots">Llista de franges de treball.</param>
    /// <returns>Total d'hores.</returns>
    double CalculateTotalHours(IEnumerable<WorkdaySlot> slots);

    /// <summary>
    /// Calcula el percentatge de teletreball sobre el total d'hores.
    /// </summary>
    /// <param name="slots">Llista de franges de treball.</param>
    /// <returns>Percentatge de teletreball (0-100).</returns>
    double CalculateTeleworkPercentage(IEnumerable<WorkdaySlot> slots);

    /// <summary>
    /// Calcula les hores totals de teletreball.
    /// </summary>
    /// <param name="slots">Llista de franges de treball.</param>
    /// <returns>Total d'hores de teletreball.</returns>
    double CalculateTeleworkHours(IEnumerable<WorkdaySlot> slots);

    /// <summary>
    /// Calcula les hores totals de treball a l'oficina.
    /// </summary>
    /// <param name="slots">Llista de franges de treball.</param>
    /// <returns>Total d'hores a l'oficina.</returns>
    double CalculateOfficeHours(IEnumerable<WorkdaySlot> slots);
}
