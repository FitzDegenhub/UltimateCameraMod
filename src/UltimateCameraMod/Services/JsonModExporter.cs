using System.Text;
using System.Text.Json;
using UltimateCameraMod.Models;

namespace UltimateCameraMod.Services;

/// <summary>
/// Generates Crimson Desert Mod Manager v8 compatible JSON patch files by
/// binary-diffing vanilla encrypted camera bytes against UCM-modified bytes.
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
    /// patch entries. Consecutive changed regions separated by <= 4 identical
    /// bytes are merged into one entry for efficiency.
    /// </summary>
    public static List<PatchChange> GeneratePatches(byte[] vanillaBytes, byte[] modifiedBytes)
    {
        if (vanillaBytes.Length != modifiedBytes.Length)
            throw new ArgumentException(
                $"Byte arrays must be the same length. Vanilla={vanillaBytes.Length}, Modified={modifiedBytes.Length}");

        const int MergeGap = 4;
        var changes = new List<PatchChange>();

        int i = 0;
        int len = vanillaBytes.Length;

        while (i < len)
        {
            if (vanillaBytes[i] == modifiedBytes[i]) { i++; continue; }

            // Start of a differing region
            int start = i;
            int end = i;

            while (end < len)
            {
                if (vanillaBytes[end] != modifiedBytes[end])
                {
                    end++;
                    continue;
                }

                // Check if the gap to the next diff is small enough to merge
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
            changes.Add(new PatchChange(start, original, patched, "Camera parameter change"));

            i = end;
        }

        return changes;
    }

    /// <summary>
    /// Ensures each change's <c>original</c> matches <paramref name="vanillaBytes"/> and that applying
    /// all changes yields <paramref name="modifiedBytes"/> exactly. If this passes, the JSON is
    /// internally consistent; if an external mod manager still skips, it is using different vanilla bytes
    /// or a different <c>game_file</c> resolution than UCM's <see cref="CameraMod.ReadVanillaBackupBytesWithMeta"/>.
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

    /// <param name="pazPayloadBase">
    /// Absolute offset of the camera entry inside the host <c>*.paz</c> file. CD JSON Mod Manager treats
    /// <c>offset</c> as absolute within that archive; UCM diffs are relative to the entry blob only.
    /// </param>
    public static string BuildJson(
        ModInfo info,
        List<PatchChange> changes,
        string gameFile,
        string sourceGroup,
        long pazPayloadBase = 0)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();

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
            writer.WriteNumber("offset", c.Offset + pazPayloadBase);
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

    // ── Full pipeline ────────────────────────────────────────────────

    /// <summary>
    /// Reads vanilla backup bytes, applies current settings to produce modified bytes,
    /// diffs them, and writes the resulting JSON patch file.
    /// </summary>
    public static (List<PatchChange> Changes, string Json) ExportFromModSet(
        string gameDir,
        ModInfo info,
        ModificationSet modSet,
        Action<string>? log = null)
    {
        log?.Invoke("Reading vanilla backup bytes...");
        var (vanillaBytes, gameFile, sourceGroup, pazPayloadOffset) = CameraMod.ReadVanillaBackupBytesWithMeta(gameDir, log);

        log?.Invoke("Building modified bytes...");
        byte[] modifiedBytes = CameraMod.BuildModifiedBytes(gameDir, modSet, log);

        log?.Invoke($"Diffing {vanillaBytes.Length} bytes...");
        var changes = GeneratePatches(vanillaBytes, modifiedBytes);
        VerifyPatchesRoundTrip(vanillaBytes, modifiedBytes, changes);
        log?.Invoke($"Found {changes.Count} patch regions ({changes.Sum(c => c.Original.Length / 2)} bytes changed); round-trip OK.");
        log?.Invoke($"Patch target: game_file={gameFile} source_group={sourceGroup} paz_payload_offset={pazPayloadOffset}");

        string json = BuildJson(info, changes, gameFile, sourceGroup, pazPayloadOffset);
        return (changes, json);
    }

    /// <summary>
    /// Applies a raw XML string through the pipeline, diffs against vanilla, and returns patch JSON.
    /// </summary>
    public static (List<PatchChange> Changes, string Json) ExportFromXml(
        string gameDir,
        ModInfo info,
        string xmlText,
        Action<string>? log = null)
    {
        log?.Invoke("Reading vanilla backup bytes...");
        var (vanillaBytes, gameFile, sourceGroup, pazPayloadOffset) = CameraMod.ReadVanillaBackupBytesWithMeta(gameDir, log);

        log?.Invoke("Building modified bytes from XML...");
        byte[] modifiedBytes = CameraMod.BuildModifiedBytesFromXml(gameDir, xmlText, log);

        log?.Invoke($"Diffing {vanillaBytes.Length} bytes...");
        var changes = GeneratePatches(vanillaBytes, modifiedBytes);
        VerifyPatchesRoundTrip(vanillaBytes, modifiedBytes, changes);
        log?.Invoke($"Found {changes.Count} patch regions ({changes.Sum(c => c.Original.Length / 2)} bytes changed); round-trip OK.");
        log?.Invoke($"Patch target: game_file={gameFile} source_group={sourceGroup} paz_payload_offset={pazPayloadOffset}");

        string json = BuildJson(info, changes, gameFile, sourceGroup, pazPayloadOffset);
        return (changes, json);
    }
}
