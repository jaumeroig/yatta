using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace TimeTracker.App.Services;

/// <summary>
/// Interfície per al servei de navegació entre pàgines.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navega a una pàgina específica.
    /// </summary>
    /// <typeparam name="T">Tipus de la pàgina.</typeparam>
    void Navigate<T>() where T : Page;

    /// <summary>
    /// Navega a una pàgina específica amb paràmetres.
    /// </summary>
    /// <typeparam name="T">Tipus de la pàgina.</typeparam>
    /// <param name="parameter">Paràmetre a passar a la pàgina.</param>
    void Navigate<T>(object? parameter) where T : Page;

    /// <summary>
    /// Navega enrere a la pàgina anterior.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Indica si es pot navegar enrere.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Obté o estableix el paràmetre de navegació actual.
    /// </summary>
    object? CurrentParameter { get; }

    /// <summary>
    /// Estableix el NavigationView per gestionar la navegació.
    /// </summary>
    void SetNavigationView(NavigationView navigationView);
}

/// <summary>
/// Implementació del servei de navegació.
/// </summary>
public class NavigationService : INavigationService
{
    private NavigationView? _navigationView;
    private readonly Stack<Type> _navigationStack = new();

    /// <inheritdoc/>
    public object? CurrentParameter { get; private set; }

    /// <inheritdoc/>
    public bool CanGoBack => _navigationStack.Count > 0;

    /// <inheritdoc/>
    public void SetNavigationView(NavigationView navigationView)
    {
        _navigationView = navigationView;
    }

    /// <inheritdoc/>
    public void Navigate<T>() where T : Page
    {
        Navigate<T>(null);
    }

    /// <inheritdoc/>
    public void Navigate<T>(object? parameter) where T : Page
    {
        if (_navigationView == null) return;

        // Guardar la pàgina actual a la pila si existeix
        var currentPage = GetCurrentPageType();
        if (currentPage != null && currentPage != typeof(T))
        {
            _navigationStack.Push(currentPage);
        }

        CurrentParameter = parameter;
        _navigationView.Navigate(typeof(T));
    }

    /// <inheritdoc/>
    public void GoBack()
    {
        if (_navigationView == null || !CanGoBack) return;

        var previousPage = _navigationStack.Pop();
        CurrentParameter = null;
        _navigationView.Navigate(previousPage);
    }

    private Type? GetCurrentPageType()
    {
        // Obtenir el tipus de la pàgina actual del NavigationView
        if (_navigationView?.SelectedItem is NavigationViewItem item && item.TargetPageType != null)
        {
            return item.TargetPageType;
        }

        return null;
    }
}
