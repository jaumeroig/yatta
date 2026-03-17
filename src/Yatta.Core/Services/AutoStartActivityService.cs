namespace Yatta.Core.Services;

using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Starts the timer automatically on application startup using the previous day's last activity.
/// </summary>
public class AutoStartActivityService : IAutoStartActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ITimeRecordRepository _timeRecordRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoStartActivityService"/> class.
    /// </summary>
    /// <param name="activityRepository">Activity repository.</param>
    /// <param name="settingsRepository">Settings repository.</param>
    /// <param name="timeRecordRepository">Time record repository.</param>
    public AutoStartActivityService(
        IActivityRepository activityRepository,
        ISettingsRepository settingsRepository,
        ITimeRecordRepository timeRecordRepository)
    {
        _activityRepository = activityRepository;
        _settingsRepository = settingsRepository;
        _timeRecordRepository = timeRecordRepository;
    }

    /// <inheritdoc/>
    public async Task<TimeRecord?> TryStartPreviousDayActivityAsync()
    {
        var settings = await _settingsRepository.GetAsync();
        if (!settings.StartTimerOnStartup)
        {
            return null;
        }

        var activeRecord = await _timeRecordRepository.GetActiveAsync();
        if (activeRecord != null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayRecords = await _timeRecordRepository.GetByDateAsync(today);
        if (todayRecords.Any())
        {
            return null;
        }

        var yesterday = today.AddDays(-1);
        var yesterdayRecords = await _timeRecordRepository.GetByDateAsync(yesterday);
        var lastRecord = yesterdayRecords
            .OrderByDescending(record => record.EndTime ?? record.StartTime)
            .FirstOrDefault();

        if (lastRecord == null)
        {
            return null;
        }

        var activity = await _activityRepository.GetByIdAsync(lastRecord.ActivityId);
        if (activity == null || !activity.Active)
        {
            return null;
        }

        var now = DateTime.Now;
        var newRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(now),
            StartTime = TimeOnly.FromDateTime(now),
            ActivityId = lastRecord.ActivityId,
            Telework = lastRecord.Telework
        };

        return await _timeRecordRepository.AddAsync(newRecord);
    }
}
