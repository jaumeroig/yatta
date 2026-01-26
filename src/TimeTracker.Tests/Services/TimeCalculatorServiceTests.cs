namespace TimeTracker.Tests.Services;

using TimeTracker.Core.Models;
using TimeTracker.Core.Services;

/// <summary>
/// Tests unitaris per al servei de càlcul de temps.
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
    /// Verifica que CalculateDuration calcula correctament la durada en hores.
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
    /// Verifica que CalculateDuration calcula correctament amb minuts.
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
    /// Verifica que CalculateDuration retorna zero quan les hores són iguals.
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
    /// Verifica que CalculateDuration gestiona correctament quan l'hora de fi és anterior 
    /// (es considera com un període que travessa la mitjanit, resultant en una durada negativa).
    /// Nota: TimeSpan amb TimeOnly pot donar resultats inesperats quan endTime &lt; startTime.
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
        // TimeOnly - TimeOnly retorna la diferència com si fos dins del mateix dia,
        // per tant 9:00 - 17:00 = -8 hores, però TimeSpan ho tracta com 16 hores (cap al futur).
        // Aquest comportament hauria de ser validat pel ValidationService abans.
        Assert.True(result < 0 || result > 8); // La durada no és vàlida per un dia normal
    }

    /// <summary>
    /// Verifica que CalculateDuration calcula correctament 15 minuts.
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
    /// Verifica que CalculateTotalHours retorna zero quan no hi ha registres.
    /// </summary>
    [Fact]
    public void CalculateTotalHours_TimeRecords_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var records = Enumerable.Empty<TimeRecord>();

        // Act
        var result = _sut.CalculateTotalHours(records);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifica que CalculateTotalHours suma correctament les hores dels registres.
    /// </summary>
    [Fact]
    public void CalculateTotalHours_TimeRecords_ShouldSumAllRecords()
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
    /// Verifica que CalculateTotalHours ignora registres sense hora de fi.
    /// </summary>
    [Fact]
    public void CalculateTotalHours_TimeRecords_ShouldIgnoreRecordsWithoutEndTime()
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
                EndTime = null // Registre en curs
            }
        };

        // Act
        var result = _sut.CalculateTotalHours(records);

        // Assert
        Assert.Equal(3.0, result);
    }

    #endregion

    #region CalculateTotalHours WorkdaySlot Tests

    /// <summary>
    /// Verifica que CalculateTotalHours per franges retorna zero quan no n'hi ha.
    /// </summary>
    [Fact]
    public void CalculateTotalHours_WorkdaySlots_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var slots = Enumerable.Empty<WorkdaySlot>();

        // Act
        var result = _sut.CalculateTotalHours(slots);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifica que CalculateTotalHours suma correctament les hores de les franges.
    /// </summary>
    [Fact]
    public void CalculateTotalHours_WorkdaySlots_ShouldSumAllSlots()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            },
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(18, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTotalHours(slots);

        // Assert
        Assert.Equal(9.0, result);
    }

    #endregion

    #region CalculateTeleworkHours Tests

    /// <summary>
    /// Verifica que CalculateTeleworkHours retorna zero quan no hi ha franges.
    /// </summary>
    [Fact]
    public void CalculateTeleworkHours_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var slots = Enumerable.Empty<WorkdaySlot>();

        // Act
        var result = _sut.CalculateTeleworkHours(slots);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifica que CalculateTeleworkHours suma només les franges de teletreball.
    /// </summary>
    [Fact]
    public void CalculateTeleworkHours_ShouldSumOnlyTeleworkSlots()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            },
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(18, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkHours(slots);

        // Assert
        Assert.Equal(3.0, result);
    }

    /// <summary>
    /// Verifica que CalculateTeleworkHours retorna zero quan no hi ha teletreball.
    /// </summary>
    [Fact]
    public void CalculateTeleworkHours_WhenNoTelework_ShouldReturnZero()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            }
        };

        // Act
        var result = _sut.CalculateTeleworkHours(slots);

        // Assert
        Assert.Equal(0.0, result);
    }

    #endregion

    #region CalculateOfficeHours Tests

    /// <summary>
    /// Verifica que CalculateOfficeHours retorna zero quan no hi ha franges.
    /// </summary>
    [Fact]
    public void CalculateOfficeHours_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var slots = Enumerable.Empty<WorkdaySlot>();

        // Act
        var result = _sut.CalculateOfficeHours(slots);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifica que CalculateOfficeHours suma només les franges d'oficina.
    /// </summary>
    [Fact]
    public void CalculateOfficeHours_ShouldSumOnlyOfficeSlots()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            },
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(18, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateOfficeHours(slots);

        // Assert
        Assert.Equal(6.0, result);
    }

    /// <summary>
    /// Verifica que CalculateOfficeHours retorna zero quan tot és teletreball.
    /// </summary>
    [Fact]
    public void CalculateOfficeHours_WhenAllTelework_ShouldReturnZero()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateOfficeHours(slots);

        // Assert
        Assert.Equal(0.0, result);
    }

    #endregion

    #region CalculateTeleworkPercentage Tests

    /// <summary>
    /// Verifica que CalculateTeleworkPercentage retorna zero quan no hi ha franges.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        var slots = Enumerable.Empty<WorkdaySlot>();

        // Act
        var result = _sut.CalculateTeleworkPercentage(slots);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifica que CalculateTeleworkPercentage calcula correctament el percentatge.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_ShouldCalculateCorrectPercentage()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Telework = false
            },
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(13, 0),
                EndTime = new TimeOnly(17, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkPercentage(slots);

        // Assert
        Assert.Equal(50.0, result);
    }

    /// <summary>
    /// Verifica que CalculateTeleworkPercentage retorna 100 quan tot és teletreball.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_WhenAllTelework_ShouldReturn100()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkPercentage(slots);

        // Assert
        Assert.Equal(100.0, result);
    }

    /// <summary>
    /// Verifica que CalculateTeleworkPercentage retorna 0 quan no hi ha teletreball.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_WhenNoTelework_ShouldReturnZero()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            }
        };

        // Act
        var result = _sut.CalculateTeleworkPercentage(slots);

        // Assert
        Assert.Equal(0.0, result);
    }

    /// <summary>
    /// Verifica que CalculateTeleworkPercentage calcula correctament amb proporcions no iguals.
    /// </summary>
    [Fact]
    public void CalculateTeleworkPercentage_WithUnequalSlots_ShouldCalculateCorrectPercentage()
    {
        // Arrange
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0), // 6 hores oficina
                Telework = false
            },
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(17, 0), // 2 hores teletreball
                Telework = true
            }
        };

        // Act
        var result = _sut.CalculateTeleworkPercentage(slots);

        // Assert
        Assert.Equal(25.0, result); // 2/8 = 25%
    }

    #endregion
}
