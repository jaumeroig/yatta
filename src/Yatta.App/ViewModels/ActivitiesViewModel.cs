namespace Yatta.App.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yatta.App.Helpers;
using Yatta.App.Services;
using Yatta.App.Views.Pages;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Criteria for sorting activities in the list.
/// </summary>
public enum ActivitySortCriteria
{
    /// <summary>
    /// Sort by activity name.
    /// </summary>
    Name,

    /// <summary>
    /// Sort by last record date.
    /// </summary>
    LastRecordDate
}

/// <summary>
/// ViewModel for activities management.
/// </summary>
public partial class ActivitiesViewModel : ObservableObject
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INavigationService _navigationService;
    private readonly IPageStateService _pageStateService;
    private List<Activity> _allActivities = [];
    private List<TimeRecord> _allRecords = [];

    [ObservableProperty]
    private ObservableCollection<ActivityDisplay> _activities = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showInactive = false;

    [ObservableProperty]
    private ActivitySortCriteria _selectedSortCriteria;

    /// <summary>
    /// Available sort options for the activities list.
    /// </summary>
    public List<SortOption> AvailableSortCriteria { get; }

    /// <summary>
    /// Executes when the search text changes.
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        _pageStateService.ActivitiesPage.SearchText = value;
        ApplyFilters();
    }

    /// <summary>
    /// Executes when the show inactive switch changes.
    /// </summary>
    partial void OnShowInactiveChanged(bool value)
    {
        _pageStateService.ActivitiesPage.ShowInactive = value;
        ApplyFilters();
    }

    /// <summary>
    /// Executes when the sort criteria changes.
    /// </summary>
    partial void OnSelectedSortCriteriaChanged(ActivitySortCriteria value)
    {
        _pageStateService.ActivitiesPage.SelectedSortCriteria = value;
        ApplyFilters();
    }

    public ActivitiesViewModel(
        IActivityRepository activityRepository,
        ITimeRecordRepository timeRecordRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService,
        IPageStateService pageStateService)
    {
        _activityRepository = activityRepository;
        _timeRecordRepository = timeRecordRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
        _pageStateService = pageStateService;

        // Initialize sort options with localized display names
        AvailableSortCriteria =
        [
            new SortOption(ActivitySortCriteria.Name, Resources.Resources.Sort_ByName),
            new SortOption(ActivitySortCriteria.LastRecordDate, Resources.Resources.Sort_ByLastRecordDate)
        ];

        // Restore filter state from previous session
        SearchText = _pageStateService.ActivitiesPage.SearchText;
        ShowInactive = _pageStateService.ActivitiesPage.ShowInactive;
        SelectedSortCriteria = _pageStateService.ActivitiesPage.SelectedSortCriteria;
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
    /// Applies search, active/inactive status filters and sort criteria to the activities list.
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
            filtered = filtered.Where(a =>
                a.Name.ToLower().Contains(searchLower) ||
                (a.JiraCode != null && a.JiraCode.ToLower().Contains(searchLower)));
        }

        var activityDisplays = filtered.Select(activity =>
        {
            var records = _allRecords.Where(r => r.ActivityId == activity.Id).ToList();
            var totalHours = _timeCalculatorService.CalculateTotalHours(records);
            var totalTime = DurationFormatHelper.FormatDuration(totalHours);
            var lastRecordDate = records.Any()
                ? records.Max(r => r.Date)
                : (DateOnly?)null;

            // Create subtitle with format: "X records · Xh Xm · Última imp. dd/MM/yyyy"
            var recordsText = records.Count == 1
                ? Resources.Resources.Activity_SingleRecord
                : string.Format(Resources.Resources.Activity_MultipleRecords, records.Count);
            var subtitle = records.Count > 0
                ? $"{recordsText} · {totalTime} · {Resources.Resources.Activity_LastRecordPrefix} {lastRecordDate:d}"
                : Resources.Resources.Activity_NoRecords;

            return new ActivityDisplay
            {
                Id = activity.Id,
                Name = activity.Name,
                Color = activity.Color,
                JiraCode = activity.JiraCode,
                Active = activity.Active,
                RecordCount = records.Count,
                TotalTime = totalTime,
                LastRecordDate = lastRecordDate,
                Subtitle = subtitle,
                StatusText = activity.Active
                    ? Resources.Resources.Status_Active
                    : Resources.Resources.Status_Inactive
            };
        });

        // Apply sort criteria
        activityDisplays = SelectedSortCriteria switch
        {
            ActivitySortCriteria.LastRecordDate => activityDisplays
                .OrderByDescending(a => a.LastRecordDate)
                .ThenBy(a => a.Name),
            _ => activityDisplays.OrderBy(a => a.Name)
        };

        Activities = new ObservableCollection<ActivityDisplay>(activityDisplays);
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
/// Represents a sort option with a display name.
/// </summary>
public class SortOption
{
    /// <summary>
    /// The sort criteria value.
    /// </summary>
    public ActivitySortCriteria Value { get; }

    /// <summary>
    /// The localized display name.
    /// </summary>
    public string DisplayName { get; }

    public SortOption(ActivitySortCriteria value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
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
    public string? JiraCode { get; set; }
    public bool Active { get; set; }
    public int RecordCount { get; set; }
    public string TotalTime { get; set; } = string.Empty;
    public DateOnly? LastRecordDate { get; set; }
    public string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// Subtitle with summary of records, total time and last record date.
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

