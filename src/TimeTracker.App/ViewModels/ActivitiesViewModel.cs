namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.App.Services;
using TimeTracker.App.Views.Pages;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel for activities management.
/// </summary>
public partial class ActivitiesViewModel : ObservableObject
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INavigationService _navigationService;
    private List<Activity> _allActivities = [];
    private List<TimeRecord> _allRecords = [];

    [ObservableProperty]
    private ObservableCollection<ActivityDisplay> _activities = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showInactive = false;

    /// <summary>
    /// Executes when the search text changes.
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Executes when the show inactive switch changes.
    /// </summary>
    partial void OnShowInactiveChanged(bool value)
    {
        ApplyFilters();
    }

    public ActivitiesViewModel(
        IActivityRepository activityRepository,
        ITimeRecordRepository timeRecordRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService)
    {
        _activityRepository = activityRepository;
        _timeRecordRepository = timeRecordRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Loads initial data.
    /// </summary>
    public async Task LoadDataAsync()
    {
        _allActivities = (await _activityRepository.GetAllAsync()).ToList();
        _allRecords = (await _timeRecordRepository.GetAllAsync()).ToList();
        ApplyFilters();
    }

    /// <summary>
    /// Applies search and active/inactive status filters to the activities list.
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = _allActivities.AsEnumerable();

        // Filter by active/inactive status
        if (!ShowInactive)
        {
            filtered = filtered.Where(a => a.Active);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(a => a.Name.ToLower().Contains(searchLower));
        }

        var activityDisplays = filtered.Select(activity =>
        {
            var records = _allRecords.Where(r => r.ActivityId == activity.Id).ToList();
            var totalHours = _timeCalculatorService.CalculateTotalHours(records);
            var totalTime = FormatDuration(totalHours);

            // Create subtitle with format: "X records · Xh Xm"
            var recordsText = records.Count == 1
                ? Resources.Resources.Activity_SingleRecord
                : string.Format(Resources.Resources.Activity_MultipleRecords, records.Count);
            var subtitle = records.Count > 0
                ? $"{recordsText} · {totalTime}"
                : Resources.Resources.Activity_NoRecords;

            return new ActivityDisplay
            {
                Id = activity.Id,
                Name = activity.Name,
                Color = activity.Color,
                Active = activity.Active,
                RecordCount = records.Count,
                TotalTime = totalTime,
                Subtitle = subtitle,
                StatusText = activity.Active
                    ? Resources.Resources.Status_Active
                    : Resources.Resources.Status_Inactive
            };
        }).OrderBy(a => a.Name);

        Activities = new ObservableCollection<ActivityDisplay>(activityDisplays);
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = Resources.Resources.Format_Duration;
        return string.Format(format, h, m);
    }

    /// <summary>
    /// Navigates to the detail page to create a new activity.
    /// </summary>
    [RelayCommand]
    private void NavigateToNewActivity()
    {
        _navigationService.Navigate<ActivityDetailPage>(null);
    }

    /// <summary>
    /// Navigates to the detail page to edit an existing activity.
    /// </summary>
    [RelayCommand]
    private void NavigateToActivity(ActivityDisplay activity)
    {
        _navigationService.Navigate<ActivityDetailPage>(activity.Id);
    }
}

/// <summary>
/// Display model for an activity.
/// </summary>
public class ActivityDisplay
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool Active { get; set; }
    public int RecordCount { get; set; }
    public string TotalTime { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// Subtitle with summary of records and total time.
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Returns the color as a SolidColorBrush to facilitate binding.
    /// </summary>
    public System.Windows.Media.SolidColorBrush ColorBrush
    {
        get
        {
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Color);
                return new System.Windows.Media.SolidColorBrush(color);
            }
            catch
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }
    }
}

