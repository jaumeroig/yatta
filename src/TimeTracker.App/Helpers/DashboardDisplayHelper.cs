namespace TimeTracker.App.Helpers;

using System.Collections.ObjectModel;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TimeTracker.App.Extensions;
using TimeTracker.App.ViewModels;
using TimeTracker.Core.Models;

/// <summary>
/// Shared helpers for building dashboard display objects (activity breakdown,
/// donut chart series, office/telework bar widths) used by multiple ViewModels.
/// </summary>
internal static class DashboardDisplayHelper
{
    /// <summary>
    /// Builds an <see cref="ObservableCollection{T}"/> of <see cref="ActivityBreakdownDisplay"/>
    /// from a list of <see cref="ActivityBreakdownItem"/> items.
    /// </summary>
    public static ObservableCollection<ActivityBreakdownDisplay> BuildActivityBreakdown(
        IEnumerable<ActivityBreakdownItem> activities)
    {
        return new ObservableCollection<ActivityBreakdownDisplay>(
            activities.Select(a => new ActivityBreakdownDisplay
            {
                ActivityName = a.ActivityName,
                Color = a.Color,
                ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(a.Color)),
                TotalTime = a.TotalTime.FormatDuration(),
                Percentage = $"{a.Percentage:F1}%",
                PercentageValue = a.Percentage
            }));
    }

    /// <summary>
    /// Builds a donut-chart <see cref="ISeries"/> array from a list of <see cref="ActivityBreakdownItem"/> items.
    /// </summary>
    public static ISeries[] BuildActivityDonutSeries(IEnumerable<ActivityBreakdownItem> activities)
    {
        return activities.Select(a =>
        {
            var skColor = SKColor.Parse(a.Color);
            return (ISeries)new PieSeries<double>
            {
                Values = [a.TotalTime.TotalMinutes],
                Name = a.ActivityName,
                Fill = new SolidColorPaint(skColor),
                InnerRadius = 60,
                Pushout = 0,
                ToolTipLabelFormatter = _ =>
                    $"{a.ActivityName}: {a.TotalTime.FormatDuration()} ({a.Percentage:F1}%)",
            };
        }).ToArray();
    }

    /// <summary>
    /// Calculates proportional bar widths for an office/telework comparison bar.
    /// </summary>
    /// <param name="officeHours">Total office hours.</param>
    /// <param name="teleworkHours">Total telework hours.</param>
    /// <param name="maxBarWidth">Maximum pixel width for the larger bar (default 200).</param>
    /// <returns>A tuple with the computed office and telework bar widths.</returns>
    public static (double OfficeBarWidth, double TeleworkBarWidth) CalculateBarWidths(
        double officeHours, double teleworkHours, double maxBarWidth = 200.0)
    {
        var maxHours = Math.Max(officeHours, teleworkHours);
        if (maxHours > 0)
        {
            return (officeHours / maxHours * maxBarWidth,
                    teleworkHours / maxHours * maxBarWidth);
        }

        return (0, 0);
    }
}
