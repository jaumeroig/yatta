namespace TimeTracker.Tests.Services;

using TimeTracker.Core.Models;
using TimeTracker.Core.Services;

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
    /// Verifica que ValidateNoOverlap retorna el missatge d'error correcte.
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

    #region ValidateNoOverlap WorkdaySlot Tests

    /// <summary>
    /// Verifica que ValidateNoOverlap per franges retorna true quan no hi ha franges existents.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_WorkdaySlot_WhenNoExistingSlots_ShouldReturnTrue()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(14, 0)
        };
        var existingSlots = Enumerable.Empty<WorkdaySlot>();

        // Act
        var result = _sut.ValidateNoOverlap(slot, existingSlots);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifica que ValidateNoOverlap per franges retorna true quan no hi ha solapament.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_WorkdaySlot_WhenNoOverlap_ShouldReturnTrue()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(15, 0),
            EndTime = new TimeOnly(18, 0)
        };
        var existingSlots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(14, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(slot, existingSlots);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifica que ValidateNoOverlap per franges retorna false quan hi ha solapament.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_WorkdaySlot_WhenOverlaps_ShouldReturnFalse()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(16, 0)
        };
        var existingSlots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(14, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(slot, existingSlots);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifica que ValidateNoOverlap per franges retorna el missatge d'error correcte.
    /// </summary>
    [Fact]
    public void ValidateNoOverlap_WorkdaySlot_WhenOverlaps_ShouldReturnErrorMessage()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(16, 0)
        };
        var existingSlots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(14, 0)
            }
        };

        // Act
        var result = _sut.ValidateNoOverlap(slot, existingSlots, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.StartsWith("Validation_OverlappingSlot|", errorMessage);
    }

    #endregion

    #region ValidateTimeRecord Tests

    /// <summary>
    /// Verifica que ValidateTimeRecord retorna true quan el registre és vàlid.
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
    /// Verifica que ValidateTimeRecord retorna true quan no hi ha hora de fi (registre en curs).
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
    /// Verifica que ValidateTimeRecord retorna false quan l'hora de fi és anterior a l'hora d'inici.
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

    #region ValidateWorkdaySlot Tests

    /// <summary>
    /// Verifica que ValidateWorkdaySlot retorna true quan la franja és vàlida.
    /// </summary>
    [Fact]
    public void ValidateWorkdaySlot_WhenValid_ShouldReturnTrue()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(14, 0)
        };

        // Act
        var result = _sut.ValidateWorkdaySlot(slot);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifica que ValidateWorkdaySlot retorna false quan la franja no és vàlida.
    /// </summary>
    [Fact]
    public void ValidateWorkdaySlot_WhenInvalid_ShouldReturnFalse()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(9, 0)
        };

        // Act
        var result = _sut.ValidateWorkdaySlot(slot);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifica que ValidateWorkdaySlot retorna el missatge d'error correcte.
    /// </summary>
    [Fact]
    public void ValidateWorkdaySlot_WhenInvalid_ShouldReturnErrorMessage()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(9, 0)
        };

        // Act
        var result = _sut.ValidateWorkdaySlot(slot, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Equal("Validation_EndTimeAfterStartTime", errorMessage);
    }

    #endregion
}
