using System.Text;
using System.Windows;
using System.Windows.Media;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.V3;

public partial class ExportJsonDialog : Window
{
    private readonly string _gameDir;
    private readonly string? _sessionXml;
    private List<JsonModExporter.PatchChange>? _jsonLastPatches;
    private string? _jsonLastJson;

    public ExportJsonDialog(string gameDir, string? sessionXml)
    {
        _gameDir = gameDir;
        _sessionXml = sessionXml;
        InitializeComponent();
    }

    private void OnJsonGenerate(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir)) { SetStatus("Game folder not set.", true); return; }
        if (string.IsNullOrEmpty(_sessionXml)) { SetStatus("No session XML available. Edit settings first.", true); return; }
        var info = BuildJsonModInfo();
        var gameDir = _gameDir;
        var xml = _sessionXml!;
        RunJsonGenerate(() => JsonModExporter.ExportFromXml(gameDir, info, xml,
            msg => Dispatcher.Invoke(() => SetStatus(msg, false))));
    }

    private void OnJsonGenerateFromXml(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir)) { SetStatus("Game folder not set.", true); return; }

        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select camera XML to patch",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        string xmlPath = ofd.FileName;
        var info = BuildJsonModInfo();
        var gameDir = _gameDir;
        RunJsonGenerate(() =>
        {
            string xml = File.ReadAllText(xmlPath);
            return JsonModExporter.ExportFromXml(gameDir, info, xml,
                msg => Dispatcher.Invoke(() => SetStatus(msg, false)));
        });
    }

    private void RunJsonGenerate(Func<(List<JsonModExporter.PatchChange>, string)> work)
    {
        SetStatus("Generating patches...", false);
        JsonPreviewPanel.Visibility = Visibility.Collapsed;
        _jsonLastPatches = null;
        _jsonLastJson = null;

        Task.Run(() =>
        {
            try
            {
                var (changes, json) = work();
                Dispatcher.Invoke(() =>
                {
                    _jsonLastPatches = changes;
                    _jsonLastJson = json;
                    JsonPatchCountLabel.Text = changes.Count.ToString();
                    JsonBytesChangedLabel.Text = changes.Sum(c => c.Original.Length / 2).ToString();
                    JsonPreviewPanel.Visibility = Visibility.Visible;
                    SetStatus($"Generated {changes.Count} patch regions. Click Save .json to export.", false);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SetStatus($"Generate failed: {ex.Message}", true));
            }
        });
    }

    private void OnJsonSave(object sender, RoutedEventArgs e)
    {
        if (_jsonLastJson == null) { SetStatus("Generate a patch first.", false); return; }

        string title = JsonTitleBox.Text.Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "ucm_patch"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Save JSON Patch",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = $"{safeName}.json"
        };
        if (sfd.ShowDialog(this) != true) return;

        try
        {
            File.WriteAllText(sfd.FileName, _jsonLastJson, new UTF8Encoding(false));
            SetStatus($"Saved {Path.GetFileName(sfd.FileName)} ({_jsonLastPatches!.Count} patches).", false);
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {ex.Message}", true);
        }
    }

    private JsonModExporter.ModInfo BuildJsonModInfo() => new(
        Title: JsonTitleBox.Text.Trim().Length > 0 ? JsonTitleBox.Text.Trim() : "UCM Camera Config",
        Version: JsonVersionBox.Text.Trim().Length > 0 ? JsonVersionBox.Text.Trim() : "1.0",
        Author: JsonAuthorBox.Text.Trim(),
        Description: JsonDescBox.Text.Trim(),
        NexusUrl: JsonNexusBox.Text.Trim().Length > 0 ? JsonNexusBox.Text.Trim()
            : "https://www.nexusmods.com/crimsondesert/mods/438");

    private void SetStatus(string msg, bool isError)
    {
        StatusLabel.Text = msg;
        StatusLabel.Foreground = isError
            ? (Brush)FindResource("ErrorBrush")
            : (Brush)FindResource("TextDimBrush");
    }
}
