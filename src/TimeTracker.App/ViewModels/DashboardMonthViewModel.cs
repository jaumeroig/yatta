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
/// ViewModel for the Dashboard Month page.
/// </summary>
public partial class DashboardMonthViewModel : ObservableObject
{
    private readonly IPageStateService _pageStateService;
    private readonly IDashboardService _dashboardService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private int _selectedMonth;

    [ObservableProperty]
    private int _selectedYear;

    [ObservableProperty]
    private string _monthYearDisplay = string.Empty;

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
    private ISeries[] _dailyBarSeries = [];

    [ObservableProperty]
    private Axis[] _dailyBarXAxes = [];

    [ObservableProperty]
    private Axis[] _dailyBarYAxes = [];

    [ObservableProperty]
    private ISeries[] _activitySeries = [];

    [ObservableProperty]
    private ObservableCollection<ActivityBreakdownDisplay> _activityBreakdown = [];

    public DashboardMonthViewModel(
        IPageStateService pageStateService,
        IDashboardService dashboardService,
        ILocalizationService localizationService)
    {
        _pageStateService = pageStateService ?? throw new ArgumentNullException(nameof(pageStateService));
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        var contextDate = _pageStateService.DashboardPage.ContextDate;
        _selectedMonth = contextDate.Month;
        _selectedYear = contextDate.Year;
        UpdateDisplay();
    }

    public async Task LoadDataAsync()
    {
        var contextDate = _pageStateService.DashboardPage.ContextDate;
        SelectedMonth = contextDate.Month;
        SelectedYear = contextDate.Year;
        UpdateDisplay();

        var report = await _dashboardService.GetMonthReportAsync(SelectedYear, SelectedMonth);

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

        // Daily bar chart
        BuildDailyBarChart(report.DailyBreakdown);

        // Activity donut
        ActivityBreakdown = DashboardDisplayHelper.BuildActivityBreakdown(report.Activities);

        ActivitySeries = DashboardDisplayHelper.BuildActivityDonutSeries(report.Activities);
    }

    private void BuildDailyBarChart(List<DailyHoursSummary> daily)
    {
        var dayLabels = daily.Select(d => d.Date.Day.ToString()).ToArray();
        var officeValues = daily.Select(d => d.OfficeHours).ToArray();
        var teleworkValues = daily.Select(d => d.TeleworkHours).ToArray();

        DailyBarSeries =
        [
            new StackedColumnSeries<double>
            {
                Name = _localizationService.GetString("Label_OfficeBar"),
                Values = officeValues,
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                MaxBarWidth = 16,
                Rx = 3,
                Ry = 3,
            },
            new StackedColumnSeries<double>
            {
                Name = _localizationService.GetString("Label_TeleworkBar"),
                Values = teleworkValues,
                Fill = new SolidColorPaint(SKColor.Parse("#2196F3")),
                MaxBarWidth = 16,
                Rx = 3,
                Ry = 3,
            }
        ];

        DailyBarXAxes =
        [
            new Axis
            {
                Labels = dayLabels,
                LabelsRotation = 0,
                TextSize = 11,
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
    private async Task PreviousMonth()
    {
        if (SelectedMonth == 1)
        {
            SelectedMonth = 12;
            SelectedYear--;
        }
        else
        {
            SelectedMonth--;
        }
        UpdateDisplay();
        UpdateContextDate();
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NextMonth()
    {
        if (SelectedMonth == 12)
        {
            SelectedMonth = 1;
            SelectedYear++;
        }
        else
        {
            SelectedMonth++;
        }
        UpdateDisplay();
        UpdateContextDate();
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ThisMonth()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        SelectedMonth = today.Month;
        SelectedYear = today.Year;
        UpdateDisplay();
        UpdateContextDate();
        await LoadDataAsync();
    }

    private void UpdateDisplay()
    {
        var date = new DateOnly(SelectedYear, SelectedMonth, 1);
        MonthYearDisplay = date.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        MonthYearDisplay = char.ToUpper(MonthYearDisplay[0]) + MonthYearDisplay[1..];
    }

    private void UpdateContextDate()
    {
        _pageStateService.DashboardPage.ContextDate = new DateOnly(SelectedYear, SelectedMonth, 1);
    }
}