namespace Yatta.Tests.Services;

using Moq;
using Yatta.App.Blazor;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>Protects time editing boundaries and compatibility with Blueprint themes.</summary>
public class UiEditingTests
{
    /// <summary>Editing only notes must not move midnight or truncate stored seconds.</summary>
    [Theory]
    [InlineData(0, 0, 0, "00:00")]
    [InlineData(9, 30, 42, "09:30")]
    [InlineData(23, 59, 59, "23:59")]
    public void EditedTime_UnchangedMinutePreservesOriginal(int hour, int minute, int second, string text)
    {
        TimeOnly original = new TimeOnly(hour, minute, second).Add(TimeSpan.FromTicks(123));

        Assert.Equal(original, UiFormat.EditedTime(text, original));
    }

    /// <summary>A deliberate change replaces the time and clearing the end reopens the record.</summary>
    [Fact]
    public void EditedTime_AppliesChangedAndClearedValues()
    {
        Assert.Equal(new TimeOnly(10, 45), UiFormat.EditedTime("10:45", new TimeOnly(9, 30, 42)));
        Assert.Equal(TimeOnly.MinValue, UiFormat.EditedTime("00:00", null));
        Assert.Null(UiFormat.EditedTime(string.Empty, new TimeOnly(10, 45)));
    }

    /// <summary>Day configuration rejects invalid values instead of silently normalizing them.</summary>
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(23, 59, true)]
    [InlineData(24, 0, true)]
    [InlineData(24, 1, false)]
    [InlineData(25, 0, false)]
    [InlineData(-1, 0, false)]
    [InlineData(8, -1, false)]
    [InlineData(8, 60, false)]
    public void IsValidDayTarget_EnforcesDayBoundary(int hours, int minutes, bool expected)
    {
        Assert.Equal(expected, UiFormat.IsValidDayTarget(hours, minutes));
    }

    /// <summary>Dark mode enables both application styles and Blueprint's dark variants.</summary>
    [Fact]
    public void ThemeClass_DarkEnablesBlueprintTheme()
    {
        Mock<IThemeService> theme = new();
        theme.Setup(service => service.GetCurrentTheme()).Returns(Theme.Dark);

        Assert.Contains("dark", UiFormat.ThemeClass(theme.Object).Split(' '));
    }
}
