using System.Windows;
using System.Windows.Media;

namespace UltimateCameraMod.V3;

public partial class NewPresetDialog : Window
{
    public string PresetName { get; private set; } = "";
    public string AuthorName { get; private set; } = "";
    public string Description { get; private set; } = "";

    public NewPresetDialog()
    {
        InitializeComponent();
        NameBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        NameBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        AuthorBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        AuthorBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        DescriptionBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        DescriptionBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        NameBox.Focus();
    }

    private void OnCreate(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Please enter a name.", "New Preset", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        PresetName = NameBox.Text.Trim();
        AuthorName = AuthorBox.Text.Trim();
        Description = DescriptionBox.Text.Trim();
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
