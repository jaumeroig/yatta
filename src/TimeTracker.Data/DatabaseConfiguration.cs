namespace TimeTracker.Data;

/// <summary>
/// Configuració de la base de dades.
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Obté la ruta de la base de dades.
    /// </summary>
    public static string GetDatabasePath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDirectory = Path.Combine(localAppData, "TimeTracker");
        
        // Crear el directori si no existeix
        if (!Directory.Exists(appDirectory))
        {
            Directory.CreateDirectory(appDirectory);
        }

        return Path.Combine(appDirectory, "TimeTracker.db");
    }

    /// <summary>
    /// Obté la cadena de connexió per SQLite.
    /// </summary>
    public static string GetConnectionString()
    {
        return $"Data Source={GetDatabasePath()}";
    }
}
