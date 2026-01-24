namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Servei per gestionar el tema de l'aplicació.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Obté el tema actual.
    /// </summary>
    Theme GetCurrentTheme();

    /// <summary>
    /// Aplica un tema.
    /// </summary>
    void ApplyTheme(Theme theme);
}
