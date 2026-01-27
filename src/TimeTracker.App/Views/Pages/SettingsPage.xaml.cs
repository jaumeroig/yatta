namespace TimeTracker.App.Views.Pages;

using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;


/// <summary>
/// Settings and configuration page.
/// </summary>
public partial class SettingsPage : Page
{
    private readonly SettingsViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public SettingsPage(SettingsViewModel viewModel, IBreadcrumbService breadcrumbService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _breadcrumbService = breadcrumbService;
    }

    /// <summary>
    /// Event that is called when the page is loaded.
    /// </summary>
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Configure the breadcrumb with the page title
        _breadcrumbService.SetItems(
            TimeTracker.App.Resources.Resources.Page_Settings_Title
        );

        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
