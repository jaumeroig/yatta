namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.App.Controls;
using TimeTracker.App.Helpers;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel for workday management.
/// Now based on TimeRecord instead of WorkdaySlot.
/// </summary>
public partial class JornadaViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IWorkdayService _workdayService;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly ILocalizationService _localizationService;
    private List<TimeRecord> _allRecords = [];
    private Dictionary<Guid, Activity> _activitiesCache = [];

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private string _selectedDateDisplay = string.Empty;

    [ObservableProperty]
    private ObservableCollection<WorkdayRecordDisplay> _slots = [];

    [ObservableProperty]
    private string _totalWorkedTime = "0h 0m";

    [ObservableProperty]
    private string _teleworkPercentage = "0%";

    [ObservableProperty]
    private string _officeTime = "0h 0m";

    [ObservableProperty]
    private string _teleworkTime = "0h 0m";

    [ObservableProperty]
    private string _monthYear = string.Empty;

    [ObservableProperty]
    private string _monthTotalTime = "0h 0m";

    [ObservableProperty]
    private string _monthTeleworkPercentage = "0%";

    // Properties for bar chart
    [ObservableProperty]
    private double _officeHoursValue;

    [ObservableProperty]
    private double _teleworkHoursValue;

    [ObservableProperty]
    private double _officeBarWidth;

    [ObservableProperty]
    private double _teleworkBarWidth;

    // Timeline segments for WorkdayTimelineBar
    [ObservableProperty]
    private ObservableCollection<TimeSegment> _timelineSegments = [];

    // Dynamic start/end for the timeline bar (derived from actual records)
    [ObservableProperty]
    private DateTime _timelineStart = DateTime.Today.AddHours(9);

    [ObservableProperty]
    private DateTime _timelineEnd = DateTime.Today.AddHours(18);

    // Dates with records (for calendar)
    [ObservableProperty]
    private ObservableCollection<DateTime> _datesWithRecords = [];

    // Dates categorized by location for calendar indicators
    [ObservableProperty]
    private ObservableCollection<DateTime> _teleworkDates = [];

    [ObservableProperty]
    private ObservableCollection<DateTime> _officeDates = [];

    [ObservableProperty]
    private ObservableCollection<DateTime> _bothDates = [];

    public JornadaViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        IWorkdayService workdayService,
        ITimeCalculatorService timeCalculatorService,
        ILocalizationService localizationService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _workdayService = workdayService;
        _timeCalculatorService = timeCalculatorService;
        _localizationService = localizationService;

        UpdateDateDisplay();
        UpdateMonthYearDisplay();
    }

    /// <summary>
    /// Loads initial data.
    /// </summary>
    public async Task LoadDataAsync()
    {
        await LoadActivitiesCacheAsync();
        await LoadRecordsForDateAsync(SelectedDate);
        await UpdateMonthlySummaryAsync();
        await LoadDatesWithRecordsAsync();
    }

    /// <summary>
    /// Loads the activities cache for displaying activity names.
    /// </summary>
    private async Task LoadActivitiesCacheAsync()
    {
        var activities = await _activityRepository.GetAllAsync();
        _activitiesCache = activities.ToDictionary(a => a.Id, a => a);
    }

    /// <summary>
    /// Loads the dates in the month that have records, categorized by location.
    /// </summary>
    private async Task LoadDatesWithRecordsAsync()
    {
        var firstDay = new DateOnly(SelectedDate.Year, SelectedDate.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var monthRecords = await _timeRecordRepository.GetByDateRangeAsync(firstDay, lastDay);

        var telework = new HashSet<DateTime>();
        var office = new HashSet<DateTime>();
        var both = new HashSet<DateTime>();
        var allDates = new HashSet<DateTime>();

        foreach (var record in monthRecords)
        {
            var dt = record.Date.ToDateTime(TimeOnly.MinValue);
            allDates.Add(dt);

            if (record.Telework)
            {
                if (office.Contains(dt)) both.Add(dt); else telework.Add(dt);
            }
            else
            {
                if (telework.Contains(dt)) both.Add(dt); else office.Add(dt);
            }
        }

        // Remove dates from single sets if they are in both
        foreach (var dt in both)
        {
            telework.Remove(dt);
            office.Remove(dt);
        }

        DatesWithRecords = new ObservableCollection<DateTime>(allDates.OrderBy(d => d));
        TeleworkDates = new ObservableCollection<DateTime>(telework.OrderBy(d => d));
        OfficeDates = new ObservableCollection<DateTime>(office.OrderBy(d => d));
        BothDates = new ObservableCollection<DateTime>(both.OrderBy(d => d));
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        UpdateDateDisplay();
        _ = LoadRecordsForDateAsync(value);
    }

    private async Task LoadRecordsForDateAsync(DateTime date)
    {
        var dateOnly = DateOnly.FromDateTime(date);
        _allRecords = (await _timeRecordRepository.GetByDateAsync(dateOnly)).ToList();
        UpdateRecordsDisplay();
        UpdateDailySummary();
    }

    private void UpdateRecordsDisplay()
    {
        // Group records by location (office / telework) and aggregate times
        var groups = _allRecords
            .GroupBy(r => r.Telework)
            .OrderBy(g => g.Min(r => r.StartTime))
            .Select(group =>
            {
                var records = group.OrderBy(r => r.StartTime).ToList();
                var firstStart = records.First().StartTime;
                var hasInProgress = records.Any(r => !r.EndTime.HasValue);
                var lastEnd = hasInProgress
                    ? (TimeOnly?)null
                    : records.Max(r => r.EndTime!.Value);

                // Sum total hours for all records (use current time for in-progress ones)
                var now = TimeOnly.FromDateTime(DateTime.Now);
                var totalHours = records
                    .Sum(r => _timeCalculatorService.CalculateDuration(r.StartTime, r.EndTime ?? now));

                var isTelework = group.Key;

                return new WorkdayRecordDisplay
                {
                    StartTime = firstStart.ToString("HH:mm"),
                    EndTime = lastEnd?.ToString("HH:mm") ?? Resources.Resources.Today_InProgress,
                    Duration = DurationFormatHelper.FormatDuration(totalHours),
                    LocationText = isTelework
                        ? Resources.Resources.Location_Telework
                        : Resources.Resources.Location_Office,
                    LocationIcon = isTelework ? "Home24" : "Building24",
                    Telework = isTelework
                };
            });

        Slots = new ObservableCollection<WorkdayRecordDisplay>(groups);
        UpdateTimelineSegments();
    }

    private void UpdateTimelineSegments()
    {
        var date = DateOnly.FromDateTime(SelectedDate);
        var now = DateTime.Now;

        // Build segments: include records with EndTime, and also in-progress records (use current time)
        var segments = _allRecords
            .OrderBy(r => r.StartTime)
            .Where(r => r.EndTime.HasValue || date == DateOnly.FromDateTime(now))
            .Select(record => new TimeSegment
            {
                Label = record.Telework
                    ? Resources.Resources.Location_Telework
                    : Resources.Resources.Location_Office,
                Start = date.ToDateTime(record.StartTime),
                End = record.EndTime.HasValue
                    ? date.ToDateTime(record.EndTime.Value)
                    : now,
                Color = record.Telework
                    ? Color.FromRgb(0x21, 0x96, 0xF3)  // #2196F3 blue
                    : Color.FromRgb(0x4C, 0xAF, 0x50)  // #4CAF50 green
            })
            .ToList();

        TimelineSegments = new ObservableCollection<TimeSegment>(segments);

        // Calculate dynamic timeline bounds from all records (including in-progress)
        if (_allRecords.Count > 0)
        {
            var minStart = _allRecords.Min(r => r.StartTime);
            var maxEnd = _allRecords.Max(r => r.EndTime ?? TimeOnly.FromDateTime(now));

            TimelineStart = date.ToDateTime(minStart);
            TimelineEnd = date.ToDateTime(maxEnd);

            if (TimelineEnd <= TimelineStart)
            {
                TimelineEnd = TimelineStart.AddMinutes(30);
            }
        }
        else
        {
            // Fallback: default range when no records
            TimelineStart = date.ToDateTime(new TimeOnly(9, 0));
            TimelineEnd = date.ToDateTime(new TimeOnly(18, 0));
        }
    }

    private void UpdateDailySummary()
    {
        var totalHours = _timeCalculatorService.CalculateTotalHours(_allRecords);
        var teleworkHours = _timeCalculatorService.CalculateTeleworkHours(_allRecords);
        var officeHours = _timeCalculatorService.CalculateOfficeHours(_allRecords);
        var percentage = _timeCalculatorService.CalculateTeleworkPercentage(_allRecords);

        TotalWorkedTime = DurationFormatHelper.FormatDuration(totalHours);
        TeleworkTime = DurationFormatHelper.FormatDuration(teleworkHours);
        OfficeTime = DurationFormatHelper.FormatDuration(officeHours);
        TeleworkPercentage = $"{percentage:F0}%";

        // Update values for bar chart
        OfficeHoursValue = officeHours;
        TeleworkHoursValue = teleworkHours;
        UpdateBarWidths();
    }

    private void UpdateBarWidths()
    {
        (OfficeBarWidth, TeleworkBarWidth) = DashboardDisplayHelper.CalculateBarWidths(
            OfficeHoursValue, TeleworkHoursValue);
    }

    private async Task UpdateMonthlySummaryAsync()
    {
        var firstDay = new DateOnly(SelectedDate.Year, SelectedDate.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var totalHours = await _workdayService.GetTotalHoursAsync(firstDay, lastDay);
        var percentage = await _workdayService.GetTeleworkPercentageAsync(firstDay, lastDay);

        MonthTotalTime = DurationFormatHelper.FormatDuration(totalHours);
        MonthTeleworkPercentage = $"{percentage:F0}%";
    }

    private void UpdateDateDisplay()
    {
        var culture = Resources.Resources.Culture ?? CultureInfo.CurrentCulture;
        var longDate = SelectedDate.ToString("D", culture);
        SelectedDateDisplay = char.ToUpper(longDate[0], culture) + longDate[1..];
    }

    private void UpdateMonthYearDisplay()
    {
        var culture = Resources.Resources.Culture ?? CultureInfo.CurrentCulture;
        var month = culture.TextInfo.ToTitleCase(SelectedDate.ToString("MMMM", culture));
        MonthYear = $"{month} {SelectedDate.Year}";
    }

    [RelayCommand]
    private void PreviousDay()
    {
        SelectedDate = SelectedDate.AddDays(-1);
    }

    [RelayCommand]
    private void NextDay()
    {
        SelectedDate = SelectedDate.AddDays(1);
    }

    [RelayCommand]
    private void Today()
    {
        SelectedDate = DateTime.Today;
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        SelectedDate = SelectedDate.AddMonths(-1);
        UpdateMonthYearDisplay();
        _ = UpdateMonthlySummaryAsync();
    }

    [RelayCommand]
    private void NextMonth()
    {
        SelectedDate = SelectedDate.AddMonths(1);
        UpdateMonthYearDisplay();
        _ = UpdateMonthlySummaryAsync();
    }
}

/// <summary>
/// Display model for a time record in the workday view.
/// </summary>
public class WorkdayRecordDisplay
{
    public Guid Id { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string LocationText { get; set; } = string.Empty;
    public string LocationIcon { get; set; } = string.Empty;
    public bool Telework { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityColor { get; set; } = string.Empty;
}
