namespace TimeTracker.App.Behaviors;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

/// <summary>
/// Behavior that allows propagating the scroll event to the parent ScrollViewer
/// when a child element captures the mouse wheel event.
/// </summary>
public static class ScrollParentBehavior
{
    /// <summary>
    /// Multiplication factor for scrolling. Adjusts the scroll speed.
    /// </summary>
    private const double ScrollFactor = 0.5;

    /// <summary>
    /// Attached property to enable scroll propagation to parent.
    /// </summary>
    public static readonly DependencyProperty BubbleScrollProperty =
        DependencyProperty.RegisterAttached(
            "BubbleScroll",
            typeof(bool),
            typeof(ScrollParentBehavior),
            new PropertyMetadata(false, OnBubbleScrollChanged));

    /// <summary>
    /// Gets the BubbleScroll value for the specified element.
    /// </summary>
    public static bool GetBubbleScroll(DependencyObject obj)
    {
        return (bool)obj.GetValue(BubbleScrollProperty);
    }

    /// <summary>
    /// Sets the BubbleScroll value for the specified element.
    /// </summary>
    public static void SetBubbleScroll(DependencyObject obj, bool value)
    {
        obj.SetValue(BubbleScrollProperty, value);
    }

    private static void OnBubbleScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.PreviewMouseWheel += OnPreviewMouseWheel;
            }
            else
            {
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
            }
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not UIElement element)
            return;

        // Find the parent ScrollViewer in the visual tree
        var scrollViewer = FindParentScrollViewer(element);
        if (scrollViewer == null)
            return;

        // Mark the event as handled to prevent other elements from processing it
        e.Handled = true;

        // Calculate the scroll amount
        // e.Delta is typically 120 or -120 per wheel "notch"
        // Positive value = scroll up, negative = scroll down
        double newOffset = scrollViewer.VerticalOffset - (e.Delta * ScrollFactor);
        
        // Ensure the new offset is within bounds
        newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableHeight));
        
        scrollViewer.ScrollToVerticalOffset(newOffset);
    }

    /// <summary>
    /// Searches for the first parent ScrollViewer in the visual tree.
    /// </summary>
    /// <param name="element">The element from which to start the search.</param>
    /// <returns>The parent ScrollViewer or null if not found.</returns>
    private static ScrollViewer? FindParentScrollViewer(DependencyObject element)
    {
        var parent = VisualTreeHelper.GetParent(element);

        while (parent != null)
        {
            if (parent is ScrollViewer scrollViewer)
                return scrollViewer;

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }
}
