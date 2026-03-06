namespace Yatta.App.Services;

using System.Collections.ObjectModel;
using Wpf.Ui.Controls;


/// <summary>
/// Represents a breadcrumb item for navigation.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Text to display in the breadcrumb.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Action to execute when the item is clicked.
    /// </summary>
    public Action? ClickAction { get; set; }

    /// <summary>
    /// Creates a new breadcrumb item.
    /// </summary>
    /// <param name="label">Text to display.</param>
    /// <param name="clickAction">Optional action when clicked.</param>
    public BreadcrumbItem(string label, Action? clickAction = null)
    {
        Label = label;
        ClickAction = clickAction;
    }

    /// <inheritdoc/>
    public override string ToString() => Label;
}

/// <summary>
/// Interface for the breadcrumb management service.
/// </summary>
public interface IBreadcrumbService
{
    /// <summary>
    /// Collection of breadcrumb items (strings to display).
    /// </summary>
    ObservableCollection<string> Items { get; }

    /// <summary>
    /// Sets the BreadcrumbBar to manage.
    /// </summary>
    /// <param name="breadcrumbBar">The BreadcrumbBar control.</param>
    void SetBreadcrumbBar(BreadcrumbBar breadcrumbBar);

    /// <summary>
    /// Sets a single item in the breadcrumb (for main pages).
    /// </summary>
    /// <param name="pageTitle">Page title.</param>
    void SetItems(string pageTitle);

    /// <summary>
    /// Sets the breadcrumb items.
    /// </summary>
    /// <param name="items">Items to display.</param>
    void SetItems(params BreadcrumbItem[] items);

    /// <summary>
    /// Clears the breadcrumb (hides it or shows only the root item).
    /// </summary>
    void Clear();

    /// <summary>
    /// Handles clicks on a breadcrumb item.
    /// </summary>
    /// <param name="index">Index of the clicked item.</param>
    void HandleItemClicked(int index);
}

/// <summary>
/// Implementation of the breadcrumb management service.
/// </summary>
public class BreadcrumbService : IBreadcrumbService
{
    private BreadcrumbBar? _breadcrumbBar;
    private readonly List<Action?> _clickActions = new();

    /// <inheritdoc/>
    public ObservableCollection<string> Items { get; } = new();

    /// <inheritdoc/>
    public void SetBreadcrumbBar(BreadcrumbBar breadcrumbBar)
    {
        _breadcrumbBar = breadcrumbBar;
        _breadcrumbBar.ItemsSource = Items;
        _breadcrumbBar.ItemClicked += OnItemClicked;
    }

    /// <inheritdoc/>
    public void SetItems(string pageTitle)
    {
        Items.Clear();
        _clickActions.Clear();
        Items.Add(pageTitle);
        _clickActions.Add(null);
    }

    /// <inheritdoc/>
    public void SetItems(params BreadcrumbItem[] items)
    {
        Items.Clear();
        _clickActions.Clear();
        
        foreach (var item in items)
        {
            Items.Add(item.Label);
            _clickActions.Add(item.ClickAction);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Items.Clear();
        _clickActions.Clear();
    }

    /// <inheritdoc/>
    public void HandleItemClicked(int index)
    {
        if (index >= 0 && index < _clickActions.Count)
        {
            _clickActions[index]?.Invoke();
        }
    }

    private void OnItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        HandleItemClicked(args.Index);
    }
}
