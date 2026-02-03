namespace TimeTracker.Core.Models;

/// <summary>
/// Defines the different types of workdays.
/// </summary>
public enum DayType
{
    /// <summary>
    /// Normal workday with standard working hours.
    /// </summary>
    WorkDay = 0,

    /// <summary>
    /// Intensive workday (e.g., summer hours with shorter duration).
    /// </summary>
    IntensiveDay = 1,

    /// <summary>
    /// Official holiday (non-working day).
    /// </summary>
    Holiday = 2,

    /// <summary>
    /// Free choice day (personal day off).
    /// </summary>
    FreeChoice = 3,

    /// <summary>
    /// Vacation day.
    /// </summary>
    Vacation = 4
}
