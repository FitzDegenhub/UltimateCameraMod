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
    private const string Ver = "3.0";

    /// <summary>UCM Quick horizontal shift help when Centered camera is off (keep in sync with HShiftTip default in XAML).</summary>
    private const string HShiftTipUnlocked =
        "Left/right framing as a delta on RightOffset (on foot, mounts, aim, and related rows). 0 matches vanilla side bias. Increase toward ~0.5 to pull toward geometric center in the file; negative values bias further left. Matches the top-down FoV preview. Centered camera below is separate: it locks this slider to 0 and applies stronger centering.";
    private const string LegacyPresetsDirName = "presets";
    private const string UcmPresetsDirName = "ucm_presets";
    private const string MyPresetsDirName = "my_presets";
    private const string CommunityPresetsDirName = "community_presets";
    private const string ImportPresetsDirName = "import_presets";
    private const string LegacyImportedPresetsDirName = "imported_presets";
    private const string NexusUrl = "https://www.nexusmods.com/crimsondesert/mods/438";
    private const string GitHubUrl = "https://github.com/FitzDegenhub/UltimateCameraMod";
    private const string KoFiUrl = "https://ko-fi.com/0xfitz";

    private static readonly JsonSerializerOptions PresetFileJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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

    // â”€â”€ Mode state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private string _activeMode = "simple";
    private bool _isExpertMode;
    private bool _advCtrlNeedsRefresh;
    private bool _expertNeedsRefresh;
    private bool _sessionIsFullPreset;
    private string _selectedStyleId = "cinematic";
    private List<AdvancedRow> _advAllRows = new();

    // â”€â”€ JSON Mod Manager state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    private static string AdvOverridesPath => Path.Combine(ExeDir, "advanced_overrides.json");
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

    private static bool _legacyPresetFoldersMigrated;
    private static bool _importPresetsFolderMigrated;

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

    // â”€â”€ Style/FoV/Combat data â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static readonly (string Id, string Label)[] Styles =
    {
        ("western",   "Heroic  -  Shoulder-level OTS, great framing"),
        ("cinematic", "Panoramic  -  Head-height wide pullback, filmic"),
        ("default",   "Vanilla  -  Unmodified game camera (use Steadycam for UCM smoothing)"),
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

    // â”€â”€ Constructor â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static string WindowStatePath => Path.Combine(ExeDir, "window_state.json");

    public MainWindow()
    {
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        InitializeComponent();

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

        var preset = new Dictionary<string, object>
        {
            ["name"] = item.Name,
            ["author"] = item.SourceLabel,
            ["description"] = item.StatusText,
            ["kind"] = item.KindId,
            ["locked"] = item.IsLocked,
            ["style_id"] = GetSelectedStyleId(),
            ["session_xml"] = _sessionXml,
            ["settings"] = BuildCurrentPresetSettingsPayload()
        };

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

    private void InitializePreviewDebounce()
    {
        _previewDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _previewDebounceTimer.Tick += (_, _) =>
        {
            _previewDebounceTimer!.Stop();
            SyncPreview();
        };
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
            ApplyNativeWindowIcons();

            if (!_shellTaskbarPropertyStoreApplied)
            {
                IntPtr h = new WindowInteropHelper(this).Handle;
                if (h != IntPtr.Zero)
                    _shellTaskbarPropertyStoreApplied = ShellTaskbarPropertyStore.TryApply(h, ExeDir);
            }

            _savedState = LoadInstallState();
            PopulateControls();
            RefreshPresetManagerLists(preserveSelection: false);
            string savedStyle = _savedState?.GetValueOrDefault("style")?.ToString() ?? "";
            // _activeTab assignment removed (always custom)

            var (detectedPath, platform) = GameDetector.FindGameDir();
            _gameDir = detectedPath ?? "";
            _detectedPlatform = platform;

            if (string.IsNullOrEmpty(_gameDir))
            {
                GamePathLabel.Text = "Game folder:  not detected (optional for browsing presets)";
                SetStatus("Game not detected. Browse presets and edit settings — set game folder to enable JSON export.", "Warn");
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
                TryRestoreLastInstallSessionAfterGameDirResolved();
            }

            SwitchEditorTab("simple");
            ApplyPresetEditingLockUi();

            ExpertDataGrid.LayoutUpdated += ExpertDataGrid_OnLayoutUpdated;

            ScheduleTaskbarIconDelayedRetries();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup error:\n{ex}", "Ultimate Camera Mod", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExpertDataGrid_OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (!_isExpertMode || ExpertDataGrid.ItemsSource == null)
            return;

        _godModeExpandHookDebounceTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(75) };
        _godModeExpandHookDebounceTimer.Stop();
        _godModeExpandHookDebounceTimer.Tick -= OnGodModeExpandHookDebounceTick;
        _godModeExpandHookDebounceTimer.Tick += OnGodModeExpandHookDebounceTick;
        _godModeExpandHookDebounceTimer.Start();
    }

    private void OnGodModeExpandHookDebounceTick(object? sender, EventArgs e)
    {
        _godModeExpandHookDebounceTimer?.Stop();
        HookGodModeGroupExpanders();
    }

    private void ScheduleGodModeExpandHook()
    {
        if (!_isExpertMode)
            return;
        Dispatcher.BeginInvoke(new Action(HookGodModeGroupExpanders), DispatcherPriority.Loaded);
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null)
            yield break;
        int count = VisualTreeHelper.GetChildrenCount(depObj);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
            if (child is T typed)
                yield return typed;
            foreach (T nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private void HookGodModeGroupExpanders()
    {
        if (!_isExpertMode || ExpertDataGrid == null || ExpertDataGrid.ItemsSource == null)
            return;

        _godModeExpandSyncSuppress = true;
        try
        {
            foreach (var expander in FindVisualChildren<System.Windows.Controls.Expander>(ExpertDataGrid))
            {
                if (expander.DataContext is not CollectionViewGroup g)
                    continue;
                string name = g.Name?.ToString() ?? "";
                if (string.IsNullOrEmpty(name))
                    continue;

                if (!_godModeExpanderHooked.TryGetValue(expander, out _))
                {
                    _godModeExpanderHooked.Add(expander, null);
                    expander.Expanded += GodModeSectionExpander_OnExpanded;
                    expander.Collapsed += GodModeSectionExpander_OnCollapsed;
                }

                expander.IsExpanded = _godModeExpandedSections.Contains(name);
            }
        }
        finally
        {
            _godModeExpandSyncSuppress = false;
        }
    }

    private void GodModeSectionExpander_OnExpanded(object sender, RoutedEventArgs e)
    {
        if (_godModeExpandSyncSuppress)
            return;
        if (sender is System.Windows.Controls.Expander exp && exp.DataContext is CollectionViewGroup gv)
        {
            string name = gv.Name?.ToString() ?? "";
            if (name.Length > 0)
                _godModeExpandedSections.Add(name);
        }
    }

    private void GodModeSectionExpander_OnCollapsed(object sender, RoutedEventArgs e)
    {
        if (_godModeExpandSyncSuppress)
            return;
        if (sender is System.Windows.Controls.Expander exp && exp.DataContext is CollectionViewGroup gv)
        {
            string name = gv.Name?.ToString() ?? "";
            if (name.Length > 0)
                _godModeExpandedSections.Remove(name);
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

    /// <summary>Bump when built-in Vanilla.json must be rewritten (true game baseline, not UCM-tuned).</summary>
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

    /// <summary>Rename .json preset files to .ucmpreset during startup migration (skip if .ucmpreset already exists).</summary>
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

    private void GenerateBuiltInPresets()
    {
        if (string.IsNullOrEmpty(_gameDir)) return;
        try
        {
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
            foreach (var (id, label) in Styles)
            {
                string styleName = label.Split("  -  ")[0].Trim();
                string fileName = styleName + ".ucmpreset";
                string path = Path.Combine(UcmPresetsDir, fileName);
                if (id == "default")
                {
                    if (!VanillaBuiltInPresetNeedsRefresh(path)) continue;
                }
                else if (File.Exists(path))
                {
                    continue;
                }

                var (dist, up, ro) = StyleParams[id];
                string xml;
                Dictionary<string, object> settings;

                if (string.Equals(id, "default", StringComparison.OrdinalIgnoreCase))
                {
                    // True vanilla: no BuildModifications — raw game XML from backup/live PAZ.
                    xml = vanillaXml;
                    // Quick sliders use BuildCustom delta; JSON must store delta, not literal XML RightOffset (~0.5).
                    double rightOffsetSetting;
                    if (!CameraMod.TryParseUcmQuickFootBaselineFromXml(vanillaXml, out dist, out up, out double roAbs))
                    {
                        (dist, up, ro) = StyleParams[id];
                        rightOffsetSetting = ro;
                    }
                    else
                        rightOffsetSetting = CameraRules.QuickShiftDeltaFromFootZl2RightOffset(roAbs);

                    settings = new Dictionary<string, object>
                    {
                        ["distance"] = Math.Round(dist, 2),
                        ["height"] = Math.Round(up, 2),
                        ["right_offset"] = Math.Round(rightOffsetSetting, 2),
                        ["fov"] = 0,
                        ["combat_pullback"] = 0.0,
                        ["centered"] = false,
                        ["mount_height"] = false,
                        ["steadycam"] = false
                    };
                }
                else
                {
                    var modSet = CameraRules.BuildModifications(id, 25, false,
                        combatPullback: 0.0, mountHeight: false, steadycam: true);
                    xml = CameraMod.ApplyModifications(vanillaXml, modSet);
                    settings = new Dictionary<string, object>
                    {
                        ["distance"] = Math.Round(dist, 2),
                        ["height"] = Math.Round(up, 2),
                        ["right_offset"] = Math.Round(ro, 2),
                        ["fov"] = 25,
                        ["combat_pullback"] = 0.0,
                        ["centered"] = false,
                        ["mount_height"] = false,
                        ["steadycam"] = true
                    };
                }

                string desc = label.Contains("  -  ") ? label.Split("  -  ")[1].Trim() : "";
                var preset = new Dictionary<string, object>
                {
                    ["name"] = styleName,
                    ["author"] = "0xFitz",
                    ["description"] = desc,
                    ["kind"] = id == "default" ? "default" : "style",
                    ["locked"] = true,
                    ["style_id"] = id,
                    ["session_xml"] = xml,
                    ["settings"] = settings
                };
                if (string.Equals(id, "default", StringComparison.OrdinalIgnoreCase))
                    preset["vanilla_preset_rev"] = VanillaBuiltinPresetRevision;

                File.WriteAllText(path, JsonSerializer.Serialize(preset, PresetFileJsonOptions));
            }
            // Deploy shipped community presets from embedded resources
            DeployShippedCommunityPresets();
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to generate built-in presets: {ex.Message}", "Warn");
        }
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

                var preset = new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["author"] = author,
                    ["description"] = desc.Replace("\n", " ").Trim().Length > 200
                        ? desc.Replace("\n", " ").Trim()[..197] + "..."
                        : desc.Replace("\n", " ").Trim(),
                    ["kind"] = "style",
                    ["locked"] = true,
                    ["session_xml"] = sessionXml,
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
                    }
                };

                if (!string.IsNullOrWhiteSpace(url))
                    preset["url"] = url;

                File.WriteAllText(destPath, JsonSerializer.Serialize(preset, PresetFileJsonOptions));
            }
            catch { /* Skip malformed shipped presets */ }
        }
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
        RefreshGameUpdateNotice();

        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SyncPreview();
            _activePickerKey = null;
            RefreshPresetManagerLists();
            UpdateLoadedPresetContextUi();
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
                    SetStatus($"Could not read game camera files: {ex.Message}", "Error"));
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
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.InvokeAsync(() =>
                SetStatus($"Game folder scan failed: {ex.Message}", "Error"));
        }
        finally
        {
            SetGlobalBusy(false);
        }
    }

    // â”€â”€ Dark titlebar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [DllImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute", PreserveSig = true)]
    private static extern int SetWindowThemeAttribute(IntPtr hwnd, int attr, ref int value, int size);

    private const int WM_SETICON = 0x0080;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;
    private const int ICON_SMALL2 = 2;
    private const uint IMAGE_ICON = 1;
    private const uint LR_DEFAULTCOLOR = 0;
    private const uint LR_LOADFROMFILE = 0x00000010;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr LoadImage(IntPtr hInst, IntPtr lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    private const int GCLP_HICON = -14;
    private const int GCLP_HICONSM = -34;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static readonly object TaskbarIconLock = new();
    private static IntPtr _hIcon16;
    private static IntPtr _hIcon24;
    private static IntPtr _hIconBig;

    private static IEnumerable<string> EnumerateUcmIcoFilePaths()
    {
        string assets = Path.Combine(ExeDir, "Assets", "ucm.ico");
        string root = Path.Combine(ExeDir, "ucm.ico");
        if (File.Exists(assets)) yield return assets;
        if (File.Exists(root) && !string.Equals(assets, root, StringComparison.OrdinalIgnoreCase))
            yield return root;
    }

    /// <summary>Load icon handles once per process; fill any missing size (PE + file). Do not bail after partial success — taskbar needs BIG.</summary>
    private static void EnsureTaskbarIconAssets()
    {
        lock (TaskbarIconLock)
        {
            if (_hIcon16 != IntPtr.Zero && _hIcon24 != IntPtr.Zero && _hIconBig != IntPtr.Zero)
                return;

            IntPtr mod = GetModuleHandle(null);
            if (_hIcon16 == IntPtr.Zero)
                _hIcon16 = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 16, 16, LR_DEFAULTCOLOR);
            if (_hIcon24 == IntPtr.Zero)
                _hIcon24 = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 24, 24, LR_DEFAULTCOLOR);
            if (_hIconBig == IntPtr.Zero)
                _hIconBig = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 32, 32, LR_DEFAULTCOLOR);
            if (_hIconBig == IntPtr.Zero)
                _hIconBig = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 48, 48, LR_DEFAULTCOLOR);
            if (_hIconBig == IntPtr.Zero)
                _hIconBig = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 256, 256, LR_DEFAULTCOLOR);

            foreach (string icoPath in EnumerateUcmIcoFilePaths())
            {
                if (_hIcon16 == IntPtr.Zero)
                    _hIcon16 = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 16, 16, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIcon24 == IntPtr.Zero)
                    _hIcon24 = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 24, 24, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIconBig == IntPtr.Zero)
                    _hIconBig = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 32, 32, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIconBig == IntPtr.Zero)
                    _hIconBig = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 48, 48, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIconBig == IntPtr.Zero)
                    _hIconBig = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 256, 256, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIcon16 != IntPtr.Zero && _hIcon24 != IntPtr.Zero && _hIconBig != IntPtr.Zero)
                    break;
            }

            if (_hIcon24 == IntPtr.Zero && _hIcon16 != IntPtr.Zero)
                _hIcon24 = _hIcon16;
            if (_hIconBig == IntPtr.Zero && _hIcon16 != IntPtr.Zero)
                _hIconBig = _hIcon16;
        }
    }

    private void OnMainWindowContentRendered(object? sender, EventArgs e)
    {
        if (_taskbarIconContentRenderedDone) return;
        _taskbarIconContentRenderedDone = true;
        ApplyNativeWindowIcons();
    }

    private void OnMainWindowFirstActivated(object? sender, EventArgs e)
    {
        if (_taskbarIconActivatedDone) return;
        _taskbarIconActivatedDone = true;
        ApplyNativeWindowIcons();
        Activated -= OnMainWindowFirstActivated;
    }

    private void OnMainWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool vis && vis && IsLoaded)
            ApplyNativeWindowIcons();
    }

    private void ScheduleTaskbarIconDelayedRetries()
    {
        void Kick(int ms)
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            t.Tick += (_, _) =>
            {
                t.Stop();
                ApplyNativeWindowIcons();
            };
            t.Start();
        }

        Kick(120);
        Kick(400);
        Kick(1200);
        Kick(2500);
        Kick(4500);
    }

    /// <summary>
    /// Windows taskbar often uses Win32 window icons, not only WPF <see cref="Window.Icon"/>.
    /// WPF can reset HWND icons after <see cref="SourceInitializedEventArgs"/>; call again from <see cref="OnLoaded"/> and deferred priorities.
    /// </summary>
    private void ApplyNativeWindowIcons()
    {
        try
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;

            EnsureTaskbarIconAssets();

            IntPtr mid = _hIcon24 != IntPtr.Zero ? _hIcon24 : _hIcon16;
            if (_hIcon16 != IntPtr.Zero)
                SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, _hIcon16);
            if (mid != IntPtr.Zero)
                SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL2, mid);
            if (_hIconBig != IntPtr.Zero)
                SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_BIG, _hIconBig);

            // Some shell paths read class icons on cold start; mirror WM_SETICON here.
            if (_hIconBig != IntPtr.Zero)
                SetClassLongPtr(hwnd, GCLP_HICON, _hIconBig);
            if (_hIcon16 != IntPtr.Zero)
                SetClassLongPtr(hwnd, GCLP_HICONSM, _hIcon16);
        }
        catch
        {
            // Non-fatal: title bar pack URI icon still applies where supported.
        }
    }

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

    // â”€â”€ Populate controls â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void PopulateControls()
    {
        _suppressEvents = true;
        try
        {
            string savedStyle = _savedState?.GetValueOrDefault("style")?.ToString() ?? "cinematic";
            bool isKnownStyle = string.Equals(savedStyle, "custom", StringComparison.OrdinalIgnoreCase)
                || Array.Exists(Styles, s => s.Id == savedStyle);
            _selectedStyleId = isKnownStyle ? savedStyle : "cinematic";

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
            CombatPullbackSlider.Value = Math.Clamp(savedPullback, -0.4, 0.6);
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

    // â”€â”€ Stale data cleanup â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
            GameInstallBaselineTracker.Delete(ExeDir);

            SetStatus($"Cleaned old v{(string.IsNullOrEmpty(savedVer) ? "?" : savedVer)} data. v3 now exports JSON only.", "Warn");
        }
        catch { }
    }

    // â”€â”€ State persistence â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    private static void TryClearAdvOverridesFile()
    {
        try
        {
            if (File.Exists(AdvOverridesPath))
                File.Delete(AdvOverridesPath);
        }
        catch { }
    }

    /// <summary>
    /// Restores <see cref="_sessionXml"/> from <c>last_install.json</c> after the game folder is known
    /// so Fine Tune / God Mode / export match the last closed session.
    /// </summary>
    private void TryRestoreLastInstallSessionAfterGameDirResolved()
    {
        string? xml = GetStringFromSavedState(_savedState, "session_xml");
        if (string.IsNullOrWhiteSpace(xml))
            return;

        try
        {
            TryClearAdvOverridesFile();
            _sessionXml = xml;
            _advCtrlNeedsRefresh = true;
            _expertNeedsRefresh = true;
            // Don't override Quick sliders — preset loader sets them from settings block.
        }
        catch { }
    }

    /// <summary>
    /// Aligns UCM Quick distance / height / shift with on-foot ZL2 framing in the session snapshot.
    /// </summary>
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

    private static string PresetManagerKey(PresetManagerItem item) => $"{item.KindId}:{item.Name}";

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

        // Custom Offsets panel: greyed for UCM presets (hard-locked by design) and padlocked user presets
        bool offsetsLocked = isUcmPreset || quickLocked;
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
        QuickGlobalPanel.Opacity = quickLocked ? 0.45 : 1.0;
        QuickGlobalPanel.IsHitTestVisible = !quickLocked;

        if (!quickLocked)
            ApplyCenteredLock();

        // Fine Tune sliders: grey out for UCM presets and hard-locked presets
        double deepOpacity = deepLocked ? 0.38 : 1.0;
        foreach (var slider in _advCtrlAllSliders)
            if (slider != null) { slider.IsEnabled = !deepLocked; slider.Opacity = 1.0; }
        ExpertDataGrid.IsReadOnly = deepLocked;
        ExpertDataGrid.Opacity = deepOpacity;

        // Tab buttons: dim Fine Tune + God Mode for UCM presets to signal read-only
        TabFineTune.Opacity = isUcmPreset ? 0.5 : 1.0;
        TabGodMode.Opacity  = isUcmPreset ? 0.5 : 1.0;

        if (!deepLocked)
            ApplySteadycamSliderLock();
    }

    /// <summary>
    /// Disables fine-tune sliders for parameters that Steadycam controls when Steadycam is active,
    /// so manual adjustments can't silently conflict with Steadycam's applied values.
    /// </summary>
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
                    var preset = LoadImportedPreset(item.Name) ?? throw new InvalidOperationException("Imported preset could not be loaded.");
                    item.IsLocked = preset.Locked;
                    _selectedImportedPreset = preset;
                    LoadImportedPresetIntoSession(preset);
                    break;
                }
                case "style":
                case "user":
                default:
                {
                    if (string.IsNullOrEmpty(item.FilePath) || !File.Exists(item.FilePath))
                    {
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

                    // Restore UCM Quick settings from the settings block
                    if (root.TryGetProperty("settings", out var settings))
                    {
                        _suppressEvents = true;
                        try
                        {
                            if (root.TryGetProperty("style_id", out var sidEl) && sidEl.ValueKind == JsonValueKind.String)
                                _selectedStyleId = sidEl.GetString() ?? "cinematic";

                            if (settings.TryGetProperty("distance", out var dv2) && dv2.ValueKind == JsonValueKind.Number)
                                DistSlider.Value = Math.Clamp(dv2.GetDouble(), 1.5, 12.0);
                            if (settings.TryGetProperty("height", out var hv2) && hv2.ValueKind == JsonValueKind.Number)
                                HeightSlider.Value = Math.Clamp(hv2.GetDouble(), -1.6, 0.5);
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
                                CombatPullbackSlider.Value = Math.Clamp(cpEl.GetDouble(), -0.4, 0.6);
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
                            _suppressEvents = false;
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
                        SyncPreview();
                    }

                    SetLoadedPresetContext(item.Name, item.KindLabel, item.SourceLabel,
                        item.StatusText, item.SummaryText, item.Url);
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

    private string BuildImportedPresetStatusText(ImportedPreset preset) =>
        BuildImportedPresetStatusTextFromMetadata(preset.LastBuiltAgainst, TryGetCurrentGameFingerprint());

    private static string BuildImportedPresetStatusTextFromMetadata(
        ImportedPresetFingerprint? lastBuilt, ImportedPresetFingerprint? currentFingerprint) =>
        lastBuilt == null
            ? "Imported only - not yet rebuilt for this game."
            : currentFingerprint == null
                ? "Saved preset ready - set a game folder to check rebuild status."
                : FingerprintsMatch(lastBuilt, currentFingerprint)
                    ? "Ready - already rebuilt for this game version."
                    : "Needs rebuild - current game version differs from the last build.";

    private static ImportedPresetFingerprint? ReadLastBuiltAgainstFromJson(JsonElement root)
    {
        if (!root.TryGetProperty("LastBuiltAgainst", out var lb) || lb.ValueKind != JsonValueKind.Object)
            return null;

        static string ReadStr(JsonElement obj, string prop) =>
            obj.TryGetProperty(prop, out var p) && p.ValueKind == JsonValueKind.String
                ? p.GetString() ?? ""
                : "";

        static int ReadInt(JsonElement obj, string prop)
        {
            if (!obj.TryGetProperty(prop, out var p))
                return 0;
            return p.ValueKind switch
            {
                JsonValueKind.Number => p.TryGetInt32(out var i) ? i : 0,
                JsonValueKind.String => int.TryParse(p.GetString(), out var j) ? j : 0,
                _ => 0
            };
        }

        var fp = new ImportedPresetFingerprint
        {
            GameFile = ReadStr(lb, "GameFile"),
            SourceGroup = ReadStr(lb, "SourceGroup"),
            CompSize = ReadInt(lb, "CompSize"),
            OrigSize = ReadInt(lb, "OrigSize"),
            ContentSha256 = ReadStr(lb, "ContentSha256")
        };

        if (string.IsNullOrEmpty(fp.GameFile) && fp.CompSize == 0 && fp.OrigSize == 0
            && string.IsNullOrEmpty(fp.ContentSha256))
            return null;

        return fp;
    }

    private bool TryBuildImportedPresetManagerItem(string filePath, string fileStem,
        ImportedPresetFingerprint? currentGameFp, out PresetManagerItem? item)
    {
        item = null;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
            var root = doc.RootElement;

            string displayName = fileStem;
            if (root.TryGetProperty("Name", out var n) && n.ValueKind == JsonValueKind.String)
            {
                string? s = n.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    displayName = s;
            }

            string sourceType = "xml";
            if (root.TryGetProperty("SourceType", out var st) && st.ValueKind == JsonValueKind.String)
            {
                string? t = st.GetString();
                if (!string.IsNullOrWhiteSpace(t))
                    sourceType = t;
            }

            string sourceDisplayName = "";
            if (root.TryGetProperty("SourceDisplayName", out var sd) && sd.ValueKind == JsonValueKind.String)
                sourceDisplayName = sd.GetString() ?? "";

            int valueCount = 0;
            if (root.TryGetProperty("Values", out var vals) && vals.ValueKind == JsonValueKind.Object)
            {
                foreach (var _ in vals.EnumerateObject())
                    valueCount++;
            }

            string author = "";
            if (root.TryGetProperty("Author", out var au) && au.ValueKind == JsonValueKind.String)
                author = au.GetString() ?? "";

            string description = "";
            if (root.TryGetProperty("Description", out var desc) && desc.ValueKind == JsonValueKind.String)
                description = desc.GetString() ?? "";

            ImportedPresetFingerprint? lastBuilt = ReadLastBuiltAgainstFromJson(root);
            string statusText = BuildImportedPresetStatusTextFromMetadata(lastBuilt, currentGameFp);

            bool locked = false;
            if (root.TryGetProperty("Locked", out var lk))
                locked = lk.ValueKind == JsonValueKind.True;
            else if (root.TryGetProperty("locked", out var lk2))
                locked = lk2.ValueKind == JsonValueKind.True;

            // Build clean summary — just description + URL if available. Author shown separately in banner.
            string summary;
            if (!string.IsNullOrWhiteSpace(description))
            {
                string shortDesc = description.Replace("\n", " ").Trim();
                if (shortDesc.Length > 200) shortDesc = shortDesc[..197] + "...";
                summary = shortDesc;
            }
            else
            {
                summary = $"Imported from {sourceType.ToUpperInvariant()} ({sourceDisplayName})";
            }

            string sidebarSource = string.IsNullOrWhiteSpace(author)
                ? sourceDisplayName
                : author;

            item = new PresetManagerItem
            {
                Name = fileStem,
                KindId = "imported",
                KindLabel = ImportedPresetKindLabel(sourceType),
                SourceLabel = sidebarSource,
                StatusText = statusText,
                SummaryText = summary,
                FilePath = filePath,
                CanRebuild = true,
                IsLocked = locked
            };
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string BuildImportedPresetSummaryText(ImportedPreset preset)
    {
        if (!string.IsNullOrWhiteSpace(preset.Description))
            return preset.Description.Replace("\n", " ").Trim();
        return $"Imported from {preset.SourceType.ToUpperInvariant()} ({preset.SourceDisplayName})";
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

    /// <summary>Get both .ucmpreset and .json files from a directory (for backwards-compatible preset scanning).</summary>
    private static IEnumerable<string> GetPresetFiles(string dir)
    {
        if (!Directory.Exists(dir)) return Enumerable.Empty<string>();
        return Directory.GetFiles(dir, "*.ucmpreset")
            .Concat(Directory.GetFiles(dir, "*.json"))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
    }

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

    // LoadQuickPresetByName, LoadFineTunePresetByName, LoadGodModePresetByName removed —
    // unified file-based loader in ActivatePickerFromSelection handles all preset types.

    private void LoadImportedPresetIntoSession(ImportedPreset preset)
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
            throw new InvalidOperationException("Game folder not set. v3 needs the current game to rebuild this preset safely.");

        // Use the raw XML directly if available, otherwise rebuild from values
        string xml;
        if (!string.IsNullOrWhiteSpace(preset.RawXml))
        {
            // Strip BOM and comments — ParseXmlToRows needs clean game-format XML
            xml = preset.RawXml.TrimStart('\uFEFF');
            xml = CameraMod.StripComments(xml);
        }
        else
        {
            xml = BuildRebuiltXmlFromImportedPreset(preset);
            SetStatus($"Imported preset '{preset.Name}' had no embedded XML; rebuilt from saved settings.", "Warn");
        }
        RefreshUIFromSessionXml(xml);
        // Force Quick sliders to match the imported XML's ZL2 values
        TryApplyQuickSlidersFromSessionXml(xml);
        MarkImportedPresetAsBuilt(preset, refreshPresetSidebar: false);

        string authorOrSource = !string.IsNullOrWhiteSpace(preset.Author)
            ? preset.Author
            : preset.SourceDisplayName;

        SetLoadedPresetContext(preset.Name,
            ImportedPresetKindLabel(preset.SourceType),
            authorOrSource,
            BuildImportedPresetStatusText(preset),
            BuildImportedPresetSummaryText(preset),
            preset.Url);
    }

    private static string ImportedPresetPath(string name) =>
        Path.Combine(ImportedPresetsDir, $"{SanitizeFileStem(name)}.json");

    private static string ImportedPresetKindLabel(string sourceType)
    {
        if (sourceType.Equals("paz", StringComparison.OrdinalIgnoreCase))
            return "Imported 0.paz";
        if (sourceType.Equals("mod", StringComparison.OrdinalIgnoreCase))
            return "Mod package";
        return "Imported XML";
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

    private void RefreshImportedPresetCombo()
    {
        RefreshPresetManagerLists();
    }

    private ImportedPreset? LoadImportedPreset(string name)
    {
        string path = ImportedPresetPath(name);
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        var preset = JsonSerializer.Deserialize<ImportedPreset>(json);
        if (preset == null)
            return null;

        preset.Name = string.IsNullOrWhiteSpace(preset.Name) ? name : preset.Name;
        preset.Values ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        return preset;
    }

    private void SaveImportedPreset(ImportedPreset preset)
    {
        preset.Name = string.IsNullOrWhiteSpace(preset.Name) ? "imported_preset" : preset.Name.Trim();
        string path = ImportedPresetPath(preset.Name);
        File.WriteAllText(path, JsonSerializer.Serialize(preset, PresetFileJsonOptions));
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

    private string PromptForImportedPresetName(string suggestedName)
    {
        var dlg = new InputDialog("Save Imported Preset", "Enter a name for this imported preset:")
        {
            Owner = this
        };
        dlg.InitialText = SanitizeFileStem(Path.GetFileNameWithoutExtension(suggestedName));
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ResponseText))
            return "";

        string name = SanitizeFileStem(dlg.ResponseText);
        if (name.Length > 60)
            name = name[..60];
        return name;
    }

    private static Dictionary<string, string> BuildImportedValueMap(string xml)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in CameraMod.ParseXmlToRows(xml))
            values[row.FullKey] = row.Value;
        return values;
    }

    private static ImportedPresetFingerprint BuildImportedPresetFingerprint(UltimateCameraMod.Paz.PazEntry entry, byte[] rawBytes)
    {
        return new ImportedPresetFingerprint
        {
            GameFile = entry.Path,
            SourceGroup = Path.GetFileName(Path.GetDirectoryName(entry.PazFile) ?? "0010") ?? "0010",
            CompSize = entry.CompSize,
            OrigSize = entry.OrigSize,
            ContentSha256 = Convert.ToHexString(SHA256.HashData(rawBytes)).ToLowerInvariant()
        };
    }

    private static ImportedPresetFingerprint? TryGetGameFingerprintForDir(string? gameDir)
    {
        if (string.IsNullOrWhiteSpace(gameDir))
            return null;

        var (entry, rawBytes) = CameraMod.ReadCameraEntryWithRawBytes(gameDir);
        return BuildImportedPresetFingerprint(entry, rawBytes);
    }

    private ImportedPresetFingerprint? TryGetCurrentGameFingerprint()
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
            return null;

        if (_cachedGameFingerprint != null
            && string.Equals(_cachedGameFingerprintDir, _gameDir, StringComparison.OrdinalIgnoreCase))
            return _cachedGameFingerprint;

        ImportedPresetFingerprint? fp = TryGetGameFingerprintForDir(_gameDir);
        _cachedGameFingerprintDir = _gameDir;
        _cachedGameFingerprint = fp;
        return fp;
    }

    private static bool FingerprintsMatch(ImportedPresetFingerprint? left, ImportedPresetFingerprint? right)
    {
        return left != null
            && right != null
            && left.GameFile == right.GameFile
            && left.SourceGroup == right.SourceGroup
            && left.CompSize == right.CompSize
            && left.OrigSize == right.OrigSize
            && left.ContentSha256 == right.ContentSha256;
    }

    private ImportedPreset BuildImportedPreset(string name, string sourceType, string sourceDisplayName,
        string? sourcePath, string xml, ImportedPresetFingerprint? importedFingerprint = null,
        string? author = null, string? description = null, string? url = null)
    {
        return new ImportedPreset
        {
            Name = name,
            SourceType = sourceType,
            SourceDisplayName = sourceDisplayName,
            SourcePath = sourcePath,
            Author = author,
            Description = description,
            Url = url,
            ImportedAtUtc = DateTime.UtcNow,
            RawXml = xml,
            ImportedSourceFingerprint = importedFingerprint,
            Values = BuildImportedValueMap(xml)
        };
    }

    private void SelectImportedPreset(string name)
    {
        var item = _presetManagerItems.FirstOrDefault(i =>
            i.KindId == "imported" && string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        if (item != null)
            SetSelectedPresetManagerItem(item, updateDetails: true);
    }

    private string ResolvePamtPathForImportedPaz(string pazPath)
    {
        string siblingPamt = Path.Combine(Path.GetDirectoryName(pazPath) ?? ".", "0.pamt");
        if (File.Exists(siblingPamt))
            return siblingPamt;

        if (!string.IsNullOrWhiteSpace(_gameDir))
        {
            string currentGamePamt = Path.Combine(_gameDir, "0010", "0.pamt");
            if (File.Exists(currentGamePamt))
                return currentGamePamt;
        }

        throw new InvalidOperationException(
            "No matching 0.pamt was found beside the selected 0.paz. Set your game folder so v3 can use the current 0010\\0.pamt as the archive index.");
    }

    private void SaveImportedPresetFromXml(string sourceType, string sourceDisplayName, string? sourcePath,
        string xml, ImportedPresetFingerprint? importedFingerprint = null)
    {
        string name = PromptForImportedPresetName(sourceDisplayName);
        if (string.IsNullOrWhiteSpace(name))
            return;

        string path = ImportedPresetPath(name);
        if (File.Exists(path))
        {
            var overwrite = MessageBox.Show(
                $"Overwrite imported preset '{name}'?",
                "Overwrite Imported Preset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (overwrite != MessageBoxResult.Yes)
                return;
        }

        var preset = BuildImportedPreset(name, sourceType, sourceDisplayName, sourcePath, xml, importedFingerprint);
        SaveImportedPreset(preset);
        RefreshPresetManagerLists();
        SelectImportedPreset(preset.Name);
        QueueSavedToast("Imported preset saved");
        SetStatus($"Imported preset '{preset.Name}' saved with {preset.Values.Count} values.", "Success");
    }

    private ImportedPreset? RequireSelectedImportedPreset()
    {
        if (_selectedPresetManagerItem == null || _selectedPresetManagerItem.KindId != "imported")
        {
            SetStatus("Select an imported preset first.", "TextSecondary");
            return null;
        }

        string name = _selectedPresetManagerItem.Name;
        _selectedImportedPreset = LoadImportedPreset(name);
        if (_selectedImportedPreset == null)
            SetStatus($"Failed to load imported preset '{name}'.", "Error");
        return _selectedImportedPreset;
    }

    private static ModificationSet BuildImportedPresetModSet(ImportedPreset preset)
    {
        var mods = new Dictionary<string, Dictionary<string, (string, string)>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (fullKey, value) in preset.Values)
        {
            if (!CameraSessionState.TryParseFullKey(fullKey, out string modKey, out string attribute))
                continue;

            if (!mods.TryGetValue(modKey, out var attrs))
            {
                attrs = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
                mods[modKey] = attrs;
            }

            attrs[attribute] = ("SET", value);
        }

        return new ModificationSet { ElementMods = mods, FovValue = 0 };
    }

    private string BuildRebuiltXmlFromImportedPreset(ImportedPreset preset)
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
            throw new InvalidOperationException("Game folder not set.");

        string vanillaXml = GetStrippedVanillaXmlForCurrentGame();
        return CameraMod.ApplyModifications(vanillaXml, BuildImportedPresetModSet(preset));
    }

    /// <param name="refreshPresetSidebar">When false, skip rescanning all preset files (faster when loading/rebuilding in place).</param>
    private void MarkImportedPresetAsBuilt(ImportedPreset preset, bool refreshPresetSidebar = true)
    {
        var fingerprint = TryGetCurrentGameFingerprint();
        if (fingerprint == null)
            return;

        preset.LastBuiltAgainst = fingerprint;
        preset.LastBuiltAtUtc = DateTime.UtcNow;
        SaveImportedPreset(preset);
        _selectedImportedPreset = preset;
        if (refreshPresetSidebar)
        {
            RefreshPresetManagerLists();
            SelectImportedPreset(preset.Name);
        }

        UpdateImportedPresetDetails();
    }

    private void UpdateImportedPresetDetails()
    {
        if (_selectedImportedPreset == null)
        {
            UpdatePresetManagerDetails();
            return;
        }

        string status = BuildImportedPresetStatusText(_selectedImportedPreset);
        string summary = BuildImportedPresetSummaryText(_selectedImportedPreset);

        if (_selectedPresetManagerItem != null && _selectedPresetManagerItem.KindId == "imported")
        {
            _selectedPresetManagerItem.StatusText = status;
            _selectedPresetManagerItem.SummaryText = summary;
        }

        UpdatePresetManagerDetails();
    }

    // â”€â”€ Update detection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    private void RefreshGameUpdateNotice()
    {
        try
        {
            if (!IsLoaded)
                return;

            if (string.IsNullOrWhiteSpace(_gameDir))
            {
                if (IsLoaded) GameUpdateStrip.Visibility = Visibility.Collapsed;
                return;
            }

            string backupsDir = Path.Combine(ExeDir, "backups");
            var ev = GameInstallBaselineTracker.Evaluate(ExeDir, Ver, _gameDir, _detectedPlatform, backupsDir);
            if (!ev.ShowWarning)
            {
                _gameUpdateAutoBackupDispatched = false;
                _gameUpdatePostRefreshNote = null;
                if (!_gameUpdateNoticeSessionDismissed)
                    GameUpdateStrip.Visibility = Visibility.Collapsed;
                return;
            }

            // Drift vs last Install baseline: re-snapshot camera from live 0.paz once (even if the strip is dismissed).
            if (!_gameUpdateAutoBackupDispatched)
            {
                _gameUpdateAutoBackupDispatched = true;
                string gameDir = _gameDir;
                Task.Run(() =>
                {
                    try
                    {
                        CameraMod.RefreshVanillaBackupFromLivePaz(gameDir, _ => { });
                        Dispatcher.BeginInvoke(() =>
                        {
                            _gameUpdatePostRefreshNote =
                                "Camera backup was refreshed from your current game files. Use Install when you are ready to re-apply your preset.";
                            RefreshGameUpdateNotice();
                        }, DispatcherPriority.Background);
                    }
                    catch
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            _gameUpdatePostRefreshNote =
                                "Could not refresh camera backup automatically. If the camera is still modded, verify game files in Steam and restart UCM.";
                            RefreshGameUpdateNotice();
                        }, DispatcherPriority.Background);
                    }
                });
            }

            if (_gameUpdateNoticeSessionDismissed)
            {
                GameUpdateStrip.Visibility = Visibility.Collapsed;
                return;
            }

            GameUpdateText.Text = string.IsNullOrEmpty(_gameUpdatePostRefreshNote)
                ? ev.Message
                : ev.Message + "\n\n" + _gameUpdatePostRefreshNote;
            GameUpdateStrip.Visibility = Visibility.Visible;
        }
        catch
        {
            if (IsLoaded) GameUpdateStrip.Visibility = Visibility.Collapsed;
        }
    }

    private void OnGameUpdateDismissClick(object sender, RoutedEventArgs e)
    {
        _gameUpdateNoticeSessionDismissed = true;
        GameUpdateStrip.Visibility = Visibility.Collapsed;
    }

    private void OnGameUpdateSnoozeClick(object sender, RoutedEventArgs e)
    {
        GameInstallBaselineTracker.SetSnooze(ExeDir, TimeSpan.FromDays(7));
        RefreshGameUpdateNotice();
    }

    private void ShowBannerFromState(Dictionary<string, object> state)
    {
        int fov = GetInt(state, "fov", 0);
        string style = state.GetValueOrDefault("style")?.ToString() ?? "default";
        bool bane = GetBool(state, "bane");
        double combatPb = 0.0;
        if (state.TryGetValue("combat_pullback", out var cpbObj) && cpbObj != null)
            double.TryParse(cpbObj.ToString(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out combatPb);
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
        if (combatPb != 0) globals.Add($"Lock-on {(int)Math.Round(combatPb * 100):+0;-0}%");
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

    // â”€â”€ GitHub version check â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    // â”€â”€ Tab switching â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    // SwitchTab removed — always in "custom" mode in unified preset system.

    // â”€â”€ Preview sync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    // â”€â”€ Event handlers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
            HShiftSlider.IsEnabled = true;
            HShiftLabel.Foreground = (Brush)FindResource("TextPrimaryBrush");
            HShiftTip.Text = HShiftTipUnlocked;
        }
    }

    // â”€â”€ Selections â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    // â”€â”€ Presets â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    // â”€â”€ Install â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void OnInstall(object s, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus("Game folder not set.", "Warn");
            return;
        }

        string xml;
        try
        {
            xml = BuildSessionXmlForMode(_activeMode);
            _sessionXml = xml;
        }
        catch (Exception ex)
        {
            SetStatus($"Install failed before write: {ex.Message}", "Error");
            return;
        }

        string gameDir = _gameDir;
        string platform = _detectedPlatform;
        string activeMode = _activeMode;
        string styleId = GetSelectedStyleId();

        SetGlobalBusy(true, "Installing camera to game…");
        SetStatus("Preparing current session XML…", "Accent");
        Task.Run(() =>
        {
            Action<string> log = msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent"));
            static string HashBytes(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

            var (beforeEntry, beforeRaw) = CameraMod.ReadCameraEntryWithRawBytes(gameDir);
            Dictionary<string, object> installResult = CameraMod.InstallRawXml(gameDir, xml, log: log);

            var (afterEntry, afterRaw) = CameraMod.ReadCameraEntryWithRawBytes(gameDir);
            bool payloadChanged = !beforeRaw.AsSpan().SequenceEqual(afterRaw);
            long finalPayloadBytes = afterRaw.Length;
            long finalCompBytes = afterEntry.CompSize;
            string tracePath = Path.Combine(ExeDir, "install_trace.txt");
            File.WriteAllText(tracePath,
                $"time_utc={DateTime.UtcNow:O}\n" +
                $"game_dir={gameDir}\n" +
                $"mode={activeMode}\n" +
                $"style_id={styleId}\n" +
                $"session_xml_sha256={HashBytes(Encoding.UTF8.GetBytes(xml))}\n" +
                $"entry_path={afterEntry.Path}\n" +
                $"paz_file={afterEntry.PazFile}\n" +
                $"offset={afterEntry.Offset}\n" +
                $"comp_size={afterEntry.CompSize}\n" +
                $"before_sha256={HashBytes(beforeRaw)}\n" +
                $"after_sha256={HashBytes(afterRaw)}\n" +
                $"payload_changed={(payloadChanged ? "true" : "false")}\n");

            return (installResult, payloadChanged, tracePath, finalPayloadBytes, finalCompBytes);
        })
            .ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    SetGlobalBusy(false);
                    if (t.IsFaulted)
                    {
                        string message = t.Exception?.GetBaseException().Message ?? "Unknown error";
                        SetStatus($"Install failed: {message}", "Error");
                        return;
                    }

                    QueueSavedToast("Installed");
                    var (installResult, payloadChanged, tracePath, finalPayloadBytes, finalCompBytes) = t.Result;
                    bool ok = installResult.TryGetValue("status", out var statusObj)
                        && string.Equals(statusObj?.ToString(), "ok", StringComparison.OrdinalIgnoreCase);
                    if (!ok)
                    {
                        SetStatus($"Install returned unexpected status. See {Path.GetFileName(tracePath)}.", "Warn");
                        return;
                    }

                    try
                    {
                        GameInstallBaselineTracker.SaveAfterSuccessfulInstall(ExeDir, Ver, gameDir, platform);
                        _gameUpdateNoticeSessionDismissed = false;
                    }
                    catch { }

                    RefreshGameUpdateNotice();

                    SetStatus(
                        payloadChanged
                            ? $"Installed current session to game. Camera payload updated — {finalPayloadBytes:N0} bytes ({finalCompBytes:N0} compressed in PAZ)."
                            : $"Install completed; camera entry in PAZ was unchanged ({finalPayloadBytes:N0} bytes payload, {finalCompBytes:N0} compressed).",
                        payloadChanged ? "Success" : "Warn");
                });
            });
    }

    // â”€â”€ Restore â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void OnBrowseCommunity(object sender, RoutedEventArgs e)
    {
        var dlg = new CommunityBrowserDialog(CommunityPresetsDir, () =>
        {
            Dispatcher.Invoke(() => RefreshPresetManagerLists(preserveSelection: true));
        })
        {
            Owner = this
        };
        dlg.ShowDialog();
    }

    private void OnRestore(object s, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus("Game folder not set.", "Warn");
            return;
        }

        string gameDir = _gameDir;
        SetGlobalBusy(true, "Restoring vanilla camera…");
        SetStatus("Restoring vanilla camera…", "Accent");
        Task.Run(() => CameraMod.RestoreCamera(gameDir,
            log: msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent"))))
            .ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    SetGlobalBusy(false);
                    if (t.IsFaulted)
                    {
                        string message = t.Exception?.GetBaseException().Message ?? "Unknown error";
                        SetStatus($"Restore failed: {message}", "Error");
                        return;
                    }

                    var result = t.Result;
                    string status = result.TryGetValue("status", out var value) ? value?.ToString() ?? "" : "";
                    switch (status)
                    {
                        case "ok":
                            QueueSavedToast("Restored");
                            SetStatus("Restored vanilla camera from backup.", "Success");
                            break;
                        case "no_backup":
                            SetStatus("No backup found. The game camera may already be vanilla.", "Warn");
                            break;
                        case "stale_backup":
                            SetStatus("Backup was stale after a game update and has been cleared. Verify files, then install again.", "Warn");
                            RefreshGameUpdateNotice();
                            break;
                        default:
                            SetStatus("Restore finished with an unknown result.", "Warn");
                            break;
                    }
                });
            });
    }

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    private void QueueSavedToast(string text = "Saved", bool isError = false)
    {
        if (!IsLoaded || _suppressEvents || _saveToastDelayTimer == null || _saveToastHideTimer == null)
            return;

        _pendingSaveToastText = text;
        _pendingSaveToastIsError = isError;
        _saveToastHideTimer.Stop();
        _saveToastDelayTimer.Stop();
        _saveToastDelayTimer.Start();
    }

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

    // â”€â”€ Advanced mode toggle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    // â”€â”€ 4-Mode navigation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void OnTabQuick(object s, RoutedEventArgs e) => SwitchEditorTab("simple");
    private void OnTabFineTune(object s, RoutedEventArgs e) => SwitchEditorTab("advanced");
    private void OnTabGodMode(object s, RoutedEventArgs e) => SwitchEditorTab("expert");

    private void OnExportJson(object sender, RoutedEventArgs e)
    {
        var dlg = new ExportJsonDialog(_gameDir, () =>
        {
            CaptureSessionXml();
            return _sessionXml;
        })
        { Owner = this };
        dlg.ShowDialog();
    }

    private void SwitchEditorTab(string tab, bool captureCurrent = true)
    {
        if (tab != "simple" && tab != "advanced" && tab != "expert")
            return;

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

        // Update tab button styles
        TabQuick.Style = tab == "simple" ? _accentButtonStyle : _subtleButtonStyle;
        TabFineTune.Style = tab == "advanced" ? _accentButtonStyle : _subtleButtonStyle;
        TabGodMode.Style = tab == "expert" ? _accentButtonStyle : _subtleButtonStyle;

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

    private void ApplySessionXmlToAdvancedControls(string xmlText)
    {
        if (_advCtrlSliders.Count == 0) return;

        var rows = CameraMod.ParseXmlToRows(xmlText);
        var lookup = new Dictionary<string, string>();
        foreach (var r in rows) lookup[r.FullKey] = r.Value;

        // Shared sliders: multiple keys point to the same Slider object (e.g. all on-foot sections
        // share one ZoomDistance slider). We must only set each physical slider once, using the first
        // matching key (the representative section), otherwise the last iterated key wins and may
        // revert the slider to vanilla if that section wasn't modified by the style.
        var alreadySet = new HashSet<Slider>();
        _suppressEvents = true;
        try
        {
            foreach (var (key, slider) in _advCtrlSliders)
            {
                if (slider == null || alreadySet.Contains(slider)) continue;
                if (lookup.TryGetValue(key, out string? val) &&
                    double.TryParse(val, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double d))
                {
                    double clamped = Math.Clamp(d, slider.Minimum, slider.Maximum);
                    slider.Value = clamped;
                    alreadySet.Add(slider);

                    if (_advCtrlValueLabels.TryGetValue(key, out var valueLabel))
                    {
                        valueLabel.Text = $"{clamped:F2}";
                        _advCtrlVanilla.TryGetValue(key, out string? vanStr);
                        double vanVal = double.TryParse(vanStr,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double vv) ? vv : clamped;
                        valueLabel.Foreground = Math.Abs(clamped - vanVal) > 0.001
                            ? _accentBrush
                            : _textDimBrush;
                    }
                }
            }
        }
        finally
        {
            _suppressEvents = false;
        }

        AdvCtrlUpdateChangedLabel();
        ApplyAdvCtrlSearch();
    }

    private void EnterExpertMode()
    {
        try
        {
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
            _advAllRows = CameraMod.ParseXmlToRows(vanillaXml)
                .Where(r => r.Section.StartsWith("Player_", StringComparison.Ordinal))
                .ToList();

            string liveXml = _sessionXml ?? CameraMod.ReadLiveXml(_gameDir);
            var liveRows = CameraMod.ParseXmlToRows(liveXml);
            var liveLookup = new Dictionary<string, string>();
            foreach (var lr in liveRows)
                liveLookup[lr.FullKey] = lr.Value;
            foreach (var row in _advAllRows)
                if (liveLookup.TryGetValue(row.FullKey, out string? liveVal))
                    row.Value = liveVal;

            // File overlays session: after Quick tab, _sessionXml is curated XML and omits God-only cells;
            // advanced_overrides.json keeps those edits. Preset / full session loads clear that file first.
            AdvLoadOverrides();
            AdvBindGrid();
            AdvPopulateFilter();
            AdvRefreshPresetCombo();
            AdvUpdateRowCount();

            var lightText = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
            AdvSearchBox.Foreground = lightText;
            AdvSearchBox.CaretBrush = lightText;

            SetStatus("God Mode — edit raw XML values and export them as JSON.", "TextDim");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load camera XML:\n{ex.Message}", "God Mode",
                MessageBoxButton.OK, MessageBoxImage.Error);
            SwitchAppMode("simple");
        }

        ApplyPresetEditingLockUi();
    }

    private void AdvBindGrid()
    {
        _advFilteredRows = new ObservableCollection<AdvancedRow>(_advAllRows);
        var view = CollectionViewSource.GetDefaultView(_advFilteredRows);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Section"));
        ExpertDataGrid.ItemsSource = view;
        ScheduleGodModeExpandHook();
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
        ScheduleGodModeExpandHook();
    }

    private void OnExpertCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        if (IsActivePresetEditingLocked())
            return;

        if (e.EditAction != DataGridEditAction.Commit)
            return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            AdvSaveOverrides();
            AdvUpdateRowCount();
            SaveCurrentUiState();
            QueueSavedToast();
        }), DispatcherPriority.Background);
    }

    private void OnAdvExpandAll(object sender, RoutedEventArgs e)
    {
        bool expanding = (sender as System.Windows.Controls.Button)?.Content?.ToString() == "Expand All";
        if (expanding)
        {
            _godModeExpandedSections.Clear();
            foreach (var r in _advFilteredRows)
                _godModeExpandedSections.Add(r.Section);
        }
        else
            _godModeExpandedSections.Clear();

        ExpertDataGrid.GroupStyle.Clear();
        ExpertDataGrid.GroupStyle.Add(BuildAdvGroupStyle(expanding));
        if (sender is System.Windows.Controls.Button btn)
            btn.Content = expanding ? "Collapse All" : "Expand All";

        ScheduleGodModeExpandHook();
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
        if (IsActivePresetEditingLocked())
        {
            SetStatus("Unlock this preset in the sidebar to reset God Mode values.", "Warn");
            return;
        }

        try
        {
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);

            string styleId = GetSelectedStyleId();
            int fov = GetSelectedFov();
            bool bane = BaneCheck.IsChecked == true;
            double pullback = GetCombatPullback();
            bool mount = MountHeightCheck.IsChecked == true;
            double? customUp = null;
            if (styleId == "custom")
            {
                CameraRules.RegisterCustomStyle(DistSlider.Value, HeightSlider.Value, HShiftSlider.Value);
                customUp = HeightSlider.Value;
            }
            bool sc = SteadycamCheck.IsChecked == true;
            var modSet = CameraRules.BuildModifications(styleId, fov, bane, combatPullback: pullback, mountHeight: mount, customUp: customUp, steadycam: sc);
            vanillaXml = CameraMod.ApplyModifications(vanillaXml, modSet);

            var defaultRows = CameraMod.ParseXmlToRows(vanillaXml);
            var lookup = new Dictionary<string, string>();
            foreach (var dr in defaultRows)
                lookup[dr.FullKey] = dr.Value;

            foreach (var row in _advAllRows)
                row.Value = lookup.TryGetValue(row.FullKey, out string? val) ? val : row.VanillaValue;

            AdvApplyFilter();
            AdvSaveOverrides();
            SaveCurrentUiState(immediate: true);
            SetStatus("Reset to UCM Quick defaults plus vanilla.", "Success");
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

        var dlg = new ExportDialog("God Mode Overrides", encoded) { Owner = this };
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
        AdvSaveOverrides();
        SaveCurrentUiState(immediate: true);
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
            AdvSaveOverrides();
            SaveCurrentUiState(immediate: true);
            SetStatus($"Imported {applied} values from {Path.GetFileName(ofd.FileName)}.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"XML import failed: {ex.Message}", "Error");
        }
    }

    private void OnAdvApply(object sender, RoutedEventArgs e)
    {
        SetStatus("Apply to game has been removed from v3. Use Export JSON to package these overrides.", "Warn");
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

    // â”€â”€ Advanced presets â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void AdvRefreshPresetCombo()
    {
        // AdvPresetCombo removed from XAML — preset selection is sidebar-only now.
    }

    // â”€â”€ XML Export / Import â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Decodes the live camera entry from the game PAZ (no v3 UI button; kept for parity / future use).
    /// Sidebar Export opens <see cref="ExportJsonDialog"/> for session/preset XML, JSON, or patched 0.paz.
    /// </summary>
    private async void OnExportXmlFile(object sender, RoutedEventArgs e)
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

        string gameDir = _gameDir;
        string destPath = sfd.FileName;
        SetGlobalBusy(true, "Exporting camera XML\u2026");
        try
        {
            await Task.Run(() => CameraMod.ExportLiveXml(gameDir, destPath)).ConfigureAwait(true);
            SetStatus($"Exported to {Path.GetFileName(destPath)}.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Export failed: {ex.Message}", "Error");
        }
        finally
        {
            SetGlobalBusy(false);
        }
    }

    private async void OnImportXmlFile(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir))
        {
            SetStatus("Game folder not set.", "Warn");
            return;
        }

        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import Camera XML â€” installs directly to game",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        var confirm = MessageBox.Show(
            $"Install '{Path.GetFileName(ofd.FileName)}' directly to the game?\n\nThis will overwrite your current camera settings.",
            "Import XML", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        string gameDir = _gameDir;
        string platform = _detectedPlatform;
        string srcPath = ofd.FileName;
        SetGlobalBusy(true, "Installing camera XML\u2026");
        try
        {
            await Task.Run(() =>
            {
                string xml = File.ReadAllText(srcPath);
                CameraMod.InstallRawXml(gameDir, xml);
            }).ConfigureAwait(true);
            try
            {
                GameInstallBaselineTracker.SaveAfterSuccessfulInstall(ExeDir, Ver, gameDir, platform);
                _gameUpdateNoticeSessionDismissed = false;
            }
            catch { }
            RefreshGameUpdateNotice();
            SetStatus($"Installed {Path.GetFileName(srcPath)} to game.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Import failed: {ex.Message}", "Error");
        }
        finally
        {
            SetGlobalBusy(false);
        }
    }

    // â”€â”€ Advanced Controls â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    // Stores all slider controls keyed by ModKey.Attribute (same as AdvancedRow.FullKey)
    private readonly Dictionary<string, Slider> _advCtrlSliders = new();
    private readonly Dictionary<string, TextBlock> _advCtrlValueLabels = new();
    private readonly Dictionary<string, string> _advCtrlVanilla = new();
    // Every slider instance ever created — used for lock/unlock so duplicate-key sliders aren't missed
    private readonly List<Slider> _advCtrlAllSliders = new();
    // Keys controlled by Steadycam -- sliders for these are locked when Steadycam is on
    private HashSet<string>? _steadycamKeys;

    // ── Cached resources (avoid repeated FindResource lookups) ──
    private Style? _accentButtonStyle;
    private Style? _subtleButtonStyle;
    private Brush? _textSecondaryBrush;
    private Brush? _accentBrush;
    private Brush? _textDimBrush;
    private DispatcherTimer? _advCtrlSearchDebounceTimer;
    // AdvCtrlPresetsDir removed — session presets live under ucm_presets / my_presets

    private void EnterAdvancedControlsMode()
    {
        if (_advCtrlSliders.Count > 0)
        {
            string xml = _sessionXml ?? BuildCuratedSessionXml();
            ApplySessionXmlToAdvancedControls(xml);
            ApplyPresetEditingLockUi();
            return;
        }

        try
        {
            // Load vanilla values for display and reset (cached after first load)
            if (_advCtrlVanilla.Count == 0)
            {
                string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
                var vanillaRows = CameraMod.ParseXmlToRows(vanillaXml);
                foreach (var r in vanillaRows) _advCtrlVanilla[r.FullKey] = r.VanillaValue;
            }

            // Suppress layout passes while building ~150 slider controls
            AdvCtrlPanel.BeginInit();
            try
            {
                BuildAdvCtrlSection_OnFoot();
                BuildAdvCtrlSection_Mount();
                BuildAdvCtrlSection_Global();
                BuildAdvCtrlSection_SpecialMounts();
                BuildAdvCtrlSection_Combat();
                BuildAdvCtrlSection_Smooth();
                BuildAdvCtrlSection_Aim();
            }
            finally
            {
                AdvCtrlPanel.EndInit();
            }

            AdvCtrlRefreshPresetCombo();
            string sessionXml = _sessionXml ?? BuildCuratedSessionXml();
            ApplySessionXmlToAdvancedControls(sessionXml);
            AdvCtrlUpdateChangedLabel();
            ApplyAdvCtrlSearch();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load camera XML:\n{ex.Message}", "UCM Fine Tune",
                MessageBoxButton.OK, MessageBoxImage.Error);
            SwitchAppMode("simple");
        }

        ApplyPresetEditingLockUi();
    }

    // â”€â”€ Control builder helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private Grid BuildSliderRow(string modKey, string attribute, double min, double max, double step,
        string? tooltip = null)
    {
        string fullKey = $"{modKey}.{attribute}";
        _advCtrlVanilla.TryGetValue(fullKey, out string? vanillaStr);
        double vanillaVal = double.TryParse(vanillaStr, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double vv) ? vv : (min + max) / 2;
        double current = vanillaVal;

        string searchText = $"{modKey} {attribute} {tooltip ?? CameraParamDocs.Get(attribute)}";
        var row = new Grid { Margin = new Thickness(0, 2, 0, 2), Tag = searchText };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200), MinWidth = 130 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 60 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46), MinWidth = 46 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46), MinWidth = 46 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56), MinWidth = 56 });

        // Label
        var label = new TextBlock
        {
            Text = attribute,
            FontSize = 11,
            Foreground = _textSecondaryBrush,
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
            Foreground = _accentBrush,
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
            Foreground = _textDimBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 0, 2, 0),
            ToolTip = $"Vanilla: {vanillaVal:F2}"
        };
        Grid.SetColumn(vanillaLabel, 3);

        // Reset button
        var resetBtn = new System.Windows.Controls.Button
        {
            Content = "Reset",
            FontSize = 10,
            Width = 50,
            Height = 22,
            Padding = new Thickness(6, 2, 6, 2),
            Margin = new Thickness(2, 0, 0, 0),
            ToolTip = $"Reset to vanilla ({vanillaVal:F2})",
            Style = _subtleButtonStyle,
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
            if (_suppressEvents) return;
            if (IsActivePresetEditingLocked() || IsActivePresetDeepEditLocked()) { ShowLockedToastIfNeeded(); slider.Value = e.OldValue; return; }
            valueLabel.Text = $"{e.NewValue:F2}";
            bool changed = Math.Abs(e.NewValue - vanillaVal) > 0.001;
            valueLabel.Foreground = changed
                ? _accentBrush
                : _textDimBrush;
            AdvCtrlUpdateChangedLabel();
            SaveCurrentUiState();
            QueueSavedToast();
        };

        row.Children.Add(label);
        row.Children.Add(slider);
        row.Children.Add(valueLabel);
        row.Children.Add(vanillaLabel);
        row.Children.Add(resetBtn);

        _advCtrlSliders[fullKey] = slider;
        _advCtrlAllSliders.Add(slider);
        _advCtrlValueLabels[fullKey] = valueLabel;

        return row;
    }

    private TextBlock BuildAdvCtrlSubHeader(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = _textSecondaryBrush,
            Margin = new Thickness(0, 10, 0, 4)
        };
    }

    /// <summary>Wraps child elements in a bordered card with a title — same style as zoom level groups.</summary>
    private Border WrapInCard(string title, params UIElement[] children)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = _accentBrush,
            Margin = new Thickness(0, 0, 0, 8)
        });
        foreach (var child in children)
            stack.Children.Add(child);

        return new Border
        {
            Background = (Brush)FindResource("BgInputBrush"),
            BorderBrush = (Brush)FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 10, 10, 10),
            Margin = new Thickness(0, 8, 0, 6),
            Tag = title,
            Child = stack
        };
    }

    private Grid BuildSharedSliderRow(string labelText, string[] modKeys, string attribute,
        double min, double max, double step, string? tooltip = null)
    {
        string representative = modKeys[0];
        var row = BuildSliderRow(representative, attribute, min, max, step, tooltip);
        if (row.Children[0] is TextBlock label)
            label.Text = labelText;

        var actualSlider = _advCtrlSliders[$"{representative}.{attribute}"];
        foreach (string modKey in modKeys)
            _advCtrlSliders[$"{modKey}.{attribute}"] = actualSlider;

        return row;
    }

    private FrameworkElement BuildZoomLevelGroup(string title, string[] sections, int zoomLevel,
        (string Attr, double Min, double Max, double Step)[] attrs)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = _accentBrush,
            Margin = new Thickness(0, 0, 0, 8)
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
            stack.Children.Add(row);

            // Re-register the actual slider for all sections
            var actualSlider = _advCtrlSliders[$"{modKey}.{attr}"];
            foreach (var sec in sections)
            {
                string k = $"{sec}/ZoomLevel[{zoomLevel}].{attr}";
                _advCtrlSliders[k] = actualSlider;
            }
        }
        return new Border
        {
            Background = (Brush)FindResource("BgInputBrush"),
            BorderBrush = (Brush)FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 10, 10, 10),
            Margin = new Thickness(0, 8, 0, 6),
            Tag = title,
            Child = stack
        };
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

        foreach (int zl in new[] { 2, 3 })
            panel.Children.Add(BuildZoomLevelGroup($"Zoom Level {zl}", horseSections, zl, zoomAttrs));

        AdvCtrlMountGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Global()
    {
        var panel = new StackPanel();

        var sharedFovSliders = new List<UIElement>();
        sharedFovSliders.Add(BuildSharedSliderRow("On-foot FoV",
            new[]
            {
                "Player_Basic_Default_Run", "Player_Basic_Default_Runfast", "Player_Basic_Default_Walk"
            },
            "Fov", 25.0, 75.0, 1.0, "Shared field of view for the main on-foot movement cameras."));
        sharedFovSliders.Add(BuildSharedSliderRow("Combat FoV",
            new[]
            {
                "Player_Weapon_Default", "Player_Weapon_Default_Run", "Player_Weapon_Default_RunFast",
                "Player_Weapon_Default_RunFast_Follow", "Player_Weapon_Default_Walk",
                "Player_Weapon_Rush", "Player_Weapon_Guard"
            },
            "Fov", 25.0, 75.0, 1.0, "Shared field of view for the core weapon and combat movement cameras."));
        sharedFovSliders.Add(BuildSharedSliderRow("Force/Titan/Cinematic FoV",
            new[]
            {
                "Player_Force_LockOn", "Player_LockOn_Titan", "Cinematic_LockOn"
            },
            "Fov", 25.0, 75.0, 1.0));
        sharedFovSliders.Add(BuildSharedSliderRow("Warmachine/Broom FoV",
            new[]
            {
                "Player_Ride_Warmachine", "Player_Ride_Warmachine_Aim",
                "Player_Ride_Warmachine_Dash", "Player_Ride_Broom"
            },
            "Fov", 25.0, 75.0, 1.0));
        {
            var elephantFovRow = BuildSliderRow("Player_Ride_Elephant", "Fov", 25.0, 75.0, 1.0, "Elephant field of view.");
            if (elephantFovRow.Children[0] is TextBlock elephantFovLabel)
                elephantFovLabel.Text = "Elephant FoV";
            sharedFovSliders.Add(elephantFovRow);
        }
        {
            var wyvernFovRow = BuildSliderRow("Player_Ride_Wyvern", "Fov", 25.0, 75.0, 1.0, "Wyvern field of view.");
            if (wyvernFovRow.Children[0] is TextBlock wyvernFovLabel)
                wyvernFovLabel.Text = "Wyvern FoV";
            sharedFovSliders.Add(wyvernFovRow);
        }
        {
            var swimFovRow = BuildSliderRow("Player_Swim_Default", "Fov", 25.0, 75.0, 1.0, "Swimming field of view.");
            if (swimFovRow.Children[0] is TextBlock swimFovLabel)
                swimFovLabel.Text = "Swim FoV";
            sharedFovSliders.Add(swimFovRow);
        }
        panel.Children.Add(WrapInCard("Shared FoV", sharedFovSliders.ToArray()));

        var twoTargetSliders = new List<UIElement>();
        {
            var interactionZl3Row = BuildSliderRow("Player_Interaction_TwoTarget/ZoomLevel[3]", "MaxZoomDistance", 4.0, 20.0, 0.5);
            if (interactionZl3Row.Children[0] is TextBlock interactionZl3Label)
                interactionZl3Label.Text = "Interaction ZL3 Max";
            twoTargetSliders.Add(interactionZl3Row);
        }
        {
            var interactionZl4Row = BuildSliderRow("Player_Interaction_TwoTarget/ZoomLevel[4]", "MaxZoomDistance", 4.0, 20.0, 0.5);
            if (interactionZl4Row.Children[0] is TextBlock interactionZl4Label)
                interactionZl4Label.Text = "Interaction ZL4 Max";
            twoTargetSliders.Add(interactionZl4Row);
        }
        panel.Children.Add(WrapInCard("Two-target framing", twoTargetSliders.ToArray()));

        var traversalFovSliders = new List<UIElement>();
        foreach (var (modKey, labelText) in new[]
        {
            ("Player_Swim_Default", "Swim FoV"),
            ("Player_Basic_Climb", "Climb FoV"),
            ("Player_Basic_Gliding", "Glide FoV"),
            ("Player_Basic_FreeFall", "Freefall FoV")
        })
        {
            var row = BuildSliderRow(modKey, "Fov", 25.0, 75.0, 1.0);
            if (row.Children[0] is TextBlock label)
                label.Text = labelText;
            traversalFovSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Traversal FoV", traversalFovSliders.ToArray()));

        AdvCtrlGlobalGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_SpecialMounts()
    {
        var panel = new StackPanel();

        var elephantSliders = new List<UIElement>();
        foreach (int zl in new[] { 1, 2, 3, 4 })
        {
            var row = BuildSliderRow($"Player_Ride_Elephant/ZoomLevel[{zl}]", "ZoomDistance", 0.5, 25.0, 0.1);
            if (row.Children[0] is TextBlock label)
                label.Text = $"Elephant ZL{zl}";
            elephantSliders.Add(row);
        }
        foreach (int zl in new[] { 2, 3, 4 })
        {
            var row = BuildSliderRow($"Player_Ride_Elephant/ZoomLevel[{zl}]", "UpOffset", -2.0, 3.0, 0.1);
            if (row.Children[0] is TextBlock label)
                label.Text = $"Elephant ZL{zl} Height";
            elephantSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Elephant", elephantSliders.ToArray()));

        var wyvernSliders = new List<UIElement>();
        foreach (int zl in new[] { 1, 2, 3, 4 })
        {
            var row = BuildSliderRow($"Player_Ride_Wyvern/ZoomLevel[{zl}]", "ZoomDistance", 1.0, 30.0, 0.1);
            if (row.Children[0] is TextBlock label)
                label.Text = $"Wyvern ZL{zl}";
            wyvernSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Wyvern", wyvernSliders.ToArray()));

        var canoeMiscSliders = new List<UIElement>();
        foreach (var (modKey, attr, min, max, step, labelText) in new[]
        {
            ("Player_Ride_Canoe/ZoomLevel[2]", "ZoomDistance", 1.0, 20.0, 0.1, "Canoe ZL2"),
            ("Player_Ride_Canoe/ZoomLevel[3]", "ZoomDistance", 1.0, 20.0, 0.1, "Canoe ZL3"),
            ("Player_Ride_Warmachine/ZoomLevel[2]", "ZoomDistance", 1.0, 20.0, 0.1, "Warmachine ZL2"),
            ("Player_Ride_Warmachine/ZoomLevel[3]", "ZoomDistance", 1.0, 20.0, 0.1, "Warmachine ZL3"),
            ("Player_Ride_Broom/ZoomLevel[2]", "ZoomDistance", 1.0, 24.0, 0.1, "Broom ZL2"),
            ("Player_Ride_Broom/ZoomLevel[3]", "ZoomDistance", 1.0, 24.0, 0.1, "Broom ZL3")
        })
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock label)
                label.Text = labelText;
            canoeMiscSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Canoe / Warmachine / Broom", canoeMiscSliders.ToArray()));

        AdvCtrlSpecialMountGrid.Children.Add(panel);
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
            ("Player_Interaction_TwoTarget", "TargetRate", 0.0, 1.0, 0.05),
            ("Player_Interaction_TwoTarget", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn_System", "TargetRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn_System", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Revive_LockOn_System", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn_Non_Rotate", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn_WrestleOnly", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_StartAggro_TwoTarget", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Wanted_TwoTarget", "ScreenClampRate", 0.0, 1.0, 0.05),
        };

        var trackingSliders = new List<UIElement>();
        foreach (var (sec, attr, min, max, step) in sectionAttrs)
        {
            var row = BuildSliderRow(sec, attr, min, max, step);
            if (row.Children[0] is TextBlock lbl)
                lbl.Text = $"{sec.Replace("Player_", "")} - {attr}";
            trackingSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Lock-On Tracking", trackingSliders.ToArray()));

        // ZoomDistance per lock-on section per zoom level
        // All sections now expose ZL2+ZL3+ZL4; missing levels are injected by Steadycam.
        var lockOnSections = new[]
        {
            ("Player_Weapon_LockOn",              new[] { 2, 3, 4 }),
            ("Player_Weapon_TwoTarget",           new[] { 1, 2, 3, 4 }),
            ("Player_Interaction_TwoTarget",      new[] { 1, 2, 3, 4 }),
            ("Player_FollowLearn_LockOn_Boss",    new[] { 2, 3, 4 }),
            ("Player_Weapon_LockOn_System",       new[] { 2, 3, 4 }),
            ("Player_Revive_LockOn_System",       new[] { 2, 3, 4 }),
            ("Player_Weapon_LockOn_Non_Rotate",   new[] { 2, 3, 4 }),
            ("Player_Weapon_LockOn_WrestleOnly",  new[] { 2, 3, 4 }),
        };

        foreach (var (sec, levels) in lockOnSections)
        {
            var zoomSliders = new List<UIElement>();
            foreach (int zl in levels)
            {
                var row = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "ZoomDistance", 1.0, 20.0, 0.5);
                if (row.Children[0] is TextBlock lbl) lbl.Text = $"ZL{zl} ZoomDistance";
                zoomSliders.Add(row);
            }
            panel.Children.Add(WrapInCard($"{sec.Replace("Player_", "")} - Zoom Distances", zoomSliders.ToArray()));
        }

        var fovSliders = new List<UIElement>();
        foreach (var (modKey, labelText) in new[]
        {
            ("Player_Weapon_LockOn", "Weapon LockOn FoV"),
            ("Player_Weapon_TwoTarget", "Weapon TwoTarget FoV"),
            ("Player_Interaction_TwoTarget", "Interaction TwoTarget FoV"),
            ("Player_FollowLearn_LockOn_Boss", "Boss LockOn FoV"),
            ("Player_Weapon_LockOn_System", "LockOn System FoV"),
            ("Player_Revive_LockOn_System", "Revive LockOn FoV"),
            ("Player_Weapon_LockOn_Non_Rotate", "Non-Rotate LockOn FoV"),
            ("Player_Weapon_LockOn_WrestleOnly", "Wrestle LockOn FoV"),
            ("Player_StartAggro_TwoTarget", "StartAggro FoV"),
            ("Player_Wanted_TwoTarget", "Wanted FoV")
        })
        {
            var row = BuildSliderRow(modKey, "Fov", 25.0, 75.0, 1.0);
            if (row.Children[0] is TextBlock label)
                label.Text = labelText;
            fovSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("FoV Touch Points", fovSliders.ToArray()));

        AdvCtrlCombatGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Smooth()
    {
        var panel = new StackPanel();

        var smoothSliders = new List<UIElement>();
        var smoothEntries = new[]
        {
            ("Player_Basic_Default_Run/CameraBlendParameter",  "BlendInTime",  0.0, 3.0, 0.1, "Run blend-in"),
            ("Player_Basic_Default_Run/CameraBlendParameter",  "BlendOutTime", 0.0, 3.0, 0.1, "Run blend-out"),
            ("Player_Weapon_Guard/CameraBlendParameter",       "BlendInTime",  0.0, 3.0, 0.1, "Guard blend-in"),
            ("Player_Weapon_Guard/CameraBlendParameter",       "BlendOutTime", 0.0, 3.0, 0.1, "Guard blend-out"),
            ("Player_Basic_Default_Run/OffsetByVelocity",      "OffsetLength", 0.0, 2.0, 0.1, "Run sway"),
            ("Player_Weapon_Default_Run/OffsetByVelocity",     "OffsetLength", 0.0, 2.0, 0.1, "Combat run sway"),
            ("Player_Weapon_Default_RunFast/OffsetByVelocity", "OffsetLength", 0.0, 2.0, 0.1, "Combat sprint sway"),
            ("Player_Weapon_Default_RunFast_Follow/OffsetByVelocity", "OffsetLength", 0.0, 2.0, 0.1, "Follow sprint sway"),
            ("Player_Animal_Default/CameraBlendParameter",     "BlendInTime",  0.0, 3.0, 0.1, "Animal idle blend-in"),
            ("Player_Animal_Default_Run/CameraBlendParameter", "BlendInTime",  0.0, 3.0, 0.1, "Animal run blend-in"),
            ("Player_Animal_Default_Run/OffsetByVelocity",     "OffsetLength", 0.0, 2.0, 0.1, "Animal run sway"),
            ("Player_Animal_Default_Runfast/CameraBlendParameter", "BlendInTime", 0.0, 3.0, 0.1, "Animal sprint blend-in"),
            ("Player_Animal_Default_Runfast/OffsetByVelocity", "OffsetLength", 0.0, 2.0, 0.1, "Animal sprint sway"),
            ("Player_Animal_Default_Runfast/OffsetByVelocity", "DampSpeed",    0.0, 2.0, 0.1, "Animal sprint damp"),
            ("Player_Weapon_LockOn/CameraBlendParameter",               "BlendInTime",  0.0, 3.0, 0.1, "LockOn blend-in"),
            ("Player_Weapon_LockOn/CameraBlendParameter",               "BlendOutTime", 0.0, 3.0, 0.1, "LockOn blend-out"),
            ("Player_Weapon_LockOn_System/CameraBlendParameter",        "BlendInTime",  0.0, 3.0, 0.1, "LockOn System blend-in"),
            ("Player_Weapon_LockOn_System/CameraBlendParameter",        "BlendOutTime", 0.0, 3.0, 0.1, "LockOn System blend-out"),
            ("Player_FollowLearn_LockOn_Boss/CameraBlendParameter",     "BlendInTime",  0.0, 3.0, 0.1, "Boss LockOn blend-in"),
            ("Player_FollowLearn_LockOn_Boss/CameraBlendParameter",     "BlendOutTime", 0.0, 3.0, 0.1, "Boss LockOn blend-out"),
        };

        foreach (var (modKey, attr, min, max, step, friendlyName) in smoothEntries)
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock lbl) lbl.Text = friendlyName;
            smoothSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("On-foot and combat smoothing", smoothSliders.ToArray()));

        string[] onFootFollowSections =
        {
            "Player_Basic_Default", "Player_Basic_Default_Walk",
            "Player_Basic_Default_Run", "Player_Basic_Default_Runfast"
        };

        var onFootFollowSliders = new List<UIElement>();
        onFootFollowSliders.Add(BuildSharedSliderRow("On-foot yaw follow", onFootFollowSections, "FollowYawSpeedRate", 0.0, 2.0, 0.05));
        onFootFollowSliders.Add(BuildSharedSliderRow("On-foot pitch follow", onFootFollowSections, "FollowPitchSpeedRate", 0.0, 2.0, 0.05));
        onFootFollowSliders.Add(BuildSharedSliderRow("On-foot follow delay", onFootFollowSections, "FollowStartTime", 0.0, 5.0, 0.1));
        onFootFollowSliders.Add(BuildSharedSliderRow("On-foot pivot damping", Array.ConvertAll(onFootFollowSections, s => $"{s}/CameraDamping"), "PivotDampingMaxDistance", 0.0, 2.0, 0.05));
        panel.Children.Add(WrapInCard("On-foot follow behavior", onFootFollowSliders.ToArray()));

        string[] horseSections =
        {
            "Player_Ride_Horse", "Player_Ride_Horse_Run", "Player_Ride_Horse_Fast_Run",
            "Player_Ride_Horse_Dash", "Player_Ride_Horse_Dash_Att",
            "Player_Ride_Horse_Att_Thrust", "Player_Ride_Horse_Att_R", "Player_Ride_Horse_Att_L"
        };

        var horseSyncSliders = new List<UIElement>();
        horseSyncSliders.Add(BuildSharedSliderRow("Horse yaw follow", horseSections, "FollowYawSpeedRate", 0.0, 2.0, 0.05));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse pitch follow", horseSections, "FollowPitchSpeedRate", 0.0, 2.0, 0.05));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse follow delay", horseSections, "FollowStartTime", 0.0, 5.0, 0.1));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse default pitch", horseSections, "FollowDefaultPitch", -10.0, 30.0, 0.5));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse blend-in", Array.ConvertAll(horseSections, s => $"{s}/CameraBlendParameter"), "BlendInTime", 0.0, 3.0, 0.1));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse blend-out", Array.ConvertAll(horseSections, s => $"{s}/CameraBlendParameter"), "BlendOutTime", 0.0, 3.0, 0.1));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse pivot damping", Array.ConvertAll(horseSections, s => $"{s}/CameraDamping"), "PivotDampingMaxDistance", 0.0, 2.0, 0.05));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse sway", Array.ConvertAll(horseSections, s => $"{s}/OffsetByVelocity"), "OffsetLength", 0.0, 2.0, 0.1));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse sway damp", Array.ConvertAll(horseSections, s => $"{s}/OffsetByVelocity"), "DampSpeed", 0.0, 2.0, 0.1));
        panel.Children.Add(WrapInCard("Horse state synchronization", horseSyncSliders.ToArray()));

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
            var aimGroupSliders = new List<UIElement>();
            foreach (var (sec, zl) in entries)
            {
                string shortName = $"{sec.Replace("Player_", "").Replace("_Aim_Zoom", "").Replace("_Aim", "")} ZL{zl}";

                var distRow = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "ZoomDistance", 0.5, 20.0, 0.1);
                if (distRow.Children[0] is TextBlock distLabel)
                    distLabel.Text = $"{shortName} Dist";
                aimGroupSliders.Add(distRow);

                var upRow = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "UpOffset", -2.0, 2.0, 0.1);
                if (upRow.Children[0] is TextBlock upLabel)
                    upLabel.Text = $"{shortName} Height";
                aimGroupSliders.Add(upRow);

                var rightRow = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "RightOffset", -1.0, 3.0, 0.05);
                if (rightRow.Children[0] is TextBlock rightLabel)
                    rightLabel.Text = $"{shortName} Shift";
                aimGroupSliders.Add(rightRow);
            }
            panel.Children.Add(WrapInCard(groupName, aimGroupSliders.ToArray()));
        }

        var traversalFramingSliders = new List<UIElement>();
        foreach (var (modKey, attr, min, max, step, labelText) in new[]
        {
            ("Player_Swim_Default/ZoomLevel[2]", "ZoomDistance", 0.5, 20.0, 0.1, "Swim ZL2 Dist"),
            ("Player_Swim_Default/ZoomLevel[2]", "UpOffset", -2.0, 2.0, 0.1, "Swim ZL2 Height"),
            ("Player_Basic_Climb/ZoomLevel[2]", "ZoomDistance", 0.5, 20.0, 0.1, "Climb ZL2 Dist"),
            ("Player_Basic_Climb/ZoomLevel[2]", "UpOffset", -2.0, 2.0, 0.1, "Climb ZL2 Height"),
            ("Player_Basic_Gliding/ZoomLevel[2]", "ZoomDistance", 0.5, 20.0, 0.1, "Glide ZL2 Dist"),
            ("Player_Basic_Gliding/ZoomLevel[2]", "UpOffset", -2.0, 2.0, 0.1, "Glide ZL2 Height"),
            ("Player_Basic_FreeFall/ZoomLevel[2]", "ZoomDistance", 0.5, 20.0, 0.1, "Freefall ZL2 Dist"),
            ("Player_Basic_FreeFall/ZoomLevel[2]", "UpOffset", -2.0, 2.0, 0.1, "Freefall ZL2 Height")
        })
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock label)
                label.Text = labelText;
            traversalFramingSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Traversal framing", traversalFramingSliders.ToArray()));

        AdvCtrlAimGrid.Children.Add(panel);
    }

    // â”€â”€ Advanced Controls apply / ModSet â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
    }

    private void ApplyAdvCtrlSearch()
    {
        if (!IsLoaded) return;

        string search = AdvCtrlSearchBox?.Text?.Trim() ?? "";
        bool hasSearch = !string.IsNullOrWhiteSpace(search);

        if (hasSearch)
        {
            AdvSectionA.IsExpanded = true;
            AdvSectionB.IsExpanded = true;
            AdvSectionC.IsExpanded = true;
            AdvSectionD.IsExpanded = true;
            AdvSectionE.IsExpanded = true;
            AdvSectionF.IsExpanded = true;
            AdvSectionG.IsExpanded = true;
        }

        ApplyAdvCtrlSearchToElement(AdvCtrlOnFootGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlMountGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlGlobalGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlSpecialMountGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlCombatGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlSmoothGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlAimGrid, search);
    }

    private bool ApplyAdvCtrlSearchToElement(UIElement element, string search)
    {
        bool hasSearch = !string.IsNullOrWhiteSpace(search);

        if (!hasSearch)
        {
            element.Visibility = Visibility.Visible;

            if (element is Border visibleBorder && visibleBorder.Child is UIElement borderChild)
                ApplyAdvCtrlSearchToElement(borderChild, search);
            else if (element is Panel visiblePanel)
                foreach (UIElement child in visiblePanel.Children)
                    ApplyAdvCtrlSearchToElement(child, search);

            return true;
        }

        if (element is Grid row && row.Tag is string rowTag)
        {
            bool match = rowTag.Contains(search, StringComparison.OrdinalIgnoreCase);
            row.Visibility = match ? Visibility.Visible : Visibility.Collapsed;
            return match;
        }

        if (element is TextBlock textBlock)
        {
            bool match = textBlock.Text.Contains(search, StringComparison.OrdinalIgnoreCase);
            textBlock.Visibility = match ? Visibility.Visible : Visibility.Collapsed;
            return match;
        }

        if (element is Border border)
        {
            bool titleMatch = border.Tag is string borderTag &&
                              borderTag.Contains(search, StringComparison.OrdinalIgnoreCase);
            bool childMatch = border.Child is UIElement borderChild &&
                              ApplyAdvCtrlSearchToElement(borderChild, search);
            bool visible = titleMatch || childMatch;
            border.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            return visible;
        }

        if (element is Panel panel)
        {
            bool anyVisible = false;
            foreach (UIElement child in panel.Children)
                anyVisible |= ApplyAdvCtrlSearchToElement(child, search);

            if (panel != AdvCtrlPanel)
                panel.Visibility = anyVisible ? Visibility.Visible : Visibility.Collapsed;

            return anyVisible;
        }

        return true;
    }

    private void OnAdvCtrlSearchChanged(object sender, TextChangedEventArgs e)
    {
        _advCtrlSearchDebounceTimer?.Stop();
        _advCtrlSearchDebounceTimer?.Start();
    }

    private void OnAdvCtrlClearSearch(object sender, RoutedEventArgs e)
    {
        AdvCtrlSearchBox.Clear();
        ApplyAdvCtrlSearch();
    }

    private void OnAdvCtrlApply(object sender, RoutedEventArgs e)
    {
        SetStatus("UCM Fine Tune no longer writes to game files in v3. Use Export JSON instead.", "Warn");
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
            SaveCurrentUiState(immediate: true);
            SetStatus("Loaded values from UCM Quick.", "Success");
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
        SaveCurrentUiState(immediate: true);
        SetStatus("Reset all UCM Fine Tune controls to vanilla.", "Success");
    }

    // â”€â”€ Advanced Controls presets â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void AdvCtrlRefreshPresetCombo()
    {
        // AdvCtrlPresetCombo removed from XAML — preset selection is sidebar-only now.
    }

    // â”€â”€ JSON Mod Manager â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    private void OnPresetNew(object sender, RoutedEventArgs e)
    {
        var dlg = new NewPresetDialog { Owner = this };
        if (dlg.ShowDialog() != true)
            return;

        string name = dlg.PresetName;
        string safeName = new string(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        try
        {
            CaptureSessionXml();
            string xml = _sessionXml ?? BuildCuratedSessionXml();

            var preset = new Dictionary<string, object>
            {
                ["name"] = name,
                ["author"] = dlg.AuthorName,
                ["description"] = dlg.Description,
                ["kind"] = "user",
                ["locked"] = false,
                ["style_id"] = GetSelectedStyleId(),
                ["session_xml"] = xml,
                ["settings"] = new Dictionary<string, object>
                {
                    ["distance"] = Math.Round(DistSlider.Value, 2),
                    ["height"] = Math.Round(HeightSlider.Value, 2),
                    ["right_offset"] = Math.Round(HShiftSlider.Value, 2),
                    ["fov"] = GetSelectedFov(),
                    ["combat_pullback"] = GetCombatPullback(),
                    ["centered"] = BaneCheck.IsChecked == true,
                    ["mount_height"] = MountHeightCheck.IsChecked == true,
                    ["steadycam"] = SteadycamCheck.IsChecked == true
                }
            };

            File.WriteAllText(Path.Combine(MyPresetsDir, $"{safeName}.ucmpreset"),
                JsonSerializer.Serialize(preset, PresetFileJsonOptions));
            RefreshPresetManagerLists(preserveSelection: false);
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

    private void OnPresetManagerDelete(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        var confirm = MessageBox.Show(
            $"Delete preset '{item.Name}'?",
            "Delete Preset",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
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

    private void OnPresetManagerRename(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        var dlg = new InputDialog("Rename Preset", "Enter the new preset name:")
        {
            Owner = this,
            InitialText = item.Name
        };
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ResponseText))
            return;

        string newName = SanitizeFileStem(dlg.ResponseText);
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
                var preset = LoadImportedPreset(item.Name) ?? throw new InvalidOperationException("Imported preset could not be loaded.");
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
                    dict["name"] = dlg.ResponseText.Trim();
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

    private void OnPresetManagerDuplicate(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null)
            return;

        var dlg = new InputDialog("Duplicate Preset", "Enter a name for the duplicated preset:")
        {
            Owner = this,
            InitialText = $"{item.Name}_copy"
        };
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ResponseText))
            return;

        string newName = SanitizeFileStem(dlg.ResponseText);
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
                var preset = LoadImportedPreset(item.Name) ?? throw new InvalidOperationException("Imported preset could not be loaded.");
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
                dict["name"] = dlg.ResponseText.Trim();
                dict["locked"] = false;
                if (promoteUcmToMyPresets)
                    dict["kind"] = "user";
                File.WriteAllText(newPath, JsonSerializer.Serialize(dict, PresetFileJsonOptions));
            }

            RefreshPresetManagerLists(preserveSelection: false);
            var duplicated = _presetManagerItems.FirstOrDefault(i =>
                string.Equals(i.FilePath, newPath, StringComparison.OrdinalIgnoreCase));
            if (duplicated != null)
                SetSelectedPresetManagerItem(duplicated, updateDetails: true);

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
            var preset = LoadImportedPreset(item.Name) ?? throw new InvalidOperationException("Imported preset could not be loaded.");
            _selectedImportedPreset = preset;
            string xml = BuildRebuiltXmlFromImportedPreset(preset);
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

    private void OnImportPreset(object sender, RoutedEventArgs e)
    {
        var dlg = new ImportPresetDialog { Owner = this };
        if (dlg.ShowDialog() != true) return;

        switch (dlg.SelectedMode)
        {
            case "mod_package": ImportModManagerPackage(); break;
            case "xml": ImportRawXml(); break;
            case "paz": ImportFromPaz(); break;
            case "ucmpreset": ImportUcmPreset(); break;
        }
    }

    private void ImportModManagerPackage()
    {
        using var folderDlg = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select the mod folder (containing manifest.json)",
            UseDescriptionForTitle = true
        };
        if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

        string folder = folderDlg.SelectedPath;
        try
        {
            string manifestPath = Path.Combine(folder, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                SetStatus("No manifest.json found in that folder.", "Error");
                return;
            }

            using var manifestDoc = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var manifest = manifestDoc.RootElement;
            string title = manifest.TryGetProperty("title", out var tv) ? tv.GetString() ?? "" : Path.GetFileName(folder);
            string author = manifest.TryGetProperty("author", out var av) ? av.GetString() ?? "" : "";
            string rawDesc = manifest.TryGetProperty("description", out var dv) ? dv.GetString() ?? "" : "";
            string version = manifest.TryGetProperty("version", out var vv) ? vv.GetString() ?? "" : "";
            string nexusUrl = manifest.TryGetProperty("nexus_url", out var nu) ? nu.GetString() ?? "" : "";

            string filesDir = manifest.TryGetProperty("files_dir", out var fd) ? fd.GetString() ?? "files" : "files";
            string fullFilesDir = Path.Combine(folder, filesDir);
            string? xmlPath = Directory.GetFiles(fullFilesDir, "playercamerapreset.xml", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (xmlPath == null)
            {
                SetStatus("No playercamerapreset.xml found in the mod package.", "Error");
                return;
            }

            string xml = File.ReadAllText(xmlPath);
            string safeStem = new string(title.Trim().Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            if (string.IsNullOrWhiteSpace(safeStem))
                safeStem = "Imported_Mod";

            string shortDesc = rawDesc.Split("\n\n")[0].Replace("\n", " ").Trim();
            if (shortDesc.Length > 200) shortDesc = shortDesc[..197] + "...";

            var metaDlg = new ImportMetadataDialog(
                $"Importing mod package: {Path.GetFileName(folder)}",
                safeStem,
                string.IsNullOrWhiteSpace(author) ? null : author,
                string.IsNullOrWhiteSpace(shortDesc) ? null : shortDesc,
                string.IsNullOrWhiteSpace(nexusUrl) ? null : nexusUrl)
            { Owner = this };
            if (metaDlg.ShowDialog() != true) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                var overwrite = MessageBox.Show(
                    $"Overwrite imported preset '{chosenName}'?",
                    "Import Preset",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (overwrite != MessageBoxResult.Yes)
                    return;
            }

            string sourceLabel = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(version))
                sourceLabel += $" (v{version})";

            var imported = BuildImportedPreset(chosenName, "mod", sourceLabel, xmlPath, xml, null,
                metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(imported);
            RefreshPresetManagerLists(preserveSelection: false);
            SelectImportedPreset(SanitizeFileStem(imported.Name));
            QueueSavedToast($"Imported '{imported.Name}'");
            SetStatus($"Imported mod package as '{imported.Name}'.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Import failed: {ex.Message}", "Error");
        }
    }

    private void ImportRawXml()
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
            string baseName = Path.GetFileNameWithoutExtension(ofd.FileName);
            if (baseName.Equals("playercamerapreset", StringComparison.OrdinalIgnoreCase))
                baseName = "Imported Camera";

            var metaDlg = new ImportMetadataDialog(
                $"Importing XML: {Path.GetFileName(ofd.FileName)}",
                baseName)
            { Owner = this };
            if (metaDlg.ShowDialog() != true) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                var overwrite = MessageBox.Show(
                    $"Overwrite imported preset '{chosenName}'?",
                    "Import XML",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (overwrite != MessageBoxResult.Yes)
                    return;
            }

            var preset = BuildImportedPreset(chosenName, "xml", Path.GetFileName(ofd.FileName), ofd.FileName, xml,
                null, metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(preset);
            RefreshPresetManagerLists();
            SelectImportedPreset(preset.Name);
            QueueSavedToast("Imported preset saved");
            SetStatus($"Imported '{preset.Name}' with {preset.Values.Count} values.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"XML import failed: {ex.Message}", "Error");
        }
    }

    private void OnImportedPresetImportXml(object sender, RoutedEventArgs e)
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import playercamerapreset.xml as saved preset",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        try
        {
            string xml = File.ReadAllText(ofd.FileName);
            SaveImportedPresetFromXml("xml", Path.GetFileName(ofd.FileName), ofd.FileName, xml);
        }
        catch (Exception ex)
        {
            SetStatus($"XML import failed: {ex.Message}", "Error");
        }
    }

    private async void ImportFromPaz()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import 0.paz — extract camera XML",
            Filter = "PAZ files (*.paz)|*.paz|All files (*.*)|*.*",
            FileName = "0.paz"
        };
        if (ofd.ShowDialog(this) != true) return;

        string pazPath = ofd.FileName;
        string pamtPath;
        try
        {
            pamtPath = ResolvePamtPathForImportedPaz(pazPath);
        }
        catch (Exception ex)
        {
            SetStatus($"0.paz import failed: {ex.Message}", "Error");
            return;
        }

        SetGlobalBusy(true, "Decrypting 0.paz\u2026");
        try
        {
            string xml = await Task.Run(() => CameraMod.ReadXmlFromPaz(pazPath, pamtPath)).ConfigureAwait(true);
            SetGlobalBusy(false);

            var metaDlg = new ImportMetadataDialog(
                $"Importing PAZ: {Path.GetFileName(pazPath)}",
                "Imported Camera")
            { Owner = this };
            if (metaDlg.ShowDialog() != true) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                var overwrite = MessageBox.Show(
                    $"Overwrite imported preset '{chosenName}'?",
                    "Import 0.paz",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (overwrite != MessageBoxResult.Yes)
                    return;
            }

            var imported = BuildImportedPreset(chosenName, "paz", Path.GetFileName(pazPath), pazPath, xml, null,
                metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(imported);
            RefreshPresetManagerLists(preserveSelection: false);
            SelectImportedPreset(SanitizeFileStem(imported.Name));
            QueueSavedToast($"Imported '{imported.Name}'");
            SetStatus($"Imported 0.paz as '{imported.Name}'.", "Success");
        }
        catch (Exception ex)
        {
            SetGlobalBusy(false);
            SetStatus($"0.paz import failed: {ex.Message}", "Error");
        }
    }

    private void ImportUcmPreset()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import UCM Preset",
            Filter = "UCM Preset (*.ucmpreset)|*.ucmpreset|JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = ""
        };
        if (ofd.ShowDialog(this) != true) return;

        try
        {
            string json = File.ReadAllText(ofd.FileName);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("session_xml", out _) && !root.TryGetProperty("RawXml", out _))
            {
                SetStatus("This file doesn't appear to be a UCM preset (no session_xml found).", "Error");
                return;
            }

            string name = root.TryGetProperty("name", out var nv) ? nv.GetString() ?? "" : Path.GetFileNameWithoutExtension(ofd.FileName);
            string safeName = new string(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            string destPath = Path.Combine(MyPresetsDir, $"{safeName}.ucmpreset");

            if (File.Exists(destPath))
            {
                var overwrite = MessageBox.Show($"A preset named '{name}' already exists. Overwrite?",
                    "Import UCM Preset", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (overwrite != MessageBoxResult.Yes) return;
            }

            File.Copy(ofd.FileName, destPath, true);
            RefreshPresetManagerLists(preserveSelection: false);
            QueueSavedToast($"Imported '{name}'");
            SetStatus($"Imported UCM preset '{name}'.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Import failed: {ex.Message}", "Error");
        }
    }

    private void OnImportedPresetDelete(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null || item.KindId != "imported")
        {
            SetStatus("Select an imported preset first.", "TextSecondary");
            return;
        }

        string name = item.Name;
        var confirm = MessageBox.Show(
            $"Delete imported preset '{name}'?",
            "Delete Imported Preset",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            string path = ImportedPresetPath(name);
            if (File.Exists(path))
                File.Delete(path);

            _selectedImportedPreset = null;
            RefreshPresetManagerLists(preserveSelection: false);
            QueueSavedToast("Imported preset deleted");
            SetStatus($"Imported preset '{name}' deleted.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Delete failed: {ex.Message}", "Error");
        }
    }

    private void OnImportedPresetLoadIntoGodMode(object sender, RoutedEventArgs e)
    {
        var preset = RequireSelectedImportedPreset();
        if (preset == null)
            return;

        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus("Game folder not set. v3 needs the current game to rebuild this preset safely.", "Warn");
            return;
        }

        try
        {
            _sessionXml = BuildRebuiltXmlFromImportedPreset(preset);
            SwitchAppMode("expert", captureCurrent: false);
            MarkImportedPresetAsBuilt(preset, refreshPresetSidebar: false);
            QueueSavedToast("Imported preset loaded");
            SetStatus($"Loaded imported preset '{preset.Name}' into God Mode.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Load failed: {ex.Message}", "Error");
        }
    }

    private void OnImportedPresetGenerateJson(object sender, RoutedEventArgs e)
    {
        var preset = RequireSelectedImportedPreset();
        if (preset == null)
            return;

        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus("Game folder not set. v3 needs the current game to rebuild this preset safely.", "Warn");
            return;
        }

        string rebuiltXml;
        try
        {
            rebuiltXml = BuildRebuiltXmlFromImportedPreset(preset);
            _sessionXml = rebuiltXml;
            MarkImportedPresetAsBuilt(preset, refreshPresetSidebar: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Rebuild failed: {ex.Message}", "Error");
            return;
        }

        // Use rebuilt XML directly — CaptureSessionXml() would rebuild from God Mode UI and can diverge.
        var dlg = new ExportJsonDialog(_gameDir, () => rebuiltXml) { Owner = this };
        dlg.ShowDialog();
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

