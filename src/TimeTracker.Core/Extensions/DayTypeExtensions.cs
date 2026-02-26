namespace TimeTracker.Core.Extensions;

using System.Reflection;

using TimeTracker.Core.Attributes;
using TimeTracker.Core.Models;

/// <summary>
/// Extension methods for the <see cref="DayType"/> enum.
/// </summary>
public static class DayTypeExtensions
{
    /// <summary>
    /// Determines whether the specified day type is workable based on the <see cref="WorkableDayAttribute"/>.
    /// </summary>
    /// <param name="dayType">The day type to check.</param>
    /// <returns>True if the day type is marked as workable; otherwise, false.</returns>
    public static bool IsWorkable(this DayType dayType)
    {
        var fieldInfo = dayType.GetType().GetField(dayType.ToString());

        if (fieldInfo == null)
        {
            return false;
        }

        var attribute = fieldInfo.GetCustomAttribute<WorkableDayAttribute>();

        return attribute?.IsWorkable ?? false;
    }
}
