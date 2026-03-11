namespace Yatta.App;

using Wpf.Ui.Controls;
using Yatta.App.ViewModels;
using Yatta.App.Views.Pages;
using Yatta.App.Services;
using Yatta.Core.Interfaces;
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
    private bool _closeConfirmed = false;
    private TodayPage? _todayPage;

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
        
        
        // Navigate to the Today page by default
        Loaded += (_, _) => 
        {
            NavigationView.Navigate(typeof(TodayPage));
        };
        
        // Track when navigating to TodayPage
        NavigationView.Navigated += (_, args) =>
        {
            if (args.Page is TodayPage todayPage)
            {
                _todayPage = todayPage;
            }
        };
        
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
    /// When the app is actually closing and there is an active record, cancels the close
    /// and defers the confirmation dialog to avoid WPF limitations during Closing event.
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
                return;
            }
            // If MinimizeToTray is false, fall through to close confirmation below
        }

        // If the close was already confirmed by the dialog, allow closing
        if (_closeConfirmed)
        {
            return;
        }

        // Always cancel the close — we cannot show dialogs or change visibility during
        // the Closing event. Instead, defer the confirmation to after the event completes.
        e.Cancel = true;

        // Defer the dialog to the next dispatcher frame so the Closing event can complete
        _ = Dispatcher.InvokeAsync(async () =>
        {
            await HandleCloseConfirmationAsync();
        });
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
    /// Handles the Tray Icon "Change Activity" menu click.
    /// Opens the change activity dialog.
    /// </summary>
    private void TrayChangeActivity_Click(object sender, RoutedEventArgs e)
    {
        ShowChangeActivityDialog();
    }

    /// <summary>
    /// Handles the Tray Icon "Close" menu click.
    /// Shows close confirmation if there is an active record before closing.
    /// </summary>
    private async void TrayClose_Click(object sender, RoutedEventArgs e)
    {
        await HandleCloseConfirmationAsync();
    }

    /// <summary>
    /// Handles the close confirmation flow.
    /// Checks for active records, shows the confirmation dialog if needed,
    /// and shuts down the application based on user response.
    /// </summary>
    private async Task HandleCloseConfirmationAsync()
    {
        var dialogResult = await ShowCloseConfirmationIfNeededAsync();

        if (dialogResult == CloseConfirmationResult.Cancel)
        {
            // User cancelled — reset flags and keep the app open
            _isRealClose = false;
            return;
        }

        // User confirmed or no active record — proceed with close
        _isRealClose = true;
        _closeConfirmed = true;
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Checks if there is an active time record and shows a confirmation dialog if so.
    /// </summary>
    /// <returns>The result of the confirmation dialog.</returns>
    private async Task<CloseConfirmationResult> ShowCloseConfirmationIfNeededAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var timeRecordRepository = scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>();
        var activeRecord = await timeRecordRepository.GetActiveAsync();

        if (activeRecord == null)
        {
            return CloseConfirmationResult.NoActiveRecord;
        }

        // Ensure the window is visible so the dialog can be shown
        if (!IsVisible || WindowState == WindowState.Minimized)
        {
            ShowInTaskbar = true;
            Show();
            WindowState = WindowState.Normal;
        }
        Activate();

        // Load the activity name
        var activityRepository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();
        var activity = await activityRepository.GetByIdAsync(activeRecord.ActivityId);
        string activityName = activity?.Name ?? string.Empty;

        // Calculate duration
        var startDateTime = activeRecord.Date.ToDateTime(activeRecord.StartTime);
        var duration = DateTime.Now - startDateTime;
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }
        string durationText = (int)duration.TotalHours > 0
            ? $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}"
            : $"{duration.Minutes:D2}:{duration.Seconds:D2}";

        // Build the dialog content with activity information
        var content = Yatta.App.Resources.Resources.Dialog_CloseApp_Message
            + "\n\n" + string.Format(Yatta.App.Resources.Resources.Dialog_CloseApp_Activity, activityName)
            + "\n" + string.Format(Yatta.App.Resources.Resources.Dialog_CloseApp_StartTime, activeRecord.StartTime.ToString("HH:mm"))
            + "\n" + string.Format(Yatta.App.Resources.Resources.Dialog_CloseApp_Duration, durationText);

        var dialog = new ContentDialog(RootContentDialogHost)
        {
            Title = Yatta.App.Resources.Resources.Dialog_CloseApp_Title,
            Content = content,
            PrimaryButtonText = Yatta.App.Resources.Resources.Button_Yes,
            SecondaryButtonText = Yatta.App.Resources.Resources.Button_No,
            CloseButtonText = Yatta.App.Resources.Resources.Button_Cancel
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Stop the active record
            activeRecord.EndTime = TimeOnly.FromDateTime(DateTime.Now);
            await timeRecordRepository.UpdateAsync(activeRecord);
            return CloseConfirmationResult.StopAndClose;
        }
        else if (result == ContentDialogResult.Secondary)
        {
            return CloseConfirmationResult.CloseWithoutStopping;
        }
        else
        {
            return CloseConfirmationResult.Cancel;
        }
    }

    /// <summary>
    /// Shows the change activity dialog from the global hotkey.
    /// Brings the window to the foreground if needed and opens the dialog.
    /// </summary>
    public void ShowChangeActivityDialog()
    {
        // First, bring the main window to the foreground if hidden or minimized
        if (!IsVisible || WindowState == WindowState.Minimized)
        {
            ShowInTaskbar = true;
            Show();
            WindowState = WindowState.Normal;
        }
        
        // Activate and bring to front
        Activate();
        Topmost = true;
        Topmost = false;
        Focus();

        // Navigate to TodayPage only if not already there
        bool needsNavigation = true;
        if (_todayPage != null && _todayPage.IsLoaded)
        {
            // TodayPage is already loaded, no need to navigate
            needsNavigation = false;
        }

        if (needsNavigation)
        {
            NavigationView.Navigate(typeof(TodayPage));
        }

        // Give the navigation a moment to complete if needed, then trigger the dialog
        Dispatcher.InvokeAsync(() =>
        {
            _todayPage?.BringChangeActivityDialogToFront();
        }, System.Windows.Threading.DispatcherPriority.Background);
    }

    /// <summary>
    /// Represents the result of the close confirmation dialog.
    /// </summary>
    private enum CloseConfirmationResult
    {
        /// <summary>No active record exists, proceed with closing.</summary>
        NoActiveRecord,
        /// <summary>User chose to stop the activity and close the app.</summary>
        StopAndClose,
        /// <summary>User chose to close the app without stopping the activity.</summary>
        CloseWithoutStopping,
        /// <summary>User cancelled, the app should remain open.</summary>
        Cancel
    }
}