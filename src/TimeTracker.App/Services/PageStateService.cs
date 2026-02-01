namespace TimeTracker.App.Services;

/// <summary>
/// Interface for managing page state across navigations.
/// </summary>
public interface IPageStateService
{
    /// <summary>
    /// Gets the filter state for the Activities page.
    /// </summary>
    ActivitiesPageState ActivitiesPage { get; }
}

/// <summary>
/// State for the Activities page filters.
/// </summary>
public class ActivitiesPageState
{
    /// <summary>
    /// Search text filter.
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Whether to show inactive (archived) activities.
    /// </summary>
    public bool ShowInactive { get; set; } = false;
}

/// <summary>
/// Service for managing page state across navigations.
/// This service is registered as a singleton to preserve state between page instances.
/// </summary>
public class PageStateService : IPageStateService
{
    /// <inheritdoc/>
    public ActivitiesPageState ActivitiesPage { get; } = new();
}
