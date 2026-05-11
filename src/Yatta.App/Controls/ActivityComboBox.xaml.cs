namespace Yatta.App.Controls;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Wpf.Ui.Controls;

using Yatta.Core.Models;

/// <summary>
/// Autocomplete control for selecting an activity by searching Name or JiraCode.
/// </summary>
public partial class ActivityComboBox : UserControl
{
    private List<Activity> _allActivities = new();
    private bool _isUpdatingText;

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ActivityComboBox),
            new PropertyMetadata(null, OnItemsSourceChanged));

    /// <summary>
    /// Identifies the <see cref="SelectedValue"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(ActivityComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityComboBox"/> class.
    /// </summary>
    public ActivityComboBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the collection of activities to display.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected activity identifier.
    /// </summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActivityComboBox control)
        {
            if (e.NewValue is IEnumerable items)
            {
                control._allActivities = items.Cast<Activity>().ToList();
            }
            else
            {
                control._allActivities = new List<Activity>();
            }

            control.SuggestBox.ItemsSource = control._allActivities;
            control.UpdateTextFromSelectedValue();
        }
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActivityComboBox control)
        {
            control.UpdateTextFromSelectedValue();
        }
    }

    private void UpdateTextFromSelectedValue()
    {
        if (SelectedValue is Guid selectedId && selectedId != Guid.Empty)
        {
            Activity? activity = _allActivities.FirstOrDefault(a => a.Id == selectedId);
            if (activity != null)
            {
                _isUpdatingText = true;
                SuggestBox.Text = GetDisplayText(activity);
                _isUpdatingText = false;
                return;
            }
        }

        _isUpdatingText = true;
        SuggestBox.Text = string.Empty;
        _isUpdatingText = false;
    }

    private static string GetDisplayText(Activity activity)
    {
        if (!string.IsNullOrWhiteSpace(activity.JiraCode))
        {
            return $"{activity.Name} ({activity.JiraCode})";
        }

        return activity.Name;
    }

    private void SuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (_isUpdatingText)
        {
            return;
        }

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            string searchText = sender.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(searchText))
            {
                sender.ItemsSource = _allActivities;
            }
            else
            {
                List<Activity> filtered = _allActivities
                    .Where(a =>
                        a.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(a.JiraCode) && a.JiraCode.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                sender.ItemsSource = filtered;
            }
        }
    }

    private void SuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is Activity activity)
        {
            _isUpdatingText = true;
            SelectedValue = activity.Id;
            sender.Text = GetDisplayText(activity);
            _isUpdatingText = false;
        }
    }

    private void SuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        // Try to find an activity matching the submitted text
        string queryText = args.QueryText?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(queryText))
        {
            Activity? match = _allActivities.FirstOrDefault(a =>
                a.Name.Equals(queryText, StringComparison.OrdinalIgnoreCase) ||
                GetDisplayText(a).Equals(queryText, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                _isUpdatingText = true;
                SelectedValue = match.Id;
                sender.Text = GetDisplayText(match);
                _isUpdatingText = false;
                return;
            }
        }

        // No match found — revert to the previously selected value
        UpdateTextFromSelectedValue();
    }

    private void SuggestBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is AutoSuggestBox suggestBox)
        {
            suggestBox.ItemsSource = _allActivities;
            suggestBox.IsSuggestionListOpen = true;
        }
    }

    private void SuggestBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Delay to allow SuggestionChosen to fire first when clicking a dropdown item
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!SuggestBox.IsKeyboardFocusWithin)
            {
                UpdateTextFromSelectedValue();
            }
        }), DispatcherPriority.Background);
    }
}
