namespace TimeTracker.App.Helpers;

/// <summary>
/// Helper per accedir als recursos de manera segura des dels ViewModels.
/// </summary>
public static class ResourceHelper
{
    private static System.Resources.ResourceManager? _resourceManager;
    
    private static System.Resources.ResourceManager ResourceManager
    {
        get
        {
            if (_resourceManager == null)
            {
                _resourceManager = new System.Resources.ResourceManager(
                    "TimeTracker.App.Resources.Resources", 
                    typeof(ResourceHelper).Assembly);
            }
            return _resourceManager;
        }
    }
    
    /// <summary>
    /// Obté un string del Resource Manager segons la cultura actual.
    /// </summary>
    /// <param name="key">Clau del recurs.</param>
    /// <param name="fallback">Text per defecte si no es troba el recurs.</param>
    /// <returns>El text traduït o el fallback.</returns>
    public static string GetString(string key, string fallback = "")
    {
        try
        {
            return ResourceManager.GetString(key, System.Globalization.CultureInfo.CurrentUICulture) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
}
