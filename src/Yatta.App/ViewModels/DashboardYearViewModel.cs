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
/// ViewModel for the Dashboard Year page.
/// </summary>
public partial class DashboardYearViewModel : ObservableObject
{
    private readonly IPageStateService _pageStateService;
    private readonly IDashboardService _dashboardService;
    private readonly ILocalizationService _localizationService;
    private readonly IAnnualQuotaRepository _annualQuotaRepository;

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
    private ISeries[] _teleworkSeries = [];

    // Day type counts
    [ObservableProperty]
    private string _workDayCount = "0";

    [ObservableProperty]
    private string _holidayCount = "0";

    [ObservableProperty]
    private string _vacationCount = "0";

    [ObservableProperty]
    private string _freeChoiceCount = "0";

    // Annual quota
    [ObservableProperty]
    private bool _isConfigureYearQuotaDialogOpen;

    [ObservableProperty]
    private ConfigureYearQuotaModel _configureYearQuotaModel = new();

    [ObservableProperty]
    private int _vacationAvailable;

    [ObservableProperty]
    private int _vacationUsed;

    [ObservableProperty]
    private int _vacationRemaining;

    [ObservableProperty]
    private bool _isVacationRemainingNegative;

    [ObservableProperty]
    private int _freeChoiceAvailable;

    [ObservableProperty]
    private int _freeChoiceUsed;

    [ObservableProperty]
    private int _freeChoiceRemaining;

    [ObservableProperty]
    private bool _isFreeChoiceRemainingNegative;

    [ObservableProperty]
    private int _intensiveAvailable;

    [ObservableProperty]
    private int _intensiveUsed;

    [ObservableProperty]
    private int _intensiveRemaining;

    [ObservableProperty]
    private bool _isIntensiveRemainingNegative;

    [ObservableProperty]
    private bool _hasQuotaConfigured;

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
        ILocalizationService localizationService,
        IAnnualQuotaRepository annualQuotaRepository)
    {
        _pageStateService = pageStateService ?? throw new ArgumentNullException(nameof(pageStateService));
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _annualQuotaRepository = annualQuotaRepository ?? throw new ArgumentNullException(nameof(annualQuotaRepository));
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

        // Telework donut
        TeleworkSeries = DashboardDisplayHelper.BuildTeleworkDonutSeries(
            report.OfficeTime.TotalHours, report.TeleworkTime.TotalHours,
            _localizationService.GetString(nameof(Resources.Resources.Location_Office)),
            _localizationService.GetString(nameof(Resources.Resources.Location_Telework)),
            OfficeTimeDisplay, TeleworkTimeDisplay);

        // Day type counts
        WorkDayCount = (report.DayTypeCounts.GetValueOrDefault(DayType.WorkDay) + report.DayTypeCounts.GetValueOrDefault(DayType.IntensiveDay)).ToString();
        HolidayCount = report.DayTypeCounts.GetValueOrDefault(DayType.Holiday).ToString();
        VacationCount = report.DayTypeCounts.GetValueOrDefault(DayType.Vacation).ToString();
        FreeChoiceCount = report.DayTypeCounts.GetValueOrDefault(DayType.FreeChoice).ToString();

        // Load annual quota data
        await LoadQuotaDataAsync(report.DayTypeCounts);

        // Monthly bar chart (aggregate daily to monthly)
        BuildMonthlyBarChart(report.DailyBreakdown);

        // Activity donut
        ActivityBreakdown = DashboardDisplayHelper.BuildActivityBreakdown(report.Activities);

        ActivitySeries = DashboardDisplayHelper.BuildActivityDonutSeries(report.Activities);
    }

    private async Task LoadQuotaDataAsync(Dictionary<DayType, int> dayTypeCounts)
    {
        var quota = await _annualQuotaRepository.GetByYearAsync(SelectedYear);

        if (quota != null)
        {
            HasQuotaConfigured = true;

            // Vacation
            VacationAvailable = quota.VacationDays;
            VacationUsed = dayTypeCounts.GetValueOrDefault(DayType.Vacation);
            VacationRemaining = VacationAvailable - VacationUsed;
            IsVacationRemainingNegative = VacationRemaining < 0;

            // Free choice
            FreeChoiceAvailable = quota.FreeChoiceDays;
            FreeChoiceUsed = dayTypeCounts.GetValueOrDefault(DayType.FreeChoice);
            FreeChoiceRemaining = FreeChoiceAvailable - FreeChoiceUsed;
            IsFreeChoiceRemainingNegative = FreeChoiceRemaining < 0;

            // Intensive days
            IntensiveAvailable = quota.IntensiveDays;
            IntensiveUsed = dayTypeCounts.GetValueOrDefault(DayType.IntensiveDay);
            IntensiveRemaining = IntensiveAvailable - IntensiveUsed;
            IsIntensiveRemainingNegative = IntensiveRemaining < 0;
        }
        else
        {
            HasQuotaConfigured = false;
            VacationAvailable = 0;
            VacationUsed = 0;
            VacationRemaining = 0;
            IsVacationRemainingNegative = false;

            FreeChoiceAvailable = 0;
            FreeChoiceUsed = 0;
            FreeChoiceRemaining = 0;
            IsFreeChoiceRemainingNegative = false;

            IntensiveAvailable = 0;
            IntensiveUsed = 0;
            IntensiveRemaining = 0;
            IsIntensiveRemainingNegative = false;
        }
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
                Name = _localizationService.GetString(nameof(Resources.Resources.Location_Office)),
                Values = officeByMonth,
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                MaxBarWidth = 24,
                Rx = 3,
                Ry = 3,
            },
            new StackedColumnSeries<double>
            {
                Name = _localizationService.GetString(nameof(Resources.Resources.Location_Telework)),
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

    [RelayCommand]
    private async Task ConfigureQuota()
    {
        var quota = await _annualQuotaRepository.GetByYearAsync(SelectedYear);

        ConfigureYearQuotaModel = new ConfigureYearQuotaModel
        {
            Year = SelectedYear,
            VacationDays = quota?.VacationDays ?? 0,
            FreeChoiceDays = quota?.FreeChoiceDays ?? 0,
            IntensiveDays = quota?.IntensiveDays ?? 0,
            ValidationError = string.Empty
        };

        IsConfigureYearQuotaDialogOpen = true;
    }

    [RelayCommand]
    private async Task SaveConfigureQuota()
    {
        // No validation needed - NumberBox handles min/max
        var quota = new AnnualQuota
        {
            Year = SelectedYear,
            VacationDays = ConfigureYearQuotaModel.VacationDays,
            FreeChoiceDays = ConfigureYearQuotaModel.FreeChoiceDays,
            IntensiveDays = ConfigureYearQuotaModel.IntensiveDays
        };

        await _annualQuotaRepository.SaveAsync(quota);

        IsConfigureYearQuotaDialogOpen = false;
        await LoadDataAsync();
    }
}
