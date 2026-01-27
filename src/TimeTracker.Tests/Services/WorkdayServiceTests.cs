namespace TimeTracker.Tests.Services;

using Moq;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using TimeTracker.Core.Services;

/// <summary>
/// Unit tests for the workday service.
/// </summary>
public class WorkdayServiceTests
{
    private readonly Mock<IWorkdaySlotRepository> _mockRepository;
    private readonly Mock<ITimeCalculatorService> _mockCalculator;
    private readonly Mock<IValidationService> _mockValidation;
    private readonly WorkdayService _sut;

    public WorkdayServiceTests()
    {
        _mockRepository = new Mock<IWorkdaySlotRepository>();
        _mockCalculator = new Mock<ITimeCalculatorService>();
        _mockValidation = new Mock<IValidationService>();
        _sut = new WorkdayService(
            _mockRepository.Object,
            _mockCalculator.Object,
            _mockValidation.Object);
    }

    #region GetDailySummaryAsync Tests

    /// <summary>
    /// Verifies that GetDailySummaryAsync returns a summary with correct values.
    /// </summary>
    [Fact]
    public async Task GetDailySummaryAsync_ShouldReturnCorrectSummary()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = date,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            },
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = date,
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(18, 0),
                Telework = true
            }
        };

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(slots);

        _mockCalculator
            .Setup(c => c.CalculateTotalHours(It.IsAny<IEnumerable<WorkdaySlot>>()))
            .Returns(9.0);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkHours(It.IsAny<IEnumerable<WorkdaySlot>>()))
            .Returns(3.0);

        _mockCalculator
            .Setup(c => c.CalculateOfficeHours(It.IsAny<IEnumerable<WorkdaySlot>>()))
            .Returns(6.0);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkPercentage(It.IsAny<IEnumerable<WorkdaySlot>>()))
            .Returns(33.33);

        // Act
        var result = await _sut.GetDailySummaryAsync(date);

        // Assert
        Assert.Equal(date, result.Date);
        Assert.Equal(9.0, result.TotalHours);
        Assert.Equal(3.0, result.TeleworkHours);
        Assert.Equal(6.0, result.OfficeHours);
        Assert.Equal(33.33, result.TeleworkPercentage);
        Assert.Equal(2, result.SlotCount);
    }

    /// <summary>
    /// Verifies that GetDailySummaryAsync returns an empty summary when there are no slots.
    /// </summary>
    [Fact]
    public async Task GetDailySummaryAsync_WhenNoSlots_ShouldReturnEmptySummary()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(Enumerable.Empty<WorkdaySlot>());

        _mockCalculator
            .Setup(c => c.CalculateTotalHours(It.IsAny<IEnumerable<WorkdaySlot>>()))
            .Returns(0.0);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkHours(It.IsAny<IEnumerable<WorkdaySlot>>()))
            .Returns(0.0);

        _mockCalculator
            .Setup(c => c.CalculateOfficeHours(It.IsAny<IEnumerable<WorkdaySlot>>()))
            .Returns(0.0);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkPercentage(It.IsAny<IEnumerable<WorkdaySlot>>()))
            .Returns(0.0);

        // Act
        var result = await _sut.GetDailySummaryAsync(date);

        // Assert
        Assert.Equal(date, result.Date);
        Assert.Equal(0.0, result.TotalHours);
        Assert.Equal(0.0, result.TeleworkHours);
        Assert.Equal(0.0, result.OfficeHours);
        Assert.Equal(0.0, result.TeleworkPercentage);
        Assert.Equal(0, result.SlotCount);
    }

    #endregion

    #region CanAddWorkdaySlotAsync Tests

    /// <summary>
    /// Verifica que CanAddWorkdaySlotAsync retorna true quan la franja és vàlida.
    /// </summary>
    [Fact]
    public async Task CanAddWorkdaySlotAsync_WhenValid_ShouldReturnTrue()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(14, 0)
        };

        _mockValidation
            .Setup(v => v.ValidateWorkdaySlot(slot, out It.Ref<string>.IsAny))
            .Returns(true);

        _mockRepository
            .Setup(r => r.GetByDateAsync(slot.Date))
            .ReturnsAsync(Enumerable.Empty<WorkdaySlot>());

        _mockValidation
            .Setup(v => v.ValidateNoOverlap(slot, It.IsAny<IEnumerable<WorkdaySlot>>(), out It.Ref<string>.IsAny))
            .Returns(true);

        // Act
        var result = await _sut.CanAddWorkdaySlotAsync(slot);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifica que CanAddWorkdaySlotAsync retorna false quan la franja no és vàlida.
    /// </summary>
    [Fact]
    public async Task CanAddWorkdaySlotAsync_WhenInvalidSlot_ShouldReturnFalse()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(9, 0) // Hora fi anterior a hora inici
        };

        var errorMessage = "Validation_EndTimeAfterStartTime";
        _mockValidation
            .Setup(v => v.ValidateWorkdaySlot(slot, out errorMessage))
            .Returns(false);

        // Act
        var result = await _sut.CanAddWorkdaySlotAsync(slot);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifica que CanAddWorkdaySlotAsync retorna false quan hi ha solapament.
    /// </summary>
    [Fact]
    public async Task CanAddWorkdaySlotAsync_WhenOverlaps_ShouldReturnFalse()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(15, 0)
        };

        var existingSlots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        var emptyError = string.Empty;
        _mockValidation
            .Setup(v => v.ValidateWorkdaySlot(slot, out emptyError))
            .Returns(true);

        _mockRepository
            .Setup(r => r.GetByDateAsync(slot.Date))
            .ReturnsAsync(existingSlots);

        var overlapError = "Validation_OverlappingSlot|09:00|12:00";
        _mockValidation
            .Setup(v => v.ValidateNoOverlap(slot, existingSlots, out overlapError))
            .Returns(false);

        // Act
        var result = await _sut.CanAddWorkdaySlotAsync(slot);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ValidateWorkdaySlotAsync Tests

    /// <summary>
    /// Verifica que ValidateWorkdaySlotAsync retorna error quan la franja no és vàlida.
    /// </summary>
    [Fact]
    public async Task ValidateWorkdaySlotAsync_WhenInvalid_ShouldReturnError()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(9, 0)
        };

        var errorMessage = "Validation_EndTimeAfterStartTime";
        _mockValidation
            .Setup(v => v.ValidateWorkdaySlot(slot, out errorMessage))
            .Returns(false);

        // Act
        var (isValid, error) = await _sut.ValidateWorkdaySlotAsync(slot);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Validation_EndTimeAfterStartTime", error);
    }

    /// <summary>
    /// Verifica que ValidateWorkdaySlotAsync retorna error quan hi ha solapament.
    /// </summary>
    [Fact]
    public async Task ValidateWorkdaySlotAsync_WhenOverlaps_ShouldReturnOverlapError()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(15, 0)
        };

        var existingSlots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        var emptyError = string.Empty;
        _mockValidation
            .Setup(v => v.ValidateWorkdaySlot(slot, out emptyError))
            .Returns(true);

        _mockRepository
            .Setup(r => r.GetByDateAsync(slot.Date))
            .ReturnsAsync(existingSlots);

        var overlapError = "Validation_OverlappingSlot|09:00|12:00";
        _mockValidation
            .Setup(v => v.ValidateNoOverlap(slot, existingSlots, out overlapError))
            .Returns(false);

        // Act
        var (isValid, error) = await _sut.ValidateWorkdaySlotAsync(slot);

        // Assert
        Assert.False(isValid);
        Assert.StartsWith("Validation_OverlappingSlot", error);
    }

    /// <summary>
    /// Verifica que ValidateWorkdaySlotAsync retorna èxit quan tot és vàlid.
    /// </summary>
    [Fact]
    public async Task ValidateWorkdaySlotAsync_WhenValid_ShouldReturnSuccess()
    {
        // Arrange
        var slot = new WorkdaySlot
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(14, 0)
        };

        var emptyError = string.Empty;
        _mockValidation
            .Setup(v => v.ValidateWorkdaySlot(slot, out emptyError))
            .Returns(true);

        _mockRepository
            .Setup(r => r.GetByDateAsync(slot.Date))
            .ReturnsAsync(Enumerable.Empty<WorkdaySlot>());

        _mockValidation
            .Setup(v => v.ValidateNoOverlap(slot, It.IsAny<IEnumerable<WorkdaySlot>>(), out emptyError))
            .Returns(true);

        // Act
        var (isValid, error) = await _sut.ValidateWorkdaySlotAsync(slot);

        // Assert
        Assert.True(isValid);
        Assert.Equal(string.Empty, error);
    }

    #endregion

    #region GetTotalHoursAsync Tests

    /// <summary>
    /// Verifica que GetTotalHoursAsync retorna el total d'hores correcte.
    /// </summary>
    [Fact]
    public async Task GetTotalHoursAsync_ShouldReturnCorrectTotalHours()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var endDate = startDate.AddDays(7);
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = startDate,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0)
            }
        };

        _mockRepository
            .Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(slots);

        _mockCalculator
            .Setup(c => c.CalculateTotalHours(slots))
            .Returns(8.0);

        // Act
        var result = await _sut.GetTotalHoursAsync(startDate, endDate);

        // Assert
        Assert.Equal(8.0, result);
    }

    #endregion

    #region GetTeleworkPercentageAsync Tests

    /// <summary>
    /// Verifica que GetTeleworkPercentageAsync retorna el percentatge correcte.
    /// </summary>
    [Fact]
    public async Task GetTeleworkPercentageAsync_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var endDate = startDate.AddDays(7);
        var slots = new List<WorkdaySlot>
        {
            new WorkdaySlot
            {
                Id = Guid.NewGuid(),
                Date = startDate,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                Telework = true
            }
        };

        _mockRepository
            .Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(slots);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkPercentage(slots))
            .Returns(100.0);

        // Act
        var result = await _sut.GetTeleworkPercentageAsync(startDate, endDate);

        // Assert
        Assert.Equal(100.0, result);
    }

    #endregion
}
