namespace Yatta.App.Views.Dialogs;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Yatta.App.Models;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Modal dialog to manage notification settings and schedule the next reminder.
/// </summary>
public partial class ReminderCustomDialogWindow : FluentWindow
{
    private readonly ILocalizationService _localizationService;
    private readonly int _defaultMinutes;
    private readonly ReminderInterval _defaultInterval;
    private readonly bool _initialNotificationsEnabled;
    private readonly bool _initialKeepNotificationsVisible;

    /// <summary>
    /// Gets the dialog result, or null if the dialog was cancelled.
    /// </summary>
    public ReminderDialogResult? Result { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReminderCustomDialogWindow"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve services.</param>
    /// <param name="defaultMinutes">The default custom reminder interval in minutes.</param>
    /// <param name="defaultInterval">The default reminder interval preset.</param>
    /// <param name="notificationsEnabled">Whether notifications are currently enabled.</param>
    /// <param name="keepNotificationsVisible">Whether notifications should remain visible until dismissed.</param>
    public ReminderCustomDialogWindow(
        IServiceProvider serviceProvider,
        int defaultMinutes,
        ReminderInterval defaultInterval,
        bool notificationsEnabled,
        bool keepNotificationsVisible)
    {
        _localizationService = serviceProvider.GetRequiredService<ILocalizationService>();
        _defaultMinutes = defaultMinutes;
        _defaultInterval = defaultInterval;
        _initialNotificationsEnabled = notificationsEnabled;
        _initialKeepNotificationsVisible = keepNotificationsVisible;
        InitializeComponent();

        // Subscribe radio button events after initialization to avoid firing
        // handlers before the visual tree is fully created.
        UntilTimeRadioButton.Checked += OnReminderOptionChanged;
        UntilTimeRadioButton.Unchecked += OnReminderOptionChanged;
        UntilSpecificTimeRadioButton.Checked += OnReminderOptionChanged;
        UntilSpecificTimeRadioButton.Unchecked += OnReminderOptionChanged;
        UntilTomorrowRadioButton.Checked += OnReminderOptionChanged;
        UntilTomorrowRadioButton.Unchecked += OnReminderOptionChanged;
    }

    /// <summary>
    /// Initializes localized text and pre-fills the inputs with the current values.
    /// </summary>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        DialogTitleText.Text = _localizationService.GetString("Dialog_CustomReminder_Title");
        DialogDescriptionText.Text = _localizationService.GetString("Dialog_CustomReminder_Description");
        PrimaryButton.Content = _localizationService.GetString("Button_Save");
        CancelButton.Content = _localizationService.GetString("Button_Cancel");
        Title = _localizationService.GetString("Dialog_CustomReminder_Title");

        NotificationsEnabledToggle.IsChecked = _initialNotificationsEnabled;
        KeepNotificationsVisibleToggle.IsChecked = _initialKeepNotificationsVisible;

        SelectDefaultInterval(_defaultInterval);
        DefaultHoursNumberBox.Value = _defaultMinutes / 60;
        DefaultMinutesNumberBox.Value = _defaultMinutes % 60;
        NextHoursNumberBox.Value = _defaultMinutes / 60;
        NextMinutesNumberBox.Value = _defaultMinutes % 60;

        // Default the specific time picker to the next hour from now.
        var nextHour = DateTime.Now.AddHours(1);
        SpecificTimePicker.TimeText = $"{nextHour.Hour:D2}:00";

        FixWindowSize();
        AttachCyclicSpin(DefaultHoursNumberBox);
        AttachCyclicSpin(DefaultMinutesNumberBox);
        AttachCyclicSpin(NextHoursNumberBox);
        AttachCyclicSpin(NextMinutesNumberBox);

        UpdateInputControlsState();
    }

    /// <summary>
    /// Locks the window to its current size so it cannot be resized by the user.
    /// </summary>
    private void FixWindowSize()
    {
        MinWidth = Width;
        MaxWidth = Width;
        MinHeight = ActualHeight;
        MaxHeight = ActualHeight;
    }

    /// <summary>
    /// Attaches cyclic spin behavior to a NumberBox so that values wrap around
    /// when the user tries to go past the minimum or maximum.
    /// </summary>
    private void AttachCyclicSpin(NumberBox numberBox)
    {
        numberBox.PreviewKeyDown += OnNumberBoxPreviewKeyDown;
        numberBox.PreviewMouseWheel += OnNumberBoxPreviewMouseWheel;

        if (numberBox.Template?.FindName("PART_InlineIncrementButton", numberBox) is RepeatButton incrementButton)
        {
            incrementButton.PreviewMouseDown += (s, e) =>
            {
                if (WrapOnSpin(numberBox, 1))
                    e.Handled = true;
            };
        }

        if (numberBox.Template?.FindName("PART_InlineDecrementButton", numberBox) is RepeatButton decrementButton)
        {
            decrementButton.PreviewMouseDown += (s, e) =>
            {
                if (WrapOnSpin(numberBox, -1))
                    e.Handled = true;
            };
        }
    }

    /// <summary>
    /// Wraps the NumberBox value when spinning past its limits.
    /// </summary>
    /// <returns>True if the value was wrapped; otherwise false.</returns>
    private static bool WrapOnSpin(NumberBox numberBox, int direction)
    {
        var currentValue = numberBox.Value ?? numberBox.Minimum;

        if (direction > 0 && Math.Abs(currentValue - numberBox.Maximum) < 0.001)
        {
            numberBox.Value = numberBox.Minimum;
            return true;
        }

        if (direction < 0 && Math.Abs(currentValue - numberBox.Minimum) < 0.001)
        {
            numberBox.Value = numberBox.Maximum;
            return true;
        }

        return false;
    }

    private void OnNumberBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not NumberBox numberBox)
            return;

        var direction = e.Key switch
        {
            Key.Up => 1,
            Key.Down => -1,
            _ => 0
        };

        if (direction != 0 && WrapOnSpin(numberBox, direction))
            e.Handled = true;
    }

    private void OnNumberBoxPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not NumberBox numberBox)
            return;

        var direction = e.Delta > 0 ? 1 : (e.Delta < 0 ? -1 : 0);
        if (direction != 0 && WrapOnSpin(numberBox, direction))
            e.Handled = true;
    }

    /// <summary>
    /// Enables or disables the reminder time section based on the notifications toggle.
    /// </summary>
    private void OnNotificationsEnabledChanged(object sender, RoutedEventArgs e)
    {
        UpdateInputControlsState();
        ErrorText.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Enables or disables the time input controls based on the selected radio option
    /// and the notifications toggle state.
    /// </summary>
    private void OnReminderOptionChanged(object sender, RoutedEventArgs e)
    {
        UpdateInputControlsState();
        ErrorText.Visibility = Visibility.Collapsed;
    }

    private void UpdateInputControlsState()
    {
        if (UntilTimeRadioButton == null || TimeInputPanel == null || AdjustButtonsPanel == null
            || UntilSpecificTimeRadioButton == null || SpecificTimePanel == null
            || ReminderOptionsPanel == null || KeepNotificationsVisibleToggle == null
            || DefaultIntervalComboBox == null || DefaultCustomIntervalPanel == null
            || UntilTimeOptionBorder == null || SpecificTimeOptionBorder == null || UntilTomorrowOptionBorder == null)
            return;

        bool notificationsEnabled = NotificationsEnabledToggle.IsChecked == true;
        KeepNotificationsVisibleToggle.IsEnabled = notificationsEnabled;
        DefaultIntervalComboBox.IsEnabled = notificationsEnabled;
        DefaultCustomIntervalPanel.IsEnabled = notificationsEnabled && GetSelectedDefaultInterval() == ReminderInterval.Custom;
        DefaultCustomIntervalPanel.Visibility = GetSelectedDefaultInterval() == ReminderInterval.Custom
            ? Visibility.Visible
            : Visibility.Collapsed;
        ReminderOptionsPanel.IsEnabled = notificationsEnabled;

        if (!notificationsEnabled)
        {
            UpdateReminderOptionVisualState();
            return;
        }

        var isTimeSelected = UntilTimeRadioButton.IsChecked == true;
        var isSpecificTimeSelected = UntilSpecificTimeRadioButton.IsChecked == true;
        TimeInputPanel.IsEnabled = isTimeSelected;
        AdjustButtonsPanel.IsEnabled = isTimeSelected;
        SpecificTimePanel.IsEnabled = isSpecificTimeSelected;
        SpecificTimePanel.Visibility = isSpecificTimeSelected ? Visibility.Visible : Visibility.Collapsed;

        UpdateReminderOptionVisualState();
    }

    private void OnUntilTimeOptionClick(object sender, MouseButtonEventArgs e)
    {
        UntilTimeRadioButton.IsChecked = true;
    }

    private void OnSpecificTimeOptionClick(object sender, MouseButtonEventArgs e)
    {
        UntilSpecificTimeRadioButton.IsChecked = true;
    }

    private void OnUntilTomorrowOptionClick(object sender, MouseButtonEventArgs e)
    {
        UntilTomorrowRadioButton.IsChecked = true;
    }

    private void UpdateReminderOptionVisualState()
    {
        SetOptionVisualState(UntilTimeOptionBorder, UntilTimeRadioButton.IsChecked == true);
        SetOptionVisualState(SpecificTimeOptionBorder, UntilSpecificTimeRadioButton.IsChecked == true);
        SetOptionVisualState(UntilTomorrowOptionBorder, UntilTomorrowRadioButton.IsChecked == true);
    }

    private void SetOptionVisualState(Border border, bool isSelected)
    {
        border.BorderBrush = GetBrush(isSelected
            ? "SystemAccentColorPrimaryBrush"
            : "ControlStrokeColorDefaultBrush");
        border.Background = GetBrush(isSelected
            ? "ControlFillColorSecondaryBrush"
            : "SubtleFillColorTransparentBrush");
    }

    private Brush GetBrush(string resourceKey)
    {
        return TryFindResource(resourceKey) as Brush ?? Brushes.Transparent;
    }

    private void OnDefaultIntervalSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateInputControlsState();
        ErrorText.Visibility = Visibility.Collapsed;
    }

    private void SelectDefaultInterval(ReminderInterval interval)
    {
        foreach (var item in DefaultIntervalComboBox.Items.OfType<ComboBoxItem>())
        {
            if (Enum.TryParse(item.Tag?.ToString(), out ReminderInterval itemInterval)
                && itemInterval == interval)
            {
                DefaultIntervalComboBox.SelectedItem = item;
                return;
            }
        }

        DefaultIntervalComboBox.SelectedIndex = 4;
    }

    private ReminderInterval GetSelectedDefaultInterval()
    {
        if (DefaultIntervalComboBox.SelectedItem is ComboBoxItem item
            && Enum.TryParse(item.Tag?.ToString(), out ReminderInterval interval))
        {
            return interval;
        }

        return ReminderInterval.Hours2;
    }

    /// <summary>
    /// Adjusts the current time value by the given number of minutes.
    /// </summary>
    private void AdjustMinutes(int minutesDelta)
    {
        var hours = (int)(NextHoursNumberBox.Value ?? 0);
        var minutes = (int)(NextMinutesNumberBox.Value ?? 0);
        var totalMinutes = (hours * 60) + minutes + minutesDelta;

        if (totalMinutes < 1)
            totalMinutes = 1;
        if (totalMinutes > 1440)
            totalMinutes = 1440;

        NextHoursNumberBox.Value = totalMinutes / 60;
        NextMinutesNumberBox.Value = totalMinutes % 60;
        ErrorText.Visibility = Visibility.Collapsed;
    }

    private void OnMinus15mClick(object sender, RoutedEventArgs e) => AdjustMinutes(-15);

    private void OnMinus1hClick(object sender, RoutedEventArgs e) => AdjustMinutes(-60);

    private void OnPlus1hClick(object sender, RoutedEventArgs e) => AdjustMinutes(60);

    private void OnPlus15mClick(object sender, RoutedEventArgs e) => AdjustMinutes(15);

    /// <summary>
    /// Handles the accept button click. Validates and stores the result.
    /// </summary>
    private void OnPrimaryClick(object sender, RoutedEventArgs e)
    {
        bool notificationsEnabled = NotificationsEnabledToggle.IsChecked == true;
        bool keepVisible = KeepNotificationsVisibleToggle.IsChecked == true;
        ReminderInterval defaultInterval = GetSelectedDefaultInterval();
        var defaultMinutes = GetDefaultReminderMinutes(defaultInterval);

        if (!defaultMinutes.HasValue)
        {
            return;
        }

        // When notifications are disabled, ignore the time selection.
        if (!notificationsEnabled)
        {
            Result = new ReminderDialogResult
            {
                NotificationsEnabled = false,
                KeepNotificationsVisible = keepVisible,
                DefaultReminderInterval = defaultInterval,
                DefaultReminderMinutes = defaultMinutes.Value,
                CustomMinutes = null
            };
            Close();
            return;
        }

        int? customMinutes = null;

        if (UntilSpecificTimeRadioButton.IsChecked == true)
        {
            var timeText = SpecificTimePicker.TimeText?.Trim() ?? string.Empty;
            var parts = timeText.Split(':');
            if (parts.Length != 2
                || !int.TryParse(parts[0], out int hour)
                || !int.TryParse(parts[1], out int minute)
                || hour < 0 || hour > 23
                || minute < 0 || minute > 59)
            {
                ErrorText.Text = _localizationService.GetString("Validation_InvalidTimeFormat");
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            var now = DateTime.Now;
            var target = now.Date.Add(new TimeOnly(hour, minute).ToTimeSpan());
            if (target <= now)
                target = target.AddDays(1);

            var totalMinutes = (int)Math.Ceiling((target - now).TotalMinutes);
            if (totalMinutes < 1) totalMinutes = 1;
            if (totalMinutes > 1440) totalMinutes = 1440;

            customMinutes = totalMinutes;
        }
        else if (UntilTomorrowRadioButton.IsChecked == true)
        {
            var remaining = DateTime.Today.AddDays(1) - DateTime.Now;
            var totalMinutes = (int)remaining.TotalMinutes;
            if (totalMinutes < 1) totalMinutes = 1;
            if (totalMinutes > 1440) totalMinutes = 1440;

            customMinutes = totalMinutes;
        }
        else
        {
            var hours = (int)(NextHoursNumberBox.Value ?? 0);
            var minutes = (int)(NextMinutesNumberBox.Value ?? 0);
            var total = (hours * 60) + minutes;

            if (total < 1 || total > 1440)
            {
                ErrorText.Text = _localizationService.GetString("Validation_CustomReminderRange");
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            customMinutes = total;
        }

        Result = new ReminderDialogResult
        {
            NotificationsEnabled = true,
            KeepNotificationsVisible = keepVisible,
            DefaultReminderInterval = defaultInterval,
            DefaultReminderMinutes = defaultMinutes.Value,
            CustomMinutes = customMinutes
        };
        Close();
    }

    private int? GetDefaultReminderMinutes(ReminderInterval defaultInterval)
    {
        var presetMinutes = defaultInterval switch
        {
            ReminderInterval.Minutes15 => 15,
            ReminderInterval.Minutes10 => 10,
            ReminderInterval.Minutes30 => 30,
            ReminderInterval.Hour1 => 60,
            ReminderInterval.Hours2 => 120,
            _ => (int?)null
        };

        if (presetMinutes.HasValue)
            return presetMinutes.Value;

        var hours = (int)(DefaultHoursNumberBox.Value ?? 0);
        var minutes = (int)(DefaultMinutesNumberBox.Value ?? 0);
        var total = (hours * 60) + minutes;

        if (total < 1 || total > 1440)
        {
            ErrorText.Text = _localizationService.GetString("Validation_CustomReminderRange");
            ErrorText.Visibility = Visibility.Visible;
            return null;
        }

        return total;
    }

    /// <summary>
    /// Handles the cancel button click.
    /// </summary>
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Result = null;
        Close();
    }
}
