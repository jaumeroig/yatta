namespace Yatta.Core.Services;

using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Implementation of the service for workday-specific logic.
/// Now based on TimeRecord instead of WorkdaySlot.
/// </summary>
public class WorkdayService : IWorkdayService
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;

    /// <summary>
    /// Constructor of the workday service.
    /// </summary>
    /// <param name="timeRecordRepository">Time records repository.</param>
    /// <param name="timeCalculatorService">Time calculation service.</param>
    public WorkdayService(
        ITimeRecordRepository timeRecordRepository,
        ITimeCalculatorService timeCalculatorService)
    {
        _timeRecordRepository = timeRecordRepository;
        _timeCalculatorService = timeCalculatorService;
    }

    /// <inheritdoc/>
    public async Task<WorkdaySummary> GetDailySummaryAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var records = await _timeRecordRepository.GetByDateAsync(date);
        var recordsList = records.ToList();

        var totalHours = _timeCalculatorService.CalculateTotalHours(recordsList);
        var teleworkHours = _timeCalculatorService.CalculateTeleworkHours(recordsList);
        var officeHours = _timeCalculatorService.CalculateOfficeHours(recordsList);
        var teleworkPercentage = _timeCalculatorService.CalculateTeleworkPercentage(recordsList);

        return new WorkdaySummary
        {
            Date = date,
            TotalHours = totalHours,
            TeleworkHours = teleworkHours,
            OfficeHours = officeHours,
            TeleworkPercentage = teleworkPercentage,
            RecordCount = recordsList.Count
        };
    }

    /// <inheritdoc/>
    public async Task<double> GetTotalHoursAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var records = await _timeRecordRepository.GetByDateRangeAsync(startDate, endDate);
        return _timeCalculatorService.CalculateTotalHours(records);
    }

    /// <inheritdoc/>
    public async Task<double> GetTeleworkPercentageAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var records = await _timeRecordRepository.GetByDateRangeAsync(startDate, endDate);
        return _timeCalculatorService.CalculateTeleworkPercentage(records);
    }
}
