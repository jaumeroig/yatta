using System.Windows.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

public partial class ActivitatsPage : Page
{
    public ActivitatsPage(ActivitatsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
