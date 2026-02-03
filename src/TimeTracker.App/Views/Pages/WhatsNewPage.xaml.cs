namespace TimeTracker.App.Views.Pages;

using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

/// <summary>
/// What's New page that displays version history.
/// </summary>
public partial class WhatsNewPage : Page
{
    private readonly WhatsNewViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public WhatsNewPage(WhatsNewViewModel viewModel, IBreadcrumbService breadcrumbService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _breadcrumbService = breadcrumbService;
    }

    /// <summary>
    /// Event that is called when the page is loaded.
    /// </summary>
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Configure the breadcrumb with the page title
        _breadcrumbService.SetItems(
            TimeTracker.App.Resources.Resources.Page_WhatsNew_Title
        );

        _viewModel.LoadDataCommand.Execute(null);
    }
}
