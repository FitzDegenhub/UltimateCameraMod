using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UltimateCameraMod.Models;

namespace UltimateCameraMod.Services;

/// <summary>
/// Generates Crimson Desert JSON Mod Manager compatible patch files by binary-diffing the
/// <strong>decompressed</strong> camera entry payload (same buffer the manager patches after decompress).
/// </summary>
public static class JsonModExporter
{
    public record PatchChange(long Offset, string Original, string Patched, string Label);

    public record ModInfo(
        string Title,
        string Version,
        string Author,
        string Description,
        string NexusUrl);

    // ── Diff engine ──────────────────────────────────────────────────

    /// <summary>
    /// Walks two equal-length byte arrays and groups differing regions into
    /// patch entries. Consecutive changed regions separated by &lt;= 4 identical
    /// bytes are merged into one entry for efficiency.
    /// </summary>
    public static List<PatchChange> GeneratePatches(byte[] vanillaBytes, byte[] modifiedBytes)
    {
        if (vanillaBytes.Length != modifiedBytes.Length)
            throw new ArgumentException(
                $"Byte arrays must be the same length. Vanilla={vanillaBytes.Length}, Modified={modifiedBytes.Length}");

        var xmlContext = BuildXmlOffsetMap(vanillaBytes);

        const int MergeGap = 4;
        var changes = new List<PatchChange>();

        int i = 0;
        int len = vanillaBytes.Length;

        while (i < len)
        {
            if (vanillaBytes[i] == modifiedBytes[i]) { i++; continue; }

            int start = i;
            int end = i;

            while (end < len)
            {
                if (vanillaBytes[end] != modifiedBytes[end])
                {
                    end++;
                    continue;
                }

                int gapEnd = end;
                while (gapEnd < len && vanillaBytes[gapEnd] == modifiedBytes[gapEnd])
                    gapEnd++;

                if (gapEnd - end <= MergeGap && gapEnd < len)
                {
                    end = gapEnd;
                    continue;
                }

                break;
            }

            string original = Convert.ToHexString(vanillaBytes, start, end - start).ToLowerInvariant();
            string patched = Convert.ToHexString(modifiedBytes, start, end - start).ToLowerInvariant();
            string label = LabelForOffset(xmlContext, start);
            changes.Add(new PatchChange(start, original, patched, label));

            i = end;
        }

        return changes;
    }

    // ── XML-aware labeling ────────────────────────────────────────────

    private static readonly Regex OpenTagRx = new(
        @"<(\w[\w.:-]*)(?:\s[^>]*)?>",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex CloseTagRx = new(
        @"</(\w[\w.:-]*)>",
        RegexOptions.Compiled);

    /// <summary>
    /// Builds a sorted list of (byteOffset, xmlPath) by scanning the UTF-8 text
    /// portion of the decompressed payload. Falls back gracefully if the buffer
    /// is not valid XML (e.g. trailing zero-padding region).
    /// </summary>
    private static List<(int Offset, string Path)> BuildXmlOffsetMap(byte[] buffer)
    {
        var map = new List<(int, string)>();
        string text;
        try
        {
            int xmlEnd = buffer.Length;
            while (xmlEnd > 0 && buffer[xmlEnd - 1] == 0) xmlEnd--;
            if (xmlEnd == 0) return map;
            text = Encoding.UTF8.GetString(buffer, 0, xmlEnd);
        }
        catch
        {
            return map;
        }

        var stack = new Stack<string>();
        int charPos = 0;

        while (charPos < text.Length)
        {
            if (text[charPos] != '<') { charPos++; continue; }

            var closeMatch = CloseTagRx.Match(text, charPos);
            if (closeMatch.Success && closeMatch.Index == charPos)
            {
                if (stack.Count > 0) stack.Pop();
                charPos += closeMatch.Length;
                continue;
            }

            var openMatch = OpenTagRx.Match(text, charPos);
            if (openMatch.Success && openMatch.Index == charPos)
            {
                string tagName = openMatch.Groups[1].Value;
                bool selfClosing = openMatch.Value.EndsWith("/>");

                if (!selfClosing)
                    stack.Push(tagName);

                int byteOffset = Encoding.UTF8.GetByteCount(text.AsSpan(0, charPos));
                var parts = stack.Reverse().ToArray();
                string path = parts.Length <= 3
                    ? string.Join(" / ", parts)
                    : string.Join(" / ", parts.Skip(parts.Length - 3));
                map.Add((byteOffset, path));

                charPos += openMatch.Length;
                continue;
            }

            charPos++;
        }

        return map;
    }

    private static string LabelForOffset(List<(int Offset, string Path)> map, int byteOffset)
    {
        if (map.Count == 0)
            return "Camera parameter change";

        int lo = 0, hi = map.Count - 1, best = -1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (map[mid].Offset <= byteOffset)
            {
                best = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return best >= 0 ? map[best].Path : "Camera parameter change";
    }

    /// <summary>
    /// Ensures each change's <c>original</c> matches <paramref name="vanillaBytes"/> and that applying
    /// all changes yields <paramref name="modifiedBytes"/> exactly.
    /// </summary>
    private static void VerifyPatchesRoundTrip(byte[] vanillaBytes, byte[] modifiedBytes, List<PatchChange> changes)
    {
        byte[] reconstructed = (byte[])vanillaBytes.Clone();
        foreach (var c in changes)
        {
            if (c.Original.Length % 2 != 0 || c.Patched.Length % 2 != 0)
                throw new InvalidOperationException("Patch hex strings must have even length.");

            int n = c.Original.Length / 2;
            if (c.Offset < 0 || c.Offset + n > reconstructed.Length)
                throw new InvalidOperationException(
                    $"Patch offset {c.Offset} (length {n}) is out of range for buffer length {reconstructed.Length}.");

            byte[] orig = Convert.FromHexString(c.Original);
            byte[] patch = Convert.FromHexString(c.Patched);
            if (orig.Length != patch.Length)
                throw new InvalidOperationException("Patch original and patched byte lengths differ.");

            for (int i = 0; i < n; i++)
            {
                if (reconstructed[c.Offset + i] != orig[i])
                {
                    throw new InvalidOperationException(
                        $"Internal verify failed at offset {c.Offset + i}: patch 'original' does not match vanilla snapshot.");
                }
            }

            Array.Copy(patch, 0, reconstructed, (int)c.Offset, n);
        }

        if (!reconstructed.AsSpan().SequenceEqual(modifiedBytes))
        {
            throw new InvalidOperationException(
                "Internal verify failed: applying exported patches to vanilla does not reproduce modified bytes.");
        }
    }

    // ── JSON builder ─────────────────────────────────────────────────

    /// <summary>
    /// CD JSON Mod Manager v1: <c>offset</c> is absolute within the decompressed entry buffer (0-based).
    /// </summary>
    public static string BuildJson(
        ModInfo info,
        List<PatchChange> changes,
        string gameFile,
        string sourceGroup)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();

        // Top-level metadata for CDUMM and other managers that read root-level keys
        writer.WriteString("name", info.Title);
        writer.WriteString("version", info.Version);
        writer.WriteString("author", info.Author);
        writer.WriteString("description", info.Description);

        // Nested modinfo block for CD JSON Mod Manager / UCM compatibility
        writer.WriteStartObject("modinfo");
        writer.WriteString("title", info.Title);
        writer.WriteString("version", info.Version);
        writer.WriteString("author", info.Author);
        writer.WriteString("description", info.Description);
        writer.WriteString("nexus_url", info.NexusUrl);
        writer.WriteEndObject();

        writer.WriteStartArray("patches");
        writer.WriteStartObject();
        writer.WriteString("game_file", gameFile);
        writer.WriteString("source_group", sourceGroup);

        writer.WriteStartArray("changes");
        foreach (var c in changes)
        {
            writer.WriteStartObject();
            writer.WriteNumber("offset", c.Offset);
            writer.WriteString("original", c.Original);
            writer.WriteString("patched", c.Patched);
            writer.WriteString("label", c.Label);
            writer.WriteEndObject();
        }
        writer.WriteEndArray(); // changes

        writer.WriteEndObject();
        writer.WriteEndArray(); // patches

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    /// <summary>
    /// Builds a multi-preset JSON where CDUMM shows a radio-button picker at import time.
    /// Each preset becomes a separate <c>patches[]</c> entry targeting the same <c>game_file</c>,
    /// with every change label prefixed by <c>[PresetName]</c>.
    /// Requires at least two presets to trigger CDUMM's <c>_detect_preset_groups</c>.
    /// </summary>
    public static string BuildMultiPresetJson(
        ModInfo info,
        IReadOnlyList<(string PresetName, List<PatchChange> Changes)> presets,
        string gameFile,
        string sourceGroup)
    {
        if (presets.Count < 2)
            throw new ArgumentException("Multi-preset export requires at least two presets.");

        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();

        writer.WriteString("name", info.Title);
        writer.WriteString("version", info.Version);
        writer.WriteString("author", info.Author);
        writer.WriteString("description", info.Description);

        writer.WriteStartObject("modinfo");
        writer.WriteString("title", info.Title);
        writer.WriteString("version", info.Version);
        writer.WriteString("author", info.Author);
        writer.WriteString("description", info.Description);
        writer.WriteString("nexus_url", info.NexusUrl);
        writer.WriteEndObject();

        writer.WriteStartArray("patches");
        foreach (var (presetName, changes) in presets)
        {
            writer.WriteStartObject();
            writer.WriteString("game_file", gameFile);
            writer.WriteString("source_group", sourceGroup);

            writer.WriteStartArray("changes");
            foreach (var c in changes)
            {
                writer.WriteStartObject();
                writer.WriteNumber("offset", c.Offset);
                writer.WriteString("original", c.Original);
                writer.WriteString("patched", c.Patched);
                writer.WriteString("label", $"[{presetName}] {c.Label}");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    // ── Full pipeline ────────────────────────────────────────────────

    /// <summary>
    /// Reads vanilla backup, applies current settings, diffs decompressed payloads, writes JSON.
    /// </summary>
    public static (List<PatchChange> Changes, string Json) ExportFromModSet(
        string gameDir,
        ModInfo info,
        ModificationSet modSet,
        Action<string>? log = null)
    {
        log?.Invoke("Reading stored vanilla backup (decompressed baseline)...");
        var (baselineBytes, gameFile, sourceGroup) = CameraMod.ReadStoredVanillaDecompressedPayloadForJson(gameDir, log);

        log?.Invoke("Building modified decompressed payload...");
        byte[] modifiedBytes = CameraMod.BuildModifiedDecompressedPayload(gameDir, modSet, log);

        log?.Invoke($"Diffing {baselineBytes.Length} bytes (decompressed)...");
        var changes = GeneratePatches(baselineBytes, modifiedBytes);
        VerifyPatchesRoundTrip(baselineBytes, modifiedBytes, changes);
        log?.Invoke($"Found {changes.Count} patch regions ({changes.Sum(c => c.Original.Length / 2)} bytes changed); round-trip OK.");
        log?.Invoke($"Patch target: game_file={gameFile} source_group={sourceGroup} (offsets in decompressed buffer)");

        string json = BuildJson(info, changes, gameFile, sourceGroup);
        return (changes, json);
    }

    /// <summary>
    /// Applies raw XML through the pipeline, diffs decompressed payloads, returns patch JSON.
    /// </summary>
    public static (List<PatchChange> Changes, string Json) ExportFromXml(
        string gameDir,
        ModInfo info,
        string xmlText,
        Action<string>? log = null)
    {
        log?.Invoke("Reading live camera chunk (decompressed baseline)...");
        var (baselineBytes, gameFile, sourceGroup) = CameraMod.ReadLiveCameraDecompressedPayloadForJson(gameDir, log);

        log?.Invoke("Building modified decompressed payload from XML...");
        byte[] modifiedBytes = CameraMod.BuildModifiedDecompressedPayloadFromXml(gameDir, xmlText, log);

        log?.Invoke($"Diffing {baselineBytes.Length} bytes (decompressed)...");
        var changes = GeneratePatches(baselineBytes, modifiedBytes);
        VerifyPatchesRoundTrip(baselineBytes, modifiedBytes, changes);
        log?.Invoke($"Found {changes.Count} patch regions ({changes.Sum(c => c.Original.Length / 2)} bytes changed); round-trip OK.");
        log?.Invoke($"Patch target: game_file={gameFile} source_group={sourceGroup} (offsets in decompressed buffer)");

        string json = BuildJson(info, changes, gameFile, sourceGroup);
        return (changes, json);
    }
}
