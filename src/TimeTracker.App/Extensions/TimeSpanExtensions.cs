namespace TimeTracker.App.Extensions;


internal static class TimeSpanExtensions
{
    public static string FormatDuration(this TimeSpan timeSpan)
    {
        var totalMinutes = (int)timeSpan.TotalMinutes;
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = Resources.Resources.Format_Duration;
        return string.Format(format, h, m);
    }
}
