namespace Yatta.App.Blazor;

using System.Globalization;

/// <summary>Formats times consistently across the hybrid interface.</summary>
public static class UiFormat
{
    /// <summary>Returns the CSS theme shared by all hybrid windows.</summary>
    public static string ThemeClass(Yatta.Core.Interfaces.IThemeService theme) => theme.GetCurrentTheme() switch
    {
        Yatta.Core.Models.Theme.Dark => "theme-dark dark",
        Yatta.Core.Models.Theme.System when Wpf.Ui.Appearance.ApplicationThemeManager.GetSystemTheme() == Wpf.Ui.Appearance.SystemTheme.Dark => "theme-dark dark",
        _ => "theme-light"
    };

    /// <summary>Preserves seconds and fractional ticks when a minute-resolution field was not changed.</summary>
    /// <param name="text">The value submitted by a time input.</param>
    /// <param name="original">The original stored time, if any.</param>
    /// <returns>The original time when unchanged, or the parsed edited time.</returns>
    /// <exception cref="FormatException">The input is not a valid time.</exception>
    public static TimeOnly? EditedTime(string text, TimeOnly? original)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return text == original?.ToString("HH:mm", CultureInfo.InvariantCulture)
            ? original
            : TimeOnly.Parse(text, CultureInfo.InvariantCulture);
    }

    /// <summary>Checks that a day target is between zero and twenty-four hours.</summary>
    public static bool IsValidDayTarget(int hours, int minutes) =>
        hours is >= 0 and <= 24 && minutes is >= 0 and <= 59 && (hours < 24 || minutes == 0);

    /// <summary>Formats a duration as hours and minutes.</summary>
    public static string Duration(TimeSpan value)
    {
        int minutes = Math.Max(0, (int)Math.Round(value.TotalMinutes));
        return $"{minutes / 60}h {minutes % 60:00}m";
    }

    /// <summary>Returns the elapsed duration of a record.</summary>
    public static TimeSpan Elapsed(Yatta.Core.Models.TimeRecord record, DateTime now)
    {
        DateTime start = record.Date.ToDateTime(record.StartTime);
        DateTime end = record.EndTime.HasValue ? record.Date.ToDateTime(record.EndTime.Value) : now;
        return end > start ? end - start : TimeSpan.Zero;
    }
}
