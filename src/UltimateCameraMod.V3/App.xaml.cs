using System.Windows;
using System.Windows.Threading;

namespace UltimateCameraMod.V3;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainException;
        try
        {
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            string log = Path.Combine(AppContext.BaseDirectory, "crash.log");
            File.WriteAllText(log, $"{DateTime.Now}\nSTARTUP CRASH:\n{ex}");
            MessageBox.Show($"Startup crash:\n\n{ex.Message}", "UCM Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        string log = Path.Combine(AppContext.BaseDirectory, "crash.log");
        File.WriteAllText(log, $"{DateTime.Now}\n{e.Exception}");
        MessageBox.Show($"Unhandled error:\n\n{e.Exception.Message}\n\nFull details saved to crash.log",
            "Ultimate Camera Mod - Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnDomainException(object sender, UnhandledExceptionEventArgs e)
    {
        string log = Path.Combine(AppContext.BaseDirectory, "crash.log");
        File.WriteAllText(log, $"{DateTime.Now}\n{e.ExceptionObject}");
    }
}

