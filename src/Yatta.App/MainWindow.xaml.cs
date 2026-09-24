namespace Yatta.App;

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Yatta.App.Services;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>Windows shell for the Blazor interface and desktop integrations.</summary>
public partial class MainWindow : FluentWindow
{
    private readonly IServiceProvider _services;
    private readonly UiEventService _events;
    private readonly TimeEntryService _entries;
    private readonly ILocalizationService _localization;
    private readonly DispatcherTimer _tooltipTimer;
    private bool _allowClose;
    private bool _closingInProgress;
    private TrayPanelWindow? _trayPanel;
    private ChangeActivityWindow? _quickWindow;
    private DateTime _lastTrayPanelClosedAt = DateTime.MinValue;

    /// <summary>Creates the desktop shell.</summary>
    public MainWindow(IServiceProvider services, UiEventService events, TimeEntryService entries)
    {
        InitializeComponent();
        _services = services;
        _events = events;
        _entries = entries;
        _localization = services.GetRequiredService<ILocalizationService>();
        Resources.Add("services", services);
        _tooltipTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _tooltipTimer.Tick += async (_, _) => await UpdateTrayTooltipAsync();
        _tooltipTimer.Start();
        Loaded += async (_, _) => await UpdateTrayTooltipAsync();
        Closing += OnClosing;
        StateChanged += OnStateChanged;
        KeyDown += OnKeyDown;
        _events.DataChanged += OnDataChanged;
        _localization.CultureChanged += OnCultureChanged;
    }

    private void OnDataChanged(object? sender, EventArgs args) => _ = Dispatcher.InvokeAsync(async () => await UpdateTrayTooltipAsync());

    private void OnCultureChanged(object? sender, EventArgs args) => _ = Dispatcher.InvokeAsync(async () =>
    {
        Title = _localization.GetString("App_Title");
        TitleBar.Title = Title;
        TrayOpenItem.Header = _localization.GetString("Tray_Open");
        TrayChangeActivityItem.Header = _localization.GetString("Tray_StartActivity");
        TrayStopActivityItem.Header = _localization.GetString("Tray_StopActivity");
        TrayCloseItem.Header = _localization.GetString("Tray_Close");
        await UpdateTrayTooltipAsync();
    });

    private async void OnKeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.T && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            await _services.GetRequiredService<INotificationService>().ForceShowNotificationAsync();
            args.Handled = true;
        }
    }

    private void OnClosing(object? sender, CancelEventArgs args)
    {
        if (_allowClose) { return; }
        args.Cancel = true;
        if (!_closingInProgress) { _ = Dispatcher.InvokeAsync(HandleWindowCloseAsync); }
    }

    private async Task HandleWindowCloseAsync()
    {
        if (_closingInProgress) { return; }
        _closingInProgress = true;
        try
        {
            using IServiceScope scope = _services.CreateScope();
            AppSettings settings = await scope.ServiceProvider.GetRequiredService<ISettingsRepository>().GetAsync();
            if (settings.MinimizeToTray) { ShowInTaskbar = false; Hide(); }
            else { await RequestQuitAsync(); }
        }
        finally { _closingInProgress = false; }
    }

    private async void OnStateChanged(object? sender, EventArgs args)
    {
        if (WindowState != WindowState.Minimized) { return; }
        using IServiceScope scope = _services.CreateScope();
        AppSettings settings = await scope.ServiceProvider.GetRequiredService<ISettingsRepository>().GetAsync();
        if (settings.MinimizeToTray && WindowState == WindowState.Minimized) { ShowInTaskbar = false; Hide(); }
    }

    private void TrayOpen_Click(object sender, RoutedEventArgs args)
    {
        if (DateTime.UtcNow - _lastTrayPanelClosedAt <= TimeSpan.FromMilliseconds(400)) { return; }
        if (_trayPanel is not null) { _trayPanel.Close(); return; }
        _trayPanel = new TrayPanelWindow(_services);
        _trayPanel.Closed += (_, _) => { _lastTrayPanelClosedAt = DateTime.UtcNow; _trayPanel = null; };
        _trayPanel.Show();
        _trayPanel.Activate();
    }

    private void TrayOpen_DoubleClick(object sender, RoutedEventArgs args) { _trayPanel?.Close(); ShowMainWindow(); }
    private void TrayMenuOpen_Click(object sender, RoutedEventArgs args) => ShowMainWindow();
    private void TrayChangeActivity_Click(object sender, RoutedEventArgs args) => ShowChangeActivityDialog();
    private async void TrayStopActivity_Click(object sender, RoutedEventArgs args) => await StopActiveRecordAsync();
    private async void TrayClose_Click(object sender, RoutedEventArgs args) => await RequestQuitAsync();

    /// <summary>Shows and activates the main window.</summary>
    public void ShowMainWindow()
    {
        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    /// <summary>Stops the active entry from native UI.</summary>
    public async Task StopActiveRecordAsync() => await _entries.StopAsync();

    /// <summary>Shows the activity picker in the visible Blazor view or a quick window.</summary>
    public void ShowChangeActivityDialog()
    {
        if (IsVisible && WindowState != WindowState.Minimized)
        {
            ShowMainWindow();
            _events.RequestChangeActivity();
            return;
        }
        if (_quickWindow is not null) { _quickWindow.Activate(); return; }
        _quickWindow = new ChangeActivityWindow(_services);
        _quickWindow.Closed += (_, _) => _quickWindow = null;
        _quickWindow.Show();
        _quickWindow.Activate();
    }

    /// <summary>Closes the Blazor tray panel after an action.</summary>
    public void CloseTrayPanel() => _trayPanel?.Close();

    private async void TrayContextMenu_Opened(object sender, RoutedEventArgs args)
    {
        using IServiceScope scope = _services.CreateScope();
        bool active = await scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>().GetActiveAsync() is not null;
        TrayChangeActivityItem.Header = active ? Yatta.App.Resources.Resources.Tray_ChangeActivity : Yatta.App.Resources.Resources.Tray_StartActivity;
        TrayStopActivityItem.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task RequestQuitAsync()
    {
        if (_closingInProgress && _allowClose) { return; }
        using IServiceScope scope = _services.CreateScope();
        TimeRecord? active = await scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>().GetActiveAsync();
        if (active is not null)
        {
            ShowMainWindow();
            CloseDecision answer = await _events.AskCloseDecisionAsync();
            if (answer == CloseDecision.Cancel) { return; }
            if (answer == CloseDecision.StopAndClose) { await _entries.StopAsync(); }
        }
        _allowClose = true;
        Application.Current.Shutdown();
    }

    private async Task UpdateTrayTooltipAsync()
    {
        using IServiceScope scope = _services.CreateScope();
        TimeRecord? active = await scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>().GetActiveAsync();
        if (active is null) { TrayNotifyIcon.TooltipText = $"Yatta - {Yatta.App.Resources.Resources.Tray_NoActivity}"; return; }
        Activity? activity = await scope.ServiceProvider.GetRequiredService<IActivityRepository>().GetByIdAsync(active.ActivityId);
        TrayNotifyIcon.TooltipText = string.Format(Yatta.App.Resources.Resources.Tray_TooltipActive, "Yatta", activity?.Name ?? string.Empty, Blazor.UiFormat.Duration(Blazor.UiFormat.Elapsed(active, DateTime.Now)));
    }

    protected override void OnClosed(EventArgs args)
    {
        _tooltipTimer.Stop();
        _events.DataChanged -= OnDataChanged;
        _localization.CultureChanged -= OnCultureChanged;
        base.OnClosed(args);
    }
}
