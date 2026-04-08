using System.Windows;
using UltimateCameraMod.V3.Localization;

namespace UltimateCameraMod.V3;

public partial class ImportPresetDialog : Window
{
    private static string L(string key) => TranslationSource.Instance[key];
    public string SelectedMode { get; private set; } = "";

    public ImportPresetDialog()
    {
        InitializeComponent();
    }

    private void OnImportModPackage(object sender, RoutedEventArgs e)
    {
        SelectedMode = "mod_package";
        DialogResult = true;
    }

    private void OnImportXml(object sender, RoutedEventArgs e)
    {
        SelectedMode = "xml";
        DialogResult = true;
    }

    private void OnImportPaz(object sender, RoutedEventArgs e)
    {
        SelectedMode = "paz";
        DialogResult = true;
    }

    private void OnImportUcmPreset(object sender, RoutedEventArgs e)
    {
        SelectedMode = "ucmpreset";
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
