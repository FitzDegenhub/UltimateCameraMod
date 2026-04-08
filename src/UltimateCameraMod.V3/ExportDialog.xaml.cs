using System.Windows;
using System.Windows.Media;
using UltimateCameraMod.V3.Localization;

namespace UltimateCameraMod.V3;

public partial class ExportDialog : Window
{
    private static string L(string key) => TranslationSource.Instance[key];
    private static readonly SolidColorBrush LightText = new(Color.FromRgb(0xe0, 0xe0, 0xe0));
    private static readonly SolidColorBrush AccentText = new(Color.FromRgb(0xd4, 0xa5, 0x37));

    public ExportDialog(string name, string code)
    {
        InitializeComponent();
        TitleText.Text = string.Format(L("Dlg_ShareString"), name);
        CodeBox.Text = code;
        CodeBox.Foreground = AccentText;
        CodeBox.CaretBrush = LightText;
        CodeBox.SelectAll();
    }

    private void OnCopy(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(CodeBox.Text);
        CopyBtn.Content = L("Btn_Copied");
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1500)
        };
        timer.Tick += (_, _) => { CopyBtn.Content = L("Btn_CopyClipboard"); timer.Stop(); };
        timer.Start();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}

