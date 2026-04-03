using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace UltimateCameraMod.V3;

public partial class App : Application
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appID);

    protected override void OnStartup(StartupEventArgs e)
    {
        // Force InvariantCulture so all double→string formatting uses '.' as decimal separator.
        // Without this, European locales (e.g. German, French) produce "7,5" in XML values
        // which the game cannot parse, breaking all camera modifications.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Stable taskbar / jump list identity (must run before any HWND is created).
        try { SetCurrentProcessExplicitAppUserModelID(ApplicationIdentity.AppUserModelId); } catch { }

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

