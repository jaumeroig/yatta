namespace Yatta.Core.Models;

/// <summary>
/// Breakdown of time spent on a specific activity within a period.
/// </summary>
public class ActivityBreakdownItem
{
    /// <summary>
    /// Activity identifier.
    /// </summary>
    public Guid ActivityId { get; set; }

    /// <summary>
    /// Name of the activity.
    /// </summary>
    public string ActivityName { get; set; } = string.Empty;

    /// <summary>
    /// Color associated with the activity (hex string).
    /// </summary>
    public string Color { get; set; } = "#808080";

    /// <summary>
    /// Total time worked on this activity.
    /// </summary>
    public TimeSpan TotalTime { get; set; }

    /// <summary>
    /// Percentage of total time this activity represents.
    /// </summary>
    public double Percentage { get; set; }
}
