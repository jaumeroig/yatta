using System.Windows.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

public partial class OpcionsPage : Page
{
    public OpcionsPage(OpcionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
