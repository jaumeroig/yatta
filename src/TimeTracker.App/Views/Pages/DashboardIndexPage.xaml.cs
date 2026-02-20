using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Dashboard index page showing four large tiles to navigate to each period dashboard.
/// </summary>
public partial class DashboardIndexPage : Page
{
    private readonly DashboardIndexViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public DashboardIndexPage(DashboardIndexViewModel viewModel, IBreadcrumbService breadcrumbService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _breadcrumbService = breadcrumbService ?? throw new ArgumentNullException(nameof(breadcrumbService));
        DataContext = viewModel;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _breadcrumbService.SetItems(
            TimeTracker.App.Resources.Resources.Nav_Dashboard
        );
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
    }
}
