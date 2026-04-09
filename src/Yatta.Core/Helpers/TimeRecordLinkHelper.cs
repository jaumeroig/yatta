namespace Yatta.Core.Helpers;

using System;

/// <summary>
/// Provides validation helpers for time record links.
/// </summary>
public static class TimeRecordLinkHelper
{
    /// <summary>
    /// Determines whether the value is a valid absolute HTTP or HTTPS link.
    /// </summary>
    /// <param name="value">The link value to validate.</param>
    /// <returns><see langword="true"/> when the value is a valid HTTP or HTTPS link; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string? value)
    {
        return TryCreateUri(value, out _);
    }

    /// <summary>
    /// Tries to create a valid absolute HTTP or HTTPS <see cref="Uri"/> from the provided value.
    /// </summary>
    /// <param name="value">The link value to validate.</param>
    /// <param name="uri">The parsed <see cref="Uri"/> when validation succeeds; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the value is a valid HTTP or HTTPS link; otherwise, <see langword="false"/>.</returns>
    public static bool TryCreateUri(string? value, out Uri? uri)
    {
        uri = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var parsedUri))
        {
            return false;
        }

        if (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        uri = parsedUri;
        return true;
    }
}
