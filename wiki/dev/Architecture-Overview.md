# Architecture Overview

This document describes the high-level architecture of Ultimate Camera Mod (UCM), a C# WPF desktop application that modifies camera settings in Crimson Desert by patching the game's PAZ archive files.

## Four-Layer Architecture

UCM is organized into four distinct layers, each with clear responsibilities:

| Layer | Namespace / Location | Responsibility |
|-------|---------------------|----------------|
| **PAZ Layer** | `UltimateCameraMod.Paz` | Read/write the game's encrypted, compressed PAZ archive format |
| **Camera Rules Layer** | `UltimateCameraMod.Models.CameraRules` | Produce a `ModificationSet` (dictionary of XML attribute patches) from user settings |
| **Camera Mod Service** | `UltimateCameraMod.Services.CameraMod` | Apply modifications to vanilla XML, handle backup/restore, install pipeline |
| **UI Layer** | `UltimateCameraMod.V3` | WPF application with three editing modes and preset management |

```
+-----------------------------------------------------------------------+
|                           UI Layer (V3)                                |
|  MainWindow.xaml.cs  |  .Presets.cs  |  .Editors.cs  |  .Export.cs   |
|  .FineTune.cs  |  .GodMode.cs  |  .Import.cs  |  .Community.cs      |
+-----------------------------------------------------------------------+
         |                      |                       |
         | user settings        | session XML           | install/export
         v                      v                       v
+------------------------+  +---------------------------+
| Camera Rules Layer     |  | Camera Mod Service        |
| CameraRules.cs         |  | CameraMod.cs              |
| -> ModificationSet     |  | -> ApplyModifications()   |
|    (XML patches)       |  | -> InstallRawXml()        |
+------------------------+  | -> ParseXmlToRows()       |
                            +---------------------------+
                                        |
                                        | read/write encrypted archives
                                        v
                            +---------------------------+
                            | PAZ Layer                 |
                            | PamtReader  | AssetCodec  |
                            | ArchiveWriter | LZ4/ChaCha|
                            +---------------------------+
                                        |
                                        v
                            +---------------------------+
                            | Game Files (0010/0.paz)   |
                            | playercamerapreset.xml    |
                            +---------------------------+
```

## Data Flow: User Input to Game File Modification

The full pipeline from user interaction to modified game file follows this path:

```
User moves slider (UCM Quick)
    |
    v
CameraRules.RegisterCustomStyle(distance, height, rightOffset)
CameraRules.BuildModifications(style, fov, bane, combatPullback, mountHeight, steadycam)
    |
    v
ModificationSet  { ElementMods: Dict<string, Dict<string, (Action, Value)>>, FovValue: int }
    |
    v
CameraMod.ReadVanillaXml(gameDir)      -- reads backup from backups/original_backup.bin
    |                                      decrypts with AssetCodec (ChaCha20)
    |                                      decompresses with LZ4
    |                                      strips XML comments
    v
CameraMod.ApplyModifications(vanillaXml, modSet)
    |    walks XML line by line
    |    tracks depth stack for section context
    |    applies SET/REMOVE operations per element key
    |    adds FoV delta to all Player_, Cinematic_, Glide_ sections
    |    auto-injects missing ZoomLevel nodes for ZL3/ZL4
    v
Session XML string  (_sessionXml)       -- single source of truth
    |
    v  [on Install]
CameraMod.InstallRawXml(gameDir, xmlText)
    |
    +-- StripComments(xmlText)
    +-- UTF8 encode
    +-- ArchiveWriter.MatchCompressedSize(bytes, compSize, origSize)
    |       binary-search XML comment padding to hit exact compressed size
    +-- CompressionUtils.Lz4Compress(matched)
    +-- AssetCodec.Encode(compressed, filename)     -- ChaCha20 encrypt
    +-- ArchiveWriter.UpdateEntry(entry, encoded)   -- write to 0.paz at offset
    +-- Restore file timestamps on 0.paz
```

## Project Layout

### Shared Library: `src/UltimateCameraMod/`

This project targets netstandard-compatible code and contains no UI dependencies. It can be referenced by any .NET application (CLI tools, test harnesses, etc.).

#### Models/

| File | Purpose |
|------|---------|
| `CameraRules.cs` | Camera modification rule engine. Contains all section lists (BasicSections, WeaponSections, AllMain, LockOnSections, HorseRideSections, AllMountSections, BaneSections), all style builders (Heroic, Panoramic, CloseUp, LowVariant, Survival, Custom), and the layered composition system (`BuildModifications`). Produces a `ModificationSet`. |
| `AdvancedRow.cs` | Data model for God Mode DataGrid rows. Implements `INotifyPropertyChanged`. Each row represents one XML attribute with `Section`, `SubElement`, `Attribute`, `VanillaValue`, `Value`. Computed properties: `IsModified` (Value != VanillaValue), `ModKey` (Section/SubElement path), `FullKey` (ModKey.Attribute). |
| `CameraParamDocs.cs` | Tooltip and documentation strings for camera parameters. Provides `Get(string attribute)` for attribute-level help text in the UI. |
| `PresetCodec.cs` | Base64 `UCM:` prefix import/export for legacy v2 preset strings. Encodes name, distance, height, rightOffset to URL-safe Base64 JSON. Decodes with clamping: distance [1.5, 12.0], height [-1.6, 0.5], rightOffset [-3.0, 3.0]. |

#### Services/

| File | Purpose |
|------|---------|
| `CameraMod.cs` | The XML modification engine. Core methods: `ApplyModifications()` (line-by-line XML patching), `ParseXmlToRows()` (XML to AdvancedRow list), `ReadVanillaXml()` / `ReadLiveXml()` (read from backup or live PAZ), `InstallRawXml()` (full install pipeline), `ValidateVanilla()` (5 signature checks for tainted backups). Also handles backup management with `EnsureBackup()` and `RefreshVanillaBackupFromLivePaz()`. |
| `GameDetector.cs` | Auto-detect game directory by scanning Steam library folders (via `libraryfolders.vdf`), Epic Games manifests, and brute-force drive scanning. Returns the path to the Crimson Desert install root. |
| `JsonModExporter.cs` | Binary diff engine for CD JSON Mod Manager export. Compares vanilla encrypted bytes against modified encrypted bytes to produce a JSON patch file with byte-level offsets. |
| `GameInstallBaselineTracker.cs` | Detects game patches by monitoring Steam's `appmanifest_*.acf` file. When the game updates, triggers a vanilla backup refresh so UCM's baseline stays in sync. |

#### Paz/

| File | Purpose |
|------|---------|
| `PamtReader.cs` | Parses `.pamt` index files to discover file entries within `.paz` archives. Walks the PAMT binary structure: magic bytes, PAZ count, hash table, folder section (root prefix), node section (path tree with parent references), and record section (20-byte file entries with offset/compSize/origSize/flags). |
| `PazEntry.cs` | Data model for a single PAZ file entry. Properties: `Path`, `PazFile`, `Offset`, `CompSize`, `OrigSize`, `Flags`, `PazIndex`. Computed: `Compressed` (CompSize != OrigSize), `CompressionType` (flags bits 16-19), `IsXml`. |
| `ArchiveWriter.cs` | PAZ write-back engine. Core method `MatchCompressedSize()` adjusts plaintext so it compresses to an exact target size via multi-strategy XML comment padding. Preserves file timestamps on `.paz` after writing to avoid triggering game integrity checks. |
| `AssetCodec.cs` | ChaCha20 key derivation and encode/decode. Derives a 256-bit key and 128-bit nonce from a filename hash using `NameHasher.ComputeHash()` with seed `0x000C5EDE`, then XOR-based key expansion with 8 delta constants. `Encode` and `Decode` are the same operation (XOR cipher). |
| `StreamTransform.cs` | Pure C# ChaCha20 stream cipher (RFC 7539). 20-round quarter-round function, 256-bit key, 128-bit nonce (first 4 bytes used as initial counter). No external crypto dependency. Constants: `0x61707865`, `0x3320646e`, `0x79622d32`, `0x6b206574` ("expand 32-byte k"). |
| `CompressionUtils.cs` | LZ4 block compression/decompression using the `K4os.Compression.LZ4` NuGet package. No frame header (raw block format matching the game's PAZ entries). |
| `NameHasher.cs` | Lookup3 hash function (Bob Jenkins). Returns the primary hash value `c`. Used for filename-based key derivation in `AssetCodec.BuildParameters()`. Init value `0xDEADBEEF + length + initval`, mix rounds of 12 bytes, final avalanche. |

### WPF Application: `src/UltimateCameraMod.V3/`

The v3 application is a WPF project with a partial-class MainWindow design, where each partial class file handles a specific concern.

#### MainWindow Partial Classes

| File | Purpose |
|------|---------|
| `MainWindow.xaml` | XAML layout: sidebar with preset list, 3-tab editor area (UCM Quick / Fine Tune / God Mode), preview controls, status bar. |
| `MainWindow.xaml.cs` | Constructor, field declarations, initialization. Sets `InvariantCulture` globally to prevent European locale decimal separator issues. Directory path constants: `UcmPresetsDir` ("ucm_presets"), `MyPresetsDir` ("my_presets"), `CommunityPresetsDir` ("community_presets"), `ImportedPresetsDir` ("import_presets"). |
| `MainWindow.Presets.cs` | Preset manager: `BuildPresetManagerItems()` scans directories to build the sidebar list. `ActivatePickerFromSelection()` loads a preset's settings and session_xml into the UI. `RefreshPresetManagerLists()` handles catalog fingerprinting to skip redundant rebuilds. Preset saving, renaming, deleting, duplicating. Legacy migration from v2 `.json` to v3 `.ucmpreset`. |
| `MainWindow.Editors.cs` | Tab switching logic (`SwitchEditorTab`), undo system (stack of `UndoSnapshot` records, max depth 20), `CaptureSessionXml()` (rebuilds `_sessionXml` from current editor), `BuildSessionXmlForMode()` (dispatches to `BuildCuratedSessionXml` or `BuildGodModeSessionXml`), `BuildCurrentSimpleModSet()`, `BuildExpertModSet()`, preview sync. |
| `MainWindow.FineTune.cs` | Fine Tune tab: builds curated slider sections (On-Foot, Mount, Global, Special Mounts, Combat, Smoothing, Aim). Each slider maps to a `ModKey.Attribute` key. Reads session XML to populate sliders; writes slider changes back as a `ModificationSet` overlay. Steadycam keys are locked (non-editable) when Steadycam is enabled. |
| `MainWindow.GodMode.cs` | God Mode tab: `EnterExpertMode()` parses vanilla XML into `AdvancedRow` list, overlays session XML values, loads `advanced_overrides.json` file overlay. DataGrid with `CollectionViewSource` grouping by Section. Filter combo (All, Modified only, or by section prefix). |
| `MainWindow.Import.cs` | Import dialog handlers for mod packages, raw XML files, `.paz` archives, `.ucmpreset` files, and legacy `UCM:` base64 strings. Builds `ImportedPreset` objects stored in `import_presets/` as JSON. |
| `MainWindow.Export.cs` | Export and Install logic. Install calls `CameraMod.InstallRawXml()`. Export supports multiple formats: raw XML, patched `.paz` copy, JSON Mod Manager patches. |
| `MainWindow.Community.cs` | Catalog browser for UCM and community presets. Downloads preset definition files from GitHub. `GenerateBuiltInPresets()` bakes session_xml for each UCM style preset. |
| `MainWindow.Taskbar.cs` | Windows taskbar integration (jump list, progress bar). |

#### Controls/

| File | Purpose |
|------|---------|
| `CameraPreview.cs` | Custom WPF Canvas control rendering a top-down camera distance preview. Shows player position, camera distance arc, and height offset. |
| `FovPreview.cs` | Custom WPF Canvas control rendering a field-of-view cone visualization. Shows FoV angle, horizontal shift, centered mode indicator. |

#### Dialogs

| File | Purpose |
|------|---------|
| `NewPresetDialog.xaml/.cs` | New preset creation with type selection (UCM Quick or God Mode). |
| `ImportPresetDialog.xaml/.cs` | Import source selection (file, paste, URL). |
| `ImportMetadataDialog.xaml/.cs` | Review imported preset metadata before saving. |
| `ExportDialog.xaml/.cs` | Multi-format export (XML, PAZ, .ucmpreset). |
| `ExportJsonDialog.xaml/.cs` | CD JSON Mod Manager export with binary diff. |
| `CommunityBrowserDialog.xaml/.cs` | Browse and download UCM/community presets from catalog. |

#### Other Files

| File | Purpose |
|------|---------|
| `TutorialOverlay.cs` | First-run tutorial spotlight system. Highlights UI elements sequentially. |
| `CameraSessionState.cs` | Session state tracking for the current editing session. |
| `ShellTaskbarPropertyStore.cs` | Native Windows taskbar icon management via COM interop. |
| `ApplicationIdentity.cs` | App identity constants and version info. |
| `Models/PresetManagerItem.cs` | Sidebar preset list item model. Properties: Name, KindId, KindLabel, SourceLabel, StatusText, FilePath, IsLocked, Url, IsPlaceholder. Computed: GroupLabel, IsUcmPreset. |
| `Models/ImportedPreset.cs` | Model for imported preset metadata (name, author, source path, fingerprint, locked state, raw XML). |

## Key Design Decisions

### InvariantCulture Enforcement

The `MainWindow` constructor forces `CultureInfo.InvariantCulture` globally. Without this, systems using European locales (German, French, etc.) write decimal numbers with commas instead of dots (e.g., `3,4` instead of `3.4`), which produces invalid XML attribute values that crash the game on load.

### Session XML as Single Source of Truth

The `_sessionXml` string field in MainWindow is the single source of truth for the current camera configuration. All three editors read from and write to this field:

- **UCM Quick**: Rebuilds `_sessionXml` from scratch via `CameraRules.BuildModifications()` + `CameraMod.ApplyModifications()` on vanilla XML.
- **Fine Tune**: Reads `_sessionXml` to populate sliders; writes slider changes as an overlay `ModificationSet` applied on top of the Quick-generated XML.
- **God Mode**: Reads `_sessionXml` to populate the DataGrid; writes cell edits back via `BuildExpertModSet()`.

When switching tabs, `CaptureSessionXml()` rebuilds `_sessionXml` from the current editor so the next editor sees the latest state. For raw XML imports, `_sessionIsRawImport` prevents rebuilding through CameraRules (which would overwrite the imported values).

### Timestamp Preservation

`ArchiveWriter.SaveTimestamps()` captures creation, access, and modification timestamps on `0.paz` before writing, then restores them after. This prevents the game's integrity checker or Steam from detecting the file was modified by timestamp alone.

### SilenceKill Section Safety

The `Player_SilenceKill` and `Player_SilenceKill_Back` sections must never be modified. Changing any attribute on these sections (ZoomDistance, MaxZoomDistance, or CameraBlendParameter) causes an immediate game crash on load. This was confirmed via bisect testing. The root cause is unknown but likely related to special engine handling of stealth finisher cameras. These sections are excluded from all modification code paths.

### Additive FoV

FoV is applied additively: the user's FoV delta (e.g., +20) is added on top of each section's current FoV value after all other modifications. This preserves intentional per-section FoV differences while giving the user global control. The additive logic runs inside `ApplyModifications()` for every `Player_`, `Cinematic_`, and `Glide_` prefixed section.
