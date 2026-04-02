namespace Yatta.App.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Yatta.App.Controls;
using Yatta.App.Helpers;
using Yatta.App.Models;
using Yatta.App.Services;
using Yatta.App.Views.Pages;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;
using AppResources = Yatta.App.Resources.Resources;

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
    private readonly INotificationService _notificationService;
    private readonly IWorkdayConfigService _workdayConfigService;
    private readonly IValidationService _validationService;
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

    [ObservableProperty]
    private bool _sortAscending = false;

    [ObservableProperty]
    private bool _isEditRecordDialogOpen;

    [ObservableProperty]
    private TimeRecordEditModel _editRecordModel = new();

    [ObservableProperty]
    private bool _isDeleteConfirmationOpen;

    [ObservableProperty]
    private bool _isConfigureDayDialogOpen;

    [ObservableProperty]
    private ConfigureDayModel _configureDayModel = new();

    private TimeRecordDisplay? _pendingDeleteRecord;

    public HistoricViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService,
        ISettingsRepository settingsRepository,
        INotificationService notificationService,
        IWorkdayConfigService workdayConfigService,
        IValidationService validationService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
        _settingsRepository = settingsRepository;
        _notificationService = notificationService;
        _workdayConfigService = workdayConfigService;
        _validationService = validationService;
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
            EndTime = record.EndTime?.ToString("HH:mm") ?? TimeRecordDisplay.ActiveEndTimePlaceholder,
            Duration = FormatDuration(duration),
            Date = record.Date,
            Telework = record.Telework
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
        return date.ToString("D", culture).Capitalize();
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
    /// Opens the dialog to create a new record.
    /// </summary>
    [RelayCommand]
    private async Task OpenNewRecordDialogAsync()
    {
        EditRecordModel = new TimeRecordEditModel
        {
            DialogTitle = AppResources.Dialog_NewRecord_Title,
            RecordId = Guid.NewGuid(),
            IsNewRecord = true,
            AvailableActivities = new ObservableCollection<Activity>(_allActivities),
            SelectedActivityId = Guid.Empty,
            Date = DateTime.Today,
            StartTimeText = await GetDefaultStartTimeAsync(DateOnly.FromDateTime(DateTime.Today)),
            EndTimeText = "",
            Notes = string.Empty,
            Telework = false
        };
        IsEditRecordDialogOpen = true;
    }

    /// <summary>
    /// Opens the dialog to edit an existing record.
    /// </summary>
    [RelayCommand]
    private async Task OpenEditRecordDialogAsync(TimeRecordDisplay recordDisplay)
    {
        var record = await _timeRecordRepository.GetByIdAsync(recordDisplay.Id);
        if (record == null)
        {
            return;
        }

        // Ensure the record's activity is available for selection (might be inactive)
        var activity = await _activityRepository.GetByIdAsync(record.ActivityId);
        var availableActivities = new List<Activity>(_allActivities);
        if (activity != null && !availableActivities.Any(a => a.Id == activity.Id))
        {
            availableActivities.Insert(0, activity);
        }

        EditRecordModel = new TimeRecordEditModel
        {
            DialogTitle = AppResources.Dialog_EditRecord_Title,
            RecordId = record.Id,
            IsNewRecord = false,
            AvailableActivities = new ObservableCollection<Activity>(availableActivities),
            SelectedActivityId = record.ActivityId,
            Date = record.Date.ToDateTime(TimeOnly.MinValue),
            StartTimeText = record.StartTime.ToString("HH:mm"),
            EndTimeText = record.EndTime?.ToString("HH:mm") ?? "",
            Notes = record.Notes ?? string.Empty,
            Telework = record.Telework
        };
        IsEditRecordDialogOpen = true;
    }

    /// <summary>
    /// Saves the record from the dialog.
    /// </summary>
    [RelayCommand]
    private async Task SaveEditRecordAsync()
    {
        // Validate before saving
        if (!EditRecordModel.Validate())
        {
            return;
        }

        var startTime = TimeOnly.Parse(EditRecordModel.StartTimeText);
        TimeOnly? endTime = string.IsNullOrWhiteSpace(EditRecordModel.EndTimeText)
            ? null
            : TimeOnly.Parse(EditRecordModel.EndTimeText);

        var record = new TimeRecord
        {
            Id = EditRecordModel.RecordId,
            ActivityId = EditRecordModel.SelectedActivityId,
            Date = DateOnly.FromDateTime(EditRecordModel.Date),
            StartTime = startTime,
            EndTime = endTime,
            Notes = string.IsNullOrWhiteSpace(EditRecordModel.Notes) ? null : EditRecordModel.Notes,
            Telework = EditRecordModel.Telework
        };

        // Validate overlap with existing records on the same date
        var existingRecords = await _timeRecordRepository.GetByDateAsync(record.Date);
        if (!_validationService.ValidateNoOverlap(record, existingRecords, out var overlapError))
        {
            EditRecordModel.ValidationError = ValidationErrorHelper.Localize(overlapError);
            return;
        }

        try
        {
            if (EditRecordModel.IsNewRecord)
            {
                await _timeRecordRepository.AddAsync(record);

                // Reset notification timer if the new record is active (no end time)
                if (!record.EndTime.HasValue)
                {
                    _notificationService.ResetTimer();
                }
            }
            else
            {
                // Check if this is an active record (was active before update)
                var existingRecord = await _timeRecordRepository.GetByIdAsync(EditRecordModel.RecordId);
                var wasActive = existingRecord?.EndTime == null;
                var isNowActive = !record.EndTime.HasValue;

                await _timeRecordRepository.UpdateAsync(record);

                // Reset timer if record is still active or became active
                if (isNowActive || wasActive)
                {
                    _notificationService.ResetTimer();
                }
            }

            IsEditRecordDialogOpen = false;
            await LoadDataAsync();
        }
        catch (Exception)
        {
            // In case of unexpected error, show generic message
            EditRecordModel.ValidationError = AppResources.Validation_RecordSaveError;
        }
    }

    /// <summary>
    /// Closes the edit record dialog.
    /// </summary>
    [RelayCommand]
    private void CloseEditRecordDialog()
    {
        IsEditRecordDialogOpen = false;
    }

    /// <summary>
    /// Requests deletion of a time record (opens confirmation dialog).
    /// </summary>
    [RelayCommand]
    private void RequestDeleteRecord(TimeRecordDisplay recordDisplay)
    {
        _pendingDeleteRecord = recordDisplay;
        IsDeleteConfirmationOpen = true;
    }

    /// <summary>
    /// Confirms and executes the pending record deletion.
    /// </summary>
    [RelayCommand]
    private async Task ConfirmDeleteRecordAsync()
    {
        if (_pendingDeleteRecord == null)
        {
            return;
        }

        try
        {
            await _timeRecordRepository.DeleteAsync(_pendingDeleteRecord.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting record: {ex.Message}");
        }
        finally
        {
            _pendingDeleteRecord = null;
            IsDeleteConfirmationOpen = false;
        }
    }

    /// <summary>
    /// Cancels the pending record deletion.
    /// </summary>
    [RelayCommand]
    private void CancelDeleteRecord()
    {
        _pendingDeleteRecord = null;
        IsDeleteConfirmationOpen = false;
    }

    [RelayCommand]
    private async Task OpenEditRecordFromSegmentAsync(TimeSegment segment)
    {
        if (segment.RecordId == null)
            return;

        // Find the corresponding TimeRecordDisplay in any DayGroup
        TimeRecordDisplay? recordDisplay = null;
        foreach (var dayGroup in GroupedRecords)
        {
            recordDisplay = dayGroup.Records.FirstOrDefault(r => r.Id == segment.RecordId.Value);
            if (recordDisplay != null)
                break;
        }

        if (recordDisplay != null)
        {
            await OpenEditRecordDialogAsync(recordDisplay);
        }
    }

    [RelayCommand]
    private void RequestDeleteRecordFromSegment(TimeSegment segment)
    {
        if (segment.RecordId == null || segment.IsActive)
            return;

        // Find the corresponding TimeRecordDisplay in any DayGroup
        TimeRecordDisplay? recordDisplay = null;
        foreach (var dayGroup in GroupedRecords)
        {
            recordDisplay = dayGroup.Records.FirstOrDefault(r => r.Id == segment.RecordId.Value);
            if (recordDisplay != null)
                break;
        }

        if (recordDisplay != null)
        {
            RequestDeleteRecord(recordDisplay);
        }
    }

    /// <summary>
    /// Gets the default start time for a new record on the given date.
    /// If there are existing records, returns the end time of the last one.
    /// Otherwise, returns the current time.
    /// </summary>
    private async Task<string> GetDefaultStartTimeAsync(DateOnly date)
    {
        var records = await _timeRecordRepository.GetByDateAsync(date);
        var lastRecord = records
            .Where(r => r.EndTime.HasValue)
            .OrderByDescending(r => r.EndTime)
            .FirstOrDefault();

        if (lastRecord?.EndTime != null)
        {
            return lastRecord.EndTime.Value.ToString("HH:mm");
        }

        return DateTime.Now.ToString("HH:mm");
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedActivityFilter = null;
        SelectedDate = null;
    }

    /// <summary>
    /// Opens the configure day dialog for the specified date.
    /// </summary>
    [RelayCommand]
    private async Task ConfigureDayAsync(DateOnly date)
    {
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
                ConfigureDayModel.ValidationError = AppResources.Validation_TargetDurationInvalid;
                return;
            }
        }

        await _workdayConfigService.SetConfigurationAsync(
            ConfigureDayModel.Date,
            ConfigureDayModel.DayType,
            targetDuration);

        IsConfigureDayDialogOpen = false;
        await LoadDataAsync();
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
