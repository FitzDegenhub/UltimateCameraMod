# UI Architecture

This document covers the WPF structure, partial class organization, theme system, custom controls, tutorial overlay, sidebar grouping, and debounce patterns used in the UCM V3 application.

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Framework | .NET 6.0 (WPF, Windows Desktop) |
| UI toolkit | Windows Presentation Foundation (WPF) |
| Language | C# 10+ |
| Layout model | XAML declarative + code-behind procedural generation |
| Data binding | `CollectionViewSource`, `PropertyGroupDescription`, `ObservableCollection<T>` |
| Threading | `DispatcherTimer` for debouncing, `Task.Run` for background work, `Dispatcher.Invoke` for UI marshaling |

Note: The `.csproj` targets `net6.0-windows` and enables both `UseWPF` and `UseWindowsForms` (the latter for `FolderBrowserDialog` fallback).

## MainWindow Partial Class Organization

The `MainWindow` class is split across 8+ partial class files to keep concerns separated. All files declare `public partial class MainWindow : Window` in the `UltimateCameraMod.V3` namespace.

### File Breakdown

| File | Responsibilities | Key Members |
|------|-----------------|-------------|
| `MainWindow.xaml.cs` | Constructor, field declarations, initialization, game detection, window state, constants | `Ver`, `ExeDir`, `_gameDir`, `_sessionXml`, `_suppressEvents`, directory constants, URL constants, `PresetFileJsonOptions` |
| `MainWindow.Presets.cs` | `BuildPresetManagerItems`, sidebar list management, preset loading/saving/switching, auto-save | `BuildPresetManagerItems()`, `AppendSessionJsonPresetsFromDir()`, `RefreshPresetManagerLists()`, `ActivatePickerFromSelection()` |
| `MainWindow.Editors.cs` | Undo system, tab switching, mode navigation, session XML building, event handlers, preview sync | `UndoSnapshot`, `_undoStack`, `CaptureUndoSnapshot()`, `SwitchEditorTab()`, `SwitchAppMode()`, `CaptureSessionXml()`, `BuildSessionXmlForMode()`, `SyncPreview()` |
| `MainWindow.FineTune.cs` | Advanced Controls mode, programmatic slider generation (~150 sliders), search/filter, Steadycam lock | `_advCtrlSliders`, `EnterAdvancedControlsMode()`, `BuildAdvCtrlSection_OnFoot()`, `BuildAdvCtrlSection_Mount()`, `BuildAdvCtrlSection_Global()`, etc. |
| `MainWindow.GodMode.cs` | Expert/God Mode, DataGrid binding, section filtering, override persistence | `EnterExpertMode()`, `AdvBindGrid()`, `AdvLoadOverrides()`, `AdvPopulateFilter()`, `BuildExpertModSet()` |
| `MainWindow.Import.cs` | Import dialog routing, 4 import handlers, metadata dialog | Import handlers for XML, PAZ, JSON, UCM preset formats |
| `MainWindow.Export.cs` | Export dialog, Install to Game logic (background thread), restore, install trace | `OnExportJson()`, `OnInstall()`, `OnRestore()`, `ShowBannerFromState()` |
| `MainWindow.Community.cs` | Catalog browser, GitHub version check, update notifications, background preset fetch | `OnBrowsePresetCatalog()`, `CheckGitHubVersion()`, `FetchUcmPresetsAsync()`, `CheckUcmPresetUpdatesAsync()`, `CheckCommunityPresetUpdatesAsync()`, `OnPresetUpdateClick()` |
| `MainWindow.Taskbar.cs` | Taskbar icon management, shell property store integration | Taskbar icon and Windows shell integration |

### Shared State

Because all partial class files share the same class instance, they share state through private fields declared primarily in `MainWindow.xaml.cs`:

```csharp
private string _gameDir = "";
private string _detectedPlatform = "Unknown";
private string? _sessionXml;
private bool _suppressEvents;
private string _activeMode = "simple";
private bool _isExpertMode;
private bool _sessionIsFullPreset;
private bool _sessionIsRawImport;
private string _selectedStyleId = "panoramic";
private List<AdvancedRow> _advAllRows = new();
```

The `_suppressEvents` flag is used throughout all partial files to prevent cascading event handlers during programmatic UI updates. The pattern is:

```csharp
_suppressEvents = true;
try
{
    // Update UI controls without triggering change handlers
    DistSlider.Value = snap.Dist;
    HeightSlider.Value = snap.Height;
}
finally
{
    _suppressEvents = false;
}
```

### Mode State Machine

The app has three editor modes managed by `SwitchEditorTab(string tab)`:

```
"simple"   -> Quick tab (SimpleView visible)
"advanced" -> Fine Tune tab (AdvancedControlsView visible)
"expert"   -> God Mode tab (ExpertView visible)
```

Tab switching:
1. Hides all editor views (`Visibility.Collapsed`)
2. Updates tab button styles (gold accent for active, subtle for inactive)
3. Sets `_activeMode` and `_isExpertMode`
4. Shows the target view and enters the corresponding mode

Special rules:
- Raw XML imports (`_sessionIsRawImport`) are locked to God Mode only. Attempting to switch to Quick or Fine Tune shows an informational dialog.
- UCM presets (`IsUcmPreset`) prompt for duplication before entering Fine Tune or God Mode, as these tabs allow edits that could corrupt the preset's tuned values.
- `SwitchAppMode()` is a legacy compatibility shim that maps old mode names to the new 3-tab model.

## Undo System

The undo system is a simple snapshot stack defined in `MainWindow.Editors.cs`:

```csharp
private sealed record UndoSnapshot(
    double Dist, double Height, double HShift,
    int FovIdx, double CombatPb,
    bool Bane, bool Mount, bool Steadycam);

private readonly Stack<UndoSnapshot> _undoStack = new();
private const int MaxUndoDepth = 20;
```

`CaptureUndoSnapshot()` pushes the current Quick tab slider/checkbox state onto the stack before each change. When the stack exceeds 20 entries, it is trimmed by copying to an array, clearing, and re-pushing the most recent 20.

`OnUndo()` pops the top snapshot and restores all controls within a `_suppressEvents` guard to avoid triggering change handlers during the restore.

## Theme System

The dark theme is defined in `App.xaml` with named brush resources. The color palette:

| Resource Name | Hex Value | Usage |
|--------------|-----------|-------|
| Accent | `#D4A843` | Gold accent for active states, buttons, highlights |
| Background | `#1C1C1E` | Main window background |
| BgPanel | darker variant | Panel/card backgrounds |
| BgInput | darker variant | Input field backgrounds |
| TextPrimary | `#E8E8E8` | Primary text |
| TextSecondary | lighter gray | Secondary labels |
| TextDim | `#888888` | Disabled/dim text |
| Border | gray variant | Panel borders |
| Warn | yellow/orange | Warning indicators |
| Success | green | Success indicators |
| Error | red | Error indicators |

Button styles:
- `AccentButton`: Gold background (`#D4A843`) with dark text (`#1A1A1A`). Used for primary actions and active tab indicators.
- `SubtleButton`: Transparent background with light text (`#E0E0E0`). Used for secondary actions and inactive tabs.

Tab buttons dynamically swap between these styles:

```csharp
TabQuick.Style = tab == "simple" ? _accentButtonStyle : _subtleButtonStyle;
TabQuick.Foreground = tab == "simple" ? darkFg : brightFg;
```

Cached style references (`_accentButtonStyle`, `_subtleButtonStyle`, `_textSecondaryBrush`, etc.) are stored as fields to avoid repeated `FindResource` lookups during frequent operations like tab switching.

## Custom Controls

### CameraPreview

**File**: `src/UltimateCameraMod.V3/Controls/CameraPreview.cs`

A custom `Canvas` control that renders a top-down side-view visualization of the camera distance and height relative to the player character. Dimensions: 420x370 pixels.

Key visual elements:
- Ground plane with grid markers
- Character silhouette (gold accent color, `#C8A24E`)
- Camera icon with lens detail
- Distance measurement line with dashed guides
- Height offset visualization
- Sight line from camera to character (semi-transparent gold)
- Label text showing the current preset name

```csharp
public void UpdateParams(double dist, double up, string label = "Custom")
```

All elements are drawn procedurally (no XAML template). Brushes are frozen (`Freeze()`) for rendering performance. The control redraws completely on each `UpdateParams` call by clearing and repopulating `Children`.

### FovPreview

**File**: `src/UltimateCameraMod.V3/Controls/FovPreview.cs`

A custom `Canvas` control that renders a top-down field-of-view cone visualization. Dimensions: 420x370 pixels.

Shows:
- Camera position with directional cone
- Character position relative to the camera offset
- FoV cone lines with dashed borders
- Horizontal offset visualization
- Centered vs. offset comparison

```csharp
public void UpdateParams(int fovDelta, double roff, bool centered, double distance = 5.0)
```

Same rendering strategy as `CameraPreview`: frozen brushes, procedural drawing, complete redraw on parameter change.

### Preview Sync

Both controls are updated by `SyncPreview()` in `MainWindow.Editors.cs`:

```csharp
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
```

## Tutorial Overlay

**File**: `src/UltimateCameraMod.V3/TutorialOverlay.cs`

A Canvas-based spotlight system that guides new users through the UI. It overlays the entire window with a translucent dark mask and highlights specific UI elements with a cutout.

### Architecture

```csharp
public class TutorialOverlay : Canvas
{
    private readonly List<TutorialStep> _steps;
    private int _currentStep;
    private readonly Action _onComplete;
    private readonly Window _owner;
}
```

Each step is a record:

```csharp
public sealed record TutorialStep(
    string Title,
    string Description,
    Func<FrameworkElement?> TargetElement);
```

The `TargetElement` is a `Func<>` (not a direct reference) because the target element may not exist or may not be visible when the tutorial is constructed. The function is evaluated at each step.

### Rendering

For each step, `ShowStep()`:

1. Clears all children
2. Evaluates `step.TargetElement()` to get the target `FrameworkElement`
3. Computes the spotlight rectangle (target bounds + 10px padding on each side)
4. Creates a `CombinedGeometry` with `GeometryCombineMode.Exclude` to cut the spotlight out of the full-window dark rectangle
5. Draws the dark mask as a `Path` with `Color.FromArgb(0xCC, 0, 0, 0)` (80% opacity black)
6. Draws a gold border (`#C8A24E`) around the spotlight rectangle with 8px corner radius
7. Positions an info card (see below) near the spotlight

### Card Positioning

The info card is placed using a priority system:
1. **Below** the spotlight if there is room (spotlight bottom + 210px < window height)
2. **Above** the spotlight if there is room (spotlight top - 210px > 0)
3. **To the right** of the spotlight, vertically centered

All positions are clamped to keep the card within the window bounds.

### Info Card

The card is a `Border` (dark background `#242424`, gold border, 10px corner radius) containing:
- Step counter ("Step 1 of 5")
- Title in gold, 16pt bold
- Description in light text, 12pt
- "Next" / "Get Started" button (gold background)
- "Skip tutorial" button (transparent, gray text)

The overlay responds to window resizes by recalculating the spotlight position.

## Sidebar Grouping

The preset sidebar uses WPF's `CollectionViewSource` with `PropertyGroupDescription` to create collapsible groups.

### PresetManagerItem

Each sidebar entry is a `PresetManagerItem` with a `KindLabel` property that determines its group:

```csharp
KindLabel = kind switch
{
    "default" => "Default",
    "style" => "UCM style",
    "community" => "Community",
    "imported" => "Imported",
    _ => "My preset"
}
```

### Grouping Setup

```csharp
var view = CollectionViewSource.GetDefaultView(_presetManagerItems);
view.GroupDescriptions.Clear();
view.GroupDescriptions.Add(new PropertyGroupDescription("KindLabel"));
```

Each group header has a "Browse" button for catalog groups (UCM presets, Community presets). The `OnBrowsePresetCatalog` handler inspects the `CollectionViewGroup.Name` to determine which catalog to open.

### Placeholder Items

To ensure group headers always appear (even when empty), placeholder items are inserted:

```csharp
items.Add(new PresetManagerItem
{
    Name = "\0",        // sorts first, invisible
    KindId = "style",
    KindLabel = "UCM style",
    IsPlaceholder = true
});
```

Placeholder items have `IsPlaceholder = true` and are filtered out of interactive operations.

### Building the List

`BuildPresetManagerItems()` in `MainWindow.Presets.cs` constructs the full list by scanning each preset directory. For efficiency, only the first 4KB of each file is read to extract metadata:

```csharp
string header;
using (var reader = new StreamReader(file))
{
    var buf = new char[4096];
    int read = reader.Read(buf, 0, buf.Length);
    header = new string(buf, 0, read);
}
string name = ExtractJsonStringField(header, "name") ?? Path.GetFileNameWithoutExtension(file);
```

This avoids parsing the potentially large `session_xml` field just to populate the sidebar.

## Debounce Patterns

UCM uses `DispatcherTimer` instances for debouncing throughout the application. The general pattern:

```csharp
private DispatcherTimer? _myDebounceTimer;

private void ScheduleMyDebouncedAction()
{
    _myDebounceTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
    _myDebounceTimer.Stop();
    _myDebounceTimer.Tick -= OnMyDebounce;
    _myDebounceTimer.Tick += OnMyDebounce;
    _myDebounceTimer.Start();
}

private void OnMyDebounce(object? sender, EventArgs e)
{
    _myDebounceTimer?.Stop();
    ActualWork();
}
```

### Active Debounce Timers

| Timer | Interval | Purpose |
|-------|----------|---------|
| `_saveToastDelayTimer` | 500ms | Delay before showing the "Saved" toast notification |
| `_saveToastHideTimer` | ~2s | Auto-hide the save toast after display |
| `_installStateDebounceTimer` | varies | Debounce install state file writes |
| `_previewDebounceTimer` | varies | Batch camera preview redraws during slider dragging |
| `_syncEditorsDebounceTimer` | 300ms | Debounce session XML rebuild from Quick settings to Fine Tune / God Mode |
| `_advCtrlSearchDebounceTimer` | varies | Debounce search text filtering in Fine Tune |

### Coalesced Preview Sync

For preview updates, UCM uses a two-tier approach:

1. **Coalesced sync** (`ScheduleCoalescedPreviewSync`): Posts a single `Dispatcher.BeginInvoke` at `DispatcherPriority.Render`. A boolean flag `_previewSyncPosted` prevents duplicate posts. This batches multiple rapid changes into a single redraw at the next render pass.

2. **Debounced sync** (`ScheduleDebouncedPreviewSync`): Uses `_previewDebounceTimer` for a time-based delay. Falls back to coalesced sync if the timer is not initialized.

The coalesced approach is preferred for immediate visual feedback (e.g., checkbox changes), while the debounced approach is used for continuous input (e.g., slider dragging) to reduce CPU load.

### Quick Settings to Editors Sync

`ScheduleSyncQuickSettingsToEditors()` uses a 300ms debounce timer. When Quick tab sliders change, it waits for 300ms of inactivity before rebuilding the session XML and pushing it to Fine Tune and God Mode editors. This prevents expensive XML rebuilds on every slider tick during dragging.

The immediate version `SyncQuickSettingsToEditorsNow()` bypasses the timer and is called directly during tab switching.

## NewPresetDialog

`NewPresetDialog.xaml.cs` presents radio button cards for selecting between:
- **UCM Preset**: Uses UCM's camera rule system (style, FoV, Steadycam, etc.)
- **Manual Preset**: Raw XML editing in God Mode only

The dialog uses card-style radio buttons (visually distinct bordered panels) rather than standard WPF radio buttons.

## Session XML Flow

Understanding how `_sessionXml` flows through the system is critical:

```
Quick tab slider change
    |
    +-> OnSliderChanged / OnSettingChanged
    |       |
    |       +-> ScheduleSyncQuickSettingsToEditors (300ms debounce)
    |               |
    |               +-> BuildSimpleSessionXml()
    |                       |
    |                       +-> CameraMod.ReadVanillaXml(_gameDir)
    |                       +-> CameraRules.BuildModifications(...)
    |                       +-> CameraMod.ApplyModifications(vanillaXml, modSet)
    |                       +-> _sessionXml = result
    |
    +-> Tab switch to Fine Tune
    |       |
    |       +-> CaptureSessionXml()
    |       +-> EnterAdvancedControlsMode()
    |               |
    |               +-> ApplySessionXmlToAdvancedControls(xml)
    |
    +-> Tab switch to God Mode
    |       |
    |       +-> CaptureSessionXml()
    |       +-> EnterExpertMode()
    |               |
    |               +-> Parse vanilla + live XML into DataGrid rows
    |               +-> AdvLoadOverrides() (advanced_overrides.json)
    |
    +-> Install / Export
            |
            +-> BuildSessionXmlForMode(_activeMode)
            +-> _sessionXml used as the source of truth
```

For raw imports (`_sessionIsRawImport`), the session XML is never rebuilt through CameraRules. It is used as-is, with only God Mode edits layered on top.

For full preset loads (`_sessionIsFullPreset`), the `_sessionXml` is not overwritten by Quick slider changes. This prevents Fine Tune / God Mode values from being discarded when the user adjusts Quick sliders.

## Relevant Source Files

| File | Role |
|------|------|
| `src/UltimateCameraMod.V3/MainWindow.xaml` | XAML layout definition |
| `src/UltimateCameraMod.V3/MainWindow.xaml.cs` | Constructor, fields, initialization |
| `src/UltimateCameraMod.V3/MainWindow.Editors.cs` | Undo, tab switching, preview sync, event handlers |
| `src/UltimateCameraMod.V3/MainWindow.FineTune.cs` | Advanced Controls slider generation |
| `src/UltimateCameraMod.V3/MainWindow.GodMode.cs` | Expert mode DataGrid binding |
| `src/UltimateCameraMod.V3/MainWindow.Presets.cs` | Sidebar list building |
| `src/UltimateCameraMod.V3/MainWindow.Import.cs` | Import dialog handlers |
| `src/UltimateCameraMod.V3/MainWindow.Export.cs` | Export/install handlers |
| `src/UltimateCameraMod.V3/MainWindow.Community.cs` | Catalog browser, version check |
| `src/UltimateCameraMod.V3/MainWindow.Taskbar.cs` | Taskbar integration |
| `src/UltimateCameraMod.V3/TutorialOverlay.cs` | Tutorial spotlight system |
| `src/UltimateCameraMod.V3/Controls/CameraPreview.cs` | Distance/height visualization |
| `src/UltimateCameraMod.V3/Controls/FovPreview.cs` | FoV cone visualization |
| `src/UltimateCameraMod.V3/NewPresetDialog.xaml.cs` | New preset type selection |
| `src/UltimateCameraMod.V3/App.xaml` | Theme definitions, global styles |
| `src/UltimateCameraMod.V3/App.xaml.cs` | Application startup |
