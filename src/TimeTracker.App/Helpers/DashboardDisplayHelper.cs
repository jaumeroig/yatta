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
/// donut chart series, office/telework donut) used by multiple ViewModels.
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
                Stroke = null,
                InnerRadius = 50,
                MaxRadialColumnWidth = 30,
                Pushout = 0,
                ToolTipLabelFormatter = _ =>
                    $"{a.ActivityName}: {a.TotalTime.FormatDuration()} ({a.Percentage:F1}%)",
            };
        }).ToArray();
    }

    /// <summary>
    /// Builds a donut-chart <see cref="ISeries"/> array for the office/telework breakdown.
    /// </summary>
    /// <param name="officeHours">Total office hours.</param>
    /// <param name="teleworkHours">Total telework hours.</param>
    /// <param name="officeLabel">Localized label for office.</param>
    /// <param name="teleworkLabel">Localized label for telework.</param>
    /// <param name="officeTimeFormatted">Formatted office time string for tooltip.</param>
    /// <param name="teleworkTimeFormatted">Formatted telework time string for tooltip.</param>
    public static ISeries[] BuildTeleworkDonutSeries(
        double officeHours, double teleworkHours,
        string officeLabel, string teleworkLabel,
        string officeTimeFormatted, string teleworkTimeFormatted)
    {
        if (officeHours <= 0 && teleworkHours <= 0)
            return [];

        var totalHours = officeHours + teleworkHours;
        var officePercent = totalHours > 0 ? officeHours / totalHours * 100 : 0;
        var teleworkPercent = totalHours > 0 ? teleworkHours / totalHours * 100 : 0;

        var series = new List<ISeries>();

        if (officeHours > 0)
        {
            series.Add(new PieSeries<double>
            {
                Values = [officeHours],
                Name = officeLabel,
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                Stroke = null,
                InnerRadius = 50,
                MaxRadialColumnWidth = 30,
                Pushout = 0,
                ToolTipLabelFormatter = _ =>
                    $"{officeLabel}: {officeTimeFormatted} ({officePercent:F0}%)",
            });
        }

        if (teleworkHours > 0)
        {
            series.Add(new PieSeries<double>
            {
                Values = [teleworkHours],
                Name = teleworkLabel,
                Fill = new SolidColorPaint(SKColor.Parse("#2196F3")),
                Stroke = null,
                InnerRadius = 50,
                MaxRadialColumnWidth = 30,
                Pushout = 0,
                ToolTipLabelFormatter = _ =>
                    $"{teleworkLabel}: {teleworkTimeFormatted} ({teleworkPercent:F0}%)",
            });
        }

        return series.ToArray();
    }
}
