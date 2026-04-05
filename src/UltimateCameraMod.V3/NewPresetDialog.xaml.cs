using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UltimateCameraMod.V3;

public partial class NewPresetDialog : UserControl
{
    public string PresetName { get; private set; } = "";
    public string AuthorName { get; private set; } = "";
    public string Description { get; private set; } = "";
    public bool IsManualPreset { get; private set; }

    public Action<NewPresetDialog>? OnResult;

    public NewPresetDialog()
    {
        InitializeComponent();
        NameBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        NameBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        AuthorBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        AuthorBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        DescriptionBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        DescriptionBox.CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));

        // Highlight selected card border
        RadioUcm.Checked += (_, _) =>
        {
            UcmCard.BorderBrush = (SolidColorBrush)FindResource("AccentBrush");
            ManualCard.BorderBrush = (SolidColorBrush)FindResource("BorderBrush");
        };
        RadioManual.Checked += (_, _) =>
        {
            ManualCard.BorderBrush = (SolidColorBrush)FindResource("AccentBrush");
            UcmCard.BorderBrush = (SolidColorBrush)FindResource("BorderBrush");
        };

        Loaded += (_, _) => NameBox.Focus();
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
        IsManualPreset = RadioManual.IsChecked == true;
        OnResult?.Invoke(this);
    }

    private void OnCancel(object sender, RoutedEventArgs e) => OnResult?.Invoke(null!);
}
