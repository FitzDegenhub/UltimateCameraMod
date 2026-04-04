using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using UltimateCameraMod.V3.Controls;
using UltimateCameraMod.V3.Models;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.V3;

public partial class MainWindow : Window
{
    private static string PresetManagerKey(PresetManagerItem item) => $"{item.KindId}:{item.Name}";

    private List<PresetManagerItem> BuildPresetManagerItems(ImportedPresetFingerprint? gameFingerprint)
    {
        var items = new List<PresetManagerItem>();

        void AppendSessionJsonPresetsFromDir(string directory, bool defaultLocked, string? kindOverride)
        {
            if (!Directory.Exists(directory))
                return;

            foreach (string file in GetPresetFiles(directory))
            {
                try
                {
                    // Read the first 4KB to extract metadata without parsing the huge session_xml
                    string header;
                    using (var reader = new StreamReader(file))
                    {
                        var buf = new char[4096];
                        int read = reader.Read(buf, 0, buf.Length);
                        header = new string(buf, 0, read);
                    }

                    string name = ExtractJsonStringField(header, "name") ?? Path.GetFileNameWithoutExtension(file);
                    string author = ExtractJsonStringField(header, "author") ?? "";
                    string desc = ExtractJsonStringField(header, "description") ?? "";
                    string kind = kindOverride ?? ExtractJsonStringField(header, "kind") ?? "user";
                    bool isLocked = ExtractJsonBoolField(header, "locked") ?? defaultLocked;
                    string url = ExtractJsonStringField(header, "url") ?? "";

                    items.Add(new PresetManagerItem
                    {
                        Name = name,
                        KindId = kind,
                        KindLabel = kind switch
                        {
                            "default" => "Default",
                            "style" => "UCM style",
                            "community" => "Community",
                            "imported" => "Imported",
                            _ => "My preset"
                        },
                        SourceLabel = author,
                        StatusText = desc,
                        SummaryText = string.IsNullOrEmpty(desc)
                            ? "Custom preset."
                            : desc,
                        FilePath = file,
                        CanRebuild = false,
                        IsLocked = isLocked,
                        Url = url
                    });
                }
                catch { /* skip malformed preset files */ }
            }
        }

        AppendSessionJsonPresetsFromDir(UcmPresetsDir, defaultLocked: true, kindOverride: null);

        // Ensure "UCM presets" group header always shows so the Browse button is visible
        if (!items.Any(i => i.KindId == "style"))
        {
            items.Add(new PresetManagerItem
            {
                Name = "\0",
                KindId = "style",
                KindLabel = "UCM style",
                FilePath = "",
                IsLocked = true,
                IsPlaceholder = true
            });
        }

        // Ensure "Game Default" group header always shows
        if (!items.Any(i => i.KindId == "default"))
        {
            items.Add(new PresetManagerItem
            {
                Name = "\0",
                KindId = "default",
                KindLabel = "Game default",
                FilePath = "",
                IsLocked = true,
                IsPlaceholder = true
            });
        }

        AppendSessionJsonPresetsFromDir(CommunityPresetsDir, defaultLocked: true, kindOverride: "community");

        // Ensure "Community presets" group header always shows so the download icon is visible.
        // Use a hidden placeholder — the lock button in the ItemTemplate won't render for empty names.
        if (!items.Any(i => i.KindId == "community"))
        {
            items.Add(new PresetManagerItem
            {
                Name = "\0",
                KindId = "community",
                KindLabel = "Community",
                FilePath = "",
                IsLocked = true,
                IsPlaceholder = true
            });
        }

        AppendSessionJsonPresetsFromDir(MyPresetsDir, defaultLocked: false, kindOverride: null);

        // Imported presets (from ImportedPresetsDir)
        foreach (string file in Directory.GetFiles(ImportedPresetsDir, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            string fileStem = Path.GetFileNameWithoutExtension(file) ?? "";
            if (!TryBuildImportedPresetManagerItem(file, fileStem, gameFingerprint, out PresetManagerItem? importedItem) || importedItem is null)
                continue;
            items.Add(importedItem);
        }

        // Sort: UCM presets (default + style), community, then my_presets (user), then import_presets (Imported)
        int GroupOrder(string kind) => kind switch
        {
            "default" => 0,
            "style" => 0,
            "community" => 1,
            "user" => 2,
            "imported" => 3,
            _ => 4
        };

        return items
            .OrderBy(i => GroupOrder(i.KindId))
            .ThenBy(i => i.KindId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void RefreshPresetManagerLists(bool preserveSelection = true,
        (string fp, ImportedPresetFingerprint? gameFp)? precomputedCatalog = null)
    {
        MigrateImportedPresetsFolderIfNeeded();
        MigrateLegacyPresetFoldersIfNeeded();

        string fp;
        ImportedPresetFingerprint? gameFp;
        if (precomputedCatalog != null)
        {
            fp = precomputedCatalog.Value.fp;
            gameFp = precomputedCatalog.Value.gameFp;
        }
        else
        {
            fp = ComputePresetCatalogFingerprint(out gameFp);
        }

        if (preserveSelection
            && _presetManagerItems.Count > 0
            && string.Equals(fp, _presetCatalogFingerprint, StringComparison.Ordinal))
            return;

        _presetCatalogFingerprint = fp;

        string? previousKey = preserveSelection && _selectedPresetManagerItem != null
            ? PresetManagerKey(_selectedPresetManagerItem)
            : null;

        _suppressPresetPickerActivation = true;
        try
        {
            _presetManagerItems.Clear();
            foreach (var item in BuildPresetManagerItems(gameFp))
                _presetManagerItems.Add(item);

            if (PresetRailList != null)
            {
                var view = CollectionViewSource.GetDefaultView(_presetManagerItems);
                view.GroupDescriptions.Clear();
                view.GroupDescriptions.Add(new PropertyGroupDescription("GroupLabel"));
                PresetRailList.ItemsSource = view;
            }

            PresetManagerItem? selected = null;
            if (!string.IsNullOrWhiteSpace(previousKey))
                selected = _presetManagerItems.FirstOrDefault(item => PresetManagerKey(item) == previousKey);
            selected ??= _presetManagerItems.FirstOrDefault();

            SetSelectedPresetManagerItem(selected, updateDetails: true);
            UpdateLoadedPresetContextUi();
        }
        finally
        {
            _suppressPresetPickerActivation = false;
        }

        if (_selectedPresetManagerItem != null)
            ActivatePickerFromSelection(_selectedPresetManagerItem, skipCapture: true);
    }

    private void SetSelectedPresetManagerItem(PresetManagerItem? item, bool updateDetails)
    {
        _selectedPresetManagerItem = item;

        if (PresetRailList != null && !ReferenceEquals(PresetRailList.SelectedItem, item))
            PresetRailList.SelectedItem = item;

        if (updateDetails)
            UpdatePresetManagerDetails();

        ApplyPresetEditingLockUi();
    }

    private PresetManagerItem? RequireSelectedPresetManagerItem()
    {
        if (_selectedPresetManagerItem != null)
            return _selectedPresetManagerItem;

        SetStatus("Select a preset first.", "TextSecondary");
        return null;
    }

    private void UpdatePresetManagerDetails()
    {
        // Details now shown in the active preset banner via UpdateLoadedPresetContextUi
    }

    private void ActivatePickerFromSelection(PresetManagerItem item, bool skipCapture = false)
    {
        string slotKey = PresetManagerKey(item);
        if (string.Equals(_activePickerKey, slotKey, StringComparison.Ordinal))
            return;

        string? previousPickerKey = _activePickerKey;
        if (!skipCapture)
        {
            try
            {
                CaptureSessionXml();
                var previousItem = FindPresetItemByKey(previousPickerKey);
                if (previousItem != null)
                    SavePresetManagerItemSession(previousItem);
            }
            catch
            {
                // Do not block switching presets if autosave fails.
            }
        }

        // Commit slot key before any work. Imported presets call RefreshPresetManagerLists(), which
        // invokes ActivatePickerFromSelection again; without this, the nested call does not early-return
        // and re-enters LoadImportedPresetIntoSession (save + refresh loop → hang / stack death).
        _activePickerKey = slotKey;

        try
        {
            switch (item.KindId)
            {
                case "imported":
                {
                    var preset = LoadImportedPreset(item.Name) ?? throw new InvalidOperationException("Imported preset could not be loaded — the file may be missing, corrupted, or in an unsupported format.");
                    item.IsLocked = preset.Locked;
                    _selectedImportedPreset = preset;
                    LoadImportedPresetIntoSession(preset);
                    break;
                }
                case "style":
                case "user":
                default:
                {
                    _sessionIsRawImport = false; // May be overridden below if preset_mode is "godmode"
                    if (string.IsNullOrEmpty(item.FilePath) || !File.Exists(item.FilePath))
                    {
                        // UCM style presets are generated by GenerateBuiltInPresets() which needs
                        // the game dir. On a fresh install the file won't exist yet at this point —
                        // silently skip rather than showing a spurious error to the user.
                        bool isUcmStylePreset = item.KindId == "style" || item.KindId == "default";
                        if (!isUcmStylePreset)
                            SetStatus($"Preset file not found: {item.FilePath}", "Error");
                        _activePickerKey = previousPickerKey;
                        break;
                    }

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    string json = File.ReadAllText(item.FilePath);
                    long t1 = sw.ElapsedMilliseconds;
                    using var doc = JsonDocument.Parse(json);
                    long t2 = sw.ElapsedMilliseconds;
                    var root = doc.RootElement;

                    // Detect preset mode: "godmode" presets use raw XML, no UCM rules
                    if (root.TryGetProperty("preset_mode", out var modeEl)
                        && modeEl.ValueKind == JsonValueKind.String
                        && modeEl.GetString() == "godmode")
                    {
                        _sessionIsRawImport = true;
                    }

                    // Restore UCM Quick settings from the settings block (skip for godmode presets)
                    if (!_sessionIsRawImport && root.TryGetProperty("settings", out var settings))
                    {
                        _suppressEvents = true;
                        try
                        {
                            if (root.TryGetProperty("style_id", out var sidEl) && sidEl.ValueKind == JsonValueKind.String)
                                _selectedStyleId = sidEl.GetString() ?? "panoramic";

                            if (settings.TryGetProperty("distance", out var dv2) && dv2.ValueKind == JsonValueKind.Number)
                                DistSlider.Value = Math.Clamp(dv2.GetDouble(), 1.5, 12.0);
                            if (settings.TryGetProperty("height", out var hv2) && hv2.ValueKind == JsonValueKind.Number)
                                HeightSlider.Value = Math.Clamp(hv2.GetDouble(), -1.6, 1.5);
                            if (settings.TryGetProperty("right_offset", out var rv2) && rv2.ValueKind == JsonValueKind.Number)
                                HShiftSlider.Value = Math.Clamp(rv2.GetDouble(), -3.0, 3.0);

                            DistLabel.Text = $"{DistSlider.Value:F1}";
                            HeightLabel.Text = $"{HeightSlider.Value:F1}";
                            HShiftLabel.Text = $"{HShiftSlider.Value:F1}";

                            // Restore global settings
                            if (settings.TryGetProperty("fov", out var fovEl) && fovEl.ValueKind == JsonValueKind.Number)
                            {
                                int fov = fovEl.GetInt32();
                                int fovIdx = Array.FindIndex(FovOptions, f => f.Value == fov);
                                if (fovIdx >= 0) FovCombo.SelectedIndex = fovIdx;
                            }
                            if (settings.TryGetProperty("combat_pullback", out var cpEl) && cpEl.ValueKind == JsonValueKind.Number)
                            {
                                CombatPullbackSlider.Value = Math.Clamp(cpEl.GetDouble(), -0.6, 0.6);
                                CombatPullbackLabel.Text = FormatPullback(CombatPullbackSlider.Value);
                            }
                            else if (settings.TryGetProperty("combat", out var combEl) && combEl.ValueKind == JsonValueKind.String)
                            {
                                // Legacy preset migration: map old string values to approximate pull-back
                                string comb = combEl.GetString() ?? "";
                                CombatPullbackSlider.Value = comb switch { "wide" => 0.25, "max" => 0.5, _ => 0.0 };
                                CombatPullbackLabel.Text = FormatPullback(CombatPullbackSlider.Value);
                            }
                            if (settings.TryGetProperty("centered", out var baneEl))
                                BaneCheck.IsChecked = baneEl.ValueKind == JsonValueKind.True;
                            if (settings.TryGetProperty("mount_height", out var mountEl))
                                MountHeightCheck.IsChecked = mountEl.ValueKind == JsonValueKind.True;
                            if (settings.TryGetProperty("steadycam", out var scEl))
                                SteadycamCheck.IsChecked = scEl.ValueKind == JsonValueKind.True;
                        }
                        finally
                        {
                            // Keep _suppressEvents = true through RefreshUIFromSessionXml
                            // to prevent Quick event handlers from scheduling SyncQuickSettingsToEditors
                            // which would overwrite _sessionXml with Quick-only values.
                        }
                    }

                    CaptureCustomDraft(markDirty: false, updateSelector: false);

                    long t3 = sw.ElapsedMilliseconds;
                    // Apply the full session XML
                    if (root.TryGetProperty("session_xml", out var xmlEl) && xmlEl.ValueKind == JsonValueKind.String)
                    {
                        string? xml = xmlEl.GetString();
                        long t4 = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrWhiteSpace(xml))
                            RefreshUIFromSessionXml(xml, skipQuickSliders: true);
                        long t5 = sw.ElapsedMilliseconds;
                        File.WriteAllText(Path.Combine(ExeDir, "load_timing.txt"),
                            $"File.Read: {t1}ms\nJsonParse: {t2-t1}ms\nSettings: {t3-t2}ms\nGetXml: {t4-t3}ms\nRefreshUI: {t5-t4}ms\nTotal: {t5}ms\nXML len: {xml?.Length ?? 0}");
                    }
                    else
                    {
                        // Definition-only preset (no session_xml): build session from Quick + Fine Tune so install/export work.
                        _sessionIsFullPreset = false;
                        SyncPreview();
                        if (!string.IsNullOrEmpty(_gameDir))
                        {
                            try
                            {
                                _sessionXml = BuildCuratedSessionXml();
                            }
                            catch { /* game dir / vanilla read may still be settling */ }
                        }
                    }

                    // Release _suppressEvents AFTER session XML is set, so Quick event handlers
                    // don't schedule SyncQuickSettingsToEditors which would overwrite _sessionXml
                    _suppressEvents = false;

                    // SyncPreview was skipped inside RefreshUIFromSessionXml because _suppressEvents
                    // was still true. Now that events are re-enabled, sync the preview and FoV.
                    SyncPreview();

                    SetLoadedPresetContext(item.Name, item.KindLabel, item.SourceLabel,
                        item.StatusText, item.SummaryText, item.Url);

                    // God Mode presets: lock Quick/Fine Tune and switch to God Mode tab
                    if (_sessionIsRawImport)
                    {
                        ApplyPresetEditingLockUi();
                        SwitchEditorTab("expert", captureCurrent: false);
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _suppressEvents = false;
            _activePickerKey = previousPickerKey;
            SetStatus($"Could not activate preset: {ex.Message}", "Error");
        }

        if (string.Equals(_activePickerKey, slotKey, StringComparison.Ordinal))
            ApplyPresetEditingLockUi();
    }

    private static bool TrySelectComboItem(ComboBox combo, string name)
    {
        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (string.Equals(combo.Items[i]?.ToString(), name, StringComparison.OrdinalIgnoreCase))
            {
                combo.SelectedIndex = i;
                return true;
            }
        }

        return false;
    }

    private string? TryReadVanillaXmlSafe()
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
            return null;
        try
        {
            return GetStrippedVanillaXmlForCurrentGame();
        }
        catch
        {
            return null;
        }
    }


    private void RefreshUIFromSessionXml(string xml, bool skipQuickSliders = false)
    {
        _sessionXml = xml;
        _sessionIsFullPreset = true;
        // Cancel any pending Quick→editors sync that would overwrite _sessionXml with Quick-only values
        _syncEditorsDebounceTimer?.Stop();
        // Full session snapshots replace God Mode overlay file so stale overrides cannot fight the grid.
        TryClearAdvOverridesFile();
        // UCM Quick sliders must track _sessionXml even on the default tab (imported presets / Picker loads
        // only ran ApplySessionXmlToAdvancedControls when Advanced was visible — left distance/height/shift stale).
        if (!skipQuickSliders)
            TryApplyQuickSlidersFromSessionXml(xml);
        // Mark editors as needing refresh — they'll pick up _sessionXml when the user switches tabs
        _advCtrlNeedsRefresh = true;
        _expertNeedsRefresh = true;
        // Only refresh the currently active editor
        try
        {
            if (_activeMode == "advanced" && _advCtrlSliders.Count > 0)
                ApplySessionXmlToAdvancedControls(xml);
            else if (_activeMode == "expert" && !string.IsNullOrWhiteSpace(_gameDir))
                EnterExpertMode();
        }
        catch (Exception ex)
        {
            SetStatus($"Warning: could not fully apply preset to editors: {ex.Message}", "Warn");
        }
        SyncPreview();
        SaveCurrentUiState();
        ApplyPresetEditingLockUi();
    }

    private void TryApplyQuickSlidersFromSessionXml(string xml)
    {
        try
        {
            if (!CameraMod.TryParseUcmQuickFootBaselineFromXml(xml, out double dist, out double upv, out double rov))
                return;

            _suppressEvents = true;
            try
            {
                DistSlider.Value = Math.Clamp(dist, DistSlider.Minimum, DistSlider.Maximum);
                HeightSlider.Value = Math.Clamp(upv, HeightSlider.Minimum, HeightSlider.Maximum);
                double shiftDelta = CameraRules.QuickShiftDeltaFromFootZl2RightOffset(rov);
                HShiftSlider.Value = Math.Clamp(shiftDelta, HShiftSlider.Minimum, HShiftSlider.Maximum);
            }
            finally
            {
                _suppressEvents = false;
            }

            DistLabel.Text = $"{DistSlider.Value:F1}";
            HeightLabel.Text = $"{HeightSlider.Value:F1}";
            HShiftLabel.Text = $"{HShiftSlider.Value:F1}";
            CaptureCustomDraft(markDirty: false, updateSelector: false);
            ApplyCenteredLock();
            SyncPreview();
        }
        catch { }
    }

    private static void TryClearAdvOverridesFile()
    {
        try
        {
            if (File.Exists(AdvOverridesPath))
                File.Delete(AdvOverridesPath);
        }
        catch { }
    }

    private void TryRestoreLastInstallSessionAfterGameDirResolved()
    {
        string? xml = GetStringFromSavedState(_savedState, "session_xml");
        if (string.IsNullOrWhiteSpace(xml))
            return;

        try
        {
            TryClearAdvOverridesFile();
            _sessionXml = xml;
            _sessionIsFullPreset = true;
            _advCtrlNeedsRefresh = true;
            _expertNeedsRefresh = true;
            // Don't override Quick sliders — preset loader sets them from settings block.
        }
        catch { }
    }

    private void SetLoadedPresetContext(string name, string kindLabel, string sourceLabel, string statusText, string summaryText, string? url = null)
    {
        _loadedPresetName = name;
        _loadedPresetKindLabel = kindLabel;
        _loadedPresetSourceLabel = sourceLabel;
        _loadedPresetStatusText = statusText;
        _loadedPresetSummaryText = summaryText;
        _loadedPresetUrl = url;
        UpdateLoadedPresetContextUi();
    }

    private void UpdateLoadedPresetContextUi()
    {
        if (ActivePresetName != null)
            ActivePresetName.Text = _loadedPresetName;
        if (ActivePresetDetail != null)
            ActivePresetDetail.Text = _loadedPresetSummaryText;
        if (ActivePresetAuthor != null)
        {
            string src = _loadedPresetSourceLabel;
            ActivePresetAuthor.Text = string.IsNullOrWhiteSpace(src) || src == "scratch" || src == "shipped_camera"
                ? ""
                : $"by {src}";
        }
        if (ActivePresetLinkBtn != null)
        {
            ActivePresetLinkBtn.Visibility = !string.IsNullOrWhiteSpace(_loadedPresetUrl)
                ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void OnActivePresetLinkClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_loadedPresetUrl)) return;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_loadedPresetUrl) { UseShellExecute = true });
        }
        catch { }
    }

    private bool IsActivePresetEditingLocked() =>
        _selectedPresetManagerItem?.IsLocked == true;

    /// <summary>
    /// True when the active preset is a UCM preset and the user is on Fine Tune or God Mode.
    /// UCM Quick remains editable for all presets (personal preference sliders can't break a preset).
    /// Fine Tune and God Mode can silently corrupt a UCM preset's carefully tuned values.
    /// </summary>
    private bool IsActivePresetDeepEditLocked() =>
        _selectedPresetManagerItem?.IsUcmPreset == true &&
        (_activeMode == "advanced" || _activeMode == "expert");

    /// <summary>
    /// True when UCM Quick Global Settings (FOV, Combat, Steadycam, etc.) should be blocked.
    /// Only fires for non-UCM presets the user has manually padlocked.
    /// UCM presets always allow Global Settings edits — they are personal preferences.
    /// </summary>
    private bool IsQuickEditLocked() =>
        IsActivePresetEditingLocked() && _selectedPresetManagerItem?.IsUcmPreset == false;

    /// <summary>
    /// True when the Custom Offsets sliders (Distance, Height, H-Shift) should be blocked.
    /// Blocked for UCM presets (hard-locked by design) and manually padlocked user presets.
    /// </summary>
    private bool IsOffsetsEditLocked() =>
        _selectedPresetManagerItem?.IsUcmPreset == true || IsActivePresetEditingLocked();

    /// <summary>
    /// Applies the two-tier preset lock to the editor UI using opacity-based greying.
    /// Controls remain visible and readable but are visually dimmed and non-interactive.
    ///
    /// Tier 1 — full lock (user-toggled padlock on a non-UCM preset):
    ///   Everything greyed out including UCM Quick.
    ///
    /// Tier 2 — UCM preset:
    ///   Quick panels are fully live (personal preferences — can't corrupt a preset).
    ///   Fine Tune sliders and God Mode grid are greyed out (read-only).
    ///   Fine Tune / God Mode tab buttons are dimmed to signal read-only entry.
    /// </summary>
    private void ApplyPresetEditingLockUi()
    {
        bool isUcmPreset = _selectedPresetManagerItem?.IsUcmPreset == true;
        bool quickLocked = IsQuickEditLocked();
        bool deepLocked  = isUcmPreset || IsActivePresetEditingLocked();

        // Raw XML imports: lock everything except God Mode
        bool rawImportLocked = _sessionIsRawImport;
        bool offsetsLocked = isUcmPreset || quickLocked || rawImportLocked;
        double offsetsOpacity = offsetsLocked ? 0.45 : 1.0;
        QuickOffsetsPanel.Opacity = offsetsOpacity;
        QuickOffsetsPanel.IsHitTestVisible = !offsetsLocked;
        // Also disable the sliders directly so the thumb style's IsEnabled=False trigger fires
        _suppressEvents = true;
        try
        {
            DistSlider.IsEnabled = !offsetsLocked;
            HeightSlider.IsEnabled = !offsetsLocked;
            HShiftSlider.IsEnabled = !offsetsLocked;
        }
        finally { _suppressEvents = false; }

        // Global Settings panel: always full brightness and interactive (FOV, Combat, Steadycam are personal preferences)
        // — unless this is a raw import, in which case UCM controls don't apply
        QuickGlobalPanel.Opacity = (quickLocked || rawImportLocked) ? 0.45 : 1.0;
        QuickGlobalPanel.IsHitTestVisible = !(quickLocked || rawImportLocked);

        if (!quickLocked && !rawImportLocked)
            ApplyCenteredLock();

        // Fine Tune sliders: grey out for UCM presets, hard-locked presets, and raw imports
        bool fineTuneLocked = deepLocked || rawImportLocked;
        double deepOpacity = fineTuneLocked ? 0.38 : 1.0;
        foreach (var slider in _advCtrlAllSliders)
            if (slider != null) { slider.IsEnabled = !fineTuneLocked; slider.Opacity = 1.0; }

        // God Mode: raw imports keep full editing; UCM presets and locked presets are read-only
        ExpertDataGrid.IsReadOnly = deepLocked && !rawImportLocked;
        ExpertDataGrid.Opacity = (deepLocked && !rawImportLocked) ? 0.38 : 1.0;

        // Tab buttons: dim Quick + Fine Tune for raw imports; dim Fine Tune + God Mode for UCM presets
        TabQuick.Opacity = rawImportLocked ? 0.38 : 1.0;
        TabFineTune.Opacity = (isUcmPreset || rawImportLocked) ? 0.38 : 1.0;
        TabGodMode.Opacity  = isUcmPreset ? 0.5 : 1.0;

        if (!fineTuneLocked)
            ApplySteadycamSliderLock();
    }

    private void ApplySteadycamSliderLock()
    {
        if (_advCtrlSliders.Count == 0) return;
        _steadycamKeys ??= CameraRules.GetSteadycamKeys();
        bool steadycamOn = SteadycamCheck.IsChecked == true;
        bool presetLocked = IsActivePresetEditingLocked() || IsActivePresetDeepEditLocked();

        var alreadyProcessed = new HashSet<Slider>(ReferenceEqualityComparer.Instance);
        foreach (var (key, slider) in _advCtrlSliders)
        {
            if (slider == null || alreadyProcessed.Contains(slider)) continue;
            if (!_steadycamKeys.Contains(key)) continue;
            alreadyProcessed.Add(slider);

            bool shouldLock = steadycamOn && !presetLocked;
            slider.IsEnabled = !shouldLock;
            slider.Opacity = shouldLock ? 0.38 : 1.0;
            if (_advCtrlValueLabels.TryGetValue(key, out var lbl) && lbl != null)
                lbl.Opacity = shouldLock ? 0.38 : 1.0;
            if (shouldLock)
                slider.ToolTip = "Controlled by Steadycam — uncheck Steadycam to adjust manually";
        }
    }

    private DateTime _lastLockedToastTime = DateTime.MinValue;

    private void OnEditorPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (IsActivePresetEditingLocked())
            ShowLockedToastIfNeeded();
    }

    /// <summary>Shows a "locked" toast if the user tries to edit while preset is locked. Throttled to 3s.</summary>
    private void ShowLockedToastIfNeeded()
    {
        bool hard = IsActivePresetEditingLocked();
        bool deep = !hard && IsActivePresetDeepEditLocked();
        bool offsets = !hard && !deep && IsOffsetsEditLocked();
        if (!hard && !deep && !offsets) return;
        if ((DateTime.Now - _lastLockedToastTime).TotalSeconds > 3)
        {
            _lastLockedToastTime = DateTime.Now;
            string msg;
            if (deep)
                msg = "\uD83D\uDD12 UCM preset \u2014 duplicate it to use Fine Tune or God Mode";
            else if (offsets)
                msg = "\uD83D\uDD12 UCM preset \u2014 duplicate to adjust distance and height";
            else if (_selectedPresetManagerItem?.IsUcmPreset == true)
                msg = "\uD83D\uDD12 UCM preset \u2014 duplicate to create an editable copy";
            else
                msg = "\uD83D\uDD12 Preset is locked \u2014 unlock the padlock to edit";
            QueueSavedToast(msg, isError: true);
        }
    }

    private void OnPresetManagerSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressPresetPickerActivation)
            return;

        var item = (sender as System.Windows.Controls.Primitives.Selector)?.SelectedItem as PresetManagerItem;
        SetSelectedPresetManagerItem(item, updateDetails: false);

        if (item == null)
        {
            _selectedImportedPreset = null;
            UpdatePresetManagerDetails();
            return;
        }

        _selectedImportedPreset = null;
        ActivatePickerFromSelection(item, skipCapture: false);

        if (item.KindId == "imported" && _selectedImportedPreset != null)
            UpdateImportedPresetDetails();

        UpdatePresetManagerDetails();
        ApplyPresetEditingLockUi();
    }

    private void OnPresetManagerOpen(object sender, RoutedEventArgs e)
    {
        // Manager view removed; sidebar handles preset browsing.
    }

    private async void OnPresetNew(object sender, RoutedEventArgs e)
    {
        var ctrl = new NewPresetDialog();
        var tcs = new TaskCompletionSource<NewPresetDialog?>();
        ctrl.OnResult = result => { CloseOverlay(null); tcs.TrySetResult(result?.PresetName != null ? result : null); };
        _ = ShowOverlayAsync(ctrl, width: 540, height: 900);
        var dlg = await tcs.Task;
        if (dlg == null) return;

        string name = dlg.PresetName;
        string safeName = new string(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        try
        {
            Dictionary<string, object> preset;

            if (dlg.IsManualPreset)
            {
                // Full Manual Control: start from vanilla XML, no UCM rules
                if (string.IsNullOrWhiteSpace(_gameDir))
                {
                    SetStatus("Game folder not set. Cannot create a manual preset without vanilla camera data.", "Warn");
                    return;
                }
                string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);

                preset = new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["author"] = dlg.AuthorName,
                    ["description"] = dlg.Description,
                    ["kind"] = "user",
                    ["locked"] = false,
                    ["preset_mode"] = "godmode",
                    ["session_xml"] = vanillaXml,
                };

                _sessionIsRawImport = true;
                _sessionXml = vanillaXml;
                _sessionIsFullPreset = true;
            }
            else
            {
                // Managed by UCM: current behavior
                CaptureSessionXml();
                string xml = _sessionXml ?? BuildCuratedSessionXml();

                preset = new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["author"] = dlg.AuthorName,
                    ["description"] = dlg.Description,
                    ["kind"] = "user",
                    ["locked"] = false,
                    ["preset_mode"] = "ucm",
                    ["style_id"] = GetSelectedStyleId(),
                    ["session_xml"] = xml,
                    ["settings"] = BuildCurrentPresetSettingsPayload()
                };
            }

            string presetPath = Path.Combine(MyPresetsDir, $"{safeName}.ucmpreset");
            File.WriteAllText(presetPath, JsonSerializer.Serialize(preset, PresetFileJsonOptions));
            RefreshPresetManagerLists(preserveSelection: false);

            // Select the new preset in the sidebar
            var newItem = _presetManagerItems.FirstOrDefault(i => i.Name == name && i.KindId == "user");
            if (newItem != null)
                SetSelectedPresetManagerItem(newItem, updateDetails: true);

            if (dlg.IsManualPreset)
            {
                ApplyPresetEditingLockUi();
                SwitchEditorTab("expert", captureCurrent: false);
            }

            QueueSavedToast($"Preset '{name}' created");
            SetStatus($"Created preset '{name}'.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to create preset: {ex.Message}", "Error");
        }
    }

    private void OnPresetManagerLoad(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        try
        {
            _activePickerKey = null;
            ActivatePickerFromSelection(item, skipCapture: true);

            QueueSavedToast("Preset loaded");
            SetStatus($"Preset '{item.Name}' loaded into the current session.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Load failed: {ex.Message}", "Error");
        }
    }

    private async void OnPresetManagerDelete(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        if (item.IsUcmPreset)
        {
            _ = ShowAlertOverlayAsync("Cannot Delete", "UCM presets cannot be deleted. They are managed by the UCM catalog.");
            return;
        }

        if (!await ShowConfirmOverlayAsync("Delete Preset", $"Delete preset '{item.Name}'? This cannot be undone.", "Delete", "Cancel"))
            return;

        try
        {
            if (File.Exists(item.FilePath))
                File.Delete(item.FilePath);

            if (item.KindId == "imported")
                _selectedImportedPreset = null;

            RefreshPresetManagerLists(preserveSelection: false);
            QueueSavedToast("Preset deleted");
            SetStatus($"Preset '{item.Name}' deleted.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Delete failed: {ex.Message}", "Error");
        }
    }

    private async void OnPresetManagerRename(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        if (item.IsUcmPreset)
        {
            _ = ShowAlertOverlayAsync("Cannot Rename", "UCM presets cannot be renamed. Duplicate it first to create your own editable copy.");
            return;
        }

        string? response = await ShowInputOverlayAsync("Rename Preset", "Enter the new preset name:", item.Name);
        if (string.IsNullOrWhiteSpace(response))
            return;

        string newName = SanitizeFileStem(response);
        if (string.Equals(newName, item.Name, StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            string newPath = item.KindId == "imported"
                ? ImportedPresetPath(newName)
                : Path.Combine(Path.GetDirectoryName(item.FilePath) ?? ExeDir, $"{newName}.ucmpreset");

            if (File.Exists(newPath))
            {
                SetStatus($"A preset named '{newName}' already exists.", "Warn");
                return;
            }

            if (item.KindId == "imported")
            {
                var preset = LoadImportedPreset(item.Name) ?? throw new InvalidOperationException("Imported preset could not be loaded — the file may be missing, corrupted, or in an unsupported format.");
                preset.Name = newName;
                SaveImportedPreset(preset);
                if (File.Exists(item.FilePath))
                    File.Delete(item.FilePath);
                _selectedImportedPreset = preset;
            }
            else
            {
                // Update the name field inside the preset file, then rename
                try
                {
                    string json = File.ReadAllText(item.FilePath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
                    dict["name"] = response.Trim();
                    File.WriteAllText(item.FilePath, JsonSerializer.Serialize(dict, PresetFileJsonOptions));
                }
                catch { /* proceed with rename even if internal update fails */ }
                File.Move(item.FilePath, newPath);
            }

            RefreshPresetManagerLists(preserveSelection: false);
            var renamed = _presetManagerItems.FirstOrDefault(i => i.KindId == item.KindId && i.Name == newName);
            if (renamed != null)
                SetSelectedPresetManagerItem(renamed, updateDetails: true);

            if (string.Equals(_loadedPresetName, item.Name, StringComparison.OrdinalIgnoreCase))
                SetLoadedPresetContext(newName, _loadedPresetKindLabel, _loadedPresetSourceLabel, _loadedPresetStatusText, _loadedPresetSummaryText);

            QueueSavedToast("Preset renamed");
            SetStatus($"Preset '{item.Name}' renamed to '{newName}'.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Rename failed: {ex.Message}", "Error");
        }
    }

    private async void OnPresetManagerDuplicate(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        string? response = await ShowInputOverlayAsync("Duplicate Preset", "Enter a name for the duplicated preset:", $"{item.Name}_copy");
        if (string.IsNullOrWhiteSpace(response))
            return;

        string newName = SanitizeFileStem(response);
        try
        {
            bool promoteUcmToMyPresets = !string.Equals(item.KindId, "imported", StringComparison.OrdinalIgnoreCase)
                && (string.Equals(item.KindId, "default", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(item.KindId, "style", StringComparison.OrdinalIgnoreCase));

            string newPath = item.KindId == "imported"
                ? ImportedPresetPath(newName)
                : promoteUcmToMyPresets
                    ? Path.Combine(MyPresetsDir, $"{newName}.ucmpreset")
                    : Path.Combine(Path.GetDirectoryName(item.FilePath) ?? ExeDir, $"{newName}.ucmpreset");

            if (File.Exists(newPath))
            {
                SetStatus($"A preset named '{newName}' already exists.", "Warn");
                return;
            }

            if (item.KindId == "imported")
            {
                var preset = LoadImportedPreset(item.Name) ?? throw new InvalidOperationException("Imported preset could not be loaded — the file may be missing, corrupted, or in an unsupported format.");
                preset.Name = newName;
                preset.Locked = false;
                SaveImportedPreset(preset);
            }
            else
            {
                // Copy file but update the name field inside the JSON
                string json = File.ReadAllText(item.FilePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                    ?? new Dictionary<string, object>();
                dict["name"] = response.Trim();
                dict["locked"] = false;
                if (promoteUcmToMyPresets)
                    dict["kind"] = "user";
                File.WriteAllText(newPath, JsonSerializer.Serialize(dict, PresetFileJsonOptions));
            }

            RefreshPresetManagerLists(preserveSelection: false);
            var duplicated = _presetManagerItems.FirstOrDefault(i =>
                string.Equals(i.FilePath, newPath, StringComparison.OrdinalIgnoreCase));
            if (duplicated != null)
            {
                SetSelectedPresetManagerItem(duplicated, updateDetails: true);
                // Fully activate the duplicate so _sessionXml, Quick sliders, and Fine Tune
                // all reflect the new preset rather than the source preset's stale state.
                _activePickerKey = null;
                ActivatePickerFromSelection(duplicated, skipCapture: true);
            }

            QueueSavedToast("Preset duplicated");
            SetStatus($"Preset '{item.Name}' duplicated as '{newName}'.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Duplicate failed: {ex.Message}", "Error");
        }
    }

    private void OnPresetManagerRebuild(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        if (item.KindId != "imported")
        {
            SetStatus("Only imported XML / 0.paz presets need rebuild support.", "TextSecondary");
            return;
        }

        try
        {
            var preset = LoadImportedPreset(item.Name) ?? throw new InvalidOperationException("Imported preset could not be loaded — the file may be missing, corrupted, or in an unsupported format.");
            _selectedImportedPreset = preset;
            // Use raw XML directly if available — don't apply CameraRules on top
            string xml;
            if (!string.IsNullOrWhiteSpace(preset.RawXml))
            {
                xml = CameraMod.StripComments(preset.RawXml.TrimStart('\uFEFF'));
                _sessionIsRawImport = true;
            }
            else
            {
                xml = BuildRebuiltXmlFromImportedPreset(preset);
            }
            RefreshUIFromSessionXml(xml);
            MarkImportedPresetAsBuilt(preset, refreshPresetSidebar: false);
            SetLoadedPresetContext(preset.Name,
                ImportedPresetKindLabel(preset.SourceType),
                preset.SourceDisplayName,
                BuildImportedPresetStatusText(preset),
                "This imported preset has been rebuilt against the current game and is ready to export or inspect further.");
            QueueSavedToast("Preset rebuilt");
            SetStatus($"Imported preset '{preset.Name}' rebuilt for the current game.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Rebuild failed: {ex.Message}", "Error");
        }
    }

    private void OnPresetManagerGenerate(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        if (item.KindId != "imported")
        {
            SetStatus("Load this preset into the current session first, then generate from Export JSON.", "TextSecondary");
            return;
        }

        OnImportedPresetGenerateJson(sender, e);
    }

    private void OnPresetSidebarLockClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not PresetManagerItem item)
            return;

        if (item.IsUcmPreset)
            return;

        item.IsLocked = !item.IsLocked;
        PersistPresetLockField(item);

        if (_selectedImportedPreset != null
            && string.Equals(item.KindId, "imported", StringComparison.OrdinalIgnoreCase)
            && string.Equals(item.Name, _selectedImportedPreset.Name, StringComparison.OrdinalIgnoreCase))
            _selectedImportedPreset.Locked = item.IsLocked;

        if (ReferenceEquals(_selectedPresetManagerItem, item))
            ApplyPresetEditingLockUi();

        SetStatus(item.IsLocked
                ? $"Preset '{item.Name}' is locked — unlock the padlock to edit."
                : $"Preset '{item.Name}' is unlocked.",
            item.IsLocked ? "Warn" : "TextDim");
    }

    private void PersistPresetLockField(PresetManagerItem item)
    {
        if (string.IsNullOrWhiteSpace(item.FilePath) || !File.Exists(item.FilePath))
            return;

        try
        {
            if (string.Equals(item.KindId, "imported", StringComparison.OrdinalIgnoreCase))
            {
                var preset = LoadImportedPreset(item.Name);
                if (preset == null) return;
                preset.Locked = item.IsLocked;
                SaveImportedPreset(preset);
                _selectedImportedPreset = preset;
                return;
            }

            string text = File.ReadAllText(item.FilePath);
            JsonNode? root = JsonNode.Parse(text);
            if (root is not JsonObject obj) return;
            obj["locked"] = item.IsLocked;
            File.WriteAllText(item.FilePath, root.ToJsonString(PresetFileJsonOptions));
        }
        catch
        {
            // Keep UI lock state even if disk write fails; next refresh may resync from file.
        }
    }

    private PresetManagerItem? FindPresetItemByKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var item = _presetManagerItems.FirstOrDefault(i =>
            string.Equals(PresetManagerKey(i), key, StringComparison.Ordinal));

        if (item == null && _selectedPresetManagerItem != null &&
            string.Equals(PresetManagerKey(_selectedPresetManagerItem), key, StringComparison.Ordinal))
            item = _selectedPresetManagerItem;

        return item;
    }

    private Dictionary<string, object> BuildCurrentPresetSettingsPayload()
    {
        return new Dictionary<string, object>
        {
            ["distance"] = Math.Round(DistSlider.Value, 2),
            ["height"] = Math.Round(HeightSlider.Value, 2),
            ["right_offset"] = Math.Round(HShiftSlider.Value, 2),
            ["fov"] = GetSelectedFov(),
            ["combat_pullback"] = GetCombatPullback(),
            ["centered"] = BaneCheck.IsChecked == true,
            ["mount_height"] = MountHeightCheck.IsChecked == true,
            ["steadycam"] = SteadycamCheck.IsChecked == true
        };
    }

    private void SavePresetManagerItemSession(PresetManagerItem item)
    {
        if (item.IsLocked)
            return;

        if (string.IsNullOrWhiteSpace(_sessionXml))
            return;

        if (string.Equals(item.KindId, "imported", StringComparison.OrdinalIgnoreCase))
        {
            var imported = LoadImportedPreset(item.Name);
            if (imported == null)
                return;

            imported.Values = BuildImportedValueMap(_sessionXml);
            var fp = TryGetCurrentGameFingerprint();
            if (fp != null)
            {
                imported.LastBuiltAgainst = fp;
                imported.LastBuiltAtUtc = DateTime.UtcNow;
            }

            SaveImportedPreset(imported);
            _selectedImportedPreset = imported;
            return;
        }

        if (string.IsNullOrWhiteSpace(item.FilePath))
            return;

        // Check if this is a godmode preset by reading the existing file
        bool isGodMode = false;
        try
        {
            if (File.Exists(item.FilePath))
            {
                string existing = File.ReadAllText(item.FilePath);
                using var existingDoc = JsonDocument.Parse(existing);
                if (existingDoc.RootElement.TryGetProperty("preset_mode", out var pm)
                    && pm.ValueKind == JsonValueKind.String
                    && pm.GetString() == "godmode")
                    isGodMode = true;
            }
        }
        catch { /* proceed with default UCM mode */ }

        var preset = new Dictionary<string, object>
        {
            ["name"] = item.Name,
            ["author"] = item.SourceLabel,
            ["description"] = item.StatusText,
            ["kind"] = item.KindId,
            ["locked"] = item.IsLocked,
        };

        if (isGodMode)
        {
            preset["preset_mode"] = "godmode";
            preset["session_xml"] = _sessionXml;
        }
        else
        {
            preset["preset_mode"] = "ucm";
            preset["style_id"] = GetSelectedStyleId();
            preset["session_xml"] = _sessionXml;
            preset["settings"] = BuildCurrentPresetSettingsPayload();
        }

        File.WriteAllText(item.FilePath, JsonSerializer.Serialize(preset, PresetFileJsonOptions));
    }

    private void AutosaveActivePresetFileIfNeeded()
    {
        try
        {
            var item = FindPresetItemByKey(_activePickerKey);
            if (item == null || item.IsLocked)
                return;

            SavePresetManagerItemSession(item);
        }
        catch
        {
            // Keep transient session save resilient even if preset file autosave fails.
        }
    }

    private void GenerateBuiltInPresets()
    {
        if (string.IsNullOrEmpty(_gameDir)) return;
        try
        {
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);

            // --- Vanilla (default) preset: always game-specific, generated locally ---
            string vanillaLabel = Styles.FirstOrDefault(s => s.Id == "default").Label ?? "Vanilla  -  Unmodified game camera (use Steadycam for UCM smoothing)";
            string vanillaName = vanillaLabel.Split("  -  ")[0].Trim();
            string vanillaPath = Path.Combine(UcmPresetsDir, vanillaName + ".ucmpreset");
            if (VanillaBuiltInPresetNeedsRefresh(vanillaPath))
            {
                var (dist, up, ro) = StyleParams["default"];
                double rightOffsetSetting;
                if (!CameraMod.TryParseUcmQuickFootBaselineFromXml(vanillaXml, out dist, out up, out double roAbs))
                    rightOffsetSetting = ro;
                else
                    rightOffsetSetting = CameraRules.QuickShiftDeltaFromFootZl2RightOffset(roAbs);

                var vanillaPreset = new Dictionary<string, object>
                {
                    ["name"] = vanillaName,
                    ["author"] = "0xFitz",
                    ["description"] = vanillaLabel.Contains("  -  ") ? vanillaLabel.Split("  -  ")[1].Trim() : "",
                    ["kind"] = "default",
                    ["locked"] = true,
                    ["style_id"] = "default",
                    ["vanilla_preset_rev"] = VanillaBuiltinPresetRevision,
                    ["session_xml"] = vanillaXml,
                    ["settings"] = new Dictionary<string, object>
                    {
                        ["distance"] = Math.Round(dist, 2),
                        ["height"] = Math.Round(up, 2),
                        ["right_offset"] = Math.Round(rightOffsetSetting, 2),
                        ["fov"] = 0,
                        ["combat_pullback"] = 0.0,
                        ["centered"] = false,
                        ["mount_height"] = false,
                        ["steadycam"] = false
                    }
                };
                File.WriteAllText(vanillaPath, JsonSerializer.Serialize(vanillaPreset, PresetFileJsonOptions));
            }

            // --- UCM style presets: bake session_xml from definitions ---
            // UCM presets in the repo are definition-only (style_id + settings, no session_xml).
            // We generate session_xml here from vanilla XML + current CameraRules so presets
            // always reflect the latest rules (lock-on scaling, steadycam, etc.).
            foreach (string path in Directory.GetFiles(UcmPresetsDir, "*.ucmpreset"))
            {
                try
                {
                    string head;
                    using (var reader = new StreamReader(path))
                    {
                        var buf = new char[512];
                        int read = reader.Read(buf, 0, buf.Length);
                        head = new string(buf, 0, read);
                    }
                    string? kind = ExtractJsonStringField(head, "kind");
                    if (kind != "style") continue;

                    if (!UcmStylePresetNeedsRefresh(path)) continue;

                    string json = File.ReadAllText(path);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    string styleId = root.TryGetProperty("style_id", out var sidEl) ? sidEl.GetString() ?? "" : "";
                    if (string.IsNullOrEmpty(styleId) || !StyleParams.ContainsKey(styleId)) continue;

                    int fov = 25;
                    double combatPullback = 0.0;
                    bool mountHeight = false;
                    bool steadycam = true;
                    if (root.TryGetProperty("settings", out var settingsEl))
                    {
                        if (settingsEl.TryGetProperty("fov", out var fovEl) && fovEl.ValueKind == JsonValueKind.Number)
                            fov = fovEl.GetInt32();
                        if (settingsEl.TryGetProperty("combat_pullback", out var cpEl) && cpEl.ValueKind == JsonValueKind.Number)
                            combatPullback = cpEl.GetDouble();
                        if (settingsEl.TryGetProperty("mount_height", out var mhEl))
                            mountHeight = mhEl.ValueKind == JsonValueKind.True;
                        if (settingsEl.TryGetProperty("steadycam", out var scEl))
                            steadycam = scEl.ValueKind == JsonValueKind.True;
                    }

                    var modSet = CameraRules.BuildModifications(styleId, fov, false,
                        combatPullback: combatPullback, mountHeight: mountHeight, steadycam: steadycam);
                    string builtXml = CameraMod.ApplyModifications(vanillaXml, modSet);

                    var rebuilt = new Dictionary<string, object>();
                    foreach (var prop in root.EnumerateObject())
                    {
                        if (prop.Name is "session_xml" or "ucm_preset_rev") continue;
                        rebuilt[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText()) ?? "";
                    }
                    rebuilt["session_xml"] = builtXml;
                    rebuilt["ucm_preset_rev"] = UcmStylePresetRevision;
                    File.WriteAllText(path, JsonSerializer.Serialize(rebuilt, PresetFileJsonOptions));
                }
                catch { /* skip malformed files */ }
            }

            // Deploy shipped community presets from embedded resources
            DeployShippedCommunityPresets();
        }
        catch (Exception ex)
        {
            SetStatus("Failed to generate built-in presets.", "Warn");
            ShowFatalOverlayAndClose("Failed to Generate Presets", ex.Message);
        }
    }

    /// <summary>
    /// Bump this whenever CameraRules changes in a way that affects UCM style preset output
    /// (e.g. lock-on scaling, steadycam smoothing, section exclusions). Forces all existing
    /// style presets to be regenerated on next launch so they stay in sync with the live rules.
    /// </summary>
    private const int UcmStylePresetRevision = 3;

    private static bool UcmStylePresetNeedsRefresh(string filePath)
    {
        if (!File.Exists(filePath)) return true;
        try
        {
            using var reader = new StreamReader(filePath);
            var buf = new char[4096];
            int read = reader.Read(buf, 0, buf.Length);
            string head = new string(buf, 0, read);
            string needle = $"\"ucm_preset_rev\":{UcmStylePresetRevision}";
            string needleSpaced = $"\"ucm_preset_rev\": {UcmStylePresetRevision}";
            bool hasCurrentRev = head.Contains(needle, StringComparison.Ordinal)
                              || head.Contains(needleSpaced, StringComparison.Ordinal);
            bool hasSessionXml = head.Contains("\"session_xml\"", StringComparison.Ordinal);
            return !hasCurrentRev || !hasSessionXml;
        }
        catch { return true; }
    }

    private void DeployShippedCommunityPresets()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string prefix = "UltimateCameraMod.V3.ShippedPresets.";

        foreach (string resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            if (!resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) continue;

            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;
                using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                string json = reader.ReadToEnd();

                // Parse the imported preset format
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                string name = root.TryGetProperty("Name", out var nv) ? nv.GetString() ?? "" : "";
                if (string.IsNullOrWhiteSpace(name)) continue;

                string destPath = Path.Combine(UcmPresetsDir, name + ".ucmpreset");
                // Also skip if legacy .json version exists (user may have modified it)
                string legacyPath = Path.Combine(UcmPresetsDir, name + ".json");
                if (File.Exists(destPath) || File.Exists(legacyPath)) continue;

                string author = root.TryGetProperty("Author", out var av) ? av.GetString() ?? "" : "";
                string desc = root.TryGetProperty("Description", out var dv) ? dv.GetString() ?? "" : "";
                string url = root.TryGetProperty("Url", out var uv) ? uv.GetString() ?? "" : "";
                string rawXml = root.TryGetProperty("RawXml", out var xv) ? xv.GetString() ?? "" : "";

                if (string.IsNullOrWhiteSpace(rawXml)) continue;

                // Strip BOM and comments for clean session XML
                string sessionXml = rawXml.TrimStart('\uFEFF');
                sessionXml = CameraMod.StripComments(sessionXml);

                // Extract ZL2 baseline for Quick slider settings
                double dist = 5.0, height = 0.0, roff = 0.0;
                if (CameraMod.TryParseUcmQuickFootBaselineFromXml(sessionXml, out double d, out double u, out double ro))
                {
                    dist = d; height = u;
                    roff = CameraRules.QuickShiftDeltaFromFootZl2RightOffset(ro);
                }

                // url, name, author, description must all appear before session_xml so they
                // fall within the 4 KB header window read by AppendSessionJsonPresetsFromDir.
                var preset = new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["author"] = author,
                    ["url"] = url,
                    ["description"] = desc.Replace("\n", " ").Trim(),
                    ["kind"] = "style",
                    ["locked"] = true,
                    ["settings"] = new Dictionary<string, object>
                    {
                        ["distance"] = Math.Round(dist, 2),
                        ["height"] = Math.Round(height, 2),
                        ["right_offset"] = Math.Round(roff, 2),
                        ["fov"] = 0,
                        ["combat_pullback"] = 0.0,
                        ["centered"] = false,
                        ["mount_height"] = false,
                        ["steadycam"] = false
                    },
                    ["session_xml"] = sessionXml,
                };

                File.WriteAllText(destPath, JsonSerializer.Serialize(preset, PresetFileJsonOptions));
            }
            catch { /* Skip malformed shipped presets */ }
        }
    }

    private static void MigrateJsonToUcmPreset(string dir)
    {
        if (!Directory.Exists(dir)) return;
        foreach (string jsonFile in Directory.GetFiles(dir, "*.json"))
        {
            string ucmPresetFile = Path.ChangeExtension(jsonFile, ".ucmpreset");
            if (!File.Exists(ucmPresetFile))
            {
                try { File.Move(jsonFile, ucmPresetFile); }
                catch { /* skip if rename fails */ }
            }
        }
    }

    private const int VanillaBuiltinPresetRevision = 3;


    private static bool VanillaBuiltInPresetNeedsRefresh(string filePath)
    {
        // Check both .ucmpreset and .json variants
        if (!File.Exists(filePath))
        {
            string alt = Path.GetExtension(filePath).Equals(".ucmpreset", StringComparison.OrdinalIgnoreCase)
                ? Path.ChangeExtension(filePath, ".json")
                : Path.ChangeExtension(filePath, ".ucmpreset");
            if (File.Exists(alt))
                filePath = alt;
            else
                return true;
        }
        try
        {
            using var reader = new StreamReader(filePath);
            var buf = new char[8192];
            int read = reader.Read(buf, 0, buf.Length);
            string head = new string(buf, 0, read);
            string needle = $"\"vanilla_preset_rev\":{VanillaBuiltinPresetRevision}";
            string needleSpaced = $"\"vanilla_preset_rev\": {VanillaBuiltinPresetRevision}";
            return !head.Contains(needle, StringComparison.Ordinal)
                   && !head.Contains(needleSpaced, StringComparison.Ordinal);
        }
        catch
        {
            return true;
        }
    }

    /// <summary>Moves imported_presets/ to import_presets/ (one-time rename).</summary>
    private static void MigrateImportedPresetsFolderIfNeeded()
    {
        if (_importPresetsFolderMigrated)
            return;
        _importPresetsFolderMigrated = true;

        string oldDir = Path.Combine(ExeDir, LegacyImportedPresetsDirName);
        if (!Directory.Exists(oldDir))
            return;

        string newDir = Path.Combine(ExeDir, ImportPresetsDirName);
        Directory.CreateDirectory(newDir);

        foreach (string file in Directory.GetFiles(oldDir, "*.json", SearchOption.TopDirectoryOnly))
        {
            string fileName = Path.GetFileName(file) ?? "preset.json";
            string dest = Path.Combine(newDir, fileName);
            int n = 0;
            while (File.Exists(dest))
            {
                n++;
                dest = Path.Combine(newDir, $"{Path.GetFileNameWithoutExtension(fileName)}_{n}.json");
            }

            try
            {
                File.Move(file, dest);
            }
            catch
            {
                // leave file in legacy folder
            }
        }

        try
        {
            if (!Directory.EnumerateFileSystemEntries(oldDir).Any())
                Directory.Delete(oldDir);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>Moves legacy flat presets/ into ucm_presets (default, style) and my_presets (user).</summary>
    private static void MigrateLegacyPresetFoldersIfNeeded()
    {
        if (_legacyPresetFoldersMigrated)
            return;
        _legacyPresetFoldersMigrated = true;

        string legacy = Path.Combine(ExeDir, LegacyPresetsDirName);
        if (!Directory.Exists(legacy))
            return;

        Directory.CreateDirectory(UcmPresetsDir);
        Directory.CreateDirectory(MyPresetsDir);

        foreach (string file in Directory.GetFiles(legacy, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                string header;
                using (var reader = new StreamReader(file))
                {
                    var buf = new char[2048];
                    int read = reader.Read(buf, 0, buf.Length);
                    header = new string(buf, 0, read);
                }

                string kind = ExtractJsonStringField(header, "kind") ?? "user";
                string destDir = (kind == "default" || kind == "style") ? UcmPresetsDir : MyPresetsDir;
                string destName = Path.GetFileName(file) ?? "preset.json";
                string dest = Path.Combine(destDir, destName);
                int n = 0;
                while (File.Exists(dest))
                {
                    n++;
                    dest = Path.Combine(destDir, $"{Path.GetFileNameWithoutExtension(destName)}_{n}.json");
                }

                File.Move(file, dest);
            }
            catch
            {
                // leave unmigrated file in legacy folder
            }
        }

        try
        {
            if (!Directory.EnumerateFileSystemEntries(legacy).Any())
                Directory.Delete(legacy);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>Get both .ucmpreset and .json files from a directory (for backwards-compatible preset scanning).</summary>
    private static IEnumerable<string> GetPresetFiles(string dir)
    {
        if (!Directory.Exists(dir)) return Enumerable.Empty<string>();
        return Directory.GetFiles(dir, "*.ucmpreset")
            .Concat(Directory.GetFiles(dir, "*.json"))
            .Where(f => !Path.GetFileName(f).StartsWith('.'))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
    }

    private static void AppendPresetCatalogDirFingerprints(StringBuilder sb)
    {
        static void AppendDir(StringBuilder sb2, string dir, string pattern)
        {
            try
            {
                if (!Directory.Exists(dir))
                    return;
                foreach (string f in Directory.GetFiles(dir, pattern).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    sb2.Append(f).Append('=').Append(File.GetLastWriteTimeUtc(f).Ticks).Append(';');
            }
            catch
            {
                // ignore IO errors for fingerprint
            }
        }

        AppendDir(sb, Path.Combine(ExeDir, UcmPresetsDirName), "*.ucmpreset");
        AppendDir(sb, Path.Combine(ExeDir, UcmPresetsDirName), "*.json");
        AppendDir(sb, Path.Combine(ExeDir, MyPresetsDirName), "*.ucmpreset");
        AppendDir(sb, Path.Combine(ExeDir, MyPresetsDirName), "*.json");
        AppendDir(sb, Path.Combine(ExeDir, CommunityPresetsDirName), "*.ucmpreset");
        AppendDir(sb, Path.Combine(ExeDir, CommunityPresetsDirName), "*.json");
        AppendDir(sb, Path.Combine(ExeDir, LegacyPresetsDirName), "*.json");
        AppendDir(sb, Path.Combine(ExeDir, ImportPresetsDirName), "*.json");
    }

    private static string BuildCatalogFingerprintCore(ImportedPresetFingerprint? gameFp, string gameDirKey)
    {
        var sb = new StringBuilder(512);
        sb.Append(gameDirKey ?? "");
        sb.Append('|');
        sb.Append(gameFp?.ContentSha256 ?? "nogame");
        sb.Append('|');
        AppendPresetCatalogDirFingerprints(sb);
        return sb.ToString();
    }

    private string ComputePresetCatalogFingerprint(out ImportedPresetFingerprint? gameFp)
    {
        gameFp = TryGetGameFingerprintForDir(_gameDir);
        return BuildCatalogFingerprintCore(gameFp, _gameDir ?? "");
    }

}
