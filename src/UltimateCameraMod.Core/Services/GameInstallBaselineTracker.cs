using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UltimateCameraMod.Services;

/// <summary>
/// Captures launcher/install metadata after a successful mod apply and compares on next launch
/// so users can reinstall after game patches (Steam appmanifest + universal PAZ entry sizes).
/// </summary>
public static class GameInstallBaselineTracker
{
    private const string BaselineFileName = "game_install_baseline.json";

    public sealed record BaselineFile(
        string UcmVersion,
        string GameDir,
        string Platform,
        long? SteamLastUpdated,
        long? SteamSizeOnDisk,
        DateTime CapturedUtc,
        DateTime? SnoozeUntilUtc);

    public sealed record Evaluation(bool ShowWarning, string Message);

    public static string BaselinePath(string exeDir) => Path.Combine(exeDir, BaselineFileName);

    public static BaselineFile? Load(string exeDir)
    {
        string path = BaselinePath(exeDir);
        if (!File.Exists(path)) return null;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            string ver = root.TryGetProperty("ucm_version", out var v) ? v.GetString() ?? "" : "";
            string gameDir = root.TryGetProperty("game_dir", out var g) ? g.GetString() ?? "" : "";
            string platform = root.TryGetProperty("platform", out var p) ? p.GetString() ?? "" : "";
            long? steamLu = TryGetInt64(root, "steam_last_updated");
            long? steamSz = TryGetInt64(root, "steam_size_on_disk");
            DateTime captured = root.TryGetProperty("captured_utc", out var c) && DateTime.TryParse(c.GetString(), null, DateTimeStyles.RoundtripKind, out var ct)
                ? ct.ToUniversalTime()
                : DateTime.MinValue;
            DateTime? snooze = null;
            if (root.TryGetProperty("snooze_until_utc", out var sz) && DateTime.TryParse(sz.GetString(), null, DateTimeStyles.RoundtripKind, out var st))
                snooze = st.ToUniversalTime();
            return new BaselineFile(ver, gameDir, platform, steamLu, steamSz, captured, snooze);
        }
        catch
        {
            return null;
        }
    }

    public static void SaveAfterSuccessfulInstall(string exeDir, string ucmVersion, string gameDir, string platform)
    {
        long? steamLu = null;
        long? steamSz = null;
        if (string.Equals(platform, "Steam", StringComparison.OrdinalIgnoreCase) &&
            TryReadSteamManifestBaselines(gameDir, out var lu, out var sz))
        {
            steamLu = lu;
            steamSz = sz;
        }

        var data = new Dictionary<string, object?>
        {
            ["ucm_version"] = ucmVersion,
            ["game_dir"] = gameDir,
            ["platform"] = platform,
            ["steam_last_updated"] = steamLu,
            ["steam_size_on_disk"] = steamSz,
            ["captured_utc"] = DateTime.UtcNow.ToString("O"),
        };

        WriteBaselineFile(exeDir, data);
    }

    public static void SetSnooze(string exeDir, TimeSpan duration)
    {
        var existing = Load(exeDir);
        var until = DateTime.UtcNow.Add(duration);
        var data = new Dictionary<string, object?>
        {
            ["ucm_version"] = existing?.UcmVersion ?? "",
            ["game_dir"] = existing?.GameDir ?? "",
            ["platform"] = existing?.Platform ?? "",
            ["steam_last_updated"] = existing?.SteamLastUpdated,
            ["steam_size_on_disk"] = existing?.SteamSizeOnDisk,
            ["captured_utc"] = existing is { CapturedUtc: var c } && c != DateTime.MinValue ? c.ToString("O") : DateTime.UtcNow.ToString("O"),
            ["snooze_until_utc"] = until.ToString("O"),
        };
        WriteBaselineFile(exeDir, data);
    }

    public static void Delete(string exeDir)
    {
        try
        {
            string p = BaselinePath(exeDir);
            if (File.Exists(p)) File.Delete(p);
        }
        catch { }
    }

    /// <summary>
    /// Compare live game + launcher state to last successful install baseline.
    /// </summary>
    public static Evaluation Evaluate(
        string exeDir,
        string ucmVersion,
        string gameDir,
        string platform,
        string backupsDir)
    {
        if (string.IsNullOrWhiteSpace(gameDir))
            return new Evaluation(false, "");

        BaselineFile? baseline = Load(exeDir);
        if (baseline != null && baseline.SnoozeUntilUtc.HasValue && DateTime.UtcNow < baseline.SnoozeUntilUtc.Value)
            return new Evaluation(false, "");

        bool steamMismatch = false;
        bool metaMismatch = false;

        if (baseline != null &&
            string.Equals(baseline.UcmVersion, ucmVersion, StringComparison.Ordinal) &&
            string.Equals(NormalizePath(baseline.GameDir), NormalizePath(gameDir), StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(platform, "Steam", StringComparison.OrdinalIgnoreCase) &&
                baseline.SteamLastUpdated.HasValue &&
                TryReadSteamManifestBaselines(gameDir, out var curLu, out var curSz))
            {
                if (curLu != baseline.SteamLastUpdated.Value)
                    steamMismatch = true;
                else if (baseline.SteamSizeOnDisk.HasValue && curSz != baseline.SteamSizeOnDisk.Value)
                    steamMismatch = true;
            }
        }

        string metaPath = Path.Combine(backupsDir, "backup_meta.txt");
        if (File.Exists(metaPath))
        {
            try
            {
                var entry = CameraMod.FindCameraEntry(gameDir);
                if (TryParseBackupMeta(metaPath, out int savedComp, out int savedOrig))
                {
                    if (savedComp != entry.CompSize || savedOrig != entry.OrigSize)
                        metaMismatch = true;
                }
            }
            catch
            {
                // Unreadable game dir / PAMT — skip meta check
            }
        }

        if (!steamMismatch && !metaMismatch)
            return new Evaluation(false, "");

        string msg;
        if (steamMismatch && metaMismatch)
            msg = "Game updated or camera data changed — use Install to game again to refresh backup and 0.paz.";
        else if (steamMismatch)
            msg = "Steam updated this install since your last UCM install — use Install to game again.";
        else
            msg = "Game camera data changed since your last UCM backup — use Install to game again.";

        return new Evaluation(true, msg);
    }

    public static bool TryParseBackupMeta(string metaPath, out int compSize, out int origSize)
    {
        compSize = 0;
        origSize = 0;
        if (!File.Exists(metaPath)) return false;
        try
        {
            string text = File.ReadAllText(metaPath);
            bool compOk = false, origOk = false;
            foreach (var part in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.StartsWith("comp_size=", StringComparison.Ordinal) &&
                    int.TryParse(part["comp_size=".Length..], out int c))
                {
                    compSize = c;
                    compOk = true;
                }
                else if (part.StartsWith("orig_size=", StringComparison.Ordinal) &&
                         int.TryParse(part["orig_size=".Length..], out int o))
                {
                    origSize = o;
                    origOk = true;
                }
            }
            return compOk && origOk;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads Steam <c>LastUpdated</c> and <c>SizeOnDisk</c> from the app manifest for this install folder.
    /// </summary>
    public static bool TryReadSteamManifestBaselines(string gameDir, out long lastUpdated, out long sizeOnDisk)
    {
        lastUpdated = 0;
        sizeOnDisk = 0;
        try
        {
            string? steamApps = GetSteamAppsDirectoryFromGameDir(gameDir);
            if (steamApps == null || !Directory.Exists(steamApps)) return false;

            string installdirToken = new DirectoryInfo(gameDir).Name;
            foreach (string acfPath in Directory.GetFiles(steamApps, "appmanifest_*.acf"))
            {
                string text;
                try { text = File.ReadAllText(acfPath); }
                catch { continue; }

                if (!TryGetAcfQuotedValue(text, "installdir", out string? idir) ||
                    !string.Equals((idir ?? "").Trim(), installdirToken, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!TryGetAcfQuotedValue(text, "LastUpdated", out string? luStr) || !long.TryParse(luStr, out lastUpdated))
                    lastUpdated = 0;
                if (!TryGetAcfQuotedValue(text, "SizeOnDisk", out string? szStr) || !long.TryParse(szStr, out sizeOnDisk))
                    sizeOnDisk = 0;
                return lastUpdated > 0;
            }
        }
        catch { }

        return false;
    }

    private static string? GetSteamAppsDirectoryFromGameDir(string gameDir)
    {
        try
        {
            var dir = new DirectoryInfo(gameDir);
            if (dir.Parent?.Name.Equals("common", StringComparison.OrdinalIgnoreCase) != true)
                return null;
            return dir.Parent.Parent?.FullName;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetAcfQuotedValue(string acfText, string key, out string? value)
    {
        value = null;
        var m = Regex.Match(acfText, $"\"{Regex.Escape(key)}\"\\s+\"([^\"]*)\"", RegexOptions.IgnoreCase);
        if (!m.Success) return false;
        value = m.Groups[1].Value;
        return true;
    }

    private static long? TryGetInt64(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var el)) return null;
        if (el.ValueKind == JsonValueKind.Null) return null;
        if (el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out long n)) return n;
        if (el.ValueKind == JsonValueKind.String && long.TryParse(el.GetString(), out long s)) return s;
        return null;
    }

    private static void WriteBaselineFile(string exeDir, Dictionary<string, object?> data)
    {
        string path = BaselinePath(exeDir);
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private static string NormalizePath(string path)
    {
        try
        {
            return Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }
        catch
        {
            return path;
        }
    }
}
