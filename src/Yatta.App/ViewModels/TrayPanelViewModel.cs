namespace Yatta.App.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yatta.App.Models;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;
using AppResources = Yatta.App.Resources.Resources;

/// <summary>
/// ViewModel for the tray icon information panel.
/// </summary>
public partial class TrayPanelViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IWorkdayConfigService _workdayConfigService;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly DispatcherTimer _timer;
    private bool _isDisposed;

    [ObservableProperty]
    private string _currentDate = string.Empty;

    [ObservableProperty]
    private string _workedTime = "0h 0m";

    [ObservableProperty]
    private string _startTime = "--:--";

    [ObservableProperty]
    private string _workdayStatus = string.Empty;

    [ObservableProperty]
    private bool _hasActiveRecord;

    [ObservableProperty]
    private string _activityName = string.Empty;

    [ObservableProperty]
    private string _activityColor = "#0078D4";

    [ObservableProperty]
    private string _elapsedTime = "00:00";

    public TrayPanelViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        IWorkdayConfigService workdayConfigService,
        ITimeCalculatorService timeCalculatorService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _workdayConfigService = workdayConfigService;
        _timeCalculatorService = timeCalculatorService;

        // Timer to update elapsed time every second
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += async (_, _) => await UpdateElapsedTimeAsync();
        _timer.Start();
    }

    /// <summary>
    /// Loads the workday and activity data.
    /// </summary>
    public async Task LoadDataAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        CurrentDate = DateTime.Today.ToString("dddd, d MMMM yyyy");

        // Load workday configuration
        var workdayConfig = await _workdayConfigService.GetEffectiveConfigurationAsync(today);

        // Calculate worked time
        var records = await _timeRecordRepository.GetByDateAsync(today);
        var totalMinutes = 0.0;
        foreach (var record in records)
        {
            var endTime = record.EndTime ?? TimeOnly.FromDateTime(DateTime.Now);
            var duration = endTime.ToTimeSpan() - record.StartTime.ToTimeSpan();
            totalMinutes += duration.TotalMinutes;
        }

        var hours = (int)(totalMinutes / 60);
        var minutes = (int)(totalMinutes % 60);
        WorkedTime = $"{hours}h {minutes}m";

        // Get start time from first record
        var firstRecord = records.OrderBy(r => r.StartTime).FirstOrDefault();
        StartTime = firstRecord?.StartTime.ToString("HH:mm") ?? "--:--";

        // Get active record
        var activeRecord = await _timeRecordRepository.GetActiveAsync();
        HasActiveRecord = activeRecord != null;

        if (activeRecord != null)
        {
            var activity = await _activityRepository.GetByIdAsync(activeRecord.ActivityId);
            if (activity != null)
            {
                ActivityName = activity.Name;
                ActivityColor = activity.Color ?? "#0078D4";
            }

            WorkdayStatus = AppResources.TrayPanel_StatusActive;
        }
        else
        {
            WorkdayStatus = records.Any()
                ? AppResources.TrayPanel_StatusPaused
                : AppResources.TrayPanel_StatusNotStarted;
        }

        await UpdateElapsedTimeAsync();
    }

    /// <summary>
    /// Updates the elapsed time for the active record.
    /// </summary>
    private async Task UpdateElapsedTimeAsync()
    {
        // Early return if disposed to prevent accessing disposed resources
        if (_isDisposed)
        {
            return;
        }

        if (!HasActiveRecord)
        {
            ElapsedTime = "00:00";
            return;
        }

        try
        {
            var activeRecord = await _timeRecordRepository.GetActiveAsync();
            if (activeRecord == null)
            {
                ElapsedTime = "00:00";
                return;
            }

            var startDateTime = activeRecord.Date.ToDateTime(activeRecord.StartTime);
            var duration = DateTime.Now - startDateTime;
            if (duration < TimeSpan.Zero)
                duration = TimeSpan.Zero;

            ElapsedTime = $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}";
        }
        catch (ObjectDisposedException)
        {
            // Ignore if the repository/context has been disposed
        }
    }

    /// <summary>
    /// Stops the timer when the panel is closed.
    /// </summary>
    public void Cleanup()
    {
        _isDisposed = true;

        if (_timer != null)
        {
            _timer.Stop();
        }
    }
}
