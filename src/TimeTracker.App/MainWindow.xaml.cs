namespace TimeTracker.App;

using Wpf.Ui.Controls;
using TimeTracker.App.ViewModels;
using TimeTracker.App.Views.Pages;
using TimeTracker.App.Services;
using TimeTracker.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.ComponentModel;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly ISettingsRepository _settingsRepository;
    private bool _isRealClose = false;

    public MainWindow(IServiceProvider serviceProvider, MainWindowViewModel viewModel, ISettingsRepository settingsRepository)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        _settingsRepository = settingsRepository;
        
        NavigationView.SetServiceProvider(serviceProvider);
        
        
        // Configure the NavigationService to allow programmatic navigation
        var navigationService = serviceProvider.GetRequiredService<INavigationService>();
        navigationService.SetNavigationView(NavigationView);
        
        
        // Configure the BreadcrumbService with the global BreadcrumbBar
        var breadcrumbService = serviceProvider.GetRequiredService<IBreadcrumbService>();
        breadcrumbService.SetBreadcrumbBar(BreadcrumbBar);
        
        
        // Navigate to the Records page by default
        Loaded += (_, _) => NavigationView.Navigate(typeof(TimeRecordsPage));
        
        // Handle window closing to minimize to tray instead of closing
        Closing += MainWindow_Closing;
    }

    /// <summary>
    /// Handles the window closing event to minimize to tray instead of closing.
    /// </summary>
    private async void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!_isRealClose)
        {
            // Check if minimize to tray is enabled
            var settings = await _settingsRepository.GetAsync();
            if (settings.MinimizeToTray)
            {
                e.Cancel = true;
                ShowInTaskbar = false;
                Hide();
            }
            // If MinimizeToTray is false, allow the window to close normally (exit the app)
        }
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
        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    /// <summary>
    /// Handles the Tray Icon "Close" menu click.
    /// </summary>
    private void TrayClose_Click(object sender, RoutedEventArgs e)
    {
        _isRealClose = true;
        Application.Current.Shutdown();
    }
}