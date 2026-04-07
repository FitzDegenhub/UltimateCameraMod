using System.Text.Json;

namespace UltimateCameraMod.Services;

/// <summary>
/// Auto-detect Crimson Desert game directory on Windows via Registry, Epic manifests, and drive scanning.
/// </summary>
public class WindowsGameDetector : IGameDetector
{
    private static bool IsGameDir(string path) =>
        Directory.Exists(path) && File.Exists(Path.Combine(path, "0010", "0.paz"));

    public (string? Path, string Platform) FindGameDir()
    {
        // Method 1: Relative to exe (mod dropped inside game folder)
        string exeDir = AppContext.BaseDirectory;
        string? parent = Path.GetDirectoryName(exeDir);
        if (parent != null && IsGameDir(parent))
            return (parent, "Local");

        // Method 2: Steam — registry + libraryfolders.vdf
        var steam = TrySteam();
        if (steam != null) return (steam, "Steam");

        // Method 3: Epic Games — registry + .item manifests
        var epic = TryEpic();
        if (epic != null) return (epic, "Epic");

        // Method 4: Brute-force all drives (Steam, Epic, Xbox, generic)
        var brute = TryBruteForce();
        if (brute.Path != null) return brute;

        return (null, "Unknown");
    }

    public string? FindGameDirLegacy() => FindGameDir().Path;

    public bool CheckWritePermission(string gameDir)
    {
        try
        {
            string pazFile = Path.Combine(gameDir, "0010", "0.paz");
            using var fs = File.Open(pazFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Steam ────────────────────────────────────────────────────────

    private static string? TrySteam()
    {
        try
        {
            // Microsoft.Win32.Registry is available on Windows via the .NET runtime
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (key == null) return null;
            using (key)
            {
                string? steamPath = key.GetValue("SteamPath") as string;
                if (steamPath == null) return null;

                steamPath = steamPath.Replace("/", "\\");

                string candidate = Path.Combine(steamPath, "steamapps", "common", "Crimson Desert");
                if (IsGameDir(candidate)) return candidate;

                string vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(vdf)) return null;

                foreach (var line in File.ReadLines(vdf))
                {
                    if (!line.Contains("\"path\"")) continue;
                    var parts = line.Split('"');
                    if (parts.Length < 4) continue;
                    string libPath = parts[3].Replace("\\\\", "\\");
                    string c = Path.Combine(libPath, "steamapps", "common", "Crimson Desert");
                    if (IsGameDir(c)) return c;
                }
            }
        }
        catch { }

        return null;
    }

    // ── Epic Games ───────────────────────────────────────────────────

    private static string? TryEpic()
    {
        try
        {
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\WOW6432Node\Epic Games\EpicGamesLauncher");
            if (key == null) return null;
            using (key)
            {
                string? appDataPath = key.GetValue("AppDataPath") as string;
                if (appDataPath == null) return null;

                string manifestDir = Path.Combine(appDataPath, "Manifests");
                if (!Directory.Exists(manifestDir)) return null;

                foreach (string itemFile in Directory.GetFiles(manifestDir, "*.item"))
                {
                    try
                    {
                        string json = File.ReadAllText(itemFile);
                        if (!json.Contains("Crimson Desert", StringComparison.OrdinalIgnoreCase))
                            continue;

                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("InstallLocation", out var locProp))
                        {
                            string loc = locProp.GetString()?.Replace("\\\\", "\\") ?? "";
                            if (IsGameDir(loc)) return loc;
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }

        return null;
    }

    // ── Brute-force drive scan ───────────────────────────────────────

    private static readonly string[] PathTemplates =
    {
        // Steam
        @"{0}:\SteamLibrary\steamapps\common\Crimson Desert",
        @"{0}:\Steam\steamapps\common\Crimson Desert",
        @"{0}:\Games\Steam\steamapps\common\Crimson Desert",
        @"{0}:\Program Files (x86)\Steam\steamapps\common\Crimson Desert",
        @"{0}:\Program Files\Steam\steamapps\common\Crimson Desert",

        // Epic Games
        @"{0}:\Epic Games\Crimson Desert",
        @"{0}:\Program Files\Epic Games\Crimson Desert",
        @"{0}:\Games\Epic Games\Crimson Desert",

        // Xbox / Game Pass
        @"{0}:\XboxGames\Crimson Desert\Content",
        @"{0}:\XboxGames\Crimson Desert",
        @"{0}:\Xbox Games\Crimson Desert\Content",
        @"{0}:\Xbox Games\Crimson Desert",

        // Generic
        @"{0}:\Games\Crimson Desert",
    };

    private static (string? Path, string Platform) TryBruteForce()
    {
        foreach (char drive in "CDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            if (!Directory.Exists($@"{drive}:\")) continue;

            foreach (var tmpl in PathTemplates)
            {
                string p = string.Format(tmpl, drive);
                if (!IsGameDir(p)) continue;

                string platform = "Unknown";
                if (tmpl.Contains("Steam")) platform = "Steam";
                else if (tmpl.Contains("Epic")) platform = "Epic";
                else if (tmpl.Contains("Xbox")) platform = "Xbox/GamePass";

                return (p, platform);
            }
        }

        return (null, "Unknown");
    }
}
