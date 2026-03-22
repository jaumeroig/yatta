namespace Yatta.App.Helpers;

using AppResources = Yatta.App.Resources.Resources;

/// <summary>
/// Helper to localize pipe-delimited validation error messages from the validation service.
/// </summary>
internal static class ValidationErrorHelper
{
    /// <summary>
    /// Localizes a pipe-delimited validation error message.
    /// The format is "ResourceKey|arg1|arg2|...".
    /// </summary>
    /// <param name="errorMessage">The pipe-delimited error message from the validation service.</param>
    /// <returns>The localized error message.</returns>
    public static string Localize(string errorMessage)
    {
        var parts = errorMessage.Split('|');
        var key = parts[0];
        var format = AppResources.ResourceManager.GetString(key) ?? key;

        if (parts.Length > 1)
        {
            var args = parts.Skip(1).Cast<object>().ToArray();
            return string.Format(format, args);
        }

        return format;
    }
}
