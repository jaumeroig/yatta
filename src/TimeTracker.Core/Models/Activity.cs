namespace TimeTracker.Core.Models;

/// <summary>
/// Defineix les diferents activitats que es poden registrar al time tracker.
/// </summary>
public class Activity
{
    /// <summary>
    /// Identificador únic de l'activitat.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nom de l'activitat.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Color associat a l'activitat.
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Indica si l'activitat està activa.
    /// </summary>
    public bool Active { get; set; } = true;
}
