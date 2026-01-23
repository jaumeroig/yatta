using System.Windows.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

public partial class RegistresPage : Page
{
    public RegistresPage(RegistresViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
