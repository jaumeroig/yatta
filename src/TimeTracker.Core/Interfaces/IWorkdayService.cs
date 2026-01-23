namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Servei per a la lògica específica de jornada laboral.
/// </summary>
public interface IWorkdayService
{
    /// <summary>
    /// Obté el resum diari d'una jornada laboral.
    /// </summary>
    /// <param name="date">Data de la jornada.</param>
    /// <param name="cancellationToken">Token de cancel·lació.</param>
    /// <returns>Resum diari amb hores totals, teletreball i oficina.</returns>
    Task<WorkdaySummary> GetDailySummaryAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si es pot afegir una nova franja de treball a una jornada.
    /// </summary>
    /// <param name="slot">Franja de treball a afegir.</param>
    /// <param name="cancellationToken">Token de cancel·lació.</param>
    /// <returns>True si es pot afegir, false en cas contrari.</returns>
    Task<bool> CanAddWorkdaySlotAsync(WorkdaySlot slot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si es pot afegir una nova franja de treball a una jornada amb missatge d'error.
    /// </summary>
    /// <param name="slot">Franja de treball a afegir.</param>
    /// <param name="errorMessage">Missatge d'error si no es pot afegir.</param>
    /// <param name="cancellationToken">Token de cancel·lació.</param>
    /// <returns>True si es pot afegir, false en cas contrari.</returns>
    Task<(bool IsValid, string ErrorMessage)> ValidateWorkdaySlotAsync(WorkdaySlot slot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obté les hores totals treballades en un rang de dates.
    /// </summary>
    /// <param name="startDate">Data d'inici.</param>
    /// <param name="endDate">Data de fi.</param>
    /// <param name="cancellationToken">Token de cancel·lació.</param>
    /// <returns>Total d'hores treballades.</returns>
    Task<double> GetTotalHoursAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obté el percentatge de teletreball en un rang de dates.
    /// </summary>
    /// <param name="startDate">Data d'inici.</param>
    /// <param name="endDate">Data de fi.</param>
    /// <param name="cancellationToken">Token de cancel·lació.</param>
    /// <returns>Percentatge de teletreball (0-100).</returns>
    Task<double> GetTeleworkPercentageAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resum diari d'una jornada laboral.
/// </summary>
public class WorkdaySummary
{
    /// <summary>
    /// Data de la jornada.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Total d'hores treballades.
    /// </summary>
    public double TotalHours { get; set; }

    /// <summary>
    /// Hores de teletreball.
    /// </summary>
    public double TeleworkHours { get; set; }

    /// <summary>
    /// Hores a l'oficina.
    /// </summary>
    public double OfficeHours { get; set; }

    /// <summary>
    /// Percentatge de teletreball.
    /// </summary>
    public double TeleworkPercentage { get; set; }

    /// <summary>
    /// Nombre de franges de treball.
    /// </summary>
    public int SlotCount { get; set; }
}
