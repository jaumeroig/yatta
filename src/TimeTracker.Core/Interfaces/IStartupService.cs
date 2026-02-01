namespace TimeTracker.Core.Interfaces;

/// <summary>
/// Service to manage Windows startup configuration.
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Enables the application to start automatically when Windows starts.
    /// </summary>
    void EnableStartup();

    /// <summary>
    /// Disables the application from starting automatically when Windows starts.
    /// </summary>
    void DisableStartup();

    /// <summary>
    /// Checks if the application is configured to start with Windows.
    /// </summary>
    /// <returns>True if startup is enabled, false otherwise.</returns>
    bool IsStartupEnabled();
}
