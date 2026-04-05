using System.Windows;
using System.Windows.Media;

namespace UltimateCameraMod.V3;

public partial class InputDialog : Window
{
    public string ResponseText { get; private set; } = "";
    public string InitialText
    {
        get => InputBox.Text;
        set => InputBox.Text = value;
    }

    public InputDialog(string title, string prompt)
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = prompt;
        InputBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        InputBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        InputBox.Focus();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        ResponseText = InputBox.Text;
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}

