namespace TimeTracker.Core.Models;

/// <summary>
/// Defineix cada un dels registres del time tracker.
/// </summary>
public class TimeRecord
{
    /// <summary>
    /// Identificador únic del registre.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Data del registre.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Hora d'inici del registre.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Hora de finalització del registre (opcional).
    /// </summary>
    public TimeOnly? EndTime { get; set; }

    /// <summary>
    /// Identificador de l'activitat associada.
    /// </summary>
    public Guid ActivityId { get; set; }

    /// <summary>
    /// Notes addicionals del registre (opcional).
    /// </summary>
    public string? Notes { get; set; }
}
