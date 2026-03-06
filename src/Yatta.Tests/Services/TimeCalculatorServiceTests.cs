namespace Yatta.Tests.Services;

using Yatta.Core.Models;
using Yatta.Core.Services;

/// <summary>
/// Unit tests for the time calculation service.
/// </summary>
public class TimeCalculatorServiceTests
{
    private readonly TimeCalculatorService _sut;

    public TimeCalculatorServiceTests()
    {
        _sut = new TimeCalculatorService();
    }

    #region CalculateDuration Tests

    /// <summary>
    /// Verifies that CalculateDuration correctly calculates the duration in hours.
    /// </summary>
    [Fact]
    public void CalculateDuration_ShouldReturnCorrectHours()
    {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        // Act
        var result = _sut.CalculateDuration(startTime, endTime);

        // Assert
        Assert.Equal(8.0, result);
    }

    /// <summary>
    /// Verifies that CalculateDuration correctly calculates with minutes.
    /// </summary>
    [Fact]
    public void CalculateDuration_WithMinutes_ShouldReturnCorrectHours()
    {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(10, 30);

        // Act
        var result = _sut.CalculateDuration(startTime, endTime);

        // Assert
        Assert.Equal(1.5, result);
    }

    /// <summary>
    /// Verifies that CalculateDuration returns zero when the times are equal.
    /// </summary>
    [Fact]
    public void CalculateDuration_WhenSameTime_ShouldReturnZero()
    {
        // Arrange
        var startTime = new TimeOnly(12, 0);
        var endTime = new TimeOnly(12, 0);

        // Act
        var result = _sut.CalculateDuration(startTime, endTime);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifies that CalculateDuration correctly handles when the end time is before 
    /// (considered as a period crossing midnight, resulting in a negative duration).
    /// Note: TimeSpan with TimeOnly can give unexpected results when endTime &lt; startTime.
    /// </summary>
    [Fact]
    public void CalculateDuration_WhenEndTimeBeforeStartTime_ShouldHandleCorrectly()
    {
        // Arrange
        var startTime = new TimeOnly(17, 0);
        var endTime = new TimeOnly(9, 0);

        // Act
        var result = _sut.CalculateDuration(startTime, endTime);

        // Assert
        // TimeOnly - TimeOnly returns the difference as if it were within the same day,
        // so 9:00 - 17:00 = -8 hours, but TimeSpan treats it as 16 hours (forward).
        // This behavior should be validated by ValidationService beforehand.
        Assert.True(result < 0 || result > 8); // The duration is not valid for a normal day
    }

    /// <summary>
    /// Verifies that CalculateDuration correctly calculates 15 minutes.
    /// </summary>
    [Fact]
    public void CalculateDuration_FifteenMinutes_ShouldReturnQuarterHour()
    {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(9, 15);

        // Act
        var result = _sut.CalculateDuration(startTime, endTime);

        // Assert
        Assert.Equal(0.25, result);
    }

    #endregion

    #region CalculateTotalHours TimeRecord Tests

    /// <summary>
    /// Verifies that CalculateTotalHours returns zero when there are no records.
    /// </summary>
    [Fact]
    public void CalculateTotalHours_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var records = Enumerable.Empty<TimeRecord>();

        // Act
        var result = _sut.CalculateTotalHours(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTotalHours correctly sums the hours of the records.
    /// </summary>
    [Fact]
    public void CalculateTotalHours_ShouldSumAllRecords()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            },
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(13, 0),
                EndTime = new TimeOnly(17, 0)
            }
        };

        // Act
        var result = _sut.CalculateTotalHours(records);

        // Assert
        Assert.Equal(7.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTotalHours ignores records without end time.
    /// </summary>
    [Fact]
    public void CalculateTotalHours_ShouldIgnoreRecordsWithoutEndTime()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            },
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(13, 0),
                EndTime = null // In-progress record
            }
        };

        // Act
        var result = _sut.CalculateTotalHours(records);

        // Assert
        Assert.Equal(3.0, result);
    }

    #endregion

    #region CalculateTeleworkHours Tests

    /// <summary>
    /// Verifies that CalculateTeleworkHours returns zero when there are no records.
    /// </summary>
    [Fact]
    public void CalculateTeleworkHours_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var records = Enumerable.Empty<TimeRecord>();

        // Act
        var result = _sut.CalculateTeleworkHours(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTeleworkHours sums only the telework records.
    /// </summary>
    [Fact]
    public void CalculateTeleworkHours_ShouldSumOnlyTeleworkRecords()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            },
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(18, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkHours(records);

        // Assert
        Assert.Equal(3.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTeleworkHours returns zero when there is no telework.
    /// </summary>
    [Fact]
    public void CalculateTeleworkHours_WhenNoTelework_ShouldReturnZero()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            }
        };

        // Act
        var result = _sut.CalculateTeleworkHours(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTeleworkHours ignores records without end time.
    /// </summary>
    [Fact]
    public void CalculateTeleworkHours_ShouldIgnoreRecordsWithoutEndTime()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = null,
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkHours(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    #endregion

    #region CalculateOfficeHours Tests

    /// <summary>
    /// Verifies that CalculateOfficeHours returns zero when there are no records.
    /// </summary>
    [Fact]
    public void CalculateOfficeHours_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var records = Enumerable.Empty<TimeRecord>();

        // Act
        var result = _sut.CalculateOfficeHours(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifies that CalculateOfficeHours sums only the office records.
    /// </summary>
    [Fact]
    public void CalculateOfficeHours_ShouldSumOnlyOfficeRecords()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            },
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(18, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateOfficeHours(records);

        // Assert
        Assert.Equal(6.0, result);
    }

    /// <summary>
    /// Verifies that CalculateOfficeHours returns zero when everything is telework.
    /// </summary>
    [Fact]
    public void CalculateOfficeHours_WhenAllTelework_ShouldReturnZero()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateOfficeHours(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifies that CalculateOfficeHours ignores records without end time.
    /// </summary>
    [Fact]
    public void CalculateOfficeHours_ShouldIgnoreRecordsWithoutEndTime()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = null,
                Telework = false
            }
        };

        // Act
        var result = _sut.CalculateOfficeHours(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    #endregion

    #region CalculateTeleworkPercentage Tests

    /// <summary>
    /// Verifies that CalculateTeleworkPercentage returns zero when there are no records.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var records = Enumerable.Empty<TimeRecord>();

        // Act
        var result = _sut.CalculateTeleworkPercentage(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTeleworkPercentage correctly calculates the percentage.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_ShouldCalculateCorrectPercentage()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Telework = false
            },
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(13, 0),
                EndTime = new TimeOnly(17, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkPercentage(records);

        // Assert
        Assert.Equal(50.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTeleworkPercentage returns 100 when everything is telework.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_WhenAllTelework_ShouldReturn100()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkPercentage(records);

        // Assert
        Assert.Equal(100.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTeleworkPercentage returns 0 when there is no telework.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_WhenNoTelework_ShouldReturnZero()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            }
        };

        // Act
        var result = _sut.CalculateTeleworkPercentage(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifies that CalculateTeleworkPercentage correctly calculates with unequal proportions.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_WithUnequalRecords_ShouldCalculateCorrectPercentage()
    {
        // Arrange
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0), // 6 office hours
                Telework = false
            },
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(17, 0), // 2 telework hours
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkPercentage(records);

        // Assert
        Assert.Equal(25.0, result); // 2/8 = 25%
    }

    #endregion
}
