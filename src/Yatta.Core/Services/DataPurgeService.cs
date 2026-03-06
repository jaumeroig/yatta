namespace Yatta.Core.Services;

using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Implementation of the data purge service.
/// </summary>
public class DataPurgeService : IDataPurgeService
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IWorkdayRepository _workdayRepository;

    public DataPurgeService(
        ITimeRecordRepository timeRecordRepository,
        IWorkdayRepository workdayRepository)
    {
        _timeRecordRepository = timeRecordRepository;
        _workdayRepository = workdayRepository;
    }

    /// <inheritdoc/>
    public DateOnly? CalculateCutoffDate(RetentionPolicy policy, int customDays, DateOnly? referenceDate = null)
    {
        var today = referenceDate ?? DateOnly.FromDateTime(DateTime.Today);

        return policy switch
        {
            RetentionPolicy.Forever => null,
            RetentionPolicy.OneYear => today.AddYears(-1),
            RetentionPolicy.TwoYears => today.AddYears(-2),
            RetentionPolicy.ThreeYears => today.AddYears(-3),
            RetentionPolicy.Custom => customDays > 0 ? today.AddDays(-customDays) : null,
            _ => null
        };
    }

    /// <inheritdoc/>
    public async Task<(int TimeRecordCount, int WorkdayCount)> GetPurgeableCountAsync(DateOnly cutoffDate)
    {
        var timeRecordCount = await _timeRecordRepository.CountBeforeDateAsync(cutoffDate);
        var workdayCount = await _workdayRepository.CountBeforeDateAsync(cutoffDate);

        return (timeRecordCount, workdayCount);
    }

    /// <inheritdoc/>
    public async Task<(int TimeRecordsDeleted, int WorkdaysDeleted)> ExecutePurgeAsync(DateOnly cutoffDate)
    {
        var timeRecordsDeleted = await _timeRecordRepository.DeleteBeforeDateAsync(cutoffDate);
        var workdaysDeleted = await _workdayRepository.DeleteBeforeDateAsync(cutoffDate);

        return (timeRecordsDeleted, workdaysDeleted);
    }
}
