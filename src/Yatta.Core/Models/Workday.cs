namespace Yatta.Core.Models;

/// <summary>
/// Represents the configuration for a specific workday, including its type and target duration.
/// </summary>
public class Workday
{
    /// <summary>
    /// Unique identifier of the workday configuration.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The date for which this configuration applies.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The type of day (WorkDay, IntensiveDay, Holiday, FreeChoice, Vacation).
    /// </summary>
    public DayType DayType { get; set; } = DayType.WorkDay;

    /// <summary>
    /// The target working duration for this day.
    /// Only applicable for WorkDay and IntensiveDay types.
    /// For non-working day types (Holiday, FreeChoice, Vacation), this should be TimeSpan.Zero.
    /// </summary>
    public TimeSpan TargetDuration { get; set; } = TimeSpan.Zero;
}
