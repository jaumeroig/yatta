namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.App.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.Views.Pages;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel for time records management.
/// </summary>
public partial class TimeRecordViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INavigationService _navigationService;
    private List<TimeRecord> _allRecords = [];
    private List<Activity> _allActivities = [];

    [ObservableProperty]
    private ObservableCollection<DayGroup> _groupedRecords = [];

    [ObservableProperty]
    private ObservableCollection<Activity> _activities = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Activity? _selectedActivityFilter;

    [ObservableProperty]
    private DateTime? _selectedDate;

    [ObservableProperty]
    private string _todayWorkedTime = "0h 0m";

    public TimeRecordViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Loads initial data.
    /// </summary>
    public async Task LoadDataAsync()
    {
        _allActivities = (await _activityRepository.GetActiveAsync()).ToList();

        // Add "All activities" option at the beginning
        var allActivitiesText = Resources.Resources.Filter_AllActivities;
        var allActivitiesOption = new Activity { Id = Guid.Empty, Name = allActivitiesText };
        var activitiesWithAll = new List<Activity> { allActivitiesOption };
        activitiesWithAll.AddRange(_allActivities);
        Activities = new ObservableCollection<Activity>(activitiesWithAll);

        _allRecords = (await _timeRecordRepository.GetAllAsync()).ToList();
        ApplyFilters();
        CalculateTodayWorkedTime();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedActivityFilterChanged(Activity? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedDateChanged(DateTime? value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allRecords.AsEnumerable();

        // Filter by text (search in notes and activity name)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(r =>
                (r.Notes?.Contains(searchLower, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                _allActivities.FirstOrDefault(a => a.Id == r.ActivityId)?.Name.Contains(searchLower, StringComparison.InvariantCultureIgnoreCase) == true);
        }

        // Filter by activity (except "All activities")
        if (SelectedActivityFilter != null && SelectedActivityFilter.Id != Guid.Empty)
        {
            filtered = filtered.Where(r => r.ActivityId == SelectedActivityFilter.Id);
        }

        // Filter by date
        if (SelectedDate.HasValue)
        {
            var date = DateOnly.FromDateTime(SelectedDate.Value);
            filtered = filtered.Where(r => r.Date == date);
        }

        // Group by day
        var groups = filtered
            .GroupBy(r => r.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new DayGroup
            {
                Date = g.Key,
                DateDisplay = FormatDate(g.Key),
                TotalWorked = FormatDuration(_timeCalculatorService.CalculateTotalHours(g)),
                Records = new ObservableCollection<TimeRecordDisplay>(
                    g.OrderBy(r => r.StartTime).Select(r => CreateRecordDisplay(r))),
                TimelineSegments = new ObservableCollection<TimeSegment>(
                    g.Where(r => r.EndTime.HasValue)
                     .OrderBy(r => r.StartTime)
                     .Select(r => CreateTimelineSegment(r, g.Key)))
            });

        GroupedRecords = new ObservableCollection<DayGroup>(groups);
    }

    private TimeRecordDisplay CreateRecordDisplay(TimeRecord record)
    {
        var activity = _allActivities.FirstOrDefault(a => a.Id == record.ActivityId);
        var duration = record.EndTime.HasValue
            ? _timeCalculatorService.CalculateDuration(record.StartTime, record.EndTime.Value)
            : 0;

        return new TimeRecordDisplay
        {
            Id = record.Id,
            ActivityName = activity?.Name ?? Resources.Resources.Activity_Unknown,
            ActivityColor = activity?.Color ?? "#808080",
            Notes = record.Notes ?? string.Empty,
            StartTime = record.StartTime.ToString("HH:mm"),
            EndTime = record.EndTime?.ToString("HH:mm") ?? "--:--",
            Duration = FormatDuration(duration),
            Date = record.Date
        };
    }

    private TimeSegment CreateTimelineSegment(TimeRecord record, DateOnly date)
    {
        var activity = _allActivities.FirstOrDefault(a => a.Id == record.ActivityId);
        var color = Colors.Gray;
        if (activity?.Color != null)
        {
            try
            {
                color = (Color)ColorConverter.ConvertFromString(activity.Color);
            }
            catch
            {
                // Keep default gray
            }
        }

        return new TimeSegment
        {
            Label = activity?.Name ?? Resources.Resources.Activity_Unknown,
            Start = date.ToDateTime(record.StartTime),
            End = date.ToDateTime(record.EndTime!.Value),
            Color = color
        };
    }

    private static string FormatDate(DateOnly date)
    {
        return date.ToLongDateString();
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = Resources.Resources.Format_Duration;
        return string.Format(format, h, m);
    }

    private void CalculateTodayWorkedTime()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayRecords = _allRecords.Where(r => r.Date == today);
        var totalHours = _timeCalculatorService.CalculateTotalHours(todayRecords);
        TodayWorkedTime = FormatDuration(totalHours);
    }

    /// <summary>
    /// Navigates to the detail page to create a new record.
    /// </summary>
    [RelayCommand]
    private void NavigateToNewRecord()
    {
        _navigationService.Navigate<TimeRecordDetailPage>(null);
    }

    /// <summary>
    /// Navigates to the detail page to edit an existing record.
    /// </summary>
    [RelayCommand]
    private void NavigateToRecord(TimeRecordDisplay record)
    {
        _navigationService.Navigate<TimeRecordDetailPage>(record.Id);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedActivityFilter = null;
        SelectedDate = null;
    }
}

/// <summary>
/// Group of records per day.
/// </summary>
public class DayGroup
{
    public DateOnly Date { get; set; }
    public string DateDisplay { get; set; } = string.Empty;
    public string TotalWorked { get; set; } = string.Empty;
    public ObservableCollection<TimeRecordDisplay> Records { get; set; } = [];
    public ObservableCollection<TimeSegment> TimelineSegments { get; set; } = [];
}

/// <summary>
/// Display model for a time record.
/// </summary>
public class TimeRecordDisplay
{
    public Guid Id { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityColor { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public DateOnly Date { get; set; }

    /// <summary>
    /// Returns the color as a SolidColorBrush to facilitate binding.
    /// </summary>
    public System.Windows.Media.SolidColorBrush ActivityColorBrush
    {
        get
        {
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ActivityColor);
                return new System.Windows.Media.SolidColorBrush(color);
            }
            catch
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }
    }
}
