namespace TimeTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Threading;
using TimeTracker.App.Controls;
using TimeTracker.App.Models;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AppResources = TimeTracker.App.Resources.Resources;

/// <summary>
/// ViewModel for the Today page.
/// </summary>
public partial class TodayViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IWorkdayConfigService _workdayConfigService;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INotificationService _notificationService;
    private readonly DispatcherTimer _timer;
    private List<Activity> _allActivities = [];

    [ObservableProperty]
    private string _currentDate = string.Empty;

    [ObservableProperty]
    private string _currentTime = string.Empty;

    [ObservableProperty]
    private string _dayTypeDisplay = string.Empty;

    [ObservableProperty]
    private string _targetDuration = string.Empty;

    [ObservableProperty]
    private string _workdayStartTime = string.Empty;

    [ObservableProperty]
    private string _remainingTime = string.Empty;

    [ObservableProperty]
    private string _estimatedEndTime = "--:--";

    [ObservableProperty]
    private string _workedTime = "0h 0m";

    [ObservableProperty]
    private bool _isWorkingDay = true;

    [ObservableProperty]
    private TimeRecordDisplay? _activeRecord;

    [ObservableProperty]
    private ObservableCollection<TimeRecordDisplay> _completedRecords = [];

    [ObservableProperty]
    private ObservableCollection<TimeSegment> _timelineSegments = [];

    [ObservableProperty]
    private DateTime _workdayStart = DateTime.Today.AddHours(9);

    [ObservableProperty]
    private DateTime _workdayEnd = DateTime.Today.AddHours(18);

    [ObservableProperty]
    private string _elapsedTime = "0h 0m";

    [ObservableProperty]
    private bool _hasActiveRecord;

    [ObservableProperty]
    private bool _canPlay;

    [ObservableProperty]
    private bool _isConfigureDayDialogOpen;

    [ObservableProperty]
    private ConfigureDayModel _configureDayModel = new();

    [ObservableProperty]
    private bool _isChangeActivityDialogOpen;

    [ObservableProperty]
    private ChangeActivityModel _changeActivityModel = new();

    [ObservableProperty]
    private bool _isEditRecordDialogOpen;

    [ObservableProperty]
    private TimeRecordEditModel _editRecordModel = new();

    [ObservableProperty]
    private bool _isDeleteConfirmationOpen;

    private TimeRecordDisplay? _pendingDeleteRecord;

    public TodayViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        IWorkdayConfigService workdayConfigService,
        ITimeCalculatorService timeCalculatorService,
        INotificationService notificationService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _workdayConfigService = workdayConfigService;
        _timeCalculatorService = timeCalculatorService;
        _notificationService = notificationService;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        UpdateDateTime();
    }

    /// <summary>
    /// Loads initial data for the page.
    /// </summary>
    public async Task LoadDataAsync()
    {
        _allActivities = (await _activityRepository.GetActiveAsync()).ToList();
        await LoadDayConfigurationAsync();
        await LoadTodayRecordsAsync();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateDateTime();
        UpdateElapsedTime();
        UpdateWorkedTime();
        UpdateRemainingTime();
        UpdateActiveSegmentEnd();
    }

    private void UpdateDateTime()
    {
        var now = DateTime.Now;
        CurrentDate = now.ToString("D").Capitalize();
        CurrentTime = now.ToString("HH:mm");
    }

    private async Task LoadDayConfigurationAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var workday = await _workdayConfigService.GetEffectiveConfigurationAsync(today);
        var isWorking = await _workdayConfigService.IsWorkingDayAsync(today);

        IsWorkingDay = isWorking;
        DayTypeDisplay = GetDayTypeDisplayName(workday.DayType);

        if (isWorking)
        {
            TargetDuration = FormatDuration(workday.TargetDuration.TotalHours);
        }
        else
        {
            TargetDuration = TimeTracker.App.Resources.Resources.Today_NoWorkingDay;
            RemainingTime = TimeTracker.App.Resources.Resources.Today_NoWorkingDay;
        }
    }

    private async Task LoadTodayRecordsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var records = (await _timeRecordRepository.GetByDateAsync(today)).OrderByDescending(r => r.StartTime).ToList();

        var activeRecord = records.FirstOrDefault(r => r.EndTime == null);
        var completedRecords = records.Where(r => r.EndTime != null).ToList();

        HasActiveRecord = activeRecord != null;
        CanPlay = activeRecord == null;

        if (activeRecord != null)
        {
            ActiveRecord = CreateRecordDisplay(activeRecord, true);
            UpdateElapsedTime();
        }
        else
        {
            ActiveRecord = null;
            ElapsedTime = "0h 0m";
        }

        CompletedRecords = new ObservableCollection<TimeRecordDisplay>(
            completedRecords.Select(r => CreateRecordDisplay(r, false)));

        UpdateTimelineSegments(records);
        CalculateWorkedTime(records);
        UpdateRemainingTime();
        UpdateWorkdayStartTime(records);
    }

    private TimeRecordDisplay CreateRecordDisplay(TimeRecord record, bool isActive)
    {
        var activity = _allActivities.FirstOrDefault(a => a.Id == record.ActivityId);
        var duration = record.EndTime.HasValue
            ? _timeCalculatorService.CalculateDuration(record.StartTime, record.EndTime.Value)
            : 0;

        return new TimeRecordDisplay
        {
            Id = record.Id,
            ActivityName = activity?.Name ?? TimeTracker.App.Resources.Resources.Activity_Unknown,
            ActivityColor = activity?.Color ?? "#808080",
            Notes = record.Notes ?? string.Empty,
            StartTime = record.StartTime.ToString("HH:mm"),
            EndTime = record.EndTime?.ToString("HH:mm") ?? "--:--",
            Duration = FormatDuration(duration),
            Date = record.Date,
            IsActive = isActive,
            Telework = record.Telework
        };
    }

    [RelayCommand]
    private async Task OpenEditRecordDialogAsync(TimeRecordDisplay recordDisplay)
    {
        var record = await _timeRecordRepository.GetByIdAsync(recordDisplay.Id);
        if (record == null)
        {
            return;
        }

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
            EndTimeText = record.EndTime?.ToString("HH:mm") ?? string.Empty,
            Notes = record.Notes ?? string.Empty,
            Telework = record.Telework
        };

        IsEditRecordDialogOpen = true;
    }

    [RelayCommand]
    private async Task SaveEditRecordAsync()
    {
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

        try
        {
            var existingRecord = await _timeRecordRepository.GetByIdAsync(EditRecordModel.RecordId);
            var wasActive = existingRecord?.EndTime == null;
            var isNowActive = !record.EndTime.HasValue;

            await _timeRecordRepository.UpdateAsync(record);

            if (isNowActive || wasActive)
            {
                _notificationService.ResetTimer();
            }

            IsEditRecordDialogOpen = false;
            await LoadTodayRecordsAsync();
        }
        catch (Exception)
        {
            EditRecordModel.ValidationError = AppResources.Validation_RecordSaveError;
        }
    }

    [RelayCommand]
    private void CloseEditRecordDialog()
    {
        IsEditRecordDialogOpen = false;
    }

    [RelayCommand]
    private void RequestDeleteRecord(TimeRecordDisplay recordDisplay)
    {
        _pendingDeleteRecord = recordDisplay;
        IsDeleteConfirmationOpen = true;
    }

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
            await LoadTodayRecordsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting record: {ex.Message}");
            if (EditRecordModel != null)
            {
                EditRecordModel.ValidationError = $"Error deleting record: {ex.Message}";
            }
        }
        finally
        {
            _pendingDeleteRecord = null;
            IsDeleteConfirmationOpen = false;
        }
    }

    [RelayCommand]
    private void CancelDeleteRecord()
    {
        _pendingDeleteRecord = null;
        IsDeleteConfirmationOpen = false;
    }

    private void UpdateTimelineSegments(List<TimeRecord> records)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var segments = records
            .OrderBy(r => r.StartTime)
            .Select(r =>
            {
                var activity = _allActivities.FirstOrDefault(a => a.Id == r.ActivityId);
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

                var endTime = r.EndTime ?? now;

                return new TimeSegment
                {
                    RecordId = r.Id,
                    Label = activity?.Name ?? TimeTracker.App.Resources.Resources.Activity_Unknown,
                    Start = today.ToDateTime(r.StartTime),
                    End = today.ToDateTime(endTime),
                    Color = color,
                    IsActive = r.EndTime == null
                };
            })
            .ToList();

        TimelineSegments = new ObservableCollection<TimeSegment>(segments);

        // Set WorkdayStart/WorkdayEnd to match the actual records exactly
        if (records.Count > 0)
        {
            var minStart = records.Min(r => r.StartTime);
            var maxEnd = records.Max(r => r.EndTime ?? TimeOnly.FromDateTime(DateTime.Now));

            WorkdayStart = today.ToDateTime(minStart);
            WorkdayEnd = today.ToDateTime(maxEnd);

            if (WorkdayEnd <= WorkdayStart)
            {
                WorkdayEnd = WorkdayStart.AddMinutes(30);
            }
        }
        else
        {
            WorkdayStart = DateTime.Today.AddHours(9);
            WorkdayEnd = DateTime.Today.AddHours(18);
        }
    }

    private void CalculateWorkedTime(List<TimeRecord> records)
    {
        var totalHours = CalculateTotalHoursIncludingActive(records);
        WorkedTime = FormatDuration(totalHours);
    }

    private async void UpdateWorkedTime()
    {
        if (!HasActiveRecord) return;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var records = (await _timeRecordRepository.GetByDateAsync(today)).ToList();
        var totalHours = CalculateTotalHoursIncludingActive(records);
        WorkedTime = FormatDuration(totalHours);
    }

    private double CalculateTotalHoursIncludingActive(List<TimeRecord> records)
    {
        var totalHours = _timeCalculatorService.CalculateTotalHours(records);

        var activeRecord = records.FirstOrDefault(r => r.EndTime == null);
        if (activeRecord != null)
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            totalHours += _timeCalculatorService.CalculateDuration(activeRecord.StartTime, now);
        }

        return totalHours;
    }

    private void UpdateElapsedTime()
    {
        if (ActiveRecord == null) return;

        var now = TimeOnly.FromDateTime(DateTime.Now);
        var startTime = TimeOnly.Parse(ActiveRecord.StartTime);
        var elapsed = _timeCalculatorService.CalculateDuration(startTime, now);
        ElapsedTime = FormatDuration(elapsed);
    }

    private void UpdateActiveSegmentEnd()
    {
        if (!HasActiveRecord || TimelineSegments.Count == 0) return;

        var lastSegment = TimelineSegments[^1];
        var now = DateTime.Now;
        var newEnd = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        if (lastSegment.End != newEnd)
        {
            lastSegment.End = newEnd;

            // Update WorkdayEnd to match the active segment end
            if (newEnd > WorkdayEnd)
                WorkdayEnd = newEnd;

            // Force re-render by replacing the collection
            TimelineSegments = new ObservableCollection<TimeSegment>(TimelineSegments);
        }
    }

    private async void UpdateRemainingTime()
    {
        if (!IsWorkingDay) return;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var records = (await _timeRecordRepository.GetByDateAsync(today)).ToList();
        var workedHours = CalculateTotalHoursIncludingActive(records);

        var workedDuration = TimeSpan.FromHours(workedHours);
        var remaining = await _workdayConfigService.GetRemainingWorkTimeAsync(today, workedDuration);

        RemainingTime = remaining > TimeSpan.Zero ? FormatTimeSpan(remaining) : "0h 0m";
        if (remaining > TimeSpan.Zero)
        {
            var endTime = DateTime.Now.Add(remaining);
            // Round up to the next minute if there are any remaining seconds
            if (endTime.Second > 0)
                endTime = endTime.AddMinutes(1);
            EstimatedEndTime = endTime.ToString("HH:mm");
        }
        else
        {
            EstimatedEndTime = "--:--";
        }
    }

    private void UpdateWorkdayStartTime(List<TimeRecord> records)
    {
        if (!records.Any())
        {
            WorkdayStartTime = "--:--";
            return;
        }

        var firstRecord = records.OrderBy(r => r.StartTime).First();
        WorkdayStartTime = firstRecord.StartTime.ToString("HH:mm");
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = TimeTracker.App.Resources.Resources.Format_Duration;
        return string.Format(format, h, m);
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        var totalMinutes = (int)timeSpan.TotalMinutes;
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = TimeTracker.App.Resources.Resources.Format_Duration;
        return string.Format(format, h, m);
    }

    private static string GetDayTypeDisplayName(DayType dayType)
    {
        return dayType switch
        {
            DayType.WorkDay => TimeTracker.App.Resources.Resources.Today_DayType_WorkDay,
            DayType.IntensiveDay => TimeTracker.App.Resources.Resources.Today_DayType_IntensiveDay,
            DayType.Holiday => TimeTracker.App.Resources.Resources.Today_DayType_Holiday,
            DayType.FreeChoice => TimeTracker.App.Resources.Resources.Today_DayType_FreeChoice,
            DayType.Vacation => TimeTracker.App.Resources.Resources.Today_DayType_Vacation,
            _ => string.Empty
        };
    }

    [RelayCommand]
    private async Task StopRecordAsync()
    {
        if (ActiveRecord == null) return;

        var record = await _timeRecordRepository.GetByIdAsync(ActiveRecord.Id);
        if (record == null) return;

        record.EndTime = TimeOnly.FromDateTime(DateTime.Now);
        await _timeRecordRepository.UpdateAsync(record);

        await LoadTodayRecordsAsync();
    }

    [RelayCommand]
    private async Task SegmentResizedAsync(SegmentResizeResult? result)
    {
        if (result?.ResizedSegment.RecordId == null) return;

        // Update the primary resized segment
        await UpdateSegmentRecordAsync(result.ResizedSegment);

        // Update the affected neighbor if it was pushed/shrunk
        if (result.AffectedNeighbor?.RecordId != null)
        {
            await UpdateSegmentRecordAsync(result.AffectedNeighbor);
        }

        await LoadTodayRecordsAsync();
    }

    /// <summary>
    /// Persists a segment's current Start/End times to the database.
    /// For active records (EndTime == null), only StartTime is updated.
    /// </summary>
    private async Task UpdateSegmentRecordAsync(TimeSegment segment)
    {
        if (segment.RecordId == null) return;

        var record = await _timeRecordRepository.GetByIdAsync(segment.RecordId.Value);
        if (record == null) return;

        record.StartTime = TimeOnly.FromDateTime(segment.Start);

        // Only update EndTime for completed records; active records keep null EndTime
        if (record.EndTime != null)
        {
            record.EndTime = TimeOnly.FromDateTime(segment.End);
        }

        await _timeRecordRepository.UpdateAsync(record);
    }

    [RelayCommand]
    private async Task PlayRecordAsync()
    {
        // Open the change activity dialog to start a new record
        await OpenChangeActivityDialogAsync();
    }

    [RelayCommand]
    private async Task ChangeActivityAsync()
    {
        await OpenChangeActivityDialogAsync();
    }

    /// <summary>
    /// Opens the change/start activity dialog, pre-populating from the active record if one exists.
    /// </summary>
    private async Task OpenChangeActivityDialogAsync()
    {
        var now = DateTime.Now;
        var model = new ChangeActivityModel
        {
            AvailableActivities = new ObservableCollection<Activity>(_allActivities),
            StartDate = now.Date,
            StartTimeText = now.ToString("HH:mm"),
            Notes = string.Empty,
            Telework = false,
            ValidationError = string.Empty
        };

        if (ActiveRecord != null)
        {
            // Pre-select the current activity and copy its properties
            var activeTimeRecord = await _timeRecordRepository.GetActiveAsync();
            if (activeTimeRecord != null)
            {
                model.SelectedActivityId = activeTimeRecord.ActivityId;
                model.Telework = activeTimeRecord.Telework;
                model.Notes = activeTimeRecord.Notes ?? string.Empty;

                // Store original values to detect changes
                model.OriginalActivityId = activeTimeRecord.ActivityId;
                model.OriginalTelework = activeTimeRecord.Telework;
                model.OriginalNotes = activeTimeRecord.Notes ?? string.Empty;
                model.OriginalStartTimeText = now.ToString("HH:mm");
            }

            model.HasActiveRecord = true;
        }
        else
        {
            model.HasActiveRecord = false;
            model.SelectedActivityId = Guid.Empty;
        }

        ChangeActivityModel = model;
        IsChangeActivityDialogOpen = true;
    }

    [RelayCommand]
    private void CloseChangeActivityDialog()
    {
        IsChangeActivityDialogOpen = false;
    }

    [RelayCommand]
    private async Task SaveChangeActivityAsync()
    {
        // Validate that an activity is selected
        if (ChangeActivityModel.SelectedActivityId == Guid.Empty)
        {
            ChangeActivityModel.ValidationError = Resources.Resources.Validation_ActivityRequired;
            return;
        }

        // Parse the start time
        if (!TimeOnly.TryParse(ChangeActivityModel.StartTimeText, out var switchTime))
        {
            ChangeActivityModel.ValidationError = Resources.Resources.Validation_InvalidStartTime;
            return;
        }

        var switchDate = DateOnly.FromDateTime(ChangeActivityModel.StartDate);

        // If there's an active record, finalize it with the switch time as EndTime
        if (ChangeActivityModel.HasActiveRecord)
        {
            var activeRecord = await _timeRecordRepository.GetActiveAsync();
            if (activeRecord != null)
            {
                activeRecord.EndTime = switchTime;
                await _timeRecordRepository.UpdateAsync(activeRecord);
            }
        }

        // Create the new time record
        var newRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = switchDate,
            StartTime = switchTime,
            EndTime = null,
            ActivityId = ChangeActivityModel.SelectedActivityId,
            Notes = string.IsNullOrWhiteSpace(ChangeActivityModel.Notes) ? null : ChangeActivityModel.Notes,
            Telework = ChangeActivityModel.Telework
        };

        await _timeRecordRepository.AddAsync(newRecord);

        // Reset notification timer when activity changes
        _notificationService.ResetTimer();

        // Close dialog and reload data
        IsChangeActivityDialogOpen = false;
        await LoadTodayRecordsAsync();
    }

    [RelayCommand]
    private async Task ConfigureDayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentConfig = await _workdayConfigService.GetEffectiveConfigurationAsync(today);
        
        ConfigureDayModel = new ConfigureDayModel
        {
            Date = today,
            DayType = currentConfig.DayType,
            TargetDurationHours = currentConfig.TargetDuration.TotalHours,
            TargetDurationText = FormatHoursToHHmm(currentConfig.TargetDuration.TotalHours),
            ValidationError = string.Empty
        };
        
        IsConfigureDayDialogOpen = true;
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

    [RelayCommand]
    private void CloseConfigureDayDialog()
    {
        IsConfigureDayDialogOpen = false;
    }

    [RelayCommand]
    private async Task SaveConfigureDayAsync()
    {
        // Validate
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
        
        // Save configuration
            
        await _workdayConfigService.SetConfigurationAsync(
            ConfigureDayModel.Date,
            ConfigureDayModel.DayType,
            targetDuration);
        
        // Close dialog and reload
        IsConfigureDayDialogOpen = false;
        await LoadDayConfigurationAsync();
    }
}

/// <summary>
/// Represents a day type option for the dropdown.
/// </summary>
public class DayTypeOption
{
    /// <summary>
    /// Day type value.
    /// </summary>
    public DayType Value { get; set; }

    /// <summary>
    /// Name to display in the UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// Model for the Configure Day dialog.
/// </summary>
public partial class ConfigureDayModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogTitle))]
    private DateOnly _date;

    [ObservableProperty]
    private DayType _dayType;

    [ObservableProperty]
    private double _targetDurationHours = 8;

    [ObservableProperty]
    private string _targetDurationText = string.Empty;

    [ObservableProperty]
    private string _validationError = string.Empty;

    /// <summary>
    /// Title shown in the dialog header, including the date in short format.
    /// </summary>
    public string DialogTitle =>
        $"{TimeTracker.App.Resources.Resources.Dialog_ConfigureDay_Title} {Date.ToString("d")}";

    /// <summary>
    /// Available day type options for the dropdown.
    /// </summary>
    public static List<DayTypeOption> AvailableDayTypes { get; } =
    [
        new DayTypeOption { Value = DayType.WorkDay, DisplayName = TimeTracker.App.Resources.Resources.Today_DayType_WorkDay },
        new DayTypeOption { Value = DayType.IntensiveDay, DisplayName = TimeTracker.App.Resources.Resources.Today_DayType_IntensiveDay },
        new DayTypeOption { Value = DayType.Holiday, DisplayName = TimeTracker.App.Resources.Resources.Today_DayType_Holiday },
        new DayTypeOption { Value = DayType.FreeChoice, DisplayName = TimeTracker.App.Resources.Resources.Today_DayType_FreeChoice },
        new DayTypeOption { Value = DayType.Vacation, DisplayName = TimeTracker.App.Resources.Resources.Today_DayType_Vacation },
    ];

    public bool IsWorkingDayType => DayType == DayType.WorkDay || DayType == DayType.IntensiveDay;

    partial void OnDayTypeChanged(DayType value)
    {
        OnPropertyChanged(nameof(IsWorkingDayType));
    }
}

/// <summary>
/// Model for the Change/Start Activity dialog.
/// Holds the state for switching or starting a new time record.
/// </summary>
public partial class ChangeActivityModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Activity> _availableActivities = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private Guid _selectedActivityId;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private string _startTimeText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private bool _telework;

    /// <summary>
    /// Inverse of Telework for radio button binding (office mode).
    /// </summary>
    public bool IsOffice
    {
        get => !Telework;
        set
        {
            if (value)
            {
                Telework = false;
            }
        }
    }

    partial void OnTeleworkChanged(bool value)
    {
        OnPropertyChanged(nameof(IsOffice));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _validationError = string.Empty;

    [ObservableProperty]
    private bool _hasActiveRecord;

    // Original values to detect if any field was modified
    public Guid OriginalActivityId { get; set; }
    public bool OriginalTelework { get; set; }
    public string OriginalNotes { get; set; } = string.Empty;
    public string OriginalStartTimeText { get; set; } = string.Empty;

    /// <summary>
    /// Returns the dialog title based on whether there is an active record.
    /// </summary>
    public string DialogTitle => HasActiveRecord
        ? TimeTracker.App.Resources.Resources.Dialog_ChangeActivity_Title
        : TimeTracker.App.Resources.Resources.Dialog_StartActivity_Title;

    /// <summary>
    /// Returns the primary button text based on whether there is an active record.
    /// </summary>
    public string PrimaryButtonText => HasActiveRecord
        ? TimeTracker.App.Resources.Resources.Button_Change
        : TimeTracker.App.Resources.Resources.Button_Start;

    /// <summary>
    /// Determines if the form is valid and has been modified.
    /// When no active record exists, validates that an activity is selected and the start time is valid.
    /// When an active record exists, checks if any field has been modified relative to the original values.
    /// </summary>
    public bool HasChanges
    {
        get
        {
            if (!HasActiveRecord)
            {
                return SelectedActivityId != Guid.Empty
                    && TimeOnly.TryParse(StartTimeText, out _);
            }

            return SelectedActivityId != OriginalActivityId
                || Telework != OriginalTelework
                || !string.Equals(Notes, OriginalNotes, StringComparison.Ordinal)
                || !string.Equals(StartTimeText, OriginalStartTimeText, StringComparison.Ordinal);
        }
    }

    partial void OnHasActiveRecordChanged(bool value)
    {
        OnPropertyChanged(nameof(DialogTitle));
        OnPropertyChanged(nameof(PrimaryButtonText));
        OnPropertyChanged(nameof(HasChanges));
    }
}
