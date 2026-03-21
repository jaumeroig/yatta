namespace Yatta.App;

using System.Windows;
using Velopack;

/// <summary>
/// Application entry point. Velopack must be initialized before WPF starts.
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
