namespace TimeTracker.App.Services;

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Core.Interfaces;
using TimeTracker.App.Resources;

/// <summary>
/// Localization/internationalization service.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IServiceProvider _serviceProvider;
    private CultureInfo _currentCulture;

    public event EventHandler? CultureChanged;

    public LocalizationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // Initialize with system language by default
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
            // Use system language
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
                // If the culture is not valid, use Spanish by default
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
    /// Initializes the localization service by loading the saved language.
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
            // If there's an error, keep system language
        }
    }

    private static CultureInfo GetSystemCulture()
    {
        var systemCulture = CultureInfo.CurrentUICulture;
        
        // If the system is in Spanish or Catalan, use it
        if (systemCulture.Name.StartsWith("es") || systemCulture.Name.StartsWith("ca"))
        {
            return systemCulture;
        }
        
        // By default, use Spanish
        return new CultureInfo("es-ES");
    }

    private void ApplyCulture(CultureInfo culture)
    {
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        
        // Update the ResourceManager to use the new culture
        Resources.Culture = culture;
    }
}
