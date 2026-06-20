namespace Yatta.App.Views.Dialogs;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Yatta.Core.Interfaces;
using AppResources = Yatta.App.Resources.Resources;

/// <summary>
/// Modal dialog to collect a custom reminder interval in hours and minutes,
/// or to snooze until midnight (tomorrow at 00:00).
/// </summary>
public partial class ReminderCustomDialogWindow : FluentWindow
{
    private readonly ILocalizationService _localizationService;
    private readonly int _defaultMinutes;

    /// <summary>
    /// The custom minutes entered by the user, or null if cancelled.
    /// When the "until tomorrow" option is selected, this is the number of
    /// minutes from now until midnight.
    /// </summary>
    public int? CustomMinutes { get; private set; }

    public ReminderCustomDialogWindow(IServiceProvider serviceProvider, int defaultMinutes)
    {
        _localizationService = serviceProvider.GetRequiredService<ILocalizationService>();
        _defaultMinutes = defaultMinutes;
        InitializeComponent();

        // Subscribe radio button events after initialization to avoid firing
        // handlers before the visual tree is fully created.
        UntilTimeRadioButton.Checked += OnReminderOptionChanged;
        UntilTimeRadioButton.Unchecked += OnReminderOptionChanged;
        UntilTomorrowRadioButton.Checked += OnReminderOptionChanged;
        UntilTomorrowRadioButton.Unchecked += OnReminderOptionChanged;
    }

    /// <summary>
    /// Initializes localized text and pre-fills the inputs with the current custom interval.
    /// </summary>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        DialogTitleText.Text = _localizationService.GetString("Dialog_CustomReminder_Title");
        DialogDescriptionText.Text = _localizationService.GetString("Dialog_CustomReminder_Description");
        PrimaryButton.Content = _localizationService.GetString("Button_Save");
        CancelButton.Content = _localizationService.GetString("Button_Cancel");
        Title = _localizationService.GetString("Dialog_CustomReminder_Title");

        HoursNumberBox.Value = _defaultMinutes / 60;
        MinutesNumberBox.Value = _defaultMinutes % 60;

        FixWindowSize();
        AttachCyclicSpin(HoursNumberBox);
        AttachCyclicSpin(MinutesNumberBox);

        UpdateInputControlsState();
        HoursNumberBox.Focus();
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
    /// Enables or disables the time input controls based on the selected radio option.
    /// </summary>
    private void OnReminderOptionChanged(object sender, RoutedEventArgs e)
    {
        UpdateInputControlsState();
        ErrorText.Visibility = Visibility.Collapsed;
    }

    private void UpdateInputControlsState()
    {
        if (UntilTimeRadioButton == null || TimeInputPanel == null || AdjustButtonsPanel == null)
            return;

        var isTimeSelected = UntilTimeRadioButton.IsChecked == true;
        TimeInputPanel.IsEnabled = isTimeSelected;
        AdjustButtonsPanel.IsEnabled = isTimeSelected;
    }

    /// <summary>
    /// Adjusts the current time value by the given number of minutes.
    /// </summary>
    private void AdjustMinutes(int minutesDelta)
    {
        var hours = (int)(HoursNumberBox.Value ?? 0);
        var minutes = (int)(MinutesNumberBox.Value ?? 0);
        var totalMinutes = (hours * 60) + minutes + minutesDelta;

        if (totalMinutes < 1)
            totalMinutes = 1;
        if (totalMinutes > 1440)
            totalMinutes = 1440;

        HoursNumberBox.Value = totalMinutes / 60;
        MinutesNumberBox.Value = totalMinutes % 60;
        ErrorText.Visibility = Visibility.Collapsed;
    }

    private void OnMinus15mClick(object sender, RoutedEventArgs e) => AdjustMinutes(-15);

    private void OnMinus1hClick(object sender, RoutedEventArgs e) => AdjustMinutes(-60);

    private void OnPlus1hClick(object sender, RoutedEventArgs e) => AdjustMinutes(60);

    private void OnPlus15mClick(object sender, RoutedEventArgs e) => AdjustMinutes(15);

    /// <summary>
    /// Handles the accept button click. Validates and stores the value.
    /// </summary>
    private void OnPrimaryClick(object sender, RoutedEventArgs e)
    {
        if (UntilTomorrowRadioButton.IsChecked == true)
        {
            var remaining = DateTime.Today.AddDays(1) - DateTime.Now;
            var totalMinutes = (int)remaining.TotalMinutes;
            if (totalMinutes < 1) totalMinutes = 1;
            if (totalMinutes > 1440) totalMinutes = 1440;

            CustomMinutes = totalMinutes;
            Close();
            return;
        }

        var hours = (int)(HoursNumberBox.Value ?? 0);
        var minutes = (int)(MinutesNumberBox.Value ?? 0);
        var total = (hours * 60) + minutes;

        if (total < 1 || total > 1440)
        {
            ErrorText.Text = _localizationService.GetString("Validation_CustomReminderRange");
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        CustomMinutes = total;
        Close();
    }

    /// <summary>
    /// Handles the cancel button click.
    /// </summary>
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        CustomMinutes = null;
        Close();
    }
}
