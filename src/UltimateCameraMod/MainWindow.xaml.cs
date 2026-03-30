using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using UltimateCameraMod.Controls;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod;

public partial class MainWindow : Window
{
    private const string Ver = "2.0";
    private const string NexusUrl = "https://www.nexusmods.com/crimsondesert/mods/438";
    private const string GitHubUrl = "https://github.com/FitzDegenhub/UltimateCameraMod";

    private static readonly string ExeDir =
        Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

    private string _gameDir = "";
    private string _detectedPlatform = "Unknown";
    private string _activeTab = "presets";
    private bool _suppressEvents;
    private Dictionary<string, object>? _savedState;

    // ── Advanced mode state ───────────────────────────────────────────
    private bool _isAdvancedMode;
    private List<AdvancedRow> _advAllRows = new();
    private ObservableCollection<AdvancedRow> _advFilteredRows = new();
    private static string AdvOverridesPath => Path.Combine(ExeDir, "advanced_overrides.json");
    private static string AdvPresetsDir
    {
        get { string d = Path.Combine(ExeDir, "advanced_presets"); Directory.CreateDirectory(d); return d; }
    }

    // ── Style/FoV/Combat data ────────────────────────────────────────

    private static readonly (string Id, string Label)[] Styles =
    {
        ("western",   "Heroic  -  Shoulder-level OTS, great framing"),
        ("cinematic", "Panoramic  -  Head-height wide pullback, filmic"),
        ("default",   "Smoothed  -  Vanilla framing + smoothing"),
        ("immersive", "Close-Up  -  Shoulder OTS, tighter (16:9 feel)"),
        ("lowcam",    "Low Rider  -  Hip-level, full body + horizon"),
        ("vlowcam",   "Knee Cam  -  Knee-height dramatic low angle"),
        ("ulowcam",   "Dirt Cam  -  Ground-level, extreme low"),
        ("re2",       "Survival  -  Tight horror-game OTS (16:9 feel)"),
    };

    private static readonly (int Value, string Label)[] FovOptions =
    {
        (0,  "No change (40\u00b0)  -  Vanilla"),
        (10, "+10\u00b0 (50\u00b0)  -  Minimal, good for 16:9"),
        (15, "+15\u00b0 (55\u00b0)  -  Subtle improvement"),
        (20, "+20\u00b0 (60\u00b0)  -  Sweet spot for 21:9"),
        (25, "+25\u00b0 (65\u00b0)  -  Great for 21:9 + 32:9"),
        (30, "+30\u00b0 (70\u00b0)  -  Perfect for 32:9"),
        (40, "+40\u00b0 (80\u00b0)  -  Extreme, slight fisheye"),
    };

    private static readonly (string Id, string Label)[] CombatOptions =
    {
        ("default", "Default  -  Standard combat camera"),
        ("wide",    "Wider  -  More room to see the battlefield"),
        ("max",     "Maximum  -  Widest possible combat view"),
    };

    private static readonly Dictionary<string, (double Dist, double Up, double Ro)> StyleParams = new()
    {
        ["western"] = (5.0, -0.2, 0.0),
        ["cinematic"] = (7.5, 0.0, 0.0),
        ["default"] = (3.4, 0.0, 0.0),
        ["immersive"] = (4.0, -0.2, 0.0),
        ["lowcam"] = (5.0, -0.8, 0.0),
        ["vlowcam"] = (5.0, -1.2, 0.0),
        ["ulowcam"] = (5.0, -1.5, 0.0),
        ["re2"] = (3.0, 0.0, 0.7),
    };

    // ── Constructor ──────────────────────────────────────────────────

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ApplyDarkTitlebar();

            _savedState = LoadInstallState();
            PopulateControls();
            string savedStyle = _savedState?.GetValueOrDefault("style")?.ToString() ?? "";
            SwitchTab(savedStyle == "custom" ? "custom" : "presets");

            var (detectedPath, platform) = GameDetector.FindGameDir();
            _gameDir = detectedPath ?? "";
            _detectedPlatform = platform;

            if (string.IsNullOrEmpty(_gameDir))
                _gameDir = BrowseForGameDir();

            if (string.IsNullOrEmpty(_gameDir))
            {
                GamePathLabel.Text = "Game folder:  NOT SET  (click Install to browse)";
                SetStatus("Game folder not set. Click Install and you'll be prompted to browse.", "Warn");
            }
            else
            {
                OnGameDirResolved();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup error:\n{ex}", "Ultimate Camera Mod", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string BrowseForGameDir()
    {
        try
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Crimson Desert folder (contains the '0010' folder)",
                UseDescriptionForTitle = true
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folder = dlg.SelectedPath;
                if (File.Exists(Path.Combine(folder, "0010", "0.paz")))
                    return folder;

                MessageBox.Show("That folder doesn't contain 0010\\0.paz.\n" +
                    "Make sure you selected the correct Crimson Desert directory.",
                    "Wrong Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Folder dialog error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        return "";
    }

    private void OnGameDirResolved()
    {
        string backupsDir = Path.Combine(ExeDir, "backups");
        CameraMod.BackupsDirOverride = () => backupsDir;
        HudMod.BackupsDirOverride = () => Path.Combine(backupsDir, "hud");

        _savedState = LoadInstallState();

        string pt = _gameDir;
        if (pt.Length > 55) pt = "..." + pt[^52..];

        string platformTag = _detectedPlatform != "Unknown" ? $" [{_detectedPlatform}]" : "";
        GamePathLabel.Text = $"Game folder:  {pt}{platformTag}";

        if (_detectedPlatform == "Xbox/GamePass" && !GameDetector.CheckWritePermission(_gameDir))
        {
            SetStatus("Xbox/Game Pass: game folder is read-only. Move the game or fix folder permissions.", "Warn");
            MessageBox.Show(
                "Xbox / Game Pass game folder appears to be read-only.\n\n" +
                "To fix this, try one of:\n" +
                "  1. Xbox App \u2192 Crimson Desert \u2192 Manage \u2192 Move to a different drive\n" +
                "  2. Right-click the game folder \u2192 Properties \u2192 uncheck \"Read-only\" \u2192 Apply to all subfolders",
                "Write Permission Required", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        CheckForUpdate();
        SyncPreview();
    }

    // ── Dark titlebar ────────────────────────────────────────────────

    [DllImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute", PreserveSig = true)]
    private static extern int SetWindowThemeAttribute(IntPtr hwnd, int attr, ref int value, int size);

    private void ApplyDarkTitlebar()
    {
        try
        {
            var hwnd = new WindowInteropHelper(this).EnsureHandle();
            int value = 1;
            SetWindowThemeAttribute(hwnd, 20, ref value, sizeof(int));
        }
        catch { }
    }

    // ── Populate controls ────────────────────────────────────────────

    private void PopulateControls()
    {
        _suppressEvents = true;

        StyleCombo.Items.Clear();
        foreach (var (_, label) in Styles)
            StyleCombo.Items.Add(label);

        string savedStyle = _savedState?.GetValueOrDefault("style")?.ToString() ?? "cinematic";
        int styleIdx = Array.FindIndex(Styles, s => s.Id == savedStyle);
        StyleCombo.SelectedIndex = styleIdx >= 0 ? styleIdx : 1;

        FovCombo.Items.Clear();
        foreach (var (_, label) in FovOptions)
            FovCombo.Items.Add(label);

        int savedFov = GetInt(_savedState, "fov", 25);
        int fovIdx = Array.FindIndex(FovOptions, f => f.Value == savedFov);
        FovCombo.SelectedIndex = fovIdx >= 0 ? fovIdx : 4;

        CombatCombo.Items.Clear();
        foreach (var (_, label) in CombatOptions)
            CombatCombo.Items.Add(label);

        string savedCombat = _savedState?.GetValueOrDefault("combat")?.ToString() ?? "default";
        int combatIdx = Array.FindIndex(CombatOptions, c => c.Id == savedCombat);
        CombatCombo.SelectedIndex = combatIdx >= 0 ? combatIdx : 0;

        BaneCheck.IsChecked = GetBool(_savedState, "bane");
        MountHeightCheck.IsChecked = GetBool(_savedState, "mount_height");

        int savedHudWidth = GetInt(_savedState, "hud_width", 0);
        HudCheck.IsChecked = savedHudWidth > 0;
        HudWidthSlider.Value = savedHudWidth > 0 ? savedHudWidth : 2520;
        ApplyHudLock();

        RefreshPresetCombo();

        if (_savedState?.ContainsKey("custom") == true)
        {
            try
            {
                var custom = JsonSerializer.Deserialize<JsonElement>(_savedState["custom"].ToString()!);
                DistSlider.Value = custom.TryGetProperty("distance", out var d) ? d.GetDouble() : 5.0;
                HeightSlider.Value = custom.TryGetProperty("height", out var h) ? h.GetDouble() : 0.0;
                HShiftSlider.Value = custom.TryGetProperty("right_offset", out var r) ? r.GetDouble() : 0.0;
            }
            catch { }

            string savedPreset = _savedState?.GetValueOrDefault("custom_preset")?.ToString() ?? "";
            if (!string.IsNullOrEmpty(savedPreset))
            {
                for (int i = 0; i < PresetCombo.Items.Count; i++)
                {
                    if (PresetCombo.Items[i]?.ToString() == savedPreset)
                    {
                        PresetCombo.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        _suppressEvents = false;
    }

    // ── State persistence ────────────────────────────────────────────

    private static string StatePath => Path.Combine(ExeDir, "last_install.json");

    private static Dictionary<string, object>? LoadInstallState()
    {
        try
        {
            if (!File.Exists(StatePath)) return null;
            string json = File.ReadAllText(StatePath);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }
        catch { return null; }
    }

    private void SaveInstallState(int compSize, string style, int fov, bool bane, string combat,
        Dictionary<string, double>? customParams, bool mountHeight, int hudWidth)
    {
        var state = new Dictionary<string, object>
        {
            ["comp_size"] = compSize, ["style"] = style, ["fov"] = fov,
            ["bane"] = bane, ["combat"] = combat, ["mount_height"] = mountHeight,
            ["hud_width"] = hudWidth,
        };
        if (customParams != null) state["custom"] = customParams;
        if (style == "custom" && PresetCombo.SelectedIndex > 0)
            state["custom_preset"] = PresetCombo.SelectedItem?.ToString() ?? "";
        try { File.WriteAllText(StatePath, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true })); }
        catch { }
    }

    // ── Update detection ─────────────────────────────────────────────

    private void CheckForUpdate()
    {
        try
        {
            var status = CameraMod.DetectLiveStatus(_gameDir);
            bool hudModded = HudMod.DetectHudModified(_gameDir);

            if (!status.IsModified && !hudModded)
            {
                BannerPanel.Visibility = Visibility.Collapsed;
                return;
            }

            BannerPanel.Visibility = Visibility.Visible;

            var parts = new List<string>();

            if (status.FovDelta != 0)
                parts.Add($"FoV {(status.FovDelta > 0 ? "+" : "")}{status.FovDelta}\u00b0");
            if (status.StyleModified)
                parts.Add("Camera style");
            if (status.CenteredCamera)
                parts.Add("Centered");
            if (status.CombatModified)
                parts.Add("Combat camera");
            if (status.MountModified)
                parts.Add("Mount camera");
            if (hudModded)
                parts.Add("HUD centered");
            if (status.IsModified && parts.Count == 0)
                parts.Add("Camera modified");

            string details = parts.Count > 0 ? string.Join("  |  ", parts) : "modified";

            BannerPanel.Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x1F, 0x00));
            BannerText.Text = $"\u26A0  Game files modified:  {details}";
            BannerText.Foreground = FindResource("WarnBrush") as Brush;
        }
        catch { BannerPanel.Visibility = Visibility.Collapsed; }
    }

    // ── Tab switching ────────────────────────────────────────────────

    private void SwitchTab(string tab)
    {
        _activeTab = tab;
        if (tab == "presets")
        {
            PresetsBorder.Visibility = Visibility.Visible;
            CustomPanel.Visibility = Visibility.Collapsed;
            TabPresets.Style = (Style)FindResource("AccentButton");
            TabCustom.Style = (Style)FindResource("SubtleButton");
            TabCustom.BorderThickness = new Thickness(0);
        }
        else
        {
            PresetsBorder.Visibility = Visibility.Collapsed;
            CustomPanel.Visibility = Visibility.Visible;
            TabCustom.Style = (Style)FindResource("AccentButton");
            TabPresets.Style = (Style)FindResource("SubtleButton");
            TabPresets.BorderThickness = new Thickness(0);
        }
        SyncPreview();
    }

    private void OnTabPresets(object s, RoutedEventArgs e) => SwitchTab("presets");
    private void OnTabCustom(object s, RoutedEventArgs e) => SwitchTab("custom");

    // ── Preview sync ─────────────────────────────────────────────────

    private void SyncPreview()
    {
        if (_suppressEvents) return;
        int fov = GetSelectedFov();
        bool centered = BaneCheck.IsChecked == true;

        if (_activeTab == "custom")
        {
            double d = DistSlider.Value, h = HeightSlider.Value, ro = HShiftSlider.Value;
            Preview.UpdateParams(d, h, "Custom");
            FovPreviewCtrl.UpdateParams(fov, ro, centered);
        }
        else
        {
            string sid = GetSelectedStyleId();
            var (d, u, ro) = StyleParams.GetValueOrDefault(sid, (3.4, 0.0, 0.0));
            string name = sid;
            foreach (var (id, lbl) in Styles)
                if (id == sid) { name = lbl.Split("  -  ")[0]; break; }
            Preview.UpdateParams(d, u, name);
            FovPreviewCtrl.UpdateParams(fov, ro, centered);
        }
    }

    // ── Event handlers ───────────────────────────────────────────────

    private void OnSettingChanged(object s, RoutedEventArgs e) { if (!IsLoaded) return; ApplyCenteredLock(); SyncPreview(); }
    private void OnSettingChanged(object s, SelectionChangedEventArgs e) { if (!IsLoaded) return; ApplyCenteredLock(); SyncPreview(); }

    private void OnSliderChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_suppressEvents || !IsLoaded) return;
        DistLabel.Text = $"{DistSlider.Value:F1}";
        HeightLabel.Text = $"{HeightSlider.Value:F1}";
        HShiftLabel.Text = $"{HShiftSlider.Value:F1}";
        SyncPreview();
    }

    private void OnBaneChanged(object s, RoutedEventArgs e) { if (!IsLoaded) return; ApplyCenteredLock(); SyncPreview(); }

    private void OnHudToggle(object s, RoutedEventArgs e) { if (!IsLoaded) return; ApplyHudLock(); SyncPreview(); }

    private void OnHudSliderChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_suppressEvents || !IsLoaded) return;
        int w = (int)Math.Round(HudWidthSlider.Value);
        HudWidthLabel.Text = $"{w}px";
    }

    private void ApplyCenteredLock()
    {
        if (BaneCheck.IsChecked == true)
        {
            HShiftSlider.Value = 0;
            HShiftSlider.IsEnabled = false;
            HShiftLabel.Text = "0.0";
            HShiftLabel.Foreground = (Brush)FindResource("TextDimBrush");
            HShiftTip.Text = "\u26A0 Locked to 0 \u2014 untick Centered Camera to adjust";
        }
        else
        {
            HShiftSlider.IsEnabled = true;
            HShiftLabel.Foreground = (Brush)FindResource("TextPrimaryBrush");
            HShiftTip.Text = "How far left/right the camera sits from the character. Centered camera locks this to 0.";
        }
    }

    private void ApplyHudLock()
    {
        bool enabled = HudCheck.IsChecked == true;
        HudWidthSlider.IsEnabled = enabled;
        HudWidthLabel.Text = enabled ? $"{(int)HudWidthSlider.Value}px" : "Off";
        HudWidthLabel.Foreground = enabled
            ? (Brush)FindResource("TextPrimaryBrush")
            : (Brush)FindResource("TextDimBrush");
    }

    // ── Selections ───────────────────────────────────────────────────

    private string GetSelectedStyleId()
    {
        int idx = StyleCombo.SelectedIndex;
        return idx >= 0 && idx < Styles.Length ? Styles[idx].Id : "cinematic";
    }

    private int GetSelectedFov()
    {
        int idx = FovCombo.SelectedIndex;
        return idx >= 0 && idx < FovOptions.Length ? FovOptions[idx].Value : 25;
    }

    private string GetSelectedCombat()
    {
        int idx = CombatCombo.SelectedIndex;
        return idx >= 0 && idx < CombatOptions.Length ? CombatOptions[idx].Id : "default";
    }

    // ── Presets ──────────────────────────────────────────────────────

    private string PresetsDir
    {
        get
        {
            string d = Path.Combine(ExeDir, "custom_presets");
            Directory.CreateDirectory(d);
            return d;
        }
    }

    private List<string> ListPresets() =>
        Directory.GetFiles(PresetsDir, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n).ToList();

    private void RefreshPresetCombo()
    {
        var presets = ListPresets();
        PresetCombo.Items.Clear();
        PresetCombo.Items.Add("(new \u2014 adjust sliders)");
        foreach (var p in presets) PresetCombo.Items.Add(p);
        PresetCombo.SelectedIndex = 0;
    }

    private void OnPresetSelected(object s, SelectionChangedEventArgs e)
    {
        if (_suppressEvents || PresetCombo.SelectedIndex <= 0) return;
        string name = PresetCombo.SelectedItem?.ToString() ?? "";
        try
        {
            string json = File.ReadAllText(Path.Combine(PresetsDir, $"{name}.json"));
            var data = JsonSerializer.Deserialize<Dictionary<string, double>>(json);
            if (data == null) return;
            _suppressEvents = true;
            DistSlider.Value = Math.Clamp(data.GetValueOrDefault("distance", 5.0), 1.5, 12.0);
            HeightSlider.Value = Math.Clamp(data.GetValueOrDefault("height", 0.0), -1.6, 0.5);
            HShiftSlider.Value = Math.Clamp(data.GetValueOrDefault("right_offset", 0.0), -1.0, 1.0);
            DistLabel.Text = $"{DistSlider.Value:F1}";
            HeightLabel.Text = $"{HeightSlider.Value:F1}";
            HShiftLabel.Text = $"{HShiftSlider.Value:F1}";
            _suppressEvents = false;
            SyncPreview();
            SetStatus($"Preset '{name}' loaded.", "Success");
        }
        catch (Exception ex) { SetStatus($"Load failed: {ex.Message}", "Error"); }
    }

    private void OnSavePreset(object s, RoutedEventArgs e)
    {
        string name;

        if (PresetCombo.SelectedIndex > 0)
        {
            name = PresetCombo.SelectedItem?.ToString() ?? "";
            var confirm = MessageBox.Show(
                $"Overwrite preset '{name}' with current slider values?",
                "Overwrite Preset", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;
        }
        else
        {
            var dlg = new InputDialog("Save Custom Preset", "Enter a name for this preset:") { Owner = this };
            if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ResponseText)) return;
            name = dlg.ResponseText.Trim().Replace(" ", "_");
            if (name.Length > 30) name = name[..30];
        }

        try
        {
            var data = new { distance = Math.Round(DistSlider.Value, 2), height = Math.Round(HeightSlider.Value, 2), right_offset = Math.Round(HShiftSlider.Value, 2) };
            File.WriteAllText(Path.Combine(PresetsDir, $"{name}.json"),
                JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            RefreshPresetCombo();
            for (int i = 0; i < PresetCombo.Items.Count; i++)
                if (PresetCombo.Items[i]?.ToString() == name) { PresetCombo.SelectedIndex = i; break; }
            SetStatus($"Preset '{name}' saved.", "Success");
        }
        catch (Exception ex) { SetStatus($"Save failed: {ex.Message}", "Error"); }
    }

    private void OnDeletePreset(object s, RoutedEventArgs e)
    {
        if (PresetCombo.SelectedIndex <= 0) { SetStatus("Select a saved preset first.", "TextSecondary"); return; }
        string name = PresetCombo.SelectedItem?.ToString() ?? "";
        try
        {
            string path = Path.Combine(PresetsDir, $"{name}.json");
            if (File.Exists(path)) File.Delete(path);
            RefreshPresetCombo();
            SetStatus($"Preset '{name}' deleted.", "Success");
        }
        catch (Exception ex) { SetStatus($"Delete failed: {ex.Message}", "Error"); }
    }

    private void OnExportString(object s, RoutedEventArgs e)
    {
        var dlg = new InputDialog("Export Preset String", "Name this preset for sharing:") { Owner = this };
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ResponseText)) return;
        string name = dlg.ResponseText.Trim();
        if (name.Length > 30) name = name[..30];
        string code = PresetCodec.Encode(name, DistSlider.Value, HeightSlider.Value, HShiftSlider.Value);
        var exportDlg = new ExportDialog(name, code) { Owner = this };
        exportDlg.ShowDialog();
        SetStatus($"Preset '{name}' exported.", "Success");
    }

    private void OnImportString(object s, RoutedEventArgs e)
    {
        var dlg = new ImportDialog { Owner = this };
        if (dlg.ShowDialog() != true || dlg.Result == null) return;
        var (name, dist, height, roff) = dlg.Result.Value;
        _suppressEvents = true;
        DistSlider.Value = dist; HeightSlider.Value = height; HShiftSlider.Value = roff;
        DistLabel.Text = $"{dist:F1}"; HeightLabel.Text = $"{height:F1}"; HShiftLabel.Text = $"{roff:F1}";
        _suppressEvents = false;
        string safeName = name.Replace(" ", "_");
        if (safeName.Length > 30) safeName = safeName[..30];
        try
        {
            var data = new { distance = Math.Round(dist, 2), height = Math.Round(height, 2), right_offset = Math.Round(roff, 2) };
            File.WriteAllText(Path.Combine(PresetsDir, $"{safeName}.json"),
                JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
        RefreshPresetCombo();
        SyncPreview();
        SetStatus($"Imported '{name}' and saved \u2014 hit Install to apply.", "Success");
    }

    // ── Install ──────────────────────────────────────────────────────

    private void OnInstall(object s, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir))
        {
            _gameDir = BrowseForGameDir();
            if (string.IsNullOrEmpty(_gameDir))
            {
                SetStatus("No game folder selected.", "Error");
                return;
            }
            OnGameDirResolved();
        }

        SetButtons(false);
        SetStatus("Installing...", "Accent");

        string styleId = _activeTab == "custom" ? "custom" : GetSelectedStyleId();
        int fov = GetSelectedFov();
        string combat = GetSelectedCombat();
        bool bane = BaneCheck.IsChecked == true;
        bool mountHeight = MountHeightCheck.IsChecked == true;
        int hudWidth = HudCheck.IsChecked == true ? (int)HudWidthSlider.Value : 0;

        Dictionary<string, double>? customParams = null;
        double? customUp = null;
        if (styleId == "custom")
        {
            customParams = new()
            {
                ["distance"] = Math.Round(DistSlider.Value, 2),
                ["height"] = Math.Round(HeightSlider.Value, 2),
                ["right_offset"] = Math.Round(HShiftSlider.Value, 2),
            };
            customUp = customParams["height"];
            CameraRules.RegisterCustomStyle(customParams["distance"], customParams["height"], customParams["right_offset"]);
        }

        Task.Run(() =>
        {
            try
            {
                var result = CameraMod.InstallCameraMod(_gameDir, styleId, fov, bane, combat,
                    mountHeight: mountHeight, customUp: customUp,
                    log: msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent")));
                bool ok = result.GetValueOrDefault("status")?.ToString() == "ok";
                int compSize = ok && result.ContainsKey("comp_size") ? (int)result["comp_size"] : 0;

                if (ok && hudWidth > 0)
                {
                    var hudResult = HudMod.InstallCenteredHud(_gameDir, hudWidth,
                        log: msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent")));
                    if (hudResult.GetValueOrDefault("status")?.ToString() != "ok") ok = false;
                }
                else if (ok && hudWidth == 0)
                {
                    HudMod.RestoreHud(_gameDir);
                }

                Dispatcher.Invoke(() =>
                {
                    if (ok)
                    {
                        SetStatus("Installed! Launch the game.", "Success");
                        SaveInstallState(compSize, styleId, fov, bane, combat, customParams, mountHeight, hudWidth);
                    }
                    else
                    {
                        SetStatus("Install failed. Is the game running?", "Error");
                    }
                    CheckForUpdate();
                    SetButtons(true);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Error: {ex.Message}", "Error");
                    SetButtons(true);
                });
            }
        });
    }

    // ── Restore ──────────────────────────────────────────────────────

    private void OnRestore(object s, RoutedEventArgs e)
    {
        SetButtons(false);
        SetStatus("Restoring vanilla...", "Accent");

        Task.Run(() =>
        {
            try
            {
                var result = CameraMod.RestoreCamera(_gameDir,
                    log: msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent")));
                string status = result.GetValueOrDefault("status")?.ToString() ?? "error";
                HudMod.RestoreHud(_gameDir);

                Dispatcher.Invoke(() =>
                {
                    switch (status)
                    {
                        case "ok":
                            SetStatus("Vanilla restored (camera + HUD).", "Success"); break;
                        case "no_backup":
                            SetStatus("No backup found. Camera may already be vanilla.", "TextSecondary"); break;
                        case "stale_backup":
                            SetStatus("Backup is from old version. Verify game files.", "Error"); break;
                        default:
                            SetStatus("Restore failed. Verify game files.", "Error"); break;
                    }
                    CheckForUpdate();
                    SetButtons(true);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Error: {ex.Message}", "Error");
                    SetButtons(true);
                });
            }
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private void SetStatus(string text, string brushKey)
    {
        StatusLabel.Text = text;
        StatusLabel.Foreground = brushKey switch
        {
            "Success" => (Brush)FindResource("SuccessBrush"),
            "Error" => (Brush)FindResource("ErrorBrush"),
            "Accent" => (Brush)FindResource("AccentBrush"),
            "Warn" => (Brush)FindResource("WarnBrush"),
            "TextSecondary" => (Brush)FindResource("TextSecondaryBrush"),
            _ => (Brush)FindResource("TextDimBrush"),
        };
    }

    private void SetButtons(bool enabled)
    {
        InstallBtn.IsEnabled = enabled;
        RestoreBtn.IsEnabled = enabled;
    }

    // ── Advanced mode toggle ─────────────────────────────────────────

    private void OnToggleAdvanced(object s, RoutedEventArgs e)
    {
        if (!_isAdvancedMode)
        {
            if (string.IsNullOrEmpty(_gameDir))
            {
                MessageBox.Show("Set a game folder first (click Install to browse).",
                    "Advanced Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            EnterAdvancedMode();
        }
        else
        {
            ExitAdvancedMode();
        }
    }

    private void EnterAdvancedMode()
    {
        try
        {
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
            _advAllRows = CameraMod.ParseXmlToRows(vanillaXml)
                .Where(r => r.Section.StartsWith("Player_", StringComparison.Ordinal))
                .ToList();

            string liveXml = CameraMod.ReadLiveXml(_gameDir);
            var liveRows = CameraMod.ParseXmlToRows(liveXml);
            var liveLookup = new Dictionary<string, string>();
            foreach (var lr in liveRows)
                liveLookup[lr.FullKey] = lr.Value;
            foreach (var row in _advAllRows)
                if (liveLookup.TryGetValue(row.FullKey, out string? liveVal))
                    row.Value = liveVal;

            AdvLoadOverrides();
            AdvBindGrid();
            AdvPopulateFilter();
            AdvRefreshPresetCombo();
            AdvUpdateRowCount();

            var lightText = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
            AdvSearchBox.Foreground = lightText;
            AdvSearchBox.CaretBrush = lightText;

            _isAdvancedMode = true;
            SimpleView.Visibility = Visibility.Collapsed;
            AdvancedView.Visibility = Visibility.Visible;
            SimpleButtons.Visibility = Visibility.Collapsed;
            AdvancedButtons.Visibility = Visibility.Visible;
            SetStatus("Advanced mode — edit values and click Apply to Game.", "TextDim");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load camera XML:\n{ex.Message}", "Advanced Editor",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExitAdvancedMode()
    {
        _isAdvancedMode = false;
        SimpleView.Visibility = Visibility.Visible;
        AdvancedView.Visibility = Visibility.Collapsed;
        SimpleButtons.Visibility = Visibility.Visible;
        AdvancedButtons.Visibility = Visibility.Collapsed;
        CheckForUpdate();
        SetStatus("Ready", "TextDim");
    }

    private void AdvBindGrid()
    {
        _advFilteredRows = new ObservableCollection<AdvancedRow>(_advAllRows);
        var view = CollectionViewSource.GetDefaultView(_advFilteredRows);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Section"));
        AdvDataGrid.ItemsSource = view;
    }

    private void AdvPopulateFilter()
    {
        AdvFilterCombo.Items.Clear();
        AdvFilterCombo.Items.Add("All");
        AdvFilterCombo.Items.Add("Modified only");

        var prefixes = _advAllRows
            .Select(r =>
            {
                var parts = r.Section.Split('_');
                return parts.Length > 1 ? parts[1] : r.Section;
            })
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .OrderBy(p => p);
        foreach (var p in prefixes)
            AdvFilterCombo.Items.Add(p);

        AdvFilterCombo.SelectedIndex = 0;
    }

    private void AdvUpdateRowCount()
    {
        int modified = _advAllRows.Count(r => r.IsModified);
        AdvRowCountLabel.Text = $"{_advFilteredRows.Count} rows  |  {modified} modified";
    }

    private void OnAdvSearchChanged(object sender, System.Windows.Input.KeyEventArgs e) => AdvApplyFilter();

    private void OnAdvFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || !_isAdvancedMode) return;
        AdvApplyFilter();
    }

    private void AdvApplyFilter()
    {
        string search = AdvSearchBox.Text?.Trim().ToLowerInvariant() ?? "";
        string filter = AdvFilterCombo.SelectedItem?.ToString() ?? "All";

        var filtered = _advAllRows.AsEnumerable();

        if (filter == "Modified only")
            filtered = filtered.Where(r => r.IsModified);
        else if (filter != "All")
            filtered = filtered.Where(r => r.Section.Contains(filter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(r =>
                r.Section.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.SubElement.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Attribute.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Value.Contains(search, StringComparison.OrdinalIgnoreCase));

        _advFilteredRows.Clear();
        foreach (var r in filtered) _advFilteredRows.Add(r);

        var view = CollectionViewSource.GetDefaultView(_advFilteredRows);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Section"));
        AdvDataGrid.ItemsSource = view;
        AdvUpdateRowCount();
    }

    private void OnAdvResetDefaults(object sender, RoutedEventArgs e)
    {
        try
        {
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);

            string styleId = _activeTab == "custom" ? "custom" : GetSelectedStyleId();
            int fov = GetSelectedFov();
            bool bane = BaneCheck.IsChecked == true;
            string combat = GetSelectedCombat();
            bool mount = MountHeightCheck.IsChecked == true;
            double? customUp = null;
            if (styleId == "custom")
            {
                CameraRules.RegisterCustomStyle(DistSlider.Value, HeightSlider.Value, HShiftSlider.Value);
                customUp = HeightSlider.Value;
            }
            var modSet = CameraRules.BuildModifications(styleId, fov, bane, combat, mountHeight: mount, customUp: customUp);
            vanillaXml = CameraMod.ApplyModifications(vanillaXml, modSet);

            var defaultRows = CameraMod.ParseXmlToRows(vanillaXml);
            var lookup = defaultRows.ToDictionary(r => r.FullKey, r => r.Value);

            foreach (var row in _advAllRows)
                row.Value = lookup.TryGetValue(row.FullKey, out string? val) ? val : row.VanillaValue;

            AdvApplyFilter();
            SetStatus("Reset to defaults (your preset + vanilla).", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Reset failed: {ex.Message}", "Error");
        }
    }

    private void OnAdvExport(object sender, RoutedEventArgs e)
    {
        var modified = _advAllRows.Where(r => r.IsModified).ToList();
        if (modified.Count == 0) { SetStatus("No modified values to export.", "TextSecondary"); return; }

        var payload = modified.ToDictionary(r => r.FullKey, r => r.Value);
        string json = JsonSerializer.Serialize(payload);
        string encoded = "UCM_ADV:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var dlg = new ExportDialog("Advanced Overrides", encoded) { Owner = this };
        dlg.ShowDialog();
        SetStatus($"Exported {modified.Count} modified values.", "Success");
    }

    private void OnAdvImport(object sender, RoutedEventArgs e)
    {
        var dlg = new AdvancedImportDialog { Owner = this };
        if (dlg.ShowDialog() != true || dlg.Result == null) return;

        int applied = 0;
        var lookup = _advAllRows.ToDictionary(r => r.FullKey, r => r);
        foreach (var (key, val) in dlg.Result)
        {
            if (lookup.TryGetValue(key, out var row)) { row.Value = val; applied++; }
        }

        AdvApplyFilter();
        SetStatus($"Imported {applied} values.", "Success");
    }

    private void OnAdvApply(object sender, RoutedEventArgs e)
    {
        var modifiedRows = _advAllRows.Where(r => r.IsModified).ToList();
        if (modifiedRows.Count == 0) { SetStatus("No changes to apply.", "TextSecondary"); return; }

        AdvApplyBtn.IsEnabled = false;
        SetStatus("Applying...", "Accent");

        var elementMods = new Dictionary<string, Dictionary<string, (string Action, string Value)>>();
        foreach (var row in modifiedRows)
        {
            if (!elementMods.TryGetValue(row.ModKey, out var attrs))
            {
                attrs = new Dictionary<string, (string, string)>();
                elementMods[row.ModKey] = attrs;
            }
            attrs[row.Attribute] = ("SET", row.Value);
        }
        var modSet = new ModificationSet { ElementMods = elementMods, FovValue = 0 };

        Task.Run(() =>
        {
            try
            {
                CameraMod.InstallWithModSet(_gameDir, modSet,
                    msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent")));
                Dispatcher.Invoke(() =>
                {
                    AdvSaveOverrides();
                    SetStatus($"Applied {modifiedRows.Count} changes to game files.", "Success");
                    AdvApplyBtn.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Apply failed: {ex.Message}", "Error");
                    AdvApplyBtn.IsEnabled = true;
                });
            }
        });
    }

    private void AdvSaveOverrides()
    {
        try
        {
            var modified = _advAllRows.Where(r => r.IsModified)
                .ToDictionary(r => r.FullKey, r => r.Value);
            if (modified.Count == 0) { if (File.Exists(AdvOverridesPath)) File.Delete(AdvOverridesPath); return; }
            File.WriteAllText(AdvOverridesPath,
                JsonSerializer.Serialize(modified, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    private void AdvLoadOverrides()
    {
        try
        {
            if (!File.Exists(AdvOverridesPath)) return;
            string json = File.ReadAllText(AdvOverridesPath);
            var overrides = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (overrides == null) return;
            var lookup = _advAllRows.ToDictionary(r => r.FullKey, r => r);
            foreach (var (key, val) in overrides)
                if (lookup.TryGetValue(key, out var row)) row.Value = val;
        }
        catch { }
    }

    // ── Advanced presets ──────────────────────────────────────────────

    private void AdvRefreshPresetCombo()
    {
        var presets = Directory.GetFiles(AdvPresetsDir, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n).ToList();

        AdvPresetCombo.Items.Clear();
        AdvPresetCombo.Items.Add("(current edits)");
        foreach (var p in presets) AdvPresetCombo.Items.Add(p);
        AdvPresetCombo.SelectedIndex = 0;
    }

    private void OnAdvPresetSelected(object sender, SelectionChangedEventArgs e)
    {
        if (!_isAdvancedMode || AdvPresetCombo.SelectedIndex <= 0) return;
        string name = AdvPresetCombo.SelectedItem?.ToString() ?? "";
        try
        {
            string json = File.ReadAllText(Path.Combine(AdvPresetsDir, $"{name}.json"));
            var overrides = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (overrides == null) return;

            foreach (var row in _advAllRows) row.Value = row.VanillaValue;

            var lookup = _advAllRows.ToDictionary(r => r.FullKey, r => r);
            int applied = 0;
            foreach (var (key, val) in overrides)
                if (lookup.TryGetValue(key, out var row)) { row.Value = val; applied++; }

            AdvApplyFilter();
            SetStatus($"Loaded preset '{name}' ({applied} overrides).", "Success");
        }
        catch (Exception ex) { SetStatus($"Load failed: {ex.Message}", "Error"); }
    }

    private void OnAdvSavePreset(object sender, RoutedEventArgs e)
    {
        string name;

        if (AdvPresetCombo.SelectedIndex > 0)
        {
            name = AdvPresetCombo.SelectedItem?.ToString() ?? "";
            var confirm = MessageBox.Show(
                $"Overwrite advanced preset '{name}' with current values?",
                "Overwrite Preset", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;
        }
        else
        {
            var dlg = new InputDialog("Save Advanced Preset", "Enter a name for this preset:") { Owner = this };
            if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ResponseText)) return;
            name = dlg.ResponseText.Trim().Replace(" ", "_");
            if (name.Length > 40) name = name[..40];
        }

        try
        {
            var modified = _advAllRows.Where(r => r.IsModified)
                .ToDictionary(r => r.FullKey, r => r.Value);

            if (modified.Count == 0) { SetStatus("No modified values to save.", "TextSecondary"); return; }

            File.WriteAllText(Path.Combine(AdvPresetsDir, $"{name}.json"),
                JsonSerializer.Serialize(modified, new JsonSerializerOptions { WriteIndented = true }));

            AdvRefreshPresetCombo();
            for (int i = 0; i < AdvPresetCombo.Items.Count; i++)
                if (AdvPresetCombo.Items[i]?.ToString() == name) { AdvPresetCombo.SelectedIndex = i; break; }

            SetStatus($"Advanced preset '{name}' saved ({modified.Count} overrides).", "Success");
        }
        catch (Exception ex) { SetStatus($"Save failed: {ex.Message}", "Error"); }
    }

    private void OnAdvDeletePreset(object sender, RoutedEventArgs e)
    {
        if (AdvPresetCombo.SelectedIndex <= 0) { SetStatus("Select a saved preset first.", "TextSecondary"); return; }
        string name = AdvPresetCombo.SelectedItem?.ToString() ?? "";
        try
        {
            string path = Path.Combine(AdvPresetsDir, $"{name}.json");
            if (File.Exists(path)) File.Delete(path);
            AdvRefreshPresetCombo();
            SetStatus($"Advanced preset '{name}' deleted.", "Success");
        }
        catch (Exception ex) { SetStatus($"Delete failed: {ex.Message}", "Error"); }
    }

    private void OnNexusClick(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(NexusUrl) { UseShellExecute = true });
    private void OnGitHubClick(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(GitHubUrl) { UseShellExecute = true });

    private static int GetInt(Dictionary<string, object>? dict, string key, int def = 0)
    {
        if (dict == null || !dict.TryGetValue(key, out var val)) return def;
        if (val is JsonElement je) return je.TryGetInt32(out int i) ? i : def;
        return int.TryParse(val.ToString(), out int r) ? r : def;
    }

    private static bool GetBool(Dictionary<string, object>? dict, string key)
    {
        if (dict == null || !dict.TryGetValue(key, out var val)) return false;
        if (val is JsonElement je) return je.ValueKind == JsonValueKind.True;
        return bool.TryParse(val.ToString(), out bool b) && b;
    }
}
