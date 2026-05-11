namespace Yatta.Core.Models;

/// <summary>
/// Defines the different activities that can be recorded in the time tracker.
/// </summary>
public class Activity
{
    /// <summary>
    /// Unique identifier of the activity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the activity.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Color associated with the activity.
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Optional Jira issue code associated with this activity.
    /// </summary>
    public string? JiraCode { get; set; }

    /// <summary>
    /// Indicates if the activity is active.
    /// </summary>
    public bool Active { get; set; } = true;
}
