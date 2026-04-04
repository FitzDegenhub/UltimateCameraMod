using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.V3;

public partial class ExportJsonDialog : UserControl
{
    public Action? OnCloseRequested;

    private readonly string _gameDir;
    private readonly Func<string?> _getSessionXml;

    private List<JsonModExporter.PatchChange>? _jsonLastPatches;
    private string? _jsonLastJson;
    private string? _preparedXml;

    public ExportJsonDialog(string gameDir, Func<string?> getSessionXmlForExport)
    {
        _gameDir = gameDir;
        _getSessionXml = getSessionXmlForExport;
        InitializeComponent();
        // Do not use Checked= in XAML with IsChecked=True: the event runs mid-parse before HelpDetailText / Step 2 / preview controls exist.
        FormatJsonRadio.Checked += OnExportFormatChanged;
        FormatXmlRadio.Checked += OnExportFormatChanged;
        FormatPazRadio.Checked += OnExportFormatChanged;
        FormatPresetRadio.Checked += OnExportFormatChanged;
        RefreshFormatDependentUi();
    }

    private enum ShareExportFormat
    {
        Json,
        Xml,
        Paz,
        UcmPreset
    }

    private ShareExportFormat SelectedFormat
    {
        get
        {
            if (FormatPresetRadio.IsChecked == true)
                return ShareExportFormat.UcmPreset;
            if (FormatPazRadio.IsChecked == true)
                return ShareExportFormat.Paz;
            if (FormatXmlRadio.IsChecked == true)
                return ShareExportFormat.Xml;
            return ShareExportFormat.Json;
        }
    }

    private void OnExportFormatChanged(object sender, RoutedEventArgs e)
    {
        ClearPreparedExport();
        RefreshFormatDependentUi();
    }

    private void RefreshFormatDependentUi()
    {
        switch (SelectedFormat)
        {
            case ShareExportFormat.Json:
                SetJsonFormatHelpDetail();
                Step2Body.Text =
                    "Uses your live UCM session (Quick, Fine Tune, or God Mode). " +
                    "Pick a preset in the sidebar or use Import if you are starting from XML or a PAZ on disk.";
                break;
            case ShareExportFormat.Xml:
                HelpDetailText.Text =
                    "Exports playercamerapreset.xml with the same normalization UCM uses when installing " +
                    "(UTF-8 BOM, comments stripped). Great for Nexus text uploads, hand editing, or re-import in UCM. " +
                    "Whoever installs it still needs matching compressed entry sizes for their game build.";
                Step2Body.Text =
                    "Prepare checks that your session encodes for this game folder, then you save the XML file.";
                break;
            case ShareExportFormat.Paz:
                HelpDetailText.Text =
                    "Nexus-style “drop-in” archive: a copy of your game’s 0010/0.paz with only the camera data updated " +
                    "to match your current UCM session — players who don’t use a JSON mod manager can replace one file. " +
                    "It only works for the same game patch as your install (same archive layout); say that on the mod page. " +
                    "Tell downloaders to back up vanilla 0010/0.paz before swapping.";
                Step2Body.Text =
                    "Prepare verifies encoding against your game folder. Save writes a full patched 0.paz (large file). " +
                    "Session source is the same as the other formats — sidebar preset or Import first if needed.";
                break;
            case ShareExportFormat.UcmPreset:
                HelpDetailText.Text =
                    "Exports your current session as a .ucmpreset file that other UCM users can drop into their " +
                    "presets folder or share via the community catalog. Contains your full camera configuration " +
                    "including all Quick, Fine Tune, and God Mode settings.";
                Step2Body.Text =
                    "No encoding needed — saves your session directly. Fill in the info fields above so others " +
                    "know what they're getting.";
                break;
        }
    }

    private const string NexusJsonModManagerUrl = "https://www.nexusmods.com/crimsondesert/mods/113";
    private const string NexusCdummUrl = "https://www.nexusmods.com/crimsondesert/mods/207";

    private void SetJsonFormatHelpDetail()
    {
        HelpDetailText.Text = null;
        HelpDetailText.Inlines.Clear();

        Brush accent = (Brush)FindResource("AccentBrush");
        var mono = new FontFamily("Consolas");

        void AddRun(string text) => HelpDetailText.Inlines.Add(new Run(text));

        void AddLink(string label, string url)
        {
            var link = new Hyperlink(new Run(label))
            {
                NavigateUri = new Uri(url),
                Foreground = accent,
            };
            link.RequestNavigate += OnExportHelpHyperlinkNavigate;
            HelpDetailText.Inlines.Add(link);
        }

        AddRun("Exports a byte-patch ");
        HelpDetailText.Inlines.Add(new Run(".json") { FontFamily = mono });
        AddRun(" you can import into ");
        AddLink("JSON Mod Manager", NexusJsonModManagerUrl);
        AddRun(" (Nexus mod 113) or ");
        AddLink("Crimson Desert Ultimate Mods Manager", NexusCdummUrl);
        AddRun(" (CDUMM, Nexus mod 207). Use whichever mod manager you prefer — recipients do not need UCM.\n\n");
        AddRun("Prepare is only available when your live ");
        HelpDetailText.Inlines.Add(new Run("playercamerapreset") { FontFamily = mono });
        AddRun(" entry still matches UCM's vanilla backup (e.g. verify game files in Steam if you already applied UCM or another camera mod). After a game update, re-export from a PC on the same build.");
    }

    private void OnExportHelpHyperlinkNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch
        {
            // Browser / shell may be unavailable; ignore.
        }
    }

    private void ClearPreparedExport()
    {
        ExportPreviewPanel.Visibility = Visibility.Collapsed;
        _jsonLastPatches = null;
        _jsonLastJson = null;
        _preparedXml = null;
        JsonStatsPanel.Visibility = Visibility.Collapsed;
        XmlSaveHint.Visibility = Visibility.Collapsed;
        PazSaveHint.Visibility = Visibility.Collapsed;
        FingerprintLabel.Text = "";
    }

    private void OnPrepareFromCurrent(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir))
        {
            SetStatus("Game folder not set.", true);
            return;
        }

        string? xml = _getSessionXml?.Invoke();
        if (string.IsNullOrEmpty(xml))
        {
            SetStatus("No session XML available. Edit settings or load a preset first.", true);
            return;
        }

        PrepareWithXml(xml);
    }

    private void PrepareWithXml(string xml)
    {
        switch (SelectedFormat)
        {
            case ShareExportFormat.Json:
                try
                {
                    if (!CameraMod.IsLiveCameraPayloadMatchingStoredBackup(_gameDir,
                            msg => Dispatcher.Invoke(() => SetStatus(msg, false))))
                    {
                        MessageBox.Show(
                            "The camera data in your game folder does not match UCM's vanilla backup.\n\n" +
                            "JSON patches for JSON Mod Manager or Crimson Desert Ultimate Mods Manager must use " +
                            "vanilla \"original\" bytes. If UCM or another tool has already changed " +
                            "playercamerapreset in 0.paz, exported JSON will not apply for other players.\n\n" +
                            "Fix: Steam → Crimson Desert → Properties → Installed Files → " +
                            "\"Verify integrity of game files\", then reopen UCM and export again. " +
                            "If you use a mod manager, revert camera changes there before verifying.",
                            "Cannot export JSON",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        SetStatus("JSON export needs vanilla camera files (verify game, then try again).", true);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    SetStatus($"Could not verify camera files: {ex.Message}", true);
                    return;
                }

                var jsonInfo = BuildJsonModInfo();
                RunJsonGenerate(() =>
                {
                    return JsonModExporter.ExportFromXml(_gameDir, jsonInfo, xml,
                        msg => Dispatcher.Invoke(() => SetStatus(msg, false)));
                });
                break;
            case ShareExportFormat.Xml:
            case ShareExportFormat.Paz:
                RunValidateThenReady(xml);
                break;
            case ShareExportFormat.UcmPreset:
                if (string.IsNullOrWhiteSpace(xml))
                {
                    SetStatus("Session XML is empty.", true);
                    return;
                }
                _preparedXml = xml;
                _jsonLastJson = null;
                _jsonLastPatches = null;
                ExportPreviewPanel.Visibility = Visibility.Visible;
                JsonStatsPanel.Visibility = Visibility.Collapsed;
                XmlSaveHint.Visibility = Visibility.Collapsed;
                PazSaveHint.Visibility = Visibility.Collapsed;
                FingerprintLabel.Text = $"Session XML: {xml.Length:N0} characters";
                SaveExportButton.Content = "Save .ucmpreset...";
                SetStatus("Ready to save. Click Save when ready.", false);
                break;
        }
    }

    private void RunJsonGenerate(Func<(List<JsonModExporter.PatchChange>, string)> work)
    {
        SetStatus("Generating patches...", false);
        ClearPreparedExport();

        Task.Run(() =>
        {
            try
            {
                var (changes, json) = work();
                string fingerprint = CameraMod.GetExportCompatibilityNote(_gameDir, null);
                Dispatcher.Invoke(() =>
                {
                    _jsonLastPatches = changes;
                    _jsonLastJson = json;
                    _preparedXml = null;

                    JsonPatchCountLabel.Text = changes.Count.ToString();
                    JsonBytesChangedLabel.Text = changes.Sum(c => c.Original.Length / 2).ToString();

                    ExportPreviewPanel.Visibility = Visibility.Visible;
                    JsonStatsPanel.Visibility = Visibility.Visible;
                    XmlSaveHint.Visibility = Visibility.Collapsed;
                    PazSaveHint.Visibility = Visibility.Collapsed;
                    FingerprintLabel.Text = fingerprint;

                    SaveExportButton.Content = "Save .json...";
                    SetStatus($"Generated {changes.Count} patch regions. Save when ready.", false);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SetStatus($"Prepare failed: {ex.Message}", true));
            }
        });
    }

    private void RunValidateThenReady(string xml)
    {
        SetStatus("Validating encoding for this game...", false);
        ClearPreparedExport();

        var gameDir = _gameDir;
        var fmt = SelectedFormat;
        Task.Run(() =>
        {
            try
            {
                CameraMod.BuildModifiedBytesFromXml(gameDir, xml,
                    msg => Dispatcher.Invoke(() => SetStatus(msg, false)));
                string fingerprint = CameraMod.GetExportCompatibilityNote(gameDir, null);
                Dispatcher.Invoke(() =>
                {
                    _preparedXml = xml;
                    _jsonLastJson = null;
                    _jsonLastPatches = null;

                    ExportPreviewPanel.Visibility = Visibility.Visible;
                    JsonStatsPanel.Visibility = Visibility.Collapsed;
                    FingerprintLabel.Text = fingerprint;

                    if (fmt == ShareExportFormat.Xml)
                    {
                        XmlSaveHint.Visibility = Visibility.Visible;
                        PazSaveHint.Visibility = Visibility.Collapsed;
                        XmlSaveHint.Text =
                            "XML encoding matches this install's camera entry. Save as playercamerapreset.xml for sharing or editing.";
                        SaveExportButton.Content = "Save .xml...";
                    }
                    else
                    {
                        XmlSaveHint.Visibility = Visibility.Collapsed;
                        PazSaveHint.Visibility = Visibility.Visible;
                        PazSaveHint.Text =
                            "Upload size will match a full 0.paz. Recipients replace …\\0010\\0.paz after backing up vanilla. " +
                            "Only share with players on the same game version as this export.";
                        SaveExportButton.Content = "Save 0.paz...";
                    }

                    SetStatus("Ready to save.", false);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SetStatus($"Prepare failed: {ex.Message}", true));
            }
        });
    }

    private void OnSaveExport(object sender, RoutedEventArgs e)
    {
        switch (SelectedFormat)
        {
            case ShareExportFormat.Json:
                SaveJsonExport();
                break;
            case ShareExportFormat.Xml:
                SaveXmlExport();
                break;
            case ShareExportFormat.Paz:
                SavePazExport();
                break;
            case ShareExportFormat.UcmPreset:
                SaveUcmPresetExport();
                break;
        }
    }

    private void SaveJsonExport()
    {
        if (_jsonLastJson == null)
        {
            SetStatus("Prepare the export first.", false);
            return;
        }

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
        if (sfd.ShowDialog(Window.GetWindow(this)) != true)
            return;

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

    private void SaveXmlExport()
    {
        if (string.IsNullOrEmpty(_preparedXml))
        {
            SetStatus("Prepare the export first.", false);
            return;
        }

        string title = JsonTitleBox.Text.Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "playercamerapreset"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Save camera XML",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = $"{safeName}.xml"
        };
        if (sfd.ShowDialog(Window.GetWindow(this)) != true)
            return;

        try
        {
            CameraMod.ExportPresetXml(sfd.FileName, _preparedXml);
            SetStatus($"Saved {Path.GetFileName(sfd.FileName)}.", false);
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {ex.Message}", true);
        }
    }

    private void SavePazExport()
    {
        if (string.IsNullOrEmpty(_preparedXml))
        {
            SetStatus("Prepare the export first.", false);
            return;
        }

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Save patched 0.paz",
            Filter = "PAZ archive (*.paz)|*.paz|All files (*.*)|*.*",
            FileName = "0.paz"
        };
        if (sfd.ShowDialog(Window.GetWindow(this)) != true)
            return;

        string destPath = sfd.FileName;
        string gameDir = _gameDir;
        string xml = _preparedXml;

        SetStatus("Writing archive (copy + patch)...", false);
        SaveExportButton.IsEnabled = false;

        Task.Run(() =>
        {
            try
            {
                CameraMod.ExportPatchedPaz(gameDir, destPath, xml,
                    msg => Dispatcher.Invoke(() => SetStatus(msg, false)));
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Saved {Path.GetFileName(destPath)}.", false);
                    SaveExportButton.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Save failed: {ex.Message}", true);
                    SaveExportButton.IsEnabled = true;
                });
            }
        });
    }

    private void SaveUcmPresetExport()
    {
        if (string.IsNullOrWhiteSpace(_preparedXml))
        {
            SetStatus("Prepare first.", true);
            return;
        }

        string title = JsonTitleBox.Text.Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "ucm_preset"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Save UCM Preset",
            Filter = "UCM Preset (*.ucmpreset)|*.ucmpreset|All files (*.*)|*.*",
            FileName = $"{safeName}.ucmpreset"
        };
        if (sfd.ShowDialog(Window.GetWindow(this)) != true) return;

        try
        {
            var preset = new Dictionary<string, object>
            {
                ["name"] = string.IsNullOrWhiteSpace(title) ? "Exported Preset" : title,
                ["author"] = JsonAuthorBox.Text.Trim(),
                ["description"] = JsonDescBox.Text.Trim(),
                ["kind"] = "user",
                ["session_xml"] = _preparedXml,
                ["settings"] = new Dictionary<string, object>
                {
                    ["distance"] = 5.0,
                    ["height"] = 0.0,
                    ["right_offset"] = 0.0,
                    ["fov"] = 0,
                    ["combat"] = "default",
                    ["centered"] = false,
                    ["mount_height"] = false,
                    ["steadycam"] = true
                }
            };

            string url = JsonNexusBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(url))
                preset["url"] = url;

            string json = System.Text.Json.JsonSerializer.Serialize(preset,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            File.WriteAllText(sfd.FileName, json, new UTF8Encoding(false));
            SetStatus($"Saved {Path.GetFileName(sfd.FileName)}", false);
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
