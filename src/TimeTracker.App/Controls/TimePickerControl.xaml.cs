namespace TimeTracker.App.Controls;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

/// <summary>
/// Reusable time picker control that allows selecting hours (0-23) and minutes (0-59)
/// via dropdowns or manual entry. Validates input and coerces out-of-range values.
/// </summary>
public partial class TimePickerControl : UserControl
{
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
    /// Initializes a new instance of the <see cref="TimePickerControl"/> class.
    /// </summary>
    public TimePickerControl()
    {
        Hours = new ObservableCollection<string>();
        Minutes = new ObservableCollection<string>();

        for (int i = 0; i < 24; i++)
        {
            Hours.Add(i.ToString("D2"));
        }

        for (int i = 0; i < 60; i++)
        {
            Minutes.Add(i.ToString("D2"));
        }

        InitializeComponent();

        HourComboBox.AddHandler(
            TextBoxBase.TextChangedEvent,
            new TextChangedEventHandler(OnComboBoxTextChanged));
        MinuteComboBox.AddHandler(
            TextBoxBase.TextChangedEvent,
            new TextChangedEventHandler(OnComboBoxTextChanged));

        HourComboBox.PreviewTextInput += OnPreviewTextInput;
        MinuteComboBox.PreviewTextInput += OnPreviewTextInput;

        HourComboBox.LostFocus += OnHourLostFocus;
        MinuteComboBox.LostFocus += OnMinuteLostFocus;
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
    /// Gets the collection of available hours (00-23).
    /// </summary>
    public ObservableCollection<string> Hours { get; }

    /// <summary>
    /// Gets the collection of available minutes (00-59).
    /// </summary>
    public ObservableCollection<string> Minutes { get; }

    private static void OnTimeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePickerControl control && !control._isUpdating)
        {
            control.ParseTimeText((string)e.NewValue);
        }
    }

    private void ParseTimeText(string timeText)
    {
        if (string.IsNullOrWhiteSpace(timeText))
        {
            return;
        }

        _isUpdating = true;
        try
        {
            var parts = timeText.Split(':');
            if (parts.Length == 2
                && int.TryParse(parts[0], out int hour) && hour >= 0 && hour <= 23
                && int.TryParse(parts[1], out int minute) && minute >= 0 && minute <= 59)
            {
                HourComboBox.Text = hour.ToString("D2");
                MinuteComboBox.Text = minute.ToString("D2");
            }
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

        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        _isUpdating = true;
        try
        {
            string hourText = HourComboBox.Text?.Trim() ?? string.Empty;
            string minuteText = MinuteComboBox.Text?.Trim() ?? string.Empty;

            if (int.TryParse(hourText, out int hour) && hour >= 0 && hour <= 23
                && int.TryParse(minuteText, out int minute) && minute >= 0 && minute <= 59)
            {
                TimeText = $"{hour:D2}:{minute:D2}";
            }
            else
            {
                TimeText = $"{hourText}:{minuteText}";
            }
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
            if (!char.IsDigit(c))
            {
                e.Handled = true;
                return;
            }
        }
    }

    private void OnHourLostFocus(object sender, RoutedEventArgs e)
    {
        CoerceComboBoxValue(HourComboBox, 0, 23);
    }

    private void OnMinuteLostFocus(object sender, RoutedEventArgs e)
    {
        CoerceComboBoxValue(MinuteComboBox, 0, 59);
    }

    private void CoerceComboBoxValue(ComboBox comboBox, int min, int max)
    {
        string text = comboBox.Text?.Trim() ?? string.Empty;
        if (int.TryParse(text, out int value))
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            comboBox.Text = value.ToString("D2");
        }
        else if (!string.IsNullOrEmpty(text))
        {
            comboBox.Text = min.ToString("D2");
        }
    }
}
