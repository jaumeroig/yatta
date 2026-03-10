namespace Yatta.Data;

/// <summary>
/// Database configuration.
/// </summary>
public static class DatabaseConfiguration
{
    private const string AppName = "Yatta";

    /// <summary>
    /// Gets the database path.
    /// </summary>
    /// <remarks>
    /// Uses retries to handle cases where the user profile is not yet fully loaded
    /// (e.g., when the app starts automatically at Windows login via the Run registry key).
    /// </remarks>
    public static string GetDatabasePath()
    {
        var localAppData = GetLocalAppDataWithRetry();

        if (string.IsNullOrEmpty(localAppData))
            throw new InvalidOperationException(
                "Cannot determine the local application data path. The user profile may not be fully loaded.");

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

    /// <summary>
    /// Attempts to resolve LocalApplicationData up to 5 times with a 1-second delay between
    /// retries, to handle cases where the profile is still being initialized at login time.
    /// </summary>
    private static string GetLocalAppDataWithRetry()
    {
        const int maxRetries = 5;
        const int delayMs = 1000;

        for (var i = 0; i < maxRetries; i++)
        {
            var path = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.Create);

            if (!string.IsNullOrEmpty(path))
                return path;

            if (i < maxRetries - 1)
                Thread.Sleep(delayMs);
        }

        return string.Empty;
    }
}
