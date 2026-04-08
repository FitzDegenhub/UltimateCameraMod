using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UltimateCameraMod.V3.Localization;

namespace UltimateCameraMod.V3;

public partial class AdvancedImportDialog : Window
{
    private static string L(string key) => TranslationSource.Instance[key];
    public Dictionary<string, string>? Result { get; private set; }

    public AdvancedImportDialog()
    {
        InitializeComponent();
        PasteBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        PasteBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        PasteBox.Focus();
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        string raw = PasteBox.Text.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            PreviewText.Text = "";
            ImportBtn.IsEnabled = false;
            return;
        }

        try
        {
            var decoded = Decode(raw);
            PreviewText.Text = $"\u2714  {string.Format(L("Dlg_AdvSettingsFound"), decoded.Count)}";
            PreviewText.Foreground = (Brush)FindResource("SuccessBrush");
            ImportBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            PreviewText.Text = $"\u2716  {ex.Message}";
            PreviewText.Foreground = (Brush)FindResource("ErrorBrush");
            ImportBtn.IsEnabled = false;
        }
    }

    private void OnImport(object sender, RoutedEventArgs e)
    {
        try
        {
            Result = Decode(PasteBox.Text.Trim());
            DialogResult = true;
        }
        catch { }
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;

    private static Dictionary<string, string> Decode(string input)
    {
        if (!input.StartsWith("UCM_ADV:"))
            throw new FormatException(L("Dlg_NotAdvString"));

        string b64 = input["UCM_ADV:".Length..];
        byte[] bytes = Convert.FromBase64String(b64);
        string json = Encoding.UTF8.GetString(bytes);

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? throw new FormatException(L("Dlg_InvalidPayload"));
    }
}

