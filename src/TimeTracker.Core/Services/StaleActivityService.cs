namespace TimeTracker.Core.Services;

using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the stale activity service.
/// Detects and closes active time records from previous days on application startup.
/// </summary>
public class StaleActivityService : IStaleActivityService
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IWorkdayConfigService _workdayConfigService;
    private readonly ITimeCalculatorService _timeCalculatorService;

    /// <summary>
    /// Constructor for the stale activity service.
    /// </summary>
    /// <param name="timeRecordRepository">Time record repository.</param>
    /// <param name="workdayConfigService">Workday configuration service.</param>
    /// <param name="timeCalculatorService">Time calculator service.</param>
    public StaleActivityService(
        ITimeRecordRepository timeRecordRepository,
        IWorkdayConfigService workdayConfigService,
        ITimeCalculatorService timeCalculatorService)
    {
        _timeRecordRepository = timeRecordRepository;
        _workdayConfigService = workdayConfigService;
        _timeCalculatorService = timeCalculatorService;
    }

    /// <inheritdoc/>
    public async Task<StaleActivityResult?> CloseStaleActivitiesAsync()
    {
        var activeRecord = await _timeRecordRepository.GetActiveAsync();

        if (activeRecord == null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        // Only close activities from previous days, not today
        if (activeRecord.Date >= today)
        {
            return null;
        }

        var endTime = await CalculateEndTimeAsync(activeRecord);

        activeRecord.EndTime = endTime;
        await _timeRecordRepository.UpdateAsync(activeRecord);

        return new StaleActivityResult
        {
            Date = activeRecord.Date,
            EndTime = endTime
        };
    }

    /// <summary>
    /// Calculates the end time for a stale record based on the target duration for that day.
    /// </summary>
    /// <param name="staleRecord">The stale active record.</param>
    /// <returns>The calculated end time.</returns>
    private async Task<TimeOnly> CalculateEndTimeAsync(TimeRecord staleRecord)
    {
        var targetDuration = await _workdayConfigService.GetTargetDurationAsync(staleRecord.Date);
        var allRecords = await _timeRecordRepository.GetByDateAsync(staleRecord.Date);

        // Calculate already worked hours from completed records (excluding the stale one)
        var completedRecords = allRecords.Where(r => r.Id != staleRecord.Id && r.EndTime.HasValue);
        var workedHours = _timeCalculatorService.CalculateTotalHours(completedRecords);

        var remainingHours = targetDuration.TotalHours - workedHours;

        if (remainingHours <= 0)
        {
            // Target already reached by other records; close at the start time
            return staleRecord.StartTime;
        }

        var remainingDuration = TimeSpan.FromHours(remainingHours);
        var endTimeSpan = staleRecord.StartTime.ToTimeSpan() + remainingDuration;

        // Cap at 23:59 to stay within the same day
        if (endTimeSpan >= TimeSpan.FromDays(1))
        {
            return new TimeOnly(23, 59);
        }

        return TimeOnly.FromTimeSpan(endTimeSpan);
    }
}
