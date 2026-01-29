namespace TimeTracker.App;

using Wpf.Ui.Controls;
using TimeTracker.App.ViewModels;
using TimeTracker.App.Views.Pages;
using TimeTracker.App.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

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
        
        
        // Configure the NavigationService to allow programmatic navigation
        var navigationService = serviceProvider.GetRequiredService<INavigationService>();
        navigationService.SetNavigationView(NavigationView);
        
        
        // Configure the BreadcrumbService with the global BreadcrumbBar
        var breadcrumbService = serviceProvider.GetRequiredService<IBreadcrumbService>();
        breadcrumbService.SetBreadcrumbBar(BreadcrumbBar);
        
        
        // Navigate to the Records page by default
        Loaded += (_, _) => NavigationView.Navigate(typeof(TimeRecordsPage));
    }

    /// <summary>
    /// Gets the global ContentDialogHost to show dialogs.
    /// </summary>
    public ContentDialogHost DialogHost => RootContentDialogHost;

    /// <summary>
    /// Handles the Tray Icon "Open" menu click.
    /// </summary>
    private void TrayOpen_Click(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    /// <summary>
    /// Handles the Tray Icon "Close" menu click.
    /// </summary>
    private void TrayClose_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}