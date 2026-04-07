using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace UltimateCameraMod.Avalonia;

public partial class ImportMetadataDialog : UserControl
{
    public string PresetName { get; private set; } = "";
    public string? PresetAuthor { get; private set; }
    public string? PresetDescription { get; private set; }
    public string? PresetUrl { get; private set; }

    public Action<ImportMetadataDialog?>? OnResult;

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

        AttachedToVisualTree += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    // Avalonia requires a parameterless constructor for the XAML loader
    public ImportMetadataDialog() : this("", "") { }

    private void OnConfirmImport(object? sender, RoutedEventArgs e)
    {
        string name = (NameBox.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            // Simple inline feedback instead of WPF MessageBox
            NameBox.BorderBrush = new SolidColorBrush(Color.Parse("#ef4444"));
            NameBox.Focus();
            return;
        }

        PresetName = name;
        PresetAuthor = string.IsNullOrWhiteSpace(AuthorBox.Text) ? null : (AuthorBox.Text ?? "").Trim();
        PresetDescription = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : (DescriptionBox.Text ?? "").Trim();
        PresetUrl = string.IsNullOrWhiteSpace(UrlBox.Text) ? null : (UrlBox.Text ?? "").Trim();
        OnResult?.Invoke(this);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => OnResult?.Invoke(null);
}
