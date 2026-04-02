using System.Windows;
using System.Windows.Media;

namespace UltimateCameraMod.V3;

public partial class ExportDialog : Window
{
    private static readonly SolidColorBrush LightText = new(Color.FromRgb(0xe0, 0xe0, 0xe0));
    private static readonly SolidColorBrush AccentText = new(Color.FromRgb(0xd4, 0xa5, 0x37));

    public ExportDialog(string name, string code)
    {
        InitializeComponent();
        TitleText.Text = $"Share this string for \"{name}\":";
        CodeBox.Text = code;
        CodeBox.Foreground = AccentText;
        CodeBox.CaretBrush = LightText;
        CodeBox.SelectAll();
    }

    private void OnCopy(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(CodeBox.Text);
        CopyBtn.Content = "Copied!";
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1500)
        };
        timer.Tick += (_, _) => { CopyBtn.Content = "Copy to clipboard"; timer.Stop(); };
        timer.Start();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}

