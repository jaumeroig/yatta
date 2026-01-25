using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Pàgina de gestió d'activitats.
/// Mostra una llista d'activitats amb navegació a la pàgina de detall.
/// </summary>
public partial class ActivitatsPage : Page
{
    private readonly ActivitatsViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public ActivitatsPage(ActivitatsViewModel viewModel, IBreadcrumbService breadcrumbService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _breadcrumbService = breadcrumbService ?? throw new ArgumentNullException(nameof(breadcrumbService));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Configurar el breadcrumb amb el títol de la pàgina
        _breadcrumbService.SetItems(
            TimeTracker.App.Resources.Resources.Page_Activities_Title
        );
        
        await _viewModel.LoadDataAsync();
    }
}
