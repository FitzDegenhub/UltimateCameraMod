using System.Text;
using System.Text.RegularExpressions;
using UltimateCameraMod.Models;
using UltimateCameraMod.Paz;

namespace UltimateCameraMod.Services;

/// <summary>
/// Camera configuration engine. Reads XML from archive, applies settings,
/// size-matches, compresses, encodes, and writes back.
/// </summary>
public static class CameraMod
{
    private static readonly Regex TagRe = new(@"<(\w+)\s+([^>]*?)(/?)>", RegexOptions.Compiled);
    private static readonly Regex BareOpenRe = new(@"^<(\w+)>$", RegexOptions.Compiled);

    private static readonly HashSet<string> SubElementTags = new()
    {
        "CameraDamping", "CameraBlendParameter", "OffsetByVelocity", "PivotHeight", "ZoomLevel"
    };

    private static readonly HashSet<string> VanillaFovValues = new() { "45", "53", "40" };

    public static Func<string>? BackupsDirOverride { get; set; }
    public static string? AppVersion { get; set; }

    private static string BackupsDir => BackupsDirOverride?.Invoke()
        ?? Path.Combine(AppContext.BaseDirectory, "backups");

    // ── XML modification engine ──────────────────────────────────────

    public static string ApplyModifications(string xmlText, ModificationSet modSet)
    {
        var elementMods = modSet.ElementMods;
        int fovValue = modSet.FovValue;

        var lines = xmlText.Split('\n');
        var depthStack = new List<(string Tag, bool IsSection)>();
        var keyCounter = new Dictionary<string, int>();
        var result = new List<string>();
        var appliedZoomLevels = new HashSet<string>();

        foreach (var line in lines)
        {
            string stripped = line.Trim();

            if (stripped == "</>")
            {
                if (depthStack.Count > 0 && depthStack[^1].Tag == "ZoomLevelInfo")
                {
                    string sectionTag = "";
                    for (int i = depthStack.Count - 1; i >= 0; i--)
                    {
                        if (depthStack[i].IsSection) { sectionTag = depthStack[i].Tag; break; }
                    }
                    if (!string.IsNullOrEmpty(sectionTag))
                    {
                        var prefix = $"{sectionTag}/ZoomLevel[";
                        var pending = new List<(int level, string xmlLine)>();
                        foreach (var (modKey, modAttrs) in elementMods)
                        {
                            if (!modKey.StartsWith(prefix)) continue;
                            if (appliedZoomLevels.Contains(modKey)) continue;
                            string levelStr = modKey.Substring(prefix.Length).TrimEnd(']');
                            if (!int.TryParse(levelStr, out int levelNum)) continue;
                            var parts = new List<string> { $"Level=\"{levelStr}\"" };
                            foreach (var (attr, (_, val)) in modAttrs)
                                parts.Add($"{attr}=\"{val}\"");
                            string indent = new string('\t', depthStack.Count);
                            pending.Add((levelNum, $"{indent}<ZoomLevel {string.Join(" ", parts)}/>"));
                        }
                        pending.Sort((a, b) => a.level.CompareTo(b.level));
                        foreach (var (_, xmlLine) in pending)
                            result.Add(xmlLine);
                    }
                }

                result.Add(line);
                if (depthStack.Count > 0)
                    depthStack.RemoveAt(depthStack.Count - 1);
                continue;
            }

            var bm = BareOpenRe.Match(stripped);
            if (bm.Success)
            {
                string bareTag = bm.Groups[1].Value;
                depthStack.Add((bareTag, false));
                result.Add(line);

                if (bareTag == "ZoomLevelInfo")
                {
                    string sectionTag = "";
                    for (int i = depthStack.Count - 1; i >= 0; i--)
                    {
                        if (depthStack[i].IsSection) { sectionTag = depthStack[i].Tag; break; }
                    }
                    if (!string.IsNullOrEmpty(sectionTag))
                    {
                        var prefix = $"{sectionTag}/ZoomLevel[";
                        var earlyLevels = new List<(int level, string xmlLine)>();
                        foreach (var (modKey, modAttrs) in elementMods)
                        {
                            if (!modKey.StartsWith(prefix)) continue;
                            string levelStr = modKey.Substring(prefix.Length).TrimEnd(']');
                            if (!int.TryParse(levelStr, out int levelNum)) continue;
                            if (levelNum >= 1) continue;
                            var parts = new List<string> { $"Level=\"{levelStr}\"" };
                            foreach (var (attr, (_, val)) in modAttrs)
                                parts.Add($"{attr}=\"{val}\"");
                            string indent = new string('\t', depthStack.Count);
                            earlyLevels.Add((levelNum, $"{indent}<ZoomLevel {string.Join(" ", parts)}/>"));
                            appliedZoomLevels.Add(modKey);
                        }
                        earlyLevels.Sort((a, b) => a.level.CompareTo(b.level));
                        foreach (var (_, xmlLine) in earlyLevels)
                            result.Add(xmlLine);
                    }
                }

                continue;
            }

            var m = TagRe.Match(stripped);
            if (!m.Success)
            {
                result.Add(line);
                continue;
            }

            string tag = m.Groups[1].Value;
            string attrsStr = m.Groups[2].Value;
            bool selfClosing = m.Groups[3].Value == "/";
            var attrs = ParseAttrs(attrsStr);

            string parentTag = "";
            for (int i = depthStack.Count - 1; i >= 0; i--)
            {
                if (depthStack[i].IsSection)
                {
                    parentTag = depthStack[i].Tag;
                    break;
                }
            }

            string key;
            if (tag == "ZoomLevel")
            {
                string level = attrs.GetValueOrDefault("Level", "?");
                key = string.IsNullOrEmpty(parentTag) ? $"ZoomLevel[{level}]" : $"{parentTag}/ZoomLevel[{level}]";
            }
            else
            {
                key = string.IsNullOrEmpty(parentTag) ? tag : $"{parentTag}/{tag}";
            }

            keyCounter[key] = keyCounter.GetValueOrDefault(key, 0) + 1;
            int occurrence = keyCounter[key];
            string indexedKey = $"{key}#{occurrence}";

            string modifiedLine = line;

            string? matchKey = elementMods.ContainsKey(indexedKey) ? indexedKey
                : elementMods.ContainsKey(key) ? key : null;

            if (matchKey != null)
            {
                if (tag == "ZoomLevel")
                    appliedZoomLevels.Add(matchKey);

                foreach (var (attr, (action, value)) in elementMods[matchKey])
                {
                    if (action == "SET")
                    {
                        if (Regex.IsMatch(modifiedLine, $@"{attr}="""))
                        {
                            modifiedLine = Regex.Replace(modifiedLine, $@"{attr}=""[^""]*""",
                                $@"{attr}=""{value}""", RegexOptions.None, TimeSpan.FromSeconds(1));
                        }
                        else
                        {
                            modifiedLine = Regex.Replace(modifiedLine, @"(/?>)",
                                $@" {attr}=""{value}""$1", RegexOptions.None, TimeSpan.FromSeconds(1));
                        }
                    }
                    else if (action == "REMOVE")
                    {
                        modifiedLine = Regex.Replace(modifiedLine, $@"\s+{attr}=""[^""]*""", "");
                    }
                }
            }

            if (fovValue > 0)
            {
                string section = (SubElementTags.Contains(tag) || tag == "ZoomLevel") ? parentTag : tag;
                    bool applyFov = section.StartsWith("Player_")
                        || section.StartsWith("Cinematic_")
                        || section.StartsWith("Glide_");

                if (applyFov)
                {
                    var fovMatch = Regex.Match(modifiedLine, @"(?<!\w)Fov=""([^""]*)""");
                    if (fovMatch.Success && double.TryParse(fovMatch.Groups[1].Value, out double curFov))
                    {
                        int newFov = (int)Math.Round(curFov + fovValue);
                        modifiedLine = Regex.Replace(modifiedLine, @"(?<!\w)Fov=""[^""]*""", $@"Fov=""{newFov}""");
                    }

                    if (tag == "ZoomLevel")
                    {
                        var idfovMatch = Regex.Match(modifiedLine, @"InDoorFov=""([^""]*)""");
                        if (idfovMatch.Success && double.TryParse(idfovMatch.Groups[1].Value, out double curIdFov))
                        {
                            int newIdFov = (int)Math.Round(curIdFov + fovValue);
                            modifiedLine = Regex.Replace(modifiedLine, @"InDoorFov=""[^""]*""", $@"InDoorFov=""{newIdFov}""");
                        }
                    }
                }
            }

            result.Add(modifiedLine);

            if (!SubElementTags.Contains(tag) && tag != "ZoomLevel" && !selfClosing)
                depthStack.Add((tag, true));
        }

        return string.Join("\n", result);
    }

    public static string StripComments(string xmlText)
    {
        var lines = xmlText.Split('\n');
        var result = new List<string>();
        bool inComment = false;

        foreach (var line in lines)
        {
            string stripped = line.Trim();

            if (inComment)
            {
                if (stripped.Contains("-->")) inComment = false;
                continue;
            }

            if (stripped.Contains("<!--") && !stripped.Contains("-->"))
            {
                inComment = true;
                continue;
            }

            if (stripped.StartsWith("<!--") && stripped.EndsWith("-->")) continue;

            if (string.IsNullOrEmpty(stripped)) continue;

            result.Add(line);
        }

        return string.Join("\n", result);
    }

    public static string StripHeaderComments(string xmlText)
    {
        var lines = xmlText.Split('\n');
        var result = new List<string>();
        bool inComment = false;
        bool headerDone = false;

        foreach (var line in lines)
        {
            string stripped = line.Trim();
            if (headerDone) { result.Add(line); continue; }
            if (stripped.Contains("<!--") && !stripped.Contains("-->")) { inComment = true; continue; }
            if (inComment) { if (stripped.Contains("-->")) inComment = false; continue; }
            if (stripped.StartsWith("<!--") && stripped.EndsWith("-->")) continue;
            if (string.IsNullOrEmpty(stripped)) continue;
            headerDone = true;
            result.Add(line);
        }

        return string.Join("\n", result);
    }

    private static Dictionary<string, string> ParseAttrs(string attrsStr)
    {
        var dict = new Dictionary<string, string>();
        foreach (Match m in Regex.Matches(attrsStr, @"(\w+)=""([^""]*)"""))
            dict[m.Groups[1].Value] = m.Groups[2].Value;
        return dict;
    }

    // ── XML → AdvancedRow parsing ───────────────────────────────────

    public static List<AdvancedRow> ParseXmlToRows(string xmlText)
    {
        var rows = new List<AdvancedRow>();
        var lines = xmlText.Split('\n');
        var depthStack = new List<(string Tag, bool IsSection)>();

        foreach (var line in lines)
        {
            string stripped = line.Trim();

            if (stripped == "</>")
            {
                if (depthStack.Count > 0) depthStack.RemoveAt(depthStack.Count - 1);
                continue;
            }

            var bm = BareOpenRe.Match(stripped);
            if (bm.Success)
            {
                depthStack.Add((bm.Groups[1].Value, false));
                continue;
            }

            var m = TagRe.Match(stripped);
            if (!m.Success) continue;

            string tag = m.Groups[1].Value;
            string attrsStr = m.Groups[2].Value;
            bool selfClosing = m.Groups[3].Value == "/";
            var attrs = ParseAttrs(attrsStr);

            string parentTag = "";
            for (int i = depthStack.Count - 1; i >= 0; i--)
            {
                if (depthStack[i].IsSection) { parentTag = depthStack[i].Tag; break; }
            }

            bool isSub = SubElementTags.Contains(tag) || tag == "ZoomLevel";
            string section;
            string subElement;

            if (isSub)
            {
                section = parentTag;
                subElement = tag == "ZoomLevel"
                    ? $"ZoomLevel[{attrs.GetValueOrDefault("Level", "?")}]"
                    : tag;
            }
            else
            {
                section = tag;
                subElement = "";
            }

            if (string.IsNullOrEmpty(section)) goto pushStack;

            foreach (var (attrName, attrVal) in attrs)
            {
                if (tag == "ZoomLevel" && attrName == "Level") continue;
                rows.Add(new AdvancedRow
                {
                    Section = section,
                    SubElement = subElement,
                    Attribute = attrName,
                    VanillaValue = attrVal,
                    Value = attrVal,
                });
            }

            pushStack:
            if (!isSub && !selfClosing)
                depthStack.Add((tag, true));
        }

        return rows;
    }

    // ── Entry finding ────────────────────────────────────────────────

    public static PazEntry FindCameraEntry(string gameDir)
    {
        string pamtPath = Path.Combine(gameDir, "0010", "0.pamt");
        string pazDir = Path.Combine(gameDir, "0010");
        if (!File.Exists(pamtPath))
            throw new FileNotFoundException($"PAMT not found: {pamtPath}");

        var entries = PamtReader.Parse(pamtPath, pazDir);
        return entries.FirstOrDefault(e => e.Path.Contains("playercamerapreset.xml"))
            ?? throw new InvalidOperationException("playercamerapreset.xml not found in PAMT");
    }

    // ── Live detection from game files ──────────────────────────────

    public static string ReadLiveXml(string gameDir)
    {
        var entry = FindCameraEntry(gameDir);
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

        byte[] dec = AssetCodec.Decode(raw, Path.GetFileName(entry.Path));
        byte[] plain = CompressionUtils.Lz4Decompress(dec, entry.OrigSize);
        return Encoding.UTF8.GetString(plain).TrimEnd('\0');
    }

    public class LiveModStatus
    {
        public bool IsModified { get; set; }
        public int FovDelta { get; set; }
        public string? DetectedFov { get; set; }
        public bool CenteredCamera { get; set; }
        public bool CombatModified { get; set; }
        public bool MountModified { get; set; }
        public bool StyleModified { get; set; }
    }

    private const string VanillaDefaultFov = "45";

    public static LiveModStatus DetectLiveStatus(string gameDir)
    {
        string xml = ReadLiveXml(gameDir);
        var status = new LiveModStatus();

        // FoV: check Player_Basic_Default_Run element attribute
        var runMatch = Regex.Match(xml,
            @"<Player_Basic_Default_Run\s+([^>]*?)/?>",
            RegexOptions.Singleline);
        if (runMatch.Success)
            status.DetectedFov = ExtractAttr(runMatch.Groups[1].Value, "Fov");

        if (status.DetectedFov == null)
        {
            var altMatch = Regex.Match(xml,
                @"<Player_Basic_Default\s+([^>]*?)/?>",
                RegexOptions.Singleline);
            if (altMatch.Success)
                status.DetectedFov = ExtractAttr(altMatch.Groups[1].Value, "Fov");
        }

        bool fovChanged = false;
        if (status.DetectedFov != null && int.TryParse(status.DetectedFov, out int fov))
        {
            status.FovDelta = fov - int.Parse(VanillaDefaultFov);
            fovChanged = !VanillaFovValues.Contains(status.DetectedFov);
        }

        // Centered camera: check Player_Basic_Default_Run ZL2 specifically.
        // Vanilla has RightOffset="0.5"; only Bane sets it to "0.0".
        var baneCheck = Regex.Match(xml,
            @"<Player_Basic_Default_Run\b[^>]*>[\s\S]*?<ZoomLevel\s+([^>]*?)/?>");
        if (baneCheck.Success)
        {
            string ro = ExtractAttr(baneCheck.Groups[1].Value, "RightOffset") ?? "0.5";
            status.CenteredCamera = ro == "0.0" || ro == "0.00" || ro == "0";
        }

        // Combat: our mod sets TargetRate="0.25" on Player_Weapon_LockOn (vanilla is 0.5)
        var lockOnMatch = Regex.Match(xml,
            @"<Player_Weapon_LockOn\s+([^>]*?)/?>",
            RegexOptions.Singleline);
        if (lockOnMatch.Success)
        {
            string tr = ExtractAttr(lockOnMatch.Groups[1].Value, "TargetRate") ?? "";
            status.CombatModified = tr != "" && tr != "0.5";
        }

        // Mount: our mod sets BlendInTime="0.3" on Player_Ride_Horse/CameraBlendParameter
        status.MountModified = Regex.IsMatch(xml,
            @"<Player_Ride_Horse\b[\s\S]*?BlendInTime=""0\.3""");

        // Style: our mod changes UpOffset on ZoomLevel under Player_Basic_Default.
        // Vanilla UpOffset at ZoomLevel[2] is "0.3" for Player_Basic_Default.
        var styleMatch = Regex.Match(xml,
            @"<Player_Basic_Default\s+[^>]*?>[\s\S]*?<ZoomLevel\s+([^>]*?)/?>");
        if (styleMatch.Success)
        {
            string upOff = ExtractAttr(styleMatch.Groups[1].Value, "UpOffset") ?? "0.3";
            status.StyleModified = upOff != "0.3";
        }

        status.IsModified = fovChanged || status.CenteredCamera || status.CombatModified
                            || status.MountModified || status.StyleModified;

        return status;
    }

    private static string? ExtractAttr(string attrs, string name)
    {
        var m = Regex.Match(attrs, name + @"=""([^""]+)""");
        return m.Success ? m.Groups[1].Value : null;
    }

    // ── Backup management ────────────────────────────────────────────

    private static bool ValidateVanilla(string xmlText)
    {
        var m1 = Regex.Match(xmlText, @"<Player_Basic_Default_Run\s+[^>]*?Fov=""(\d+)""");
        if (m1.Success && !VanillaFovValues.Contains(m1.Groups[1].Value)) return false;
        var m2 = Regex.Match(xmlText, @"<Player_Basic_Default_Runfast\s+[^>]*?Fov=""(\d+)""");
        if (m2.Success && !VanillaFovValues.Contains(m2.Groups[1].Value)) return false;
        return true;
    }

    private static void EnsureBackup(PazEntry entry, Action<string>? log = null)
    {
        string bdir = BackupsDir;
        string backupPath = Path.Combine(bdir, "original_backup.bin");
        string metaPath = Path.Combine(bdir, "backup_meta.txt");

        if (File.Exists(backupPath) && File.Exists(metaPath))
        {
            string meta = File.ReadAllText(metaPath);
            var parts = meta.Split();
            bool compMatch = false;
            bool versionMatch = string.IsNullOrEmpty(AppVersion);
            foreach (var part in parts)
            {
                if (part.StartsWith("comp_size="))
                    compMatch = int.TryParse(part["comp_size=".Length..], out int savedComp) && savedComp == entry.CompSize;
                if (part.StartsWith("ucm_version="))
                    versionMatch = part["ucm_version=".Length..] == AppVersion;
            }

            if (compMatch && versionMatch)
                return;

            if (!versionMatch)
                log?.Invoke("UCM version changed -- refreshing vanilla backup...");
        }

        Directory.CreateDirectory(bdir);
        using (var fs = new FileStream(entry.PazFile, FileMode.Open, FileAccess.Read))
        {
            fs.Seek(entry.Offset, SeekOrigin.Begin);
            byte[] data = new byte[entry.CompSize];
            int totalRead = 0;
            while (totalRead < data.Length)
            {
                int n = fs.Read(data, totalRead, data.Length - totalRead);
                if (n == 0) throw new EndOfStreamException();
                totalRead += n;
            }

            try
            {
                var dec = AssetCodec.Decode(data, "playercamerapreset.xml");
                var xmlBytes = CompressionUtils.Lz4Decompress(dec, entry.OrigSize);
                string xmlText = Encoding.UTF8.GetString(xmlBytes).TrimEnd('\0');
                if (!ValidateVanilla(xmlText))
                    throw new InvalidOperationException(
                        "Game files appear to be already modified by another camera mod.\n\n" +
                        "TO FIX:\n" +
                        "1. Close this tool\n" +
                        "2. Steam > Crimson Desert > Properties > Installed Files > \"Verify integrity of game files\"\n" +
                        "3. Run this tool again");
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception)
            {
                throw new InvalidOperationException(
                    "Game files appear to be corrupted or modified by another tool.\n\n" +
                    "TO FIX:\n" +
                    "1. Close this tool\n" +
                    "2. Steam > Crimson Desert > Properties > Installed Files > \"Verify integrity of game files\"\n" +
                    "3. Run this tool again");
            }

            File.WriteAllBytes(backupPath, data);
            string verTag = string.IsNullOrEmpty(AppVersion) ? "" : $" ucm_version={AppVersion}";
            File.WriteAllText(metaPath, $"comp_size={entry.CompSize} orig_size={entry.OrigSize}{verTag}");
            log?.Invoke($"Backup saved ({entry.CompSize} bytes)");
        }
    }

    private static string GetVanillaXml(PazEntry entry)
    {
        string backupPath = Path.Combine(BackupsDir, "original_backup.bin");
        byte[] raw = File.ReadAllBytes(backupPath);
        try
        {
            byte[] decrypted = AssetCodec.Decode(raw, "playercamerapreset.xml");
            byte[] xmlBytes = CompressionUtils.Lz4Decompress(decrypted, entry.OrigSize);
            return Encoding.UTF8.GetString(xmlBytes).TrimEnd('\0');
        }
        catch (Exception)
        {
            File.Delete(backupPath);
            string metaPath = Path.Combine(BackupsDir, "backup_meta.txt");
            if (File.Exists(metaPath)) File.Delete(metaPath);
            throw new InvalidOperationException(
                "Backup file is corrupted (possibly from a previous version or another tool).\n\n" +
                "The bad backup has been cleared. TO FIX:\n" +
                "1. Steam > Crimson Desert > Properties > Installed Files > \"Verify integrity of game files\"\n" +
                "2. Try Install again");
        }
    }

    // ── Public helpers for Advanced Editor ─────────────────────────

    public static string ReadVanillaXml(string gameDir)
    {
        var entry = FindCameraEntry(gameDir);
        EnsureBackup(entry);
        string xml = GetVanillaXml(entry);
        return StripComments(xml);
    }

    public static Dictionary<string, object> InstallWithModSet(string gameDir, ModificationSet modSet, Action<string>? log = null)
    {
        log?.Invoke("Finding camera entry...");
        var entry = FindCameraEntry(gameDir);

        log?.Invoke("Ensuring backup...");
        EnsureBackup(entry, log);

        log?.Invoke("Reading vanilla XML...");
        string vanillaXml = StripComments(GetVanillaXml(entry));

        log?.Invoke("Applying modifications...");
        string modifiedXml = ApplyModifications(vanillaXml, modSet);

        log?.Invoke("Encoding and size-matching...");
        byte[] xmlBytes = new UTF8Encoding(true).GetBytes(modifiedXml);
        byte[] matched = ArchiveWriter.MatchCompressedSize(xmlBytes, entry.CompSize, entry.OrigSize);

        log?.Invoke("Compressing...");
        byte[] compressed = CompressionUtils.Lz4Compress(matched);
        if (compressed.Length != entry.CompSize)
            throw new InvalidOperationException($"Size mismatch: {compressed.Length} != {entry.CompSize}");

        log?.Invoke("Encoding...");
        byte[] encoded = AssetCodec.Encode(compressed, "playercamerapreset.xml");

        log?.Invoke("Writing game files...");
        ArchiveWriter.UpdateEntry(entry, encoded);

        log?.Invoke("Done!");
        return new Dictionary<string, object>
        {
            ["status"] = "ok",
            ["comp_size"] = entry.CompSize
        };
    }

    // ── Main operations ──────────────────────────────────────────────

    public static Dictionary<string, object> InstallCameraMod(string gameDir, string style, int fov,
        bool bane, string combat, bool mountHeight = false, double? customUp = null,
        bool steadycam = true, bool extraZoom = false, bool horseFirstPerson = false,
        Action<string>? log = null)
    {
        log?.Invoke("Finding camera entry...");
        var entry = FindCameraEntry(gameDir);
        log?.Invoke($"Found: offset={entry.Offset}, comp_size={entry.CompSize}, orig_size={entry.OrigSize}");

        log?.Invoke("Ensuring backup...");
        EnsureBackup(entry, log);

        log?.Invoke("Extracting vanilla XML...");
        string vanillaXml = GetVanillaXml(entry);

        bool needsInjection = extraZoom || horseFirstPerson || steadycam;
        if (needsInjection)
        {
            log?.Invoke("Stripping all comments (making room for injected elements)...");
            vanillaXml = StripComments(vanillaXml);
        }
        else
        {
            log?.Invoke("Stripping header comments...");
            vanillaXml = StripHeaderComments(vanillaXml);
        }

        log?.Invoke("Building modification rules...");
        var modSet = CameraRules.BuildModifications(style, fov, bane, combat,
            mountHeight: mountHeight, customUp: customUp, steadycam: steadycam,
            extraZoom: extraZoom, horseFirstPerson: horseFirstPerson);
        int modCount = modSet.ElementMods.Values.Sum(v => v.Count);
        log?.Invoke($"Rules: {modCount} attribute changes" +
            (modSet.FovValue > 0 ? $", FoV=+{modSet.FovValue}" : ""));

        log?.Invoke("Applying modifications...");
        string modifiedXml = ApplyModifications(vanillaXml, modSet);

        try
        {
            string debugPath = Path.Combine(BackupsDir, "debug_modified.xml");
            File.WriteAllText(debugPath, modifiedXml);
            log?.Invoke($"Debug XML written to: {debugPath}");
        }
        catch { }

        log?.Invoke("Encoding and size-matching...");
        byte[] xmlBytes = new UTF8Encoding(true).GetBytes(modifiedXml);
        byte[] matched = ArchiveWriter.MatchCompressedSize(xmlBytes, entry.CompSize, entry.OrigSize);

        log?.Invoke("Compressing...");
        byte[] compressed = CompressionUtils.Lz4Compress(matched);
        if (compressed.Length != entry.CompSize)
            throw new InvalidOperationException($"Size mismatch: {compressed.Length} != {entry.CompSize}");

        log?.Invoke("Encoding...");
        byte[] encoded = AssetCodec.Encode(compressed, "playercamerapreset.xml");

        log?.Invoke("Writing game files...");
        ArchiveWriter.UpdateEntry(entry, encoded);

        log?.Invoke("Done!");
        return new Dictionary<string, object>
        {
            ["status"] = "ok",
            ["comp_size"] = entry.CompSize
        };
    }

    public static Dictionary<string, object> RestoreCamera(string gameDir, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        string backupPath = Path.Combine(BackupsDir, "original_backup.bin");

        if (!File.Exists(backupPath))
        {
            log?.Invoke("No backup found. The camera may already be vanilla.");
            return new() { ["status"] = "no_backup" };
        }

        byte[] backupData = File.ReadAllBytes(backupPath);
        if (backupData.Length != entry.CompSize)
        {
            log?.Invoke("Backup size mismatch — game may have updated. Deleting stale backup.");
            File.Delete(backupPath);
            string metaPath = Path.Combine(BackupsDir, "backup_meta.txt");
            if (File.Exists(metaPath)) File.Delete(metaPath);
            return new() { ["status"] = "stale_backup" };
        }

        log?.Invoke("Restoring original camera...");
        ArchiveWriter.UpdateEntry(entry, backupData);
        log?.Invoke("Done! Camera restored to vanilla.");
        return new() { ["status"] = "ok" };
    }
}
