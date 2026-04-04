# Known Pitfalls

This document catalogs every known dangerous operation, crash-inducing configuration, size constraint, data corruption risk, and subtle behavioral trap in UCM. If you are modifying the camera modification engine, the PAZ archive writer, or the XML generation pipeline, read this entire document first.

## Game-Crashing Camera Sections

### Player_SilenceKill and Player_SilenceKill_Back

**Severity**: Immediate game crash on load.

**Rule**: NEVER modify any attribute in `Player_SilenceKill` or `Player_SilenceKill_Back`.

These two sections control the stealth kill camera. Changing ANY attribute in these sections causes the game to crash immediately on load. This includes:

- `ZoomDistance`
- `MaxZoomDistance`
- `CameraBlendParameter` (any sub-attribute: `BlendInTime`, `BlendOutTime`, etc.)
- Any other attribute

This was confirmed through systematic bisect testing: all other sections can be modified freely, but touching any value in either SilenceKill section reliably triggers a crash. The root cause is unknown and likely relates to how the engine treats these sections differently from other camera presets internally.

**How UCM handles it**: `CameraRules.BuildSmoothing()` explicitly skips these sections. The smoothing layer applies blend parameter changes to many combat and finisher sections, but `Player_SilenceKill` and `Player_SilenceKill_Back` are excluded from the modification set. Any code that generates modifications for "all combat sections" or "all sections matching a pattern" must also exclude these.

**If you add new modification logic**: Always check that your section filter excludes `Player_SilenceKill` and `Player_SilenceKill_Back`. A regex like `Player_.*Kill` would incorrectly match these. Prefer explicit inclusion lists over pattern matching for combat sections.

### Player_Interaction_LockOn and Interaction_LookAt

**Severity**: Correlated game crashes; not 100% reproducible.

**Rule**: Leave these sections at vanilla values. Do not scale `ZoomDistance`, `MaxZoomDistance`, or inject additional ZoomLevel entries (ZL2, ZL3, ZL4).

These sections control the NPC dialogue camera. Scaling `ZoomDistance`/`MaxZoomDistance` or injecting ZoomLevel entries here correlates with game crashes, though the crashes are not as reliably reproducible as SilenceKill. The crashes tend to occur when entering dialogue with NPCs.

**How UCM handles it**: These sections are left at vanilla values. The lock-on distance synchronization system (`BuildLockOnDistances`) only targets combat lock-on sections, not dialogue lock-on.

## PAZ Archive Size Constraint

### The Problem

The game's PAZ archive format has fixed-size slots for each entry. When UCM modifies the camera XML, the modified data (after LZ4 compression) must produce a compressed payload that is EXACTLY the same size as the original entry's compressed payload (`comp_size`). Additionally, the decompressed/plaintext data must be exactly `orig_size` bytes.

If either constraint is violated:
- Compressed too large: Install fails with a "too large" error. The game's archive reader would read past the slot boundary and corrupt memory.
- Compressed too small: The archive reader would interpret trailing garbage bytes as part of the next entry.
- Plaintext wrong size: LZ4 decompression would produce the wrong number of bytes, causing a read error.

### Size Matching Strategy

`ArchiveWriter.MatchCompressedSize(byte[] plaintext, int targetCompSize, int targetOrigSize)` uses a cascade of strategies to hit the exact compressed size. See [Export Pipeline](Export-Pipeline.md) for the full algorithm.

The key insight: XML comments (`<!--random content-->`) are invisible to the game's XML parser but affect LZ4 compression ratios. By inserting comments with carefully sized random content, UCM can precisely control the compressed output size.

### When Size Matching Fails

Size matching can fail in two directions:

**Too large** (more common): The modified XML has more content than vanilla (e.g., injected ZoomLevel entries, longer attribute values). After compression, it exceeds the slot size. Common causes:

1. **Corrupted vanilla backup**: If the backup was captured from already-modified game files, the "vanilla" baseline is already larger than true vanilla. This leaves less headroom for modifications. Fix: delete the `backups/` folder, verify game files on Steam, relaunch UCM.

2. **Too many modifications**: Extensive Fine Tune and God Mode edits, especially ZoomLevel injection for ZL3/ZL4 on many sections, can push the XML past the size limit. Fix: reduce the number of overrides or use a simpler preset.

3. **Game update**: If the game patches and the vanilla XML changes, the backup is stale. Fix: verify game files on Steam, delete the `backups/` folder, relaunch.

**Too small** (rare): The modified XML compresses significantly better than vanilla (e.g., many attributes set to the same value, creating LZ4 patterns). The inflation strategies (random comments, whitespace replacement) usually handle this, but in extreme cases they may fail. This throws an `InvalidOperationException`.

### ZoomLevel Injection and Size Growth

When user modifications target ZL3 or ZL4 that do not exist in the vanilla XML, `CameraMod.ApplyModifications` auto-injects new `<ZoomLevel>` elements at the `</ZoomLevelInfo>` closing tag. Each injected ZoomLevel adds approximately 80-120 bytes of plaintext XML. If many sections receive ZL3/ZL4 injection, the cumulative size increase can push the compressed output past the slot limit.

This is the most common cause of "too large" errors for users who heavily customize Fine Tune or God Mode settings.

## Timestamp Preservation

### The Problem

The game (or its launcher/anti-cheat) may check file metadata (creation time, last access time, last write time) on `0.paz` to detect modifications. If timestamps change, the game might trigger a re-verification or refuse to load.

### The Solution

`ArchiveWriter.SaveTimestamps(string path)` captures all three timestamps before writing and returns a closure that restores them after the write completes:

```csharp
public static Action SaveTimestamps(string path)
{
    var ct = File.GetCreationTimeUtc(path);
    var at = File.GetLastAccessTimeUtc(path);
    var mt = File.GetLastWriteTimeUtc(path);
    return () =>
    {
        File.SetCreationTimeUtc(path, ct);
        File.SetLastAccessTimeUtc(path, at);
        File.SetLastWriteTimeUtc(path, mt);
    };
}
```

This is called in `UpdateEntryAt` which wraps the actual file write. On non-Windows platforms, the method returns a no-op closure (checked via `RuntimeInformation.IsOSPlatform`).

### When This Can Break

- If UCM crashes between the write and the timestamp restore, timestamps will be modified. This is unlikely but possible.
- If the game checks timestamps at a finer granularity than `File.Set*TimeUtc` provides, the restore may not be sufficient.
- If another process modifies `0.paz` concurrently (e.g., game update in progress), the saved timestamps become stale.

## InvariantCulture Requirement

### The Problem

On systems with European locales (German, French, etc.), the decimal separator is a comma instead of a period. If `CultureInfo.CurrentCulture` is used for number formatting, XML attributes like `ZoomDistance="3.4"` would be written as `ZoomDistance="3,4"`. The game's XML parser expects period-separated decimals and will either crash or silently use wrong values.

### The Solution

`CultureInfo.InvariantCulture` must be forced in the application constructor or at startup. All number-to-string conversions in the XML pipeline use `InvariantCulture`:

- `CameraRules` builds `ModificationSet` values as strings with period separators
- `CameraMod.ApplyModifications` writes attributes with invariant formatting
- All slider value formatting uses `$"{value:F1}"` which respects the current culture, so the thread culture must be set globally

### Where This Goes Wrong

If a contributor adds new code that formats numbers for XML output without specifying `InvariantCulture`, it will work correctly on US/UK systems but produce corrupted XML on European systems. This is a subtle bug because:

1. The developer's machine works fine
2. CI (if any) likely runs on a US-locale machine
3. The bug only manifests for users in specific locales
4. The corrupted XML may still partially load, causing intermittent/hard-to-diagnose camera glitches

**Rule**: Any code that produces XML attribute values containing decimal numbers MUST use `InvariantCulture` or be covered by a global culture override.

## advanced_overrides.json Lifecycle

### The Problem

God Mode edits are persisted in `advanced_overrides.json`, separate from preset files. This allows God Mode changes to survive app restarts. However, this creates a risk of "stale override bleed-through": if the user loads a different preset, the God Mode overrides from the previous session could contaminate the new preset's values.

### The Solution

`advanced_overrides.json` is cleared when loading a new preset. The sequence:

1. User selects a new preset in the sidebar
2. Preset loading code clears `advanced_overrides.json`
3. The preset's `session_xml` is loaded as the new baseline
4. If the user enters God Mode, they see the preset's values (not stale overrides)

### When This Can Go Wrong

- If the clearing step is skipped (e.g., a new import code path that does not call the override-clearing logic), stale overrides will bleed into the imported preset.
- If the user modifies God Mode, switches to Quick tab, makes changes there, and switches back to God Mode, the overrides file may contain values that conflict with the Quick tab changes. The God Mode values will take precedence because they are layered on top.

### File Format

`advanced_overrides.json` is a JSON object mapping `section/attribute` keys to values:

```json
{
  "Player_Basic_Default/ZoomLevel[2].ZoomDistance": "4.5",
  "Player_Basic_Default/ZoomLevel[2].MaxZoomDistance": "12.0"
}
```

## DispatcherTimer Debouncing

### The Problem

Without debouncing, rapid slider changes would trigger expensive operations (XML rebuild, preview redraw, file writes) on every tick. This causes:
- UI freezing during slider dragging
- Excessive disk I/O from rapid file saves
- Wasted CPU cycles on intermediate states that are immediately superseded

### Active Debounce Timers

| Timer | Interval | What It Debounces | Risk If Missing |
|-------|----------|-------------------|-----------------|
| Save toast | 500ms | "Saved" notification display | UI flickering, toast appearing and disappearing rapidly |
| Preview sync | render-priority | Camera/FoV preview redraws | Laggy preview during slider dragging |
| Editor sync | 300ms | Session XML rebuild from Quick to Fine Tune/God Mode | Expensive XML processing on every slider tick |
| Search filter | varies | Fine Tune search text filtering | DataGrid thrashing on every keystroke |
| Install state | varies | Install state file writes | Excessive disk writes |

### The Coalesced Pattern

For preview updates, UCM uses a render-priority dispatch instead of a timer:

```csharp
private void ScheduleCoalescedPreviewSync()
{
    if (_previewSyncPosted) return;
    _previewSyncPosted = true;
    Dispatcher.BeginInvoke(new Action(() =>
    {
        _previewSyncPosted = false;
        SyncPreview();
    }), DispatcherPriority.Render);
}
```

This ensures exactly one preview update per render frame, regardless of how many slider changes occurred in that frame. The boolean flag prevents duplicate posts.

## UCM Preset Read-Only Protection

### The Problem

UCM presets are carefully tuned by the developer. If users accidentally modify them in Fine Tune or God Mode, the changes would be saved back to the preset file, corrupting the original tuning.

### The Solution

UCM presets have `"locked": true` in their JSON. When the user attempts to enter Fine Tune or God Mode with a UCM preset selected, a dialog warns them:

```
UCM presets are protected - Fine Tune changes could corrupt the preset's carefully tuned values.
Duplicate this preset first to create your own editable copy.
Open Fine Tune in read-only mode anyway?
```

If they choose "Yes", the tab opens in read-only mode (values visible but changes do not save). If "No", the tab switch is canceled.

## Raw XML Import Restrictions

### The Problem

When a user imports a raw XML file (from another mod or manual editing), the XML was not generated by UCM's camera rule system. If the user then switches to Quick or Fine Tune tabs, UCM would rebuild the session XML from its own rules, discarding all the imported values.

### The Solution

When `_sessionIsRawImport` is `true`:
- Quick and Fine Tune tabs are blocked with an informational dialog
- Only God Mode editing is available
- `CaptureSessionXml()` is a no-op (the imported XML is never rebuilt)
- `SyncQuickSettingsToEditorsNow()` returns early

```csharp
if (_sessionIsRawImport && (tab == "simple" || tab == "advanced"))
{
    MessageBox.Show(
        "This preset was imported as raw XML and is not managed by UCM.\n\n"
        + "UCM Quick and Fine Tune use UCM's camera rule system which would override "
        + "values from the imported mod.",
        ...);
    return;
}
```

## Session XML State Flags

Two boolean flags control how `_sessionXml` is treated:

### `_sessionIsFullPreset`

Set to `true` when a preset is loaded from disk (import, sidebar selection) that contains Fine Tune or God Mode values beyond what Quick sliders can represent. When `true`:

- Quick slider changes do NOT overwrite `_sessionXml`
- The loaded session XML is preserved until the user explicitly makes an edit
- Install uses the loaded `_sessionXml` directly, not a rebuild from Quick sliders

Set to `false` when: user changes any Quick slider, enters Fine Tune, or creates a new preset.

### `_sessionIsRawImport`

Set to `true` when a raw XML import is loaded. This is a stronger restriction than `_sessionIsFullPreset`. When `true`:

- Quick and Fine Tune tabs are locked out entirely
- Session XML is never rebuilt through CameraRules
- God Mode edits are layered directly onto the imported XML

These flags are separate because a preset can be a "full preset" (has Fine Tune values) without being a "raw import" (was still generated by UCM).

## Game Detection and Backup Staleness

### The Problem

UCM auto-detects the game installation and creates a backup of the vanilla camera data on first run. If the game updates, the backup becomes stale. Installing a preset built against a stale backup can produce corrupted output (wrong offsets, wrong sizes, missing data).

### The Solution

`GameInstallBaselineTracker` records a fingerprint of the game installation after each successful install. On subsequent launches, it compares the current game state against the saved fingerprint. If they differ (game was updated), it:

1. Shows a warning strip in the UI
2. Automatically refreshes the vanilla backup from the live PAZ (background task)
3. Clears the warning after the user installs again

### The Auto-Refresh Race

The automatic backup refresh runs on a background thread:

```csharp
Task.Run(() =>
{
    CameraMod.RefreshVanillaBackupFromLivePaz(gameDir, _ => { });
    Dispatcher.BeginInvoke(() => { /* update UI */ });
});
```

If the user clicks Install before the refresh completes, they could install against a stale backup. The `_gameUpdateAutoBackupDispatched` flag prevents multiple refresh dispatches, and the warning strip remains visible to discourage premature installs.

## Potential Data Loss Scenarios

### Concurrent PAZ Modification

If the game, Steam, or another mod tool modifies `0.paz` while UCM is writing to it, the file can be corrupted. UCM does not acquire a file lock for the duration of the install. The write in `UpdateEntryAt` opens the file with `FileMode.Open, FileAccess.Write`, seeks to the entry offset, and writes the payload. If another process writes to the same region concurrently, the result is undefined.

**Mitigation**: Users should close the game before installing. UCM does not enforce this.

### Backup Directory Deletion

If the user manually deletes the `backups/` directory while a preset is loaded, the next Install will fail because it cannot read the vanilla baseline. The error message guides the user to verify game files and restart UCM.

### Kill During Install

If the user kills UCM (Task Manager, power failure) during the Install write, the PAZ file may be left in a partially written state. The timestamp restoration closure will not execute, so timestamps will also be wrong. The user must verify game files on Steam to recover.

## Summary of Do-Not-Touch Rules

| Section / Feature | Rule | Consequence of Violation |
|-------------------|------|--------------------------|
| `Player_SilenceKill` | Never modify any attribute | Immediate game crash on load |
| `Player_SilenceKill_Back` | Never modify any attribute | Immediate game crash on load |
| `Player_Interaction_LockOn` | Do not scale ZoomDistance/MaxZoomDistance or inject ZL2-4 | Correlated game crashes in NPC dialogue |
| `Interaction_LookAt` | Do not scale ZoomDistance/MaxZoomDistance or inject ZL2-4 | Correlated game crashes in NPC dialogue |
| PAZ compressed size | Must exactly match `comp_size` | Archive corruption, game crash |
| PAZ plaintext size | Must exactly match `orig_size` | LZ4 decompression failure, game crash |
| PAZ timestamps | Must be preserved across writes | Potential game integrity check failure |
| Decimal formatting | Must use `InvariantCulture` | Corrupted XML on European locales |
| `advanced_overrides.json` | Must be cleared on preset load | Stale God Mode values bleed into new presets |
| `_sessionIsRawImport` | Must prevent CameraRules rebuild | Imported mod values silently discarded |

## Relevant Source Files

| File | Role |
|------|------|
| `src/UltimateCameraMod/Models/CameraRules.cs` | Camera rule engine, section exclusion lists |
| `src/UltimateCameraMod/Services/CameraMod.cs` | XML modification, ZoomLevel injection, backup management |
| `src/UltimateCameraMod/Paz/ArchiveWriter.cs` | Size matching, timestamp preservation |
| `src/UltimateCameraMod/Services/GameInstallBaselineTracker.cs` | Game update detection, backup staleness |
| `src/UltimateCameraMod.V3/MainWindow.Editors.cs` | Session state flags, tab switching restrictions |
| `src/UltimateCameraMod.V3/MainWindow.GodMode.cs` | Override persistence, `AdvLoadOverrides()` |
| `src/UltimateCameraMod.V3/MainWindow.Export.cs` | Install pipeline, install trace |
