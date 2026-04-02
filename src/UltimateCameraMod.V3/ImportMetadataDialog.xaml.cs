using System.Windows;
using System.Windows.Media;

namespace UltimateCameraMod.V3;

public partial class ImportMetadataDialog : Window
{
    public string PresetName { get; private set; } = "";
    public string? PresetAuthor { get; private set; }
    public string? PresetDescription { get; private set; }
    public string? PresetUrl { get; private set; }

    public ImportMetadataDialog(string sourceHint, string suggestedName,
        string? author = null, string? description = null, string? url = null)
    {
        InitializeComponent();
        SourceHintLabel.Text = sourceHint;
        NameBox.Text = suggestedName;
        AuthorBox.Text = author ?? "";
        DescriptionBox.Text = description ?? "";
        UrlBox.Text = url ?? "";

        var fg = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        var caret = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        foreach (var tb in new[] { NameBox, AuthorBox, DescriptionBox, UrlBox })
        {
            tb.Foreground = fg;
            tb.CaretBrush = caret;
        }

        Loaded += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    private void OnConfirmImport(object sender, RoutedEventArgs e)
    {
        string name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a name for this preset.", "Name required",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            NameBox.Focus();
            return;
        }

        PresetName = name;
        PresetAuthor = string.IsNullOrWhiteSpace(AuthorBox.Text) ? null : AuthorBox.Text.Trim();
        PresetDescription = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
        PresetUrl = string.IsNullOrWhiteSpace(UrlBox.Text) ? null : UrlBox.Text.Trim();
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
