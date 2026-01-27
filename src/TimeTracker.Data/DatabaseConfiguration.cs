namespace TimeTracker.Data;

/// <summary>
/// Database configuration.
/// </summary>
public static class DatabaseConfiguration
{
    private const string AppName = "TimeTracker";

    /// <summary>
    /// Gets the database path.
    /// </summary>
    public static string GetDatabasePath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDirectory = Path.Combine(localAppData, AppName);
        
        
        // Create the directory if it doesn't exist
        if (!Directory.Exists(appDirectory))
        {
            Directory.CreateDirectory(appDirectory);
        }

        return Path.Combine(appDirectory, $"{AppName}.db");
    }

    /// <summary>
    /// Gets the connection string for SQLite.
    /// </summary>
    public static string GetConnectionString()
    {
        return $"Data Source={GetDatabasePath()}";
    }
}
