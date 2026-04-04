# Catalog and Download System

This document covers the preset catalog infrastructure, download flow, update detection mechanism, session XML baking for UCM presets, and the `.catalog_state.json` sidecar format.

## Overview

UCM distributes presets through two GitHub-hosted catalogs:

| Source | Repository | Branch / Path | Catalog URL |
|--------|-----------|---------------|-------------|
| UCM Presets | `FitzDegenhub/UltimateCameraMod` | `v3-dev` branch, `/ucm_presets/` | `https://raw.githubusercontent.com/FitzDegenhub/UltimateCameraMod/v3-dev/ucm_presets/catalog.json` |
| Community Presets | `FitzDegenhub/ucm-community-presets` | `main` branch | `https://raw.githubusercontent.com/FitzDegenhub/ucm-community-presets/main/catalog.json` |

Both catalogs are fetched over HTTPS from GitHub's raw content CDN. The app uses `HttpClient` with a 10-second timeout and a `User-Agent` header of `UltimateCameraMod/3.0`.

## Catalog JSON Format

The catalog is a JSON object with a top-level `presets` array. Each entry in the array has the following shape:

```json
{
  "presets": [
    {
      "id": "panoramic-v2",
      "file": "panoramic-v2.ucmpreset",
      "name": "Panoramic v2",
      "author": "0xFitz",
      "description": "Wide cinematic camera with smooth transitions.",
      "url": "https://www.nexusmods.com/crimsondesert/mods/438",
      "version": "1.2",
      "sha256": "a1b2c3d4e5f6...",
      "size_bytes": 48320,
      "tags": ["cinematic", "wide", "steadycam"]
    }
  ]
}
```

### Field Reference

| Field | Type | Purpose |
|-------|------|---------|
| `id` | `string` | Unique identifier for the preset. Used as fallback filename for community presets (`{id}.ucmpreset`). |
| `file` | `string` | Filename of the preset file in the repository. UCM presets use this directly as the local filename. |
| `name` | `string` | Human-readable display name shown in the browser card UI. |
| `author` | `string` | Creator attribution displayed below the preset name. |
| `description` | `string` | Truncated to 200 characters in the card UI. Full text stored in the preset file itself. |
| `url` | `string` | Optional Nexus Mods or external link. If present, a "Nexus" button appears on the card. |
| `version` | `string` | Semantic version string. Currently informational only; not used for update comparison. |
| `sha256` | `string` | Lowercase hex SHA-256 hash of the preset file. The primary mechanism for update detection. |
| `size_bytes` | `long` | File size in bytes. Currently informational; the app enforces a separate 2 MB download cap. |
| `tags` | `string[]` | Tag labels rendered as pill badges on the card. Used for visual categorization, not filtering. |

### CatalogEntry Class

In `CommunityBrowserDialog.xaml.cs`, catalog entries are deserialized into a private sealed class:

```csharp
private sealed class CatalogEntry
{
    public string Id { get; set; } = "";
    public string File { get; set; } = "";
    public string Name { get; set; } = "";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    public string Url { get; set; } = "";
    public string Version { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public long SizeBytes { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}
```

Parsing uses `System.Text.Json.JsonDocument` with manual property extraction via `TryGetProperty`. This is deliberate: it makes the parser tolerant of missing fields and avoids exceptions when the catalog evolves.

## Local Directory Layout

Presets are stored in directories relative to the executable:

```
UltimateCameraMod.V3.exe
ucm_presets/
    catalog_state.json          <-- sidecar for update detection
    panoramic-v2.ucmpreset
    heroic.ucmpreset
    ...
my_presets/
    my-custom-camera.ucmpreset
community_presets/
    some-community-preset.ucmpreset
    ...
import_presets/
    ...
```

The directory constants are defined in `MainWindow.xaml.cs`:

```csharp
private const string UcmPresetsDirName = "ucm_presets";
private const string MyPresetsDirName = "my_presets";
private const string CommunityPresetsDirName = "community_presets";
private const string ImportPresetsDirName = "import_presets";
```

## Download Flow

### CommunityBrowserDialog

The `CommunityBrowserDialog` is a WPF `Window` that serves both UCM and community preset browsing. It is parameterized at construction time:

```csharp
public CommunityBrowserDialog(
    string presetsDir,
    Action onPresetsChanged,
    string catalogUrl = "https://raw.githubusercontent.com/FitzDegenhub/ucm-community-presets/main/catalog.json",
    string rawBaseUrl = "https://raw.githubusercontent.com/FitzDegenhub/ucm-community-presets/main/",
    string title = "Community Presets",
    bool needsSessionXmlBake = false)
```

When opened for UCM presets, the caller passes:
- `catalogUrl`: points to the `v3-dev` branch catalog
- `rawBaseUrl`: the raw GitHub base URL for that branch
- `needsSessionXmlBake: true`: signals that downloaded presets contain only style definitions and need session XML generation

When opened for community presets, defaults apply and `needsSessionXmlBake` is `false`.

### Download Sequence

1. **Fetch catalog**: `FetchCatalogAsync()` fires on `Loaded`. The raw JSON is fetched, parsed, and stored in `_catalog`.

2. **Render cards**: `RenderPresetList()` iterates the catalog entries. For each entry, `IsPresetDownloaded()` checks whether the file already exists on disk. Downloaded presets show a disabled "Downloaded" button; others show an active "Download" button.

3. **Download click**: `OnDownloadClick` handler:
   - Disables the button and shows "Downloading..."
   - Fetches raw bytes from `_rawBaseUrl + entry.File`
   - Enforces a **2 MB size cap** (`MaxPresetSize = 2 * 1024 * 1024`)
   - Validates the downloaded content is valid JSON containing either `session_xml`, `RawXml`, or `style_id`
   - Saves raw bytes to disk (not re-serialized, to preserve the exact SHA hash)
   - Updates `.catalog_state.json` with the entry's SHA-256 hash (UCM presets only)
   - Invokes `_onPresetsChanged` callback to refresh the sidebar

4. **Filename resolution**:
   - UCM presets (`needsSessionXmlBake == true`): use `entry.File` directly as the filename
   - Community presets with a `File` field: use `Path.GetFileName(entry.File)`
   - Fallback: `{entry.Id}.ucmpreset`

### Background Fetch on Launch

In addition to the browser dialog, UCM performs an automatic background fetch on every launch via `FetchUcmPresetsAsync()` in `MainWindow.Community.cs`. This method:

1. Fetches the UCM catalog
2. Iterates entries and checks whether each file exists locally
3. Downloads only missing presets (does not update existing ones)
4. Updates `.catalog_state.json` for each new download
5. Calls `RefreshPresetManagerLists` on the dispatcher to update the sidebar

Existing presets with SHA mismatches are handled separately by the update detection system (see below), not by this initial fetch.

## .catalog_state.json Format

The `.catalog_state.json` file is a simple JSON dictionary mapping filenames to their SHA-256 hashes. It lives inside the `ucm_presets/` directory.

```json
{
  "panoramic-v2.ucmpreset": "a1b2c3d4e5f6789...",
  "heroic.ucmpreset": "9f8e7d6c5b4a321..."
}
```

### Purpose

This sidecar file records the SHA-256 hash of each preset file at the time it was downloaded from the catalog. It serves as the "last known remote state" for update detection.

### Read / Write Operations

**Writing** (after download):

```csharp
string statePath = Path.Combine(_presetsDir, ".catalog_state.json");
var state = new Dictionary<string, string>();
if (File.Exists(statePath))
{
    string stateJson = File.ReadAllText(statePath);
    state = JsonSerializer.Deserialize<Dictionary<string, string>>(stateJson) ?? new();
}
state[destFileName] = entry.Sha256;
File.WriteAllText(statePath,
    JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
```

**Reading** (during update check): `ReadCatalogState()` deserializes the file into a `Dictionary<string, string>`.

### Important Detail: Raw Byte Preservation

Downloaded preset files are written to disk as raw bytes (`File.WriteAllBytesAsync`), not re-serialized from parsed JSON. This is critical because re-serialization would alter formatting, whitespace, and key ordering, which would produce a different SHA-256 hash and break update detection.

## Update Detection

UCM checks for preset updates at launch via two async methods:
- `CheckUcmPresetUpdatesAsync()` for UCM presets
- `CheckCommunityPresetUpdatesAsync()` for community presets

### UCM Preset Update Detection

The algorithm for UCM presets:

1. Fetch the remote catalog and build a `Dictionary<string, string>` of `filename -> sha256`
2. Read the local `.catalog_state.json` sidecar
3. For each preset in the sidebar:
   - Skip non-UCM presets and placeholders
   - Look up the filename in the sidecar. If there is no sidecar entry, skip (the preset was manually placed or pre-installed, not downloaded through the app)
   - Compare the sidecar's stored SHA against the catalog's SHA
   - If they differ, set `item.HasUpdate = true` on the `PresetManagerItem`

### Community Preset Update Detection

Community presets do not use the sidecar file. Instead, the local file's SHA-256 is computed on the fly:

1. Fetch the remote catalog and build a `Dictionary<string, string>` of `id -> sha256`
2. For each community preset in the sidebar:
   - Read the local file bytes and compute `SHA256.HashData(bytes)`
   - Compare against the catalog hash
   - If they differ, set `HasUpdate = true`

### Update UI Indicator

When `HasUpdate` is `true` on a `PresetManagerItem`, the sidebar displays a pulsating update icon next to the preset name. Clicking this icon triggers `OnPresetUpdateClick`.

### Update Prompt and Duplication

`OnPresetUpdateClick` presents a three-way dialog:

1. **Yes**: Duplicate the current version to `my_presets/` with `_old` suffix before downloading the update
2. **No**: Overwrite directly without saving the old version
3. **Cancel**: Abort the update

The duplication logic:
```csharp
string dupName = $"{item.Name}_old";
string dupPath = Path.Combine(MyPresetsDir, $"{SanitizeFileStem(dupName)}.ucmpreset");
// Read existing file, change name/kind/locked, write to my_presets
```

After downloading the update:
- UCM presets: raw bytes are written and the sidecar is updated
- Community presets: the file is rebuilt with metadata (name, author, description, url) preserved from the existing file where the download lacks those fields

After update, both `CheckUcmPresetUpdatesAsync` and `CheckCommunityPresetUpdatesAsync` are re-invoked to refresh update icons on other presets.

## Session XML Baking

UCM presets and community presets differ fundamentally in what they contain:

| Preset Type | Contains `session_xml`? | Needs Baking? |
|-------------|------------------------|---------------|
| UCM Preset | No (style definition only) | Yes |
| Community Preset | Yes (fully baked) | No |

### What is Session XML?

The `session_xml` is the complete, modified `playercamerapreset.xml` content that gets compressed and written into the game's `0.paz` archive. It is the final output of the camera modification pipeline.

### Baking Process for UCM Presets

UCM preset files in the repository contain a style definition (parameters like `style_id`, distance, height, offset values) but no `session_xml`. The session XML must be generated locally because it depends on the user's game installation:

1. **Read vanilla XML**: `CameraMod.ReadVanillaXml(gameDir)` extracts the unmodified camera XML from the game's PAZ archive or backup
2. **Apply CameraRules**: The style definition and user settings are passed through `CameraRules.BuildModifications()` to produce a `ModificationSet`
3. **Generate modified XML**: `CameraMod.ApplyModifications(vanillaXml, modSet)` produces the final session XML
4. **Store**: The session XML is saved into the preset file's `session_xml` field

This baking is performed by `GenerateBuiltInPresets()`, which is called:
- After the game directory is detected
- After new UCM presets are downloaded via the browser dialog

### Why Community Presets Skip Baking

Community presets are authored by third parties who have already generated the full session XML against a specific game version. The preset file shipped in the community repository contains the complete `session_xml`, so it can be used directly without access to the user's vanilla game files. This also means community presets are tied to a specific game version and may need updates when the game patches.

### The `needsSessionXmlBake` Flag

The `CommunityBrowserDialog` constructor accepts a `needsSessionXmlBake` boolean. When `true`:
- The header subtitle changes to mention "official UCM camera presets"
- Downloaded files use `entry.File` directly as the local filename (not just `Path.GetFileName`)
- The main window's `onPresetsChanged` callback triggers `GenerateBuiltInPresets()` in addition to `RefreshPresetManagerLists`

## Error Handling

The download system is designed to fail gracefully:

- Network timeouts (10s for catalog, 15s for individual downloads) prevent the UI from hanging
- Individual download failures are caught and shown on the button ("Failed") with a status bar message
- Catalog fetch failures show a full error panel with a "Retry" button
- Background fetch (`FetchUcmPresetsAsync`) catches all exceptions silently to avoid interrupting startup
- Invalid preset content (missing `session_xml`, `RawXml`, or `style_id`) is rejected with an "Invalid" label

## Relevant Source Files

| File | Role |
|------|------|
| `src/UltimateCameraMod.V3/CommunityBrowserDialog.xaml.cs` | Catalog browser dialog, download handler, card rendering |
| `src/UltimateCameraMod.V3/MainWindow.Community.cs` | Background fetch, GitHub version check, update detection, update download |
| `src/UltimateCameraMod.V3/MainWindow.Presets.cs` | `BuildPresetManagerItems`, sidebar list management, `ReadCatalogState` |
| `src/UltimateCameraMod.V3/MainWindow.xaml.cs` | Directory constants, catalog URL constants |
| `src/UltimateCameraMod.V3/Models/PresetManagerItem.cs` | `HasUpdate` property binding for sidebar update icons |
