namespace TimeTracker.App.Services;

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Core.Interfaces;
using TimeTracker.App.Resources;

/// <summary>
/// Servei de localització/internacionalització.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IServiceProvider _serviceProvider;
    private CultureInfo _currentCulture;

    public event EventHandler? CultureChanged;

    public LocalizationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // Inicialitzar amb idioma del sistema per defecte
        _currentCulture = GetSystemCulture();
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
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    public string GetCurrentCulture()
    {
        return _currentCulture.Name;
    }

    /// <summary>
    /// Inicialitza el servei de localització carregant l'idioma guardat.
    /// </summary>
    public async Task InitializeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
        
        try
        {
            var settings = await settingsRepository.GetAsync();
            
            if (settings != null && !string.IsNullOrEmpty(settings.Language))
            {
                var savedCulture = new CultureInfo(settings.Language);
                _currentCulture = savedCulture;
                ApplyCulture(_currentCulture);
            }
        }
        catch
        {
            // Si hi ha error, mantenir idioma del sistema
        }
    }

    private static CultureInfo GetSystemCulture()
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
}
