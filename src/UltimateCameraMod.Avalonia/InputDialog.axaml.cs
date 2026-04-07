using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace UltimateCameraMod.Avalonia;

public partial class InputDialog : Window
{
    public string ResponseText { get; private set; } = "";
    public bool Confirmed { get; private set; }

    public string InitialText
    {
        get => InputBox.Text ?? "";
        set => InputBox.Text = value;
    }

    public InputDialog(string title, string prompt)
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = prompt;
        InputBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        InputBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        Opened += (_, _) => InputBox.Focus();
    }

    // Avalonia requires a parameterless constructor for the XAML loader
    public InputDialog() : this("Input", "") { }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        ResponseText = InputBox.Text ?? "";
        Confirmed = true;
        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }
}
