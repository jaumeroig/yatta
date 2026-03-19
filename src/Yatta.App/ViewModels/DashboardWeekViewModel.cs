namespace Yatta.App.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Yatta.App.Extensions;
using Yatta.App.Helpers;
using Yatta.App.Services;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

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
    private ISeries[] _teleworkSeries = [];

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
        WorkedTimeDisplay = report.TotalWorked.FormatDuration();
        TargetTimeDisplay = report.TotalTarget.FormatDuration();
        DifferentialDisplay = (report.Differential >= TimeSpan.Zero ? "+" : "-") + report.Differential.FormatDuration();
        IsDifferentialPositive = report.Differential >= TimeSpan.Zero;

        // Office / Telework
        OfficeTimeDisplay = report.OfficeTime.FormatDuration();
        TeleworkTimeDisplay = report.TeleworkTime.FormatDuration();
        TeleworkPercentageDisplay = $"{report.TeleworkPercentage:F0}%";

        // Telework donut
        TeleworkSeries = DashboardDisplayHelper.BuildTeleworkDonutSeries(
            report.OfficeTime.TotalHours, report.TeleworkTime.TotalHours,
            _localizationService.GetString(nameof(Resources.Resources.Location_Office)),
            _localizationService.GetString(nameof(Resources.Resources.Location_Telework)),
            OfficeTimeDisplay, TeleworkTimeDisplay);

        // Day type counts
        WorkDayCount = (report.DayTypeCounts.GetValueOrDefault(DayType.WorkDay) + report.DayTypeCounts.GetValueOrDefault(DayType.IntensiveDay)).ToString();
        IntensiveDayCount = report.DayTypeCounts.GetValueOrDefault(DayType.IntensiveDay).ToString();
        HolidayCount = report.DayTypeCounts.GetValueOrDefault(DayType.Holiday).ToString();
        VacationCount = report.DayTypeCounts.GetValueOrDefault(DayType.Vacation).ToString();

        // Daily stacked bar chart
        BuildDailyBarChart(report.DailyBreakdown);

        // Activity donut
        ActivityBreakdown = DashboardDisplayHelper.BuildActivityBreakdown(report.Activities);

        ActivitySeries = DashboardDisplayHelper.BuildActivityDonutSeries(report.Activities);
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
                Name = _localizationService.GetString(nameof(Resources.Resources.Location_Office)),
                Values = officeValues,
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                MaxBarWidth = 30,
                Rx = 4,
                Ry = 4,
            },
            new StackedColumnSeries<double>
            {
                Name = _localizationService.GetString(nameof(Resources.Resources.Location_Telework)),
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
}
