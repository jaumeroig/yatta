using Wpf.Ui.Controls;
using TimeTracker.App.ViewModels;
using TimeTracker.App.Views.Pages;
using TimeTracker.App.Services;
using Microsoft.Extensions.DependencyInjection;

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
        
        // Configurar el NavigationService per permetre navegació programàtica
        var navigationService = serviceProvider.GetRequiredService<INavigationService>();
        navigationService.SetNavigationView(NavigationView);
        
        // Configurar el BreadcrumbService amb el BreadcrumbBar global
        var breadcrumbService = serviceProvider.GetRequiredService<IBreadcrumbService>();
        breadcrumbService.SetBreadcrumbBar(BreadcrumbBar);
        
        // Navegar a la pàgina de Registres per defecte
        Loaded += (_, _) => NavigationView.Navigate(typeof(RegistresPage));
    }

    /// <summary>
    /// Obté el ContentDialogHost global per mostrar diàlegs.
    /// </summary>
    public ContentDialogHost DialogHost => RootContentDialogHost;
}