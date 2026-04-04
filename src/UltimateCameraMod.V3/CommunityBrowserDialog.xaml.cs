using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UltimateCameraMod.V3;

public partial class CommunityBrowserDialog : UserControl
{
    public Action? OnCloseRequested;
    private const long MaxPresetSize = 2 * 1024 * 1024; // 2MB

    private readonly string _catalogUrl;
    private readonly string _rawBaseUrl;
    private readonly string _presetsDir;
    private readonly Action _onPresetsChanged;
    private readonly bool _needsSessionXmlBake;
    private List<CatalogEntry>? _catalog;

    private sealed class CatalogEntry
    {
        public string Id { get; set; } = "";
        public string File { get; set; } = "";
        public string Name { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string Url { get; set; } = "";
        public string Version { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public long SizeBytes { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public CommunityBrowserDialog(string presetsDir, Action onPresetsChanged,
        string catalogUrl = "https://raw.githubusercontent.com/FitzDegenhub/ucm-community-presets/main/catalog.json",
        string rawBaseUrl = "https://raw.githubusercontent.com/FitzDegenhub/ucm-community-presets/main/",
        string title = "Community Presets",
        bool needsSessionXmlBake = false)
    {
        _presetsDir = presetsDir;
        _onPresetsChanged = onPresetsChanged;
        _catalogUrl = catalogUrl;
        _rawBaseUrl = rawBaseUrl;
        _needsSessionXmlBake = needsSessionXmlBake;
        InitializeComponent();
        HeaderTitle.Text = title.ToUpperInvariant();
        if (needsSessionXmlBake)
            HeaderSubtitle.Text = "Browse and download official UCM camera presets. Downloaded presets appear in your sidebar.";
        Loaded += async (_, _) => await FetchCatalogAsync();
    }

    private async Task FetchCatalogAsync()
    {
        ShowLoading("Fetching community catalog...");
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("UltimateCameraMod/3.0");
            http.Timeout = TimeSpan.FromSeconds(10);

            string json = await http.GetStringAsync(_catalogUrl);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var entries = new List<CatalogEntry>();
            if (root.TryGetProperty("presets", out var presetsArr) && presetsArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in presetsArr.EnumerateArray())
                {
                    entries.Add(new CatalogEntry
                    {
                        Id = el.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
                        File = el.TryGetProperty("file", out var f) ? f.GetString() ?? "" : "",
                        Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                        Author = el.TryGetProperty("author", out var a) ? a.GetString() ?? "" : "",
                        Description = el.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "",
                        Url = el.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "",
                        Version = el.TryGetProperty("version", out var v) ? v.GetString() ?? "" : "",
                        Sha256 = el.TryGetProperty("sha256", out var s) ? s.GetString() ?? "" : "",
                        SizeBytes = el.TryGetProperty("size_bytes", out var sz) && sz.ValueKind == JsonValueKind.Number ? sz.GetInt64() : 0,
                        Tags = el.TryGetProperty("tags", out var t) && t.ValueKind == JsonValueKind.Array
                            ? t.EnumerateArray().Select(x => x.GetString() ?? "").ToArray()
                            : Array.Empty<string>()
                    });
                }
            }

            _catalog = entries;
            RenderPresetList();
        }
        catch (Exception ex)
        {
            ShowError($"Could not fetch community catalog.\n\n{ex.Message}");
        }
    }

    private void RenderPresetList()
    {
        if (_catalog == null || _catalog.Count == 0)
        {
            ShowError("No community presets available yet.\n\nCheck back later or contribute your own!");
            return;
        }

        PresetListPanel.Children.Clear();
        Directory.CreateDirectory(_presetsDir);

        int downloadedCount = 0;
        foreach (var entry in _catalog)
        {
            bool isDownloaded = IsPresetDownloaded(entry);
            if (isDownloaded) downloadedCount++;

            var card = BuildPresetCard(entry, isDownloaded);
            PresetListPanel.Children.Add(card);
        }

        ShowContent();
        StatusText.Text = $"{_catalog.Count} presets available, {downloadedCount} downloaded";
    }

    private bool IsPresetDownloaded(CatalogEntry entry)
    {
        // UCM presets use the catalog filename; community presets use the entry id
        if (_needsSessionXmlBake && !string.IsNullOrEmpty(entry.File))
        {
            string path = Path.Combine(_presetsDir, entry.File);
            return System.IO.File.Exists(path);
        }
        if (!string.IsNullOrEmpty(entry.File))
        {
            string path = Path.Combine(_presetsDir, Path.GetFileName(entry.File));
            if (System.IO.File.Exists(path)) return true;
        }
        string idPath = Path.Combine(_presetsDir, $"{entry.Id}.ucmpreset");
        return System.IO.File.Exists(idPath);
    }

    private Border BuildPresetCard(CatalogEntry entry, bool isDownloaded)
    {
        var nameBlock = new TextBlock
        {
            Text = entry.Name,
            FontSize = 14, FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        };

        var authorBlock = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(entry.Author) ? "" : $"by {entry.Author}",
            FontSize = 10,
            Foreground = (Brush)FindResource("TextDimBrush"),
            Margin = new Thickness(0, 2, 0, 0)
        };

        var descBlock = new TextBlock
        {
            Text = entry.Description.Length > 200 ? entry.Description[..197] + "..." : entry.Description,
            FontSize = 11, TextWrapping = TextWrapping.Wrap,
            Foreground = (Brush)FindResource("TextSecondaryBrush"),
            Margin = new Thickness(0, 6, 0, 0)
        };

        // Tags
        var tagsPanel = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
        foreach (string tag in entry.Tags)
        {
            var tagBorder = new Border
            {
                Background = (Brush)FindResource("BgInputBrush"),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(0, 0, 4, 0),
                Child = new TextBlock
                {
                    Text = tag, FontSize = 9,
                    Foreground = (Brush)FindResource("TextDimBrush")
                }
            };
            tagsPanel.Children.Add(tagBorder);
        }

        // Left side: name, author, description, tags
        var leftStack = new StackPanel();
        leftStack.Children.Add(nameBlock);
        if (!string.IsNullOrWhiteSpace(entry.Author)) leftStack.Children.Add(authorBlock);
        leftStack.Children.Add(descBlock);
        if (entry.Tags.Length > 0) leftStack.Children.Add(tagsPanel);

        // Right side: action buttons stacked vertically
        var actionBtn = new Button
        {
            Height = 30, FontSize = 11, Padding = new Thickness(16, 0, 16, 0),
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Right,
            Tag = entry
        };

        if (isDownloaded)
        {
            actionBtn.Content = "Downloaded \u2714";
            actionBtn.Style = (Style)FindResource("SubtleButton");
            actionBtn.IsEnabled = false;
        }
        else
        {
            actionBtn.Content = "Download";
            actionBtn.Style = (Style)FindResource("AccentButton");
            actionBtn.Click += OnDownloadClick;
        }

        var rightStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(16, 0, 0, 0),
            MinWidth = 110
        };
        rightStack.Children.Add(actionBtn);

        if (!string.IsNullOrWhiteSpace(entry.Url))
        {
            var linkBtn = new Button
            {
                Content = "\uD83D\uDD17 Nexus",
                Height = 28, FontSize = 10, Padding = new Thickness(10, 0, 10, 0),
                MinWidth = 110,
                HorizontalAlignment = HorizontalAlignment.Right,
                Style = (Style)FindResource("SubtleButton"),
                Foreground = (Brush)FindResource("AccentBrush"),
                BorderBrush = (Brush)FindResource("AccentBrush"),
                Margin = new Thickness(0, 6, 0, 0),
                Tag = entry.Url
            };
            linkBtn.Click += (_, _) =>
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(entry.Url) { UseShellExecute = true }); }
                catch { }
            };
            rightStack.Children.Add(linkBtn);
        }

        // Layout: left content takes available space, right buttons fixed
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(leftStack, 0);
        Grid.SetColumn(rightStack, 1);
        grid.Children.Add(leftStack);
        grid.Children.Add(rightStack);

        return new Border
        {
            Background = (Brush)FindResource("BgPanelBrush"),
            BorderBrush = (Brush)FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 14, 16, 14),
            Margin = new Thickness(0, 0, 0, 10),
            Child = grid
        };
    }

    private async void OnDownloadClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not CatalogEntry entry) return;

        btn.IsEnabled = false;
        btn.Content = "Downloading...";

        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("UltimateCameraMod/3.0");
            http.Timeout = TimeSpan.FromSeconds(15);

            string downloadUrl = _rawBaseUrl + entry.File;
            byte[] rawBytes = await http.GetByteArrayAsync(downloadUrl);

            if (rawBytes.Length > MaxPresetSize)
            {
                btn.Content = "Too large";
                StatusText.Text = $"Preset '{entry.Name}' exceeds 2MB limit.";
                return;
            }

            // Validate
            string content = System.Text.Encoding.UTF8.GetString(rawBytes);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            bool hasSessionXml = root.TryGetProperty("session_xml", out _) || root.TryGetProperty("RawXml", out _);
            bool hasStyleId = root.TryGetProperty("style_id", out _);
            if (!hasSessionXml && !hasStyleId && !_needsSessionXmlBake)
            {
                btn.Content = "Invalid";
                StatusText.Text = $"Preset '{entry.Name}' doesn't contain camera data.";
                return;
            }

            // Save raw bytes as-is to preserve SHA hash for update detection.
            // Preset files in the repo should already have metadata (name, author, url, description).
            Directory.CreateDirectory(_presetsDir);
            string destFileName = _needsSessionXmlBake && !string.IsNullOrEmpty(entry.File)
                ? entry.File
                : !string.IsNullOrEmpty(entry.File)
                    ? Path.GetFileName(entry.File)
                    : $"{entry.Id}.ucmpreset";
            string destPath = Path.Combine(_presetsDir, destFileName);
            await System.IO.File.WriteAllBytesAsync(destPath, rawBytes);

            // Update UCM sidecar with the catalog SHA so update detection works
            if (!string.IsNullOrEmpty(entry.Sha256) && _presetsDir.Contains("ucm_presets"))
            {
                try
                {
                    string statePath = Path.Combine(_presetsDir, ".catalog_state.json");
                    var state = new Dictionary<string, string>();
                    if (System.IO.File.Exists(statePath))
                    {
                        string stateJson = System.IO.File.ReadAllText(statePath);
                        state = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(stateJson) ?? new();
                    }
                    state[destFileName] = entry.Sha256;
                    System.IO.File.WriteAllText(statePath,
                        System.Text.Json.JsonSerializer.Serialize(state, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                }
                catch { }
            }

            btn.Content = "Downloaded \u2714";
            btn.Style = (Style)FindResource("SubtleButton");
            StatusText.Text = $"Downloaded '{entry.Name}'";

            _onPresetsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            btn.Content = "Failed";
            btn.IsEnabled = true;
            StatusText.Text = $"Download failed: {ex.Message}";
        }
    }

    private void ShowLoading(string text)
    {
        LoadingPanel.Visibility = Visibility.Visible;
        LoadingText.Text = text;
        ErrorPanel.Visibility = Visibility.Collapsed;
        PresetListScroller.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string text)
    {
        LoadingPanel.Visibility = Visibility.Collapsed;
        ErrorPanel.Visibility = Visibility.Visible;
        ErrorText.Text = text;
        PresetListScroller.Visibility = Visibility.Collapsed;
    }

    private void ShowContent()
    {
        LoadingPanel.Visibility = Visibility.Collapsed;
        ErrorPanel.Visibility = Visibility.Collapsed;
        PresetListScroller.Visibility = Visibility.Visible;
    }

    private async void OnRetry(object sender, RoutedEventArgs e) => await FetchCatalogAsync();
    private async void OnRefresh(object sender, RoutedEventArgs e) => await FetchCatalogAsync();
    private void OnClose(object sender, RoutedEventArgs e) => OnCloseRequested?.Invoke();
}
