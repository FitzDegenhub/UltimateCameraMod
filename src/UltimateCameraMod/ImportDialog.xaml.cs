using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UltimateCameraMod.Models;

namespace UltimateCameraMod;

public partial class ImportDialog : Window
{
    public (string Name, double Distance, double Height, double RightOffset)? Result { get; private set; }

    public ImportDialog()
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
            var (n, d, h, r) = PresetCodec.Decode(raw);
            PreviewText.Text = $"\u2714  {n}  |  Dist {d:F1}  |  Height {h:+0.00}  |  Shift {r:+0.00}";
            PreviewText.Foreground = (Brush)FindResource("SuccessBrush");
            ImportBtn.IsEnabled = true;
        }
        catch (FormatException ex)
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
            Result = PresetCodec.Decode(PasteBox.Text.Trim());
            DialogResult = true;
        }
        catch { }
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
