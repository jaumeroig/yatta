namespace TimeTracker.App.Controls;

using System.Text;
using System.Windows;
using System.Windows.Input;

/// <summary>
/// A TextBox control that captures keyboard shortcuts automatically.
/// When focused, it captures key combinations and displays them in the proper format.
/// </summary>
public class HotkeyTextBox : Wpf.Ui.Controls.TextBox
{
    /// <summary>
    /// Dependency property for the captured hotkey.
    /// </summary>
    public static readonly DependencyProperty HotkeyProperty =
        DependencyProperty.Register(
            nameof(Hotkey),
            typeof(string),
            typeof(HotkeyTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHotkeyChanged));

    private bool _isCapturing;

    public HotkeyTextBox()
    {
        // Make the control read-only by default
        IsReadOnly = true;
        IsReadOnlyCaretVisible = false;
        
        // Set up event handlers
        GotFocus += OnGotFocus;
        LostFocus += OnLostFocus;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    /// <summary>
    /// Gets or sets the captured hotkey string.
    /// </summary>
    public string Hotkey
    {
        get => (string)GetValue(HotkeyProperty);
        set => SetValue(HotkeyProperty, value);
    }

    private static void OnHotkeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HotkeyTextBox textBox)
        {
            textBox.Text = e.NewValue as string ?? string.Empty;
        }
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        _isCapturing = true;
        
        // Show placeholder text when focused
        if (string.IsNullOrEmpty(Hotkey))
        {
            Text = TimeTracker.App.Resources.Resources.Placeholder_PressKeys;
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        _isCapturing = false;
        
        // Restore the hotkey text if it was set
        if (!string.IsNullOrEmpty(Hotkey))
        {
            Text = Hotkey;
        }
        else
        {
            Text = string.Empty;
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isCapturing)
        {
            return;
        }

        // Prevent the key from being processed normally
        e.Handled = true;

        // Get the actual key (not the system key)
        Key key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore modifier keys by themselves
        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin)
        {
            return;
        }

        // Special handling for certain keys
        if (key == Key.Back || key == Key.Delete)
        {
            // Clear the hotkey
            Hotkey = string.Empty;
            Text = TimeTracker.App.Resources.Resources.Placeholder_PressKeys;
            return;
        }

        if (key == Key.Escape)
        {
            // Cancel capture and restore original value
            _isCapturing = false;
            Text = Hotkey;
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            return;
        }

        // Build the hotkey string
        var hotkeyString = BuildHotkeyString(key, Keyboard.Modifiers);
        
        if (!string.IsNullOrEmpty(hotkeyString))
        {
            Hotkey = hotkeyString;
            Text = hotkeyString;
        }
    }

    private static string BuildHotkeyString(Key key, ModifierKeys modifiers)
    {
        var parts = new StringBuilder();

        // Add modifiers in a consistent order
        if ((modifiers & ModifierKeys.Control) != 0)
        {
            parts.Append("Control+");
        }

        if ((modifiers & ModifierKeys.Alt) != 0)
        {
            parts.Append("Alt+");
        }

        if ((modifiers & ModifierKeys.Shift) != 0)
        {
            parts.Append("Shift+");
        }

        if ((modifiers & ModifierKeys.Windows) != 0)
        {
            parts.Append("Win+");
        }

        // Add the key
        parts.Append(GetKeyName(key));

        return parts.ToString();
    }

    private static string GetKeyName(Key key)
    {
        // Handle special keys
        return key switch
        {
            Key.Space => "Space",
            Key.Enter => "Enter",
            Key.Tab => "Tab",
            Key.Escape => "Escape",
            Key.Back => "Backspace",
            Key.Delete => "Delete",
            Key.Insert => "Insert",
            Key.Home => "Home",
            Key.End => "End",
            Key.PageUp => "PageUp",
            Key.PageDown => "PageDown",
            Key.Left => "Left",
            Key.Up => "Up",
            Key.Right => "Right",
            Key.Down => "Down",
            Key.F1 => "F1",
            Key.F2 => "F2",
            Key.F3 => "F3",
            Key.F4 => "F4",
            Key.F5 => "F5",
            Key.F6 => "F6",
            Key.F7 => "F7",
            Key.F8 => "F8",
            Key.F9 => "F9",
            Key.F10 => "F10",
            Key.F11 => "F11",
            Key.F12 => "F12",
            Key.NumPad0 => "NumPad0",
            Key.NumPad1 => "NumPad1",
            Key.NumPad2 => "NumPad2",
            Key.NumPad3 => "NumPad3",
            Key.NumPad4 => "NumPad4",
            Key.NumPad5 => "NumPad5",
            Key.NumPad6 => "NumPad6",
            Key.NumPad7 => "NumPad7",
            Key.NumPad8 => "NumPad8",
            Key.NumPad9 => "NumPad9",
            Key.Multiply => "Multiply",
            Key.Add => "Add",
            Key.Subtract => "Subtract",
            Key.Divide => "Divide",
            Key.Decimal => "Decimal",
            _ => key.ToString()
        };
    }
}
