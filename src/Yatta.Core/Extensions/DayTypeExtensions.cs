namespace Yatta.Core.Extensions;

using System.Collections.Frozen;
using System.Reflection;

using Yatta.Core.Attributes;
using Yatta.Core.Models;

/// <summary>
/// Extension methods for the <see cref="DayType"/> enum.
/// </summary>
public static class DayTypeExtensions
{
    /// <summary>
    /// Cached workable status for each <see cref="DayType"/> value.
    /// Built once at startup from <see cref="WorkableDayAttribute"/> metadata to avoid repeated reflection.
    /// </summary>
    private static readonly FrozenDictionary<DayType, bool> WorkableCache = BuildWorkableCache();

    /// <summary>
    /// Determines whether the specified day type is workable based on the <see cref="WorkableDayAttribute"/>.
    /// </summary>
    /// <param name="dayType">The day type to check.</param>
    /// <returns>True if the day type is marked as workable; otherwise, false.</returns>
    public static bool IsWorkable(this DayType dayType)
    {
        return WorkableCache.TryGetValue(dayType, out var isWorkable) && isWorkable;
    }

    private static FrozenDictionary<DayType, bool> BuildWorkableCache()
    {
        var cache = new Dictionary<DayType, bool>();

        foreach (var dayType in Enum.GetValues<DayType>())
        {
            var fieldInfo = typeof(DayType).GetField(dayType.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<WorkableDayAttribute>();
            cache[dayType] = attribute?.IsWorkable ?? false;
        }

        return cache.ToFrozenDictionary();
    }
}
