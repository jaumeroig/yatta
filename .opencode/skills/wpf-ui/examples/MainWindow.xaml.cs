using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace YourApp;

/// <summary>
/// Main application window with NavigationView shell.
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        // Auto-sync with Windows system theme (dark/light mode)
        SystemThemeWatcher.Watch(this);
        
        InitializeComponent();
        
        // Optional: Navigate to default page on load
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Navigate to the first page after the window loads
        // The NavigationView will automatically navigate to the first item
        // if you don't specify this, but you can customize it here
    }
    
    /// <summary>
    /// Toggle between light and dark theme.
    /// </summary>
    public void ToggleTheme()
    {
        var currentTheme = ApplicationThemeManager.GetAppTheme();
        
        ApplicationThemeManager.Apply(
            currentTheme == ApplicationTheme.Dark 
                ? ApplicationTheme.Light 
                : ApplicationTheme.Dark,
            WindowBackdropType.Mica
        );
    }
}
