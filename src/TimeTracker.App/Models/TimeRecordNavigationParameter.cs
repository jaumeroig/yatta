namespace TimeTracker.App.Models;

/// <summary>
/// Navigation parameter for the HistoricDetailPage.
/// </summary>
public class HistoricNavigationParameter
{
    /// <summary>
    /// The ID of the time record to edit.
    /// </summary>
    public Guid RecordId { get; init; }

    /// <summary>
    /// Indicates if navigation came from a notification (should set current time as end time and focus on end time field).
    /// </summary>
    public bool FromNotification { get; init; }
}
