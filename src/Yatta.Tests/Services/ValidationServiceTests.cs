namespace Yatta.Tests.Services;

using Yatta.Core.Models;
using Yatta.Core.Services;

/// <summary>
/// Unit tests for the validation service.
/// </summary>
public class ValidationServiceTests
{
    private readonly ValidationService _sut;

    public ValidationServiceTests()
    {
        _sut = new ValidationService();
    }

    #region ValidateTimeRange Tests

    /// <summary>
    /// Verifies that ValidateTimeRange returns true when end time is after start time.
    /// </summary>
    [Fact]
    public void ValidateTimeRange_WhenEndTimeAfterStartTime_ShouldReturnTrue()
    {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        // Act
        var result = _sut.ValidateTimeRange(startTime, endTime);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that ValidateTimeRange returns false when end time is before start time.
    /// </summary>
    [Fact]
    public void ValidateTimeRange_WhenEndTimeBeforeStartTime_ShouldReturnFalse()
    {
        // Arrange
        var startTime = new TimeOnly(17, 0);
        var endTime = new TimeOnly(9, 0);

        // Act
        var result = _sut.ValidateTimeRange(startTime, endTime);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that ValidateTimeRange returns false when end time equals start time.
    /// </summary>
    [Fact]
    public void ValidateTimeRange_WhenEndTimeEqualsStartTime_ShouldReturnFalse()
    {
        // Arrange
        var startTime = new TimeOnly(12, 0);
        var endTime = new TimeOnly(12, 0);

        // Act
        var result = _sut.ValidateTimeRange(startTime, endTime);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that ValidateTimeRange with out parameter returns the correct error message.
    /// </summary>
    [Fact]
    public void ValidateTimeRange_WhenInvalid_ShouldReturnErrorMessage()
    {
        // Arrange
        var startTime = new TimeOnly(17, 0);
        var endTime = new TimeOnly(9, 0);

        // Act
        var result = _sut.ValidateTimeRange(startTime, endTime, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Equal("Validation_EndTimeAfterStartTime", errorMessage);
    }

    /// <summary>
    /// Verifies that ValidateTimeRange with out parameter returns empty string when valid.
    /// </summary>
    [Fact]
    public void ValidateTimeRange_WhenValid_ShouldReturnEmptyErrorMessage()
    {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        // Act
        var result = _sut.ValidateTimeRange(startTime, endTime, out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Equal(string.Empty, errorMessage);
    }

    #endregion

    #region ValidateNoOverlap TimeRecord Tests

    /// <summary>
    /// Verifies that ValidateNoOverlap returns true when there are no existing records.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_TimeRecord_WhenNoExistingRecords_ShouldReturnTrue()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        };
        var existingRecords = Enumerable.Empty<TimeRecord>();

        // Act
        var result = _sut.ValidateNoOverlap(record, existingRecords);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that ValidateNoOverlap returns true when the record has no end time.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_TimeRecord_WhenNoEndTime_ShouldReturnTrue()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = null
        };
        var existingRecords = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(record, existingRecords);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that ValidateNoOverlap returns true when records do not overlap.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_TimeRecord_WhenNoOverlap_ShouldReturnTrue()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(16, 0)
        };
        var existingRecords = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(record, existingRecords);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that ValidateNoOverlap returns false when the start of the new record overlaps with an existing one.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_TimeRecord_WhenStartTimeOverlaps_ShouldReturnFalse()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(14, 0)
        };
        var existingRecords = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(record, existingRecords);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that ValidateNoOverlap returns false when the end of the new record overlaps with an existing one.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_TimeRecord_WhenEndTimeOverlaps_ShouldReturnFalse()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(10, 0)
        };
        var existingRecords = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(record, existingRecords);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that ValidateNoOverlap returns false when the new record completely encompasses an existing one.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_TimeRecord_WhenNewRecordEncompasses_ShouldReturnFalse()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(14, 0)
        };
        var existingRecords = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(record, existingRecords);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that ValidateNoOverlap ignores the same record (for edits).
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_TimeRecord_WhenSameRecord_ShouldReturnTrue()
    {
        // Arrange
        var recordId = Guid.NewGuid();
        var record = new TimeRecord
        {
            Id = recordId,
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        };
        var existingRecords = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = recordId,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(record, existingRecords);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that ValidateNoOverlap returns the correct error message.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_TimeRecord_WhenOverlaps_ShouldReturnErrorMessage()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(14, 0)
        };
        var existingRecords = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(record, existingRecords, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.StartsWith("Validation_OverlappingRecord|", errorMessage);
    }

    #endregion

    #region ValidateTimeRecord Tests

    /// <summary>
    /// Verifies that ValidateTimeRecord returns true when the record is valid.
    /// </summary>
    [Fact]
    public void ValidateTimeRecord_WhenValid_ShouldReturnTrue()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        };

        // Act
        var result = _sut.ValidateTimeRecord(record);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that ValidateTimeRecord returns true when there is no end time (in-progress record).
    /// </summary>
    [Fact]
    public void ValidateTimeRecord_WhenNoEndTime_ShouldReturnTrue()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = null
        };

        // Act
        var result = _sut.ValidateTimeRecord(record);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that ValidateTimeRecord returns false when end time is before start time.
    /// </summary>
    [Fact]
    public void ValidateTimeRecord_WhenEndTimeBeforeStartTime_ShouldReturnFalse()
    {
        // Arrange
        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(9, 0)
        };

        // Act
        var result = _sut.ValidateTimeRecord(record);

        // Assert
        Assert.False(result);
    }

    #endregion
}
