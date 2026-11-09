namespace Yatta.App;

using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Controls;

/// <summary>Windows shell for the Blazor tray panel.</summary>
public partial class TrayPanelWindow : FluentWindow
{
    private bool _closing;
    private bool _closed;

    /// <summary>Creates the tray panel.</summary>
    public TrayPanelWindow(IServiceProvider services)
    {
        InitializeComponent();
        Resources.Add("services", services);
    }

    private void Window_Loaded(object sender, RoutedEventArgs args)
    {
        Rect work = SystemParameters.WorkArea;
        Left = work.Right - Width - 8;
        Top = work.Bottom - Height - 8;
    }

    /// <summary>Closes the panel once, including during overlapping tray and focus events.</summary>
    public void RequestClose()
    {
        if (_closing || _closed)
        {
            return;
        }

        Close();
    }

    private void Window_Deactivated(object sender, EventArgs args)
    {
        if (_closing || _closed)
        {
            return;
        }

        // Finish the current activation transition before closing the WebView window.
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            if (!IsActive)
            {
                RequestClose();
            }
        }));
    }

    /// <inheritdoc />
    protected override void OnClosing(CancelEventArgs args)
    {
        _closing = true;
        base.OnClosing(args);
        if (args.Cancel)
        {
            _closing = false;
        }
    }

    /// <inheritdoc />
    protected override void OnClosed(EventArgs args)
    {
        _closed = true;
        base.OnClosed(args);
    }
}
