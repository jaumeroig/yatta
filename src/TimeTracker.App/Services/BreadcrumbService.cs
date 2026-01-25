using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace TimeTracker.App.Services;

/// <summary>
/// Representa un element del breadcrumb per a la navegació.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Text a mostrar al breadcrumb.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Acció a executar quan es fa clic a l'element.
    /// </summary>
    public Action? ClickAction { get; set; }

    /// <summary>
    /// Crea un nou element de breadcrumb.
    /// </summary>
    /// <param name="label">Text a mostrar.</param>
    /// <param name="clickAction">Acció opcional al fer clic.</param>
    public BreadcrumbItem(string label, Action? clickAction = null)
    {
        Label = label;
        ClickAction = clickAction;
    }

    /// <inheritdoc/>
    public override string ToString() => Label;
}

/// <summary>
/// Interfície per al servei de gestió del breadcrumb.
/// </summary>
public interface IBreadcrumbService
{
    /// <summary>
    /// Col·lecció d'elements del breadcrumb (strings per mostrar).
    /// </summary>
    ObservableCollection<string> Items { get; }

    /// <summary>
    /// Estableix el BreadcrumbBar a gestionar.
    /// </summary>
    /// <param name="breadcrumbBar">El control BreadcrumbBar.</param>
    void SetBreadcrumbBar(BreadcrumbBar breadcrumbBar);

    /// <summary>
    /// Estableix un únic element al breadcrumb (per a pàgines principals).
    /// </summary>
    /// <param name="pageTitle">Títol de la pàgina.</param>
    void SetItems(string pageTitle);

    /// <summary>
    /// Estableix els elements del breadcrumb.
    /// </summary>
    /// <param name="items">Elements a mostrar.</param>
    void SetItems(params BreadcrumbItem[] items);

    /// <summary>
    /// Neteja el breadcrumb (amaga'l o mostra només l'element arrel).
    /// </summary>
    void Clear();

    /// <summary>
    /// Gestiona el clic en un element del breadcrumb.
    /// </summary>
    /// <param name="index">Índex de l'element clicat.</param>
    void HandleItemClicked(int index);
}

/// <summary>
/// Implementació del servei de gestió del breadcrumb.
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
