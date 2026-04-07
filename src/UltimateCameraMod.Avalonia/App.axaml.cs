using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Force InvariantCulture so all double->string formatting uses '.' as decimal separator.
        // Without this, European locales (e.g. German, French) produce "7,5" in XML values
        // which the game cannot parse, breaking all camera modifications.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        AppDomain.CurrentDomain.UnhandledException += OnDomainException;

        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            string log = Path.Combine(AppContext.BaseDirectory, "crash.log");
            File.WriteAllText(log, $"{DateTime.Now}\nSTARTUP CRASH:\n{ex}");
        }
    }

    private void OnDomainException(object sender, UnhandledExceptionEventArgs e)
    {
        string log = Path.Combine(AppContext.BaseDirectory, "crash.log");
        File.WriteAllText(log, $"{DateTime.Now}\n{e.ExceptionObject}");
    }
}
