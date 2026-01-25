using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Pàgina de gestió d'activitats.
/// Mostra una llista d'activitats amb navegació a la pàgina de detall.
/// </summary>
public partial class ActivitatsPage : Page
{
    private readonly ActivitatsViewModel _viewModel;

    public ActivitatsPage(ActivitatsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadDataAsync();
    }
}
