using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.Avalonia;

public partial class MainWindow : Window
{
    private TaskCompletionSource<object?>? _overlayTcs;
    private bool _overlayIsFatal;

    // ── Core overlay show/close ─────────────────────────────────────

    /// <summary>
    /// Shows a UI element as a centered overlay dialog. Returns when CloseOverlay is called.
    /// </summary>
    private Task<object?> ShowOverlayAsync(FrameworkElement content, double? width = null, double? height = null)
    {
        _overlayTcs = new TaskCompletionSource<object?>();

        // Wrap content with an X close button in top-right (unless fatal overlay)
        FrameworkElement wrapped;
        if (!_overlayIsFatal)
        {
            var closeX = new Button
            {
                Content = "\u2715",
                FontSize = 16,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Padding = new Thickness(4, 0, 4, 0),
                Margin = new Thickness(0, -8, -12, 0)
            };
            closeX.Click += (_, _) => CloseOverlay(null);
            // Hover effect
            closeX.MouseEnter += (_, _) => closeX.Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
            closeX.MouseLeave += (_, _) => closeX.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));

            var grid = new Grid();
            grid.Children.Add(content);
            grid.Children.Add(closeX);
            wrapped = grid;
        }
        else
        {
            wrapped = content;
        }

        OverlayContent.Content = wrapped;

        if (width.HasValue)
            OverlayCard.Width = width.Value;
        else
            OverlayCard.ClearValue(WidthProperty);

        if (height.HasValue)
            OverlayCard.MaxHeight = height.Value;
        else
            OverlayCard.MaxHeight = 850;

        OverlayHost.IsVisible = true;

        // Focus the overlay so keyboard events work
        content.Dispatcher.UIThread.InvokeAsync(new Action(() =>
        {
            content.Focus();
            content.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        return _overlayTcs.Task;
    }

    /// <summary>
    /// Closes the overlay and returns a result to the caller.
    /// </summary>
    private void CloseOverlay(object? result = null)
    {
        OverlayHost.IsVisible = false;
        OverlayContent.Content = null;
        OverlayCard.ClearValue(WidthProperty);
        _overlayIsFatal = false;
        _overlayTcs?.TrySetResult(result);
        _overlayTcs = null;
    }

    private void OnOverlayBackdropClick(object sender, PointerPressedEventArgs e)
    {
        // Backdrop clicks do nothing — users must use the overlay buttons
    }

    // ── Shared UI builders ──────────────────────────────────────────

    private static TextBlock OverlayTitle(string text) => new()
    {
        Text = text,
        FontSize = 18,
        FontWeight = FontWeights.SemiBold,
        Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0)),
        Margin = new Thickness(0, 0, 0, 12)
    };

    private static TextBlock OverlaySubtext(string text) => new()
    {
        Text = text,
        FontSize = 12,
        TextWrapping = TextWrapping.Wrap,
        Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)),
        Margin = new Thickness(0, 0, 0, 16)
    };

    private Button OverlayPrimaryButton(string text) => new()
    {
        Content = text,
        Style = (Style)FindResource("AccentButton"),
        Height = 32,
        FontSize = 12,
        Padding = new Thickness(20, 0, 20, 0),
        MinWidth = 100,
        Margin = new Thickness(0, 0, 8, 0)
    };

    private Button OverlayCancelButton(string text = "Cancel") => new()
    {
        Content = text,
        Style = (Style)FindResource("SubtleButton"),
        Height = 32,
        FontSize = 12,
        Padding = new Thickness(20, 0, 20, 0),
        MinWidth = 80
    };

    private static StackPanel OverlayButtonRow(params Button[] buttons)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0)
        };
        foreach (var btn in buttons)
            row.Children.Add(btn);
        return row;
    }

    private TextBox OverlayTextBox(string placeholder = "", string initialText = "")
    {
        var tb = new TextBox
        {
            Text = initialText,
            FontSize = 18,
            MinHeight = 54,
            Padding = new Thickness(12, 14, 12, 14),
            Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            BorderThickness = new Thickness(1),
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        tb.CaretBrush = Brushes.White;
        return tb;
    }

    private static TextBox OverlayMultiLineTextBox(string initialText = "", int lines = 4) => new()
    {
        Text = initialText,
        FontSize = 12,
        FontFamily = new FontFamily("Consolas"),
        Background = new SolidColorBrush(Color.FromRgb(0x1a, 0x1a, 0x1a)),
        Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0)),
        CaretBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0)),
        BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
        BorderThickness = new Thickness(1),
        Padding = new Thickness(8, 6, 8, 6),
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        MinHeight = lines * 20
    };

    private static TextBlock OverlayLabel(string text) => new()
    {
        Text = text,
        FontSize = 12,
        Foreground = new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0xaa)),
        Margin = new Thickness(0, 8, 0, 4)
    };

    // ── Tier 1: Simple overlay dialogs ──────────────────────────────

    /// <summary>Shows a text input overlay (replaces InputDialog). Returns null if cancelled.</summary>
    private async Task<string?> ShowInputOverlayAsync(string title, string prompt, string initialText = "")
    {
        var stack = new StackPanel { Width = 400 };
        stack.Children.Add(OverlayTitle(title));
        stack.Children.Add(OverlayLabel(prompt));

        var textBox = OverlayTextBox(initialText: initialText);
        stack.Children.Add(textBox);

        var okBtn = OverlayPrimaryButton("OK");
        var cancelBtn = OverlayCancelButton();

        okBtn.Click += (_, _) => CloseOverlay(textBox.Text?.Trim());
        cancelBtn.Click += (_, _) => CloseOverlay(null);
        textBox.KeyDown += (_, e) => { if (e.Key == Key.Enter) CloseOverlay(textBox.Text?.Trim()); };
        textBox.KeyDown += (_, e) => { if (e.Key == Key.Escape) CloseOverlay(null); };

        stack.Children.Add(OverlayButtonRow(okBtn, cancelBtn));

        var result = await ShowOverlayAsync(stack, width: 460);
        return result as string;
    }

    /// <summary>Shows a read-only text display with copy button (replaces ExportDialog).</summary>
    private async Task ShowExportStringOverlayAsync(string title, string text)
    {
        var stack = new StackPanel { Width = 500 };
        stack.Children.Add(OverlayTitle(title));

        var textBox = OverlayMultiLineTextBox(text, lines: 6);
        textBox.IsReadOnly = true;
        stack.Children.Add(textBox);

        var copyBtn = OverlayPrimaryButton("Copy to clipboard");
        var closeBtn = OverlayCancelButton("Close");

        copyBtn.Click += (_, _) =>
        {
            Clipboard.SetText(text);
            copyBtn.Content = "Copied!";
        };
        closeBtn.Click += (_, _) => CloseOverlay(null);

        stack.Children.Add(OverlayButtonRow(copyBtn, closeBtn));

        await ShowOverlayAsync(stack, width: 560);
    }

    /// <summary>Shows import source type selection (replaces ImportPresetDialog). Returns mode string or null.</summary>
    private async Task<string?> ShowImportTypeOverlayAsync()
    {
        var stack = new StackPanel { Width = 420 };
        stack.Children.Add(OverlayTitle("Import Preset"));
        stack.Children.Add(OverlaySubtext("Choose what type of file you want to import."));

        string? selectedMode = null;

        void AddImportButton(string label, string description, string mode)
        {
            var content = new StackPanel { Margin = new Thickness(2, 4, 2, 4) };
            content.Children.Add(new TextBlock
            {
                Text = label, FontSize = 13, FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0))
            });
            content.Children.Add(new TextBlock
            {
                Text = description, FontSize = 10, Margin = new Thickness(0, 2, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88))
            });

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x2a, 0x2a, 0x2a)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3a, 0x3a, 0x3a)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 6),
                Cursor = Cursors.Hand,
                Child = content
            };
            border.MouseEnter += (_, _) => border.BorderBrush = (Brush)FindResource("AccentBrush");
            border.MouseLeave += (_, _) => border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3a, 0x3a, 0x3a));
            border.MouseDown += (_, _) => CloseOverlay(mode);
            stack.Children.Add(border);
        }

        AddImportButton("Mod Manager Package", "JSON patch from JSON Mod Manager or CDUMM", "mod_package");
        AddImportButton("Raw XML", "playercamerapreset.xml file", "xml");
        AddImportButton("PAZ Archive", "Game .paz file with camera data", "paz");
        AddImportButton(".ucmpreset File", "UCM preset file from another user", "ucmpreset");

        var result = await ShowOverlayAsync(stack, width: 480);
        return result as string;
    }

    /// <summary>Shows advanced import (UCM_ADV: base64 string) overlay. Returns decoded dict or null.</summary>
    private async Task<Dictionary<string, string>?> ShowAdvancedImportOverlayAsync()
    {
        var stack = new StackPanel { Width = 480 };
        stack.Children.Add(OverlayTitle("Import God Mode Overrides"));
        stack.Children.Add(OverlaySubtext("Paste the UCM_ADV: string to import advanced settings."));

        var textBox = OverlayMultiLineTextBox(lines: 5);
        stack.Children.Add(textBox);

        var previewText = new TextBlock
        {
            FontSize = 11,
            Margin = new Thickness(0, 8, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88))
        };
        stack.Children.Add(previewText);

        var importBtn = OverlayPrimaryButton("Import");
        importBtn.IsEnabled = false;
        var cancelBtn = OverlayCancelButton();

        Dictionary<string, string>? decoded = null;

        textBox.TextChanged += (_, _) =>
        {
            string raw = textBox.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(raw))
            {
                previewText.Text = "";
                importBtn.IsEnabled = false;
                decoded = null;
                return;
            }
            try
            {
                if (!raw.StartsWith("UCM_ADV:"))
                    throw new FormatException("Must start with UCM_ADV:");
                string b64 = raw["UCM_ADV:".Length..];
                byte[] bytes = Convert.FromBase64String(b64);
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                decoded = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? throw new FormatException("Invalid payload");
                previewText.Text = $"\u2714  {decoded.Count} advanced settings found";
                previewText.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                importBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                previewText.Text = $"\u2716  {ex.Message}";
                previewText.Foreground = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                importBtn.IsEnabled = false;
                decoded = null;
            }
        };

        importBtn.Click += (_, _) => CloseOverlay(decoded);
        cancelBtn.Click += (_, _) => CloseOverlay(null);

        stack.Children.Add(OverlayButtonRow(importBtn, cancelBtn));

        var result = await ShowOverlayAsync(stack, width: 540);
        return result as Dictionary<string, string>;
    }

    // ── Alert and confirmation overlays ────────────────────────────

    /// <summary>Shows an alert/error overlay with a single OK button. Returns when dismissed.</summary>
    private async Task ShowAlertOverlayAsync(string title, string message, string buttonText = "OK", bool isError = false)
    {
        var stack = new StackPanel { Width = 460 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = isError
                ? new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36))
                : new SolidColorBrush(Color.FromRgb(0xE6, 0xA2, 0x3C)),
            Margin = new Thickness(0, 0, 0, 12)
        });
        stack.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc)),
            Margin = new Thickness(0, 0, 0, 20)
        });

        var okBtn = OverlayPrimaryButton(buttonText);
        okBtn.Click += (_, _) => CloseOverlay();
        var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        row.Children.Add(okBtn);
        stack.Children.Add(row);

        await ShowOverlayAsync(stack, width: 520);
    }

    /// <summary>Shows a Yes/No confirmation overlay. Returns true if confirmed.</summary>
    private async Task<bool> ShowConfirmOverlayAsync(string title, string message,
        string confirmText = "Yes", string cancelText = "Cancel")
    {
        var stack = new StackPanel { Width = 460 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0)),
            Margin = new Thickness(0, 0, 0, 12)
        });
        stack.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc)),
            Margin = new Thickness(0, 0, 0, 20)
        });

        var confirmBtn = OverlayPrimaryButton(confirmText);
        var cancelBtn = OverlayCancelButton(cancelText);
        confirmBtn.Click += (_, _) => CloseOverlay(true);
        cancelBtn.Click += (_, _) => CloseOverlay(false);
        stack.Children.Add(OverlayButtonRow(confirmBtn, cancelBtn));

        var result = await ShowOverlayAsync(stack, width: 520);
        return result is true;
    }

    /// <summary>Shows a Yes/No/Cancel three-option overlay. Returns MessageBoxResult.</summary>
    private async Task<MessageBoxResult> ShowThreeChoiceOverlayAsync(string title, string message,
        string yesText = "Yes", string noText = "No", string cancelText = "Cancel")
    {
        var stack = new StackPanel { Width = 460 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0)),
            Margin = new Thickness(0, 0, 0, 12)
        });
        stack.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc)),
            Margin = new Thickness(0, 0, 0, 20)
        });

        var yesBtn = OverlayPrimaryButton(yesText);
        var noBtn = OverlayCancelButton(noText);
        noBtn.Margin = new Thickness(8, 0, 0, 0);
        var cancelBtn = OverlayCancelButton(cancelText);
        cancelBtn.Margin = new Thickness(8, 0, 0, 0);
        yesBtn.Click += (_, _) => CloseOverlay(MessageBoxResult.Yes);
        noBtn.Click += (_, _) => CloseOverlay(MessageBoxResult.No);
        cancelBtn.Click += (_, _) => CloseOverlay(MessageBoxResult.Cancel);

        var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
        row.Children.Add(yesBtn);
        row.Children.Add(noBtn);
        row.Children.Add(cancelBtn);
        stack.Children.Add(row);

        var result = await ShowOverlayAsync(stack, width: 520);
        return result is MessageBoxResult mr ? mr : MessageBoxResult.Cancel;
    }

    /// <summary>Shows a fatal error overlay that closes the app when dismissed. Cannot be dismissed by clicking backdrop.</summary>
    private void ShowFatalOverlayAndClose(string title, string message)
    {
        _overlayIsFatal = true;
        var stack = new StackPanel { Width = 500 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),
            Margin = new Thickness(0, 0, 0, 12)
        });
        stack.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc)),
            Margin = new Thickness(0, 0, 0, 20)
        });

        var closeBtn = new Button
        {
            Content = "Close UCM",
            Style = (Style)FindResource("AccentButton"),
            Height = 32,
            FontSize = 12,
            Padding = new Thickness(20, 0, 20, 0),
            MinWidth = 120
        };
        closeBtn.Click += (_, _) => Application.Current.Shutdown();

        var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        row.Children.Add(closeBtn);
        stack.Children.Add(row);

        _ = ShowOverlayAsync(stack, width: 560);
    }

    /// <summary>Shows game update warning as an overlay popup with Snooze/Dismiss buttons.</summary>
    private void ShowGameUpdateOverlay(string message)
    {
        // Don't stack overlays — if one is already showing, skip
        if (OverlayHost.IsVisible == true)
            return;

        var stack = new StackPanel { Width = 500 };

        // Warning icon + title
        var titleRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        titleRow.Children.Add(new TextBlock
        {
            Text = "\u26A0",
            FontSize = 22,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xA2, 0x3C)),
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        titleRow.Children.Add(new TextBlock
        {
            Text = "Game Update Detected",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xA2, 0x3C)),
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(titleRow);

        stack.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc)),
            Margin = new Thickness(0, 0, 0, 20)
        });

        var snoozeBtn = OverlayCancelButton("Snooze 7 days");
        var dismissBtn = OverlayPrimaryButton("Got it");

        snoozeBtn.Click += (_, _) =>
        {
            GameInstallBaselineTracker.SetSnooze(ExeDir, TimeSpan.FromDays(7));
            _gameUpdateNoticeSessionDismissed = true;
            CloseOverlay();
        };
        dismissBtn.Click += (_, _) =>
        {
            _gameUpdateNoticeSessionDismissed = true;
            CloseOverlay();
        };

        stack.Children.Add(OverlayButtonRow(dismissBtn, snoozeBtn));

        _ = ShowOverlayAsync(stack, width: 560);
    }

    /// <summary>Shows import metadata form overlay. Returns the dialog or null if cancelled.</summary>
    private async Task<ImportMetadataDialog?> ShowImportMetadataOverlayAsync(
        string sourceHint, string suggestedName,
        string? author = null, string? description = null, string? url = null)
    {
        var ctrl = new ImportMetadataDialog(sourceHint, suggestedName, author, description, url);
        var tcs = new TaskCompletionSource<ImportMetadataDialog?>();
        ctrl.OnResult = result => { CloseOverlay(null); tcs.TrySetResult(result); };
        _ = ShowOverlayAsync(ctrl, width: 520);
        return await tcs.Task;
    }

    /// <summary>Shows tainted backup overlay with option to delete 0.paz and instructions to verify on Steam.</summary>
    private async Task HandleTaintedBackupAsync()
    {
        var stack = new StackPanel { Width = 500 };

        stack.Children.Add(new TextBlock
        {
            Text = "Game Files Modified",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xA2, 0x3C)),
            Margin = new Thickness(0, 0, 0, 12)
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Your game camera files have been modified by UCM v2.x, another camera mod, or a mod manager. " +
                   "UCM needs the original vanilla files to create a clean backup.\n\n" +
                   "Click the button below to remove the modified file, then verify on Steam to re-download the original.",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc)),
            Margin = new Thickness(0, 0, 0, 16)
        });

        string? pazPath = null;
        try
        {
            var entry = CameraMod.FindCameraEntry(_gameDir);
            pazPath = entry.PazFile;
        }
        catch { }

        var deleteBtn = OverlayPrimaryButton("Remove modified 0.paz");
        var closeBtn = OverlayCancelButton("Close UCM");

        var statusText = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 12, 0, 0)
        };

        deleteBtn.Click += (_, _) =>
        {
            if (!string.IsNullOrEmpty(pazPath))
            {
                try
                {
                    File.Delete(pazPath);
                    deleteBtn.IsEnabled = false;
                    deleteBtn.Content = "Removed";
                    statusText.Text = "0.paz deleted. Now:\n" +
                        "1. Close UCM\n" +
                        "2. Steam \u2192 Crimson Desert \u2192 Properties \u2192 Installed Files \u2192 \"Verify integrity of game files\"\n" +
                        "3. Relaunch UCM after verification completes";
                    statusText.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                }
                catch (Exception ex)
                {
                    statusText.Text = $"Could not delete: {ex.Message}\nManually delete: {pazPath}";
                    statusText.Foreground = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                }
            }
        };

        closeBtn.Click += (_, _) => Application.Current.Shutdown();

        var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
        row.Children.Add(deleteBtn);
        row.Children.Add(new FrameworkElement { Width = 8 });
        row.Children.Add(closeBtn);
        stack.Children.Add(row);
        stack.Children.Add(statusText);

        _overlayIsFatal = true;
        await ShowOverlayAsync(stack, width: 560);
    }
}
