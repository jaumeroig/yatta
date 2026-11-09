namespace Yatta.Tests.Services;

using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Yatta.App;

/// <summary>Exercises the tray window's native close and deactivation lifecycle.</summary>
[Collection("Localization")]
public class TrayPanelWindowTests
{
    /// <summary>Deactivation during a close must not recursively close the WPF window.</summary>
    [Fact]
    public void Close_DeactivationDuringAndAfterClosing_DoesNotThrow()
    {
        RunOnStaThread(() =>
        {
            using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
            TrayPanelWindow window = new(services);
            int closed = 0;
            window.Closed += (_, _) => closed++;
            window.Closing += (_, _) => Deactivate(window);

            window.Close();
            DrainDispatcher();

            window.RequestClose();
            window.RequestClose();
            Deactivate(window);
            Deactivate(window);
            DrainDispatcher();
            Assert.Equal(1, closed);
        });
    }

    private static void Deactivate(TrayPanelWindow window) =>
        typeof(TrayPanelWindow).GetMethod("Window_Deactivated", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(window, [window, EventArgs.Empty]);

    private static void DrainDispatcher()
    {
        DispatcherFrame frame = new();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static void RunOnStaThread(Action action)
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                action();
            }
            catch (Exception exception) { failure = exception; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)), "The tray lifecycle test did not finish.");
        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
