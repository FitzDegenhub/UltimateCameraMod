using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace UltimateCameraMod.Avalonia;

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
        RadioUcm.IsCheckedChanged += (_, _) =>
        {
            if (RadioUcm.IsChecked == true)
            {
                UcmCard.BorderBrush = (IBrush?)this.FindResource("AccentBrush") ?? Brushes.Gold;
                ManualCard.BorderBrush = (IBrush?)this.FindResource("BorderBrush") ?? Brushes.Gray;
            }
        };
        RadioManual.IsCheckedChanged += (_, _) =>
        {
            if (RadioManual.IsChecked == true)
            {
                ManualCard.BorderBrush = (IBrush?)this.FindResource("AccentBrush") ?? Brushes.Gold;
                UcmCard.BorderBrush = (IBrush?)this.FindResource("BorderBrush") ?? Brushes.Gray;
            }
        };

        AttachedToVisualTree += (_, _) => NameBox.Focus();
    }

    private void OnCreate(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            // Simple inline feedback instead of WPF MessageBox
            NameBox.BorderBrush = new SolidColorBrush(Color.Parse("#ef4444"));
            NameBox.Focus();
            return;
        }
        PresetName = (NameBox.Text ?? "").Trim();
        AuthorName = (AuthorBox.Text ?? "").Trim();
        Description = (DescriptionBox.Text ?? "").Trim();
        IsManualPreset = RadioManual.IsChecked == true;
        OnResult?.Invoke(this);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => OnResult?.Invoke(null!);
}
