namespace Yatta.App;

using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Yatta.App.Services;

/// <summary>Standalone Blazor activity picker for tray and global shortcut actions.</summary>
public partial class ChangeActivityWindow : FluentWindow
{
    private readonly UiEventService _events;

    /// <summary>Creates the quick action window.</summary>
    public ChangeActivityWindow(IServiceProvider services)
    {
        InitializeComponent();
        Resources.Add("services", services);
        _events = services.GetRequiredService<UiEventService>();
        _events.QuickActionCompleted += OnCompleted;
    }

    /// <summary>Whether the quick action saved a new active entry.</summary>
    public bool WasSaved { get; private set; }

    private void OnCompleted(object? sender, EventArgs args)
    {
        Dispatcher.Invoke(() => { WasSaved = true; Close(); });
    }

    protected override void OnClosed(EventArgs args)
    {
        _events.QuickActionCompleted -= OnCompleted;
        base.OnClosed(args);
    }
}
