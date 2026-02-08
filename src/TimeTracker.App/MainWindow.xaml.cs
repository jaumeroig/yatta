namespace TimeTracker.App;

using Wpf.Ui.Controls;
using TimeTracker.App.ViewModels;
using TimeTracker.App.Views.Pages;
using TimeTracker.App.Services;
using TimeTracker.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.ComponentModel;
using System;
using System.Windows.Input;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IServiceProvider _serviceProvider;
    private bool _isRealClose = false;

    public MainWindow(IServiceProvider serviceProvider, MainWindowViewModel viewModel, ISettingsRepository settingsRepository)
    {
        InitializeComponent();
        DataContext = viewModel;

        _settingsRepository = settingsRepository;
        _serviceProvider = serviceProvider;

        NavigationView.SetServiceProvider(serviceProvider);
        
        
        // Configure the NavigationService to allow programmatic navigation
        var navigationService = serviceProvider.GetRequiredService<INavigationService>();
        navigationService.SetNavigationView(NavigationView);
        
        
        // Configure the BreadcrumbService with the global BreadcrumbBar
        var breadcrumbService = serviceProvider.GetRequiredService<IBreadcrumbService>();
        breadcrumbService.SetBreadcrumbBar(BreadcrumbBar);
        
        
        // Navigate to the Hoy page by default
        Loaded += (_, _) => NavigationView.Navigate(typeof(HoyPage));
        
        // Handle window closing to minimize to tray instead of closing
        Closing += MainWindow_Closing;
        StateChanged += MainWindow_StateChanged;

        // DEBUG: Keyboard shortcut to test notifications (Ctrl+Shift+T)
        KeyDown += MainWindow_KeyDown;
    }

    /// <summary>
    /// DEBUG: Handles keyboard shortcuts for testing.
    /// Ctrl+Shift+T = Force show notification
    /// </summary>
    private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        // Ctrl+Shift+T to test notification
        if (e.Key == Key.T && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            var notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            await notificationService.ForceShowNotificationAsync();
            e.Handled = true;
        }
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

    private async void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            var settings = await _settingsRepository.GetAsync();
            if (settings.MinimizeToTray)
            {
                ShowInTaskbar = false;
                Hide();
            }
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

    /// <summary>
    /// Shows the change activity dialog from the global hotkey.
    /// Brings the window to the foreground if needed and opens the dialog.
    /// </summary>
    public void ShowChangeActivityDialog()
    {
        // Get the HoyPage instance
        var hoyPage = _serviceProvider.GetService<HoyPage>();
        if (hoyPage == null)
        {
            // Navigate to HoyPage first if not available
            NavigationView.Navigate(typeof(HoyPage));
            hoyPage = _serviceProvider.GetService<HoyPage>();
        }

        // Get the HoyViewModel and trigger the change activity dialog
        if (hoyPage?.DataContext is HoyViewModel viewModel)
        {
            // Execute the ChangeActivityCommand which opens the dialog
            viewModel.ChangeActivityCommand.Execute(null);
        }
    }
}