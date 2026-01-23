using System.Windows.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

public partial class JornadaPage : Page
{
    public JornadaPage(JornadaViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
