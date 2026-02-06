namespace TimeTracker.Tests.Services;

using Moq;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using TimeTracker.Core.Services;

/// <summary>
/// Unit tests for the data purge service.
/// </summary>
public class DataPurgeServiceTests
{
    private readonly Mock<ITimeRecordRepository> _timeRecordRepositoryMock;
    private readonly Mock<IWorkdayRepository> _workdayRepositoryMock;
    private readonly DataPurgeService _sut;

    public DataPurgeServiceTests()
    {
        _timeRecordRepositoryMock = new Mock<ITimeRecordRepository>();
        _workdayRepositoryMock = new Mock<IWorkdayRepository>();
        _sut = new DataPurgeService(_timeRecordRepositoryMock.Object, _workdayRepositoryMock.Object);
    }

    #region CalculateCutoffDate Tests

    /// <summary>
    /// Verifies that CalculateCutoffDate returns null when policy is Forever.
    /// </summary>
    [Fact]
    public void CalculateCutoffDate_WhenPolicyIsForever_ShouldReturnNull()
    {
        // Act
        var result = _sut.CalculateCutoffDate(RetentionPolicy.Forever, 365);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that CalculateCutoffDate returns date minus 1 year when policy is OneYear.
    /// </summary>
    [Fact]
    public void CalculateCutoffDate_WhenPolicyIsOneYear_ShouldReturnDateMinusOneYear()
    {
        // Arrange
        var referenceDate = new DateOnly(2025, 6, 15);

        // Act
        var result = _sut.CalculateCutoffDate(RetentionPolicy.OneYear, 0, referenceDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2024, 6, 15), result.Value);
    }

    /// <summary>
    /// Verifies that CalculateCutoffDate returns date minus 2 years when policy is TwoYears.
    /// </summary>
    [Fact]
    public void CalculateCutoffDate_WhenPolicyIsTwoYears_ShouldReturnDateMinusTwoYears()
    {
        // Arrange
        var referenceDate = new DateOnly(2025, 6, 15);

        // Act
        var result = _sut.CalculateCutoffDate(RetentionPolicy.TwoYears, 0, referenceDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2023, 6, 15), result.Value);
    }

    /// <summary>
    /// Verifies that CalculateCutoffDate returns date minus 3 years when policy is ThreeYears.
    /// </summary>
    [Fact]
    public void CalculateCutoffDate_WhenPolicyIsThreeYears_ShouldReturnDateMinusThreeYears()
    {
        // Arrange
        var referenceDate = new DateOnly(2025, 6, 15);

        // Act
        var result = _sut.CalculateCutoffDate(RetentionPolicy.ThreeYears, 0, referenceDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2022, 6, 15), result.Value);
    }

    /// <summary>
    /// Verifies that CalculateCutoffDate returns date minus custom days when policy is Custom.
    /// </summary>
    [Fact]
    public void CalculateCutoffDate_WhenPolicyIsCustom_ShouldReturnDateMinusCustomDays()
    {
        // Arrange
        var referenceDate = new DateOnly(2025, 6, 15);
        var customDays = 90;

        // Act
        var result = _sut.CalculateCutoffDate(RetentionPolicy.Custom, customDays, referenceDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(referenceDate.AddDays(-90), result.Value);
    }

    /// <summary>
    /// Verifies that CalculateCutoffDate returns null when policy is Custom but days is 0.
    /// </summary>
    [Fact]
    public void CalculateCutoffDate_WhenPolicyIsCustomWithZeroDays_ShouldReturnNull()
    {
        // Act
        var result = _sut.CalculateCutoffDate(RetentionPolicy.Custom, 0);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that CalculateCutoffDate returns null when policy is Custom but days is negative.
    /// </summary>
    [Fact]
    public void CalculateCutoffDate_WhenPolicyIsCustomWithNegativeDays_ShouldReturnNull()
    {
        // Act
        var result = _sut.CalculateCutoffDate(RetentionPolicy.Custom, -5);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that CalculateCutoffDate uses today when no reference date is provided.
    /// </summary>
    [Fact]
    public void CalculateCutoffDate_WhenNoReferenceDate_ShouldUseToday()
    {
        // Act
        var result = _sut.CalculateCutoffDate(RetentionPolicy.OneYear, 0);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today).AddYears(-1), result.Value);
    }

    #endregion

    #region GetPurgeableCountAsync Tests

    /// <summary>
    /// Verifies that GetPurgeableCountAsync returns correct counts from repositories.
    /// </summary>
    [Fact]
    public async Task GetPurgeableCountAsync_ShouldReturnCountsFromRepositories()
    {
        // Arrange
        var cutoffDate = new DateOnly(2024, 1, 1);
        _timeRecordRepositoryMock.Setup(r => r.CountBeforeDateAsync(cutoffDate)).ReturnsAsync(15);
        _workdayRepositoryMock.Setup(r => r.CountBeforeDateAsync(cutoffDate)).ReturnsAsync(5);

        // Act
        var (timeRecordCount, workdayCount) = await _sut.GetPurgeableCountAsync(cutoffDate);

        // Assert
        Assert.Equal(15, timeRecordCount);
        Assert.Equal(5, workdayCount);
    }

    /// <summary>
    /// Verifies that GetPurgeableCountAsync returns zero when no records exist.
    /// </summary>
    [Fact]
    public async Task GetPurgeableCountAsync_WhenNoRecords_ShouldReturnZeros()
    {
        // Arrange
        var cutoffDate = new DateOnly(2024, 1, 1);
        _timeRecordRepositoryMock.Setup(r => r.CountBeforeDateAsync(cutoffDate)).ReturnsAsync(0);
        _workdayRepositoryMock.Setup(r => r.CountBeforeDateAsync(cutoffDate)).ReturnsAsync(0);

        // Act
        var (timeRecordCount, workdayCount) = await _sut.GetPurgeableCountAsync(cutoffDate);

        // Assert
        Assert.Equal(0, timeRecordCount);
        Assert.Equal(0, workdayCount);
    }

    #endregion

    #region ExecutePurgeAsync Tests

    /// <summary>
    /// Verifies that ExecutePurgeAsync deletes records and returns correct counts.
    /// </summary>
    [Fact]
    public async Task ExecutePurgeAsync_ShouldDeleteRecordsAndReturnCounts()
    {
        // Arrange
        var cutoffDate = new DateOnly(2024, 1, 1);
        _timeRecordRepositoryMock.Setup(r => r.DeleteBeforeDateAsync(cutoffDate)).ReturnsAsync(10);
        _workdayRepositoryMock.Setup(r => r.DeleteBeforeDateAsync(cutoffDate)).ReturnsAsync(3);

        // Act
        var (timeRecordsDeleted, workdaysDeleted) = await _sut.ExecutePurgeAsync(cutoffDate);

        // Assert
        Assert.Equal(10, timeRecordsDeleted);
        Assert.Equal(3, workdaysDeleted);
        _timeRecordRepositoryMock.Verify(r => r.DeleteBeforeDateAsync(cutoffDate), Times.Once);
        _workdayRepositoryMock.Verify(r => r.DeleteBeforeDateAsync(cutoffDate), Times.Once);
    }

    /// <summary>
    /// Verifies that ExecutePurgeAsync returns zeros when no records to delete.
    /// </summary>
    [Fact]
    public async Task ExecutePurgeAsync_WhenNoRecordsToDelete_ShouldReturnZeros()
    {
        // Arrange
        var cutoffDate = new DateOnly(2024, 1, 1);
        _timeRecordRepositoryMock.Setup(r => r.DeleteBeforeDateAsync(cutoffDate)).ReturnsAsync(0);
        _workdayRepositoryMock.Setup(r => r.DeleteBeforeDateAsync(cutoffDate)).ReturnsAsync(0);

        // Act
        var (timeRecordsDeleted, workdaysDeleted) = await _sut.ExecutePurgeAsync(cutoffDate);

        // Assert
        Assert.Equal(0, timeRecordsDeleted);
        Assert.Equal(0, workdaysDeleted);
    }

    #endregion
}
