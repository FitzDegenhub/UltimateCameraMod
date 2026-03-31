using System.Text;
using System.Text.RegularExpressions;
using UltimateCameraMod.Paz;

namespace UltimateCameraMod.Services;

/// <summary>
/// HUD centering for ultrawide monitors. Updates UI HTML/CSS in archive 0012.
/// </summary>
public static class HudMod
{
    private const string UiArchive = "0012";
    private static readonly string[] HudHtmlPaths = { "ui/minimaphudview2.html", "ui/statusgaugeview2.html" };
    private const string CssPath = "ui/gamecommon.css";

    private const string SafeFrameId = "HUDSafeFrame";
    private const string SafeFrameClass = "ui-view-max-size-21-9";
    private const int DefaultMaxWidth = 2520;
    private static readonly string SafeFrameOpen = $"    <div id=\"{SafeFrameId}\" class=\"{SafeFrameClass}\">";
    private const string SafeFrameClose = "    </div>";

    private static readonly Regex BodyOpenRe = new(@"(<body\b[^>]*>)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex BodyCloseRe = new(@"(</body>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HtmlCommentRe = new(@"<!--.*?-->", RegexOptions.Singleline | RegexOptions.Compiled);

    private const string GaugeTracker = "<div id=\"HPGaugeTracker\"";
    private const string GaugeSkillPoint = "<div id=\"UIHudScaleSkillPointStatusGaugeContainer\"";

    public static Func<string>? BackupsDirOverride { get; set; }
    private static string BackupsDir => BackupsDirOverride?.Invoke()
        ?? Path.Combine(AppContext.BaseDirectory, "backups", "hud");

    // ── Text compaction ──────────────────────────────────────────────

    private static string CompactCss(string text)
    {
        var t = Regex.Replace(text, @"\n\s*\n+", "\n");
        t = Regex.Replace(t, @"[ \t]+\n", "\n");
        return Regex.Replace(t, @"\s*([{}:;,])\s*", "$1");
    }

    private static string CompactText(string entryPath, string text)
    {
        if (entryPath.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            return CompactCss(text);
        var t = text;
        if (entryPath.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            t = HtmlCommentRe.Replace(t, "");
        return Regex.Replace(t, @"\n\s*\n+", "\n");
    }

    // ── PAZ I/O ──────────────────────────────────────────────────────

    private static List<PazEntry> FindUiEntries(string gameDir)
    {
        string pamt = Path.Combine(gameDir, UiArchive, "0.pamt");
        string pazDir = Path.Combine(gameDir, UiArchive);
        if (!File.Exists(pamt))
            throw new FileNotFoundException($"UI archive not found: {pamt}");

        var entries = PamtReader.Parse(pamt, pazDir);
        var byPath = new Dictionary<string, PazEntry>();
        foreach (var e in entries)
        {
            string key = e.Path.ToLowerInvariant();
            if (!byPath.ContainsKey(key))
                byPath[key] = e;
        }

        var allPaths = HudHtmlPaths.Append(CssPath);
        var result = new List<PazEntry>();
        foreach (var p in allPaths)
        {
            if (!byPath.TryGetValue(p, out var entry))
                throw new FileNotFoundException($"{p} not found in archive");
            result.Add(entry);
        }
        return result;
    }

    private static byte[] ReadRaw(PazEntry entry)
    {
        using var fs = new FileStream(entry.PazFile, FileMode.Open, FileAccess.Read);
        fs.Seek(entry.Offset, SeekOrigin.Begin);
        byte[] data = new byte[entry.CompSize];
        int totalRead = 0;
        while (totalRead < data.Length)
        {
            int n = fs.Read(data, totalRead, data.Length - totalRead);
            if (n == 0) throw new EndOfStreamException();
            totalRead += n;
        }
        return data;
    }

    private static (string Text, bool Encrypted) DecodeText(PazEntry entry, byte[] raw)
    {
        if (entry.Compressed)
        {
            if (entry.CompressionType != 2)
                throw new InvalidOperationException($"Unsupported compression for {entry.Path}");
            try
            {
                byte[] plain = CompressionUtils.Lz4Decompress(raw, entry.OrigSize);
                try { return (Encoding.UTF8.GetString(plain).TrimEnd('\0'), false); }
                catch (DecoderFallbackException) { }
            }
            catch { }

            try
            {
                byte[] dec = AssetCodec.Decode(raw, Path.GetFileName(entry.Path));
                byte[] dp = CompressionUtils.Lz4Decompress(dec, entry.OrigSize);
                return (Encoding.UTF8.GetString(dp).TrimEnd('\0'), true);
            }
            catch { }

            byte[] fallback = CompressionUtils.Lz4Decompress(raw, entry.OrigSize);
            return (Encoding.UTF8.GetString(fallback).TrimEnd('\0'), false);
        }

        try { return (Encoding.UTF8.GetString(raw).TrimEnd('\0'), false); }
        catch
        {
            byte[] dec = AssetCodec.Decode(raw, Path.GetFileName(entry.Path));
            return (Encoding.UTF8.GetString(dec).TrimEnd('\0'), true);
        }
    }

    // ── HTML modification ────────────────────────────────────────────

    private static string ModifyHtml(string entryPath, string text)
    {
        if (text.Contains(SafeFrameId)) return text;
        if (!BodyOpenRe.IsMatch(text)) throw new InvalidOperationException($"No <body> in {entryPath}");
        if (!BodyCloseRe.IsMatch(text)) throw new InvalidOperationException($"No </body> in {entryPath}");

        string modified = BodyOpenRe.Replace(text, $"$1\n{SafeFrameOpen}\n", 1);

        if (entryPath.Equals("ui/statusgaugeview2.html", StringComparison.OrdinalIgnoreCase))
        {
            if (!modified.Contains(GaugeTracker)) throw new InvalidOperationException("HPGaugeTracker not found");
            if (!modified.Contains(GaugeSkillPoint)) throw new InvalidOperationException("SkillPointContainer not found");
            modified = modified.Replace(GaugeTracker, $"{SafeFrameClose}\n{GaugeTracker}");
            modified = modified.Replace(GaugeSkillPoint, $"{SafeFrameOpen}\n{GaugeSkillPoint}");
        }

        modified = BodyCloseRe.Replace(modified, $"{SafeFrameClose}\n$1", 1);
        return modified;
    }

    // ── CSS modification ─────────────────────────────────────────────

    private static string ModifyCss(string text, int maxWidth = DefaultMaxWidth, int maxHeight = 0)
    {
        var pat = new Regex(@"(\.ui-view-max-size-21-9\s*\{)([^}]*)(\})", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var m = pat.Match(text);
        if (!m.Success) throw new InvalidOperationException(".ui-view-max-size-21-9 not found in CSS");

        string body = m.Groups[2].Value;

        var widthRe = new Regex(@"(max-width\s*:\s*)([^;]+)(;)", RegexOptions.IgnoreCase);
        string newBody;
        if (widthRe.IsMatch(body))
            newBody = widthRe.Replace(body, $"${{1}}{maxWidth}px${{3}}", 1);
        else
        {
            newBody = body.TrimEnd();
            if (newBody.Length > 0 && !newBody.EndsWith(';')) newBody += ";";
            newBody += $" max-width: {maxWidth}px;";
        }

        if (maxHeight > 0)
        {
            var heightRe = new Regex(@"max-height\s*:\s*[^;]+;", RegexOptions.IgnoreCase);
            var marginRe = new Regex(@"margin\s*:\s*[^;]+;", RegexOptions.IgnoreCase);
            newBody = heightRe.Replace(newBody, "");
            newBody = marginRe.Replace(newBody, "");
            newBody = newBody.TrimEnd();
            if (newBody.Length > 0 && !newBody.EndsWith(';')) newBody += ";";
            newBody += $" max-height: {maxHeight}px; margin: auto;";
        }

        return text[..m.Groups[2].Index] + newBody + text[(m.Groups[2].Index + m.Groups[2].Length)..];
    }

    // ── Payload building ─────────────────────────────────────────────

    private static byte[] BuildPayload(string modifiedText, PazEntry entry, bool encrypted)
    {
        string compacted = CompactText(entry.Path, modifiedText);
        var candidates = new List<string>();
        if (compacted != modifiedText) candidates.Add(compacted);
        candidates.Add(modifiedText);

        Exception? lastErr = null;
        foreach (var candidate in candidates)
        {
            byte[] plaintext = Encoding.UTF8.GetBytes(candidate);
            try
            {
                byte[] payload;
                if (entry.Compressed)
                {
                    byte[] adjusted = ArchiveWriter.MatchCompressedSize(plaintext, entry.CompSize, entry.OrigSize);
                    payload = CompressionUtils.Lz4Compress(adjusted);
                    if (payload.Length != entry.CompSize)
                        throw new InvalidOperationException($"comp_size mismatch: {payload.Length} != {entry.CompSize}");
                }
                else
                {
                    if (plaintext.Length > entry.CompSize)
                        throw new InvalidOperationException($"Too large: {plaintext.Length} > {entry.CompSize}");
                    payload = new byte[entry.CompSize];
                    Array.Copy(plaintext, payload, plaintext.Length);
                }

                if (encrypted)
                {
                    payload = AssetCodec.Encode(payload, Path.GetFileName(entry.Path));
                    if (payload.Length != entry.CompSize)
                        throw new InvalidOperationException("Encrypted size mismatch");
                }
                return payload;
            }
            catch (Exception e) { lastErr = e; }
        }

        throw new InvalidOperationException($"Cannot fit {entry.Path}: {lastErr?.Message}");
    }

    // ── Backup management ────────────────────────────────────────────

    private static bool BackupExists() => File.Exists(Path.Combine(BackupsDir, "meta.txt"));

    private static void SaveBackups(List<PazEntry> entries)
    {
        Directory.CreateDirectory(BackupsDir);
        foreach (var entry in entries)
        {
            byte[] raw = ReadRaw(entry);
            string fname = entry.Path.Replace('/', '_').Replace('\\', '_') + ".bin";
            File.WriteAllBytes(Path.Combine(BackupsDir, fname), raw);
        }
        var meta = string.Join('\n', entries.Select(e => $"{e.Path}|{e.CompSize}|{e.OrigSize}|{e.Offset}"));
        File.WriteAllText(Path.Combine(BackupsDir, "meta.txt"), meta);
    }

    private static byte[]? LoadBackup(PazEntry entry)
    {
        string fname = entry.Path.Replace('/', '_').Replace('\\', '_') + ".bin";
        string path = Path.Combine(BackupsDir, fname);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    // ── Live detection ─────────────────────────────────────────────

    public static bool DetectHudModified(string gameDir)
    {
        try
        {
            var entries = FindUiEntries(gameDir);
            foreach (var entry in entries)
            {
                if (!entry.Path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                    continue;
                byte[] raw = ReadRaw(entry);
                var (text, _) = DecodeText(entry, raw);
                if (text.Contains(SafeFrameId))
                    return true;
            }
        }
        catch { }
        return false;
    }

    // ── Public API ───────────────────────────────────────────────────

    public static Dictionary<string, object> InstallCenteredHud(string gameDir,
        int maxWidth = DefaultMaxWidth, int maxHeight = 0, Action<string>? log = null)
    {
        log?.Invoke($"[HUD] Finding UI entries (max-width: {maxWidth}px" + (maxHeight > 0 ? $", max-height: {maxHeight}px" : "") + ")...");
        var entries = FindUiEntries(gameDir);

        var decoded = new Dictionary<string, (string Text, bool Encrypted)>();
        foreach (var entry in entries)
        {
            byte[] raw = ReadRaw(entry);
            decoded[entry.Path] = DecodeText(entry, raw);
        }

        bool allInstalled = entries
            .Where(e => e.Path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            .All(e => decoded[e.Path].Text.Contains(SafeFrameId));
        if (allInstalled)
        {
            log?.Invoke("[HUD] Already installed.");
            return new() { ["status"] = "ok" };
        }

        if (!BackupExists())
        {
            log?.Invoke("[HUD] Saving backups...");
            SaveBackups(entries);
        }

        var writes = new List<(PazEntry Entry, byte[] Payload)>();
        foreach (var entry in entries)
        {
            var (text, enc) = decoded[entry.Path];
            string modified;
            if (entry.Path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                modified = ModifyHtml(entry.Path, text);
            else if (entry.Path.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                modified = ModifyCss(text, maxWidth, maxHeight);
            else continue;

            if (modified == text) continue;
            byte[] payload = BuildPayload(modified, entry, enc);
            writes.Add((entry, payload));
        }

        foreach (var (entry, payload) in writes)
        {
            var restoreTs = ArchiveWriter.SaveTimestamps(entry.PazFile);
            using (var fs = new FileStream(entry.PazFile, FileMode.Open, FileAccess.Write))
            {
                fs.Seek(entry.Offset, SeekOrigin.Begin);
                fs.Write(payload);
            }
            restoreTs();
            log?.Invoke($"[HUD] Updated: {entry.Path}");
        }

        log?.Invoke("[HUD] Done!");
        return new() { ["status"] = "ok" };
    }

    public static Dictionary<string, object> RestoreHud(string gameDir, Action<string>? log = null)
    {
        if (!BackupExists())
            return new() { ["status"] = "no_backup" };

        var entries = FindUiEntries(gameDir);

        foreach (var entry in entries)
        {
            byte[]? data = LoadBackup(entry);
            if (data == null) return new() { ["status"] = "error" };
            if (data.Length != entry.CompSize) return new() { ["status"] = "stale_backup" };
        }

        foreach (var entry in entries)
        {
            byte[] data = LoadBackup(entry)!;
            var restoreTs = ArchiveWriter.SaveTimestamps(entry.PazFile);
            using (var fs = new FileStream(entry.PazFile, FileMode.Open, FileAccess.Write))
            {
                fs.Seek(entry.Offset, SeekOrigin.Begin);
                fs.Write(data);
            }
            restoreTs();
            log?.Invoke($"[HUD] Restored: {entry.Path}");
        }

        return new() { ["status"] = "ok" };
    }
}
