namespace TimeTracker.App.Controls;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

/// <summary>
/// Reusable time picker control that provides a single editable dropdown
/// with time options generated at a configurable step interval.
/// </summary>
public partial class TimePickerControl : UserControl
{
    private const int DefaultStepMinutes = 15;
    private bool _isUpdating;

    /// <summary>
    /// Identifies the <see cref="TimeText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TimeTextProperty =
        DependencyProperty.Register(
            nameof(TimeText),
            typeof(string),
            typeof(TimePickerControl),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTimeTextChanged));

    /// <summary>
    /// Identifies the <see cref="Step"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(int),
            typeof(TimePickerControl),
            new PropertyMetadata(DefaultStepMinutes, OnStepChanged));

    /// <summary>
    /// Initializes a new instance of the <see cref="TimePickerControl"/> class.
    /// </summary>
    public TimePickerControl()
    {
        TimeOptions = new ObservableCollection<string>();
        GenerateTimeOptions();
        InitializeComponent();

        TimeComboBox.AddHandler(
            TextBoxBase.TextChangedEvent,
            new TextChangedEventHandler(OnComboBoxTextChanged));

        TimeComboBox.PreviewTextInput += OnPreviewTextInput;
        TimeComboBox.LostFocus += OnLostFocus;
    }

    /// <summary>
    /// Gets or sets the time text in HH:mm format.
    /// </summary>
    public string TimeText
    {
        get => (string)GetValue(TimeTextProperty);
        set => SetValue(TimeTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the step interval in minutes between time options.
    /// Default is 15 minutes.
    /// </summary>
    public int Step
    {
        get => (int)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>
    /// Gets the collection of available time options.
    /// </summary>
    public ObservableCollection<string> TimeOptions { get; }

    private static void OnTimeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePickerControl control && !control._isUpdating)
        {
            control.SyncComboBoxText((string)e.NewValue);
        }
    }

    private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePickerControl control)
        {
            control.GenerateTimeOptions();
        }
    }

    private void GenerateTimeOptions()
    {
        TimeOptions.Clear();
        int step = Step > 0 ? Step : DefaultStepMinutes;
        for (int totalMinutes = 0; totalMinutes < 24 * 60; totalMinutes += step)
        {
            int hour = totalMinutes / 60;
            int minute = totalMinutes % 60;
            TimeOptions.Add($"{hour:D2}:{minute:D2}");
        }
    }

    private void SyncComboBoxText(string timeText)
    {
        if (string.IsNullOrWhiteSpace(timeText))
        {
            return;
        }

        _isUpdating = true;
        try
        {
            TimeComboBox.Text = timeText;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnComboBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating)
        {
            return;
        }

        _isUpdating = true;
        try
        {
            string text = TimeComboBox.Text?.Trim() ?? string.Empty;
            TimeText = text;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        foreach (char c in e.Text)
        {
            if (!char.IsDigit(c) && c != ':')
            {
                e.Handled = true;
                return;
            }
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        string text = TimeComboBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var parts = text.Split(':');
        if (parts.Length == 2
            && int.TryParse(parts[0], out int hour)
            && int.TryParse(parts[1], out int minute))
        {
            if (hour < 0)
            {
                hour = 0;
            }
            else if (hour > 23)
            {
                hour = 23;
            }

            if (minute < 0)
            {
                minute = 0;
            }
            else if (minute > 59)
            {
                minute = 59;
            }

            string coerced = $"{hour:D2}:{minute:D2}";
            _isUpdating = true;
            try
            {
                TimeComboBox.Text = coerced;
                TimeText = coerced;
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}
