namespace TimeTracker.App.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;
using AppResources = TimeTracker.App.Resources.Resources;

/// <summary>
/// Reusable card control for displaying a time record.
/// </summary>
public partial class TimeRecordCard : UserControl
{
    private const int MaxCollapsedNotesLength = 120;
    private bool _isNotesExpanded;

    public TimeRecordCard()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public static readonly DependencyProperty ActivityColorProperty =
        DependencyProperty.Register(nameof(ActivityColor), typeof(string), typeof(TimeRecordCard), new PropertyMetadata("#808080"));

    public static readonly DependencyProperty ActivityNameProperty =
        DependencyProperty.Register(nameof(ActivityName), typeof(string), typeof(TimeRecordCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty StartTimeProperty =
        DependencyProperty.Register(nameof(StartTime), typeof(string), typeof(TimeRecordCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty EndTimeProperty =
        DependencyProperty.Register(nameof(EndTime), typeof(string), typeof(TimeRecordCard), new PropertyMetadata("--:--"));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(string), typeof(TimeRecordCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty NotesProperty =
        DependencyProperty.Register(nameof(Notes), typeof(string), typeof(TimeRecordCard), new PropertyMetadata(string.Empty, OnNotesChanged));

    public static readonly DependencyProperty IsTeleworkProperty =
        DependencyProperty.Register(nameof(IsTelework), typeof(bool), typeof(TimeRecordCard), new PropertyMetadata(false, OnIsTeleworkChanged));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(TimeRecordCard), new PropertyMetadata(false));

    public static readonly DependencyProperty EditCommandProperty =
        DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(TimeRecordCard), new PropertyMetadata(null));

    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(TimeRecordCard), new PropertyMetadata(null));

    public static readonly DependencyProperty CardClickCommandProperty =
        DependencyProperty.Register(nameof(CardClickCommand), typeof(ICommand), typeof(TimeRecordCard), new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(TimeRecordCard), new PropertyMetadata(null));

    public string ActivityColor
    {
        get => (string)GetValue(ActivityColorProperty);
        set => SetValue(ActivityColorProperty, value);
    }

    public string ActivityName
    {
        get => (string)GetValue(ActivityNameProperty);
        set => SetValue(ActivityNameProperty, value);
    }

    public string StartTime
    {
        get => (string)GetValue(StartTimeProperty);
        set => SetValue(StartTimeProperty, value);
    }

    public string EndTime
    {
        get => (string)GetValue(EndTimeProperty);
        set => SetValue(EndTimeProperty, value);
    }

    public string Duration
    {
        get => (string)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public string Notes
    {
        get => (string)GetValue(NotesProperty);
        set => SetValue(NotesProperty, value);
    }

    public bool IsTelework
    {
        get => (bool)GetValue(IsTeleworkProperty);
        set => SetValue(IsTeleworkProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public ICommand? EditCommand
    {
        get => (ICommand?)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public ICommand? CardClickCommand
    {
        get => (ICommand?)GetValue(CardClickCommandProperty);
        set => SetValue(CardClickCommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private static void OnNotesChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is TimeRecordCard control)
        {
            control._isNotesExpanded = false;
            control.UpdateNotesPresentation();
        }
    }

    private static void OnIsTeleworkChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is TimeRecordCard control)
        {
            control.UpdateLocationIcon();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateLocationIcon();
        UpdateNotesPresentation();
    }

    private void UpdateLocationIcon()
    {
        LocationIcon.Symbol = IsTelework ? SymbolRegular.Home24 : SymbolRegular.Building24;
        LocationIcon.ToolTip = IsTelework ? AppResources.Location_Telework : AppResources.Location_Office;
    }

    private void UpdateNotesPresentation()
    {
        if (string.IsNullOrWhiteSpace(Notes))
        {
            NotesTextBlock.Visibility = Visibility.Collapsed;
            ToggleNotesButton.Visibility = Visibility.Collapsed;
            return;
        }

        NotesTextBlock.Visibility = Visibility.Visible;

        if (Notes.Length <= MaxCollapsedNotesLength)
        {
            NotesTextBlock.Text = Notes;
            ToggleNotesButton.Visibility = Visibility.Collapsed;
            return;
        }

        ToggleNotesButton.Visibility = Visibility.Visible;
        ToggleNotesIcon.Symbol = _isNotesExpanded ? SymbolRegular.ChevronUp24 : SymbolRegular.ChevronDown24;
        NotesTextBlock.Text = _isNotesExpanded
            ? Notes
            : $"{Notes[..MaxCollapsedNotesLength].TrimEnd()}…";
    }

    private void ToggleNotesButton_Click(object sender, RoutedEventArgs e)
    {
        _isNotesExpanded = !_isNotesExpanded;
        UpdateNotesPresentation();
    }

    private void RecordMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
    }

    private void EditMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (EditCommand?.CanExecute(CommandParameter) == true)
        {
            EditCommand.Execute(CommandParameter);
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (DeleteCommand?.CanExecute(CommandParameter) == true)
        {
            DeleteCommand.Execute(CommandParameter);
        }
    }
}
