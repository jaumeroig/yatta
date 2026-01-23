using System.Windows;
using Wpf.Ui.Controls;

namespace TimeTracker.App.Services;

/// <summary>
/// Servei per gestionar els diàlegs de l'aplicació.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Obté el ContentDialogHost global de l'aplicació.
    /// </summary>
    ContentDialogHost? GetDialogHost();
}

/// <summary>
/// Implementació del servei de diàlegs.
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// Obté el ContentDialogHost global de l'aplicació.
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
