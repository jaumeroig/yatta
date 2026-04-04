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
        // Arrange - use a Monday so that the default weekday mask returns WorkDay
        var date = new DateOnly(2026, 3, 30); // Monday

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
        // Arrange - use a Monday so that the default weekday mask returns WorkDay
        var date = new DateOnly(2026, 3, 30); // Monday

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
    /// Verifies that GetDayTypeAsync returns WorkDay when no configuration exists and the date is a weekday.
    /// </summary>
    [Fact]
    public async Task GetDayTypeAsync_WhenNoConfiguration_ShouldReturnWorkDay()
    {
        // Arrange - use a Monday so that the default weekday mask returns WorkDay
        var date = new DateOnly(2026, 3, 30); // Monday

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
        // Arrange - use a Monday so that the default weekday mask returns WorkDay
        var date = new DateOnly(2026, 3, 30); // Monday

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
    /// Verifies that GetDayTypeCountsAsync uses effective day types including the weekly default mask.
    /// </summary>
    [Fact]
    public async Task GetDayTypeCountsAsync_UsesEffectiveConfigurationFromMask()
    {
        // Arrange: use a Mon–Sun week (2026-03-30 to 2026-04-05)
        // Mon=2026-03-30, Tue=2026-03-31, Wed=2026-04-01, Thu=2026-04-02, Fri=2026-04-03
        // Sat=2026-04-04, Sun=2026-04-05
        // Default mask = Weekdays (Mon–Fri), so Sat/Sun → NonWorkingDay
        // Wed is configured explicitly as Vacation
        var startDate = new DateOnly(2026, 3, 30);
        var endDate = new DateOnly(2026, 4, 5);

        var wednesday = new DateOnly(2026, 4, 1);
        var vacation = new Workday
        {
            Id = Guid.NewGuid(),
            Date = wednesday,
            DayType = DayType.Vacation,
            TargetDuration = TimeSpan.Zero
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync((Workday?)null);

        _mockRepository
            .Setup(r => r.GetByDateAsync(wednesday))
            .ReturnsAsync(vacation);

        // Act
        var result = await _sut.GetDayTypeCountsAsync(startDate, endDate);

        // Assert
        Assert.Equal(4, result.GetValueOrDefault(DayType.WorkDay));    // Mon, Tue, Thu, Fri
        Assert.Equal(1, result.GetValueOrDefault(DayType.Vacation));   // Wed
        Assert.Equal(2, result.GetValueOrDefault(DayType.NonWorkingDay)); // Sat, Sun
    }

    #endregion

    #region GetRemainingWorkTimeAsync Tests

    /// <summary>
    /// Verifies that GetRemainingWorkTimeAsync returns remaining time for working days.
    /// </summary>
    [Fact]
    public async Task GetRemainingWorkTimeAsync_ForWorkingDay_ShouldReturnRemainingTime()
    {
        // Arrange - use a Monday so that the default weekday mask returns WorkDay
        var date = new DateOnly(2026, 3, 30); // Monday
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
        // Arrange - use a Monday so that the default weekday mask returns WorkDay
        var date = new DateOnly(2026, 3, 30); // Monday
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
        // Arrange - use a Monday so that the default weekday mask returns WorkDay
        var date = new DateOnly(2026, 3, 30); // Monday
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

    #region DefaultWorkingDaysMask Tests

    /// <summary>
    /// Verifies that GetEffectiveConfigurationAsync returns WorkDay for a date whose weekday
    /// is included in the DefaultWorkingDaysMask.
    /// </summary>
    [Fact]
    public async Task GetEffectiveConfigurationAsync_WhenNoOverride_AndDayIncludedInMask_ReturnsWorkDay()
    {
        // Arrange - Monday is in the default Weekdays mask
        var monday = new DateOnly(2026, 3, 30);

        _mockRepository
            .Setup(r => r.GetByDateAsync(monday))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetEffectiveConfigurationAsync(monday);

        // Assert
        Assert.Equal(DayType.WorkDay, result.DayType);
        Assert.Equal(TimeSpan.FromHours(8), result.TargetDuration);
    }

    /// <summary>
    /// Verifies that GetEffectiveConfigurationAsync returns NonWorkingDay for a date whose
    /// weekday is NOT included in the DefaultWorkingDaysMask.
    /// </summary>
    [Fact]
    public async Task GetEffectiveConfigurationAsync_WhenNoOverride_AndDayNotIncludedInMask_ReturnsNonWorkingDay()
    {
        // Arrange - Saturday is NOT in the default Weekdays mask
        var saturday = new DateOnly(2026, 3, 28);

        _mockRepository
            .Setup(r => r.GetByDateAsync(saturday))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetEffectiveConfigurationAsync(saturday);

        // Assert
        Assert.Equal(DayType.NonWorkingDay, result.DayType);
        Assert.Equal(TimeSpan.Zero, result.TargetDuration);
    }

    /// <summary>
    /// Verifies that GetEffectiveConfigurationAsync returns NonWorkingDay for Sunday when
    /// Sunday is not in the mask.
    /// </summary>
    [Fact]
    public async Task GetEffectiveConfigurationAsync_WhenNoOverride_AndSundayNotInMask_ReturnsNonWorkingDay()
    {
        // Arrange - Sunday is NOT in the default Weekdays mask
        var sunday = new DateOnly(2026, 3, 29);

        _mockRepository
            .Setup(r => r.GetByDateAsync(sunday))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetEffectiveConfigurationAsync(sunday);

        // Assert
        Assert.Equal(DayType.NonWorkingDay, result.DayType);
        Assert.Equal(TimeSpan.Zero, result.TargetDuration);
    }

    /// <summary>
    /// Verifies that GetTargetDurationAsync returns zero when the date's weekday is not
    /// included in the DefaultWorkingDaysMask.
    /// </summary>
    [Fact]
    public async Task GetTargetDurationAsync_WhenDayNotIncludedInMask_ReturnsZero()
    {
        // Arrange - Saturday is NOT in the default Weekdays mask
        var saturday = new DateOnly(2026, 3, 28);

        _mockRepository
            .Setup(r => r.GetByDateAsync(saturday))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetTargetDurationAsync(saturday);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    /// <summary>
    /// Verifies that an explicit per-date configuration overrides the DefaultWorkingDaysMask.
    /// </summary>
    [Fact]
    public async Task ExplicitConfiguration_OverridesWeeklyMask()
    {
        // Arrange - Saturday explicitly configured as WorkDay
        var saturday = new DateOnly(2026, 3, 28);
        var explicitWorkday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = saturday,
            DayType = DayType.WorkDay,
            TargetDuration = TimeSpan.FromHours(4)
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(saturday))
            .ReturnsAsync(explicitWorkday);

        // Act
        var result = await _sut.GetEffectiveConfigurationAsync(saturday);

        // Assert
        Assert.Equal(DayType.WorkDay, result.DayType);
        Assert.Equal(TimeSpan.FromHours(4), result.TargetDuration);
    }

    /// <summary>
    /// Verifies that an explicit NonWorkingDay configuration on a weekday overrides the mask.
    /// </summary>
    [Fact]
    public async Task ExplicitNonWorkingDay_OnWeekday_OverridesWeeklyMask()
    {
        // Arrange - Monday explicitly configured as NonWorkingDay
        var monday = new DateOnly(2026, 3, 30);
        var explicitWorkday = new Workday
        {
            Id = Guid.NewGuid(),
            Date = monday,
            DayType = DayType.NonWorkingDay,
            TargetDuration = TimeSpan.Zero
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(monday))
            .ReturnsAsync(explicitWorkday);

        // Act
        var result = await _sut.GetEffectiveConfigurationAsync(monday);

        // Assert
        Assert.Equal(DayType.NonWorkingDay, result.DayType);
        Assert.Equal(TimeSpan.Zero, result.TargetDuration);
    }

    /// <summary>
    /// Verifies that when Saturday is added to the mask, GetEffectiveConfigurationAsync
    /// returns WorkDay for it.
    /// </summary>
    [Fact]
    public async Task GetEffectiveConfigurationAsync_WhenSaturdayAddedToMask_ReturnsSaturdayAsWorkDay()
    {
        // Arrange - override settings to include Saturday
        _defaultSettings.DefaultWorkingDaysMask = (int)(WeeklyWorkingDays.Weekdays | WeeklyWorkingDays.Saturday);

        var saturday = new DateOnly(2026, 3, 28);

        _mockRepository
            .Setup(r => r.GetByDateAsync(saturday))
            .ReturnsAsync((Workday?)null);

        // Act
        var result = await _sut.GetEffectiveConfigurationAsync(saturday);

        // Assert
        Assert.Equal(DayType.WorkDay, result.DayType);
    }

    #endregion
}
