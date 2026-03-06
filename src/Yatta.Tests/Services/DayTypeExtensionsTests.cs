namespace Yatta.Tests.Services;

using Yatta.Core.Extensions;
using Yatta.Core.Models;

/// <summary>
/// Unit tests for the <see cref="DayTypeExtensions"/> class
/// and the <see cref="Core.Attributes.WorkableDayAttribute"/> attribute.
/// </summary>
public class DayTypeExtensionsTests
{
    #region IsWorkable Tests

    /// <summary>
    /// Verifies that WorkDay is marked as workable.
    /// </summary>
    [Fact]
    public void IsWorkable_ForWorkDay_ShouldReturnTrue()
    {
        // Arrange
        var dayType = DayType.WorkDay;

        // Act
        var result = dayType.IsWorkable();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that IntensiveDay is marked as workable.
    /// </summary>
    [Fact]
    public void IsWorkable_ForIntensiveDay_ShouldReturnTrue()
    {
        // Arrange
        var dayType = DayType.IntensiveDay;

        // Act
        var result = dayType.IsWorkable();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that Holiday is not workable.
    /// </summary>
    [Fact]
    public void IsWorkable_ForHoliday_ShouldReturnFalse()
    {
        // Arrange
        var dayType = DayType.Holiday;

        // Act
        var result = dayType.IsWorkable();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that FreeChoice is not workable.
    /// </summary>
    [Fact]
    public void IsWorkable_ForFreeChoice_ShouldReturnFalse()
    {
        // Arrange
        var dayType = DayType.FreeChoice;

        // Act
        var result = dayType.IsWorkable();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that Vacation is not workable.
    /// </summary>
    [Fact]
    public void IsWorkable_ForVacation_ShouldReturnFalse()
    {
        // Arrange
        var dayType = DayType.Vacation;

        // Act
        var result = dayType.IsWorkable();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that all DayType enum values have the WorkableDayAttribute.
    /// Ensures extensibility by catching missing attributes on new enum values.
    /// </summary>
    [Fact]
    public void AllDayTypeValues_ShouldHaveWorkableDayAttribute()
    {
        // Arrange
        var allDayTypes = Enum.GetValues<DayType>();

        // Act & Assert
        foreach (var dayType in allDayTypes)
        {
            var fieldInfo = typeof(DayType).GetField(dayType.ToString());
            Assert.NotNull(fieldInfo);

            var attribute = fieldInfo.GetCustomAttributes(typeof(Core.Attributes.WorkableDayAttribute), false);
            Assert.Single(attribute);
        }
    }

    /// <summary>
    /// Verifies that an undefined enum value returns false (safe default).
    /// </summary>
    [Fact]
    public void IsWorkable_ForUndefinedValue_ShouldReturnFalse()
    {
        // Arrange
        var dayType = (DayType)999;

        // Act
        var result = dayType.IsWorkable();

        // Assert
        Assert.False(result);
    }

    #endregion
}
