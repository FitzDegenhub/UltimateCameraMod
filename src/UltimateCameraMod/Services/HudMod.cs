// HUD centering logic adapted from CentreHUD (0BSD license).
// Ported from Python to C# for UCM integration.
// Original mod: https://github.com/bdsm/centrehud

using System.Text;
using System.Text.RegularExpressions;
using UltimateCameraMod.Paz;

namespace UltimateCameraMod.Services;

/// <summary>
/// Modifies HUD HTML/CSS files in PAZ archive 0012 to center gameplay UI within a max-width container.
/// Independent from CameraMod (which handles archive 0010).
/// </summary>
public static class HudMod
{
    private const string ArchiveDir = "0012";
    private const string SafeFrameId = "HUDSafeFrame";
    private const string CssClassName = "ucm-hud-center";

    private static readonly string[] TargetPaths =
    {
        "ui/minimaphudview2.html",
        "ui/statusgaugeview2.html",
        "ui/gamecommon.css"
    };

    // Markers in statusgaugeview2.html where the safe frame must be split
    private const string HpGaugeTrackerMarker = "<div id=\"HPGaugeTracker\"";
    private const string SkillPointMarker = "<div id=\"UIHudScaleSkillPointStatusGaugeContainer\"";

    public static Func<string>? BackupsDirOverride { get; set; }

    private static string HudBackupsDir
    {
        get
        {
            string baseDir = BackupsDirOverride?.Invoke() ?? Path.Combine(AppContext.BaseDirectory, "backups");
            string dir = Path.Combine(baseDir, "hud");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    // ── Entry discovery ─────────────────────────────────────────────

    public static List<PazEntry> FindHudEntries(string gameDir)
    {
        string pamtPath = Path.Combine(gameDir, ArchiveDir, "0.pamt");
        string pazDir = Path.Combine(gameDir, ArchiveDir);

        if (!File.Exists(pamtPath))
            throw new FileNotFoundException(
                $"HUD archive index not found at {pamtPath}. " +
                "Verify your game files on Steam.");

        var allEntries = PamtReader.Parse(pamtPath, pazDir);
        var targetSet = new HashSet<string>(TargetPaths, StringComparer.OrdinalIgnoreCase);
        var found = allEntries.Where(e => targetSet.Contains(e.Path)).ToList();

        if (found.Count != TargetPaths.Length)
        {
            var missing = TargetPaths.Where(p => !found.Any(e =>
                string.Equals(e.Path, p, StringComparison.OrdinalIgnoreCase)));
            throw new InvalidOperationException(
                $"Missing HUD entries in archive: {string.Join(", ", missing)}");
        }

        return found;
    }

    // ── Encryption detection ────────────────────────────────────────

    private static byte[] ReadEntryBytes(PazEntry entry)
    {
        using var fs = new FileStream(entry.PazFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        fs.Seek(entry.Offset, SeekOrigin.Begin);
        byte[] raw = new byte[entry.CompSize];
        int totalRead = 0;
        while (totalRead < raw.Length)
        {
            int n = fs.Read(raw, totalRead, raw.Length - totalRead);
            if (n == 0) throw new EndOfStreamException();
            totalRead += n;
        }
        return raw;
    }

    /// <summary>
    /// Decodes a HUD entry, trying unencrypted LZ4 first, then ChaCha20+LZ4.
    /// Returns the decoded text and whether encryption was detected.
    /// </summary>
    private static (string Text, bool WasEncrypted) DecodeHudEntry(PazEntry entry, byte[] rawBytes)
    {
        string filename = Path.GetFileName(entry.Path);

        if (!entry.Compressed)
        {
            // Uncompressed: try raw decode, then encrypted
            try
            {
                string text = Encoding.UTF8.GetString(rawBytes).TrimEnd('\0');
                if (LooksLikeText(text)) return (text, false);
            }
            catch { }
            try
            {
                byte[] dec = AssetCodec.Decode(rawBytes, filename);
                return (Encoding.UTF8.GetString(dec).TrimEnd('\0'), true);
            }
            catch { }
            throw new InvalidOperationException($"Could not decode HUD entry: {entry.Path}");
        }

        // Compressed: try LZ4 first (unencrypted), then ChaCha20+LZ4
        try
        {
            byte[] plain = CompressionUtils.Lz4Decompress(rawBytes, entry.OrigSize);
            string text = Encoding.UTF8.GetString(plain).TrimEnd('\0');
            if (LooksLikeText(text)) return (text, false);
        }
        catch { }

        try
        {
            byte[] decrypted = AssetCodec.Decode(rawBytes, filename);
            byte[] plain = CompressionUtils.Lz4Decompress(decrypted, entry.OrigSize);
            string text = Encoding.UTF8.GetString(plain).TrimEnd('\0');
            return (text, true);
        }
        catch { }

        throw new InvalidOperationException(
            $"Could not decode HUD entry: {entry.Path}. " +
            "Both encrypted and unencrypted decode paths failed.");
    }

    private static bool LooksLikeText(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        // HTML starts with < or whitespace then <, CSS contains { or starts with . or @
        string trimmed = s.TrimStart();
        return trimmed.StartsWith('<') || trimmed.StartsWith('.') ||
               trimmed.StartsWith('@') || trimmed.Contains('{');
    }

    // ── Text compaction ─────────────────────────────────────────────

    private static string CompactHtml(string html)
    {
        // Strip HTML comments
        string result = Regex.Replace(html, @"<!--.*?-->", "", RegexOptions.Singleline);
        // Collapse multiple blank lines
        result = Regex.Replace(result, @"\n\s*\n+", "\n");
        return result;
    }

    private static string CompactCss(string css)
    {
        // Strip CSS comments
        string result = Regex.Replace(css, @"/\*.*?\*/", "", RegexOptions.Singleline);
        // Remove blank lines
        result = Regex.Replace(result, @"\n\s*\n+", "\n");
        // Remove trailing whitespace on lines
        result = Regex.Replace(result, @"[ \t]+\n", "\n");
        // Remove whitespace around syntax characters
        result = Regex.Replace(result, @"\s*([{}:;,])\s*", "$1");
        return result;
    }

    // ── HTML injection ──────────────────────────────────────────────

    private static string BuildSafeFrameOpen()
        => $"    <div id=\"{SafeFrameId}\" class=\"{CssClassName}\">";

    private static string SafeFrameClose => "    </div>";

    /// <summary>Injects the safe frame wrapper into minimaphudview2.html.</summary>
    private static string InjectSafeFrameMinimap(string html, int maxWidth)
    {
        string compacted = CompactHtml(html);
        string sfOpen = BuildSafeFrameOpen();

        // Insert after <body...>
        var bodyOpen = Regex.Match(compacted, @"(<body\b[^>]*>)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!bodyOpen.Success)
            throw new InvalidOperationException("Opening <body> tag not found in minimaphudview2.html");

        string result = compacted.Insert(bodyOpen.Index + bodyOpen.Length, "\n" + sfOpen + "\n");

        // Insert before </body>
        var bodyClose = Regex.Match(result, @"(</body>)", RegexOptions.IgnoreCase);
        if (!bodyClose.Success)
            throw new InvalidOperationException("Closing </body> tag not found in minimaphudview2.html");

        result = result.Insert(bodyClose.Index, SafeFrameClose + "\n");

        return result;
    }

    /// <summary>
    /// Injects the safe frame wrapper into statusgaugeview2.html with split handling.
    /// The safe frame closes before HPGaugeTracker and reopens before SkillPointStatusGaugeContainer.
    /// </summary>
    private static string InjectSafeFrameStatus(string html, int maxWidth)
    {
        string compacted = CompactHtml(html);
        string sfOpen = BuildSafeFrameOpen();

        // Insert after <body...>
        var bodyOpen = Regex.Match(compacted, @"(<body\b[^>]*>)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!bodyOpen.Success)
            throw new InvalidOperationException("Opening <body> tag not found in statusgaugeview2.html");

        string result = compacted.Insert(bodyOpen.Index + bodyOpen.Length, "\n" + sfOpen + "\n");

        // Close before HPGaugeTracker
        int hpIdx = result.IndexOf(HpGaugeTrackerMarker, StringComparison.Ordinal);
        if (hpIdx < 0)
            throw new InvalidOperationException("HPGaugeTracker marker not found in statusgaugeview2.html");
        result = result.Insert(hpIdx, SafeFrameClose + "\n");

        // Reopen before SkillPointStatusGaugeContainer
        int spIdx = result.IndexOf(SkillPointMarker, StringComparison.Ordinal);
        if (spIdx < 0)
            throw new InvalidOperationException("SkillPointStatusGaugeContainer marker not found in statusgaugeview2.html");
        result = result.Insert(spIdx, sfOpen + "\n");

        // Close before </body>
        var bodyClose = Regex.Match(result, @"(</body>)", RegexOptions.IgnoreCase);
        if (!bodyClose.Success)
            throw new InvalidOperationException("Closing </body> tag not found in statusgaugeview2.html");
        result = result.Insert(bodyClose.Index, SafeFrameClose + "\n");

        return result;
    }

    /// <summary>Appends the safe frame CSS rule to gamecommon.css.</summary>
    private static string InjectCssRule(string css, int maxWidth)
    {
        string compacted = CompactCss(css);
        // Remove any existing UCM HUD rule
        compacted = Regex.Replace(compacted, @"\.ucm-hud-center\{[^}]*\}", "");
        // Append the rule (compact, no whitespace)
        string rule = $".{CssClassName}{{max-width:{maxWidth}px;margin:0 auto;width:100%;position:relative;}}";
        return compacted.TrimEnd() + "\n" + rule;
    }

    // ── Backup management ───────────────────────────────────────────

    private static string BackupFileName(string entryPath)
        => entryPath.Replace("/", "__") + ".bin";

    private static string MetaPath => Path.Combine(HudBackupsDir, "hud_meta.txt");

    private static void EnsureHudBackup(List<PazEntry> entries, Action<string>? log)
    {
        foreach (var entry in entries)
        {
            string backupPath = Path.Combine(HudBackupsDir, BackupFileName(entry.Path));
            if (File.Exists(backupPath)) continue;

            log?.Invoke($"Backing up {entry.Path}...");
            byte[] raw = ReadEntryBytes(entry);
            File.WriteAllBytes(backupPath, raw);
        }

        // Save metadata for restore
        var lines = entries.Select(e =>
            $"{e.Path}|{e.CompSize}|{e.OrigSize}|{e.Offset}|{e.PazFile}");
        File.WriteAllLines(MetaPath, lines);
    }

    // ── Install ─────────────────────────────────────────────────────

    public static void InstallHud(string gameDir, int maxWidth, Action<string>? log)
    {
        var entries = FindHudEntries(gameDir);
        EnsureHudBackup(entries, log);

        foreach (var entry in entries)
        {
            log?.Invoke($"Processing {entry.Path}...");

            byte[] rawBytes = ReadEntryBytes(entry);
            var (text, wasEncrypted) = DecodeHudEntry(entry, rawBytes);

            // Inject modifications based on file type
            string modified;
            string pathLower = entry.Path.ToLowerInvariant();
            if (pathLower == "ui/minimaphudview2.html")
                modified = InjectSafeFrameMinimap(text, maxWidth);
            else if (pathLower == "ui/statusgaugeview2.html")
                modified = InjectSafeFrameStatus(text, maxWidth);
            else if (pathLower == "ui/gamecommon.css")
                modified = InjectCssRule(text, maxWidth);
            else
                continue;

            // Encode to bytes
            byte[] modifiedBytes = Encoding.UTF8.GetBytes(modified);

            // Size-match: use CSS method for CSS, XML/HTML method for HTML
            byte[] sized = entry.IsCss
                ? ArchiveWriter.MatchCompressedSizeCss(modifiedBytes, entry.CompSize, entry.OrigSize)
                : ArchiveWriter.MatchCompressedSize(modifiedBytes, entry.CompSize, entry.OrigSize);

            // Compress
            byte[] compressed = CompressionUtils.Lz4Compress(sized);
            if (compressed.Length != entry.CompSize)
                throw new InvalidOperationException(
                    $"Compressed size mismatch for {entry.Path}: {compressed.Length} != {entry.CompSize}");

            // Encrypt if needed
            byte[] payload = wasEncrypted
                ? AssetCodec.Encode(compressed, Path.GetFileName(entry.Path))
                : compressed;

            // Write to archive
            log?.Invoke($"Writing {entry.Path}...");
            ArchiveWriter.UpdateEntry(entry, payload);
        }

        log?.Invoke("HUD centering installed.");
    }

    // ── Restore ─────────────────────────────────────────────────────

    public static void RestoreHud(string gameDir, Action<string>? log)
    {
        if (!File.Exists(MetaPath))
        {
            log?.Invoke("No HUD backup found, skipping restore.");
            return;
        }

        var entries = FindHudEntries(gameDir);

        foreach (var entry in entries)
        {
            string backupPath = Path.Combine(HudBackupsDir, BackupFileName(entry.Path));
            if (!File.Exists(backupPath))
            {
                log?.Invoke($"Backup missing for {entry.Path}, skipping.");
                continue;
            }

            byte[] backup = File.ReadAllBytes(backupPath);
            if (backup.Length != entry.CompSize)
            {
                log?.Invoke($"Backup size mismatch for {entry.Path} ({backup.Length} != {entry.CompSize}), skipping.");
                continue;
            }

            log?.Invoke($"Restoring {entry.Path}...");
            ArchiveWriter.UpdateEntry(entry, backup);
        }

        log?.Invoke("HUD restored to vanilla.");
    }

    /// <summary>Returns true if HUD backups exist (HUD was previously installed).</summary>
    public static bool HasHudBackup()
        => File.Exists(MetaPath);
}
