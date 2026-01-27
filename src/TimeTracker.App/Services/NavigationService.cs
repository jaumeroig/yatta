namespace TimeTracker.App.Services;

using System.Windows.Controls;
using Wpf.Ui.Controls;

/// <summary>
/// Interface for the navigation service between pages.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to a specific page.
    /// </summary>
    /// <typeparam name="T">Page type.</typeparam>
    void Navigate<T>() where T : Page;

    /// <summary>
    /// Navigates to a specific page with parameters.
    /// </summary>
    /// <typeparam name="T">Page type.</typeparam>
    /// <param name="parameter">Parameter to pass to the page.</param>
    void Navigate<T>(object? parameter) where T : Page;

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Indicates if navigation back is possible.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets or sets the current navigation parameter.
    /// </summary>
    object? CurrentParameter { get; }

    /// <summary>
    /// Sets the NavigationView to manage navigation.
    /// </summary>
    void SetNavigationView(NavigationView navigationView);
}

/// <summary>
/// Implementation of the navigation service.
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

        // Save the current page to the stack if it exists
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
        // Get the current page type from the NavigationView
        if (_navigationView?.SelectedItem is NavigationViewItem item && item.TargetPageType != null)
        {
            return item.TargetPageType;
        }

        return null;
    }
}
