namespace TimeTracker.Tests.Services;

using Moq;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using TimeTracker.Core.Services;

/// <summary>
/// Unit tests for the DashboardService.
/// </summary>
public class DashboardServiceTests
{
    private readonly Mock<ITimeRecordRepository> _mockTimeRecordRepository;
    private readonly Mock<IActivityRepository> _mockActivityRepository;
    private readonly Mock<IWorkdayConfigService> _mockWorkdayConfigService;
    private readonly Mock<ITimeCalculatorService> _mockTimeCalculatorService;
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _mockTimeRecordRepository = new Mock<ITimeRecordRepository>();
        _mockActivityRepository = new Mock<IActivityRepository>();
        _mockWorkdayConfigService = new Mock<IWorkdayConfigService>();
        _mockTimeCalculatorService = new Mock<ITimeCalculatorService>();
        _sut = new DashboardService(
            _mockTimeRecordRepository.Object,
            _mockActivityRepository.Object,
            _mockWorkdayConfigService.Object,
            _mockTimeCalculatorService.Object);
    }

    #region GetDayReportAsync Tests

    [Fact]
    public async Task GetDayReportAsync_WithRecords_ReturnsCorrectReport()
    {
        // Arrange
        var date = new DateOnly(2026, 2, 20);
        var activityId1 = Guid.NewGuid();
        var activityId2 = Guid.NewGuid();

        var records = new List<TimeRecord>
        {
            new() { Id = Guid.NewGuid(), Date = date, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(12, 0), ActivityId = activityId1, Telework = false },
            new() { Id = Guid.NewGuid(), Date = date, StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(17, 0), ActivityId = activityId2, Telework = true }
        };

        var activities = new List<Activity>
        {
            new() { Id = activityId1, Name = "Development", Color = "#FF0000", Active = true },
            new() { Id = activityId2, Name = "Meetings", Color = "#00FF00", Active = true }
        };

        _mockTimeRecordRepository.Setup(r => r.GetByDateAsync(date)).ReturnsAsync(records);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        _mockWorkdayConfigService.Setup(s => s.GetDayTypeAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(DayType.WorkDay);
        _mockWorkdayConfigService.Setup(s => s.GetTargetDurationAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(TimeSpan.FromHours(8));
        _mockTimeCalculatorService.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(8.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(4.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(4.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>())).Returns(50.0);

        // Act
        var result = await _sut.GetDayReportAsync(date);

        // Assert
        Assert.Equal(date, result.Date);
        Assert.Equal(DayType.WorkDay, result.DayType);
        Assert.Equal(new TimeOnly(8, 0), result.StartTime);
        Assert.Equal(TimeSpan.FromHours(8), result.TargetDuration);
        Assert.Equal(TimeSpan.FromHours(8), result.WorkedDuration);
        Assert.Equal(TimeSpan.Zero, result.Differential);
        Assert.Equal(TimeSpan.FromHours(4), result.OfficeTime);
        Assert.Equal(TimeSpan.FromHours(4), result.TeleworkTime);
        Assert.Equal(50.0, result.TeleworkPercentage);
        Assert.Equal(2, result.Records.Count);
        Assert.Equal(2, result.Activities.Count);
    }

    [Fact]
    public async Task GetDayReportAsync_WithNoRecords_ReturnsEmptyReport()
    {
        // Arrange
        var date = new DateOnly(2026, 2, 20);

        _mockTimeRecordRepository.Setup(r => r.GetByDateAsync(date)).ReturnsAsync(new List<TimeRecord>());
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Activity>());
        _mockWorkdayConfigService.Setup(s => s.GetDayTypeAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(DayType.WorkDay);
        _mockWorkdayConfigService.Setup(s => s.GetTargetDurationAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(TimeSpan.FromHours(8));
        _mockTimeCalculatorService.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);

        // Act
        var result = await _sut.GetDayReportAsync(date);

        // Assert
        Assert.Equal(date, result.Date);
        Assert.Null(result.StartTime);
        Assert.Equal(TimeSpan.FromHours(8), result.TargetDuration);
        Assert.Equal(TimeSpan.Zero, result.WorkedDuration);
        Assert.Equal(TimeSpan.FromHours(-8), result.Differential);
        Assert.Empty(result.Records);
        Assert.Empty(result.Activities);
    }

    [Fact]
    public async Task GetDayReportAsync_Holiday_ReturnsZeroTarget()
    {
        // Arrange
        var date = new DateOnly(2026, 12, 25);

        _mockTimeRecordRepository.Setup(r => r.GetByDateAsync(date)).ReturnsAsync(new List<TimeRecord>());
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Activity>());
        _mockWorkdayConfigService.Setup(s => s.GetDayTypeAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(DayType.Holiday);
        _mockWorkdayConfigService.Setup(s => s.GetTargetDurationAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(TimeSpan.Zero);
        _mockTimeCalculatorService.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);

        // Act
        var result = await _sut.GetDayReportAsync(date);

        // Assert
        Assert.Equal(DayType.Holiday, result.DayType);
        Assert.Equal(TimeSpan.Zero, result.TargetDuration);
        Assert.Equal(TimeSpan.Zero, result.Differential);
    }

    [Fact]
    public async Task GetDayReportAsync_Overtime_ReturnsPositiveDifferential()
    {
        // Arrange
        var date = new DateOnly(2026, 2, 20);

        _mockTimeRecordRepository.Setup(r => r.GetByDateAsync(date)).ReturnsAsync(new List<TimeRecord>());
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Activity>());
        _mockWorkdayConfigService.Setup(s => s.GetDayTypeAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(DayType.WorkDay);
        _mockWorkdayConfigService.Setup(s => s.GetTargetDurationAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(TimeSpan.FromHours(8));
        _mockTimeCalculatorService.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(9.5);
        _mockTimeCalculatorService.Setup(s => s.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(9.5);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);

        // Act
        var result = await _sut.GetDayReportAsync(date);

        // Assert
        Assert.True(result.Differential > TimeSpan.Zero);
        Assert.Equal(TimeSpan.FromHours(1.5), result.Differential);
    }

    #endregion

    #region GetWeekReportAsync Tests

    [Fact]
    public async Task GetWeekReportAsync_CalculatesCorrectWeekRange()
    {
        // Arrange - Wednesday Feb 20, 2026 -> Week should be Mon Feb 16 - Sun Feb 22
        var wednesday = new DateOnly(2026, 2, 20); // Friday actually, let me calculate
        // 2026-02-20 is a Friday. Monday = 2026-02-16, Sunday = 2026-02-22

        SetupEmptyPeriodMocks(new DateOnly(2026, 2, 16), new DateOnly(2026, 2, 22));

        // Act
        var result = await _sut.GetWeekReportAsync(wednesday);

        // Assert
        Assert.Equal(new DateOnly(2026, 2, 16), result.StartDate);
        Assert.Equal(new DateOnly(2026, 2, 22), result.EndDate);
    }

    [Fact]
    public async Task GetWeekReportAsync_SundayInput_ReturnsPreviousMondayWeek()
    {
        // Arrange - Sunday Feb 22, 2026 -> Week should still be Mon Feb 16 - Sun Feb 22
        var sunday = new DateOnly(2026, 2, 22);

        SetupEmptyPeriodMocks(new DateOnly(2026, 2, 16), new DateOnly(2026, 2, 22));

        // Act
        var result = await _sut.GetWeekReportAsync(sunday);

        // Assert
        Assert.Equal(new DateOnly(2026, 2, 16), result.StartDate);
        Assert.Equal(new DateOnly(2026, 2, 22), result.EndDate);
    }

    [Fact]
    public async Task GetWeekReportAsync_AccumulatesTargetPerDay()
    {
        // Arrange
        var monday = new DateOnly(2026, 2, 16);
        var sunday = new DateOnly(2026, 2, 22);

        _mockTimeRecordRepository.Setup(r => r.GetByDateRangeAsync(monday, sunday)).ReturnsAsync(new List<TimeRecord>());
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Activity>());
        _mockWorkdayConfigService.Setup(s => s.GetDayTypeCountsAsync(monday, sunday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<DayType, int> { { DayType.WorkDay, 5 }, { DayType.Holiday, 2 } });

        // Mon-Fri = 8h each, Sat-Sun = 0h
        for (var d = monday; d <= sunday; d = d.AddDays(1))
        {
            var target = d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? TimeSpan.Zero : TimeSpan.FromHours(8);
            _mockWorkdayConfigService.Setup(s => s.GetTargetDurationAsync(d, It.IsAny<CancellationToken>())).ReturnsAsync(target);
        }

        _mockTimeCalculatorService.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);

        // Act
        var result = await _sut.GetWeekReportAsync(monday);

        // Assert - 5 working days * 8h = 40h target
        Assert.Equal(TimeSpan.FromHours(40), result.TotalTarget);
        Assert.Equal(7, result.DailyBreakdown.Count);
    }

    #endregion

    #region GetMonthReportAsync Tests

    [Fact]
    public async Task GetMonthReportAsync_ReturnsCorrectDateRange()
    {
        // Arrange
        SetupEmptyPeriodMocks(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28));

        // Act
        var result = await _sut.GetMonthReportAsync(2026, 2);

        // Assert
        Assert.Equal(new DateOnly(2026, 2, 1), result.StartDate);
        Assert.Equal(new DateOnly(2026, 2, 28), result.EndDate);
        Assert.Equal(28, result.DailyBreakdown.Count);
    }

    [Fact]
    public async Task GetMonthReportAsync_LeapYear_Returns29Days()
    {
        // Arrange - 2028 is a leap year
        SetupEmptyPeriodMocks(new DateOnly(2028, 2, 1), new DateOnly(2028, 2, 29));

        // Act
        var result = await _sut.GetMonthReportAsync(2028, 2);

        // Assert
        Assert.Equal(new DateOnly(2028, 2, 29), result.EndDate);
        Assert.Equal(29, result.DailyBreakdown.Count);
    }

    #endregion

    #region GetYearReportAsync Tests

    [Fact]
    public async Task GetYearReportAsync_ReturnsFullYearRange()
    {
        // Arrange
        SetupEmptyPeriodMocks(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        // Act
        var result = await _sut.GetYearReportAsync(2026);

        // Assert
        Assert.Equal(new DateOnly(2026, 1, 1), result.StartDate);
        Assert.Equal(new DateOnly(2026, 12, 31), result.EndDate);
        Assert.Equal(365, result.DailyBreakdown.Count);
    }

    #endregion

    #region Activity Breakdown Tests

    [Fact]
    public async Task GetDayReportAsync_ActivityBreakdown_CalculatesPercentagesCorrectly()
    {
        // Arrange
        var date = new DateOnly(2026, 2, 20);
        var activityId1 = Guid.NewGuid();
        var activityId2 = Guid.NewGuid();

        var records = new List<TimeRecord>
        {
            new() { Id = Guid.NewGuid(), Date = date, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(14, 0), ActivityId = activityId1, Telework = false },
            new() { Id = Guid.NewGuid(), Date = date, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(16, 0), ActivityId = activityId2, Telework = false }
        };

        var activities = new List<Activity>
        {
            new() { Id = activityId1, Name = "Development", Color = "#FF0000" },
            new() { Id = activityId2, Name = "Meetings", Color = "#00FF00" }
        };

        _mockTimeRecordRepository.Setup(r => r.GetByDateAsync(date)).ReturnsAsync(records);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        _mockWorkdayConfigService.Setup(s => s.GetDayTypeAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(DayType.WorkDay);
        _mockWorkdayConfigService.Setup(s => s.GetTargetDurationAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(TimeSpan.FromHours(8));

        // Total hours = 8, Activity1 = 6h, Activity2 = 2h
        _mockTimeCalculatorService.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns((IEnumerable<TimeRecord> recs) =>
            {
                var list = recs.ToList();
                return list.Sum(r => r.EndTime.HasValue ? (r.EndTime.Value.ToTimeSpan() - r.StartTime.ToTimeSpan()).TotalHours : 0);
            });
        _mockTimeCalculatorService.Setup(s => s.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(8.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);

        // Act
        var result = await _sut.GetDayReportAsync(date);

        // Assert
        Assert.Equal(2, result.Activities.Count);
        var devActivity = result.Activities.First(a => a.ActivityName == "Development");
        var meetActivity = result.Activities.First(a => a.ActivityName == "Meetings");
        Assert.Equal(75.0, devActivity.Percentage, 1);
        Assert.Equal(25.0, meetActivity.Percentage, 1);
        Assert.Equal("#FF0000", devActivity.Color);
    }

    [Fact]
    public async Task GetDayReportAsync_UnknownActivity_ShowsUnknownName()
    {
        // Arrange
        var date = new DateOnly(2026, 2, 20);
        var unknownActivityId = Guid.NewGuid();

        var records = new List<TimeRecord>
        {
            new() { Id = Guid.NewGuid(), Date = date, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(12, 0), ActivityId = unknownActivityId, Telework = false }
        };

        _mockTimeRecordRepository.Setup(r => r.GetByDateAsync(date)).ReturnsAsync(records);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Activity>()); // No matching activity
        _mockWorkdayConfigService.Setup(s => s.GetDayTypeAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(DayType.WorkDay);
        _mockWorkdayConfigService.Setup(s => s.GetTargetDurationAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(TimeSpan.FromHours(8));
        _mockTimeCalculatorService.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(4.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(4.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);

        // Act
        var result = await _sut.GetDayReportAsync(date);

        // Assert
        Assert.Single(result.Activities);
        Assert.Equal("Unknown", result.Activities[0].ActivityName);
        Assert.Equal("#808080", result.Activities[0].Color);
    }

    #endregion

    #region Helper Methods

    private void SetupEmptyPeriodMocks(DateOnly startDate, DateOnly endDate)
    {
        _mockTimeRecordRepository.Setup(r => r.GetByDateRangeAsync(startDate, endDate)).ReturnsAsync(new List<TimeRecord>());
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Activity>());
        _mockWorkdayConfigService.Setup(s => s.GetDayTypeCountsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<DayType, int>());

        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            _mockWorkdayConfigService.Setup(s => s.GetTargetDurationAsync(d, It.IsAny<CancellationToken>())).ReturnsAsync(TimeSpan.Zero);
        }

        _mockTimeCalculatorService.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
        _mockTimeCalculatorService.Setup(s => s.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>())).Returns(0.0);
    }

    #endregion
}
