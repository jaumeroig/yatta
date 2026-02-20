using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Dashboard page for weekly statistics and breakdown.
/// </summary>
public partial class DashboardWeekPage : Page
{
    private readonly DashboardWeekViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly INavigationService _navigationService;

    public DashboardWeekPage(DashboardWeekViewModel viewModel, IBreadcrumbService breadcrumbService, INavigationService navigationService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _breadcrumbService = breadcrumbService ?? throw new ArgumentNullException(nameof(breadcrumbService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _breadcrumbService.SetItems(
            new BreadcrumbItem(TimeTracker.App.Resources.Resources.Nav_Dashboard, () => _navigationService.Navigate<DashboardIndexPage>()),
            new BreadcrumbItem(TimeTracker.App.Resources.Resources.Dashboard_Week)
        );

        await _viewModel.LoadDataAsync();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
    }
}
