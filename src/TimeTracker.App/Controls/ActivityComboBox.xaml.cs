namespace TimeTracker.App.Controls;

using System.Collections;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Reusable combo box control for selecting an activity with color indicator.
/// </summary>
public partial class ActivityComboBox : UserControl
{
    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ActivityComboBox),
            new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="SelectedValue"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(ActivityComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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
}
