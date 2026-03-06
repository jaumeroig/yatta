using System.Windows;
using System.Windows.Controls;
using Yatta.App.Services;
using Yatta.App.ViewModels;

namespace Yatta.App.Views.Pages;

/// <summary>
/// Dashboard page for monthly statistics and breakdown.
/// </summary>
public partial class DashboardMonthPage : Page
{
    private readonly DashboardMonthViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly INavigationService _navigationService;

    public DashboardMonthPage(DashboardMonthViewModel viewModel, IBreadcrumbService breadcrumbService, INavigationService navigationService)
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
            new BreadcrumbItem(Yatta.App.Resources.Resources.Nav_Dashboard, () => _navigationService.Navigate<DashboardIndexPage>()),
            new BreadcrumbItem(Yatta.App.Resources.Resources.Dashboard_Month)
        );

        await _viewModel.LoadDataAsync();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
    }
}
