using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace UltimateCameraMod.Avalonia;

public partial class AdvancedImportDialog : Window
{
    public Dictionary<string, string>? Result { get; private set; }
    public bool Confirmed { get; private set; }

    public AdvancedImportDialog()
    {
        InitializeComponent();
        PasteBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        PasteBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));

        PasteBox.TextChanged += OnTextChanged;
        Opened += (_, _) => PasteBox.Focus();
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        string raw = (PasteBox.Text ?? "").Trim();
        if (string.IsNullOrEmpty(raw))
        {
            PreviewText.Text = "";
            ImportBtn.IsEnabled = false;
            return;
        }

        try
        {
            var decoded = Decode(raw);
            PreviewText.Text = $"\u2714  {decoded.Count} advanced settings found";
            PreviewText.Foreground = (IBrush?)this.FindResource("SuccessBrush") ?? Brushes.Green;
            ImportBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            PreviewText.Text = $"\u2716  {ex.Message}";
            PreviewText.Foreground = (IBrush?)this.FindResource("ErrorBrush") ?? Brushes.Red;
            ImportBtn.IsEnabled = false;
        }
    }

    private void OnImport(object? sender, RoutedEventArgs e)
    {
        try
        {
            Result = Decode((PasteBox.Text ?? "").Trim());
            Confirmed = true;
            Close();
        }
        catch { }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }

    private static Dictionary<string, string> Decode(string input)
    {
        if (!input.StartsWith("UCM_ADV:"))
            throw new FormatException("Not an advanced settings string (must start with UCM_ADV:)");

        string b64 = input["UCM_ADV:".Length..];
        byte[] bytes = Convert.FromBase64String(b64);
        string json = Encoding.UTF8.GetString(bytes);

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? throw new FormatException("Invalid payload");
    }
}
