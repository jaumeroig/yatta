namespace Yatta.App.Extensions;

using Yatta.Core.Models;

internal static class DayTypeExtensions
{
    extension(DayType dayType)
    {
        public string GetDisplayName()
        {
            return dayType switch
            {
                DayType.WorkDay => Resources.Resources.Today_DayType_WorkDay,
                DayType.IntensiveDay => Resources.Resources.Today_DayType_IntensiveDay,
                DayType.Holiday => Resources.Resources.Today_DayType_Holiday,
                DayType.FreeChoice => Resources.Resources.Today_DayType_FreeChoice,
                DayType.Vacation => Resources.Resources.Today_DayType_Vacation,
                _ => throw new Exception($"No resource found for DayType: {dayType}")
            };
        }
    }
}
