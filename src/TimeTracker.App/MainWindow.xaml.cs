using Wpf.Ui;
using Wpf.Ui.Controls;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow(IServiceProvider serviceProvider, MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        NavigationView.SetServiceProvider(serviceProvider);
    }

    /// <summary>
    /// Obté el ContentDialogHost global per mostrar diàlegs.
    /// </summary>
    public ContentDialogHost DialogHost => RootContentDialogHost;
}