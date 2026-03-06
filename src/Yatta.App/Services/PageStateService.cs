namespace Yatta.App.Services;

/// <summary>
/// Interface for managing page state across navigations.
/// </summary>
public interface IPageStateService
{
    /// <summary>
    /// Gets the filter state for the Activities page.
    /// </summary>
    ActivitiesPageState ActivitiesPage { get; }

    /// <summary>
    /// Gets the shared state for Dashboard pages (context date).
    /// </summary>
    DashboardPageState DashboardPage { get; }
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
/// State for the Dashboard pages shared context date.
/// </summary>
public class DashboardPageState
{
    /// <summary>
    /// The context date shared across all dashboard sub-pages.
    /// When switching between Day/Week/Month/Year dashboards, this date is used
    /// to initialize the new view so the user doesn't lose their temporal context.
    /// </summary>
    public DateOnly ContextDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

/// <summary>
/// Service for managing page state across navigations.
/// This service is registered as a singleton to preserve state between page instances.
/// </summary>
public class PageStateService : IPageStateService
{
    /// <inheritdoc/>
    public ActivitiesPageState ActivitiesPage { get; } = new();

    /// <inheritdoc/>
    public DashboardPageState DashboardPage { get; } = new();
}
