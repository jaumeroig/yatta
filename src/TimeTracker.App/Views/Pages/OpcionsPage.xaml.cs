using System.Windows.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Pàgina d'opcions i configuració de l'aplicació.
/// </summary>
public partial class OpcionsPage : Page
{
    private readonly OpcionsViewModel _viewModel;

    public OpcionsPage(OpcionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
    }

    /// <summary>
    /// Event que es crida quan es carrega la pàgina.
    /// </summary>
    private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
