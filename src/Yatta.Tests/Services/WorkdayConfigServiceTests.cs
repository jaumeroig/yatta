namespace Yatta.Tests.Services;

using Moq;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;
using Yatta.Core.Services;

/// <summary>
/// Unit tests for the workday configuration service.
/// </summary>
public class WorkdayConfigServiceTests
{
    private readonly Mock<IWorkdayRepository> _mockRepository;
    private readonly Mock<ISettingsRepository> _mockSettingsRepository;
    private readonly WorkdayConfigService _sut;
    private readonly AppSettings _defaultSettings;

    public WorkdayConfigServiceTests()
    {
        _mockRepository = new Mock<IWorkdayRepository>();
        _mockSettingsRepository = new Mock<ISettingsRepository>();

        _defaultSettings = new AppSettings
        {
            Id = 1,
            WorkdayTotalTime = TimeSpan.FromHours(8)
        };

        _mockSettingsRepository
            .Setup(r => r.GetAsync())
            .ReturnsAsync(_defaultSettings);

        _sut = new WorkdayConfigService(
            _mockRepository.Object,
            _mockSettingsRepository.Object);
    }

    #region GetEffectiveConfigurationAsync Tests

    /// <summary>
    /// Verifies that GetEffectiveConfigurationAsync returns the saved configuration when it exists.
    /// </summary>
    [Fact]
    public async Task GetEffectiveConfigurationAsync_WhenConfigurationExists_ShouldReturnSavedConfiguration()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var savedWorkday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.IntensiveDay,
            TargetDuration = TimeSpan.FromHours(6)
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(savedWorkday);

        // Act
        var result = await _sut.GetEffectiveConfigurationAsync(date);

        // Assert
        Assert.Equal(savedWorkday.Id, result.Id);
        Assert.Equal(DayType.IntensiveDay, result.DayType);
        Assert.Equal(TimeSpan.FromHours(6), result.TargetDuration);
    }

    /// <summary>
    /// Verifies that GetEffectiveConfigurationAsync returns default configuration when no saved configuration exists.
    /// </summary>
    [Fact]
    public async Task GetEffectiveConfigurationAsync_WhenNoConfiguration_ShouldReturnDefaultConfiguration()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetEffectiveConfigurationAsync(date);

        // Assert
        Assert.Equal(Guid.Empty, result.Id);
        Assert.Equal(date, result.Date);
        Assert.Equal(DayType.WorkDay, result.DayType);
        Assert.Equal(TimeSpan.FromHours(8), result.TargetDuration);
    }

    #endregion

    #region GetTargetDurationAsync Tests

    /// <summary>
    /// Verifies that GetTargetDurationAsync returns the target duration for a working day.
    /// </summary>
    [Fact]
    public async Task GetTargetDurationAsync_ForWorkDay_ShouldReturnTargetDuration()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetTargetDurationAsync(date);

        // Assert
        Assert.Equal(TimeSpan.FromHours(8), result);
    }

    /// <summary>
    /// Verifies that GetTargetDurationAsync returns zero for a holiday.
    /// </summary>
    [Fact]
    public async Task GetTargetDurationAsync_ForHoliday_ShouldReturnZero()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var holidayWorkday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.Holiday,
            TargetDuration = TimeSpan.Zero
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(holidayWorkday);

        // Act
        var result = await _sut.GetTargetDurationAsync(date);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    /// <summary>
    /// Verifies that GetTargetDurationAsync returns zero for vacation days.
    /// </summary>
    [Fact]
    public async Task GetTargetDurationAsync_ForVacation_ShouldReturnZero()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var vacationWorkday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.Vacation,
            TargetDuration = TimeSpan.Zero
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(vacationWorkday);

        // Act
        var result = await _sut.GetTargetDurationAsync(date);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    /// <summary>
    /// Verifies that GetTargetDurationAsync returns the custom duration for an intensive day.
    /// </summary>
    [Fact]
    public async Task GetTargetDurationAsync_ForIntensiveDay_ShouldReturnCustomDuration()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var intensiveWorkday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.IntensiveDay,
            TargetDuration = TimeSpan.FromHours(6)
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(intensiveWorkday);

        // Act
        var result = await _sut.GetTargetDurationAsync(date);

        // Assert
        Assert.Equal(TimeSpan.FromHours(6), result);
    }

    #endregion

    #region GetDayTypeAsync Tests

    /// <summary>
    /// Verifies that GetDayTypeAsync returns WorkDay when no configuration exists.
    /// </summary>
    [Fact]
    public async Task GetDayTypeAsync_WhenNoConfiguration_ShouldReturnWorkDay()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetDayTypeAsync(date);

        // Assert
        Assert.Equal(DayType.WorkDay, result);
    }

    /// <summary>
    /// Verifies that GetDayTypeAsync returns the saved day type.
    /// </summary>
    [Fact]
    public async Task GetDayTypeAsync_WhenConfigurationExists_ShouldReturnSavedDayType()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.FreeChoice,
            TargetDuration = TimeSpan.Zero
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(workday);

        // Act
        var result = await _sut.GetDayTypeAsync(date);

        // Assert
        Assert.Equal(DayType.FreeChoice, result);
    }

    #endregion

    #region IsWorkingDayAsync Tests

    /// <summary>
    /// Verifies that IsWorkingDayAsync returns true for WorkDay.
    /// </summary>
    [Fact]
    public async Task IsWorkingDayAsync_ForWorkDay_ShouldReturnTrue()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.IsWorkingDayAsync(date);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that IsWorkingDayAsync returns true for IntensiveDay.
    /// </summary>
    [Fact]
    public async Task IsWorkingDayAsync_ForIntensiveDay_ShouldReturnTrue()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.IntensiveDay,
            TargetDuration = TimeSpan.FromHours(6)
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(workday);

        // Act
        var result = await _sut.IsWorkingDayAsync(date);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that IsWorkingDayAsync returns false for Holiday.
    /// </summary>
    [Fact]
    public async Task IsWorkingDayAsync_ForHoliday_ShouldReturnFalse()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.Holiday,
            TargetDuration = TimeSpan.Zero
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(workday);

        // Act
        var result = await _sut.IsWorkingDayAsync(date);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that IsWorkingDayAsync returns false for Vacation.
    /// </summary>
    [Fact]
    public async Task IsWorkingDayAsync_ForVacation_ShouldReturnFalse()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.Vacation,
            TargetDuration = TimeSpan.Zero
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(workday);

        // Act
        var result = await _sut.IsWorkingDayAsync(date);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that IsWorkingDayAsync returns false for FreeChoice.
    /// </summary>
    [Fact]
    public async Task IsWorkingDayAsync_ForFreeChoice_ShouldReturnFalse()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.FreeChoice,
            TargetDuration = TimeSpan.Zero
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(workday);

        // Act
        var result = await _sut.IsWorkingDayAsync(date);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region SetConfigurationAsync Tests

    /// <summary>
    /// Verifies that SetConfigurationAsync creates a new configuration when none exists.
    /// </summary>
    [Fact]
    public async Task SetConfigurationAsync_WhenNoExistingConfiguration_ShouldCreateNew()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<Workday>()))
            .ReturnsAsync((Workday w) => w);

        // Act
        var result = await _sut.SetConfigurationAsync(date, DayType.IntensiveDay, TimeSpan.FromHours(6));

        // Assert
        Assert.Equal(date, result.Date);
        Assert.Equal(DayType.IntensiveDay, result.DayType);
        Assert.Equal(TimeSpan.FromHours(6), result.TargetDuration);

        _mockRepository.Verify(r => r.SaveAsync(It.Is<Workday>(w =>
            w.Date == date &&
            w.DayType == DayType.IntensiveDay &&
            w.TargetDuration == TimeSpan.FromHours(6))), Times.Once);
    }

    /// <summary>
    /// Verifies that SetConfigurationAsync uses default duration when none specified for working days.
    /// </summary>
    [Fact]
    public async Task SetConfigurationAsync_WhenNoDurationSpecifiedForWorkDay_ShouldUseDefault()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<Workday>()))
            .ReturnsAsync((Workday w) => w);

        // Act
        var result = await _sut.SetConfigurationAsync(date, DayType.WorkDay);

        // Assert
        Assert.Equal(TimeSpan.FromHours(8), result.TargetDuration);
    }

    /// <summary>
    /// Verifies that SetConfigurationAsync sets zero duration for non-working days.
    /// </summary>
    [Fact]
    public async Task SetConfigurationAsync_ForNonWorkingDay_ShouldSetZeroDuration()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<Workday>()))
            .ReturnsAsync((Workday w) => w);

        // Act
        var result = await _sut.SetConfigurationAsync(date, DayType.Holiday, TimeSpan.FromHours(8));

        // Assert
        Assert.Equal(TimeSpan.Zero, result.TargetDuration);
    }

    /// <summary>
    /// Verifies that SetConfigurationAsync updates an existing configuration.
    /// </summary>
    [Fact]
    public async Task SetConfigurationAsync_WhenConfigurationExists_ShouldUpdate()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var existingWorkday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.WorkDay,
            TargetDuration = TimeSpan.FromHours(8)
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(existingWorkday);

        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<Workday>()))
            .ReturnsAsync((Workday w) => w);

        // Act
        var result = await _sut.SetConfigurationAsync(date, DayType.IntensiveDay, TimeSpan.FromHours(6));

        // Assert
        Assert.Equal(existingWorkday.Id, result.Id);
        Assert.Equal(DayType.IntensiveDay, result.DayType);
        Assert.Equal(TimeSpan.FromHours(6), result.TargetDuration);
    }

    #endregion

    #region ResetConfigurationAsync Tests

    /// <summary>
    /// Verifies that ResetConfigurationAsync deletes the configuration.
    /// </summary>
    [Fact]
    public async Task ResetConfigurationAsync_ShouldCallDeleteAsync()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        // Act
        await _sut.ResetConfigurationAsync(date);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(date), Times.Once);
    }

    #endregion

    #region GetDayTypeCountsAsync Tests

    /// <summary>
    /// Verifies that GetDayTypeCountsAsync returns counts from repository.
    /// </summary>
    [Fact]
    public async Task GetDayTypeCountsAsync_ShouldReturnRepositoryCounts()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var endDate = startDate.AddDays(30);
        var expectedCounts = new Dictionary<DayType, int>
        {
            { DayType.Holiday, 2 },
            { DayType.Vacation, 5 }
        };

        _mockRepository
            .Setup(r => r.GetDayTypeCountsAsync(startDate, endDate))
            .ReturnsAsync(expectedCounts);

        // Act
        var result = await _sut.GetDayTypeCountsAsync(startDate, endDate);

        // Assert
        Assert.Equal(2, result[DayType.Holiday]);
        Assert.Equal(5, result[DayType.Vacation]);
    }

    #endregion

    #region GetRemainingWorkTimeAsync Tests

    /// <summary>
    /// Verifies that GetRemainingWorkTimeAsync returns remaining time for working days.
    /// </summary>
    [Fact]
    public async Task GetRemainingWorkTimeAsync_ForWorkingDay_ShouldReturnRemainingTime()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workedDuration = TimeSpan.FromHours(5);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetRemainingWorkTimeAsync(date, workedDuration);

        // Assert
        Assert.Equal(TimeSpan.FromHours(3), result);
    }

    /// <summary>
    /// Verifies that GetRemainingWorkTimeAsync returns zero for non-working days.
    /// </summary>
    [Fact]
    public async Task GetRemainingWorkTimeAsync_ForNonWorkingDay_ShouldReturnZero()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = date,
            DayType = DayType.Holiday,
            TargetDuration = TimeSpan.Zero
        };
        var workedDuration = TimeSpan.FromHours(2);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(workday);

        // Act
        var result = await _sut.GetRemainingWorkTimeAsync(date, workedDuration);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    /// <summary>
    /// Verifies that GetRemainingWorkTimeAsync returns zero when target is reached.
    /// </summary>
    [Fact]
    public async Task GetRemainingWorkTimeAsync_WhenTargetReached_ShouldReturnZero()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workedDuration = TimeSpan.FromHours(10);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetRemainingWorkTimeAsync(date, workedDuration);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    /// <summary>
    /// Verifies that GetRemainingWorkTimeAsync returns zero when exactly at target.
    /// </summary>
    [Fact]
    public async Task GetRemainingWorkTimeAsync_WhenExactlyAtTarget_ShouldReturnZero()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var workedDuration = TimeSpan.FromHours(8);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetRemainingWorkTimeAsync(date, workedDuration);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    #endregion
}
