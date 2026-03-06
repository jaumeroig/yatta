namespace TimeTracker.Core.Services;

using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of <see cref="IDashboardService"/> that aggregates data from
/// repositories and calculators to produce dashboard reports.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IWorkdayConfigService _workdayConfigService;
    private readonly ITimeCalculatorService _timeCalculatorService;

    public DashboardService(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        IWorkdayConfigService workdayConfigService,
        ITimeCalculatorService timeCalculatorService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _workdayConfigService = workdayConfigService;
        _timeCalculatorService = timeCalculatorService;
    }

    /// <inheritdoc/>
    public async Task<DayReport> GetDayReportAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var records = (await _timeRecordRepository.GetByDateAsync(date)).OrderBy(r => r.StartTime).ToList();
        var activities = (await _activityRepository.GetAllAsync()).ToDictionary(a => a.Id);
        var dayType = await _workdayConfigService.GetDayTypeAsync(date, cancellationToken);
        var targetDuration = await _workdayConfigService.GetTargetDurationAsync(date, cancellationToken);

        var totalHours = _timeCalculatorService.CalculateTotalHours(records);
        var officeHours = _timeCalculatorService.CalculateOfficeHours(records);
        var teleworkHours = _timeCalculatorService.CalculateTeleworkHours(records);
        var teleworkPercentage = _timeCalculatorService.CalculateTeleworkPercentage(records);

        var workedDuration = TimeSpan.FromHours(totalHours);
        var officeDuration = TimeSpan.FromHours(officeHours);
        var teleworkDuration = TimeSpan.FromHours(teleworkHours);

        var activityBreakdown = BuildActivityBreakdown(records, activities);
        var startTime = records.Count > 0 ? records[0].StartTime : (TimeOnly?)null;

        return new DayReport
        {
            Date = date,
            DayType = dayType,
            StartTime = startTime,
            TargetDuration = targetDuration,
            WorkedDuration = workedDuration,
            Differential = workedDuration - targetDuration,
            OfficeTime = officeDuration,
            TeleworkTime = teleworkDuration,
            TeleworkPercentage = teleworkPercentage,
            Activities = activityBreakdown,
            Records = records
        };
    }

    /// <inheritdoc/>
    public async Task<PeriodReport> GetWeekReportAsync(DateOnly anyDayInWeek, CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = GetWeekRange(anyDayInWeek);
        return await BuildPeriodReportAsync(startDate, endDate, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PeriodReport> GetMonthReportAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return await BuildPeriodReportAsync(startDate, endDate, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PeriodReport> GetYearReportAsync(int year, CancellationToken cancellationToken = default)
    {
        var startDate = new DateOnly(year, 1, 1);
        var endDate = new DateOnly(year, 12, 31);
        return await BuildPeriodReportAsync(startDate, endDate, cancellationToken);
    }

    private async Task<PeriodReport> BuildPeriodReportAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var records = (await _timeRecordRepository.GetByDateRangeAsync(startDate, endDate)).ToList();
        var activities = (await _activityRepository.GetAllAsync()).ToDictionary(a => a.Id);
        var dayTypeCounts = await _workdayConfigService.GetDayTypeCountsAsync(startDate, endDate, cancellationToken);

        // Calculate accumulated target by iterating each day
        var totalTarget = TimeSpan.Zero;
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            totalTarget += await _workdayConfigService.GetTargetDurationAsync(d, cancellationToken);
        }

        var totalHours = _timeCalculatorService.CalculateTotalHours(records);
        var officeHours = _timeCalculatorService.CalculateOfficeHours(records);
        var teleworkHours = _timeCalculatorService.CalculateTeleworkHours(records);
        var teleworkPercentage = _timeCalculatorService.CalculateTeleworkPercentage(records);

        var totalWorked = TimeSpan.FromHours(totalHours);
        var officeDuration = TimeSpan.FromHours(officeHours);
        var teleworkDuration = TimeSpan.FromHours(teleworkHours);

        // Build daily breakdown for chart
        var dailyBreakdown = BuildDailyBreakdown(records, startDate, endDate);
        var activityBreakdown = BuildActivityBreakdown(records, activities);

        return new PeriodReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalTarget = totalTarget,
            TotalWorked = totalWorked,
            Differential = totalWorked - totalTarget,
            OfficeTime = officeDuration,
            TeleworkTime = teleworkDuration,
            TeleworkPercentage = teleworkPercentage,
            Activities = activityBreakdown,
            DayTypeCounts = dayTypeCounts,
            DailyBreakdown = dailyBreakdown
        };
    }

    private List<ActivityBreakdownItem> BuildActivityBreakdown(
        List<TimeRecord> records,
        Dictionary<Guid, Activity> activities)
    {
        var totalHours = _timeCalculatorService.CalculateTotalHours(records);
        if (totalHours <= 0)
            return [];

        var grouped = records
            .GroupBy(r => r.ActivityId)
            .Select(g =>
            {
                var activityRecords = g.ToList();
                var hours = _timeCalculatorService.CalculateTotalHours(activityRecords);
                var activity = activities.GetValueOrDefault(g.Key);

                return new ActivityBreakdownItem
                {
                    ActivityId = g.Key,
                    ActivityName = activity?.Name ?? "Unknown",
                    Color = activity?.Color ?? "#808080",
                    TotalTime = TimeSpan.FromHours(hours),
                    Percentage = totalHours > 0 ? hours / totalHours * 100.0 : 0
                };
            })
            .OrderByDescending(a => a.TotalTime)
            .ToList();

        return grouped;
    }

    private static List<DailyHoursSummary> BuildDailyBreakdown(
        List<TimeRecord> records,
        DateOnly startDate,
        DateOnly endDate)
    {
        var recordsByDate = records
            .GroupBy(r => r.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var breakdown = new List<DailyHoursSummary>();
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            if (recordsByDate.TryGetValue(d, out var dayRecords))
            {
                var officeHours = dayRecords.Where(r => !r.Telework).Sum(r => CalculateRecordHours(r));
                var teleworkHours = dayRecords.Where(r => r.Telework).Sum(r => CalculateRecordHours(r));

                breakdown.Add(new DailyHoursSummary
                {
                    Date = d,
                    OfficeHours = officeHours,
                    TeleworkHours = teleworkHours,
                    TotalHours = officeHours + teleworkHours
                });
            }
            else
            {
                breakdown.Add(new DailyHoursSummary
                {
                    Date = d,
                    OfficeHours = 0,
                    TeleworkHours = 0,
                    TotalHours = 0
                });
            }
        }

        return breakdown;
    }

    private static double CalculateRecordHours(TimeRecord record)
    {
        if (record.EndTime is null)
            return 0;

        var duration = record.EndTime.Value.ToTimeSpan() - record.StartTime.ToTimeSpan();
        return duration.TotalHours;
    }

    private static (DateOnly Start, DateOnly End) GetWeekRange(DateOnly date)
    {
        // ISO 8601: Week starts on Monday
        var dayOfWeek = date.DayOfWeek;
        var daysToMonday = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
        var monday = date.AddDays(-daysToMonday);
        var sunday = monday.AddDays(6);
        return (monday, sunday);
    }
}
