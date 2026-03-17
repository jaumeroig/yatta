namespace Yatta.Core.Interfaces;

using Yatta.Core.Models;

/// <summary>
/// Starts a new time record on application startup when the configured conditions are met.
/// </summary>
public interface IAutoStartActivityService
{
    /// <summary>
    /// Starts a new time record using the last activity from the previous day when enabled.
    /// </summary>
    /// <returns>The created time record, or null when no record should be created.</returns>
    Task<TimeRecord?> TryStartPreviousDayActivityAsync();
}
