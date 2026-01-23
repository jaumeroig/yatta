using System.Windows.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

public partial class ActivitatsPage : Page
{
    private readonly ActivitatsViewModel _viewModel;

    public ActivitatsPage(ActivitatsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.LoadDataAsync();
    }
}
