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
    // â"€â"€ Undo state â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private sealed record UndoSnapshot(double Dist, double Height, double HShift, int FovIdx, double CombatPb, bool Bane, bool Mount, bool Steadycam);
    private readonly Stack<UndoSnapshot> _undoStack = new();
    private const int MaxUndoDepth = 20;

    private void CaptureUndoSnapshot()
    {
        var snap = new UndoSnapshot(
            DistSlider.Value, HeightSlider.Value, HShiftSlider.Value,
            FovCombo.SelectedIndex, CombatPullbackSlider?.Value ?? 0,
            BaneCheck.IsChecked == true, MountHeightCheck.IsChecked == true,
            SteadycamCheck.IsChecked == true);
        _undoStack.Push(snap);
        if (_undoStack.Count > MaxUndoDepth)
        {
            // Trim to max depth
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = Math.Min(items.Length - 1, MaxUndoDepth - 1); i >= 0; i--)
                _undoStack.Push(items[i]);
        }
    }

    private void OnUndo(object sender, RoutedEventArgs e)
    {
        if (_undoStack.Count == 0) return;
        var snap = _undoStack.Pop();
        _suppressEvents = true;
        try
        {
            DistSlider.Value = snap.Dist;
            HeightSlider.Value = snap.Height;
            HShiftSlider.Value = snap.HShift;
            FovCombo.SelectedIndex = Math.Clamp(snap.FovIdx, 0, FovCombo.Items.Count - 1);
            if (CombatPullbackSlider != null) CombatPullbackSlider.Value = snap.CombatPb;
            BaneCheck.IsChecked = snap.Bane;
            MountHeightCheck.IsChecked = snap.Mount;
            SteadycamCheck.IsChecked = snap.Steadycam;
        }
        finally
        {
            _suppressEvents = false;
        }
        DistLabel.Text = $"{DistSlider.Value:F1}";
        HeightLabel.Text = $"{HeightSlider.Value:F1}";
        HShiftLabel.Text = $"{HShiftSlider.Value:F1}";
        ApplyCenteredLock();
        SyncPreview();
        ScheduleSyncQuickSettingsToEditors();
        SaveCurrentUiState();
        QueueSavedToast("Undone");
    }

    // â"€â"€ Tab switching â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    // SwitchTab removed — always in "custom" mode in unified preset system.

    // â"€â"€ 4-Mode navigation â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private void OnTabQuick(object s, RoutedEventArgs e) => SwitchEditorTab("simple");
    private void OnTabFineTune(object s, RoutedEventArgs e) => SwitchEditorTab("advanced");
    private void OnTabGodMode(object s, RoutedEventArgs e) => SwitchEditorTab("expert");

    private void SwitchEditorTab(string tab, bool captureCurrent = true)
    {
        if (tab != "simple" && tab != "advanced" && tab != "expert")
            return;

        // Raw XML imports only support God Mode editing
        if (_sessionIsRawImport && (tab == "simple" || tab == "advanced"))
        {
            MessageBox.Show(
                "This preset was imported as raw XML and is not managed by UCM.\n\n" +
                "UCM Quick and Fine Tune use UCM's camera rule system which would override " +
                "values from the imported mod. To protect your import, only God Mode editing " +
                "is available.\n\n" +
                "To use UCM features like Steadycam and FoV control, create a new UCM preset " +
                "from the sidebar instead.",
                "Raw XML Import",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        // Rebuild session XML from the current editor when switching tabs — unless a full
        // preset was loaded externally (import, preset picker) and the user hasn't edited
        // Quick sliders since.  Quick sliders only cover a subset of values; rebuilding from
        // them would discard Fine Tune / God Mode values the loaded session carries.
        if (captureCurrent && _activeMode != tab && !_sessionIsFullPreset)
            CaptureSessionXml();

        if (tab != "simple" && string.IsNullOrEmpty(_gameDir))
        {
            MessageBox.Show("Game-file install is not available in v3. Use Export (sidebar) for JSON, XML, or 0.paz sharing.",
                "Ultimate Camera Mod", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // UCM presets are read-only in Fine Tune and God Mode to prevent accidental corruption.
        // Offer to duplicate before entering these tabs.
        if ((tab == "advanced" || tab == "expert") && _selectedPresetManagerItem?.IsUcmPreset == true)
        {
            string tabName = tab == "advanced" ? "Fine Tune" : "God Mode";
            var result = MessageBox.Show(
                $"UCM presets are protected — {tabName} changes could corrupt the preset's carefully tuned values.\n\n" +
                "Duplicate this preset first to create your own editable copy, then use Fine Tune or God Mode freely.\n\n" +
                $"Open {tabName} in read-only mode anyway? (You can browse values but changes won't save.)",
                $"UCM Preset — {tabName}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
            if (result == MessageBoxResult.No)
                return;
        }

        // Hide all editor views
        SimpleView.Visibility = Visibility.Collapsed;
        AdvancedControlsView.Visibility = Visibility.Collapsed;
        ExpertView.Visibility = Visibility.Collapsed;

        // Update tab button styles + foreground (dark text on gold when active, bright white when inactive)
        var darkFg = new SolidColorBrush(Color.FromRgb(0x1a, 0x1a, 0x1a));
        var brightFg = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        TabQuick.Style = tab == "simple" ? _accentButtonStyle : _subtleButtonStyle;
        TabQuick.Foreground = tab == "simple" ? darkFg : brightFg;
        TabFineTune.Style = tab == "advanced" ? _accentButtonStyle : _subtleButtonStyle;
        TabFineTune.Foreground = tab == "advanced" ? darkFg : brightFg;
        TabGodMode.Style = tab == "expert" ? _accentButtonStyle : _subtleButtonStyle;
        TabGodMode.Foreground = tab == "expert" ? darkFg : brightFg;

        _activeMode = tab;
        _isExpertMode = tab == "expert";

        switch (tab)
        {
            case "simple":
                SimpleView.Visibility = Visibility.Visible;
                CheckForUpdate();
                SetStatus("UCM Quick — broad camera shaping with previews and common controls.", "TextDim");
                break;

            case "advanced":
                AdvancedControlsView.Visibility = Visibility.Visible;
                _advCtrlNeedsRefresh = false;
                EnterAdvancedControlsMode();
                SetStatus("UCM Fine Tune — curated deeper tuning layered on top of UCM Quick.", "TextDim");
                break;

            case "expert":
                ExpertView.Visibility = Visibility.Visible;
                _expertNeedsRefresh = false;
                EnterExpertMode();
                break;
        }

        ApplyPresetEditingLockUi();
    }

    // Legacy compat shim — internal callers still use SwitchAppMode in some flows.
    private void SwitchAppMode(string mode, bool captureCurrent = true)
    {
        // Map old mode names to the new 3-tab model
        switch (mode)
        {
            case "home":
            case "simple":
            case "manager":
                SwitchEditorTab("simple", captureCurrent);
                break;
            case "advanced":
                SwitchEditorTab("advanced", captureCurrent);
                break;
            case "expert":
            case "json":
                SwitchEditorTab("expert", captureCurrent);
                break;
        }
    }

    private void CaptureSessionXml()
    {
        if (string.IsNullOrEmpty(_gameDir)) return;
        if (_sessionIsRawImport) return; // Raw imports must not be rebuilt through CameraRules

        try
        {
            _sessionXml = BuildSessionXmlForMode(_activeMode);
            _sessionIsFullPreset = false;
        }
        catch
        {
            // Keep the last good session XML if capture fails during navigation.
        }
    }

    private string BuildSessionXmlForMode(string mode)
    {
        return mode switch
        {
            "expert" => BuildGodModeSessionXml(),
            _ => BuildCuratedSessionXml(),
        };
    }

    private string BuildSimpleSessionXml()
    {
        string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
        return CameraMod.ApplyModifications(vanillaXml, BuildCurrentSimpleModSet());
    }

    private string BuildCuratedSessionXml()
    {
        string xml = BuildSimpleSessionXml();
        if (_advCtrlSliders.Count == 0)
            return xml;

        xml = CameraMod.ApplyModifications(xml, BuildAdvancedControlsModSet());

        // If Steadycam is on, re-sync lock-on ZoomDistances to whatever on-foot ZoomDistances
        // ended up in the XML after Fine Tune overrides. Fine Tune may have changed ZL2/ZL3/ZL4
        // on Player_Basic_Default, so we read them back and re-apply BuildLockOnDistances so
        // lock-on always mirrors the user's actual chosen distances.
        if (SteadycamCheck.IsChecked == true
            && CameraMod.TryParseOnFootZoomDistances(xml, out double zl2, out double zl3, out double zl4))
        {
            var lockOnSync = new ModificationSet { ElementMods = CameraRules.BuildLockOnDistancesPublic(zl2, zl3, zl4), FovValue = 0 };
            xml = CameraMod.ApplyModifications(xml, lockOnSync);
        }

        return xml;
    }

    private string BuildGodModeSessionXml()
    {
        // Raw imports: start from the imported XML, only layer explicit God Mode edits.
        // No UCM rules (FoV normalization, Steadycam, etc.) applied.
        if (_sessionIsRawImport && !string.IsNullOrWhiteSpace(_sessionXml))
        {
            return CameraMod.ApplyModifications(_sessionXml, BuildExpertModSet());
        }

        // Start from the simple session (Steadycam, style, FOV, bane, etc. already applied)
        // then layer God Mode's explicit overrides on top. This ensures Steadycam and all
        // Quick settings are present in exports even when the user is on the God Mode tab.
        string baseXml = BuildSimpleSessionXml();
        string xml = CameraMod.ApplyModifications(baseXml, BuildExpertModSet());

        // Re-sync lock-on distances to whatever on-foot ZoomDistances ended up in the XML
        // after God Mode overrides, same as we do for Fine Tune.
        if (SteadycamCheck.IsChecked == true
            && CameraMod.TryParseOnFootZoomDistances(xml, out double zl2, out double zl3, out double zl4))
        {
            var lockOnSync = new ModificationSet { ElementMods = CameraRules.BuildLockOnDistancesPublic(zl2, zl3, zl4), FovValue = 0 };
            xml = CameraMod.ApplyModifications(xml, lockOnSync);
        }

        return xml;
    }

    private ModificationSet BuildCurrentSimpleModSet()
    {
        string styleId = GetSelectedStyleId();
        int fov = GetSelectedFov();
        bool bane = BaneCheck.IsChecked == true;
        double pullback = GetCombatPullback();
        bool mount = MountHeightCheck.IsChecked == true;
        bool sc = SteadycamCheck.IsChecked == true;

        // Match v2: named styles use StyleUpOffset for mount height; custom sliders apply only when style_id is "custom".
        double? customUp = null;
        if (string.Equals(styleId, "custom", StringComparison.OrdinalIgnoreCase))
        {
            CameraRules.RegisterCustomStyle(DistSlider.Value, HeightSlider.Value, HShiftSlider.Value);
            customUp = HeightSlider.Value;
        }

        return CameraRules.BuildModifications(styleId, fov, bane, combatPullback: pullback,
            mountHeight: mount, customUp: customUp, steadycam: sc);
    }

    private ModificationSet BuildExpertModSet()
    {
        var elementMods = new Dictionary<string, Dictionary<string, (string Action, string Value)>>();

        foreach (var row in _advAllRows.Where(r => r.IsModified))
        {
            if (!elementMods.TryGetValue(row.ModKey, out var attrs))
            {
                attrs = new Dictionary<string, (string, string)>();
                elementMods[row.ModKey] = attrs;
            }

            attrs[row.Attribute] = ("SET", row.Value);
        }

        return new ModificationSet { ElementMods = elementMods, FovValue = 0 };
    }

    // â"€â"€ Preview sync â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private void SyncPreview()
    {
        if (_suppressEvents) return;
        int fov = GetSelectedFov();
        bool centered = BaneCheck.IsChecked == true;

        double d = DistSlider.Value, h = HeightSlider.Value, ro = HShiftSlider.Value;
        string previewLabel = _selectedPresetManagerItem?.Name ?? "Custom";
        Preview.UpdateParams(d, h, previewLabel);
        FovPreviewCtrl.UpdateParams(fov, ro, centered, d);
    }

    private void ScheduleCoalescedPreviewSync()
    {
        if (_previewSyncPosted)
            return;
        _previewSyncPosted = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _previewSyncPosted = false;
            SyncPreview();
        }), DispatcherPriority.Render);
    }

    /// <summary>Batches camera preview updates while dragging distance / height / shift sliders.</summary>
    private void ScheduleDebouncedPreviewSync()
    {
        if (_previewDebounceTimer == null)
        {
            ScheduleCoalescedPreviewSync();
            return;
        }

        _previewDebounceTimer.Stop();
        _previewDebounceTimer.Start();
    }

    // â"€â"€ Event handlers â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private DispatcherTimer? _syncEditorsDebounceTimer;

    /// <summary>
    /// Schedules a debounced rebuild of session XML from Quick settings.
    /// Pushes to Fine Tune / God Mode after 300ms of inactivity.
    /// </summary>
    private void ScheduleSyncQuickSettingsToEditors()
    {
        if (string.IsNullOrEmpty(_gameDir)) return;
        _syncEditorsDebounceTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _syncEditorsDebounceTimer.Stop();
        _syncEditorsDebounceTimer.Tick -= OnSyncEditorsDebounce;
        _syncEditorsDebounceTimer.Tick += OnSyncEditorsDebounce;
        _syncEditorsDebounceTimer.Start();
    }

    private void OnSyncEditorsDebounce(object? sender, EventArgs e)
    {
        _syncEditorsDebounceTimer?.Stop();
        SyncQuickSettingsToEditorsNow();
    }

    /// <summary>
    /// Immediately rebuilds session XML from Quick settings and pushes to editors.
    /// Called by debounce timer and by tab switching.
    /// </summary>
    private void SyncQuickSettingsToEditorsNow()
    {
        if (string.IsNullOrEmpty(_gameDir)) return;
        if (_sessionIsRawImport) return; // Raw imports must not be rebuilt through CameraRules
        // Don't overwrite _sessionXml when a full preset (with Fine Tune/God Mode changes) is loaded
        if (_sessionIsFullPreset) return;
        try
        {
            string xml = BuildSimpleSessionXml();
            _sessionXml = xml;
            if (_advCtrlSliders.Count > 0)
            {
                ApplySessionXmlToAdvancedControls(xml);
                ApplySteadycamSliderLock();
            }
            _advCtrlNeedsRefresh = true;
            _expertNeedsRefresh = true;
        }
        catch { }
    }

    private void OnSettingChanged(object s, RoutedEventArgs e)
    {
        if (!IsLoaded || _suppressEvents) return;
        if (IsQuickEditLocked()) { ShowLockedToastIfNeeded(); return; }
        _sessionIsFullPreset = false;
        ApplyCenteredLock();
        ScheduleCoalescedPreviewSync();
        ScheduleSyncQuickSettingsToEditors();
        SaveCurrentUiState();
        QueueSavedToast();
    }

    private void OnSettingChanged(object s, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _suppressEvents) return;
        if (IsQuickEditLocked()) { ShowLockedToastIfNeeded(); return; }
        _sessionIsFullPreset = false;
        ApplyCenteredLock();
        ScheduleCoalescedPreviewSync();
        ScheduleSyncQuickSettingsToEditors();
        SaveCurrentUiState();
        QueueSavedToast();
    }

    private void OnSliderChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_suppressEvents || !IsLoaded) return;
        if (IsOffsetsEditLocked()) { ShowLockedToastIfNeeded(); return; }
        CaptureUndoSnapshot();
        _sessionIsFullPreset = false;
        DistLabel.Text = $"{DistSlider.Value:F1}";
        HeightLabel.Text = $"{HeightSlider.Value:F1}";
        HShiftLabel.Text = $"{HShiftSlider.Value:F1}";
        _selectedStyleId = "custom";
        CaptureCustomDraft(markDirty: true, updateSelector: true);
        SyncPreview();
        ScheduleSyncQuickSettingsToEditors();
        SaveCurrentUiState();
        QueueSavedToast();
    }

    private void OnBaneChanged(object s, RoutedEventArgs e)
    {
        if (!IsLoaded || _suppressEvents) return;
        if (IsQuickEditLocked()) { ShowLockedToastIfNeeded(); return; }
        _sessionIsFullPreset = false;
        ApplyCenteredLock();
        CaptureCustomDraft(markDirty: true, updateSelector: true);
        ScheduleCoalescedPreviewSync();
        ScheduleSyncQuickSettingsToEditors();
        SaveCurrentUiState();
        QueueSavedToast();
    }

    private void ApplyCenteredLock()
    {
        if (BaneCheck.IsChecked == true)
        {
            HShiftSlider.Value = 0;
            HShiftSlider.IsEnabled = false;
            HShiftLabel.Text = "0.0";
            HShiftLabel.Foreground = _textDimBrush;
            HShiftTip.Text = "\u26A0 Centered Camera forces character to screen center \u2014 untick to adjust";
        }
        else
        {
            // Only re-enable if the preset isn't locked
            bool offsetsLocked = _selectedPresetManagerItem?.IsUcmPreset == true
                || IsQuickEditLocked()
                || _sessionIsRawImport;
            HShiftSlider.IsEnabled = !offsetsLocked;
            HShiftLabel.Foreground = (Brush)FindResource("TextPrimaryBrush");
            HShiftTip.Text = HShiftTipUnlocked;
        }
    }

    // â"€â"€ Selections â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private string GetSelectedStyleId() => _selectedStyleId;

    private int GetSelectedFov()
    {
        int idx = FovCombo.SelectedIndex;
        return idx >= 0 && idx < FovOptions.Length ? FovOptions[idx].Value : 25;
    }

    private double GetCombatPullback() => CombatPullbackSlider.Value;

    private static string FormatPullback(double v)
    {
        int pct = (int)Math.Round(v * 100);
        return pct == 0 ? "  0%" : $"{pct:+0;-0}%";
    }

    private void OnCombatPullbackChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    {
        if (CombatPullbackLabel != null)
            CombatPullbackLabel.Text = FormatPullback(CombatPullbackSlider.Value);
        OnSettingChanged(s, e);
    }

    // â"€â"€ Presets â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    // Session presets: UcmPresetsDir (built-in default + styles), MyPresetsDir (user-created).
    // Imported creator/XML/PAZ/mod: ImportedPresetsDir (import_presets, ImportedPreset JSON).

    private void RefreshPresetCombo()
    {
        // PresetCombo removed from XAML — preset selection is sidebar-only now.
    }

    private void OnPresetSelected(object s, SelectionChangedEventArgs e)
    {
        // PresetCombo removed — preset loading is handled by sidebar selection.
    }

    // OnSavePreset, OnDeletePreset, OnExportString, OnImportString removed —
    // UCM Quick preset management buttons are no longer in the XAML.

    private void CaptureCustomDraft(bool markDirty, bool updateSelector)
    {
        _customDraftDistance = DistSlider.Value;
        _customDraftHeight = HeightSlider.Value;
        _customDraftRightOffset = HShiftSlider.Value;

        // PresetCombo removed — preserve existing _customDraftPresetName unless marking dirty.
        if (markDirty)
        {
            _customDraftDirty = true;
            _customDraftPresetName = null;
        }
        else
        {
            _customDraftDirty = string.IsNullOrWhiteSpace(_customDraftPresetName);
        }
    }

    private void RestoreCustomDraft()
    {
        _suppressEvents = true;
        DistSlider.Value = _customDraftDistance;
        HeightSlider.Value = _customDraftHeight;
        HShiftSlider.Value = _customDraftRightOffset;
        DistLabel.Text = $"{DistSlider.Value:F1}";
        HeightLabel.Text = $"{HeightSlider.Value:F1}";
        HShiftLabel.Text = $"{HShiftSlider.Value:F1}";
        _suppressEvents = false;
    }

}
