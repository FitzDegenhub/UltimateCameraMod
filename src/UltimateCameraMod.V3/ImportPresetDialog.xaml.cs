using System.Windows;

namespace UltimateCameraMod.V3;

public partial class ImportPresetDialog : Window
{
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

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
