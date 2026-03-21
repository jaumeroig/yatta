namespace Yatta.Core.Interfaces;

/// <summary>
/// Service for checking and applying application updates.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Gets whether the application is running as an installed (packaged) build.
    /// Returns false during development, so update checks are skipped.
    /// </summary>
    bool IsInstalled { get; }

    /// <summary>
    /// Checks whether a newer version is available on GitHub Releases.
    /// Returns false if not installed or if the check fails.
    /// </summary>
    Task<bool> IsUpdateAvailableAsync();

    /// <summary>
    /// Downloads the latest update and restarts the application to apply it.
    /// Does nothing if not installed or no update is available.
    /// </summary>
    Task ApplyUpdateAndRestartAsync();
}
