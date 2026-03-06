namespace Yatta.App.Services;

using System.Windows;
using Wpf.Ui.Controls;


/// <summary>
/// Service to manage application dialogs.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Gets the global ContentDialogHost of the application.
    /// </summary>
    ContentDialogHost? GetDialogHost();
}

/// <summary>
/// Implementation of the dialogs service.
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// Gets the global ContentDialogHost of the application.
    /// </summary>
    public ContentDialogHost? GetDialogHost()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            return mainWindow.DialogHost;
        }
        return null;
    }
}
