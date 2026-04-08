using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using UltimateCameraMod.Services;
using UltimateCameraMod.V3.Localization;

namespace UltimateCameraMod.V3;

public partial class ExportJsonDialog : UserControl
{
    private static string L(string key) => TranslationSource.Instance[key];
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
                Step2Body.Text = L("Help_ExportJsonStep2");
                break;
            case ShareExportFormat.Xml:
                HelpDetailText.Text = L("Help_ExportXmlFormat");
                Step2Body.Text = L("Help_ExportXmlStep2");
                break;
            case ShareExportFormat.Paz:
                HelpDetailText.Text = L("Help_ExportPazFormat");
                Step2Body.Text = L("Help_ExportPazStep2");
                break;
            case ShareExportFormat.UcmPreset:
                HelpDetailText.Text = L("Help_ExportUcmPresetFormat");
                Step2Body.Text = L("Help_ExportPresetStep2");
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
            SetStatus(L("Status_GameFolderNotSet"), true);
            return;
        }

        string? xml = _getSessionXml?.Invoke();
        if (string.IsNullOrEmpty(xml))
        {
            SetStatus(L("Status_NoSessionXmlAvailable"), true);
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
                            L("Msg_CannotExportJson"),
                            L("Msg_CannotExportJsonTitle"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        SetStatus(L("Msg_JsonExportNeedsVanilla"), true);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    SetStatus(string.Format(L("Status_CouldNotVerifyFiles"), ex.Message), true);
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
                    SetStatus(L("Status_SessionXmlEmpty"), true);
                    return;
                }
                _preparedXml = xml;
                _jsonLastJson = null;
                _jsonLastPatches = null;
                ExportPreviewPanel.Visibility = Visibility.Visible;
                JsonStatsPanel.Visibility = Visibility.Collapsed;
                XmlSaveHint.Visibility = Visibility.Collapsed;
                PazSaveHint.Visibility = Visibility.Collapsed;
                FingerprintLabel.Text = string.Format(L("Status_SessionXmlCharacters"), xml.Length);
                SaveExportButton.Content = L("Btn_SaveUcmPreset");
                SetStatus(L("Status_ReadyToSave"), false);
                break;
        }
    }

    private void RunJsonGenerate(Func<(List<JsonModExporter.PatchChange>, string)> work)
    {
        SetStatus(L("Status_GeneratingPatches"), false);
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

                    SaveExportButton.Content = L("Btn_SaveJsonFile");
                    SetStatus(string.Format(L("Help_GeneratedPatchReady"), changes.Count), false);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SetStatus(string.Format(L("Status_PrepareFailed"), ex.Message), true));
            }
        });
    }

    private void RunValidateThenReady(string xml)
    {
        SetStatus(L("Status_ValidatingEncoding"), false);
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
                        XmlSaveHint.Text = L("Help_XmlEncodingMatches");
                        SaveExportButton.Content = L("Btn_SaveXmlFile");
                    }
                    else
                    {
                        XmlSaveHint.Visibility = Visibility.Collapsed;
                        PazSaveHint.Visibility = Visibility.Visible;
                        PazSaveHint.Text = L("Help_PazUploadNote");
                        SaveExportButton.Content = L("Btn_SavePazFile");
                    }

                    SetStatus(L("Status_ReadyToSave"), false);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SetStatus(string.Format(L("Status_PrepareFailed"), ex.Message), true));
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
            SetStatus(L("Status_PrepareExportFirst"), false);
            return;
        }

        string title = JsonTitleBox.Text.Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "ucm_patch"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = L("Dlg_SaveJsonPatch"),
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = $"{safeName}.json"
        };
        if (sfd.ShowDialog(Window.GetWindow(this)) != true)
            return;

        try
        {
            File.WriteAllText(sfd.FileName, _jsonLastJson, new UTF8Encoding(false));
            SetStatus(string.Format(L("Status_SavedFilePatches"), Path.GetFileName(sfd.FileName), _jsonLastPatches!.Count), false);
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_SaveFailed"), ex.Message), true);
        }
    }

    private void SaveXmlExport()
    {
        if (string.IsNullOrEmpty(_preparedXml))
        {
            SetStatus(L("Status_PrepareExportFirst"), false);
            return;
        }

        string title = JsonTitleBox.Text.Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "playercamerapreset"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = L("Dlg_SaveCameraXml"),
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = $"{safeName}.xml"
        };
        if (sfd.ShowDialog(Window.GetWindow(this)) != true)
            return;

        try
        {
            CameraMod.ExportPresetXml(sfd.FileName, _preparedXml);
            SetStatus(string.Format(L("Status_SavedFile"), Path.GetFileName(sfd.FileName)), false);
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_SaveFailed"), ex.Message), true);
        }
    }

    private void SavePazExport()
    {
        if (string.IsNullOrEmpty(_preparedXml))
        {
            SetStatus(L("Status_PrepareExportFirst"), false);
            return;
        }

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = L("Dlg_SavePatchedPaz"),
            Filter = "PAZ archive (*.paz)|*.paz|All files (*.*)|*.*",
            FileName = "0.paz"
        };
        if (sfd.ShowDialog(Window.GetWindow(this)) != true)
            return;

        string destPath = sfd.FileName;
        string gameDir = _gameDir;
        string xml = _preparedXml;

        SetStatus(L("Status_WritingArchive"), false);
        SaveExportButton.IsEnabled = false;

        Task.Run(() =>
        {
            try
            {
                CameraMod.ExportPatchedPaz(gameDir, destPath, xml,
                    msg => Dispatcher.Invoke(() => SetStatus(msg, false)));
                Dispatcher.Invoke(() =>
                {
                    SetStatus(string.Format(L("Status_SavedFile"), Path.GetFileName(destPath)), false);
                    SaveExportButton.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus(string.Format(L("Status_SaveFailed"), ex.Message), true);
                    SaveExportButton.IsEnabled = true;
                });
            }
        });
    }

    private void SaveUcmPresetExport()
    {
        if (string.IsNullOrWhiteSpace(_preparedXml))
        {
            SetStatus(L("Status_PrepareExportFirst"), true);
            return;
        }

        string title = JsonTitleBox.Text.Trim();
        string safeName = string.IsNullOrWhiteSpace(title)
            ? "ucm_preset"
            : new string(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = L("Dlg_SaveUcmPreset"),
            Filter = "UCM Preset (*.ucmpreset)|*.ucmpreset|All files (*.*)|*.*",
            FileName = $"{safeName}.ucmpreset"
        };
        if (sfd.ShowDialog(Window.GetWindow(this)) != true) return;

        try
        {
            string url = JsonNexusBox.Text.Trim();

            // Build with metadata BEFORE session_xml so url/author/description
            // fall within the 4KB header window for fast reads
            var preset = new Dictionary<string, object>
            {
                ["name"] = string.IsNullOrWhiteSpace(title) ? L("Dlg_ExportedPreset") : title,
                ["author"] = JsonAuthorBox.Text.Trim(),
                ["description"] = JsonDescBox.Text.Trim(),
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

            string json = System.Text.Json.JsonSerializer.Serialize(preset,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(sfd.FileName, json, new UTF8Encoding(false));
            SetStatus(string.Format(L("Status_SavedFile"), Path.GetFileName(sfd.FileName)), false);
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_SaveFailed"), ex.Message), true);
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
