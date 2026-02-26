namespace TimeTracker.Tests.Services;

using Moq;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using TimeTracker.Core.Services;

/// <summary>
/// Unit tests for the stale activity service.
/// </summary>
public class StaleActivityServiceTests
{
    private readonly Mock<ITimeRecordRepository> _timeRecordRepositoryMock;
    private readonly Mock<IWorkdayConfigService> _workdayConfigServiceMock;
    private readonly Mock<ITimeCalculatorService> _timeCalculatorServiceMock;
    private readonly StaleActivityService _sut;

    public StaleActivityServiceTests()
    {
        _timeRecordRepositoryMock = new Mock<ITimeRecordRepository>();
        _workdayConfigServiceMock = new Mock<IWorkdayConfigService>();
        _timeCalculatorServiceMock = new Mock<ITimeCalculatorService>();
        _sut = new StaleActivityService(
            _timeRecordRepositoryMock.Object,
            _workdayConfigServiceMock.Object,
            _timeCalculatorServiceMock.Object);
    }

    #region No Action Scenarios

    /// <summary>
    /// Verifies that no action is taken when there is no active record.
    /// </summary>
    [Fact]
    public async Task CloseStaleActivitiesAsync_WhenNoActiveRecord_ShouldReturnNull()
    {
        // Arrange
        _timeRecordRepositoryMock.Setup(r => r.GetActiveAsync())
            .ReturnsAsync((TimeRecord?)null);

        // Act
        var result = await _sut.CloseStaleActivitiesAsync();

        // Assert
        Assert.Null(result);
        _timeRecordRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TimeRecord>()), Times.Never);
    }

    /// <summary>
    /// Verifies that active records from today are not modified.
    /// </summary>
    [Fact]
    public async Task CloseStaleActivitiesAsync_WhenActiveRecordIsToday_ShouldReturnNull()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var activeRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = today,
            StartTime = new TimeOnly(9, 0),
            EndTime = null,
            ActivityId = Guid.NewGuid()
        };

        _timeRecordRepositoryMock.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(activeRecord);

        // Act
        var result = await _sut.CloseStaleActivitiesAsync();

        // Assert
        Assert.Null(result);
        _timeRecordRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TimeRecord>()), Times.Never);
    }

    #endregion

    #region Close Stale Activity Scenarios

    /// <summary>
    /// Verifies that a stale activity from a previous day is closed with the correct end time
    /// based on the target duration.
    /// </summary>
    [Fact]
    public async Task CloseStaleActivitiesAsync_WhenActiveRecordFromPreviousDay_ShouldCloseWithTargetTime()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var activeRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = yesterday,
            StartTime = new TimeOnly(9, 0),
            EndTime = null,
            ActivityId = Guid.NewGuid()
        };

        _timeRecordRepositoryMock.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(activeRecord);
        _workdayConfigServiceMock.Setup(s => s.GetTargetDurationAsync(yesterday, default))
            .ReturnsAsync(TimeSpan.FromHours(8));
        _timeRecordRepositoryMock.Setup(r => r.GetByDateAsync(yesterday))
            .ReturnsAsync(new List<TimeRecord> { activeRecord });
        _timeCalculatorServiceMock.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(0);

        // Act
        var result = await _sut.CloseStaleActivitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(yesterday, result.Date);
        Assert.Equal(new TimeOnly(17, 0), result.EndTime); // 9:00 + 8h = 17:00
        Assert.Equal(new TimeOnly(17, 0), activeRecord.EndTime);
        _timeRecordRepositoryMock.Verify(r => r.UpdateAsync(activeRecord), Times.Once);
    }

    /// <summary>
    /// Verifies that the end time accounts for already completed records on the same day.
    /// </summary>
    [Fact]
    public async Task CloseStaleActivitiesAsync_WithCompletedRecordsSameDay_ShouldSubtractWorkedTime()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var completedRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = yesterday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(10, 0),
            ActivityId = Guid.NewGuid()
        };
        var activeRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = yesterday,
            StartTime = new TimeOnly(10, 0),
            EndTime = null,
            ActivityId = Guid.NewGuid()
        };

        _timeRecordRepositoryMock.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(activeRecord);
        _workdayConfigServiceMock.Setup(s => s.GetTargetDurationAsync(yesterday, default))
            .ReturnsAsync(TimeSpan.FromHours(8));
        _timeRecordRepositoryMock.Setup(r => r.GetByDateAsync(yesterday))
            .ReturnsAsync(new List<TimeRecord> { completedRecord, activeRecord });
        _timeCalculatorServiceMock.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(2.0); // 2 hours already worked

        // Act
        var result = await _sut.CloseStaleActivitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeOnly(16, 0), result.EndTime); // 10:00 + (8-2)h = 16:00
        _timeRecordRepositoryMock.Verify(r => r.UpdateAsync(activeRecord), Times.Once);
    }

    /// <summary>
    /// Verifies that the end time is capped at 23:59 when the target would exceed the day.
    /// </summary>
    [Fact]
    public async Task CloseStaleActivitiesAsync_WhenEndTimeExceedsDay_ShouldCapAt2359()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var activeRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = yesterday,
            StartTime = new TimeOnly(20, 0),
            EndTime = null,
            ActivityId = Guid.NewGuid()
        };

        _timeRecordRepositoryMock.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(activeRecord);
        _workdayConfigServiceMock.Setup(s => s.GetTargetDurationAsync(yesterday, default))
            .ReturnsAsync(TimeSpan.FromHours(8));
        _timeRecordRepositoryMock.Setup(r => r.GetByDateAsync(yesterday))
            .ReturnsAsync(new List<TimeRecord> { activeRecord });
        _timeCalculatorServiceMock.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(0);

        // Act
        var result = await _sut.CloseStaleActivitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeOnly(23, 59), result.EndTime); // 20:00 + 8h = 28:00, capped at 23:59
    }

    /// <summary>
    /// Verifies that when the target duration is already met, the end time equals the start time.
    /// </summary>
    [Fact]
    public async Task CloseStaleActivitiesAsync_WhenTargetAlreadyMet_ShouldSetEndTimeToStartTime()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var activeRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = yesterday,
            StartTime = new TimeOnly(17, 0),
            EndTime = null,
            ActivityId = Guid.NewGuid()
        };

        _timeRecordRepositoryMock.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(activeRecord);
        _workdayConfigServiceMock.Setup(s => s.GetTargetDurationAsync(yesterday, default))
            .ReturnsAsync(TimeSpan.FromHours(8));
        _timeRecordRepositoryMock.Setup(r => r.GetByDateAsync(yesterday))
            .ReturnsAsync(new List<TimeRecord> { activeRecord });
        _timeCalculatorServiceMock.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(8.0); // Target already fully met

        // Act
        var result = await _sut.CloseStaleActivitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeOnly(17, 0), result.EndTime); // StartTime since no remaining hours
    }

    /// <summary>
    /// Verifies that stale activities from multiple days ago are also closed.
    /// </summary>
    [Fact]
    public async Task CloseStaleActivitiesAsync_WhenActiveRecordFromMultipleDaysAgo_ShouldClose()
    {
        // Arrange
        var threeDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-3));
        var activeRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = threeDaysAgo,
            StartTime = new TimeOnly(14, 0),
            EndTime = null,
            ActivityId = Guid.NewGuid()
        };

        _timeRecordRepositoryMock.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(activeRecord);
        _workdayConfigServiceMock.Setup(s => s.GetTargetDurationAsync(threeDaysAgo, default))
            .ReturnsAsync(TimeSpan.FromHours(7));
        _timeRecordRepositoryMock.Setup(r => r.GetByDateAsync(threeDaysAgo))
            .ReturnsAsync(new List<TimeRecord> { activeRecord });
        _timeCalculatorServiceMock.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(0);

        // Act
        var result = await _sut.CloseStaleActivitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(threeDaysAgo, result.Date);
        Assert.Equal(new TimeOnly(21, 0), result.EndTime); // 14:00 + 7h = 21:00
    }

    /// <summary>
    /// Verifies that when target duration is zero (non-working day), end time equals start time.
    /// </summary>
    [Fact]
    public async Task CloseStaleActivitiesAsync_WhenNonWorkingDay_ShouldSetEndTimeToStartTime()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var activeRecord = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = yesterday,
            StartTime = new TimeOnly(9, 0),
            EndTime = null,
            ActivityId = Guid.NewGuid()
        };

        _timeRecordRepositoryMock.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(activeRecord);
        _workdayConfigServiceMock.Setup(s => s.GetTargetDurationAsync(yesterday, default))
            .ReturnsAsync(TimeSpan.Zero); // Non-working day
        _timeRecordRepositoryMock.Setup(r => r.GetByDateAsync(yesterday))
            .ReturnsAsync(new List<TimeRecord> { activeRecord });
        _timeCalculatorServiceMock.Setup(s => s.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(0);

        // Act
        var result = await _sut.CloseStaleActivitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeOnly(9, 0), result.EndTime); // No target, so EndTime = StartTime
    }

    #endregion
}
