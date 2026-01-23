namespace TimeTracker.Core.Models;

/// <summary>
/// Defineix una franja de treball d'una jornada.
/// </summary>
public class WorkdaySlot
{
    /// <summary>
    /// Identificador únic de la franja de treball.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Data de la franja de treball.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Hora d'inici de la franja de treball.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Hora de finalització de la franja de treball.
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Indica si la franja de treball és en teletreball.
    /// </summary>
    public bool Telework { get; set; } = false;
}
