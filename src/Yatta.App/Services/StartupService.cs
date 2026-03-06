namespace Yatta.App.Services;

using Microsoft.Win32;
using System;
using System.IO;
using Yatta.Core.Interfaces;

/// <summary>
/// Service to manage Windows startup configuration.
/// </summary>
public class StartupService : IStartupService
{
    private const string AppName = "TimeTracker";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Enables the application to start automatically when Windows starts.
    /// </summary>
    public void EnableStartup()
    {
        try
        {
            var exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath))
                return;

            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            key?.SetValue(AppName, $"\"{exePath}\"");
        }
        catch (Exception ex)
        {
            // Log error or handle silently - registry access might be restricted
            System.Diagnostics.Debug.WriteLine($"Failed to enable startup: {ex.Message}");
        }
    }

    /// <summary>
    /// Disables the application from starting automatically when Windows starts.
    /// </summary>
    public void DisableStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key?.GetValue(AppName) != null)
                key.DeleteValue(AppName, false);
        }
        catch (Exception ex)
        {
            // Log error or handle silently - registry access might be restricted
            System.Diagnostics.Debug.WriteLine($"Failed to disable startup: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the application is configured to start with Windows.
    /// </summary>
    /// <returns>True if startup is enabled, false otherwise.</returns>
    public bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            return key?.GetValue(AppName) != null;
        }
        catch (Exception ex)
        {
            // Log error or handle silently - registry access might be restricted
            System.Diagnostics.Debug.WriteLine($"Failed to check startup status: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the path to the executable.
    /// </summary>
    /// <returns>The path to the executable, or null if not found.</returns>
    private static string? GetExecutablePath()
    {
        // For .NET 5+ applications, Environment.ProcessPath is more reliable
        var processPath = Environment.ProcessPath;

        if (!string.IsNullOrEmpty(processPath))
        {
            // If it's a .dll (development/debug mode), try to find the .exe
            if (Path.GetExtension(processPath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                var exePath = Path.ChangeExtension(processPath, ".exe");
                if (File.Exists(exePath))
                    return exePath;
            }

            return processPath;
        }

        return null;
    }
}
