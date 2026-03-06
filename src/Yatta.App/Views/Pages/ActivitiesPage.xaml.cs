using System.Windows;
using System.Windows.Controls;
using Yatta.App.Services;
using Yatta.App.ViewModels;

namespace Yatta.App.Views.Pages;

/// <summary>
/// Activities management page.
/// Shows a list of activities with navigation to the detail page.
/// </summary>
public partial class ActivitiesPage : Page
{
    private readonly ActivitiesViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public ActivitiesPage(ActivitiesViewModel viewModel, IBreadcrumbService breadcrumbService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _breadcrumbService = breadcrumbService ?? throw new ArgumentNullException(nameof(breadcrumbService));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Configure the breadcrumb with the page title
        _breadcrumbService.SetItems(
            Yatta.App.Resources.Resources.Page_Activities_Title
        );
        
        await _viewModel.LoadDataAsync();
    }
}