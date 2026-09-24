namespace Yatta.App.Blazor;

/// <summary>Formats times consistently across the hybrid interface.</summary>
public static class UiFormat
{
    /// <summary>Returns the CSS theme shared by all hybrid windows.</summary>
    public static string ThemeClass(Yatta.Core.Interfaces.IThemeService theme) => theme.GetCurrentTheme() switch
    {
        Yatta.Core.Models.Theme.Dark => "theme-dark",
        Yatta.Core.Models.Theme.System when Wpf.Ui.Appearance.ApplicationThemeManager.GetSystemTheme() == Wpf.Ui.Appearance.SystemTheme.Dark => "theme-dark",
        _ => "theme-light"
    };

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
