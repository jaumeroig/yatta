namespace TimeTracker.App.Helpers;

using AppResources = TimeTracker.App.Resources.Resources;

/// <summary>
/// Shared duration formatting and parsing utilities used across ViewModels.
/// </summary>
internal static class DurationFormatHelper
{
    /// <summary>
    /// Formats a duration expressed in fractional hours (e.g. 1.5 → "1h 30m")
    /// using the localized <c>Format_Duration</c> resource string.
    /// </summary>
    public static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = AppResources.Format_Duration;
        return string.Format(format, h, m);
    }

    /// <summary>
    /// Converts a fractional-hours value to an "H:mm" string (e.g. 8.5 → "8:30").
    /// Returns "8:00" when the input is zero or negative.
    /// </summary>
    public static string FormatHoursToHHmm(double hours)
    {
        if (hours <= 0)
        {
            return "8:00";
        }

        var timeSpan = TimeSpan.FromHours(hours);
        return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:D2}";
    }

    /// <summary>
    /// Parses an "H:mm" string into a <see cref="TimeSpan"/>.
    /// Returns <see langword="false"/> when the text is null/empty, has an invalid format,
    /// contains negative parts, minutes &gt; 59, or represents zero duration.
    /// </summary>
    public static bool TryParseHHmm(string text, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var parts = text.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var h) || !int.TryParse(parts[1], out var m))
        {
            return false;
        }

        if (h < 0 || m < 0 || m > 59)
        {
            return false;
        }

        if (h == 0 && m == 0)
        {
            return false;
        }

        duration = new TimeSpan(h, m, 0);
        return true;
    }
}
