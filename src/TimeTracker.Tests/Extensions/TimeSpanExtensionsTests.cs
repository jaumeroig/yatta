namespace TimeTracker.Tests.Extensions;

using TimeTracker.App.Extensions;

/// <summary>
/// Unit tests for the <see cref="TimeSpanExtensions"/> class.
/// </summary>
public class TimeSpanExtensionsTests
{
    [Theory]
    [InlineData(210, "3h 30m")]
    [InlineData(-210, "3h 30m")]
    [InlineData(0, "0h 0m")]
    [InlineData(45, "0h 45m")]
    [InlineData(60, "1h 0m")]
    public void FormatDuration_WithShowSignFalse_ReturnsFormattedWithoutSign(int totalMinutes, string expected)
    {
        // Arrange
        var timeSpan = TimeSpan.FromMinutes(totalMinutes);

        // Act
        var result = timeSpan.FormatDuration(showSign: false);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(210, "+3h 30m")]
    [InlineData(-210, "-3h 30m")]
    [InlineData(0, "0h 0m")]
    public void FormatDuration_WithShowSignTrue_ReturnsDurationWithSign(int totalMinutes, string expected)
    {
        // Arrange
        var timeSpan = TimeSpan.FromMinutes(totalMinutes);

        // Act
        var result = timeSpan.FormatDuration(showSign: true);

        // Assert
        Assert.Equal(expected, result);
    }
}
