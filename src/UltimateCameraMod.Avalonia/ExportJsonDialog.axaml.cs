using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.Avalonia;

public partial class ExportJsonDialog : UserControl
{
    public Action? OnCloseRequested;

    private readonly string _gameDir;
    private readonly Func<string?> _getSessionXml;
    private readonly Func<Dictionary<string, object>>? _getSettingsPayload;
    private readonly bool _isRawImport;

    private List<JsonModExporter.PatchChange>? _jsonLastPatches;
    private string? _jsonLastJson;
    private string? _preparedXml;

    public ExportJsonDialog(string gameDir, Func<string?> getSessionXmlForExport,
        string? presetName = null, string? presetAuthor = null,
        string? presetDescription = null, string? presetUrl = null,
        bool isRawImport = false,
        Func<Dictionary<string, object>>? getSettingsPayload = null)
    {
        _gameDir = gameDir;
        _getSessionXml = getSessionXmlForExport;
        _getSettingsPayload = getSettingsPayload;
        _isRawImport = isRawImport;
        InitializeComponent();
        // Pre-fill from active preset metadata
        if (!string.IsNullOrWhiteSpace(presetName))
            JsonTitleBox.Text = presetName;
        if (!string.IsNullOrWhiteSpace(presetAuthor))
            JsonAuthorBox.Text = presetAuthor;
        if (!string.IsNullOrWhiteSpace(presetDescription))
            JsonDescBox.Text = presetDescription;
        if (!string.IsNullOrWhiteSpace(presetUrl))
            JsonNexusBox.Text = presetUrl;

        // Wire up format radio changes after controls are initialized
        FormatJsonRadio.IsCheckedChanged += OnExportFormatChanged;
        FormatXmlRadio.IsCheckedChanged += OnExportFormatChanged;
        FormatPazRadio.IsCheckedChanged += OnExportFormatChanged;
        FormatPresetRadio.IsCheckedChanged += OnExportFormatChanged;
        RefreshFormatDependentUi();
    }

    // Avalonia requires a parameterless constructor for the XAML loader
    public ExportJsonDialog() : this("", () => null) { }

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

    private void OnExportFormatChanged(object? sender, RoutedEventArgs e)
    {
        // Only respond when a radio button is being checked (not unchecked)
        if (sender is RadioButton rb && rb.IsChecked != true) return;
        ClearPreparedExport();
        RefreshFormatDependentUi();
    }

    private void RefreshFormatDependentUi()
    {
        switch (SelectedFormat)
        {
            case ShareExportFormat.Json:
                HelpDetailText.Text =
                    "Exports a byte-patch .json you can import into JSON Mod Manager (Nexus mod 113) or " +
                    "Crimson Desert Ultimate Mods Manager (CDUMM, Nexus mod 207). Use whichever mod manager " +
                    "you prefer -- recipients do not need UCM.\n\n" +
                    "Prepare is only available when your live playercamerapreset entry still matches UCM's " +
                    "vanilla backup (e.g. verify game files in Steam if you already applied UCM or another " +
                    "camera mod). After a game update, re-export from a PC on the same build.";
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
                    "Nexus-style \"drop-in\" archive: a copy of your game's 0010/0.paz with only the camera data updated " +
                    "to match your current UCM session -- players who don't use a JSON mod manager can replace one file. " +
                    "It only works for the same game patch as your install (same archive layout); say that on the mod page. " +
                    "Tell downloaders to back up vanilla 0010/0.paz before swapping.";
                Step2Body.Text =
                    "Prepare verifies encoding against your game folder. Save writes a full patched 0.paz (large file). " +
                    "Session source is the same as the other formats -- sidebar preset or Import first if needed.";
                break;
            case ShareExportFormat.UcmPreset:
                HelpDetailText.Text =
                    "Exports your current session as a .ucmpreset file that other UCM users can drop into their " +
                    "presets folder or share via the community catalog. Contains your full camera configuration " +
                    "including all Quick, Fine Tune, and God Mode settings.";
                Step2Body.Text =
                    "No encoding needed -- saves your session directly. Fill in the info fields above so others " +
                    "know what they're getting.";
                break;
        }
    }

    private void ClearPreparedExport()
    {
        ExportPreviewPanel.IsVisible = false;
        _jsonLastPatches = null;
        _jsonLastJson = null;
        _preparedXml = null;
        JsonStatsPanel.IsVisible = false;
        XmlSaveHint.IsVisible = false;
        PazSaveHint.IsVisible = false;
        FingerprintLabel.Text = "";
    }

    private void OnPrepareFromCurrent(object? sender, RoutedEventArgs e)
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
                            msg => Dispatcher.UIThread.Post(() => SetStatus(msg, false))))
                    {
                        // Show warning via status rather than modal MessageBox (Avalonia cross-platform)
                        SetStatus(
                            "JSON export needs vanilla camera files. The camera data in your game folder does not " +
                            "match UCM's vanilla backup. Verify game files in Steam, then reopen UCM and export again.",
                            true);
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
                        msg => Dispatcher.UIThread.Post(() => SetStatus(msg, false)));
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
                ExportPreviewPanel.IsVisible = true;
                JsonStatsPanel.IsVisible = false;
                XmlSaveHint.IsVisible = false;
                PazSaveHint.IsVisible = false;
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
                Dispatcher.UIThread.Post(() =>
                {
                    _jsonLastPatches = changes;
                    _jsonLastJson = json;
                    _preparedXml = null;

                    JsonPatchCountLabel.Text = changes.Count.ToString();
                    JsonBytesChangedLabel.Text = changes.Sum(c => c.Original.Length / 2).ToString();

                    ExportPreviewPanel.IsVisible = true;
                    JsonStatsPanel.IsVisible = true;
                    XmlSaveHint.IsVisible = false;
                    PazSaveHint.IsVisible = false;
                    FingerprintLabel.Text = fingerprint;

                    SaveExportButton.Content = "Save .json...";
                    SetStatus($"Generated {changes.Count} patch regions. Save when ready.", false);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() => SetStatus($"Prepare failed: {ex.Message}", true));
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
                    msg => Dispatcher.UIThread.Post(() => SetStatus(msg, false)));
                string fingerprint = CameraMod.GetExportCompatibilityNote(gameDir, null);
                Dispatcher.UIThread.Post(() =>
                {
                    _preparedXml = xml;
                    _jsonLastJson = null;
                    _jsonLastPatches = null;

                    ExportPreviewPanel.IsVisible = true;
                    JsonStatsPanel.IsVisible = false;
                    FingerprintLabel.Text = fingerprint;

                    if (fmt == ShareExportFormat.Xml)
                    {
                        XmlSaveHint.IsVisible = true;
                        PazSaveHint.IsVisible = false;
                        XmlSaveHint.Text =
                            "XML encoding matches this install's camera entry. Save as playercamerapreset.xml for sharing or editing.";
                        SaveExportButton.Content = "Save .xml...";
                    }
                    else
                    {
                        XmlSaveHint.IsVisible = false;
                        PazSaveHint.IsVisible = true;
                        PazSaveHint.Text =
                            "Upload size will match a full 0.paz. Recipients replace .../0010/0.paz after backing up vanilla. " +
                            "Only share with players on the same game version as this export.";
                        SaveExportButton.Content = "Save 0.paz...";
                    }

                    SetStatus("Ready to save.", false);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() => SetStatus($"Prepare failed: {ex.Message}", true));
            }
        });
    }

    private async void OnSaveExport(object? sender, RoutedEventArgs e)
    {
        switch (SelectedFormat)
        {
            case ShareExportFormat.Json:
                await SaveJsonExportAsync();
                break;
            case ShareExportFormat.Xml:
                await SaveXmlExportAsync();
                break;
            case ShareExportFormat.Paz:
                await SavePazExportAsync();
                break;
            case ShareExportFormat.UcmPreset:
                await SaveUcmPresetExportAsync();
                break;
        }
    }

    private async Task SaveJsonExportAsync()
    {
        if (_jsonLastJson == null)
        {
            SetStatus("Prepare the export first.", false);
            return;
        }

        string title = (JsonTitleBox.Text ?? "").Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "ucm_patch"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save JSON Patch",
            SuggestedFileName = $"{safeName}.json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON files") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All files") { Patterns = new[] { "*.*" } }
            }
        });
        if (file == null) return;

        try
        {
            string path = file.Path.LocalPath;
            File.WriteAllText(path, _jsonLastJson, new UTF8Encoding(false));
            SetStatus($"Saved {Path.GetFileName(path)} ({_jsonLastPatches!.Count} patches).", false);
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {ex.Message}", true);
        }
    }

    private async Task SaveXmlExportAsync()
    {
        if (string.IsNullOrEmpty(_preparedXml))
        {
            SetStatus("Prepare the export first.", false);
            return;
        }

        string title = (JsonTitleBox.Text ?? "").Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "playercamerapreset"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save camera XML",
            SuggestedFileName = $"{safeName}.xml",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("XML files") { Patterns = new[] { "*.xml" } },
                new FilePickerFileType("All files") { Patterns = new[] { "*.*" } }
            }
        });
        if (file == null) return;

        try
        {
            string path = file.Path.LocalPath;
            CameraMod.ExportPresetXml(path, _preparedXml);
            SetStatus($"Saved {Path.GetFileName(path)}.", false);
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {ex.Message}", true);
        }
    }

    private async Task SavePazExportAsync()
    {
        if (string.IsNullOrEmpty(_preparedXml))
        {
            SetStatus("Prepare the export first.", false);
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save patched 0.paz",
            SuggestedFileName = "0.paz",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PAZ archive") { Patterns = new[] { "*.paz" } },
                new FilePickerFileType("All files") { Patterns = new[] { "*.*" } }
            }
        });
        if (file == null) return;

        string destPath = file.Path.LocalPath;
        string gameDir = _gameDir;
        string xml = _preparedXml;

        SetStatus("Writing archive (copy + patch)...", false);
        SaveExportButton.IsEnabled = false;

        await Task.Run(() =>
        {
            try
            {
                CameraMod.ExportPatchedPaz(gameDir, destPath, xml,
                    msg => Dispatcher.UIThread.Post(() => SetStatus(msg, false)));
                Dispatcher.UIThread.Post(() =>
                {
                    SetStatus($"Saved {Path.GetFileName(destPath)}.", false);
                    SaveExportButton.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    SetStatus($"Save failed: {ex.Message}", true);
                    SaveExportButton.IsEnabled = true;
                });
            }
        });
    }

    private async Task SaveUcmPresetExportAsync()
    {
        if (string.IsNullOrWhiteSpace(_preparedXml))
        {
            SetStatus("Prepare first.", true);
            return;
        }

        string title = (JsonTitleBox.Text ?? "").Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "ucm_preset"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save UCM Preset",
            SuggestedFileName = $"{safeName}.ucmpreset",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("UCM Preset") { Patterns = new[] { "*.ucmpreset" } },
                new FilePickerFileType("All files") { Patterns = new[] { "*.*" } }
            }
        });
        if (file == null) return;

        try
        {
            string path = file.Path.LocalPath;
            string url = (JsonNexusBox.Text ?? "").Trim();

            var preset = new Dictionary<string, object>
            {
                ["name"] = string.IsNullOrWhiteSpace(title) ? "Exported Preset" : title,
                ["author"] = (JsonAuthorBox.Text ?? "").Trim(),
                ["description"] = (JsonDescBox.Text ?? "").Trim(),
                ["kind"] = "user",
            };
            if (!string.IsNullOrWhiteSpace(url))
                preset["url"] = url;
            preset["preset_mode"] = _isRawImport ? "godmode" : "ucm";
            preset["settings"] = _getSettingsPayload != null
                ? _getSettingsPayload()
                : new Dictionary<string, object>
                {
                    ["distance"] = 5.0,
                    ["height"] = 0.0,
                    ["right_offset"] = 0.0,
                    ["fov"] = 0,
                    ["combat_pullback"] = 0.0,
                    ["centered"] = false,
                    ["mount_height"] = false,
                    ["steadycam"] = true,
                    ["lock_on_auto_rotate_disabled"] = false,
                    ["center_hud"] = false,
                    ["hud_width"] = 1920
                };
            preset["session_xml"] = _preparedXml;

            string json = JsonSerializer.Serialize(preset,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json, new UTF8Encoding(false));
            SetStatus($"Saved {Path.GetFileName(path)}", false);
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {ex.Message}", true);
        }
    }

    private JsonModExporter.ModInfo BuildJsonModInfo() => new(
        Title: (JsonTitleBox.Text ?? "").Trim().Length > 0 ? (JsonTitleBox.Text ?? "").Trim() : "UCM Camera Config",
        Version: (JsonVersionBox.Text ?? "").Trim().Length > 0 ? (JsonVersionBox.Text ?? "").Trim() : "1.0",
        Author: (JsonAuthorBox.Text ?? "").Trim(),
        Description: (JsonDescBox.Text ?? "").Trim(),
        NexusUrl: (JsonNexusBox.Text ?? "").Trim().Length > 0 ? (JsonNexusBox.Text ?? "").Trim()
            : "https://www.nexusmods.com/crimsondesert/mods/438");

    private void SetStatus(string msg, bool isError)
    {
        StatusLabel.Text = msg;
        StatusLabel.Foreground = isError
            ? (IBrush?)this.FindResource("ErrorBrush") ?? Brushes.Red
            : (IBrush?)this.FindResource("TextDimBrush") ?? Brushes.Gray;
    }
}
