namespace TimeTracker.App.Views.Pages;

using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

/// <summary>
/// Interaction logic for HoyPage.xaml
/// </summary>
public partial class HoyPage : Page
{
    private readonly HoyViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public HoyPage(HoyViewModel viewModel, IBreadcrumbService breadcrumbService)
    {
        _viewModel = viewModel;
        _breadcrumbService = breadcrumbService;
        
        DataContext = _viewModel;
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _breadcrumbService.SetItems(TimeTracker.App.Resources.Resources.Page_Today_Title);
        await _viewModel.LoadDataAsync();
    }
}
