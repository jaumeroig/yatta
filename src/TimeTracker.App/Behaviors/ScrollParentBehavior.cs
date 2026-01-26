namespace TimeTracker.App.Behaviors;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

/// <summary>
/// Comportament que permet propagar l'event de scroll cap al ScrollViewer pare
/// quan un element fill captura l'event de la roda del ratolí.
/// </summary>
public static class ScrollParentBehavior
{
    /// <summary>
    /// Factor de multiplicació per al scroll. Ajusta la velocitat de l'scroll.
    /// </summary>
    private const double ScrollFactor = 0.5;

    /// <summary>
    /// Propietat adjunta per habilitar la propagació de l'scroll cap al pare.
    /// </summary>
    public static readonly DependencyProperty BubbleScrollProperty =
        DependencyProperty.RegisterAttached(
            "BubbleScroll",
            typeof(bool),
            typeof(ScrollParentBehavior),
            new PropertyMetadata(false, OnBubbleScrollChanged));

    /// <summary>
    /// Obté el valor de BubbleScroll per a l'element especificat.
    /// </summary>
    public static bool GetBubbleScroll(DependencyObject obj)
    {
        return (bool)obj.GetValue(BubbleScrollProperty);
    }

    /// <summary>
    /// Estableix el valor de BubbleScroll per a l'element especificat.
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

        // Buscar el ScrollViewer pare a l'arbre visual
        var scrollViewer = FindParentScrollViewer(element);
        if (scrollViewer == null)
            return;

        // Marcar l'event com a gestionat per evitar que altres elements el processin
        e.Handled = true;

        // Calcular la quantitat de scroll
        // e.Delta és típicament 120 o -120 per cada "notch" de la roda
        // Valor positiu = scroll amunt, negatiu = scroll avall
        double newOffset = scrollViewer.VerticalOffset - (e.Delta * ScrollFactor);
        
        // Assegurar que el nou offset està dins dels límits
        newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableHeight));
        
        scrollViewer.ScrollToVerticalOffset(newOffset);
    }

    /// <summary>
    /// Cerca el primer ScrollViewer pare a l'arbre visual.
    /// </summary>
    /// <param name="element">L'element des del qual començar la cerca.</param>
    /// <returns>El ScrollViewer pare o null si no es troba.</returns>
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
