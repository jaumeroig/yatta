namespace Yatta.App;

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Yatta.App.ViewModels;
using Yatta.Core.Interfaces;

/// <summary>
/// Tray icon information panel window.
/// Shows current workday status and active activity information.
/// </summary>
public partial class TrayPanelWindow : FluentWindow
{
    private readonly TrayPanelViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly MainWindow _mainWindow;
    private readonly IServiceScope _scope;
    private bool _isClosingRequested;

    public TrayPanelWindow(IServiceProvider serviceProvider, MainWindow mainWindow)
    {
        _serviceProvider = serviceProvider;
        _mainWindow = mainWindow;
        _scope = serviceProvider.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<TrayPanelViewModel>();
        DataContext = _viewModel;

        InitializeComponent();
    }

    /// <summary>
    /// Loads data and positions the window near the tray icon.
    /// </summary>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadDataAsync();
        PositionWindowNearTrayIcon();
    }

    /// <summary>
    /// Positions the window near the system tray icon.
    /// </summary>
    private void PositionWindowNearTrayIcon()
    {
        // Get the position of the tray icon area
        var trayRect = GetTrayIconRect();

        // Get the working area of the primary screen
        var workingArea = SystemParameters.WorkArea;

        // Calculate position (bottom-right corner of screen, above taskbar)
        var windowWidth = Width + 8; // Account for top margin
        var windowHeight = ActualHeight + 8; // Account for top margin

        // Position the window
        Left = workingArea.Right - windowWidth;
        Top = workingArea.Bottom - windowHeight;
    }

    /// <summary>
    /// Gets the rectangle of the system tray area.
    /// </summary>
    private System.Windows.Rect GetTrayIconRect()
    {
        // Get the taskbar position
        var taskbarRect = GetTaskbarRect();

        // Return a rectangle representing the notification area (tray)
        // This is typically in the bottom-right corner
        return new System.Windows.Rect(
            taskbarRect.Right - 100,
            taskbarRect.Top,
            100,
            taskbarRect.Height);
    }

    /// <summary>
    /// Gets the rectangle of the Windows taskbar.
    /// </summary>
    private System.Windows.Rect GetTaskbarRect()
    {
        APPBARDATA abd = new APPBARDATA();
        abd.cbSize = Marshal.SizeOf(abd);
        SHAppBarMessage(ABM_GETTASKBARPOS, ref abd);

        return new System.Windows.Rect(
            abd.rc.left,
            abd.rc.top,
            abd.rc.right - abd.rc.left,
            abd.rc.bottom - abd.rc.top);
    }

    /// <summary>
    /// Closes the panel when it loses focus.
    /// </summary>
    private void Window_Deactivated(object? sender, EventArgs e)
    {
        RequestClose();
    }

    /// <summary>
    /// Closes the panel.
    /// </summary>
    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        RequestClose();
    }

    /// <summary>
    /// Opens the main application window and closes the panel.
    /// </summary>
    private void OnOpenAppClick(object sender, RoutedEventArgs e)
    {
        _mainWindow.ShowInTaskbar = true;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
        RequestClose();
    }

    /// <summary>
    /// Opens the start activity dialog.
    /// </summary>
    private void OnStartActivityClick(object sender, RoutedEventArgs e)
    {
        _mainWindow.ShowChangeActivityDialog();
        RequestClose();
    }

    /// <summary>
    /// Opens the change activity dialog.
    /// </summary>
    private void OnChangeActivityClick(object sender, RoutedEventArgs e)
    {
        _mainWindow.ShowChangeActivityDialog();
        RequestClose();
    }

    /// <summary>
    /// Stops the current activity.
    /// </summary>
    private async void OnStopActivityClick(object sender, RoutedEventArgs e)
    {
        using var scope = _serviceProvider.CreateScope();
        var timeRecordRepository = scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>();
        var activeRecord = await timeRecordRepository.GetActiveAsync();

        if (activeRecord != null)
        {
            activeRecord.EndTime = TimeOnly.FromDateTime(DateTime.Now);
            await timeRecordRepository.UpdateAsync(activeRecord);
        }

        RequestClose();
    }

    /// <summary>
    /// Closes the panel only once, even if multiple events request it while the window is deactivating.
    /// </summary>
    private void RequestClose()
    {
        if (_isClosingRequested)
        {
            return;
        }

        _isClosingRequested = true;
        Close();
    }

    /// <summary>
    /// Cleanup when window is closed.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.Cleanup();
        _scope.Dispose();
        base.OnClosed(e);
    }

    #region Windows API for Taskbar Position

    private const int ABM_GETTASKBARPOS = 0x00000005;

    [DllImport("shell32.dll")]
    private static extern IntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    #endregion
}
