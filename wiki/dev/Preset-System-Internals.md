# Preset System Internals

UCM's preset system stores camera configurations as `.ucmpreset` files (JSON format) organized into directories by kind. This page covers the file format, preset kinds, directory layout, catalog state tracking, and legacy migration.

---

## The .ucmpreset File Format

A `.ucmpreset` file is a JSON document with the following top-level fields:

```json
{
  "name": "My Camera",
  "author": "PlayerName",
  "description": "A custom over-the-shoulder camera with cinematic framing.",
  "kind": "user",
  "locked": false,
  "preset_mode": "ucm",
  "style_id": "custom",
  "url": "",
  "settings": {
    "distance": 5.0,
    "height": -0.3,
    "right_offset": -0.4,
    "fov": 20,
    "combat_pullback": 0.0,
    "centered": false,
    "mount_height": true,
    "steadycam": true
  },
  "session_xml": "..."
}
```

### Field Reference

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Display name shown in the sidebar |
| `author` | string | No | Creator name, shown in sidebar subtitle |
| `description` | string | No | Short description, shown in preset status text |
| `kind` | string | Yes | Preset category: `"default"`, `"style"`, `"community"`, `"user"`, `"imported"` |
| `locked` | bool | No | Whether the preset is read-only in the editor. UCM styles are always locked. |
| `preset_mode` | string | No | `"ucm"` (Managed by UCM) or `"manual"` (Full Manual Control). Defaults to `"ucm"` if absent. |
| `style_id` | string | No | Which UCM style this preset is based on: `"heroic"`, `"panoramic"`, `"default"`, `"close-up"`, `"low-rider"`, `"knee-cam"`, `"dirt-cam"`, `"survival"`, or `"custom"` |
| `url` | string | No | External link (Nexus page, GitHub, etc.). Shown as "View on Nexus" button. |
| `settings` | object | No | UCM Quick slider values. Only meaningful for `preset_mode: "ucm"`. |
| `session_xml` | string | No | Pre-baked camera XML. For definition-based presets, this may be absent and is baked on load from `style_id` + `settings` + current vanilla XML. |

### Settings Object

| Field | Type | Range | Default | Description |
|-------|------|-------|---------|-------------|
| `distance` | float | 1.5 - 12.0 | 5.0 | Camera distance from character |
| `height` | float | -1.6 - 1.5 | 0.0 | Camera height offset (negative = lower) |
| `right_offset` | float | -3.0 - 3.0 | 0.0 | Horizontal shift delta |
| `fov` | int | 0 - 40 | 0 | Additive FoV delta in degrees |
| `combat_pullback` | float | -0.6 - 0.6 | 0.0 | Lock-on zoom offset (proportional) |
| `centered` | bool | - | false | Centered camera (RightOffset=0 everywhere) |
| `mount_height` | bool | - | true | Match mount camera height to player |
| `steadycam` | bool | - | true | Enable Steadycam smoothing |

### Header-Only Reading

When building the sidebar preset list, UCM does NOT parse the full file (which can be large due to `session_xml`). Instead, it reads only the first 4KB to extract metadata fields:

```csharp
using (var reader = new StreamReader(file))
{
    var buf = new char[4096];
    int read = reader.Read(buf, 0, buf.Length);
    header = new string(buf, 0, read);
}
string name = ExtractJsonStringField(header, "name");
```

This is why metadata fields (`name`, `author`, `description`, `kind`, `locked`, `url`) are always written at the top of the JSON file, before `settings` and `session_xml`.

---

## Preset Kinds

Each preset has a `kind` that determines its sidebar group, default lock state, and behavior.

| Kind | Sidebar Group | Default Locked | Source | Description |
|------|--------------|----------------|--------|-------------|
| `default` | Game Default | Yes | Built-in | Vanilla camera, decoded from game backup |
| `style` | UCM Presets | Yes (always) | Downloaded via Browse | Official UCM style presets |
| `community` | Community Presets | Yes | Downloaded via Browse | Community-contributed presets |
| `user` | My Presets | No | Created by user | User-created presets |
| `imported` | Imported | No | Imported from file | Raw XML/PAZ/mod manager imports |

### Kind Labels in UI

The `kind` string maps to a display label:

```csharp
kind switch
{
    "default" => "Default",
    "style" => "UCM style",
    "community" => "Community",
    "imported" => "Imported",
    _ => "My preset"
}
```

### Placeholder Items

If no UCM presets have been downloaded yet, a hidden placeholder item with `IsPlaceholder = true` is added to ensure the "UCM Presets" group header (with its Browse button) still appears in the sidebar.

---

## Directory Layout

Presets are organized into directories next to the UCM executable:

```
UltimateCameraMod.exe
ucm_presets/
  Heroic.ucmpreset
  Panoramic.ucmpreset
  Close-Up.ucmpreset
  Low Rider.ucmpreset
  Knee Cam.ucmpreset
  Dirt Cam.ucmpreset
  Survival.ucmpreset
  catalog.json              (auto-generated catalog manifest)
  .catalog_state.json       (local SHA256 tracking, not committed)
my_presets/
  My Camera.ucmpreset
  Another Preset.ucmpreset
community_presets/
  SomeCreator_Camera.ucmpreset
import_presets/
  CrimsonCamera.ucmpreset   (imported from PAZ/XML/mod package)
backups/
  0.paz                     (vanilla backup)
```

Directories are created on demand when first accessed (using `Directory.CreateDirectory` in the property getter).

### File Discovery

`GetPresetFiles()` scans a directory for preset files. It looks for both `.ucmpreset` and `.json` files (for backwards compatibility with v2 presets).

---

## Catalog State Tracking

UCM tracks which version of each downloaded preset the user has locally using `.catalog_state.json`:

```json
{
  "Heroic.ucmpreset": "a1b2c3d4e5f6...",
  "Panoramic.ucmpreset": "f6e5d4c3b2a1...",
  "Close-Up.ucmpreset": "1234567890ab..."
}
```

Each entry maps a filename to its SHA256 hash. This file is updated after every download.

### Update Detection

On launch, UCM fetches the remote `catalog.json` and compares each entry's SHA256 against the local `.catalog_state.json`. If they differ, the preset is marked with `HasUpdate = true`, which triggers a pulsating update icon in the sidebar.

When the user clicks the update icon:
1. UCM offers to duplicate the old version to My Presets (so the user doesn't lose their current version)
2. Downloads the new version from GitHub
3. Updates `.catalog_state.json` with the new SHA256
4. Refreshes the sidebar

---

## Preset Modes

New in v3.1.0. When creating a new preset, the user chooses between two modes:

### Managed by UCM (`preset_mode: "ucm"`)

- UCM camera rules are applied (FoV normalization, Steadycam, lock-on scaling, etc.)
- All three editors are available (UCM Quick, Fine Tune, God Mode)
- Session XML is built from `style_id` + `settings` via `CameraRules.BuildModifications()`
- Some values are managed by UCM and can only be overridden in God Mode

### Full Manual Control (`preset_mode: "manual"`)

- No UCM camera rules are applied
- Only God Mode editing is available
- Session XML starts as vanilla camera XML
- All edits are raw XML attribute changes
- UCM Quick and Fine Tune tabs are disabled with an explanation dialog

### How Mode is Determined

The `preset_mode` field in the preset file determines the mode. If the field is absent (old presets), it defaults to `"ucm"`.

Raw imports (PAZ, XML, mod manager packages) are always treated as manual regardless of what the file says. The `_sessionIsRawImport` flag overrides the mode at runtime.

`.ucmpreset` imports retain their original `preset_mode` from the file.

---

## PresetManagerItem Model

The sidebar represents each preset with a `PresetManagerItem`:

```csharp
public class PresetManagerItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string KindId { get; set; }        // "default", "style", "community", "user", "imported"
    public string KindLabel { get; set; }      // Display name for grouping
    public string SourceLabel { get; set; }    // Author name
    public string StatusText { get; set; }     // Description
    public string SummaryText { get; set; }    // Longer description
    public string FilePath { get; set; }       // Full path to .ucmpreset file
    public bool IsLocked { get; set; }         // Padlock state
    public bool HasUpdate { get; set; }        // Update available from catalog
    public bool IsPlaceholder { get; set; }    // Hidden item for empty groups
    public string Url { get; set; }            // External link
    public bool CanRebuild { get; set; }       // Whether session XML can be re-baked
}
```

The sidebar uses `CollectionViewSource` with `PropertyGroupDescription("KindLabel")` to create collapsible groups. Each group gets a header with the group name and, for UCM/Community groups, a Browse button.

---

## Sidebar Group Ordering

Groups appear in a fixed order determined by the order items are added to the collection:

1. Game Default (the Vanilla preset)
2. UCM Presets (downloaded official styles)
3. Community Presets (downloaded community presets)
4. My Presets (user-created)
5. Imported (from files)

### BuildPresetManagerItems()

This method scans all preset directories and builds the complete list:

1. Scan `ucm_presets/` for style presets (kind override: null, locked: true)
2. Add placeholder if no UCM presets exist (ensures Browse button is visible)
3. Scan `community_presets/` (kind override: "community", locked: true)
4. Add placeholder if no community presets exist
5. Build the Vanilla preset from game backup (kind: "default", locked: true)
6. Scan `my_presets/` (kind override: null, locked: false)
7. Scan `import_presets/` (kind override: "imported", locked: false)

---

## Legacy Migration

UCM v3 migrates presets from older formats automatically on first launch.

### v2 Preset Migration

v2 stored presets as `.json` files in a `presets/` directory with a different JSON structure. On first launch:

1. Check if `presets/` directory exists and `my_presets/` does not
2. Move all `.json` files from `presets/` to `my_presets/`
3. Convert the JSON structure to `.ucmpreset` format
4. Set `_legacyPresetFoldersMigrated = true` to prevent re-running

### Imported Presets Migration

v2 stored imported presets in `imported_presets/`. On first launch:

1. Check if `imported_presets/` exists and `import_presets/` does not
2. Move all files to `import_presets/`
3. Set `_importPresetsFolderMigrated = true`

---

## Auto-Save

When the user modifies an unlocked preset via any editor:

1. The change updates `_sessionXml`
2. A debounce timer starts (500ms)
3. If no further changes within 500ms, the preset file is written to disk
4. A "Saved" toast appears briefly in the UI
5. If the write fails, an error toast appears instead

Auto-save writes the entire preset file, including updated `settings` (from Quick slider values) and `session_xml`.

UCM presets and locked presets are never auto-saved. Attempting to edit them shows a red "This preset is locked" toast.

---

## Preset Operations

### New

Opens `NewPresetDialog` with name input and preset type selection (UCM vs Manual). Creates a new `.ucmpreset` in `my_presets/` with default settings.

### Duplicate

Copies the selected preset to `my_presets/` with a new name. Removes the lock. The duplicate retains the original's `preset_mode`.

### Rename

Prompts for a new name via `InputDialog`. Renames the file on disk and updates the sidebar.

### Delete

Confirms with the user, then deletes the file from disk and removes it from the sidebar.

### Lock / Unlock

Toggles the `locked` field in the preset file. UCM style presets cannot be unlocked. The padlock icon in the sidebar updates immediately.
