# PAZ and PAMT Archives

This document provides a deep technical explanation of Crimson Desert's PAZ archive format, how UCM reads and writes these archives, and the cryptographic and compression layers involved.

## Overview

Crimson Desert stores game assets in `.paz` archive files. Each `.paz` is a flat binary blob containing concatenated file entries. A companion `.pamt` index file provides the directory structure and entry metadata needed to locate any file within the archive.

The camera configuration lives at a specific path (`playercamerapreset.xml`) inside `0.paz`, discoverable by walking the PAMT path tree. Each entry in the archive is:

1. **LZ4 block compressed** (no frame header)
2. **ChaCha20 encrypted** using a key derived from the filename hash

UCM must reverse both layers to read the XML, modify it, and then re-apply both layers before writing back.

## PAMT Index File Structure

The PAMT file is a binary index that maps file paths to their locations within PAZ archives. `PamtReader.Parse()` walks this structure sequentially.

### Binary Layout

```
Offset  Size    Field
------  ------  ---------------------
0x00    4       Magic bytes (skipped)
0x04    4       PAZ count (uint32)
0x08    8       Hash + zero padding

-- PAZ descriptor table (one per PAZ file) --
For each PAZ (0..pazCount-1):
  +0      4     Hash (uint32)
  +4      4     Size (uint32)
  +8      4     Separator (except last entry)

-- Folder section --
  +0      4     Folder section size (uint32)
  For each folder entry until section end:
    +0    4     Parent index (uint32, 0xFFFFFFFF = root)
    +4    1     Name string length (byte)
    +5    N     Name string (UTF-8, N = string length)
  The root entry (parent == 0xFFFFFFFF) provides the folder prefix
  for all paths in this PAMT.

-- Node section (path tree) --
  +0      4     Node section size (uint32)
  For each node entry until section end:
    +0    4     Parent reference (uint32, offset within node section; 0xFFFFFFFF = root)
    +4    1     Name string length (byte)
    +5    N     Name string (UTF-8)
  Nodes are keyed by their byte offset relative to the node section start.

-- Record section --
  +0      4     Folder count (uint32)
  +4      4     Hash (uint32)
  +8      N     Folder records (16 bytes each, folderCount entries, skipped)

  -- File records (20 bytes each, until end of file) --
  For each file record:
    +0    4     Node reference (uint32, offset into node section)
    +4    4     PAZ offset (uint32, byte offset within the PAZ file)
    +8    4     Compressed size (uint32)
    +12   4     Original size (uint32, decompressed size)
    +16   4     Flags (uint32)
                  Bits 0-7:   PAZ index offset (added to PAMT stem number)
                  Bits 16-19: Compression type
```

### Path Tree Resolution

File paths are reconstructed by walking the node tree upward from a node reference to the root:

```csharp
string BuildPath(uint nodeRef)
{
    var parts = new List<string>();
    uint cur = nodeRef;
    while (cur != 0xFFFFFFFF && guard < 64)
    {
        int key = (int)cur;
        if (!nodes.TryGetValue(key, out var node)) break;
        parts.Add(node.Name);
        cur = node.Parent;
    }
    parts.Reverse();
    return string.Concat(parts);
}
```

The final full path is `{folderPrefix}/{nodePath}`. For the camera file, this resolves to a path containing `playercamerapreset.xml`.

### PAZ File Resolution

The PAZ index for a given record is computed from the PAMT filename stem and the flags field:

```csharp
int pazIndex = (int)(flags & 0xFF);
int pazNum = int.Parse(pamtStem) + pazIndex;
string pazFile = Path.Combine(pazDir, $"{pazNum}.paz");
```

For the camera file, the PAMT is `0010/0.pamt` and the PAZ is typically `0010/0.paz`.

### PazEntry Data Model

Each parsed record becomes a `PazEntry`:

```csharp
public sealed class PazEntry
{
    public string Path { get; init; }       // Full asset path (e.g., ".../playercamerapreset.xml")
    public string PazFile { get; init; }    // Absolute path to the .paz file on disk
    public int Offset { get; init; }        // Byte offset within the .paz
    public int CompSize { get; init; }      // Compressed (encrypted) size in bytes
    public int OrigSize { get; init; }      // Decompressed plaintext size in bytes
    public uint Flags { get; init; }        // Raw flags (PAZ index in low byte, compression type in bits 16-19)
    public int PazIndex { get; init; }      // Extracted PAZ index offset

    public bool Compressed => CompSize != OrigSize;
    public int CompressionType => (int)((Flags >> 16) & 0x0F);
    public bool IsXml => Path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
}
```

## ChaCha20 Encryption

Every file entry in a PAZ archive is encrypted with ChaCha20. The key and nonce are derived deterministically from the filename, meaning the same file always produces the same key material. Encoding and decoding are the same XOR operation.

### Key Derivation (AssetCodec.BuildParameters)

```
Input:  filename string (e.g., "playercamerapreset.xml")
Output: (byte[32] key, byte[16] nonce)

Step 1: Compute filename hash
  basename = Path.GetFileName(filename).ToLowerInvariant()
  seed = NameHasher.ComputeHash(UTF8(basename), initval=0x000C5EDE)

Step 2: Build nonce (128-bit)
  seedBytes = BitConverter.GetBytes(seed)       // 4 bytes, little-endian
  nonce = seedBytes || seedBytes || seedBytes || seedBytes   // repeated 4 times = 16 bytes

Step 3: Build key (256-bit)
  keyBase = seed XOR 0x60616263
  For i in 0..7:
    key[i*4 .. i*4+3] = BitConverter.GetBytes(keyBase XOR XorDeltas[i])

  XorDeltas = [
    0x00000000, 0x0A0A0A0A, 0x0C0C0C0C, 0x06060606,
    0x0E0E0E0E, 0x0A0A0A0A, 0x06060606, 0x02020202
  ]
```

### NameHasher (Lookup3)

The hash function is Bob Jenkins' Lookup3. It operates on 12-byte blocks with a final mix:

```csharp
// Initialize
a = b = c = 0xDEADBEEF + (uint)length + initval;

// Process 12-byte blocks
while (length > 12) {
    a += Le32(data, off);
    b += Le32(data, off + 4);
    c += Le32(data, off + 8);
    // Mix round (subtract, XOR, rotate)
    a -= c; a ^= Rot(c, 4);  c += b;
    b -= a; b ^= Rot(a, 6);  a += c;
    c -= b; c ^= Rot(b, 8);  b += a;
    a -= c; a ^= Rot(c, 16); c += b;
    b -= a; b ^= Rot(a, 19); a += c;
    c -= b; c ^= Rot(b, 4);  b += a;
    off += 12; length -= 12;
}

// Final block (tail bytes, avalanche)
// Returns: c (primary hash)
```

### ChaCha20 Stream Cipher (StreamTransform)

UCM includes a pure C# ChaCha20 implementation following RFC 7539. This avoids any dependency on platform-specific crypto libraries.

**State matrix layout (4x4 uint32):**

```
 0: constant[0]   1: constant[1]   2: constant[2]   3: constant[3]
 4: key[0..3]     5: key[4..7]     6: key[8..11]    7: key[12..15]
 8: key[16..19]   9: key[20..23]  10: key[24..27]  11: key[28..31]
12: counter       13: nonce[4..7]  14: nonce[8..11] 15: nonce[12..15]
```

Constants: `0x61707865 0x3320646e 0x79622d32 0x6b206574` (ASCII: "expand 32-byte k")

The counter (state[12]) is initialized from `nonce[0..3]` (first 4 bytes of the nonce) and incremented after each 64-byte block.

**Quarter-round function:**

```csharp
static void QuarterRound(uint[] s, int a, int b, int c, int d)
{
    s[a] += s[b]; s[d] ^= s[a]; s[d] = RotL(s[d], 16);
    s[c] += s[d]; s[b] ^= s[c]; s[b] = RotL(s[b], 12);
    s[a] += s[b]; s[d] ^= s[a]; s[d] = RotL(s[d],  8);
    s[c] += s[d]; s[b] ^= s[c]; s[b] = RotL(s[b],  7);
}
```

**20-round core (10 double-rounds):**

Each double-round applies 4 column quarter-rounds then 4 diagonal quarter-rounds:

```
Column rounds:    QR(0,4,8,12)  QR(1,5,9,13)  QR(2,6,10,14)  QR(3,7,11,15)
Diagonal rounds:  QR(0,5,10,15) QR(1,6,11,12) QR(2,7,8,13)   QR(3,4,9,14)
```

After 20 rounds, the working state is added element-wise to the original state, serialized to 64 bytes (little-endian), and XORed with 64 bytes of plaintext.

## LZ4 Block Compression

PAZ entries use LZ4 block compression (no frame header). UCM wraps the `K4os.Compression.LZ4` NuGet package:

```csharp
// Decompress
byte[] output = new byte[originalSize];
LZ4Codec.Decode(data, 0, data.Length, output, 0, originalSize);

// Compress
int maxLen = LZ4Codec.MaximumOutputSize(data.Length);
byte[] buffer = new byte[maxLen];
int encoded = LZ4Codec.Encode(data, 0, data.Length, buffer, 0, buffer.Length);
byte[] result = new byte[encoded];
Array.Copy(buffer, result, encoded);
```

The game expects raw LZ4 blocks without the standard LZ4 frame header (magic number, frame descriptor, etc.). The `K4os` library's `LZ4Codec.Encode/Decode` methods operate at the block level by default, which matches this requirement.

## Reading a PAZ Entry

The full read pipeline for the camera XML:

```
1. PamtReader.Parse("0010/0.pamt")
   -> List<PazEntry> (find entry where Path contains "playercamerapreset.xml")

2. FileStream.Seek(entry.Offset) + Read(entry.CompSize bytes)
   -> byte[] rawBytes (encrypted + compressed)

3. AssetCodec.Decode(rawBytes, "playercamerapreset.xml")
   -> byte[] decrypted (still compressed)

4. CompressionUtils.Lz4Decompress(decrypted, entry.OrigSize)
   -> byte[] plaintext

5. Encoding.UTF8.GetString(plaintext).TrimEnd('\0')
   -> string xmlText
```

In code (`CameraMod.DecodeEntryXml`):

```csharp
byte[] dec = AssetCodec.Decode(rawBytes, Path.GetFileName(entry.Path));
byte[] plain = CompressionUtils.Lz4Decompress(dec, entry.OrigSize);
return Encoding.UTF8.GetString(plain).TrimEnd('\0');
```

## Size Matching Constraint

This is the most technically challenging aspect of the write-back process. The PAZ archive has a fixed layout: each entry occupies a slot of exactly `CompSize` bytes at its `Offset`. UCM cannot resize the slot because that would shift every subsequent entry and invalidate all offsets in the PAMT index.

**The constraint:** The modified XML, after UTF-8 encoding, must compress (LZ4) to exactly `entry.CompSize` bytes. The plaintext buffer must be exactly `entry.OrigSize` bytes.

If the modified XML compresses to fewer bytes than `CompSize`, UCM must inject entropy to increase the compressed size. If it compresses to more, the preset is too large to install.

### ArchiveWriter.MatchCompressedSize()

This method orchestrates multiple strategies to hit the exact target:

```
Input:  byte[] plaintext, int targetCompSize, int targetOrigSize
Output: byte[] matched (exactly targetOrigSize bytes, compresses to exactly targetCompSize bytes)

Step 1: Pad or shrink plaintext to targetOrigSize
  - If plaintext > targetOrigSize: ShrinkToOrigSize (trim comments, whitespace, then entire comments)
  - If plaintext <= targetOrigSize: PadToOrigSize (zero-fill)

Step 2: Test compression
  comp = Lz4Compress(padded)
  if comp.Length == targetCompSize: done

Step 3: If comp.Length < targetCompSize (compresses too well)
  Need to inject entropy. Try strategies in order:

  Strategy A: PadWithScatteredComments()
    Insert <!--RANDOM--> comments at newline positions throughout the XML.
    Binary search on total random body length across N comment slots to hit exact comp size.
    Tries slot counts: 20, 50, 100, 200, 400, up to 800.

  Strategy B: InflateWithComments()
    Append/insert random XML comments in the padding area (after plaintext, before origSize boundary).
    Binary search on comment body length. Falls back to multi-slot insertion at newlines.

  Strategy C: InflateByReplacingCommentBodies()
    Find existing XML comments (from prior padding runs) and replace their bodies with random content.
    Binary search on how many comment body bytes to randomize.

  Strategy D: InflateByReplacingWhitespaceRuns()
    Find runs of 8+ whitespace characters and replace them with <!--RANDOM--> comments.
    Binary search on number of whitespace runs to convert.

  If none succeed: throw (file compresses too well, cannot inflate).

Step 4: If comp.Length > targetCompSize (too large)
  The preset is too large. Throw with a diagnostic message explaining:
  - Tainted vanilla backup (delete backups/, verify game files)
  - Too many Fine Tune / God Mode overrides
  - Game was updated
```

### Why XML Comments for Padding?

XML comments (`<!-- ... -->`) are the ideal padding vehicle because:

1. The game's XML parser ignores them (no gameplay effect)
2. Random content inside comments breaks LZ4's pattern matching, increasing compressed output size in a controllable way
3. Comments can be inserted at any newline boundary without affecting XML structure
4. The comment overhead is minimal (7 bytes for `<!---->`)
5. Comment body length can be binary-searched to converge on exact compressed sizes

### Binary Search Pattern

All inflation strategies use the same binary search pattern:

```csharp
// Find the random body length N that produces exactly targetCompSize
int lo = 0, hi = maxBody;
while (lo <= hi)
{
    int mid = (lo + hi) / 2;
    var trial = BuildTrial(mid);
    int c = CompressionUtils.Lz4Compress(trial).Length;
    if (c == targetCompSize) return trial;
    else if (c < targetCompSize) lo = mid + 1;
    else hi = mid - 1;
}
// Linear scan near the boundary (LZ4 is not perfectly monotonic)
for (int n = Math.Max(0, lo - 40); n < Math.Min(lo + 40, maxBody + 1); n++)
{
    var trial = BuildTrial(n);
    if (CompressionUtils.Lz4Compress(trial).Length == targetCompSize)
        return trial;
}
```

The linear scan after binary search is necessary because LZ4 compression ratios are not perfectly monotonic with respect to input entropy. Small changes in random content can cause non-monotonic jumps in compressed size.

### ShrinkToOrigSize

When the modified XML is larger than `targetOrigSize` (rare, happens with many injected ZoomLevel nodes), the writer must shrink:

1. **Trim comment bodies**: Find the largest XML comment, remove bytes from its body. Repeat.
2. **Remove adjacent duplicate whitespace**: Collapse runs of spaces/tabs.
3. **Remove entire comments**: Delete complete `<!--...-->` sequences including delimiters.

If shrinking cannot bring the size down to `targetOrigSize`, an exception is thrown.

## Writing Back to PAZ

The write-back pipeline (`CameraMod.InstallRawXml`):

```
1. FindCameraEntry(gameDir)     -> PazEntry (offset, sizes, PAZ file path)
2. EnsureBackup(entry)          -> saves original encrypted bytes to backups/original_backup.bin
3. StripComments(xmlText)       -> remove any existing XML comments
4. UTF8 encode (with BOM)       -> byte[]
5. MatchCompressedSize(bytes, entry.CompSize, entry.OrigSize)  -> byte[] (exact size)
6. Lz4Compress(matched)         -> byte[] (should be exactly entry.CompSize)
7. AssetCodec.Encode(compressed, "playercamerapreset.xml")     -> byte[] encrypted
8. ArchiveWriter.UpdateEntry(entry, encoded)
   a. SaveTimestamps(entry.PazFile)     -> capture creation/access/modify times
   b. FileStream.Seek(entry.Offset)
   c. FileStream.Write(encoded)
   d. RestoreTimestamps()               -> put original timestamps back
```

### Timestamp Preservation

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

This prevents Steam or the game from detecting the modified PAZ file via timestamp comparison. The write is invisible at the filesystem metadata level.

## Backup Management

UCM maintains a vanilla backup of the original camera entry bytes in `backups/original_backup.bin` alongside `backups/backup_meta.txt`.

### backup_meta.txt Format

```
comp_size=NNNNN orig_size=NNNNN ucm_version=X.Y.Z vanilla_verified
```

### EnsureBackup Logic

1. If backup exists with matching `comp_size`, `ucm_version`, and `vanilla_verified` flag: skip.
2. If backup exists but not vanilla-verified: re-validate by decoding and running `ValidateVanilla()`.
   - If clean: stamp `vanilla_verified` and return.
   - If tainted: delete backup, fall through to re-capture.
3. If UCM version changed: refresh backup (new version may need different baseline).
4. Read live camera bytes from PAZ, validate as vanilla, save to `original_backup.bin`.

### ValidateVanilla Checks

Five signature checks detect whether the XML has been modified by UCM or another mod:

| Check | What It Detects |
|-------|----------------|
| FoV on `Player_Basic_Default_Run` / `_Runfast` is not 45 or 53 | UCM's FoV=40 normalization |
| `Player_Basic_Default` ZL2 has `ZoomDistance="3.4"` | UCM's distance normalization |
| `Player_Basic_Default_Run` has `OffsetByVelocity OffsetLength="0"` | UCM's Steadycam sway removal |
| `Player_Weapon_LockOn` ZL has `MaxZoomDistance="30"` | UCM's lock-on ceiling raise |
| Padding comments matching `<!--[a-zA-Z0-9]{8,}-->` | UCM's ArchiveWriter size matching |

If any check fails, the backup is considered tainted and must be refreshed from a clean game install.

## Entry Finding

`CameraMod.FindCameraEntry()` locates the camera XML within the game archives:

```csharp
string pamtPath = Path.Combine(gameDir, "0010", "0.pamt");
var entries = PamtReader.Parse(pamtPath, pazDir);
return entries.FirstOrDefault(e => e.Path.Contains("playercamerapreset.xml"));
```

The camera file is always in the `0010` archive group. The PAMT index is `0010/0.pamt` and the PAZ archive is determined by the entry's `PazIndex` field (typically `0010/0.paz`).
