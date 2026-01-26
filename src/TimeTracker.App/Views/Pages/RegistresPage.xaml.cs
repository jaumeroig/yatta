using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Pàgina per a la gestió de registres de temps.
/// </summary>
public partial class RegistresPage : Page
{
    private readonly RegistresViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;

    public RegistresPage(RegistresViewModel viewModel, IBreadcrumbService breadcrumbService)
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
            TimeTracker.App.Resources.Resources.Nav_Records
        );

        await _viewModel.LoadDataAsync();
    }
}
