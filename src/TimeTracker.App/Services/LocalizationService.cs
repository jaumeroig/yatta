namespace TimeTracker.App.Services;

using System.Globalization;
using TimeTracker.Core.Interfaces;
using TimeTracker.App.Resources;

/// <summary>
/// Servei de localització/internacionalització.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ISettingsRepository _settingsRepository;
    private CultureInfo _currentCulture;

    public event EventHandler? CultureChanged;

    public LocalizationService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
        
        // Carregar preferència d'idioma o usar idioma del sistema
        _currentCulture = LoadSavedCulture();
        ApplyCulture(_currentCulture);
    }

    /// <inheritdoc/>
    public string GetString(string key)
    {
        return Resources.ResourceManager.GetString(key, _currentCulture) ?? key;
    }

    /// <inheritdoc/>
    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        try
        {
            return string.Format(_currentCulture, format, args);
        }
        catch
        {
            return format;
        }
    }

    /// <inheritdoc/>
    public void SetCulture(string? culture)
    {
        CultureInfo newCulture;
        
        if (string.IsNullOrEmpty(culture))
        {
            // Usar idioma del sistema
            newCulture = GetSystemCulture();
        }
        else
        {
            try
            {
                newCulture = new CultureInfo(culture);
            }
            catch
            {
                // Si la cultura no és vàlida, usar espanyol per defecte
                newCulture = new CultureInfo("es-ES");
            }
        }

        if (!newCulture.Equals(_currentCulture))
        {
            _currentCulture = newCulture;
            ApplyCulture(_currentCulture);
            SaveCulture(culture);
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    public string GetCurrentCulture()
    {
        return _currentCulture.Name;
    }

    private CultureInfo LoadSavedCulture()
    {
        try
        {
            var settings = _settingsRepository.GetAsync().GetAwaiter().GetResult();
            
            if (settings != null && !string.IsNullOrEmpty(settings.Language))
            {
                return new CultureInfo(settings.Language);
            }
        }
        catch
        {
            // Si hi ha error, usar idioma del sistema
        }

        return GetSystemCulture();
    }

    private CultureInfo GetSystemCulture()
    {
        var systemCulture = CultureInfo.CurrentUICulture;
        
        // Si el sistema és en espanyol o català, usar-lo
        if (systemCulture.Name.StartsWith("es") || systemCulture.Name.StartsWith("ca"))
        {
            return systemCulture;
        }
        
        // Per defecte, usar espanyol
        return new CultureInfo("es-ES");
    }

    private void ApplyCulture(CultureInfo culture)
    {
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        
        // Actualitzar el ResourceManager perquè usi la nova cultura
        Resources.Culture = culture;
    }

    private void SaveCulture(string? culture)
    {
        try
        {
            var settings = _settingsRepository.GetAsync().GetAwaiter().GetResult();
            if (settings != null)
            {
                settings.Language = culture;
                _settingsRepository.UpdateAsync(settings).GetAwaiter().GetResult();
            }
        }
        catch
        {
            // Si hi ha error guardant, no fer res
        }
    }
}
