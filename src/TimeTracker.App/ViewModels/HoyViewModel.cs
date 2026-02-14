namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.App.Controls;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel for the Today (Hoy) page.
/// </summary>
public partial class HoyViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IWorkdayConfigService _workdayConfigService;
    private readonly ITimeCalculatorService _timeCalculatorService;
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

    public HoyViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        IWorkdayConfigService workdayConfigService,
        ITimeCalculatorService timeCalculatorService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _workdayConfigService = workdayConfigService;
        _timeCalculatorService = timeCalculatorService;

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
        UpdateRemainingTime();
        UpdateActiveSegmentEnd();
    }

    private void UpdateDateTime()
    {
        var now = DateTime.Now;
        CurrentDate = now.ToString("D");
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
        CanPlay = activeRecord == null && records.Any();

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
            IsActive = isActive
        };
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
                    Label = activity?.Name ?? TimeTracker.App.Resources.Resources.Activity_Unknown,
                    Start = today.ToDateTime(r.StartTime),
                    End = today.ToDateTime(endTime),
                    Color = color
                };
            })
            .ToList();

        TimelineSegments = new ObservableCollection<TimeSegment>(segments);

        // Calculate dynamic WorkdayStart/WorkdayEnd from actual records
        if (records.Count > 0)
        {
            var minStart = records.Min(r => r.StartTime);
            var maxEnd = records.Max(r => r.EndTime ?? TimeOnly.FromDateTime(DateTime.Now));

            // Add 30min padding and clamp to 6:00-23:00.
            // Use DateTime arithmetic to avoid TimeOnly wrap-around (e.g., 00:10 - 30min => 23:40).
            var paddedStart = today.ToDateTime(minStart).AddMinutes(-30);
            var paddedEnd = today.ToDateTime(maxEnd).AddMinutes(30);

            var clampMin = today.ToDateTime(new TimeOnly(6, 0));
            var clampMax = today.ToDateTime(new TimeOnly(23, 0));

            WorkdayStart = paddedStart < clampMin ? clampMin : paddedStart;
            WorkdayEnd = paddedEnd > clampMax ? clampMax : paddedEnd;

            if (WorkdayEnd <= WorkdayStart)
            {
                WorkdayStart = today.ToDateTime(minStart);
                WorkdayEnd = today.ToDateTime(maxEnd);

                if (WorkdayEnd <= WorkdayStart)
                {
                    WorkdayEnd = WorkdayStart.AddMinutes(30);
                }
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
        var totalHours = _timeCalculatorService.CalculateTotalHours(records);
        WorkedTime = FormatDuration(totalHours);
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

            // Update WorkdayEnd if needed
            var paddedEnd = newEnd.AddMinutes(30);
            var clampMax = DateTime.Today.AddHours(23);
            var candidateEnd = paddedEnd > clampMax ? clampMax : paddedEnd;
            if (candidateEnd > WorkdayEnd)
                WorkdayEnd = candidateEnd;

            // Force re-render by replacing the collection
            TimelineSegments = new ObservableCollection<TimeSegment>(TimelineSegments);
        }
    }

    private async void UpdateRemainingTime()
    {
        if (!IsWorkingDay) return;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var records = (await _timeRecordRepository.GetByDateAsync(today)).ToList();
        var workedHours = _timeCalculatorService.CalculateTotalHours(records);

        var activeRecord = records.FirstOrDefault(r => r.EndTime == null);
        if (activeRecord != null)
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var elapsed = _timeCalculatorService.CalculateDuration(activeRecord.StartTime, now);
            workedHours += elapsed;
        }

        var workedDuration = TimeSpan.FromHours(workedHours);
        var remaining = await _workdayConfigService.GetRemainingWorkTimeAsync(today, workedDuration);

        RemainingTime = remaining > TimeSpan.Zero ? FormatTimeSpan(remaining) : "0h 0m";
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
            ValidationError = string.Empty
        };
        
        IsConfigureDayDialogOpen = true;
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
        
        if (isWorkingDay)
        {
            if (ConfigureDayModel.TargetDurationHours <= 0)
            {
                ConfigureDayModel.ValidationError = TimeTracker.App.Resources.Resources.Validation_TargetDurationInvalid;
                return;
            }
        }
        
        // Save configuration
        var targetDuration = isWorkingDay 
            ? TimeSpan.FromHours(ConfigureDayModel.TargetDurationHours) 
            : TimeSpan.Zero;
            
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
/// Model for the Configure Day dialog.
/// </summary>
public partial class ConfigureDayModel : ObservableObject
{
    [ObservableProperty]
    private DateOnly _date;

    [ObservableProperty]
    private DayType _dayType;

    [ObservableProperty]
    private double _targetDurationHours = 8;

    [ObservableProperty]
    private string _validationError = string.Empty;

    public bool IsWorkDay
    {
        get => DayType == DayType.WorkDay;
        set { if (value) DayType = DayType.WorkDay; }
    }

    public bool IsIntensiveDay
    {
        get => DayType == DayType.IntensiveDay;
        set { if (value) DayType = DayType.IntensiveDay; }
    }

    public bool IsHoliday
    {
        get => DayType == DayType.Holiday;
        set { if (value) DayType = DayType.Holiday; }
    }

    public bool IsFreeChoice
    {
        get => DayType == DayType.FreeChoice;
        set { if (value) DayType = DayType.FreeChoice; }
    }

    public bool IsVacation
    {
        get => DayType == DayType.Vacation;
        set { if (value) DayType = DayType.Vacation; }
    }

    public bool IsWorkingDayType => DayType == DayType.WorkDay || DayType == DayType.IntensiveDay;

    partial void OnDayTypeChanged(DayType value)
    {
        OnPropertyChanged(nameof(IsWorkDay));
        OnPropertyChanged(nameof(IsIntensiveDay));
        OnPropertyChanged(nameof(IsHoliday));
        OnPropertyChanged(nameof(IsFreeChoice));
        OnPropertyChanged(nameof(IsVacation));
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
    /// Determines if any field has been modified relative to the original active record values.
    /// When no active record exists, always returns true (enabling "Start").
    /// </summary>
    public bool HasChanges
    {
        get
        {
            if (!HasActiveRecord)
            {
                return true;
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
