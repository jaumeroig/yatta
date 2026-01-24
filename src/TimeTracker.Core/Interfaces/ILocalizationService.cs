namespace TimeTracker.Core.Interfaces;

/// <summary>
/// Interfície per al servei de localització/internacionalització.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Obté el text traduït per una clau de recurs.
    /// </summary>
    /// <param name="key">Clau del recurs.</param>
    /// <returns>Text traduït.</returns>
    string GetString(string key);

    /// <summary>
    /// Obté el text traduït per una clau de recurs amb format.
    /// </summary>
    /// <param name="key">Clau del recurs.</param>
    /// <param name="args">Arguments per al format.</param>
    /// <returns>Text traduït amb format.</returns>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Canvia l'idioma de l'aplicació.
    /// </summary>
    /// <param name="culture">Codi de cultura (ex: "es-ES", "ca-ES") o null per usar idioma del sistema.</param>
    void SetCulture(string? culture);

    /// <summary>
    /// Obté la cultura actual de l'aplicació.
    /// </summary>
    /// <returns>Codi de cultura actual.</returns>
    string GetCurrentCulture();

    /// <summary>
    /// Event que es dispara quan canvia l'idioma.
    /// </summary>
    event EventHandler? CultureChanged;
}
