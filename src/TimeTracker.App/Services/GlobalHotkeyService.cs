namespace TimeTracker.App.Services;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using TimeTracker.Core.Interfaces;

/// <summary>
/// Service to manage global keyboard shortcuts (hotkeys) using Windows API.
/// </summary>
public class GlobalHotkeyService : IGlobalHotkeyService, IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotkeyId = 9000;
    private const string DefaultHotkey = "Control+Alt+A";

    private IntPtr _windowHandle;
    private HwndSource? _source;
    private bool _isRegistered;
    private string? _currentHotkey;

    /// <summary>
    /// Event raised when the registered global hotkey is pressed.
    /// </summary>
    public event EventHandler? HotkeyPressed;

    /// <summary>
    /// Gets whether a hotkey is currently registered.
    /// </summary>
    public bool IsRegistered => _isRegistered;

    /// <summary>
    /// Gets the currently registered hotkey string.
    /// </summary>
    public string? CurrentHotkey => _currentHotkey;

    /// <summary>
    /// Initializes the service with the main window handle.
    /// This method must be called before RegisterHotkey.
    /// </summary>
    /// <param name="window">The main window to use for receiving hotkey messages.</param>
    public void Initialize(Window window)
    {
        _windowHandle = new WindowInteropHelper(window).Handle;
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(HwndHook);
    }

    /// <summary>
    /// Registers a global hotkey with the specified combination.
    /// </summary>
    /// <param name="hotkeyString">Hotkey combination (e.g., "Control+Alt+A").</param>
    /// <returns>True if registration was successful, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown if Initialize has not been called.</exception>
    public bool RegisterHotkey(string? hotkeyString)
    {
        if (_windowHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("GlobalHotkeyService must be initialized before registering a hotkey. Call Initialize() first.");
        }

        // Unregister any existing hotkey first
        UnregisterHotkey();

        if (string.IsNullOrWhiteSpace(hotkeyString))
        {
            hotkeyString = DefaultHotkey;
        }

        if (!ParseHotkey(hotkeyString, out uint modifiers, out uint key))
        {
            return false;
        }

        _isRegistered = RegisterHotKey(_windowHandle, HotkeyId, modifiers, key);
        if (_isRegistered)
        {
            _currentHotkey = hotkeyString;
        }

        return _isRegistered;
    }

    /// <summary>
    /// Unregisters the currently registered global hotkey.
    /// </summary>
    public void UnregisterHotkey()
    {
        if (_isRegistered && _windowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(_windowHandle, HotkeyId);
            _isRegistered = false;
            _currentHotkey = null;
        }
    }

    /// <summary>
    /// Gets the default hotkey combination.
    /// </summary>
    /// <returns>The default hotkey string (e.g., "Control+Alt+A").</returns>
    public string GetDefaultHotkey()
    {
        return DefaultHotkey;
    }

    /// <summary>
    /// Validates if a hotkey combination is valid.
    /// </summary>
    /// <param name="hotkeyString">Hotkey combination to validate.</param>
    /// <param name="errorMessage">Error message if validation fails.</param>
    /// <returns>True if the hotkey is valid, false otherwise.</returns>
    public bool ValidateHotkey(string? hotkeyString, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(hotkeyString))
        {
            errorMessage = Resources.Resources.Validation_HotkeyRequired;
            return false;
        }

        if (!ParseHotkey(hotkeyString, out uint modifiers, out uint key))
        {
            errorMessage = Resources.Resources.Validation_HotkeyInvalid;
            return false;
        }

        // Validate that at least one modifier is present
        if (modifiers == 0)
        {
            errorMessage = Resources.Resources.Validation_HotkeyNeedsModifier;
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// Parses a hotkey string into modifier and key values.
    /// </summary>
    private bool ParseHotkey(string hotkeyString, out uint modifiers, out uint key)
    {
        modifiers = 0;
        key = 0;

        var parts = hotkeyString.Split('+');
        // Need at least one modifier and one key
        if (parts.Length < 2)
        {
            return false;
        }

        // Parse modifiers (all parts except the last one)
        for (int i = 0; i < parts.Length - 1; i++)
        {
            var modifier = parts[i].Trim();
            switch (modifier)
            {
                case "Control":
                case "Ctrl":
                    modifiers |= 0x0002; // MOD_CONTROL
                    break;
                case "Alt":
                    modifiers |= 0x0001; // MOD_ALT
                    break;
                case "Shift":
                    modifiers |= 0x0004; // MOD_SHIFT
                    break;
                case "Win":
                case "Windows":
                    modifiers |= 0x0008; // MOD_WIN
                    break;
                default:
                    return false;
            }
        }

        // Parse key (last part)
        var keyString = parts[^1].Trim();
        if (!TryGetVirtualKeyCode(keyString, out key))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Converts a key string to a virtual key code.
    /// </summary>
    private bool TryGetVirtualKeyCode(string keyString, out uint keyCode)
    {
        keyCode = 0;

        // Try to parse as WPF Key enum
        if (Enum.TryParse<Key>(keyString, true, out var wpfKey))
        {
            keyCode = (uint)KeyInterop.VirtualKeyFromKey(wpfKey);
            return keyCode != 0;
        }

        return false;
    }

    /// <summary>
    /// Hook to handle Windows messages.
    /// </summary>
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Disposes the service and unregisters any active hotkeys.
    /// </summary>
    public void Dispose()
    {
        UnregisterHotkey();
        if (_source != null)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
        }
    }

    #region Windows API

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    #endregion
}
