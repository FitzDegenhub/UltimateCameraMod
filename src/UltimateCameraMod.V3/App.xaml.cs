using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using UltimateCameraMod.V3.Localization;

namespace UltimateCameraMod.V3;

public partial class App : Application
{
    private static string L(string key) => TranslationSource.Instance[key];
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
            MessageBox.Show(string.Format(L("Msg_StartupCrash"), ex.Message), L("Title_UcmFatal"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        string log = Path.Combine(AppContext.BaseDirectory, "crash.log");
        File.WriteAllText(log, $"{DateTime.Now}\n{e.Exception}");
        MessageBox.Show(string.Format(L("Msg_CrashError"), e.Exception.Message),
            L("Title_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnDomainException(object sender, UnhandledExceptionEventArgs e)
    {
        string log = Path.Combine(AppContext.BaseDirectory, "crash.log");
        File.WriteAllText(log, $"{DateTime.Now}\n{e.ExceptionObject}");
    }
}

