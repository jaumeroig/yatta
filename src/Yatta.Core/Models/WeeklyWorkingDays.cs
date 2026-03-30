namespace Yatta.Core.Models;

/// <summary>
/// Bitmask flags representing which days of the week are working days by default.
/// </summary>
[Flags]
public enum WeeklyWorkingDays
{
    /// <summary>No days selected.</summary>
    None = 0,

    /// <summary>Monday is a working day.</summary>
    Monday = 1 << 0,

    /// <summary>Tuesday is a working day.</summary>
    Tuesday = 1 << 1,

    /// <summary>Wednesday is a working day.</summary>
    Wednesday = 1 << 2,

    /// <summary>Thursday is a working day.</summary>
    Thursday = 1 << 3,

    /// <summary>Friday is a working day.</summary>
    Friday = 1 << 4,

    /// <summary>Saturday is a working day.</summary>
    Saturday = 1 << 5,

    /// <summary>Sunday is a working day.</summary>
    Sunday = 1 << 6,

    /// <summary>Monday through Friday (standard weekdays).</summary>
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

    /// <summary>Saturday and Sunday.</summary>
    Weekend = Saturday | Sunday,

    /// <summary>All seven days of the week.</summary>
    All = Weekdays | Weekend
}
