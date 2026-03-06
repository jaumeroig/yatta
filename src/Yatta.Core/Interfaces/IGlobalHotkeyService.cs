namespace Yatta.Core.Interfaces;

/// <summary>
/// Service to manage global keyboard shortcuts (hotkeys) that work system-wide.
/// </summary>
public interface IGlobalHotkeyService
{
    /// <summary>
    /// Event raised when the registered global hotkey is pressed.
    /// </summary>
    event EventHandler? HotkeyPressed;

    /// <summary>
    /// Registers a global hotkey with the specified combination.
    /// </summary>
    /// <param name="hotkeyString">Hotkey combination (e.g., "Control+Alt+A").</param>
    /// <returns>True if registration was successful, false otherwise.</returns>
    bool RegisterHotkey(string? hotkeyString);

    /// <summary>
    /// Unregisters the currently registered global hotkey.
    /// </summary>
    void UnregisterHotkey();

    /// <summary>
    /// Gets the default hotkey combination.
    /// </summary>
    /// <returns>The default hotkey string (e.g., "Control+Alt+A").</returns>
    string GetDefaultHotkey();

    /// <summary>
    /// Validates if a hotkey combination is valid.
    /// </summary>
    /// <param name="hotkeyString">Hotkey combination to validate.</param>
    /// <param name="errorMessage">Error message if validation fails.</param>
    /// <returns>True if the hotkey is valid, false otherwise.</returns>
    bool ValidateHotkey(string? hotkeyString, out string errorMessage);

    /// <summary>
    /// Gets whether a hotkey is currently registered.
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    /// Gets the currently registered hotkey string.
    /// </summary>
    string? CurrentHotkey { get; }
}
