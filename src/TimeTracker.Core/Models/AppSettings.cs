namespace TimeTracker.Core.Models;

/// <summary>
/// Defineix la configuració de l'aplicació.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Tema de l'aplicació.
    /// </summary>
    public Theme Theme { get; set; } = Theme.System;

    /// <summary>
    /// Indica si les notificacions estan activades.
    /// </summary>
    public bool Notifications { get; set; }

    /// <summary>
    /// Temps total de treball d'una jornada (per defecte 8 hores).
    /// </summary>
    public TimeSpan WorkdayTotalTime { get; set; } = TimeSpan.FromHours(8);
}
