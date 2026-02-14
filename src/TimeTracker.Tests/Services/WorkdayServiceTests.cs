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
    private readonly Mock<ITimeRecordRepository> _mockRepository;
    private readonly Mock<ITimeCalculatorService> _mockCalculator;
    private readonly WorkdayService _sut;

    public WorkdayServiceTests()
    {
        _mockRepository = new Mock<ITimeRecordRepository>();
        _mockCalculator = new Mock<ITimeCalculatorService>();
        _sut = new WorkdayService(
            _mockRepository.Object,
            _mockCalculator.Object);
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
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = date,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(14, 0),
                Telework = false
            },
            new TimeRecord
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
            .ReturnsAsync(records);

        _mockCalculator
            .Setup(c => c.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(9.0);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(3.0);

        _mockCalculator
            .Setup(c => c.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(6.0);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(33.33);

        // Act
        var result = await _sut.GetDailySummaryAsync(date);

        // Assert
        Assert.Equal(date, result.Date);
        Assert.Equal(9.0, result.TotalHours);
        Assert.Equal(3.0, result.TeleworkHours);
        Assert.Equal(6.0, result.OfficeHours);
        Assert.Equal(33.33, result.TeleworkPercentage);
        Assert.Equal(2, result.RecordCount);
    }

    /// <summary>
    /// Verifies that GetDailySummaryAsync returns an empty summary when there are no records.
    /// </summary>
    [Fact]
    public async Task GetDailySummaryAsync_WhenNoRecords_ShouldReturnEmptySummary()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        _mockRepository
            .Setup(r => r.GetByDateAsync(date))
            .ReturnsAsync(Enumerable.Empty<TimeRecord>());

        _mockCalculator
            .Setup(c => c.CalculateTotalHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(0.0);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(0.0);

        _mockCalculator
            .Setup(c => c.CalculateOfficeHours(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(0.0);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkPercentage(It.IsAny<IEnumerable<TimeRecord>>()))
            .Returns(0.0);

        // Act
        var result = await _sut.GetDailySummaryAsync(date);

        // Assert
        Assert.Equal(date, result.Date);
        Assert.Equal(0.0, result.TotalHours);
        Assert.Equal(0.0, result.TeleworkHours);
        Assert.Equal(0.0, result.OfficeHours);
        Assert.Equal(0.0, result.TeleworkPercentage);
        Assert.Equal(0, result.RecordCount);
    }

    #endregion

    #region GetTotalHoursAsync Tests

    /// <summary>
    /// Verifies that GetTotalHoursAsync returns the correct total hours.
    /// </summary>
    [Fact]
    public async Task GetTotalHoursAsync_ShouldReturnCorrectTotalHours()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var endDate = startDate.AddDays(7);
        var records = new List<TimeRecord>
        {
            new TimeRecord
            {
                Id = Guid.NewGuid(),
                Date = startDate,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0)
            }
        };

        _mockRepository
            .Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(records);

        _mockCalculator
            .Setup(c => c.CalculateTotalHours(records))
            .Returns(8.0);

        // Act
        var result = await _sut.GetTotalHoursAsync(startDate, endDate);

        // Assert
        Assert.Equal(8.0, result);
    }

    #endregion

    #region GetTeleworkPercentageAsync Tests

    /// <summary>
    /// Verifies that GetTeleworkPercentageAsync returns the correct percentage.
    /// </summary>
    [Fact]
    public async Task GetTeleworkPercentageAsync_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var endDate = startDate.AddDays(7);
        var records = new List<TimeRecord>
        {
            new TimeRecord
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
            .ReturnsAsync(records);

        _mockCalculator
            .Setup(c => c.CalculateTeleworkPercentage(records))
            .Returns(100.0);

        // Act
        var result = await _sut.GetTeleworkPercentageAsync(startDate, endDate);

        // Assert
        Assert.Equal(100.0, result);
    }

    #endregion
}
