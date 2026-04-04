# Session XML Pipeline

The session XML is the single source of truth for the camera configuration in UCM. It is a complete `playercamerapreset.xml` string that represents the user's current camera settings. All three editors (UCM Quick, Fine Tune, God Mode) read from and write to this shared session XML.

This page explains how session XML is built, how it flows between editors, and how presets populate it.

---

## What is Session XML?

Session XML is a full copy of the game's `playercamerapreset.xml` with the user's modifications applied. It lives in memory as the `_sessionXml` string field on `MainWindow`.

When the user clicks "Install to Game", this XML is what gets size-matched, compressed, encrypted, and written into the PAZ archive. When the user exports, this XML is the source for all export formats.

---

## Data Flow Diagram

```
User moves Quick sliders
        |
        v
CameraRules.BuildModifications()
  --> produces ModificationSet (attribute patches + FoV delta)
        |
        v
CameraMod.ApplyModifications(vanillaXml, modSet)
  --> applies patches to vanilla XML
        |
        v
    _sessionXml  <--- shared by all editors
        |
   +---------+---------+
   |         |         |
   v         v         v
UCM Quick  Fine Tune  God Mode
(sliders)  (sliders)  (DataGrid)
   |         |         |
   +----+----+----+----+
        |
        v
  Install / Export
```

---

## Building Session XML from UCM Quick

When the user adjusts sliders on the UCM Quick tab, the following happens:

### Step 1: Gather Settings

The current values of all Quick controls are read:
- Style ID (from dropdown or "custom" if sliders have been manually adjusted)
- Distance, Height, Horizontal Shift (from sliders)
- FoV delta (from dropdown)
- Centered Camera, Mount Camera, Steadycam (from checkboxes)
- Lock-on Zoom (from slider)

### Step 2: Build ModificationSet

`CameraRules.BuildModifications()` is called with these settings. It runs through the layered composition system (see [Camera Rules Engine](Camera-Rules-Engine.md)):

1. `BuildSharedBase()` - FoV normalization, base distances
2. `BuildSmoothing()` - Steadycam blend smoothing (if enabled)
3. `BuildSharedSteadycam()` - Mount height zeroing (if enabled)
4. `BuildLockOnDistances()` - Seed lock-on with default distances
5. Style builder (e.g., `BuildCustom()`) - Apply style's distance/height/offset
6. `BuildBaneMods()` - Centered camera (if enabled)
7. `BuildCombatPullback()` - Lock-on zoom offset (if non-zero)
8. `BuildMountHeightMods()` - Mount height sync (if enabled)

The result is a `ModificationSet` containing all XML attribute patches and the FoV delta.

### Step 3: Apply to Vanilla XML

`CameraMod.ApplyModifications(vanillaXml, modSet)` takes the vanilla camera XML (read from the backup or game files) and applies the patches line by line. The result is the session XML.

### Step 4: Store

The result is stored in `_sessionXml`. The preview controls (CameraPreview, FovPreview) update to reflect the new values. If auto-save is enabled and the preset is unlocked, the preset file is written to disk (debounced by 500ms).

---

## Fine Tune: Reading and Writing Session XML

When the user switches to the Fine Tune tab, `EnterAdvancedControlsMode()` runs:

### Reading

1. If slider controls haven't been built yet, build them (about 150 sliders organized into card sections: On Foot, Mount, Global, Special Mounts, Combat, Smoothing, Aim)
2. Read `_sessionXml` (or build it from Quick settings if null)
3. Parse the XML to extract current values for each slider
4. Also load vanilla values for comparison display
5. Apply Steadycam lock state: sliders controlled by Steadycam are greyed out with a tooltip

Each slider maps to a specific XML path like `"Player_Basic_Default/ZoomLevel[2]"` and attribute like `"ZoomDistance"`. The slider's current value comes from the session XML, and the vanilla comparison value comes from the vanilla XML.

### Writing

When the user moves a Fine Tune slider:

1. The new value is written directly into `_sessionXml` by finding and replacing the attribute value at the correct XML path
2. Lock-on distances are re-synced: the current on-foot ZL2/ZL3/ZL4 values are re-read from the session XML, and `BuildLockOnDistancesPublic()` is called to update all lock-on sections
3. The preview controls update
4. Auto-save triggers (debounced)

Fine Tune edits do NOT go back through `CameraRules.BuildModifications()`. They modify the session XML directly. This means Fine Tune edits can override values that CameraRules set, giving the user finer control.

### Steadycam Interaction

When Steadycam is enabled, certain Fine Tune sliders are locked. The set of locked keys is determined by `_steadycamKeys`, which contains all the XML paths that `BuildSmoothing()` and `BuildSharedSteadycam()` would modify. Locked sliders:
- Are visually greyed out
- Show "Controlled by Steadycam" tooltip on hover (using `ToolTipService.ShowOnDisabled`)
- Cannot be moved by the user

When the user turns Steadycam off, these sliders unlock and their values revert to whatever the session XML currently has (which may differ from the Steadycam-smoothed values).

---

## God Mode: Reading and Writing Session XML

When the user switches to the God Mode tab, `EnterExpertMode()` runs:

### Reading

1. Load vanilla XML and parse it into `AdvancedRow` objects (one per attribute per section)
2. Filter to only `Player_` sections (God Mode doesn't show non-player camera sections)
3. Load the current session XML and overlay its values onto the rows
4. Load `advanced_overrides.json` and apply any saved God Mode edits
5. Bind the rows to the DataGrid with grouping by section name

Each row shows: Section, SubElement (e.g., ZoomLevel[2], CameraBlendParameter), Attribute, Vanilla Value, Current Value. Modified values are highlighted.

### Writing

When the user edits a cell in the God Mode DataGrid:

1. The new value is written to the row's `Value` property
2. The edit is saved to `advanced_overrides.json` (a JSON file mapping `FullKey` to value)
3. The session XML is rebuilt to include this override
4. Lock-on distances are re-synced

### advanced_overrides.json

God Mode edits are persisted separately from presets in `advanced_overrides.json`. This file is:
- Written after every God Mode cell edit
- Loaded when entering God Mode
- **Cleared when loading a new preset** to prevent stale overrides from bleeding into a different preset

This separation exists because God Mode edits can target any XML attribute, including ones that CameraRules doesn't know about. Storing them separately keeps preset files clean.

---

## Preset Loading and Session XML

When the user selects a preset from the sidebar, the session XML is populated differently depending on the preset type.

### UCM Style Presets (Definition-Based)

UCM presets store a style definition (style_id + settings) rather than raw XML. Loading one:

1. Read the preset file and extract `style_id`, `settings` (distance, height, FoV, etc.)
2. Apply the settings to the Quick sliders
3. Run `CameraRules.BuildModifications()` with those settings
4. Apply to vanilla XML to produce session XML
5. Store the result in `_sessionXml`

This is called "session XML baking" because the XML is regenerated from the definition each time. This means UCM presets automatically adapt to game updates (new vanilla XML = new session XML with the same style applied on top).

### User/Community Presets

User and community presets may contain a `session_xml` field with pre-baked XML. Loading one:

1. Read the preset file
2. If `session_xml` is present, use it directly as `_sessionXml`
3. If not, treat it like a style preset and bake from the definition
4. Apply Quick slider values from the preset's `settings` field
5. Set `_sessionIsFullPreset = true` so Install uses the loaded XML directly

### Raw Imported Presets

Raw imports (from PAZ, XML files, mod manager packages) contain the original camera XML with no UCM rules:

1. Read the imported preset file
2. Use its raw XML directly as `_sessionXml`
3. Set `_sessionIsRawImport = true`
4. Disable UCM Quick and Fine Tune tabs (show explanation dialog if user tries to switch)
5. Only God Mode editing is available

God Mode edits layer on top of the raw imported XML without UCM rules interfering. This preserves the original mod author's intent.

### Full Manual Control Presets

When the user creates a new preset and chooses "Full Manual Control":

1. Vanilla XML is loaded as the starting `_sessionXml`
2. No CameraRules are applied
3. `_sessionIsRawImport = true` is set (same behavior as raw imports)
4. Only God Mode editing is available

---

## Session XML and the Install Pipeline

When the user clicks "Install to Game":

1. If `_sessionIsFullPreset` is true and `_sessionXml` is not empty, use `_sessionXml` directly
2. Otherwise, call `BuildSessionXmlForMode()` which rebuilds the XML from the current Quick settings
3. The XML is passed to `CameraMod.InstallRawXml()` for the full install pipeline (size-match, compress, encrypt, write)

Using `_sessionXml` directly for full presets is important because rebuilding from Quick settings could diverge from the saved file (Fine Tune/God Mode edits would be lost), and could change the compressed size vs. what the PAZ slot expects.

---

## Session XML Caching

UCM caches certain intermediate results to avoid redundant work:

- `_cachedVanillaXmlGameDir` / `_cachedStrippedVanillaXml`: The vanilla XML is cached after the first read from the game directory. It only changes if the user points UCM at a different game folder.
- `_cachedGameFingerprintDir` / `_cachedGameFingerprint`: PAZ entry fingerprint cached for imported preset change detection.
- `_customDraftDirty`: Flag that tracks whether Quick slider values have changed since the last session XML build. If false, the cached session XML can be reused without rebuilding.

---

## Preview Synchronization

When session XML changes (from any editor), the preview controls update:

1. `SyncPreview()` is called (debounced to avoid UI thrashing)
2. The current distance, height, and horizontal shift are extracted from the session XML
3. `CameraPreview` redraws the top-down distance visualization
4. `FovPreview` redraws the field-of-view cone
5. The preset name label updates

Preview updates are debounced with a `DispatcherTimer` to prevent excessive redraws during rapid slider movement.
