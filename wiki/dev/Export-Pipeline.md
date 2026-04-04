# Export Pipeline

This document covers all four export formats, the binary diff algorithm used for JSON Mod Manager export, the full Install to Game pipeline, and the install trace output.

## Export Formats

UCM supports four export targets:

| Format | Extension | Target Consumer | Contains |
|--------|-----------|-----------------|----------|
| JSON Mod Manager | `manifest.json` + `files/` | CD JSON Mod Manager / CDUMM | Binary diff patches against decompressed camera payload |
| XML | `.xml` | Manual modders, debugging | Raw session XML string |
| PAZ Archive | `0.paz` | Direct game file replacement | Modified copy of vanilla `0.paz` with camera entry patched |
| UCM Preset | `.ucmpreset` | UCM sharing / import | Serialized preset state as JSON |

### JSON Mod Manager Export

This is the most complex export format. It produces a patch manifest compatible with the Crimson Desert JSON Mod Manager (and CDUMM). The output describes byte-level differences between the vanilla and modified decompressed camera payloads.

**Entry point**: `JsonModExporter.ExportFromModSet()` or `JsonModExporter.ExportFromXml()` in `src/UltimateCameraMod/Services/JsonModExporter.cs`.

**Pipeline**:

1. Read the vanilla decompressed camera bytes from the stored backup
2. Build the modified decompressed payload by applying the user's session XML
3. Run the binary diff algorithm (`GeneratePatches`) to find changed regions
4. Verify the patches round-trip correctly (`VerifyPatchesRoundTrip`)
5. Serialize the patches to JSON (`BuildJson`)

**Output structure**:

```json
{
  "name": "My Camera Mod",
  "version": "1.0",
  "author": "Username",
  "description": "Custom camera preset",
  "modinfo": {
    "title": "My Camera Mod",
    "version": "1.0",
    "author": "Username",
    "description": "Custom camera preset",
    "nexus_url": ""
  },
  "patches": [
    {
      "game_file": "data/0.paz:path/to/camera/entry",
      "source_group": "camera_preset_group",
      "changes": [
        {
          "offset": 1234,
          "original": "33342e30",
          "patched": "352e3030",
          "label": "Player_Basic_Default / ZoomLevelInfo / ZoomLevel"
        }
      ]
    }
  ]
}
```

Note the dual metadata: both root-level keys (`name`, `version`, etc.) and a nested `modinfo` block. The root-level keys are for CDUMM compatibility; the nested block is for CD JSON Mod Manager / UCM compatibility.

#### Multi-Preset Export

`BuildMultiPresetJson()` produces a single JSON file containing multiple presets. Each preset becomes a separate entry in the `patches[]` array targeting the same `game_file`. Change labels are prefixed with `[PresetName]` so the mod manager can distinguish between presets.

This format requires at least two presets to trigger CDUMM's `_detect_preset_groups` behavior, which presents a radio-button picker at import time.

### XML Export

The simplest export. Writes the current session XML string directly to a `.xml` file. Useful for debugging, manual inspection, or importing into other tools.

### PAZ Export

Produces a modified copy of the vanilla `0.paz` archive:

1. Copies the vanilla `0.paz` to the output location
2. Calls `CameraMod.InstallRawXml` on the copy
3. The modified archive can be dropped into the game directory as a direct replacement

### UCM Preset Export

Serializes the current preset state to a `.ucmpreset` JSON file. This includes:
- Preset metadata (name, author, description)
- Settings (style, FoV, distance, height, etc.)
- The full `session_xml` string
- Kind and locked status

## Binary Diff Algorithm

The core of the JSON export is `JsonModExporter.GeneratePatches()`. This method walks two equal-length byte arrays and groups differing regions into `PatchChange` records.

### PatchChange Record

```csharp
public record PatchChange(long Offset, string Original, string Patched, string Label);
```

| Field | Description |
|-------|-------------|
| `Offset` | Zero-based byte offset within the decompressed entry buffer |
| `Original` | Lowercase hex string of the vanilla bytes at this region |
| `Patched` | Lowercase hex string of the modified bytes at this region |
| `Label` | XML context string describing which element this change falls within |

### Algorithm Walkthrough

```csharp
public static List<PatchChange> GeneratePatches(byte[] vanillaBytes, byte[] modifiedBytes)
```

**Precondition**: Both arrays must have the same length. This is enforced with an `ArgumentException`.

**Merge gap constant**: `MergeGap = 4`. If two changed regions are separated by 4 or fewer identical bytes, they are merged into a single patch. This reduces the number of patches and avoids fragmentation from small unchanged gaps between modified XML attribute values.

**Step-by-step**:

1. Build the XML offset map (see below) for label generation
2. Initialize scan pointer `i = 0`
3. Scan forward until `vanillaBytes[i] != modifiedBytes[i]` (a difference is found)
4. Record `start = i` and begin extending the region:
   - If bytes differ at current position, advance `end`
   - If bytes match, scan ahead to find the gap length
   - If the gap is <= `MergeGap` bytes AND there are more differences after the gap, merge by continuing to extend `end` through the gap
   - If the gap exceeds `MergeGap` or we reach the end of the buffer, finalize this region
5. Convert the `[start, end)` range to hex strings and create a `PatchChange`
6. Resume scanning from `end`

```
Vanilla:  ... AA BB CC DD EE FF 11 22 33 44 ...
Modified: ... AA BB XX YY EE FF ZZ 22 33 44 ...
                    ^^^^^^^^^^     ^^^
                    Region 1       Region 2

Gap between Region 1 end (EE FF) and Region 2 start (ZZ) = 2 bytes.
Since 2 <= MergeGap (4), these merge into one PatchChange:
  Offset: position of CC
  Original: "CCDDEEFF11"
  Patched:  "XXYEEFFZZ"
```

### XML-Aware Labeling

`BuildXmlOffsetMap()` creates a mapping from byte offsets to XML element paths. This gives each patch a human-readable label like `"Player_Basic_Default / ZoomLevelInfo / ZoomLevel"`.

**How it works**:

1. Strip trailing null bytes from the buffer to find the XML content boundary
2. Decode the bytes as UTF-8
3. Scan character by character for XML tags using two compiled regexes:
   - `OpenTagRx`: `<(\w[\w.:-]*)(?:\s[^>]*)?>` matches opening tags
   - `CloseTagRx`: `</(\w[\w.:-]*)>` matches closing tags
4. Maintain a stack of open tag names
5. At each opening tag, compute the byte offset (`Encoding.UTF8.GetByteCount` from start to current char position) and record `(byteOffset, path)` where path is the last 3 elements of the stack joined by ` / `

`LabelForOffset()` uses binary search on the sorted offset map to find the nearest XML context for a given byte offset.

### Round-Trip Verification

After generating patches, `VerifyPatchesRoundTrip()` applies every patch to a clone of the vanilla bytes and asserts the result exactly matches the modified bytes. This catches:

- Off-by-one errors in offset calculation
- Hex encoding/decoding bugs
- Overlapping patch regions

If verification fails, an `InvalidOperationException` is thrown and the export is aborted. This ensures exported patches are always correct.

## Install to Game Pipeline

The Install to Game flow writes the modified camera directly into the game's `0.paz` archive. This is the most complex operation in UCM and involves multiple safety mechanisms.

**Entry point**: `OnInstall()` in `MainWindow.Export.cs`, which dispatches the heavy work to a background `Task.Run`.

### Full Pipeline Sequence

```
User clicks "Install"
    |
    v
1. Build session XML from current editor state
    |
    v
2. CameraMod.InstallRawXml(gameDir, xml)
    |
    +-- 2a. Read vanilla backup (decompressed camera bytes)
    |
    +-- 2b. Parse the session XML string into raw bytes
    |
    +-- 2c. CameraMod.ApplyModifications: merge session XML onto vanilla
    |
    +-- 2d. Size matching (ArchiveWriter.MatchCompressedSize)
    |       - Pad or shrink plaintext to match orig_size
    |       - Compress with LZ4
    |       - If compressed size != target, adjust padding
    |       - If still too large, throw "too large" error
    |
    +-- 2e. Encrypt the compressed payload (StreamTransform)
    |
    +-- 2f. Save timestamps on 0.paz (ArchiveWriter.SaveTimestamps)
    |
    +-- 2g. Write encrypted payload at the entry's offset in 0.paz
    |
    +-- 2h. Restore original timestamps
    |
    v
3. Post-install verification
    |
    +-- 3a. Re-read the camera entry to confirm write
    |
    +-- 3b. Compare before/after payload bytes
    |
    +-- 3c. Write install_trace.txt
    |
    +-- 3d. Update GameInstallBaselineTracker
    |
    v
4. Show result in status bar
```

### Size Matching: ArchiveWriter.MatchCompressedSize

This is the most intricate part of the install pipeline. The game's PAZ archive has fixed-size slots. The modified XML, after LZ4 compression, must produce exactly the same number of compressed bytes as the original entry (`targetCompSize`), and the plaintext must be exactly `targetOrigSize` bytes.

```csharp
public static byte[] MatchCompressedSize(
    byte[] plaintext,
    int targetCompSize,
    int targetOrigSize)
```

**Strategy cascade** (tried in order until one succeeds):

1. **Direct pad/shrink**: Pad with nulls or shrink to `targetOrigSize`, compress, check if compressed size matches.

2. **If compressed too small** (compresses too well, `delta < 0`):
   - **PadWithScatteredComments**: Insert XML comments (`<!--random-->`) at newline positions throughout the content to break LZ4's pattern matching. Uses binary search on comment body length to hit the exact compressed size.
   - **InflateWithComments**: Append a single large XML comment, or replace trailing padding with spaces/comments. Binary search on body length.
   - **InflateByReplacingCommentBodies**: Replace existing XML comment bodies with random content to increase entropy.
   - **InflateByReplacingWhitespaceRuns**: Replace whitespace runs (>= 8 bytes) with XML comments containing random content.

3. **If compressed too large** (`delta > 0`):
   - The modified XML is too complex to fit in the slot. An `InvalidOperationException` is thrown with a diagnostic message suggesting common causes:
     - Vanilla backup was captured from already-modified game files
     - Too many Fine Tune / God Mode edits
     - Game was updated

**Why XML comments?** LZ4 achieves compression through pattern matching. Vanilla XML has repetitive structure that compresses well. Modified XML may compress smaller (fewer repeated patterns) or larger (more unique values). XML comments with random printable ASCII content (`<!--xK9mPq...-->`) are invisible to the game's XML parser but break LZ4's pattern matching, allowing precise control over compressed output size.

**Random content generation**: `MakeXmlSafeRandomContent()` generates bytes from a safe alphabet (printable ASCII excluding `-`, `<`, `>`, `&`) using `RandomNumberGenerator.GetBytes()`. This avoids accidental XML-invalid sequences.

**Binary search pattern**: Each inflation strategy uses the same pattern:
1. Try minimum and maximum body lengths to establish bounds
2. Binary search on body length to find the exact compressed size
3. Linear scan near the boundary (within ~30-50 bytes) to handle LZ4's non-monotonic compression behavior

### ShrinkToOrigSize

When the modified XML exceeds `targetOrigSize`, `ShrinkToOrigSize` attempts three strategies:

1. **Trim comment bodies**: Find XML comments and remove characters from their bodies
2. **Remove duplicate whitespace**: Strip adjacent whitespace characters
3. **Remove entire comments**: Delete complete `<!--...-->` sequences

If after all strategies the result still exceeds `targetOrigSize`, an `InvalidOperationException` is thrown.

### Timestamp Preservation

`ArchiveWriter.SaveTimestamps(path)` captures the creation, last access, and last write timestamps of the PAZ file before modification, and returns an `Action` that restores them after the write. This prevents the game's integrity check from detecting the modification based on file metadata changes.

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

The pattern is: capture before write, perform write, restore after. This is called in `UpdateEntryAt`:

```csharp
public static void UpdateEntryAt(string pazFilePath, long offset, byte[] payload)
{
    var restoreTs = SaveTimestamps(pazFilePath);
    using (var fs = new FileStream(pazFilePath, FileMode.Open, FileAccess.Write))
    {
        fs.Seek(offset, SeekOrigin.Begin);
        fs.Write(payload);
    }
    restoreTs();
}
```

## Install Trace Output

After every install, UCM writes `install_trace.txt` next to the executable. This file contains diagnostic information for troubleshooting:

```
time_utc=2025-01-15T14:32:10.1234567Z
game_dir=C:\Program Files (x86)\Steam\steamapps\common\CrimsonDesert
mode=simple
style_id=panoramic
session_xml_sha256=ABC123...
entry_path=data/camera/playercamerapreset
paz_file=C:\...\0.paz
offset=12345678
comp_size=48320
before_sha256=DEF456...
after_sha256=789GHI...
payload_changed=true
```

| Field | Description |
|-------|-------------|
| `time_utc` | ISO 8601 timestamp of the install |
| `game_dir` | Detected game installation directory |
| `mode` | Active editor tab at install time (`simple`, `advanced`, `expert`) |
| `style_id` | Selected camera style ID |
| `session_xml_sha256` | SHA-256 of the session XML that was installed |
| `entry_path` | Internal PAZ entry path for the camera asset |
| `paz_file` | Full filesystem path to the modified `0.paz` |
| `offset` | Byte offset within `0.paz` where the payload was written |
| `comp_size` | Compressed size of the payload in the PAZ slot |
| `before_sha256` | SHA-256 of the raw PAZ payload bytes before install |
| `after_sha256` | SHA-256 of the raw PAZ payload bytes after install |
| `payload_changed` | `true` if the before and after hashes differ |

The `payload_changed` field is particularly useful: if it is `false`, the install was a no-op (the camera was already in the desired state).

## Restore Pipeline

`OnRestore()` calls `CameraMod.RestoreCamera(gameDir)` on a background thread. This reads the vanilla backup and writes it back to the PAZ entry, effectively reverting the camera to its unmodified state.

Possible outcomes reported via status:

| Status | Meaning |
|--------|---------|
| `ok` | Vanilla backup restored successfully |
| `no_backup` | No backup exists; camera may already be vanilla |
| `stale_backup` | Game was updated since last install; backup was refreshed from live PAZ |

## ExportFromModSet vs ExportFromXml

`JsonModExporter` provides two entry points depending on the source of modifications:

**`ExportFromModSet`**: Used when exporting from a preset with a `ModificationSet` (Quick/Fine Tune settings). Reads the stored vanilla backup as the baseline.

```csharp
public static (List<PatchChange> Changes, string Json) ExportFromModSet(
    string gameDir, ModInfo info, ModificationSet modSet, Action<string>? log = null)
```

**`ExportFromXml`**: Used when exporting from raw XML (God Mode, imported presets). Reads the live camera data as the baseline.

```csharp
public static (List<PatchChange> Changes, string Json) ExportFromXml(
    string gameDir, ModInfo info, string xmlText, Action<string>? log = null)
```

Both methods follow the same pattern: read baseline, build modified payload, diff, verify, serialize.

## Relevant Source Files

| File | Role |
|------|------|
| `src/UltimateCameraMod/Services/JsonModExporter.cs` | Binary diff engine, JSON builder, full export pipeline |
| `src/UltimateCameraMod/Paz/ArchiveWriter.cs` | Size matching, timestamp preservation, PAZ entry writing |
| `src/UltimateCameraMod/Paz/CompressionUtils.cs` | LZ4 compression/decompression wrappers |
| `src/UltimateCameraMod/Paz/StreamTransform.cs` | Encryption/decryption of PAZ payloads |
| `src/UltimateCameraMod/Services/CameraMod.cs` | XML modification engine, install orchestration |
| `src/UltimateCameraMod.V3/MainWindow.Export.cs` | Install/restore UI handlers, install trace writing |
| `src/UltimateCameraMod.V3/ExportDialog.xaml.cs` | Export format selection dialog |
| `src/UltimateCameraMod.V3/ExportJsonDialog.xaml.cs` | JSON export configuration dialog |
