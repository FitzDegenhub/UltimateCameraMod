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
using UltimateCameraMod.V3.Localization;
using UltimateCameraMod.V3.Models;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.V3;

public partial class MainWindow : Window
{
    private const string Ver = "3.2";

    private static string L(string key) => TranslationSource.Instance[key];

    private static string LanguagePath => Path.Combine(ExeDir, "ucm_language.json");

    private static readonly Dictionary<string, string> LangNativeNames = new()
    {
        ["en"] = "English",
        ["ko"] = "\ud55c\uad6d\uc5b4",
        ["ja"] = "\u65e5\u672c\u8a9e",
        ["zh-CN"] = "\u4e2d\u6587(\u7b80\u4f53)",
        ["zh-Hant"] = "\u4e2d\u6587(\u7e41\u9ad4)",
        ["th"] = "\u0e44\u0e17\u0e22",
        ["id"] = "Indonesia",
        ["tr"] = "T\u00fcrk\u00e7e",
        ["pl"] = "Polski",
        ["it"] = "Italiano",
        ["sv"] = "Svenska",
        ["nb"] = "Norsk",
        ["da"] = "Dansk",
        ["fi"] = "Suomi",
        ["de"] = "Deutsch",
        ["fr"] = "Fran\u00e7ais",
        ["es"] = "Espa\u00f1ol",
        ["pt-BR"] = "Portugu\u00eas",
        ["ru"] = "\u0420\u0443\u0441\u0441\u043a\u0438\u0439",
    };

    /// <summary>UCM Quick horizontal shift help when Centered camera is off (keep in sync with HShiftTip default in XAML).</summary>
    private static string HShiftTipUnlocked => L("Help_HShiftTipUnlocked");
    private const string LegacyPresetsDirName = "presets";
    private const string UcmPresetsDirName = "ucm_presets";
    private const string MyPresetsDirName = "my_presets";
    private const string CommunityPresetsDirName = "community_presets";
    private const string UcmPresetsCatalogUrl = "https://raw.githubusercontent.com/FitzDegenhub/UltimateCameraMod/main/ucm_presets/catalog.json";
    private const string UcmPresetsRawBaseUrl = "https://raw.githubusercontent.com/FitzDegenhub/UltimateCameraMod/main/ucm_presets/";
    private const string ImportPresetsDirName = "import_presets";
    private const string LegacyImportedPresetsDirName = "imported_presets";
    private const string NexusUrl = "https://www.nexusmods.com/crimsondesert/mods/438";
    private const string GitHubUrl = "https://github.com/FitzDegenhub/UltimateCameraMod";
    private const string KoFiUrl = "https://ko-fi.com/0xfitz";

    private static readonly JsonSerializerOptions PresetFileJsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string ExeDir =
        Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

    private string _gameDir = "";
    private string _detectedPlatform = "Unknown";

    /// <summary>Reuse stripped vanilla XML while game folder unchanged (imported preset rebuild is hot).</summary>
    private string _cachedVanillaXmlGameDir = "";
    private string? _cachedStrippedVanillaXml;

    /// <summary>Reuse PAZ fingerprint for MarkImportedPresetAsBuilt + status text while game folder unchanged.</summary>
    private string _cachedGameFingerprintDir = "";
    private ImportedPresetFingerprint? _cachedGameFingerprint;
    // _activeTab field removed — always "custom" in the unified preset system
    private bool _suppressEvents;
    private Dictionary<string, object>? _savedState;
    private string? _sessionXml;
    private DispatcherTimer? _saveToastDelayTimer;
    private DispatcherTimer? _saveToastHideTimer;
    private DispatcherTimer? _installStateDebounceTimer;
    private string? _lastWrittenInstallStateJson;
    private bool _previewSyncPosted;
    private DispatcherTimer? _previewDebounceTimer;
    private string? _presetCatalogFingerprint;
    private string _pendingSaveToastText = "Saved";
    private bool _pendingSaveToastIsError;
    private double _customDraftDistance = 5.0;
    private double _customDraftHeight;
    private double _customDraftRightOffset;
    private string? _customDraftPresetName;
    private bool _customDraftDirty = true;
    private bool _gameUpdateNoticeSessionDismissed;
    private bool _gameUpdateAutoBackupDispatched;
    private string? _gameUpdatePostRefreshNote;
    private bool _taskbarIconContentRenderedDone;
    private bool _taskbarIconActivatedDone;
    private bool _shellTaskbarPropertyStoreApplied;

    // â"€â"€ Mode state â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€
    private string _activeMode = "simple";
    private bool _isExpertMode;
    private bool _advCtrlNeedsRefresh;
    private bool _expertNeedsRefresh;
    private bool _sessionIsFullPreset;
    private bool _sessionIsRawImport;
    private string _selectedStyleId = "panoramic";
    private List<AdvancedRow> _advAllRows = new();
    private bool _sacredToastShown;

    // â"€â"€ JSON Mod Manager state â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€
    private List<JsonModExporter.PatchChange>? _jsonLastPatches;
    private string? _jsonLastJson;
    private ImportedPreset? _selectedImportedPreset;
    private PresetManagerItem? _selectedPresetManagerItem;
    private string _loadedPresetName = "Unsaved session";
    private string _loadedPresetKindLabel = "Current Session";
    private string _loadedPresetSourceLabel = "Live session";
    private string _loadedPresetStatusText = "No preset loaded yet.";
    private string _loadedPresetSummaryText = "Start from UCM Quick, load something from Preset Manager, or import a creator preset.";
    private string? _loadedPresetUrl;
    private readonly ObservableCollection<PresetManagerItem> _presetManagerItems = new();

    private bool _suppressPresetPickerActivation;
    private string? _activePickerKey;
    private ObservableCollection<AdvancedRow> _advFilteredRows = new();

    /// <summary>God Mode: section group names (DataGrid group headers) the user left expanded — kept across preset switches.</summary>
    private readonly HashSet<string> _godModeExpandedSections = new(StringComparer.OrdinalIgnoreCase);
    private bool _godModeExpandSyncSuppress;
    private readonly ConditionalWeakTable<System.Windows.Controls.Expander, object> _godModeExpanderHooked = new();
    private DispatcherTimer? _godModeExpandHookDebounceTimer;

    private static string SacredOverridesDir
    {
        get { string d = Path.Combine(ExeDir, "sacred_overrides"); Directory.CreateDirectory(d); return d; }
    }

    /// <summary>Returns the sacred overrides file path for the currently active preset, or the legacy global path as fallback.</summary>
    private string AdvOverridesPath
    {
        get
        {
            var item = FindPresetItemByKey(_activePickerKey);
            if (item != null && !string.IsNullOrEmpty(item.FilePath))
            {
                string name = Path.GetFileNameWithoutExtension(item.FilePath);
                return Path.Combine(SacredOverridesDir, name + ".json");
            }
            return Path.Combine(ExeDir, "advanced_overrides.json");
        }
    }
    private static string AdvPresetsDir
    {
        get { string d = Path.Combine(ExeDir, "advanced_presets"); Directory.CreateDirectory(d); return d; }
    }
    private static string ImportedPresetsDir
    {
        get
        {
            string d = Path.Combine(ExeDir, ImportPresetsDirName);
            Directory.CreateDirectory(d);
            return d;
        }
    }

    private static string UcmPresetsDir
    {
        get { string d = Path.Combine(ExeDir, UcmPresetsDirName); Directory.CreateDirectory(d); return d; }
    }

    private static string MyPresetsDir
    {
        get { string d = Path.Combine(ExeDir, MyPresetsDirName); Directory.CreateDirectory(d); return d; }
    }

    private static string CommunityPresetsDir
    {
        get { string d = Path.Combine(ExeDir, CommunityPresetsDirName); Directory.CreateDirectory(d); return d; }
    }

    private static string UcmCatalogStatePath => Path.Combine(UcmPresetsDir, ".catalog_state.json");

    private static Dictionary<string, string> ReadCatalogState()
    {
        try
        {
            if (File.Exists(UcmCatalogStatePath))
            {
                string json = File.ReadAllText(UcmCatalogStatePath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
        }
        catch { }
        return new();
    }

    private static void UpdateCatalogStateEntry(string filename, string sha256)
    {
        var state = ReadCatalogState();
        state[filename] = sha256;
        WriteCatalogState(state);
    }

    private static void WriteCatalogState(Dictionary<string, string> state)
    {
        Directory.CreateDirectory(UcmPresetsDir);
        File.WriteAllText(UcmCatalogStatePath, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static bool _legacyPresetFoldersMigrated;
    private static bool _importPresetsFolderMigrated;
    // â"€â"€ Style/FoV/Combat data â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private static readonly (string Id, string Label)[] Styles =
    {
        ("heroic",    "Heroic  -  Shoulder-level OTS, great framing"),
        ("panoramic", "Panoramic  -  Head-height wide pullback, filmic"),
        ("default",   "Vanilla  -  Unmodified game camera (use Steadycam for UCM smoothing)"),
        ("close-up",  "Close-Up  -  Shoulder OTS, tighter (16:9 feel)"),
        ("low-rider", "Low Rider  -  Hip-level, full body + horizon"),
        ("knee-cam",  "Knee Cam  -  Knee-height dramatic low angle"),
        ("dirt-cam",  "Dirt Cam  -  Ground-level, extreme low"),
        ("survival",  "Survival  -  Tight horror-game OTS (16:9 feel)"),
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


    private static readonly Dictionary<string, (double Dist, double Up, double Ro)> StyleParams = new()
    {
        ["heroic"] = (5.0, -0.2, 0.0),
        ["panoramic"] = (7.5, 0.0, 0.0),
        ["default"] = (3.4, 0.0, 0.0),
        ["close-up"] = (4.0, -0.2, 0.0),
        ["low-rider"] = (5.0, -0.8, 0.0),
        ["knee-cam"] = (5.0, -1.2, 0.0),
        ["dirt-cam"] = (5.0, -1.5, 0.0),
        ["survival"] = (3.0, 0.0, 0.7),
    };

    // â"€â"€ Constructor â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private static string WindowStatePath => Path.Combine(ExeDir, "window_state.json");

    private void LoadSavedLanguage()
    {
        try
        {
            if (!File.Exists(LanguagePath)) return;
            string json = File.ReadAllText(LanguagePath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("culture", out var cEl) && cEl.ValueKind == JsonValueKind.String)
            {
                string code = cEl.GetString() ?? "en";
                TranslationSource.Instance.CurrentCulture = new System.Globalization.CultureInfo(code);
            }
        }
        catch { }
    }

    private void InitLanguageSelector()
    {
        if (LanguageCombo == null) return;
        string currentCode = TranslationSource.Instance.CurrentCulture.Name;
        if (string.IsNullOrEmpty(currentCode)) currentCode = "en";

        LanguageCombo.Items.Clear();
        int selectedIdx = 0;
        int idx = 0;
        foreach (var lang in TranslationSource.AvailableLanguages)
        {
            string code = lang.Name;
            string native = LangNativeNames.TryGetValue(code, out string? n) ? n : code;
            LanguageCombo.Items.Add(new ComboBoxItem { Content = native, Tag = code });
            if (string.Equals(code, currentCode, StringComparison.OrdinalIgnoreCase))
                selectedIdx = idx;
            idx++;
        }
        LanguageCombo.SelectedIndex = selectedIdx;
        LanguageCombo.SelectionChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageCombo.SelectedItem is not ComboBoxItem item || item.Tag is not string code) return;
        TranslationSource.Instance.CurrentCulture = new System.Globalization.CultureInfo(code);
        try
        {
            string json = JsonSerializer.Serialize(new { culture = code });
            File.WriteAllText(LanguagePath, json);
        }
        catch { }
        _presetCatalogFingerprint = null;
        RefreshPresetManagerLists(preserveSelection: true);

        // Re-set status bar text for the current mode
        SetStatus(_activeMode switch
        {
            "advanced" => L("Status_FineTuneMode"),
            "expert" => L("Status_GodModeMode"),
            _ => L("Status_UcmQuickMode")
        }, "TextDim");
    }

    public MainWindow()
    {
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        LoadSavedLanguage();
        InitializeComponent();
        InitLanguageSelector();

        // WPF applies Window.Icon after HWND creation and can overwrite WM_SETICON; re-apply on later dispatcher phases.
        SourceInitialized += (_, _) =>
        {
            IntPtr shellHwnd = new WindowInteropHelper(this).Handle;
            if (shellHwnd != IntPtr.Zero && !_shellTaskbarPropertyStoreApplied)
            {
                // Taskbar group icon is driven by window property store (not WM_SETICON alone). See ShellTaskbarPropertyStore.
                _shellTaskbarPropertyStoreApplied = ShellTaskbarPropertyStore.TryApply(shellHwnd, ExeDir);
            }

            Dispatcher.BeginInvoke(ApplyNativeWindowIcons, DispatcherPriority.Loaded);
            Dispatcher.BeginInvoke(ApplyNativeWindowIcons, DispatcherPriority.Render);
            Dispatcher.BeginInvoke(ApplyNativeWindowIcons, DispatcherPriority.ApplicationIdle);
        };

        ContentRendered += OnMainWindowContentRendered;
        Activated += OnMainWindowFirstActivated;
        IsVisibleChanged += OnMainWindowIsVisibleChanged;

        Loaded += OnLoaded;
        Closing += OnWindowClosing;
        InitializeSaveToast();
        InitializeInstallStateDebounce();
        InitializePreviewDebounce();
        InitializeAdvCtrlSearchDebounce();
        CacheResourceLookups();
        RestoreWindowSize();
    }
    private void CacheResourceLookups()
    {
        _accentButtonStyle = (Style)FindResource("AccentButton");
        _subtleButtonStyle = (Style)FindResource("SubtleButton");
        _textSecondaryBrush = (Brush)FindResource("TextSecondaryBrush");
        _accentBrush = (Brush)FindResource("AccentBrush");
        _textDimBrush = (Brush)FindResource("TextDimBrush");
    }
    private void InitializeAdvCtrlSearchDebounce()
    {
        _advCtrlSearchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _advCtrlSearchDebounceTimer.Tick += (_, _) =>
        {
            _advCtrlSearchDebounceTimer!.Stop();
            ApplyAdvCtrlSearch();
        };
    }
    private void InitializeInstallStateDebounce()
    {
        _installStateDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _installStateDebounceTimer.Tick += (_, _) =>
        {
            _installStateDebounceTimer!.Stop();
            FlushInstallStateWriteIfNeeded();
        };
    }
    private void ScheduleInstallStateWrite()
    {
        if (!IsLoaded || _suppressEvents || _installStateDebounceTimer == null)
            return;
        _installStateDebounceTimer.Stop();
        _installStateDebounceTimer.Start();
    }
    private void FlushInstallStateWriteIfNeeded()
    {
        if (!IsLoaded || _suppressEvents)
            return;

        CaptureCustomDraft(markDirty: false, updateSelector: false);

        if (!string.IsNullOrEmpty(_gameDir))
        {
            try
            {
                CaptureSessionXml();
            }
            catch
            {
                // Keep previous _sessionXml if capture fails mid-write
            }
        }

        var state = new Dictionary<string, object>
        {
            ["ucm_version"] = Ver,
            ["comp_size"] = 0,
            ["style"] = GetSelectedStyleId(),
            ["fov"] = GetSelectedFov(),
            ["bane"] = BaneCheck.IsChecked == true,
            ["combat_pullback"] = GetCombatPullback(),
            ["mount_height"] = MountHeightCheck.IsChecked == true,
            ["steadycam"] = SteadycamCheck.IsChecked == true,
            ["custom"] = new Dictionary<string, double>
            {
                ["distance"] = Math.Round(_customDraftDistance, 2),
                ["height"] = Math.Round(_customDraftHeight, 2),
                ["right_offset"] = Math.Round(_customDraftRightOffset, 2)
            },
        };
        if (!_customDraftDirty && !string.IsNullOrWhiteSpace(_customDraftPresetName))
            state["custom_preset"] = _customDraftPresetName!;

        if (!string.IsNullOrWhiteSpace(_sessionXml))
            state["session_xml"] = _sessionXml;

        string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        if (string.Equals(json, _lastWrittenInstallStateJson, StringComparison.Ordinal))
        {
            AutosaveActivePresetFileIfNeeded();
            return;
        }

        _lastWrittenInstallStateJson = json;
        var path = StatePath;
        Task.Run(() => { try { File.WriteAllText(path, json); } catch { } });
        AutosaveActivePresetFileIfNeeded();
    }
    private void InitializeSaveToast()
    {
        _saveToastDelayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(220) };
        _saveToastHideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };

        _saveToastDelayTimer.Tick += (_, _) =>
        {
            _saveToastDelayTimer!.Stop();
            SaveToastText.Text = _pendingSaveToastText;
            var color = _pendingSaveToastIsError ? "#EF4444" : "#4CAF50";
            SaveToastDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            SaveToast.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"#3A{color.TrimStart('#')}"));
            SaveToast.Visibility = Visibility.Visible;
            _saveToastHideTimer!.Stop();
            _saveToastHideTimer.Start();
        };

        _saveToastHideTimer.Tick += (_, _) =>
        {
            _saveToastHideTimer!.Stop();
            SaveToast.Visibility = Visibility.Collapsed;
        };
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
            if (_shellTaskbarPropertyStoreApplied)
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                ShellTaskbarPropertyStore.TryClear(hwnd);
            }

            _installStateDebounceTimer?.Stop();
            _previewDebounceTimer?.Stop();
            FlushInstallStateWriteIfNeeded();

            // Sacred overrides are per-preset .sacred.json files -- no save-on-close needed

            string json = JsonSerializer.Serialize(new { ucm_version = Ver, width = Width, height = Height, game_dir = _gameDir, platform = _detectedPlatform });
            File.WriteAllText(WindowStatePath, json);
        }
        catch { }
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ApplyDarkTitlebar();
            ApplyNativeWindowIcons();

            if (!_shellTaskbarPropertyStoreApplied)
            {
                IntPtr h = new WindowInteropHelper(this).Handle;
                if (h != IntPtr.Zero)
                    _shellTaskbarPropertyStoreApplied = ShellTaskbarPropertyStore.TryApply(h, ExeDir);
            }

            _savedState = LoadInstallState();
            // Deploy shipped community presets before the first list refresh so their
            // files exist on disk when ActivatePickerFromSelection runs on startup.
            DeployShippedCommunityPresets();
            PopulateControls();
            RefreshPresetManagerLists(preserveSelection: false);
            string savedStyle = _savedState?.GetValueOrDefault("style")?.ToString() ?? "";
            // _activeTab assignment removed (always custom)

            // Try to restore saved game dir first (instant), fall back to detection (slow)
            string? savedGameDir = null;
            string? savedPlatform = null;
            try
            {
                if (File.Exists(WindowStatePath))
                {
                    var wsDoc = JsonDocument.Parse(File.ReadAllText(WindowStatePath));
                    if (wsDoc.RootElement.TryGetProperty("game_dir", out var gdEl))
                        savedGameDir = gdEl.GetString();
                    if (wsDoc.RootElement.TryGetProperty("platform", out var plEl))
                        savedPlatform = plEl.GetString();
                }
            }
            catch { }

            if (!string.IsNullOrEmpty(savedGameDir) && Directory.Exists(savedGameDir)
                && File.Exists(Path.Combine(savedGameDir, "0010", "0.paz")))
            {
                _gameDir = savedGameDir;
                _detectedPlatform = savedPlatform ?? "Steam";
            }
            else
            {
                var (detectedPath, platform) = GameDetector.FindGameDir();
                _gameDir = detectedPath ?? "";
                _detectedPlatform = platform;
            }

            // Snapshot fresh-install state BEFORE anything creates files.
            // Property getters (UcmPresetsDir, MyPresetsDir) auto-create directories,
            // GenerateBuiltInPresets → EnsureBackup creates backups/original_backup.bin,
            // so we must capture this before any of that runs.
            string tutorialDonePath = System.IO.Path.Combine(ExeDir, "tutorial_done.flag");
            bool isTutorialDone = System.IO.File.Exists(tutorialDonePath);
            bool hasExistingData = System.IO.File.Exists(System.IO.Path.Combine(ExeDir, "window_state.json"))
                || System.IO.File.Exists(System.IO.Path.Combine(ExeDir, "backups", "original_backup.bin"));

            if (string.IsNullOrEmpty(_gameDir))
            {
                GamePathLabel.Text = L("GamePath_NotSet");
                SetStatus(L("Status_GameFolderNotSetBrowse"), "Warn");
            }
            else
            {
                OnGameDirResolved();
                // Migrate legacy .json presets to .ucmpreset before generating built-ins
                MigrateJsonToUcmPreset(UcmPresetsDir);
                MigrateJsonToUcmPreset(MyPresetsDir);
                MigrateJsonToUcmPreset(CommunityPresetsDir);
                // Don't migrate import_presets — different schema
                GenerateBuiltInPresets();
                // If the selected preset was skipped on startup (file didn't exist yet),
                // retry activating it now that GenerateBuiltInPresets has written the files.
                if (_sessionXml == null && _selectedPresetManagerItem != null)
                {
                    _activePickerKey = null;
                    ActivatePickerFromSelection(_selectedPresetManagerItem, skipCapture: true);
                }
                // Do not overwrite XML already loaded from the selected preset (last_install runs after list refresh).
                if (string.IsNullOrWhiteSpace(_sessionXml))
                    TryRestoreLastInstallSessionAfterGameDirResolved();
            }

            SwitchEditorTab("simple");
            ApplyPresetEditingLockUi();

            ExpertDataGrid.LayoutUpdated += ExpertDataGrid_OnLayoutUpdated;

            ScheduleTaskbarIconDelayedRetries();

            // Read saved version from window_state.json for version-change detection
            string savedVersion = "";
            try
            {
                if (System.IO.File.Exists(WindowStatePath))
                {
                    var ws = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                        System.IO.File.ReadAllText(WindowStatePath));
                    if (ws != null && ws.TryGetValue("ucm_version", out var vObj) && vObj is System.Text.Json.JsonElement je
                        && je.ValueKind == System.Text.Json.JsonValueKind.String)
                        savedVersion = je.GetString() ?? "";
                }
            }
            catch { }

            if (!isTutorialDone && !string.IsNullOrEmpty(_gameDir))
            {
                if (hasExistingData)
                {
                    // Upgrading from pre-v3.0.2 (no tutorial_done.flag) -- skip welcome, mark tutorial done
                    try { System.IO.File.WriteAllText(tutorialDonePath, "done"); } catch { }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _ = ShowAlertOverlayAsync(string.Format(L("Dlg_UpdatedTitle"), Ver),
                            L("Dlg_UpdatedBody"));
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                else
                {
                    // Truly fresh install -- show welcome screen
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ShowWelcomeVerifyScreen();
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
            else if (isTutorialDone && !string.IsNullOrEmpty(_gameDir) && savedVersion != Ver)
            {
                // Existing user upgrading between versions (e.g. v3.0.2 -> v3.1)
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _ = ShowAlertOverlayAsync(string.Format(L("Dlg_UpdatedTitle"), Ver),
                        string.IsNullOrEmpty(savedVersion)
                            ? L("Dlg_UpdatedBody")
                            : string.Format(L("Dlg_UpdatedBodyFrom"), savedVersion));
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            // Users can click Browse to download UCM presets when ready
        }
        catch (Exception ex)
        {
            _ = ShowAlertOverlayAsync(L("Title_Error"), $"{ex.Message}", isError: true);
        }
    }
    private void ShowWelcomeVerifyScreen()
    {
        double w = ActualWidth;
        double h = ActualHeight;
        if (w < 1) w = 1400;
        if (h < 1) h = 900;

        var overlay = new Canvas { Width = w, Height = h };

        // Dark backdrop (same opacity as tutorial)
        var backdrop = new System.Windows.Shapes.Rectangle
        {
            Width = w, Height = h,
            Fill = new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0)),
            IsHitTestVisible = true
        };
        overlay.Children.Add(backdrop);

        // Title
        var titleBlock = new TextBlock
        {
            Text = L("Dlg_WelcomeTitle"),
            FontSize = 20, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xA2, 0x4E)),
            Margin = new Thickness(0, 0, 0, 14)
        };

        // Description
        var descBlock = new TextBlock
        {
            Text = L("Dlg_WelcomeBody"),
            FontSize = 13, TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            LineHeight = 20,
            Margin = new Thickness(0, 0, 0, 20)
        };

        // Yes button (gold, matching tutorial Next button)
        var yesBtn = new Button
        {
            Content = L("Btn_YesContinue"),
            Width = 130, Height = 36, FontSize = 13, FontWeight = FontWeights.SemiBold,
            Background = new SolidColorBrush(Color.FromRgb(0xC8, 0xA2, 0x4E)),
            Foreground = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A)),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand, Padding = new Thickness(14, 4, 14, 4)
        };

        // No button (subtle, matching tutorial Skip button)
        var noBtn = new Button
        {
            Content = L("Btn_NoCloseUcm"),
            Width = 120, Height = 36, FontSize = 12,
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand, Margin = new Thickness(12, 0, 0, 0)
        };

        yesBtn.Click += (_, _) =>
        {
            TutorialCanvas.Children.Clear();
            TutorialCanvas.Visibility = Visibility.Collapsed;

            // Try to generate presets now — if tainted backup, show the fix overlay instead of tutorial
            try
            {
                if (!string.IsNullOrEmpty(_gameDir))
                    GenerateBuiltInPresets();
                RefreshPresetManagerLists(preserveSelection: true);
                StartTutorial();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not vanilla") || ex.Message.Contains("modified"))
                    _ = HandleTaintedBackupAsync();
                else
                    StartTutorial(); // Non-tainted errors — still show tutorial
            }
        };

        noBtn.Click += (_, _) =>
        {
            Application.Current.Shutdown();
        };

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };
        buttonPanel.Children.Add(yesBtn);
        buttonPanel.Children.Add(noBtn);

        var stack = new StackPanel();
        stack.Children.Add(titleBlock);
        stack.Children.Add(descBlock);
        stack.Children.Add(buttonPanel);

        // Card (matching tutorial card style)
        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x24, 0x24, 0x24)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0xA2, 0x4E)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(30, 24, 30, 24),
            Width = 480,
            Child = stack,
            IsHitTestVisible = true
        };

        Canvas.SetLeft(card, (w - 480) / 2);
        Canvas.SetTop(card, (h - 300) / 2);
        overlay.Children.Add(card);

        TutorialCanvas.Children.Clear();
        TutorialCanvas.Children.Add(overlay);
        TutorialCanvas.Visibility = Visibility.Visible;

        // Handle resize
        SizeChanged += WelcomeResizeHandler;
        void WelcomeResizeHandler(object sender, SizeChangedEventArgs args)
        {
            overlay.Width = args.NewSize.Width;
            overlay.Height = args.NewSize.Height;
            backdrop.Width = args.NewSize.Width;
            backdrop.Height = args.NewSize.Height;
            Canvas.SetLeft(card, (args.NewSize.Width - 480) / 2);
            Canvas.SetTop(card, (args.NewSize.Height - 300) / 2);
        }
    }

    private void StartTutorial()
    {
        var steps = new List<TutorialOverlay.TutorialStep>
        {
            new(L("Tutorial_Step1Title"),
                L("Tutorial_Step1Desc"),
                () => PresetRailList),

            new(L("Tutorial_Step2Title"),
                L("Tutorial_Step2Desc"),
                () => EditorTabBar),

            new(L("Tutorial_Step3Title"),
                L("Tutorial_Step3Desc"),
                () => SettingsPanel),

            new(L("Tutorial_Step4Title"),
                L("Tutorial_Step4Desc"),
                () => PreviewsPanel),

            new(L("Tutorial_Step5Title"),
                L("Tutorial_Step5Desc"),
                () => SidebarActionButtons),

            new(L("Tutorial_Step6Title"),
                L("Tutorial_Step6Desc"),
                () => KofiBtn)
        };

        var overlay = new TutorialOverlay(steps, () =>
        {
            TutorialCanvas.Children.Clear();
            TutorialCanvas.Visibility = Visibility.Collapsed;
            // Mark tutorial as done
            try { System.IO.File.WriteAllText(System.IO.Path.Combine(ExeDir, "tutorial_done.flag"), "done"); } catch { }

            // Users can click Browse to download UCM presets when ready
        }, this);

        TutorialCanvas.Children.Clear();
        TutorialCanvas.Children.Add(overlay);
        TutorialCanvas.Visibility = Visibility.Visible;

        // Set canvas size to match window
        overlay.Width = ActualWidth;
        overlay.Height = ActualHeight;
        SizeChanged += (_, args) =>
        {
            overlay.Width = args.NewSize.Width;
            overlay.Height = args.NewSize.Height;
        };

        overlay.Start();
    }

    private string BrowseForGameDir()
    {
        try
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = L("Dlg_BrowseGameFolder"),
                UseDescriptionForTitle = true
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folder = dlg.SelectedPath;
                if (File.Exists(Path.Combine(folder, "0010", "0.paz")))
                    return folder;

                _ = ShowAlertOverlayAsync(L("Msg_WrongFolderTitle"), L("Msg_WrongFolder"));
            }
        }
        catch (Exception ex)
        {
            _ = ShowAlertOverlayAsync(L("Msg_ErrorTitle"), string.Format(L("Msg_FolderDialogError"), ex.Message), isError: true);
        }
        return "";
    }
    private void ClearImportLoadPerformanceCaches()
    {
        _cachedVanillaXmlGameDir = "";
        _cachedStrippedVanillaXml = null;
        _cachedGameFingerprintDir = "";
        _cachedGameFingerprint = null;
    }
    private string GetStrippedVanillaXmlForCurrentGame()
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
            throw new InvalidOperationException("Game folder not set.");

        if (_cachedStrippedVanillaXml != null
            && string.Equals(_cachedVanillaXmlGameDir, _gameDir, StringComparison.OrdinalIgnoreCase))
            return _cachedStrippedVanillaXml;

        string xml = CameraMod.ReadVanillaXml(_gameDir);
        _cachedVanillaXmlGameDir = _gameDir;
        _cachedStrippedVanillaXml = xml;
        return xml;
    }
    private void OnGameDirResolved()
    {
        ClearImportLoadPerformanceCaches();

        string backupsDir = Path.Combine(ExeDir, "backups");
        CameraMod.BackupsDirOverride = () => backupsDir;
        HudMod.BackupsDirOverride = () => backupsDir;
        CameraMod.AppVersion = Ver;

        CleanStaleData(backupsDir);
        _savedState = LoadInstallState();

        string pt = _gameDir;
        if (pt.Length > 55) pt = "..." + pt[^52..];

        string platformTag = _detectedPlatform != "Unknown" ? $" [{_detectedPlatform}]" : "";
        GamePathLabel.Text = string.Format(L("GamePath_Display"), pt, platformTag);

        if (_detectedPlatform == "Xbox/GamePass" && !GameDetector.CheckWritePermission(_gameDir))
        {
            SetStatus(L("Status_XboxReadOnly"), "Warn");
            _ = ShowAlertOverlayAsync(L("Msg_XboxReadOnlyTitle"), L("Msg_XboxReadOnly"));
        }

        CheckForUpdate();
        CheckGitHubVersion();
        RefreshGameUpdateNotice();

        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SyncPreview();
            _activePickerKey = null;
            RefreshPresetManagerLists();
            UpdateLoadedPresetContextUi();
            // Check for preset updates AFTER the list is populated
            CheckUcmPresetUpdatesAsync();
            CheckCommunityPresetUpdatesAsync();
        }
        else
        {
            string captured = _gameDir;
            _ = RunGameDirScanAndRefreshAsync(captured);
        }
    }
    private async Task RunGameDirScanAndRefreshAsync(string gameDir)
    {
        SetGlobalBusy(true, "Reading game camera data...");
        try
        {
            (string fp, ImportedPresetFingerprint? gameFp) bundle;
            try
            {
                bundle = await Task.Run(() =>
                {
                    ImportedPresetFingerprint? gfp = TryGetGameFingerprintForDir(gameDir);
                    string fp = BuildCatalogFingerprintCore(gfp, gameDir);
                    return (fp, gfp);
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                    SetStatus(string.Format(L("Status_CouldNotReadGameFiles"), ex.Message), "Error"));
                return;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                if (gameDir != _gameDir)
                    return;
                RefreshPresetManagerLists(preserveSelection: true, precomputedCatalog: bundle);
                SyncPreview();
                _activePickerKey = null;
                UpdateLoadedPresetContextUi();
                RefreshGameUpdateNotice();
                // Check for preset updates AFTER the list is populated
                CheckUcmPresetUpdatesAsync();
                CheckCommunityPresetUpdatesAsync();
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.InvokeAsync(() =>
                SetStatus(string.Format(L("Status_GameFolderScanFailed"), ex.Message), "Error"));
        }
        finally
        {
            SetGlobalBusy(false);
        }
    }
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
    private void PopulateControls()
    {
        _suppressEvents = true;
        try
        {
            string savedStyle = _savedState?.GetValueOrDefault("style")?.ToString() ?? "panoramic";
            bool isKnownStyle = string.Equals(savedStyle, "custom", StringComparison.OrdinalIgnoreCase)
                || Array.Exists(Styles, s => s.Id == savedStyle);
            _selectedStyleId = isKnownStyle ? savedStyle : "panoramic";

            FovCombo.Items.Clear();
            foreach (var (_, label) in FovOptions)
                FovCombo.Items.Add(label);

            int savedFov = GetInt(_savedState, "fov", 25);
            int fovIdx = Array.FindIndex(FovOptions, f => f.Value == savedFov);
            FovCombo.SelectedIndex = fovIdx >= 0 ? fovIdx : 4;

            double savedPullback = 0.0;
            if (_savedState?.TryGetValue("combat_pullback", out var cpObj) == true && cpObj != null)
                double.TryParse(cpObj.ToString(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out savedPullback);
            CombatPullbackSlider.Value = Math.Clamp(savedPullback, -0.6, 0.6);
            CombatPullbackLabel.Text = FormatPullback(CombatPullbackSlider.Value);

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
                    _customDraftPresetName = savedPreset;
            }
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

            // Only wipe backups on major version change (e.g. v2 → v3), not patch versions (3.0.1 → 3.0.2)
            string savedMajor = savedVer.Split('.')[0];
            string currentMajor = Ver.Split('.')[0];
            if (savedMajor != currentMajor)
            {
                if (Directory.Exists(backupsDir))
                    Directory.Delete(backupsDir, true);
                if (File.Exists(StatePath))
                    File.Delete(StatePath);
                GameInstallBaselineTracker.Delete(ExeDir);
                SetStatus(string.Format(L("Status_CleanedOldData"), savedVer), "Warn");
            }
        }
        catch { }
    }
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
    private static string? GetStringFromSavedState(Dictionary<string, object>? state, string key)
    {
        if (state == null || !state.TryGetValue(key, out object? v) || v == null)
            return null;
        if (v is JsonElement je)
            return je.ValueKind == JsonValueKind.String ? je.GetString() : null;
        return v.ToString();
    }
    // â"€â"€ Helpers â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private void SetStatus(string text, string brushKey)
    {
        StatusLabel.Text = text;
        StatusLabel.Foreground = brushKey switch
        {
            "Success" => (Brush)FindResource("SuccessBrush"),
            "Error" => (Brush)FindResource("ErrorBrush"),
            "Accent" => _accentBrush,
            "Warn" => (Brush)FindResource("WarnBrush"),
            "TextSecondary" => _textSecondaryBrush,
            _ => _textDimBrush,
        };
    }
    private void SetButtons(bool enabled)
    {
        _ = enabled;
    }
    private void QueueSavedToast(string? text = null, bool isError = false)
    {
        if (!IsLoaded || _suppressEvents || _saveToastDelayTimer == null || _saveToastHideTimer == null)
            return;

        _pendingSaveToastText = text ?? L("Label_Saved");
        _pendingSaveToastIsError = isError;
        _saveToastHideTimer.Stop();
        _saveToastDelayTimer.Stop();
        _saveToastDelayTimer.Start();
    }
    private void SetGlobalBusy(bool visible, string? message = null)
    {
        void Apply()
        {
            if (GlobalBusyOverlay == null || GlobalBusyText == null)
                return;
            if (!visible)
            {
                GlobalBusyOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            if (!string.IsNullOrEmpty(message))
                GlobalBusyText.Text = message;
            GlobalBusyOverlay.Visibility = Visibility.Visible;
        }

        if (Dispatcher.CheckAccess())
            Apply();
        else
            Dispatcher.Invoke(Apply);
    }
    /// <summary>Queues a disk write (500ms debounce). Use immediate=true after explicit preset/tab actions.</summary>
    private void SaveCurrentUiState(bool immediate = false)
    {
        if (!IsLoaded || _suppressEvents)
            return;

        if (immediate)
        {
            _installStateDebounceTimer?.Stop();
            FlushInstallStateWriteIfNeeded();
        }
        else
        {
            ScheduleInstallStateWrite();
        }
    }
    private void InitializePreviewDebounce()
    {
        _previewDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _previewDebounceTimer.Tick += (_, _) =>
        {
            _previewDebounceTimer!.Stop();
            SyncPreview();
        };
    }
    private void OnNexusClick(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(NexusUrl) { UseShellExecute = true });
    private void OnGitHubClick(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(GitHubUrl) { UseShellExecute = true });
    private void OnKofiClick(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(KoFiUrl) { UseShellExecute = true });
    private void OnOpenGameFolder(object s, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_gameDir) && Directory.Exists(_gameDir))
        {
            Process.Start(new ProcessStartInfo(_gameDir) { UseShellExecute = true });
            return;
        }

        // Game not detected — let the user browse for it manually
        string picked = BrowseForGameDir();
        if (string.IsNullOrEmpty(picked)) return;

        _gameDir = picked;
        _detectedPlatform = "Manual";
        OnGameDirResolved();
        MigrateJsonToUcmPreset(UcmPresetsDir);
        MigrateJsonToUcmPreset(MyPresetsDir);
        GenerateBuiltInPresets();
        RefreshPresetManagerLists(preserveSelection: false);
        SetStatus(L("Status_Ready"), "OK");
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
    /// <summary>Fast extraction of a JSON string field from a partial file header (avoids parsing session_xml).</summary>
    private static string? ExtractJsonStringField(string json, string fieldName)
    {
        string pattern = $"\"{fieldName}\"";
        int idx = json.IndexOf(pattern, StringComparison.Ordinal);
        if (idx < 0) return null;
        int colon = json.IndexOf(':', idx + pattern.Length);
        if (colon < 0) return null;
        int i = colon + 1;
        while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
        if (i >= json.Length || json[i] != '"') return null;
        int startQuote = i;
        i++;
        while (i < json.Length)
        {
            char c = json[i];
            if (c == '\\' && i + 1 < json.Length)
            {
                char e = json[i + 1];
                if (e == 'u' && i + 6 <= json.Length && Is4HexDigits(json, i + 2))
                {
                    i += 6;
                    continue;
                }
                i += 2;
                continue;
            }
            if (c == '"')
            {
                string token = json[startQuote..(i + 1)];
                try
                {
                    string? s = JsonSerializer.Deserialize<string>(token);
                    return s?.Replace("\n", " ", StringComparison.Ordinal);
                }
                catch
                {
                    return null;
                }
            }
            i++;
        }
        return null;
    }
    private static bool Is4HexDigits(string json, int start)
    {
        if (start + 4 > json.Length) return false;
        for (int k = 0; k < 4; k++)
        {
            if (!Uri.IsHexDigit(json[start + k])) return false;
        }
        return true;
    }
    private static bool? ExtractJsonBoolField(string json, string fieldName)
    {
        string pattern = $"\"{fieldName}\"";
        int idx = json.IndexOf(pattern, StringComparison.Ordinal);
        if (idx < 0) return null;
        int colon = json.IndexOf(':', idx + pattern.Length);
        if (colon < 0) return null;
        int i = colon + 1;
        while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
        if (i >= json.Length) return null;
        ReadOnlySpan<char> rest = json.AsSpan(i);
        if (rest.StartsWith("true", StringComparison.Ordinal)) return true;
        if (rest.StartsWith("false", StringComparison.Ordinal)) return false;
        return null;
    }
    private static string SanitizeFileStem(string text)
    {
        string safe = new string(text
            .Trim()
            .Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "imported_preset" : safe;
    }
}
