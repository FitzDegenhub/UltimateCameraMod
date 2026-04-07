using Avalonia.Controls;
using Avalonia.Interactivity;

namespace UltimateCameraMod.Avalonia;

public partial class ImportPresetDialog : Window
{
    public string SelectedMode { get; private set; } = "";
    public bool Confirmed { get; private set; }

    public ImportPresetDialog()
    {
        InitializeComponent();
    }

    private void OnImportModPackage(object? sender, RoutedEventArgs e)
    {
        SelectedMode = "mod_package";
        Confirmed = true;
        Close();
    }

    private void OnImportXml(object? sender, RoutedEventArgs e)
    {
        SelectedMode = "xml";
        Confirmed = true;
        Close();
    }

    private void OnImportPaz(object? sender, RoutedEventArgs e)
    {
        SelectedMode = "paz";
        Confirmed = true;
        Close();
    }

    private void OnImportUcmPreset(object? sender, RoutedEventArgs e)
    {
        SelectedMode = "ucmpreset";
        Confirmed = true;
        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }
}
