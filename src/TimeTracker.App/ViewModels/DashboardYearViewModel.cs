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
using TimeTracker.App.Extensions;
using TimeTracker.App.Helpers;
using TimeTracker.App.Services;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel for the Dashboard Year page.
/// </summary>
public partial class DashboardYearViewModel : ObservableObject
{
    private readonly IPageStateService _pageStateService;
    private readonly IDashboardService _dashboardService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private int _selectedYear;

    // Summary
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
    private string _holidayCount = "0";

    [ObservableProperty]
    private string _vacationCount = "0";

    [ObservableProperty]
    private string _freeChoiceCount = "0";

    // Charts
    [ObservableProperty]
    private ISeries[] _monthlyBarSeries = [];

    [ObservableProperty]
    private Axis[] _monthlyBarXAxes = [];

    [ObservableProperty]
    private Axis[] _monthlyBarYAxes = [];

    [ObservableProperty]
    private ISeries[] _activitySeries = [];

    [ObservableProperty]
    private ObservableCollection<ActivityBreakdownDisplay> _activityBreakdown = [];

    public DashboardYearViewModel(
        IPageStateService pageStateService,
        IDashboardService dashboardService,
        ILocalizationService localizationService)
    {
        _pageStateService = pageStateService ?? throw new ArgumentNullException(nameof(pageStateService));
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _selectedYear = _pageStateService.DashboardPage.ContextDate.Year;
    }

    public async Task LoadDataAsync()
    {
        SelectedYear = _pageStateService.DashboardPage.ContextDate.Year;

        var report = await _dashboardService.GetYearReportAsync(SelectedYear);

        // Summary
        WorkedTimeDisplay = report.TotalWorked.FormatDuration();
        TargetTimeDisplay = report.TotalTarget.FormatDuration();
        DifferentialDisplay = (report.Differential >= TimeSpan.Zero ? "+" : "-") + report.Differential.FormatDuration();
        IsDifferentialPositive = report.Differential >= TimeSpan.Zero;

        OfficeTimeDisplay = report.OfficeTime.FormatDuration();
        TeleworkTimeDisplay = report.TeleworkTime.FormatDuration();
        TeleworkPercentageDisplay = $"{report.TeleworkPercentage:F0}%";

        // Bar widths
        (OfficeBarWidth, TeleworkBarWidth) = DashboardDisplayHelper.CalculateBarWidths(
            report.OfficeTime.TotalHours, report.TeleworkTime.TotalHours);

        // Day type counts
        WorkDayCount = (report.DayTypeCounts.GetValueOrDefault(DayType.WorkDay) + report.DayTypeCounts.GetValueOrDefault(DayType.IntensiveDay)).ToString();
        HolidayCount = report.DayTypeCounts.GetValueOrDefault(DayType.Holiday).ToString();
        VacationCount = report.DayTypeCounts.GetValueOrDefault(DayType.Vacation).ToString();
        FreeChoiceCount = report.DayTypeCounts.GetValueOrDefault(DayType.FreeChoice).ToString();

        // Monthly bar chart (aggregate daily to monthly)
        BuildMonthlyBarChart(report.DailyBreakdown);

        // Activity donut
        ActivityBreakdown = DashboardDisplayHelper.BuildActivityBreakdown(report.Activities);

        ActivitySeries = DashboardDisplayHelper.BuildActivityDonutSeries(report.Activities);
    }

    private void BuildMonthlyBarChart(List<DailyHoursSummary> daily)
    {
        // Group by month
        var byMonth = daily.GroupBy(d => d.Date.Month)
            .OrderBy(g => g.Key)
            .ToList();

        var ci = CultureInfo.CurrentCulture;
        var monthLabels = Enumerable.Range(1, 12)
            .Select(m => ci.DateTimeFormat.GetAbbreviatedMonthName(m))
            .ToArray();

        var officeByMonth = new double[12];
        var teleworkByMonth = new double[12];

        foreach (var group in byMonth)
        {
            officeByMonth[group.Key - 1] = group.Sum(d => d.OfficeHours);
            teleworkByMonth[group.Key - 1] = group.Sum(d => d.TeleworkHours);
        }

        MonthlyBarSeries =
        [
            new StackedColumnSeries<double>
            {
                Name = _localizationService.GetString("Label_OfficeBar"),
                Values = officeByMonth,
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                MaxBarWidth = 24,
                Rx = 3,
                Ry = 3,
            },
            new StackedColumnSeries<double>
            {
                Name = _localizationService.GetString("Label_TeleworkBar"),
                Values = teleworkByMonth,
                Fill = new SolidColorPaint(SKColor.Parse("#2196F3")),
                MaxBarWidth = 24,
                Rx = 3,
                Ry = 3,
            }
        ];

        MonthlyBarXAxes =
        [
            new Axis
            {
                Labels = monthLabels,
                LabelsRotation = 0,
                TextSize = 12,
            }
        ];

        MonthlyBarYAxes =
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
    private async Task PreviousYear()
    {
        SelectedYear--;
        UpdateContextDate();
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NextYear()
    {
        SelectedYear++;
        UpdateContextDate();
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ThisYear()
    {
        SelectedYear = DateTime.Today.Year;
        UpdateContextDate();
        await LoadDataAsync();
    }

    private void UpdateContextDate()
    {
        _pageStateService.DashboardPage.ContextDate = new DateOnly(SelectedYear, 1, 1);
    }
}
