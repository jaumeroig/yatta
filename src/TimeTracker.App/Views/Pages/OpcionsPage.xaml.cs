using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Pàgina d'opcions i configuració de l'aplicació.
/// </summary>
public partial class OpcionsPage : Page
{
    private readonly OpcionsViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public OpcionsPage(OpcionsViewModel viewModel, IBreadcrumbService breadcrumbService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _breadcrumbService = breadcrumbService;
    }

    /// <summary>
    /// Event que es crida quan es carrega la pàgina.
    /// </summary>
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Configurar el breadcrumb amb el títol de la pàgina
        _breadcrumbService.SetItems(
            TimeTracker.App.Resources.Resources.Page_Settings_Title
        );

        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
