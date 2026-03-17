namespace Yatta.Tests.Services;

using Moq;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;
using Yatta.Core.Services;

/// <summary>
/// Unit tests for the automatic startup activity service.
/// </summary>
public class AutoStartActivityServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<ISettingsRepository> _settingsRepositoryMock;
    private readonly Mock<ITimeRecordRepository> _timeRecordRepositoryMock;
    private readonly AutoStartActivityService _sut;

    public AutoStartActivityServiceTests()
    {
        _activityRepositoryMock = new Mock<IActivityRepository>();
        _settingsRepositoryMock = new Mock<ISettingsRepository>();
        _timeRecordRepositoryMock = new Mock<ITimeRecordRepository>();

        _sut = new AutoStartActivityService(
            _activityRepositoryMock.Object,
            _settingsRepositoryMock.Object,
            _timeRecordRepositoryMock.Object);
    }

    [Fact]
    public async Task TryStartPreviousDayActivityAsync_WhenSettingIsDisabled_ShouldReturnNull()
    {
        _settingsRepositoryMock
            .Setup(repository => repository.GetAsync())
            .ReturnsAsync(new AppSettings { StartTimerOnStartup = false });

        var result = await _sut.TryStartPreviousDayActivityAsync();

        Assert.Null(result);
        _timeRecordRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<TimeRecord>()), Times.Never);
    }

    [Fact]
    public async Task TryStartPreviousDayActivityAsync_WhenThereIsAnActiveRecord_ShouldReturnNull()
    {
        _settingsRepositoryMock
            .Setup(repository => repository.GetAsync())
            .ReturnsAsync(new AppSettings { StartTimerOnStartup = true });
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetActiveAsync())
            .ReturnsAsync(new TimeRecord { Id = Guid.NewGuid(), Date = DateOnly.FromDateTime(DateTime.Today) });

        var result = await _sut.TryStartPreviousDayActivityAsync();

        Assert.Null(result);
        _timeRecordRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<TimeRecord>()), Times.Never);
    }

    [Fact]
    public async Task TryStartPreviousDayActivityAsync_WhenTodayAlreadyHasRecords_ShouldReturnNull()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        _settingsRepositoryMock
            .Setup(repository => repository.GetAsync())
            .ReturnsAsync(new AppSettings { StartTimerOnStartup = true });
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetActiveAsync())
            .ReturnsAsync((TimeRecord?)null);
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetByDateAsync(today))
            .ReturnsAsync(
            [
                new TimeRecord
                {
                    Id = Guid.NewGuid(),
                    Date = today,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0),
                    ActivityId = Guid.NewGuid()
                }
            ]);

        var result = await _sut.TryStartPreviousDayActivityAsync();

        Assert.Null(result);
        _timeRecordRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<TimeRecord>()), Times.Never);
    }

    [Fact]
    public async Task TryStartPreviousDayActivityAsync_WhenPreviousDayHasNoRecords_ShouldReturnNull()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);

        _settingsRepositoryMock
            .Setup(repository => repository.GetAsync())
            .ReturnsAsync(new AppSettings { StartTimerOnStartup = true });
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetActiveAsync())
            .ReturnsAsync((TimeRecord?)null);
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetByDateAsync(today))
            .ReturnsAsync([]);
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetByDateAsync(yesterday))
            .ReturnsAsync([]);

        var result = await _sut.TryStartPreviousDayActivityAsync();

        Assert.Null(result);
        _timeRecordRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<TimeRecord>()), Times.Never);
    }

    [Fact]
    public async Task TryStartPreviousDayActivityAsync_WhenLastActivityIsInactive_ShouldReturnNull()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);
        var activityId = Guid.NewGuid();

        _settingsRepositoryMock
            .Setup(repository => repository.GetAsync())
            .ReturnsAsync(new AppSettings { StartTimerOnStartup = true });
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetActiveAsync())
            .ReturnsAsync((TimeRecord?)null);
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetByDateAsync(today))
            .ReturnsAsync([]);
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetByDateAsync(yesterday))
            .ReturnsAsync(
            [
                new TimeRecord
                {
                    Id = Guid.NewGuid(),
                    Date = yesterday,
                    StartTime = new TimeOnly(17, 0),
                    EndTime = new TimeOnly(18, 0),
                    ActivityId = activityId
                }
            ]);
        _activityRepositoryMock
            .Setup(repository => repository.GetByIdAsync(activityId))
            .ReturnsAsync(new Activity { Id = activityId, Active = false });

        var result = await _sut.TryStartPreviousDayActivityAsync();

        Assert.Null(result);
        _timeRecordRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<TimeRecord>()), Times.Never);
    }

    [Fact]
    public async Task TryStartPreviousDayActivityAsync_WhenPreviousDayHasRecords_ShouldCreateNewActiveRecord()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);
        var earlierActivityId = Guid.NewGuid();
        var latestActivityId = Guid.NewGuid();
        TimeRecord? createdRecord = null;

        _settingsRepositoryMock
            .Setup(repository => repository.GetAsync())
            .ReturnsAsync(new AppSettings { StartTimerOnStartup = true });
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetActiveAsync())
            .ReturnsAsync((TimeRecord?)null);
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetByDateAsync(today))
            .ReturnsAsync([]);
        _timeRecordRepositoryMock
            .Setup(repository => repository.GetByDateAsync(yesterday))
            .ReturnsAsync(
            [
                new TimeRecord
                {
                    Id = Guid.NewGuid(),
                    Date = yesterday,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0),
                    ActivityId = earlierActivityId,
                    Telework = false
                },
                new TimeRecord
                {
                    Id = Guid.NewGuid(),
                    Date = yesterday,
                    StartTime = new TimeOnly(15, 0),
                    EndTime = new TimeOnly(18, 0),
                    ActivityId = latestActivityId,
                    Telework = true
                }
            ]);
        _activityRepositoryMock
            .Setup(repository => repository.GetByIdAsync(latestActivityId))
            .ReturnsAsync(new Activity { Id = latestActivityId, Active = true });
        _timeRecordRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<TimeRecord>()))
            .ReturnsAsync((TimeRecord record) =>
            {
                createdRecord = record;
                return record;
            });

        var result = await _sut.TryStartPreviousDayActivityAsync();

        Assert.NotNull(result);
        Assert.NotNull(createdRecord);
        Assert.Equal(today, createdRecord!.Date);
        Assert.Equal(latestActivityId, createdRecord.ActivityId);
        Assert.Null(createdRecord.EndTime);
        Assert.True(createdRecord.Telework);
        _timeRecordRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<TimeRecord>()), Times.Once);
    }
}
