namespace Yatta.Core.Models;

using Yatta.Core.Attributes;

/// <summary>
/// Defines the different types of workdays.
/// Use <see cref="Extensions.DayTypeExtensions.IsWorkable"/> to check if a day type is workable.
/// </summary>
public enum DayType
{
    /// <summary>
    /// Normal workday with standard working hours.
    /// </summary>
    [WorkableDay(true)]
    WorkDay = 0,

    /// <summary>
    /// Intensive workday (e.g., summer hours with shorter duration).
    /// </summary>
    [WorkableDay(true)]
    IntensiveDay = 1,

    /// <summary>
    /// Official holiday (non-working day).
    /// </summary>
    [WorkableDay(false)]
    Holiday = 2,

    /// <summary>
    /// Free choice day (personal day off).
    /// </summary>
    [WorkableDay(false)]
    FreeChoice = 3,

    /// <summary>
    /// Vacation day.
    /// </summary>
    [WorkableDay(false)]
    Vacation = 4,

    /// <summary>
    /// Non-working day based on the default weekly schedule (e.g. Saturday/Sunday).
    /// Does not count as a holiday, vacation, or free-choice day.
    /// </summary>
    [WorkableDay(false)]
    NonWorkingDay = 5
}
