namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using TimeTracker.App.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.Views.Pages;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using AppResources = TimeTracker.App.Resources.Resources;

/// <summary>
/// ViewModel for time records management.
/// </summary>
public partial class HistoricViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INavigationService _navigationService;
    private readonly ISettingsRepository _settingsRepository;
    private List<TimeRecord> _allRecords = [];
    private List<Activity> _allActivities = [];
    private bool _isLoading;

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

    [ObservableProperty]
    private bool _sortAscending = false;

    public HistoricViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService,
        ISettingsRepository settingsRepository)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
        _settingsRepository = settingsRepository;
    }

    /// <summary>
    /// Loads initial data.
    /// </summary>
    public async Task LoadDataAsync()
    {
        // Load sort preference (flag prevents saving back to DB during initial load)
        var settings = await _settingsRepository.GetAsync();
        
        SortAscending = settings.HistoricSortAscending;

        _allActivities = (await _activityRepository.GetActiveAsync()).ToList();

        // Add "All activities" option at the beginning
        var allActivitiesText = AppResources.Filter_AllActivities;
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

    /// <summary>
    /// Executes when the sort direction changes.
    /// </summary>
    partial void OnSortAscendingChanged(bool value)
    {
        if (!_isLoading)
        {
            _ = SaveSortPreferenceAsync(value);
        }

        ApplyFilters();
    }

    private async Task SaveSortPreferenceAsync(bool sortAscending)
    {
        try
        {
            var settings = await _settingsRepository.GetAsync();
            settings.HistoricSortAscending = sortAscending;
            await _settingsRepository.UpdateAsync(settings);
        }
        catch (Exception ex)
        {
            // Log error silently - don't block UI
            System.Diagnostics.Debug.WriteLine($"Error saving sort preference: {ex.Message}");
        }
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
        var groups = filtered.GroupBy(r => r.Date);

        // Apply sorting based on preference
        var orderedGroups = SortAscending
            ? groups.OrderBy(g => g.Key)           // Ascending: oldest first
            : groups.OrderByDescending(g => g.Key); // Descending: newest first

        var groupsList = orderedGroups.Select(g =>
        {
            var records = g.ToList();
            var workdayStart = g.Key.ToDateTime(records.Min(r => r.StartTime));
            var workdayEnd = CalculateWorkdayEnd(records, g.Key);

            return new DayGroup
            {
                Date = g.Key,
                DateDisplay = FormatDate(g.Key),
                TimeAgo = FormatTimeAgo(g.Key),
                TotalWorked = FormatDuration(CalculateTotalHoursWithEffectiveEnd(records, g.Key)),
                WorkdayStart = workdayStart,
                WorkdayEnd = workdayEnd,
                Records = new ObservableCollection<TimeRecordDisplay>(
                    records.OrderBy(r => r.StartTime).Select(r => CreateRecordDisplay(r))),
                TimelineSegments = new ObservableCollection<TimeSegment>(
                    records.OrderBy(r => r.StartTime)
                           .Select(r => CreateTimelineSegment(r, g.Key)))
            };
        });

        GroupedRecords = new ObservableCollection<DayGroup>(groupsList);
    }

    private TimeRecordDisplay CreateRecordDisplay(TimeRecord record)
    {
        var activity = _allActivities.FirstOrDefault(a => a.Id == record.ActivityId);
        var effectiveEnd = CalculateEffectiveEnd(record, record.Date);
        var duration = (effectiveEnd - record.Date.ToDateTime(record.StartTime)).TotalHours;

        return new TimeRecordDisplay
        {
            Id = record.Id,
            ActivityName = activity?.Name ?? AppResources.Activity_Unknown,
            ActivityColor = activity?.Color ?? "#808080",
            Notes = record.Notes ?? string.Empty,
            StartTime = record.StartTime.ToString("HH:mm"),
            EndTime = record.EndTime?.ToString("HH:mm") ?? "--:--",
            Duration = FormatDuration(duration),
            Date = record.Date
        };
    }

    private static DateTime CalculateEffectiveEnd(TimeRecord record, DateOnly date)
    {
        if (record.EndTime.HasValue)
            return date.ToDateTime(record.EndTime.Value);

        var isToday = date == DateOnly.FromDateTime(DateTime.Today);
        if (isToday)
            return DateTime.Now;

        return date.ToDateTime(record.StartTime).AddHours(1);
    }

    private static double CalculateTotalHoursWithEffectiveEnd(List<TimeRecord> records, DateOnly date)
    {
        double totalHours = 0;
        foreach (var record in records)
        {
            var start = date.ToDateTime(record.StartTime);
            var end = CalculateEffectiveEnd(record, date);
            totalHours += (end - start).TotalHours;
        }
        return totalHours;
    }

    private static DateTime CalculateWorkdayEnd(List<TimeRecord> records, DateOnly date)
    {
        var maxEnd = DateTime.MinValue;
        foreach (var record in records)
        {
            var end = CalculateEffectiveEnd(record, date);
            if (end > maxEnd)
                maxEnd = end;
        }
        return maxEnd;
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
            Label = activity?.Name ?? AppResources.Activity_Unknown,
            Start = date.ToDateTime(record.StartTime),
            End = CalculateEffectiveEnd(record, date),
            Color = color
        };
    }

    private static string FormatDate(DateOnly date)
    {
        var culture = AppResources.Culture ?? CultureInfo.CurrentCulture;
        var longDate = date.ToString("D", culture);
        return char.ToUpper(longDate[0], culture) + longDate[1..];
    }

    private static string FormatTimeAgo(DateOnly date)
    {
        var culture = AppResources.Culture ?? CultureInfo.CurrentCulture;
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        return dateTime.Humanize(culture: culture);
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = AppResources.Format_Duration;
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
        _navigationService.Navigate<HistoricDetailPage>(null);
    }

    /// <summary>
    /// Navigates to the detail page to edit an existing record.
    /// </summary>
    [RelayCommand]
    private void NavigateToRecord(TimeRecordDisplay record)
    {
        _navigationService.Navigate<HistoricDetailPage>(record.Id);
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
    public string TimeAgo { get; set; } = string.Empty;
    public string TotalWorked { get; set; } = string.Empty;
    public DateTime WorkdayStart { get; set; }
    public DateTime WorkdayEnd { get; set; }
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
    public bool IsActive { get; set; }

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
