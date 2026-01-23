using System.Windows.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

public partial class RegistresPage : Page
{
    private readonly RegistresViewModel _viewModel;

    public RegistresPage(RegistresViewModel viewModel)
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
