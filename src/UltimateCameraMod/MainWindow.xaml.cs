using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
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
    private const string Ver = "2.5";
    private const string NexusUrl = "https://www.nexusmods.com/crimsondesert/mods/438";
    private const string GitHubUrl = "https://github.com/FitzDegenhub/UltimateCameraMod";
    private const string KoFiUrl = "https://ko-fi.com/0xfitz";

    private static readonly string ExeDir =
        Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

    private string _gameDir = "";
    private string _detectedPlatform = "Unknown";
    private string _activeTab = "presets";
    private bool _suppressEvents;
    private Dictionary<string, object>? _savedState;

    // ── Mode state ────────────────────────────────────────────────────
    private string _activeMode = "simple";
    private bool _isExpertMode;
    private List<AdvancedRow> _advAllRows = new();

    // ── JSON Mod Manager state ────────────────────────────────────────
    private List<JsonModExporter.PatchChange>? _jsonLastPatches;
    private string? _jsonLastJson;
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
        ("default",   "Vanilla  -  Default framing + steadycam smoothing"),
        ("immersive", "Close-Up  -  Shoulder OTS, tighter (16:9 feel)"),
        ("lowcam",    "Low Rider  -  Hip-level, full body + horizon"),
        ("vlowcam",   "Knee Cam  -  Knee-height dramatic low angle"),
        ("ulowcam",   "Dirt Cam  -  Ground-level, extreme low"),
        ("re2",       "Survival  -  Tight horror-game OTS (16:9 feel)"),
    };

    private static readonly (int Value, string Label)[] FovOptions =
    {
        (0,  "No FoV boost (0\u00b0)  -  vanilla varies by state (40\u00b0, 45\u00b0, 53\u00b0\u2026)"),
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

    private static string WindowStatePath => Path.Combine(ExeDir, "window_state.json");

    public MainWindow()
    {
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnWindowClosing;
        RestoreWindowSize();
    }

    private void RestoreWindowSize()
    {
        try
        {
            if (!File.Exists(WindowStatePath)) return;
            var doc = JsonDocument.Parse(File.ReadAllText(WindowStatePath));
            double w = doc.RootElement.GetProperty("width").GetDouble();
            double h = doc.RootElement.GetProperty("height").GetDouble();
            if (w >= 800 && h >= 600) { Width = w; Height = h; }
        }
        catch { }
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            string json = JsonSerializer.Serialize(new { width = Width, height = Height });
            File.WriteAllText(WindowStatePath, json);
        }
        catch { }
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
        CameraMod.AppVersion = Ver;

        CleanStaleData(backupsDir);
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
        CheckGitHubVersion();
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
        SteadycamCheck.IsChecked = GetBool(_savedState, "steadycam", true);

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

            if (savedStyle == "custom")
                SwitchTab("custom");
        }

        _suppressEvents = false;

        DistLabel.Text = $"{DistSlider.Value:F1}";
        HeightLabel.Text = $"{HeightSlider.Value:F1}";
        HShiftLabel.Text = $"{HShiftSlider.Value:F1}";
        ApplyCenteredLock();
        SyncPreview();
    }

    // ── Stale data cleanup ───────────────────────────────────────────

    private void CleanStaleData(string backupsDir)
    {
        try
        {
            string metaPath = Path.Combine(backupsDir, "backup_meta.txt");
            if (!File.Exists(metaPath)) return;

            string meta = File.ReadAllText(metaPath);
            string savedVer = "";
            foreach (var part in meta.Split())
            {
                if (part.StartsWith("ucm_version="))
                    savedVer = part["ucm_version=".Length..];
            }

            if (savedVer == Ver) return;

            // Version mismatch -- wipe stale backups and saved state
            if (Directory.Exists(backupsDir))
                Directory.Delete(backupsDir, true);
            if (File.Exists(StatePath))
                File.Delete(StatePath);

            SetStatus($"Cleaned old v{(string.IsNullOrEmpty(savedVer) ? "?" : savedVer)} data. Please click Install to apply your settings.", "Warn");
        }
        catch { }
    }

    // ── State persistence ────────────────────────────────────────────

    private static string StatePath => Path.Combine(ExeDir, "last_install.json");

    private static Dictionary<string, object>? LoadInstallState()
    {
        try
        {
            if (!File.Exists(StatePath)) return null;
            string json = File.ReadAllText(StatePath);
            var state = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (state != null)
            {
                string savedVer = state.GetValueOrDefault("ucm_version")?.ToString() ?? "";
                if (savedVer != Ver)
                    return null;
            }
            return state;
        }
        catch { return null; }
    }

    private void SaveInstallState(int compSize, string style, int fov, bool bane, string combat,
        Dictionary<string, double>? customParams, bool mountHeight, bool steadycam = true)
    {
        var state = new Dictionary<string, object>
        {
            ["ucm_version"] = Ver,
            ["comp_size"] = compSize, ["style"] = style, ["fov"] = fov,
            ["bane"] = bane, ["combat"] = combat, ["mount_height"] = mountHeight,
            ["steadycam"] = steadycam,
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
            if (_savedState == null)
            {
                BannerPanel.Visibility = Visibility.Collapsed;
                return;
            }
            ShowBannerFromState(_savedState);
        }
        catch { BannerPanel.Visibility = Visibility.Collapsed; }
    }

    private void ShowBannerFromState(Dictionary<string, object> state)
    {
        int fov = GetInt(state, "fov", 0);
        string style = state.GetValueOrDefault("style")?.ToString() ?? "default";
        bool bane = GetBool(state, "bane");
        string combat = state.GetValueOrDefault("combat")?.ToString() ?? "default";
        bool mountH = GetBool(state, "mount_height");
        bool steadycam = GetBool(state, "steadycam", true);

        double dist = 5.0, height = 0.0, hshift = 0.0;
        if (state.ContainsKey("custom"))
        {
            try
            {
                var custom = JsonSerializer.Deserialize<JsonElement>(state["custom"].ToString()!);
                dist = custom.TryGetProperty("distance", out var d) ? d.GetDouble() : 5.0;
                height = custom.TryGetProperty("height", out var h) ? h.GetDouble() : 0.0;
                hshift = custom.TryGetProperty("right_offset", out var r) ? r.GetDouble() : 0.0;
            }
            catch { }
        }

        string styleName = style;
        foreach (var (id, lbl) in Styles)
            if (id == style) { styleName = lbl.Split("  -  ")[0]; break; }
        if (style == "custom") styleName = "Custom";

        var parts = new List<string>();

        if (fov != 0)
            parts.Add($"FoV +{fov}\u00b0");

        if (style == "custom")
            parts.Add($"Dist {dist:F1}  |  Height {height:F1}  |  Shift {hshift:F1}");
        else
            parts.Add(styleName);

        if (bane) parts.Add("Centered");

        var globals = new List<string>();
        if (combat != "default") globals.Add("Combat");
        if (mountH) globals.Add("Mount cam");
        if (steadycam) globals.Add("Steadycam");
        if (globals.Count > 0)
            parts.Add(string.Join(", ", globals));

        if (parts.Count == 0)
        {
            BannerPanel.Visibility = Visibility.Collapsed;
            return;
        }

        BannerPanel.Visibility = Visibility.Visible;
        BannerPanel.Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x1F, 0x00));
        BannerText.Text = $"\u26A0  Game files modified:  {string.Join("  |  ", parts)}";
        BannerText.Foreground = FindResource("WarnBrush") as Brush;
    }

    // ── GitHub version check ─────────────────────────────────────────

    private async void CheckGitHubVersion()
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("UltimateCameraMod/" + Ver);
            http.Timeout = TimeSpan.FromSeconds(8);
            string json = await http.GetStringAsync(
                "https://api.github.com/repos/FitzDegenhub/UltimateCameraMod/releases/latest");
            using var doc = JsonDocument.Parse(json);
            string tag = doc.RootElement.GetProperty("tag_name").GetString() ?? "";
            string latest = tag.TrimStart('v', 'V');

            string pad(string v) => v.Contains('.') && v.Split('.').Length == 2 ? v + ".0" : v;
            bool isOutdated = !string.IsNullOrEmpty(latest)
                && Version.TryParse(pad(latest), out var remote)
                && Version.TryParse(pad(Ver), out var local)
                && remote > local;

            Dispatcher.Invoke(() =>
            {
                if (isOutdated)
                {
                    VersionDot.Fill = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                    VersionStatus.Text = $"v{latest} available";
                    VersionStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                    VersionUpdateBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    VersionDot.Fill = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                    VersionStatus.Text = "up to date";
                    VersionStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                }
            });
        }
        catch
        {
            Dispatcher.Invoke(() =>
            {
                VersionDot.Fill = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                VersionStatus.Text = "offline";
                VersionStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            });
        }
    }

    private void OnUpdateNexusClick(object s, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(NexusUrl) { UseShellExecute = true });
    private void OnUpdateGitHubClick(object s, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(GitHubUrl + "/releases/latest") { UseShellExecute = true });

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
            FovPreviewCtrl.UpdateParams(fov, ro, centered, d);
        }
        else
        {
            string sid = GetSelectedStyleId();
            var (d, u, ro) = StyleParams.GetValueOrDefault(sid, (3.4, 0.0, 0.0));
            string name = sid;
            foreach (var (id, lbl) in Styles)
                if (id == sid) { name = lbl.Split("  -  ")[0]; break; }
            Preview.UpdateParams(d, u, name);
            FovPreviewCtrl.UpdateParams(fov, ro, centered, d);
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

    private void ApplyCenteredLock()
    {
        if (BaneCheck.IsChecked == true)
        {
            HShiftSlider.Value = 0;
            HShiftSlider.IsEnabled = false;
            HShiftLabel.Text = "0.0";
            HShiftLabel.Foreground = (Brush)FindResource("TextDimBrush");
            HShiftTip.Text = "\u26A0 Centered Camera forces character to screen center \u2014 untick to adjust";
        }
        else
        {
            HShiftSlider.IsEnabled = true;
            HShiftLabel.Foreground = (Brush)FindResource("TextPrimaryBrush");
            HShiftTip.Text = "0 = vanilla (character slightly left). Center is ~0.5. Negative = further left, positive = further right.";
        }
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
            HShiftSlider.Value = Math.Clamp(data.GetValueOrDefault("right_offset", 0.0), -3.0, 3.0);
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
        bool steadycam = SteadycamCheck.IsChecked == true;

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
                    mountHeight: mountHeight, customUp: customUp, steadycam: steadycam,
                    log: msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent")));
                bool ok = result.GetValueOrDefault("status")?.ToString() == "ok";
                int compSize = ok && result.ContainsKey("comp_size") ? (int)result["comp_size"] : 0;

                Dispatcher.Invoke(() =>
                {
                    if (ok)
                    {
                        SetStatus("Installed! Launch the game.", "Success");
                        SaveInstallState(compSize, styleId, fov, bane, combat, customParams, mountHeight, steadycam);
                        _savedState = LoadInstallState();
                        CheckForUpdate();
                    }
                    else
                    {
                        SetStatus("Install failed. Is the game running?", "Error");
                        CheckForUpdate();
                    }
                    SetButtons(true);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Error: {ex.Message}", "Error");
                    CheckForUpdate();
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

                Dispatcher.Invoke(() =>
                {
                    switch (status)
                    {
                        case "ok":
                            SetStatus("Vanilla restored.", "Success"); break;
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

    // ── 4-Mode navigation ────────────────────────────────────────────

    private void OnModeSimple(object s, RoutedEventArgs e) => SwitchAppMode("simple");
    private void OnModeAdvanced(object s, RoutedEventArgs e) => SwitchAppMode("advanced");
    private void OnModeExpert(object s, RoutedEventArgs e) => SwitchAppMode("expert");
    private void OnModeJson(object s, RoutedEventArgs e) => SwitchAppMode("json");

    private void SwitchAppMode(string mode)
    {
        if (mode != "simple" && string.IsNullOrEmpty(_gameDir))
        {
            MessageBox.Show("Set a game folder first (click Install to browse).",
                "Ultimate Camera Mod", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Hide all views and button groups
        SimpleView.Visibility = Visibility.Collapsed;
        AdvancedControlsView.Visibility = Visibility.Collapsed;
        ExpertView.Visibility = Visibility.Collapsed;
        JsonModManagerView.Visibility = Visibility.Collapsed;

        SimpleButtons.Visibility = Visibility.Collapsed;
        AdvancedControlsButtons.Visibility = Visibility.Collapsed;
        ExpertButtons.Visibility = Visibility.Collapsed;
        JsonModManagerButtons.Visibility = Visibility.Collapsed;

        // Update mode button styles
        ModeSimpleBtn.Style = (Style)FindResource(mode == "simple" ? "AccentButton" : "SubtleButton");
        ModeAdvancedBtn.Style = (Style)FindResource(mode == "advanced" ? "AccentButton" : "SubtleButton");
        ModeExpertBtn.Style = (Style)FindResource(mode == "expert" ? "AccentButton" : "SubtleButton");
        ModeJsonBtn.Style = (Style)FindResource(mode == "json" ? "AccentButton" : "SubtleButton");

        _activeMode = mode;
        _isExpertMode = mode == "expert";

        switch (mode)
        {
            case "simple":
                SimpleView.Visibility = Visibility.Visible;
                SimpleButtons.Visibility = Visibility.Visible;
                CheckForUpdate();
                SetStatus("Ready", "TextDim");
                break;

            case "advanced":
                AdvancedControlsView.Visibility = Visibility.Visible;
                AdvancedControlsButtons.Visibility = Visibility.Visible;
                EnterAdvancedControlsMode();
                SetStatus("Advanced Controls — guided per-parameter camera tuning.", "TextDim");
                break;

            case "expert":
                ExpertView.Visibility = Visibility.Visible;
                ExpertButtons.Visibility = Visibility.Visible;
                EnterExpertMode();
                break;

            case "json":
                JsonModManagerView.Visibility = Visibility.Visible;
                JsonModManagerButtons.Visibility = Visibility.Visible;
                SetStatus("JSON Mod Manager — generate Crimson Desert Mod Manager v8 patch files.", "TextDim");
                break;
        }
    }

    private void EnterExpertMode()
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

            SetStatus("Expert mode — edit raw XML values and click Apply to Game.", "TextDim");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load camera XML:\n{ex.Message}", "Expert Editor",
                MessageBoxButton.OK, MessageBoxImage.Error);
            SwitchAppMode("simple");
        }
    }

    private void AdvBindGrid()
    {
        _advFilteredRows = new ObservableCollection<AdvancedRow>(_advAllRows);
        var view = CollectionViewSource.GetDefaultView(_advFilteredRows);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Section"));
        ExpertDataGrid.ItemsSource = view;
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
        if (!IsLoaded || !_isExpertMode) return;
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
        ExpertDataGrid.ItemsSource = view;
        AdvUpdateRowCount();
    }

    private void OnAdvExpandAll(object sender, RoutedEventArgs e)
    {
        bool expanding = (sender as System.Windows.Controls.Button)?.Content?.ToString() == "Expand All";
        ExpertDataGrid.GroupStyle.Clear();
        ExpertDataGrid.GroupStyle.Add(BuildAdvGroupStyle(expanding));
        if (sender is System.Windows.Controls.Button btn)
            btn.Content = expanding ? "Collapse All" : "Expand All";
    }

    private static GroupStyle BuildAdvGroupStyle(bool expanded)
    {
        string xaml =
            "<GroupStyle xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
            "<GroupStyle.ContainerStyle>" +
            "<Style TargetType='GroupItem'>" +
            "<Setter Property='Template'><Setter.Value>" +
            "<ControlTemplate TargetType='GroupItem'>" +
            "<Expander IsExpanded='" + (expanded ? "True" : "False") + "' Background='Transparent' BorderThickness='0'>" +
            "<Expander.Header>" +
            "<TextBlock Text='{Binding Name}' FontSize='11' FontWeight='SemiBold' Foreground='#c8a24e' Margin='4,2'/>" +
            "</Expander.Header>" +
            "<ItemsPresenter/>" +
            "</Expander>" +
            "</ControlTemplate>" +
            "</Setter.Value></Setter>" +
            "</Style>" +
            "</GroupStyle.ContainerStyle>" +
            "</GroupStyle>";
        return (GroupStyle)System.Windows.Markup.XamlReader.Parse(xaml);
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
            bool sc = SteadycamCheck.IsChecked == true;
            var modSet = CameraRules.BuildModifications(styleId, fov, bane, combat, mountHeight: mount, customUp: customUp, steadycam: sc);
            vanillaXml = CameraMod.ApplyModifications(vanillaXml, modSet);

            var defaultRows = CameraMod.ParseXmlToRows(vanillaXml);
            var lookup = new Dictionary<string, string>();
            foreach (var dr in defaultRows)
                lookup[dr.FullKey] = dr.Value;

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

        var payload = new Dictionary<string, string>();
        foreach (var r in modified) payload[r.FullKey] = r.Value;
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
        var lookup = new Dictionary<string, AdvancedRow>();
        foreach (var r in _advAllRows) lookup[r.FullKey] = r;
        foreach (var (key, val) in dlg.Result)
        {
            if (lookup.TryGetValue(key, out var row)) { row.Value = val; applied++; }
        }

        AdvApplyFilter();
        SetStatus($"Imported {applied} values.", "Success");
    }

    private void OnAdvImportXml(object sender, RoutedEventArgs e)
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import playercamerapreset.xml",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        try
        {
            string xml = File.ReadAllText(ofd.FileName);
            var importedRows = CameraMod.ParseXmlToRows(xml);
            var lookup = new Dictionary<string, string>();
            foreach (var r in importedRows)
                lookup[r.FullKey] = r.Value;

            int applied = 0;
            foreach (var row in _advAllRows)
            {
                if (lookup.TryGetValue(row.FullKey, out string? val) && val != row.Value)
                {
                    row.Value = val;
                    applied++;
                }
            }

            AdvApplyFilter();
            SetStatus($"Imported {applied} values from {Path.GetFileName(ofd.FileName)}.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"XML import failed: {ex.Message}", "Error");
        }
    }

    private void OnAdvApply(object sender, RoutedEventArgs e)
    {
        var modifiedRows = _advAllRows.Where(r => r.IsModified).ToList();
        if (modifiedRows.Count == 0) { SetStatus("No changes to apply.", "TextSecondary"); return; }

        ExpertApplyBtn.IsEnabled = false;
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
                    ExpertApplyBtn.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Apply failed: {ex.Message}", "Error");
                    ExpertApplyBtn.IsEnabled = true;
                });
            }
        });
    }

    private void AdvSaveOverrides()
    {
        try
        {
            var modified = new Dictionary<string, string>();
            foreach (var r in _advAllRows.Where(r => r.IsModified)) modified[r.FullKey] = r.Value;
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
            var lookup = new Dictionary<string, AdvancedRow>();
            foreach (var r in _advAllRows) lookup[r.FullKey] = r;
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
        if (!_isExpertMode || AdvPresetCombo.SelectedIndex <= 0) return;
        string name = AdvPresetCombo.SelectedItem?.ToString() ?? "";
        try
        {
            string json = File.ReadAllText(Path.Combine(AdvPresetsDir, $"{name}.json"));
            var overrides = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (overrides == null) return;

            foreach (var row in _advAllRows) row.Value = row.VanillaValue;

            var lookup = new Dictionary<string, AdvancedRow>();
            foreach (var r in _advAllRows) lookup[r.FullKey] = r;
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
            var modified = new Dictionary<string, string>();
            foreach (var r in _advAllRows.Where(r => r.IsModified)) modified[r.FullKey] = r.Value;

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

    // ── XML Export / Import ──────────────────────────────────────────

    private void OnExportXmlFile(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir))
        {
            SetStatus("Game folder not set.", "Warn");
            return;
        }

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Export Camera XML",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (sfd.ShowDialog(this) != true) return;

        try
        {
            CameraMod.ExportLiveXml(_gameDir, sfd.FileName);
            SetStatus($"Exported to {Path.GetFileName(sfd.FileName)}.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Export failed: {ex.Message}", "Error");
        }
    }

    private void OnImportXmlFile(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir))
        {
            SetStatus("Game folder not set.", "Warn");
            return;
        }

        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import Camera XML — installs directly to game",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        var confirm = MessageBox.Show(
            $"Install '{Path.GetFileName(ofd.FileName)}' directly to the game?\n\nThis will overwrite your current camera settings.",
            "Import XML", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            string xml = File.ReadAllText(ofd.FileName);
            CameraMod.InstallRawXml(_gameDir, xml);
            SetStatus($"Installed {Path.GetFileName(ofd.FileName)} to game.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Import failed: {ex.Message}", "Error");
        }
    }

    // ── Advanced Controls ────────────────────────────────────────────

    // Stores all slider controls keyed by ModKey.Attribute (same as AdvancedRow.FullKey)
    private readonly Dictionary<string, Slider> _advCtrlSliders = new();
    private readonly Dictionary<string, TextBlock> _advCtrlValueLabels = new();
    private readonly Dictionary<string, string> _advCtrlVanilla = new();
    private static string AdvCtrlPresetsDir
    {
        get { string d = Path.Combine(ExeDir, "advanced_presets"); Directory.CreateDirectory(d); return d; }
    }

    private void EnterAdvancedControlsMode()
    {
        if (_advCtrlSliders.Count > 0) return; // already built

        try
        {
            // Load vanilla values for display and reset
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
            var vanillaRows = CameraMod.ParseXmlToRows(vanillaXml);
            _advCtrlVanilla.Clear();
            foreach (var r in vanillaRows) _advCtrlVanilla[r.FullKey] = r.VanillaValue;

            BuildAdvCtrlSection_OnFoot();
            BuildAdvCtrlSection_Mount();
            BuildAdvCtrlSection_Combat();
            BuildAdvCtrlSection_Smooth();
            BuildAdvCtrlSection_Aim();

            AdvCtrlRefreshPresetCombo();
            AdvCtrlUpdateChangedLabel();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load camera XML:\n{ex.Message}", "Advanced Controls",
                MessageBoxButton.OK, MessageBoxImage.Error);
            SwitchAppMode("simple");
        }
    }

    // ── Control builder helpers ──────────────────────────────────────

    private Grid BuildSliderRow(string modKey, string attribute, double min, double max, double step,
        string? tooltip = null)
    {
        string fullKey = $"{modKey}.{attribute}";
        _advCtrlVanilla.TryGetValue(fullKey, out string? vanillaStr);
        double vanillaVal = double.TryParse(vanillaStr, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double vv) ? vv : (min + max) / 2;
        double current = vanillaVal;

        var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130), MinWidth = 130 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 60 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46), MinWidth = 46 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46), MinWidth = 46 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28), MinWidth = 28 });

        // Label
        var label = new TextBlock
        {
            Text = attribute,
            FontSize = 11,
            Foreground = (Brush)FindResource("TextSecondaryBrush"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            ToolTip = tooltip ?? CameraParamDocs.Get(attribute)
        };
        Grid.SetColumn(label, 0);

        // Slider
        var slider = new Slider
        {
            Minimum = min, Maximum = max,
            Value = current,
            TickFrequency = step,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 4, 0),
            ToolTip = tooltip ?? CameraParamDocs.Get(attribute)
        };
        Grid.SetColumn(slider, 1);

        // Value label
        var valueLabel = new TextBlock
        {
            Text = $"{current:F2}",
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 10,
            Foreground = (Brush)FindResource("AccentBrush"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 0, 2, 0)
        };
        Grid.SetColumn(valueLabel, 2);

        // Vanilla label
        var vanillaLabel = new TextBlock
        {
            Text = $"{vanillaVal:F2}",
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 10,
            Foreground = (Brush)FindResource("TextDimBrush"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 0, 2, 0),
            ToolTip = $"Vanilla: {vanillaVal:F2}"
        };
        Grid.SetColumn(vanillaLabel, 3);

        // Reset button
        var resetBtn = new System.Windows.Controls.Button
        {
            Content = "↺",
            FontSize = 11,
            Width = 22, Height = 20,
            Margin = new Thickness(2, 0, 0, 0),
            ToolTip = $"Reset to vanilla ({vanillaVal:F2})",
            Style = (Style)FindResource("SubtleButton"),
            Tag = (slider, vanillaVal)
        };
        Grid.SetColumn(resetBtn, 4);
        resetBtn.Click += (s, e) =>
        {
            if (((System.Windows.Controls.Button)s).Tag is (Slider sl, double vv2))
                sl.Value = vv2;
        };

        slider.ValueChanged += (s, e) =>
        {
            valueLabel.Text = $"{e.NewValue:F2}";
            bool changed = Math.Abs(e.NewValue - vanillaVal) > 0.001;
            valueLabel.Foreground = changed
                ? (Brush)FindResource("AccentBrush")
                : (Brush)FindResource("TextDimBrush");
            AdvCtrlUpdateChangedLabel();
        };

        row.Children.Add(label);
        row.Children.Add(slider);
        row.Children.Add(valueLabel);
        row.Children.Add(vanillaLabel);
        row.Children.Add(resetBtn);

        _advCtrlSliders[fullKey] = slider;
        _advCtrlValueLabels[fullKey] = valueLabel;

        return row;
    }

    private StackPanel BuildZoomLevelGroup(string title, string[] sections, int zoomLevel,
        (string Attr, double Min, double Max, double Step)[] attrs)
    {
        var group = new StackPanel { Margin = new Thickness(0, 8, 0, 4) };
        group.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 11, FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("TextSecondaryBrush"),
            Margin = new Thickness(0, 0, 0, 4)
        });

        // Use first section as the representative key (all sections share same values in UCM)
        string repSection = sections[0];
        string modKey = $"{repSection}/ZoomLevel[{zoomLevel}]";

        foreach (var (attr, min, max, step) in attrs)
        {
            // Register all sections' keys so BuildAdvancedControlsModSet can apply to all
            foreach (var sec in sections)
                _advCtrlSliders[$"{sec}/ZoomLevel[{zoomLevel}].{attr}"] = null!; // placeholder

            var row = BuildSliderRow(modKey, attr, min, max, step);
            group.Children.Add(row);

            // Re-register the actual slider for all sections
            var actualSlider = _advCtrlSliders[$"{modKey}.{attr}"];
            foreach (var sec in sections)
            {
                string k = $"{sec}/ZoomLevel[{zoomLevel}].{attr}";
                _advCtrlSliders[k] = actualSlider;
            }
        }
        return group;
    }

    private void BuildAdvCtrlSection_OnFoot()
    {
        var panel = new StackPanel();
        string[] allOnFoot = {
            "Player_Basic_Default", "Player_Basic_Default_Walk",
            "Player_Basic_Default_Run", "Player_Basic_Default_Runfast",
            "Player_Weapon_Default", "Player_Weapon_Default_Walk",
            "Player_Weapon_Default_Run", "Player_Weapon_Default_RunFast",
            "Player_Weapon_Default_RunFast_Follow", "Player_Weapon_Rush", "Player_Weapon_Guard"
        };

        (string, double, double, double)[] zoomAttrs = {
            ("ZoomDistance",    1.0, 20.0, 0.1),
            ("UpOffset",       -2.0,  1.0, 0.1),
            ("InDoorUpOffset", -2.0,  1.0, 0.1),
            ("RightOffset",    -1.0,  3.0, 0.05),
        };

        foreach (int zl in new[] { 2, 3, 4 })
            panel.Children.Add(BuildZoomLevelGroup($"Zoom Level {zl}", allOnFoot, zl, zoomAttrs));

        AdvCtrlOnFootGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Mount()
    {
        var panel = new StackPanel();
        string[] horseSections = {
            "Player_Ride_Horse", "Player_Ride_Horse_Run", "Player_Ride_Horse_Fast_Run",
            "Player_Ride_Horse_Dash", "Player_Ride_Horse_Dash_Att",
            "Player_Ride_Horse_Att_Thrust", "Player_Ride_Horse_Att_R", "Player_Ride_Horse_Att_L"
        };

        (string, double, double, double)[] zoomAttrs = {
            ("ZoomDistance", 0.5, 25.0, 0.1),
            ("UpOffset",    -2.0,  2.0, 0.1),
            ("RightOffset", -1.0,  4.0, 0.05),
        };

        foreach (int zl in new[] { 0, 1, 2, 3 })
            panel.Children.Add(BuildZoomLevelGroup($"Zoom Level {zl}", horseSections, zl, zoomAttrs));

        AdvCtrlMountGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Combat()
    {
        var panel = new StackPanel();

        // Section-level attributes
        var sectionAttrs = new[]
        {
            ("Player_Weapon_LockOn",    "TargetRate",      0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn",    "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_TwoTarget", "TargetRate",      0.0, 1.0, 0.05),
            ("Player_Weapon_TwoTarget", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_TwoTarget", "LimitUnderDistance", 0.5, 10.0, 0.5),
        };

        panel.Children.Add(new TextBlock
        {
            Text = "Lock-On Tracking",
            FontSize = 11, FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("TextSecondaryBrush"),
            Margin = new Thickness(0, 8, 0, 4)
        });
        foreach (var (sec, attr, min, max, step) in sectionAttrs)
        {
            var row = BuildSliderRow(sec, attr, min, max, step);
            // Prefix label with section name for clarity
            if (row.Children[0] is TextBlock lbl)
                lbl.Text = $"{sec.Replace("Player_Weapon_", "")} — {attr}";
            panel.Children.Add(row);
        }

        // ZoomDistance per lock-on section per zoom level
        var lockOnSections = new[]
        {
            ("Player_Weapon_LockOn",    new[] { 2, 3 }),
            ("Player_Weapon_TwoTarget", new[] { 1, 2 }),
            ("Player_FollowLearn_LockOn_Boss", new[] { 2, 3 }),
        };

        foreach (var (sec, levels) in lockOnSections)
        {
            panel.Children.Add(new TextBlock
            {
                Text = $"{sec.Replace("Player_", "")} — Zoom Distances",
                FontSize = 11, FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 10, 0, 4)
            });
            foreach (int zl in levels)
            {
                var row = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "ZoomDistance", 1.0, 20.0, 0.5);
                if (row.Children[0] is TextBlock lbl) lbl.Text = $"ZL{zl} ZoomDistance";
                panel.Children.Add(row);
            }
        }

        AdvCtrlCombatGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Smooth()
    {
        var panel = new StackPanel();

        var smoothEntries = new[]
        {
            ("Player_Basic_Default_Run/CameraBlendParameter",  "BlendInTime",  0.0, 3.0, 0.1, "Run blend-in"),
            ("Player_Basic_Default_Run/CameraBlendParameter",  "BlendOutTime", 0.0, 3.0, 0.1, "Run blend-out"),
            ("Player_Weapon_Guard/CameraBlendParameter",       "BlendInTime",  0.0, 3.0, 0.1, "Guard blend-in"),
            ("Player_Weapon_Guard/CameraBlendParameter",       "BlendOutTime", 0.0, 3.0, 0.1, "Guard blend-out"),
            ("Player_Basic_Default_Run/OffsetByVelocity",      "OffsetLength", 0.0, 2.0, 0.1, "Run sway"),
            ("Player_Weapon_Default_Run/OffsetByVelocity",     "OffsetLength", 0.0, 2.0, 0.1, "Combat run sway"),
            ("Player_Ride_Horse/CameraBlendParameter",         "BlendInTime",  0.0, 3.0, 0.1, "Horse blend-in"),
            ("Player_Ride_Horse/CameraBlendParameter",         "BlendOutTime", 0.0, 3.0, 0.1, "Horse blend-out"),
            ("Player_Ride_Horse/OffsetByVelocity",             "OffsetLength", 0.0, 2.0, 0.1, "Horse sway"),
            ("Player_Ride_Horse/OffsetByVelocity",             "DampSpeed",    0.0, 2.0, 0.1, "Horse sway damp"),
            ("Player_Ride_Horse",                              "FollowYawSpeedRate",   0.0, 2.0, 0.05, "Horse yaw follow"),
            ("Player_Ride_Horse",                              "FollowPitchSpeedRate", 0.0, 2.0, 0.05, "Horse pitch follow"),
            ("Player_Ride_Horse",                              "FollowStartTime",      0.0, 5.0, 0.1,  "Horse follow delay"),
        };

        foreach (var (modKey, attr, min, max, step, friendlyName) in smoothEntries)
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock lbl) lbl.Text = friendlyName;
            panel.Children.Add(row);
        }

        AdvCtrlSmoothGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Aim()
    {
        var panel = new StackPanel();

        // Group aim sections by type
        var aimGroups = new[]
        {
            ("Lantern / Spotlight", new[] {
                ("Player_Basic_Default_Aim_Zoom", 2), ("Player_Basic_Default_Aim_Zoom", 3), ("Player_Basic_Default_Aim_Zoom", 4) }),
            ("Blinding Flash", new[] {
                ("Player_Taeguk_Aim", 2), ("Player_Taeguk_Aim", 3) }),
            ("Weapon Aim / Zoom", new[] {
                ("Player_Weapon_Aim_Zoom", 2), ("Player_Weapon_Aim_Zoom", 3),
                ("Player_Weapon_Zoom", 2), ("Player_Weapon_Zoom", 3) }),
            ("Bow", new[] {
                ("Player_Bow_Aim_Zoom", 2), ("Player_Bow_Aim_LockOn", 2) }),
            ("Glide / FreeFall", new[] {
                ("Glide_Kick_Aim_Zoom", 2), ("Player_Basic_FreeFall_Aim", 2) }),
        };

        foreach (var (groupName, entries) in aimGroups)
        {
            panel.Children.Add(new TextBlock
            {
                Text = groupName,
                FontSize = 11, FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 10, 0, 4)
            });
            foreach (var (sec, zl) in entries)
            {
                var row = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "RightOffset", -1.0, 3.0, 0.05);
                if (row.Children[0] is TextBlock lbl)
                    lbl.Text = $"{sec.Replace("Player_", "").Replace("_Aim_Zoom", "").Replace("_Aim", "")} ZL{zl}";
                panel.Children.Add(row);
            }
        }

        AdvCtrlAimGrid.Children.Add(panel);
    }

    // ── Advanced Controls apply / ModSet ─────────────────────────────

    private ModificationSet BuildAdvancedControlsModSet()
    {
        var mods = new Dictionary<string, Dictionary<string, (string, string)>>();

        foreach (var (fullKey, slider) in _advCtrlSliders)
        {
            if (slider == null) continue;
            _advCtrlVanilla.TryGetValue(fullKey, out string? vanillaStr);
            double vanillaVal = double.TryParse(vanillaStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double vv) ? vv : double.NaN;

            if (Math.Abs(slider.Value - vanillaVal) < 0.001) continue; // unchanged

            int dotIdx = fullKey.LastIndexOf('.');
            if (dotIdx < 0) continue;
            string modKey = fullKey[..dotIdx];
            string attr = fullKey[(dotIdx + 1)..];

            if (!mods.TryGetValue(modKey, out var attrs))
            {
                attrs = new Dictionary<string, (string, string)>();
                mods[modKey] = attrs;
            }
            attrs[attr] = ("SET", $"{slider.Value:F2}");
        }

        return new ModificationSet { ElementMods = mods, FovValue = 0 };
    }

    private void AdvCtrlUpdateChangedLabel()
    {
        if (!IsLoaded) return;
        int changed = _advCtrlSliders.Values
            .Where(s => s != null)
            .Count(s =>
            {
                string? key = _advCtrlSliders.FirstOrDefault(kv => kv.Value == s).Key;
                if (key == null) return false;
                _advCtrlVanilla.TryGetValue(key, out string? vs);
                double vv = double.TryParse(vs, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : double.NaN;
                return Math.Abs(s.Value - vv) > 0.001;
            });
        AdvCtrlChangedLabel.Text = $"{changed} change{(changed == 1 ? "" : "s")}";
    }

    private void OnAdvCtrlApply(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir)) { SetStatus("Game folder not set.", "Warn"); return; }

        var modSet = BuildAdvancedControlsModSet();
        int count = modSet.ElementMods.Values.Sum(v => v.Count);
        if (count == 0) { SetStatus("No changes to apply.", "TextSecondary"); return; }

        AdvCtrlApplyBtn.IsEnabled = false;
        if (AdvCtrlApplyBtnBar != null) AdvCtrlApplyBtnBar.IsEnabled = false;
        SetStatus("Applying...", "Accent");

        Task.Run(() =>
        {
            try
            {
                CameraMod.InstallWithModSet(_gameDir, modSet,
                    msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent")));
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Applied {count} Advanced Control changes to game.", "Success");
                    AdvCtrlApplyBtn.IsEnabled = true;
                    if (AdvCtrlApplyBtnBar != null) AdvCtrlApplyBtnBar.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Apply failed: {ex.Message}", "Error");
                    AdvCtrlApplyBtn.IsEnabled = true;
                    if (AdvCtrlApplyBtnBar != null) AdvCtrlApplyBtnBar.IsEnabled = true;
                });
            }
        });
    }

    private void OnAdvCtrlLoadFromSimple(object sender, RoutedEventArgs e)
    {
        if (_advCtrlSliders.Count == 0) return;
        try
        {
            var modSet = BuildCurrentSimpleModSet();
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
            string modifiedXml = CameraMod.ApplyModifications(vanillaXml, modSet);
            var rows = CameraMod.ParseXmlToRows(modifiedXml);
            var lookup = new Dictionary<string, string>();
            foreach (var r in rows) lookup[r.FullKey] = r.Value;

            _suppressEvents = true;
            foreach (var (key, slider) in _advCtrlSliders)
            {
                if (slider == null) continue;
                if (lookup.TryGetValue(key, out string? val) &&
                    double.TryParse(val, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double d))
                    slider.Value = Math.Clamp(d, slider.Minimum, slider.Maximum);
            }
            _suppressEvents = false;
            AdvCtrlUpdateChangedLabel();
            SetStatus("Loaded values from Simple mode.", "Success");
        }
        catch (Exception ex)
        {
            _suppressEvents = false;
            SetStatus($"Load failed: {ex.Message}", "Error");
        }
    }

    private void OnAdvCtrlResetVanilla(object sender, RoutedEventArgs e)
    {
        if (_advCtrlSliders.Count == 0) return;
        _suppressEvents = true;
        foreach (var (key, slider) in _advCtrlSliders)
        {
            if (slider == null) continue;
            if (_advCtrlVanilla.TryGetValue(key, out string? vs) &&
                double.TryParse(vs, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d))
                slider.Value = Math.Clamp(d, slider.Minimum, slider.Maximum);
        }
        _suppressEvents = false;
        AdvCtrlUpdateChangedLabel();
        SetStatus("Reset all Advanced Controls to vanilla.", "Success");
    }

    // ── Advanced Controls presets ────────────────────────────────────

    private void AdvCtrlRefreshPresetCombo()
    {
        var presets = Directory.GetFiles(AdvCtrlPresetsDir, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n).ToList();
        AdvCtrlPresetCombo.Items.Clear();
        AdvCtrlPresetCombo.Items.Add("(current)");
        foreach (var p in presets) AdvCtrlPresetCombo.Items.Add(p);
        AdvCtrlPresetCombo.SelectedIndex = 0;
    }

    private void OnAdvCtrlPresetSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_advCtrlSliders.Count == 0 || AdvCtrlPresetCombo.SelectedIndex <= 0) return;
        string name = AdvCtrlPresetCombo.SelectedItem?.ToString() ?? "";
        try
        {
            string json = File.ReadAllText(Path.Combine(AdvCtrlPresetsDir, $"{name}.json"));
            var data = JsonSerializer.Deserialize<Dictionary<string, double>>(json);
            if (data == null) return;
            _suppressEvents = true;
            foreach (var (key, val) in data)
                if (_advCtrlSliders.TryGetValue(key, out var sl) && sl != null)
                    sl.Value = Math.Clamp(val, sl.Minimum, sl.Maximum);
            _suppressEvents = false;
            AdvCtrlUpdateChangedLabel();
            SetStatus($"Loaded preset '{name}'.", "Success");
        }
        catch (Exception ex) { SetStatus($"Load failed: {ex.Message}", "Error"); }
    }

    private void OnAdvCtrlSavePreset(object sender, RoutedEventArgs e)
    {
        var dlg = new InputDialog("Save Advanced Preset", "Enter a name for this preset:") { Owner = this };
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ResponseText)) return;
        string name = dlg.ResponseText.Trim().Replace(" ", "_");
        if (name.Length > 40) name = name[..40];

        try
        {
            var data = new Dictionary<string, double>();
            foreach (var (key, slider) in _advCtrlSliders)
            {
                if (slider == null) continue;
                _advCtrlVanilla.TryGetValue(key, out string? vs);
                double vv = double.TryParse(vs, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : double.NaN;
                if (Math.Abs(slider.Value - vv) > 0.001)
                    data[key] = slider.Value;
            }
            if (data.Count == 0) { SetStatus("No changes to save.", "TextSecondary"); return; }
            File.WriteAllText(Path.Combine(AdvCtrlPresetsDir, $"{name}.json"),
                JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            AdvCtrlRefreshPresetCombo();
            SetStatus($"Preset '{name}' saved ({data.Count} overrides).", "Success");
        }
        catch (Exception ex) { SetStatus($"Save failed: {ex.Message}", "Error"); }
    }

    private void OnAdvCtrlDeletePreset(object sender, RoutedEventArgs e)
    {
        if (AdvCtrlPresetCombo.SelectedIndex <= 0) { SetStatus("Select a saved preset first.", "TextSecondary"); return; }
        string name = AdvCtrlPresetCombo.SelectedItem?.ToString() ?? "";
        try
        {
            string path = Path.Combine(AdvCtrlPresetsDir, $"{name}.json");
            if (File.Exists(path)) File.Delete(path);
            AdvCtrlRefreshPresetCombo();
            SetStatus($"Preset '{name}' deleted.", "Success");
        }
        catch (Exception ex) { SetStatus($"Delete failed: {ex.Message}", "Error"); }
    }

    // ── JSON Mod Manager ─────────────────────────────────────────────

    private void OnJsonGenerate(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir)) { SetStatus("Game folder not set.", "Warn"); return; }
        // Capture UI values on UI thread before going async
        var modSet = BuildCurrentSimpleModSet();
        var info = BuildJsonModInfo();
        var gameDir = _gameDir;
        RunJsonGenerate(() => JsonModExporter.ExportFromModSet(gameDir, info, modSet,
            msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent"))));
    }

    private void OnJsonGenerateFromXml(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir)) { SetStatus("Game folder not set.", "Warn"); return; }

        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select camera XML to patch",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        // Capture UI values on UI thread before going async
        string xmlPath = ofd.FileName;
        var info = BuildJsonModInfo();
        var gameDir = _gameDir;
        RunJsonGenerate(() =>
        {
            string xml = File.ReadAllText(xmlPath);
            return JsonModExporter.ExportFromXml(gameDir, info, xml,
                msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent")));
        });
    }

    private void RunJsonGenerate(Func<(List<JsonModExporter.PatchChange>, string)> work)
    {
        SetStatus("Generating patches...", "Accent");
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
                    SetStatus($"Generated {changes.Count} patch regions. Click Save .json to export.", "Success");
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SetStatus($"Generate failed: {ex.Message}", "Error"));
            }
        });
    }

    private void OnJsonSave(object sender, RoutedEventArgs e)
    {
        if (_jsonLastJson == null) { SetStatus("Generate a patch first.", "TextSecondary"); return; }

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
            SetStatus($"Saved {Path.GetFileName(sfd.FileName)} ({_jsonLastPatches!.Count} patches).", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {ex.Message}", "Error");
        }
    }

    private JsonModExporter.ModInfo BuildJsonModInfo() => new(
        Title: JsonTitleBox.Text.Trim().Length > 0 ? JsonTitleBox.Text.Trim() : "UCM Camera Config",
        Version: JsonVersionBox.Text.Trim().Length > 0 ? JsonVersionBox.Text.Trim() : "1.0",
        Author: JsonAuthorBox.Text.Trim(),
        Description: JsonDescBox.Text.Trim(),
        NexusUrl: JsonNexusBox.Text.Trim().Length > 0 ? JsonNexusBox.Text.Trim() : NexusUrl);

    private ModificationSet BuildCurrentSimpleModSet()
    {
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
        bool sc = SteadycamCheck.IsChecked == true;
        return CameraRules.BuildModifications(styleId, fov, bane, combat,
            mountHeight: mount, customUp: customUp, steadycam: sc);
    }

    private void OnNexusClick(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(NexusUrl) { UseShellExecute = true });
    private void OnGitHubClick(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(GitHubUrl) { UseShellExecute = true });
    private void OnKofiClick(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(KoFiUrl) { UseShellExecute = true });
    private void OnOpenGameFolder(object s, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_gameDir) && Directory.Exists(_gameDir))
            Process.Start(new ProcessStartInfo(_gameDir) { UseShellExecute = true });
    }

    private static int GetInt(Dictionary<string, object>? dict, string key, int def = 0)
    {
        if (dict == null || !dict.TryGetValue(key, out var val)) return def;
        if (val is JsonElement je) return je.TryGetInt32(out int i) ? i : def;
        return int.TryParse(val.ToString(), out int r) ? r : def;
    }

    private static bool GetBool(Dictionary<string, object>? dict, string key, bool defaultVal = false)
    {
        if (dict == null || !dict.TryGetValue(key, out var val)) return defaultVal;
        if (val is JsonElement je) return je.ValueKind == JsonValueKind.True;
        return bool.TryParse(val.ToString(), out bool b) && b;
    }
}
