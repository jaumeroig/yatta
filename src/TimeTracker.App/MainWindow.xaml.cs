using Wpf.Ui;
using Wpf.Ui.Controls;
using TimeTracker.App.ViewModels;
using TimeTracker.App.Views.Pages;

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
        
        // Navegar a la pàgina de Registres per defecte
        Loaded += (_, _) => NavigationView.Navigate(typeof(RegistresPage));
    }

    /// <summary>
    /// Obté el ContentDialogHost global per mostrar diàlegs.
    /// </summary>
    public ContentDialogHost DialogHost => RootContentDialogHost;
}