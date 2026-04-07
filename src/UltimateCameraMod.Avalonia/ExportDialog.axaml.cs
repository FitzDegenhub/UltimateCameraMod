using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace UltimateCameraMod.Avalonia;

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

    // Avalonia requires a parameterless constructor for the XAML loader
    public ExportDialog() : this("", "") { }

    private async void OnCopy(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
            await clipboard.SetTextAsync(CodeBox.Text);

        CopyBtn.Content = "Copied!";
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1500)
        };
        timer.Tick += (_, _) => { CopyBtn.Content = "Copy to clipboard"; timer.Stop(); };
        timer.Start();
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
