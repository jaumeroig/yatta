namespace Yatta.Tests.Services;

using Yatta.Core.Helpers;

/// <summary>
/// Unit tests for <see cref="TimeRecordLinkHelper"/>.
/// </summary>
public class TimeRecordLinkHelperTests
{
    /// <summary>
    /// Verifies that HTTPS links are accepted.
    /// </summary>
    [Fact]
    public void IsValid_WhenHttpsLink_ShouldReturnTrue()
    {
        var result = TimeRecordLinkHelper.IsValid("https://example.com/docs");

        Assert.True(result);
    }

    /// <summary>
    /// Verifies that links are trimmed before validation.
    /// </summary>
    [Fact]
    public void IsValid_WhenLinkHasWhitespace_ShouldReturnTrue()
    {
        var result = TimeRecordLinkHelper.IsValid("  https://example.com/docs  ");

        Assert.True(result);
    }

    /// <summary>
    /// Verifies that relative links are rejected.
    /// </summary>
    [Fact]
    public void IsValid_WhenLinkIsRelative_ShouldReturnFalse()
    {
        var result = TimeRecordLinkHelper.IsValid("/docs/page");

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that non-web schemes are rejected.
    /// </summary>
    [Fact]
    public void IsValid_WhenSchemeIsNotHttpOrHttps_ShouldReturnFalse()
    {
        var result = TimeRecordLinkHelper.IsValid("file:///tmp/test.txt");

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that TryCreateUri returns the parsed URI for valid links.
    /// </summary>
    [Fact]
    public void TryCreateUri_WhenLinkIsValid_ShouldReturnParsedUri()
    {
        var result = TimeRecordLinkHelper.TryCreateUri("https://example.com/docs", out var uri);

        Assert.True(result);
        Assert.NotNull(uri);
        Assert.Equal("https://example.com/docs", uri.AbsoluteUri);
    }
}
