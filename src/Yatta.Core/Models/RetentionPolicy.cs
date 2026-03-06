namespace Yatta.Core.Models;

/// <summary>
/// Defines the data retention policy options.
/// </summary>
public enum RetentionPolicy
{
    /// <summary>
    /// Keep all data forever.
    /// </summary>
    Forever = 0,

    /// <summary>
    /// Keep data for 1 year.
    /// </summary>
    OneYear = 1,

    /// <summary>
    /// Keep data for 2 years.
    /// </summary>
    TwoYears = 2,

    /// <summary>
    /// Keep data for 3 years.
    /// </summary>
    ThreeYears = 3,

    /// <summary>
    /// Keep data for a custom number of days.
    /// </summary>
    Custom = 4
}
