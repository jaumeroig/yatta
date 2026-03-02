namespace TimeTracker.App.Extensions;

using System;


/// <summary>
/// Provides extension methods for the <see cref="string"/> class.
/// </summary>
internal static class StringExtensions
{
    extension(string value)
    {
        /// <summary>
        /// Truncates the specified string to a maximum length.
        /// </summary>
        /// <remarks>Use this method to ensure that a string does not exceed a specified length, which can be
        /// useful for display, storage, or validation scenarios. If the input string is null or empty, it is returned
        /// unchanged.</remarks>
        /// <param name="value">The string to truncate. If null or empty, the original value is returned.</param>
        /// <param name="maxLength">The maximum number of characters to return. Must be zero or greater.</param>
        /// <returns>A string that is at most the specified maximum length. If the input string is shorter than or equal to the
        /// maximum length, the original string is returned; otherwise, a truncated version is returned.</returns>
        public string Truncate(int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        /// <summary>
        /// Truncates the specified string to a maximum length and appends an ellipsis if the string exceeds that length.
        /// </summary>
        /// <remarks>If the input string is null or empty, it is returned unchanged. When the maximum length is
        /// less than or equal to 3, the method returns a string of dots instead of truncating the original
        /// string.</remarks>
        /// <param name="value">The string to truncate. If null or empty, the original value is returned.</param>
        /// <param name="maxLength">The maximum length of the returned string, including the ellipsis. Must be greater than or equal to 0.</param>
        /// <returns>A truncated version of the input string with an appended ellipsis if the original string exceeds the specified
        /// maximum length. If the maximum length is less than or equal to 3, returns a string consisting of dots with the
        /// specified length.</returns>
        public string TruncateWithEllipsis(int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length <= maxLength) return value;
            if (maxLength <= 3) return new string('.', maxLength);
            return string.Concat(value.AsSpan(0, maxLength - 3), "...");
        }

        /// <summary>
        /// Returns a new string with the first character of the specified string converted to uppercase.
        /// </summary>
        /// <remarks>Only the first character is modified; all other characters remain unchanged. This method does
        /// not modify the original string.</remarks>
        /// <param name="value">The string to capitalize. Cannot be null or empty.</param>
        /// <returns>A string with the first character converted to uppercase. If the input string is null or empty, the original
        /// string is returned.</returns>
        public string Capitalize()
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length == 1) return value.ToUpper();
            return char.ToUpper(value[0]) + value[1..];
        }
    }
}
