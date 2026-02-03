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
    private string _elapsedTime = "0h 0m";

    [ObservableProperty]
    private bool _hasActiveRecord;

    [ObservableProperty]
    private bool _canPlay;

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
    }

    private void UpdateDateTime()
    {
        var now = DateTime.Now;
        CurrentDate = now.ToString("D");
        CurrentTime = now.ToString("HH:mm:ss");
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
        var segments = records
            .Where(r => r.EndTime.HasValue)
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

                return new TimeSegment
                {
                    Label = activity?.Name ?? TimeTracker.App.Resources.Resources.Activity_Unknown,
                    Start = today.ToDateTime(r.StartTime),
                    End = today.ToDateTime(r.EndTime!.Value),
                    Color = color
                };
            });

        TimelineSegments = new ObservableCollection<TimeSegment>(segments);
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
        // Get the last completed record to inherit its properties
        var today = DateOnly.FromDateTime(DateTime.Today);
        var records = (await _timeRecordRepository.GetByDateAsync(today)).OrderByDescending(r => r.StartTime).ToList();
        var lastRecord = records.FirstOrDefault(r => r.EndTime != null);

        if (lastRecord == null) return;

        // Create new record with same properties as last record
        var newRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = today,
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            EndTime = null,
            ActivityId = lastRecord.ActivityId,
            Notes = lastRecord.Notes
        };

        await _timeRecordRepository.AddAsync(newRecord);
        await LoadTodayRecordsAsync();
    }

    [RelayCommand]
    private void ChangeActivity()
    {
        // TODO: Open change activity dialog
        // This will be implemented when the change activity dialog is created
    }

    [RelayCommand]
    private void ConfigureDay()
    {
        // TODO: Open configure day dialog
        // This will be implemented when the configure day dialog is created
    }
}
