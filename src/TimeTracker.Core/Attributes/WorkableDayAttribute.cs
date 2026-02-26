namespace TimeTracker.Core.Attributes;

/// <summary>
/// Indicates whether a <see cref="Models.DayType"/> enum value represents a workable (working) day.
/// </summary>
/// <remarks>
/// Apply this attribute to each value of the <see cref="Models.DayType"/> enum
/// to declaratively specify if that day type is workable.
/// Use <see cref="Extensions.DayTypeExtensions.IsWorkable"/> to query the attribute at runtime.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class WorkableDayAttribute : Attribute
{
    /// <summary>
    /// Gets a value indicating whether the day type is workable.
    /// </summary>
    public bool IsWorkable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkableDayAttribute"/> class.
    /// </summary>
    /// <param name="isWorkable">True if the day type is workable; otherwise, false.</param>
    public WorkableDayAttribute(bool isWorkable)
    {
        IsWorkable = isWorkable;
    }
}
