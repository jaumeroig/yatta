namespace Yatta.App;

using System.Windows;
using Wpf.Ui.Controls;

/// <summary>Windows shell for the Blazor tray panel.</summary>
public partial class TrayPanelWindow : FluentWindow
{
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

    private void Window_Deactivated(object sender, EventArgs args) => Close();
}
