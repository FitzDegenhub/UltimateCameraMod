using System.Globalization;
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

    /// <summary>
    /// Reads <c>Player_Basic_Default</c> idle zoom (ZL2) distance, height, and <b>literal</b> <c>RightOffset</c> from XML.
    /// UCM Quick horizontal shift uses delta semantics (<see cref="CameraRules.BuildCustom"/>); convert with
    /// <see cref="CameraRules.QuickShiftDeltaFromFootZl2RightOffset"/> before applying to the shift slider.
    /// </summary>
    public static bool TryParseUcmQuickFootBaselineFromXml(string xml,
        out double zoomDistanceZl2, out double upOffsetZl2, out double rightOffsetZl2)
    {
        zoomDistanceZl2 = 0;
        upOffsetZl2 = 0;
        rightOffsetZl2 = 0;
        try
        {
            var rows = ParseXmlToRows(xml);
            var lookup = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var r in rows)
                lookup[r.FullKey] = r.Value;

            static bool TryD(string? s, out double d) =>
                double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d);

            bool zdOk = lookup.TryGetValue("Player_Basic_Default/ZoomLevel[2].ZoomDistance", out var zd) && TryD(zd, out zoomDistanceZl2);
            bool upOk = lookup.TryGetValue("Player_Basic_Default/ZoomLevel[2].UpOffset", out var up) && TryD(up, out upOffsetZl2);
            bool roOk = lookup.TryGetValue("Player_Basic_Default/ZoomLevel[2].RightOffset", out var ro) && TryD(ro, out rightOffsetZl2);
            return zdOk && upOk && roOk;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads the three on-foot ZoomDistance values (ZL2/ZL3/ZL4) from
    /// <c>Player_Basic_Default</c> in the given XML string.
    /// Returns true only if all three are present and parseable.
    /// </summary>
    public static bool TryParseOnFootZoomDistances(string xml,
        out double zl2, out double zl3, out double zl4)
    {
        zl2 = zl3 = zl4 = 0;
        try
        {
            var rows = ParseXmlToRows(xml);
            var lookup = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var r in rows)
                lookup[r.FullKey] = r.Value;

            static bool TryD(string? s, out double d) =>
                double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d);

            return lookup.TryGetValue("Player_Basic_Default/ZoomLevel[2].ZoomDistance", out var s2) && TryD(s2, out zl2)
                && lookup.TryGetValue("Player_Basic_Default/ZoomLevel[3].ZoomDistance", out var s3) && TryD(s3, out zl3)
                && lookup.TryGetValue("Player_Basic_Default/ZoomLevel[4].ZoomDistance", out var s4) && TryD(s4, out zl4);
        }
        catch { return false; }
    }

    // ── Entry finding ────────────────────────────────────────────────

    public static PazEntry FindCameraEntry(string gameDir)
    {
        string pamtPath = Path.Combine(gameDir, "0010", "0.pamt");
        string pazDir = Path.Combine(gameDir, "0010");
        if (!File.Exists(pamtPath))
            throw new FileNotFoundException(
                $"Game archive index not found at:\n{pamtPath}\n\n" +
                "Make sure you selected the correct Crimson Desert install folder. " +
                "The folder should contain a '0010' subfolder with 0.paz and 0.pamt files.");

        var entries = PamtReader.Parse(pamtPath, pazDir);
        return entries.FirstOrDefault(e => e.Path.Contains("playercamerapreset.xml"))
            ?? throw new InvalidOperationException(
                "Camera file (playercamerapreset.xml) was not found in the game archive.\n\n" +
                "This can happen if the game was partially installed or the archive is from a different version.\n" +
                "Try verifying game files on Steam, then launch UCM again.");
    }

    public static PazEntry FindCameraEntryFromPamt(string pamtPath, string? pazDir = null)
    {
        if (!File.Exists(pamtPath))
            throw new FileNotFoundException(
                $"Game archive index not found at:\n{pamtPath}\n\n" +
                "Make sure the selected folder contains 0.paz and 0.pamt files.");

        var entries = PamtReader.Parse(pamtPath, pazDir);
        return entries.FirstOrDefault(e => e.Path.Contains("playercamerapreset.xml"))
            ?? throw new InvalidOperationException(
                "Camera file (playercamerapreset.xml) was not found in the game archive.\n\n" +
                "Try verifying game files on Steam, then launch UCM again.");
    }

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

    private static string DecodeEntryXml(PazEntry entry, byte[] rawBytes)
    {
        byte[] dec = AssetCodec.Decode(rawBytes, Path.GetFileName(entry.Path));
        byte[] plain = CompressionUtils.Lz4Decompress(dec, entry.OrigSize);
        return Encoding.UTF8.GetString(plain).TrimEnd('\0');
    }

    public static string ReadXmlFromPaz(string pazPath, string pamtPath)
    {
        string pazDir = Path.GetDirectoryName(pazPath) ?? ".";
        var indexedEntry = FindCameraEntryFromPamt(pamtPath, pazDir);
        var entry = new PazEntry
        {
            Path = indexedEntry.Path,
            PazFile = pazPath,
            Offset = indexedEntry.Offset,
            CompSize = indexedEntry.CompSize,
            OrigSize = indexedEntry.OrigSize,
            Flags = indexedEntry.Flags,
            PazIndex = indexedEntry.PazIndex
        };

        byte[] raw = ReadEntryBytes(entry);
        return DecodeEntryXml(entry, raw);
    }

    public static (PazEntry Entry, byte[] RawBytes) ReadCameraEntryWithRawBytes(string gameDir)
    {
        var entry = FindCameraEntry(gameDir);
        return (entry, ReadEntryBytes(entry));
    }

    public static (PazEntry Entry, byte[] RawBytes) ReadCameraEntryWithRawBytes(string pazPath, string pamtPath)
    {
        string pazDir = Path.GetDirectoryName(pazPath) ?? ".";
        var indexedEntry = FindCameraEntryFromPamt(pamtPath, pazDir);
        var entry = new PazEntry
        {
            Path = indexedEntry.Path,
            PazFile = pazPath,
            Offset = indexedEntry.Offset,
            CompSize = indexedEntry.CompSize,
            OrigSize = indexedEntry.OrigSize,
            Flags = indexedEntry.Flags,
            PazIndex = indexedEntry.PazIndex
        };

        return (entry, ReadEntryBytes(entry));
    }

    // ── Live detection from game files ──────────────────────────────

    public static string ReadLiveXml(string gameDir)
    {
        var entry = FindCameraEntry(gameDir);
        byte[] raw = ReadEntryBytes(entry);
        return DecodeEntryXml(entry, raw);
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
        var reasons = new List<string>();

        // FoV check: UCM sets Fov="40" on run sections.
        var m1 = Regex.Match(xmlText, @"<Player_Basic_Default_Run\s+[^>]*?Fov=""(\d+)""");
        if (m1.Success && m1.Groups[1].Value == "40") reasons.Add($"Run Fov={m1.Groups[1].Value}");
        var m2 = Regex.Match(xmlText, @"<Player_Basic_Default_Runfast\s+[^>]*?Fov=""(\d+)""");
        if (m2.Success && m2.Groups[1].Value == "40") reasons.Add($"Runfast Fov={m2.Groups[1].Value}");

        // OffsetByVelocity check: UCM zeros camera sway.
        var m4 = Regex.Match(xmlText, @"<Player_Basic_Default_Run\s+[^>]*?>[\s\S]*?<OffsetByVelocity[^>]*?OffsetLength=""0""", RegexOptions.Multiline);
        if (m4.Success) reasons.Add("OffsetByVelocity=0");

        // Note: MaxZoomDistance="30" and XML comments are no longer checked because
        // the June 2026 game patch added both to the vanilla camera XML.

        return reasons.Count == 0;
    }

    /// <param name="forceRefreshFromPaz">
    /// When true, always re-read the camera chunk from <paramref name="entry"/>'s PAZ and rewrite the backup.
    /// </param>
    /// <summary>Reads the encrypted camera chunk exactly as stored in the live <c>.paz</c> (no vanilla check).</summary>
    private static byte[] ReadLiveCameraPayloadBytes(PazEntry entry)
    {
        using var fs = new FileStream(entry.PazFile, FileMode.Open, FileAccess.Read);
        fs.Seek(entry.Offset, SeekOrigin.Begin);
        byte[] data = new byte[entry.CompSize];
        int totalRead = 0;
        while (totalRead < data.Length)
        {
            int n = fs.Read(data, totalRead, data.Length - totalRead);
            if (n == 0)
                throw new EndOfStreamException();
            totalRead += n;
        }

        return data;
    }

    /// <summary>
    /// Decodes and LZ4-decompresses the camera entry bytes as stored in a <c>.paz</c> (or backup file).
    /// CD JSON Mod Manager v1 applies patches to this buffer; offsets are 0-based within it.
    /// </summary>
    public static byte[] DecompressCameraPayloadFromRaw(byte[] rawPazPayload, PazEntry entry)
    {
        byte[] dec = AssetCodec.Decode(rawPazPayload, Path.GetFileName(entry.Path));
        return CompressionUtils.Lz4Decompress(dec, entry.OrigSize);
    }

    private static void EnsureBackup(PazEntry entry, Action<string>? log = null, bool forceRefreshFromPaz = false)
    {
        string bdir = BackupsDir;
        string backupPath = Path.Combine(bdir, "original_backup.bin");
        string metaPath = Path.Combine(bdir, "backup_meta.txt");

        if (!forceRefreshFromPaz && File.Exists(backupPath) && File.Exists(metaPath))
        {
            string meta = File.ReadAllText(metaPath);
            var parts = meta.Split();
            bool compMatch = false;
            bool versionMatch = string.IsNullOrEmpty(AppVersion);
            bool vanillaValidated = false;
            foreach (var part in parts)
            {
                if (part.StartsWith("comp_size="))
                    compMatch = int.TryParse(part["comp_size=".Length..], out int savedComp) && savedComp == entry.CompSize;
                if (part.StartsWith("ucm_version="))
                {
                    // Accept any v3.x backup — don't require exact patch version match
                    string savedVer = part["ucm_version=".Length..];
                    string savedMajor = savedVer.Split('.')[0];
                    string currentMajor = AppVersion.Split('.')[0];
                    versionMatch = savedMajor == currentMajor;
                }
                if (part == "vanilla_verified")
                    vanillaValidated = true;
            }

            if (compMatch && versionMatch && vanillaValidated)
                return;

            if (compMatch && versionMatch && !vanillaValidated)
            {
                // Existing backup was created before comprehensive vanilla validation was added.
                // Re-validate the backup content now.
                log?.Invoke("Validating existing backup...");
                try
                {
                    byte[] raw = File.ReadAllBytes(backupPath);
                    var dec = AssetCodec.Decode(raw, "playercamerapreset.xml");
                    var xmlBytes = CompressionUtils.Lz4Decompress(dec, entry.OrigSize);
                    string xmlText = Encoding.UTF8.GetString(xmlBytes).TrimEnd('\0');
                    if (ValidateVanilla(xmlText))
                    {
                        // Backup is clean — stamp it so we skip re-validation next time.
                        File.WriteAllText(metaPath, meta + " vanilla_verified");
                        return;
                    }
                    else
                    {
                        // Backup is tainted — delete it and force re-capture from live PAZ.
                        log?.Invoke("Backup contains modified camera data — clearing...");
                        File.Delete(backupPath);
                        File.Delete(metaPath);
                        // Fall through to re-capture from live PAZ below.
                    }
                }
                catch
                {
                    File.Delete(backupPath);
                    if (File.Exists(metaPath)) File.Delete(metaPath);
                }
            }

            if (!versionMatch)
                log?.Invoke("UCM version changed -- refreshing vanilla backup...");
        }
        else if (forceRefreshFromPaz)
        {
            log?.Invoke("Syncing vanilla backup from game PAZ (export)...");
        }

        Directory.CreateDirectory(bdir);
        byte[] data = ReadLiveCameraPayloadBytes(entry);
        try
        {
            var dec = AssetCodec.Decode(data, "playercamerapreset.xml");
            var xmlBytes = CompressionUtils.Lz4Decompress(dec, entry.OrigSize);
            string xmlText = Encoding.UTF8.GetString(xmlBytes).TrimEnd('\0');
            if (!ValidateVanilla(xmlText))
            {
                // Only delete the UCM backup — NOT the game's 0.paz (too destructive)
                try { if (File.Exists(backupPath)) File.Delete(backupPath); } catch { }
                try { if (File.Exists(metaPath)) File.Delete(metaPath); } catch { }

                throw new InvalidOperationException(
                    "Game camera files are not vanilla — they have been modified by UCM v2.x, " +
                    "another camera mod, or a mod manager.\n\n" +
                    "TO FIX:\n" +
                    "1. Close UCM\n" +
                    "2. Open your game folder → 0010 → delete 0.paz\n" +
                    "3. Steam → Crimson Desert → Properties → Installed Files → \"Verify integrity of game files\"\n" +
                    "4. Wait for verification to complete, then relaunch UCM\n\n" +
                    "Steam will re-download the original camera file. This only needs to happen once.");
            }
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception)
        {
            // Only delete the UCM backup — NOT the game's 0.paz
            try { if (File.Exists(backupPath)) File.Delete(backupPath); } catch { }
            try { if (File.Exists(metaPath)) File.Delete(metaPath); } catch { }

            throw new InvalidOperationException(
                "Could not read camera data from the game archive. The file may be corrupted, " +
                "partially downloaded, or modified by another tool.\n\n" +
                "UCM has automatically removed the affected files.\n\n" +
                "TO FINISH:\n" +
                "1. Close UCM\n" +
                "2. Steam → Crimson Desert → Properties → Installed Files → \"Verify integrity of game files\"\n" +
                "3. Wait for verification to complete, then relaunch UCM\n\n" +
                "Steam will re-download the original camera file (~200 MB). This only needs to happen once.");
        }

        File.WriteAllBytes(backupPath, data);
        string verTag = string.IsNullOrEmpty(AppVersion) ? "" : $" ucm_version={AppVersion}";
        File.WriteAllText(metaPath, $"comp_size={entry.CompSize} orig_size={entry.OrigSize}{verTag} vanilla_verified");
        log?.Invoke($"Backup saved ({entry.CompSize} bytes)");
    }

    /// <summary>
    /// Re-copies the camera entry from the live <c>0.paz</c> into <c>original_backup.bin</c>, bypassing the
    /// usual skip when <c>backup_meta.txt</c> still matches. Call after the game install changes (e.g. Steam
    /// patch) so the backup matches on-disk data. Throws if live data fails the same vanilla checks used when creating a backup.
    /// </summary>
    public static void RefreshVanillaBackupFromLivePaz(string gameDir, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        EnsureBackup(entry, log, forceRefreshFromPaz: true);
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
                "UCM's vanilla backup could not be read — it may be from a previous version or corrupted.\n\n" +
                "The bad backup has been automatically cleared. TO FIX:\n" +
                "1. Steam → Crimson Desert → Properties → Installed Files → \"Verify integrity of game files\"\n" +
                "2. Launch UCM and try Install again — a fresh backup will be created");
        }
    }

    // ── Public helpers ───────────────────────────────────────────────

    public static void ExportLiveXml(string gameDir, string outputPath)
    {
        string xml = ReadLiveXml(gameDir);
        File.WriteAllText(outputPath, xml, new UTF8Encoding(true));
    }

    /// <summary>
    /// Writes session/preset XML to disk using the same normalization as install (strip comments, UTF-8 BOM).
    /// Suitable for sharing or editing; feed the same text to <see cref="InstallRawXml"/> or JSON/PAZ export.
    /// </summary>
    public static void ExportPresetXml(string outputPath, string xmlText)
    {
        string cleanXml = StripComments(xmlText);
        File.WriteAllText(outputPath, cleanXml, new UTF8Encoding(true));
    }

    /// <summary>
    /// Copies the host <c>.paz</c> from the game install, then replaces only the camera entry payload
    /// with bytes built from <paramref name="xmlText"/>. The output matches the exporter's game build
    /// (same archive layout and entry sizes as their <c>0010/0.paz</c>).
    /// </summary>
    public static void ExportPatchedPaz(string gameDir, string destinationPazPath, string xmlText, Action<string>? log = null)
    {
        log?.Invoke("Finding camera entry...");
        var entry = FindCameraEntry(gameDir);
        byte[] livePayload = ReadLiveCameraPayloadBytes(entry);

        log?.Invoke("Copying archive...");
        File.Copy(entry.PazFile, destinationPazPath, overwrite: true);

        log?.Invoke("Encoding camera XML for this game build...");
        byte[] payload = BuildModifiedBytesFromXml(gameDir, xmlText, log);

        if (payload.Length != livePayload.Length)
        {
            try { File.Delete(destinationPazPath); } catch { /* best effort */ }
            throw new InvalidOperationException(
                $"Encoded payload length ({payload.Length}) does not match live camera chunk ({livePayload.Length}); cannot patch PAZ safely.");
        }

        log?.Invoke("Patching camera entry in exported copy...");
        ArchiveWriter.UpdateEntryAt(destinationPazPath, entry.Offset, payload);
        log?.Invoke("Done.");
    }

    /// <summary>
    /// Short note listing patch targets and sizes (for mod pages / JSON description); uses live PAZ sync.
    /// </summary>
    public static string GetExportCompatibilityNote(string gameDir, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        string sourceGroup = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(entry.PazFile) ?? "0010");
        if (string.IsNullOrEmpty(sourceGroup)) sourceGroup = "0010";
        return $"Export fingerprint: game_file={entry.Path}; source_group={sourceGroup}; comp_size={entry.CompSize}; orig_size={entry.OrigSize}.";
    }

    public static Dictionary<string, object> InstallRawXml(string gameDir, string xmlText, Action<string>? log = null)
    {
        log?.Invoke("Finding camera entry...");
        var entry = FindCameraEntry(gameDir);

        log?.Invoke("Ensuring backup...");
        EnsureBackup(entry, log);

        log?.Invoke("Encoding and size-matching...");
        string cleanXml = StripComments(xmlText);
        byte[] xmlBytes = new UTF8Encoding(true).GetBytes(cleanXml);
        byte[] matched = ArchiveWriter.MatchCompressedSize(xmlBytes, entry.CompSize, entry.OrigSize);

        log?.Invoke("Compressing...");
        byte[] compressed = CompressionUtils.Lz4Compress(matched);
        if (compressed.Length != entry.CompSize)
            throw new InvalidOperationException($"Size mismatch after padding: {compressed.Length} != {entry.CompSize}");

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

    public static string ReadVanillaXml(string gameDir)
    {
        var entry = FindCameraEntry(gameDir);
        EnsureBackup(entry);
        string xml = GetVanillaXml(entry);
        return StripComments(xml);
    }

    /// <summary>
    /// Returns the raw encrypted bytes from the vanilla backup (for JSON patch diffing).
    /// </summary>
    public static byte[] ReadVanillaBackupBytes(string gameDir)
    {
        var entry = FindCameraEntry(gameDir);
        EnsureBackup(entry);
        string backupPath = Path.Combine(BackupsDir, "original_backup.bin");
        return File.ReadAllBytes(backupPath);
    }

    /// <summary>
    /// Returns the <strong>live</strong> encrypted camera bytes from the game PAZ, plus
    /// <c>game_file</c>, <c>source_group</c>, and the entry offset inside the <c>.paz</c>.
    /// </summary>
    /// <remarks>
    /// For CD JSON Mod Manager exports, use <see cref="ReadLiveCameraDecompressedPayloadForJson"/> instead:
    /// that tool patches the decompressed entry buffer with 0-based offsets.
    /// </remarks>
    public static (byte[] Bytes, string GameFile, string SourceGroup, int PazPayloadOffset) ReadVanillaBackupBytesWithMeta(
        string gameDir, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        log?.Invoke("Reading live camera chunk from PAZ (patch baseline)...");
        byte[] bytes = ReadLiveCameraPayloadBytes(entry);

        string sourceGroup = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(entry.PazFile) ?? "0010");
        if (string.IsNullOrEmpty(sourceGroup)) sourceGroup = "0010";

        return (bytes, entry.Path, sourceGroup, entry.Offset);
    }

    /// <summary>
    /// Returns encrypted bytes from UCM's stored <c>original_backup.bin</c> (after <see cref="EnsureBackup"/>),
    /// plus paths. For CD JSON Mod Manager, use <see cref="ReadStoredVanillaDecompressedPayloadForJson"/>.
    /// </summary>
    public static (byte[] Bytes, string GameFile, string SourceGroup, int PazPayloadOffset) ReadStoredVanillaBackupBytesWithMeta(
        string gameDir, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        log?.Invoke("Reading stored vanilla backup...");
        EnsureBackup(entry, log);
        string backupPath = Path.Combine(BackupsDir, "original_backup.bin");
        byte[] bytes = File.ReadAllBytes(backupPath);

        string sourceGroup = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(entry.PazFile) ?? "0010");
        if (string.IsNullOrEmpty(sourceGroup)) sourceGroup = "0010";

        return (bytes, entry.Path, sourceGroup, entry.Offset);
    }

    /// <summary>
    /// Runs the full modification pipeline and returns the encoded bytes without writing to the PAZ.
    /// Used by JsonModExporter to diff against vanilla.
    /// </summary>
    public static byte[] BuildModifiedBytes(string gameDir, ModificationSet modSet, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        EnsureBackup(entry, log);

        string vanillaXml = StripComments(GetVanillaXml(entry));
        string modifiedXml = ApplyModifications(vanillaXml, modSet);

        byte[] xmlBytes = new UTF8Encoding(true).GetBytes(modifiedXml);
        byte[] matched = ArchiveWriter.MatchCompressedSize(xmlBytes, entry.CompSize, entry.OrigSize);
        byte[] compressed = CompressionUtils.Lz4Compress(matched);
        if (compressed.Length != entry.CompSize)
            throw new InvalidOperationException($"Size mismatch: {compressed.Length} != {entry.CompSize}");

        return AssetCodec.Encode(compressed, "playercamerapreset.xml");
    }

    /// <summary>
    /// Runs the raw XML through the pipeline and returns encoded bytes without writing to the PAZ.
    /// </summary>
    public static byte[] BuildModifiedBytesFromXml(string gameDir, string xmlText, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        // No EnsureBackup: export/encoding must work while the live PAZ already reflects UCM tweaks.

        string cleanXml = StripComments(xmlText);
        byte[] xmlBytes = new UTF8Encoding(true).GetBytes(cleanXml);
        byte[] matched = ArchiveWriter.MatchCompressedSize(xmlBytes, entry.CompSize, entry.OrigSize);
        byte[] compressed = CompressionUtils.Lz4Compress(matched);
        if (compressed.Length != entry.CompSize)
            throw new InvalidOperationException($"Size mismatch: {compressed.Length} != {entry.CompSize}");

        return AssetCodec.Encode(compressed, "playercamerapreset.xml");
    }

    /// <summary>
    /// Pre-compression payload (padded XML, length <c>orig_size</c>) for the modified preset.
    /// Matches what <see cref="DecompressCameraPayloadFromRaw"/> returns for <see cref="BuildModifiedBytes"/> output.
    /// </summary>
    public static byte[] BuildModifiedDecompressedPayload(string gameDir, ModificationSet modSet, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        EnsureBackup(entry, log);

        string vanillaXml = StripComments(GetVanillaXml(entry));
        string modifiedXml = ApplyModifications(vanillaXml, modSet);

        byte[] xmlBytes = new UTF8Encoding(true).GetBytes(modifiedXml);
        return ArchiveWriter.MatchCompressedSize(xmlBytes, entry.CompSize, entry.OrigSize);
    }

    /// <summary>
    /// Pre-compression payload from raw XML text (same stage as <see cref="BuildModifiedDecompressedPayload"/>).
    /// </summary>
    public static byte[] BuildModifiedDecompressedPayloadFromXml(string gameDir, string xmlText, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);

        string cleanXml = StripComments(xmlText);
        byte[] xmlBytes = new UTF8Encoding(true).GetBytes(cleanXml);
        return ArchiveWriter.MatchCompressedSize(xmlBytes, entry.CompSize, entry.OrigSize);
    }

    /// <summary>
    /// Decompressed patch baseline from <c>original_backup.bin</c> for JSON export (mod-set path).
    /// </summary>
    public static (byte[] Bytes, string GameFile, string SourceGroup) ReadStoredVanillaDecompressedPayloadForJson(
        string gameDir, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        log?.Invoke("Reading stored vanilla backup (decompressed patch baseline)...");
        EnsureBackup(entry, log);
        string backupPath = Path.Combine(BackupsDir, "original_backup.bin");
        byte[] raw = File.ReadAllBytes(backupPath);
        byte[] decompressed = DecompressCameraPayloadFromRaw(raw, entry);

        string sourceGroup = Path.GetFileName(Path.GetDirectoryName(entry.PazFile) ?? "0010");
        if (string.IsNullOrEmpty(sourceGroup)) sourceGroup = "0010";

        return (decompressed, entry.Path, sourceGroup);
    }

    /// <summary>
    /// True when the encrypted camera chunk in the live PAZ matches <c>original_backup.bin</c>.
    /// <see cref="JsonModExporter.ExportFromXml"/> diffs against the live chunk, so patch
    /// <c>original</c> hex must match vanilla for tools like CDUMM; that only holds when live
    /// still matches the backup UCM took from a validated vanilla install.
    /// </summary>
    public static bool IsLiveCameraPayloadMatchingStoredBackup(string gameDir, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        EnsureBackup(entry, log);
        string backupPath = Path.Combine(BackupsDir, "original_backup.bin");
        byte[] live = ReadLiveCameraPayloadBytes(entry);
        byte[] backup = File.ReadAllBytes(backupPath);
        if (live.Length != backup.Length)
        {
            log?.Invoke($"Live camera payload ({live.Length} bytes) != backup ({backup.Length} bytes).");
            return false;
        }

        return live.AsSpan().SequenceEqual(backup);
    }

    /// <summary>
    /// Decompressed patch baseline from the live <c>.paz</c> for JSON export (XML paste path).
    /// </summary>
    public static (byte[] Bytes, string GameFile, string SourceGroup) ReadLiveCameraDecompressedPayloadForJson(
        string gameDir, Action<string>? log = null)
    {
        var entry = FindCameraEntry(gameDir);
        log?.Invoke("Reading live camera chunk from PAZ (decompressed patch baseline)...");
        byte[] raw = ReadLiveCameraPayloadBytes(entry);
        byte[] decompressed = DecompressCameraPayloadFromRaw(raw, entry);

        string sourceGroup = Path.GetFileName(Path.GetDirectoryName(entry.PazFile) ?? "0010");
        if (string.IsNullOrEmpty(sourceGroup)) sourceGroup = "0010";

        return (decompressed, entry.Path, sourceGroup);
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
        bool bane, double combatPullback = 0.0, bool mountHeight = false, double? customUp = null,
        bool steadycam = true, Action<string>? log = null)
    {
        log?.Invoke("Finding camera entry...");
        var entry = FindCameraEntry(gameDir);
        log?.Invoke($"Found: offset={entry.Offset}, comp_size={entry.CompSize}, orig_size={entry.OrigSize}");

        log?.Invoke("Ensuring backup...");
        EnsureBackup(entry, log);

        log?.Invoke("Extracting vanilla XML...");
        string vanillaXml = GetVanillaXml(entry);

        bool needsInjection = steadycam;
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
        var modSet = CameraRules.BuildModifications(style, fov, bane, combatPullback: combatPullback,
            mountHeight: mountHeight, customUp: customUp, steadycam: steadycam);
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
