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
using TimeTracker.App.Controls;
using TimeTracker.App.Services;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel for the Dashboard Day page.
/// </summary>
public partial class DashboardDayViewModel : ObservableObject
{
    private readonly IPageStateService _pageStateService;
    private readonly IDashboardService _dashboardService;
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkdayConfigService _workdayConfigService;
    private Dictionary<Guid, Activity> _activitiesCache = [];

    [ObservableProperty]
    private DateTime _selectedDate;

    [ObservableProperty]
    private string _fullDateDisplay = string.Empty;

    [ObservableProperty]
    private string _dayTypeDisplay = string.Empty;

    [ObservableProperty]
    private string _startTimeDisplay = "--:--";

    [ObservableProperty]
    private string _targetTimeDisplay = "0h 0m";

    [ObservableProperty]
    private string _workedTimeDisplay = "0h 0m";

    [ObservableProperty]
    private string _differentialDisplay = "0h 0m";

    [ObservableProperty]
    private bool _isDifferentialPositive;

    [ObservableProperty]
    private string _officeTimeDisplay = "0h 0m";

    [ObservableProperty]
    private string _teleworkTimeDisplay = "0h 0m";

    [ObservableProperty]
    private string _teleworkPercentageDisplay = "0%";

    [ObservableProperty]
    private double _officeBarWidth;

    [ObservableProperty]
    private double _teleworkBarWidth;

    [ObservableProperty]
    private ObservableCollection<ActivityBreakdownDisplay> _activityBreakdown = [];

    [ObservableProperty]
    private ISeries[] _activitySeries = [];

    [ObservableProperty]
    private ObservableCollection<DayRecordDisplay> _records = [];

    // Timeline properties
    [ObservableProperty]
    private ObservableCollection<TimeSegment> _timelineSegments = [];

    [ObservableProperty]
    private DateTime _timelineStart = DateTime.Today.AddHours(9);

    [ObservableProperty]
    private DateTime _timelineEnd = DateTime.Today.AddHours(18);

    // Calendar indicator dates
    [ObservableProperty]
    private ObservableCollection<DateTime> _officeDates = [];

    [ObservableProperty]
    private ObservableCollection<DateTime> _teleworkDates = [];

    [ObservableProperty]
    private ObservableCollection<DateTime> _bothDates = [];

    [ObservableProperty]
    private string _monthYear = string.Empty;

    [ObservableProperty]
    private bool _isConfigureDayDialogOpen;

    [ObservableProperty]
    private ConfigureDayModel _configureDayModel = new();

    public DashboardDayViewModel(
        IPageStateService pageStateService,
        IDashboardService dashboardService,
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        ILocalizationService localizationService,
        IWorkdayConfigService workdayConfigService)
    {
        _pageStateService = pageStateService ?? throw new ArgumentNullException(nameof(pageStateService));
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _timeRecordRepository = timeRecordRepository ?? throw new ArgumentNullException(nameof(timeRecordRepository));
        _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _workdayConfigService = workdayConfigService ?? throw new ArgumentNullException(nameof(workdayConfigService));
        _selectedDate = _pageStateService.DashboardPage.ContextDate.ToDateTime(TimeOnly.MinValue);
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        _pageStateService.DashboardPage.ContextDate = DateOnly.FromDateTime(value);
    }

    /// <summary>
    /// Loads dashboard data for the selected day.
    /// </summary>
    public async Task LoadDataAsync()
    {
        SelectedDate = _pageStateService.DashboardPage.ContextDate.ToDateTime(TimeOnly.MinValue);
        await LoadDayDataAsync();
        await LoadCalendarIndicatorsAsync();
    }

    [RelayCommand]
    private async Task SelectDate(DateTime date)
    {
        SelectedDate = date;
        await LoadDayDataAsync();
    }

    [RelayCommand]
    private async Task Today()
    {
        SelectedDate = DateTime.Today;
        await LoadDayDataAsync();
        await LoadCalendarIndicatorsAsync();
    }

    [RelayCommand]
    private async Task PreviousMonth()
    {
        SelectedDate = SelectedDate.AddMonths(-1);
        await LoadCalendarIndicatorsAsync();
    }

    [RelayCommand]
    private async Task NextMonth()
    {
        SelectedDate = SelectedDate.AddMonths(1);
        await LoadCalendarIndicatorsAsync();
    }

    /// <summary>
    /// Opens the configure day dialog for the currently selected date.
    /// </summary>
    [RelayCommand]
    private async Task ConfigureDayAsync()
    {
        var date = DateOnly.FromDateTime(SelectedDate);
        var currentConfig = await _workdayConfigService.GetEffectiveConfigurationAsync(date);

        ConfigureDayModel = new ConfigureDayModel
        {
            Date = date,
            DayType = currentConfig.DayType,
            TargetDurationHours = currentConfig.TargetDuration.TotalHours,
            TargetDurationText = FormatHoursToHHmm(currentConfig.TargetDuration.TotalHours),
            ValidationError = string.Empty
        };

        IsConfigureDayDialogOpen = true;
    }

    /// <summary>
    /// Saves the day configuration from the dialog.
    /// </summary>
    [RelayCommand]
    private async Task SaveConfigureDayAsync()
    {
        var isWorkingDay = ConfigureDayModel.DayType == DayType.WorkDay ||
                          ConfigureDayModel.DayType == DayType.IntensiveDay;

        TimeSpan targetDuration = TimeSpan.Zero;
        if (isWorkingDay)
        {
            if (!TryParseHHmm(ConfigureDayModel.TargetDurationText, out targetDuration))
            {
                ConfigureDayModel.ValidationError = TimeTracker.App.Resources.Resources.Validation_TargetDurationInvalid;
                return;
            }
        }

        await _workdayConfigService.SetConfigurationAsync(
            ConfigureDayModel.Date,
            ConfigureDayModel.DayType,
            targetDuration);

        IsConfigureDayDialogOpen = false;
        await LoadDayDataAsync();
    }

    /// <summary>
    /// Closes the configure day dialog.
    /// </summary>
    [RelayCommand]
    private void CloseConfigureDayDialog()
    {
        IsConfigureDayDialogOpen = false;
    }

    private static string FormatHoursToHHmm(double hours)
    {
        if (hours <= 0)
        {
            return "8:00";
        }

        var timeSpan = TimeSpan.FromHours(hours);
        return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:D2}";
    }

    private static bool TryParseHHmm(string text, out TimeSpan duration)
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

        if (!int.TryParse(parts[0], out var hours) || !int.TryParse(parts[1], out var minutes))
        {
            return false;
        }

        if (hours < 0 || minutes < 0 || minutes > 59)
        {
            return false;
        }

        if (hours == 0 && minutes == 0)
        {
            return false;
        }

        duration = new TimeSpan(hours, minutes, 0);
        return true;
    }

    private async Task LoadDayDataAsync()
    {
        var date = DateOnly.FromDateTime(SelectedDate);
        var report = await _dashboardService.GetDayReportAsync(date);
        _activitiesCache = (await _activityRepository.GetAllAsync()).ToDictionary(a => a.Id);

        // Date display
        FullDateDisplay = SelectedDate.ToString("dddd, d 'de' MMMM 'de' yyyy", CultureInfo.CurrentCulture);
        FullDateDisplay = char.ToUpper(FullDateDisplay[0]) + FullDateDisplay[1..];

        // Month header
        MonthYear = SelectedDate.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        MonthYear = char.ToUpper(MonthYear[0]) + MonthYear[1..];

        // Day type
        DayTypeDisplay = GetDayTypeString(report.DayType);

        // Start time
        StartTimeDisplay = report.StartTime?.ToString("HH:mm") ?? "--:--";

        // Target & worked
        TargetTimeDisplay = FormatTimeSpan(report.TargetDuration);
        WorkedTimeDisplay = FormatTimeSpan(report.WorkedDuration);

        // Differential
        IsDifferentialPositive = report.Differential >= TimeSpan.Zero;
        var absDiff = report.Differential < TimeSpan.Zero ? report.Differential.Negate() : report.Differential;
        DifferentialDisplay = (IsDifferentialPositive ? "+" : "-") + FormatTimeSpan(absDiff);

        // Office / Telework
        OfficeTimeDisplay = FormatTimeSpan(report.OfficeTime);
        TeleworkTimeDisplay = FormatTimeSpan(report.TeleworkTime);
        TeleworkPercentageDisplay = $"{report.TeleworkPercentage:F0}%";

        // Bar widths (pixel-based, max 200px – same pattern as JornadaPage)
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

        // Activity breakdown
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

        // Donut chart series
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
                ToolTipLabelFormatter = point => $"{a.ActivityName}: {FormatTimeSpan(a.TotalTime)} ({a.Percentage:F1}%)",
            };
        }).ToArray();

        // Records list
        Records = new ObservableCollection<DayRecordDisplay>(
            report.Records.Select(r =>
            {
                var activity = _activitiesCache.GetValueOrDefault(r.ActivityId);
                return new DayRecordDisplay
                {
                    ActivityName = activity?.Name ?? "Unknown",
                    ActivityColor = activity?.Color ?? "#808080",
                    ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(activity?.Color ?? "#808080")),
                    StartTime = r.StartTime.ToString("HH:mm"),
                    EndTime = r.EndTime?.ToString("HH:mm") ?? "--:--",
                    Duration = r.EndTime.HasValue ? FormatTimeSpan(r.EndTime.Value.ToTimeSpan() - r.StartTime.ToTimeSpan()) : "--:--",
                    Notes = r.Notes ?? string.Empty,
                    IsTelework = r.Telework
                };
            }));

        // Timeline
        BuildTimeline(report.Records);
    }

    private void BuildTimeline(List<TimeRecord> records)
    {
        var segments = new ObservableCollection<TimeSegment>();
        var date = DateOnly.FromDateTime(SelectedDate);

        if (records.Count > 0)
        {
            var minStart = records.Min(r => r.StartTime);
            var maxEnd = records.Where(r => r.EndTime.HasValue).Select(r => r.EndTime!.Value).DefaultIfEmpty(minStart).Max();
            TimelineStart = date.ToDateTime(minStart).AddMinutes(-30);
            TimelineEnd = date.ToDateTime(maxEnd).AddMinutes(30);

            foreach (var record in records)
            {
                if (!record.EndTime.HasValue) continue;
                var activity = _activitiesCache.GetValueOrDefault(record.ActivityId);
                var colorStr = activity?.Color ?? "#808080";
                var color = (Color)ColorConverter.ConvertFromString(colorStr);
                segments.Add(new TimeSegment
                {
                    Start = date.ToDateTime(record.StartTime),
                    End = date.ToDateTime(record.EndTime.Value),
                    Color = color,
                    Label = activity?.Name ?? "Unknown"
                });
            }
        }
        else
        {
            TimelineStart = DateTime.Today.AddHours(9);
            TimelineEnd = DateTime.Today.AddHours(18);
        }

        TimelineSegments = segments;
    }

    private async Task LoadCalendarIndicatorsAsync()
    {
        var firstOfMonth = new DateOnly(SelectedDate.Year, SelectedDate.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        var monthRecords = await _timeRecordRepository.GetByDateRangeAsync(firstOfMonth, lastOfMonth);
        var grouped = monthRecords.GroupBy(r => r.Date);

        var office = new ObservableCollection<DateTime>();
        var telework = new ObservableCollection<DateTime>();
        var both = new ObservableCollection<DateTime>();

        foreach (var group in grouped)
        {
            var hasOffice = group.Any(r => !r.Telework);
            var hasTelework = group.Any(r => r.Telework);
            var dateTime = group.Key.ToDateTime(TimeOnly.MinValue);

            if (hasOffice && hasTelework)
                both.Add(dateTime);
            else if (hasOffice)
                office.Add(dateTime);
            else if (hasTelework)
                telework.Add(dateTime);
        }

        OfficeDates = office;
        TeleworkDates = telework;
        BothDates = both;
    }

    private string GetDayTypeString(DayType dayType)
    {
        return dayType switch
        {
            DayType.WorkDay => _localizationService.GetString("Today_DayType_WorkDay"),
            DayType.IntensiveDay => _localizationService.GetString("Today_DayType_IntensiveDay"),
            DayType.Holiday => _localizationService.GetString("Today_DayType_Holiday"),
            DayType.FreeChoice => _localizationService.GetString("Today_DayType_FreeChoice"),
            DayType.Vacation => _localizationService.GetString("Today_DayType_Vacation"),
            _ => dayType.ToString()
        };
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        var totalMinutes = (int)Math.Abs(ts.TotalMinutes);
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours}h {minutes:D2}m";
    }
}

/// <summary>
/// Display model for an activity breakdown item in the dashboard.
/// </summary>
public class ActivityBreakdownDisplay
{
    public string ActivityName { get; set; } = string.Empty;
    public string Color { get; set; } = "#808080";
    public SolidColorBrush ColorBrush { get; set; } = Brushes.Gray;
    public string TotalTime { get; set; } = "0h 0m";
    public string Percentage { get; set; } = "0%";
    public double PercentageValue { get; set; }
}

/// <summary>
/// Display model for a time record in the day dashboard.
/// </summary>
public class DayRecordDisplay
{
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityColor { get; set; } = "#808080";
    public SolidColorBrush ColorBrush { get; set; } = Brushes.Gray;
    public string StartTime { get; set; } = "--:--";
    public string EndTime { get; set; } = "--:--";
    public string Duration { get; set; } = "--:--";
    public string Notes { get; set; } = string.Empty;
    public bool IsTelework { get; set; }
}

