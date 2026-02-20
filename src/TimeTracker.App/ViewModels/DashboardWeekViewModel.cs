namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TimeTracker.App.Services;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel for the Dashboard Week page.
/// </summary>
public partial class DashboardWeekViewModel : ObservableObject
{
    private readonly IPageStateService _pageStateService;
    private readonly IDashboardService _dashboardService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private DateOnly _weekStartDate;

    [ObservableProperty]
    private DateOnly _weekEndDate;

    [ObservableProperty]
    private int _weekNumber;

    [ObservableProperty]
    private string _weekRangeDisplay = string.Empty;

    // Summary cards
    [ObservableProperty]
    private string _workedTimeDisplay = "0h 00m";

    [ObservableProperty]
    private string _targetTimeDisplay = "0h 00m";

    [ObservableProperty]
    private string _differentialDisplay = "0h 00m";

    [ObservableProperty]
    private bool _isDifferentialPositive;

    [ObservableProperty]
    private string _officeTimeDisplay = "0h 00m";

    [ObservableProperty]
    private string _teleworkTimeDisplay = "0h 00m";

    [ObservableProperty]
    private string _teleworkPercentageDisplay = "0%";

    [ObservableProperty]
    private double _officeBarWidth;

    [ObservableProperty]
    private double _teleworkBarWidth;

    // Day type counts
    [ObservableProperty]
    private string _workDayCount = "0";

    [ObservableProperty]
    private string _intensiveDayCount = "0";

    [ObservableProperty]
    private string _holidayCount = "0";

    [ObservableProperty]
    private string _vacationCount = "0";

    // Charts
    [ObservableProperty]
    private ISeries[] _dailyBarSeries = [];

    [ObservableProperty]
    private Axis[] _dailyBarXAxes = [];

    [ObservableProperty]
    private Axis[] _dailyBarYAxes = [];

    [ObservableProperty]
    private ISeries[] _activitySeries = [];

    [ObservableProperty]
    private ObservableCollection<ActivityBreakdownDisplay> _activityBreakdown = [];

    public DashboardWeekViewModel(
        IPageStateService pageStateService,
        IDashboardService dashboardService,
        ILocalizationService localizationService)
    {
        _pageStateService = pageStateService ?? throw new ArgumentNullException(nameof(pageStateService));
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        SetWeekFromDate(_pageStateService.DashboardPage.ContextDate);
    }

    /// <summary>
    /// Loads dashboard data for the selected week.
    /// </summary>
    public async Task LoadDataAsync()
    {
        SetWeekFromDate(_pageStateService.DashboardPage.ContextDate);

        var report = await _dashboardService.GetWeekReportAsync(WeekStartDate);

        // Summary
        WorkedTimeDisplay = FormatTimeSpan(report.TotalWorked);
        TargetTimeDisplay = FormatTimeSpan(report.TotalTarget);
        DifferentialDisplay = (report.Differential >= TimeSpan.Zero ? "+" : "-") + FormatTimeSpan(report.Differential);
        IsDifferentialPositive = report.Differential >= TimeSpan.Zero;

        // Office / Telework
        OfficeTimeDisplay = FormatTimeSpan(report.OfficeTime);
        TeleworkTimeDisplay = FormatTimeSpan(report.TeleworkTime);
        TeleworkPercentageDisplay = $"{report.TeleworkPercentage:F0}%";

        const double maxBarWidth = 200.0;
        var maxHours = Math.Max(report.OfficeTime.TotalHours, report.TeleworkTime.TotalHours);
        if (maxHours > 0)
        {
            OfficeBarWidth = report.OfficeTime.TotalHours / maxHours * maxBarWidth;
            TeleworkBarWidth = report.TeleworkTime.TotalHours / maxHours * maxBarWidth;
        }
        else
        {
            OfficeBarWidth = 0;
            TeleworkBarWidth = 0;
        }

        // Day type counts
        WorkDayCount = (report.DayTypeCounts.GetValueOrDefault(DayType.WorkDay) + report.DayTypeCounts.GetValueOrDefault(DayType.IntensiveDay)).ToString();
        IntensiveDayCount = report.DayTypeCounts.GetValueOrDefault(DayType.IntensiveDay).ToString();
        HolidayCount = report.DayTypeCounts.GetValueOrDefault(DayType.Holiday).ToString();
        VacationCount = report.DayTypeCounts.GetValueOrDefault(DayType.Vacation).ToString();

        // Daily stacked bar chart
        BuildDailyBarChart(report.DailyBreakdown);

        // Activity donut
        ActivityBreakdown = new ObservableCollection<ActivityBreakdownDisplay>(
            report.Activities.Select(a => new ActivityBreakdownDisplay
            {
                ActivityName = a.ActivityName,
                Color = a.Color,
                ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(a.Color)),
                TotalTime = FormatTimeSpan(a.TotalTime),
                Percentage = $"{a.Percentage:F1}%",
                PercentageValue = a.Percentage
            }));

        ActivitySeries = report.Activities.Select(a =>
        {
            var skColor = SKColor.Parse(a.Color);
            return (ISeries)new PieSeries<double>
            {
                Values = [a.TotalTime.TotalMinutes],
                Name = a.ActivityName,
                Fill = new SolidColorPaint(skColor),
                InnerRadius = 60,
                Pushout = 0,
                ToolTipLabelFormatter = _ => $"{a.ActivityName}: {FormatTimeSpan(a.TotalTime)} ({a.Percentage:F1}%)",
            };
        }).ToArray();
    }

    private void BuildDailyBarChart(List<DailyHoursSummary> daily)
    {
        var dayLabels = daily.Select(d => d.Date.ToString("ddd", CultureInfo.CurrentCulture)).ToArray();
        var officeValues = daily.Select(d => d.OfficeHours).ToArray();
        var teleworkValues = daily.Select(d => d.TeleworkHours).ToArray();

        DailyBarSeries =
        [
            new StackedColumnSeries<double>
            {
                Name = _localizationService.GetString("Label_OfficeBar"),
                Values = officeValues,
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                MaxBarWidth = 30,
                Rx = 4,
                Ry = 4,
            },
            new StackedColumnSeries<double>
            {
                Name = _localizationService.GetString("Label_TeleworkBar"),
                Values = teleworkValues,
                Fill = new SolidColorPaint(SKColor.Parse("#2196F3")),
                MaxBarWidth = 30,
                Rx = 4,
                Ry = 4,
            }
        ];

        DailyBarXAxes =
        [
            new Axis
            {
                Labels = dayLabels,
                LabelsRotation = 0,
                TextSize = 13,
            }
        ];

        DailyBarYAxes =
        [
            new Axis
            {
                Labeler = val => $"{val:F0}h",
                MinLimit = 0,
                TextSize = 12,
            }
        ];
    }

    [RelayCommand]
    private async Task PreviousWeek()
    {
        SetWeekFromDate(WeekStartDate.AddDays(-7));
        UpdateContextDate();
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NextWeek()
    {
        SetWeekFromDate(WeekStartDate.AddDays(7));
        UpdateContextDate();
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ThisWeek()
    {
        SetWeekFromDate(DateOnly.FromDateTime(DateTime.Today));
        UpdateContextDate();
        await LoadDataAsync();
    }

    private void SetWeekFromDate(DateOnly date)
    {
        var dayOfWeek = date.DayOfWeek;
        var daysToMonday = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
        WeekStartDate = date.AddDays(-daysToMonday);
        WeekEndDate = WeekStartDate.AddDays(6);
        WeekNumber = ISOWeek.GetWeekOfYear(WeekStartDate.ToDateTime(TimeOnly.MinValue));

        var ci = CultureInfo.CurrentCulture;
        WeekRangeDisplay = $"{WeekStartDate.ToString("d MMM", ci)} – {WeekEndDate.ToString("d MMM yyyy", ci)}";
    }

    private void UpdateContextDate()
    {
        _pageStateService.DashboardPage.ContextDate = WeekStartDate;
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        var totalMinutes = (int)Math.Abs(ts.TotalMinutes);
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours}h {minutes:D2}m";
    }
}
