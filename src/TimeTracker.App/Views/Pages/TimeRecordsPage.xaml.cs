namespace TimeTracker.App.Views.Pages;

using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

/// <summary>
/// Page for time records management.
/// </summary>
public partial class TimeRecordsPage : Page
{
    private readonly TimeRecordViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public TimeRecordsPage(TimeRecordViewModel viewModel, IBreadcrumbService breadcrumbService)
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
            TimeTracker.App.Resources.Resources.Nav_Records
        );

        await _viewModel.LoadDataAsync();
    }
}
