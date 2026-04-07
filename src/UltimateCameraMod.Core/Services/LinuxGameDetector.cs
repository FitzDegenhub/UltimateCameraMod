using System.Text.RegularExpressions;

namespace UltimateCameraMod.Services;

/// <summary>
/// Detects Crimson Desert game directory on Linux systems.
/// Searches Steam (native + Flatpak), Heroic Games Launcher, Lutris, Bottles, and common paths.
/// </summary>
public class LinuxGameDetector : IGameDetector
{
    private static bool IsGameDir(string path) =>
        Directory.Exists(path) && File.Exists(Path.Combine(path, "0010", "0.paz"));

    public (string? Path, string Platform) FindGameDir()
    {
        // Method 1: Relative to exe (mod dropped inside game folder)
        string exeDir = AppContext.BaseDirectory;
        string? parent = System.IO.Path.GetDirectoryName(exeDir);
        if (parent != null && IsGameDir(parent))
            return (parent, "Local");

        // Method 2: Steam (native Linux)
        var steam = TrySteamNative();
        if (steam != null) return (steam, "Steam");

        // Method 3: Steam (Flatpak)
        var steamFlatpak = TrySteamFlatpak();
        if (steamFlatpak != null) return (steamFlatpak, "Steam (Flatpak)");

        // Method 4: Heroic Games Launcher (Epic on Linux)
        var heroic = TryHeroic();
        if (heroic != null) return (heroic, "Heroic/Epic");

        // Method 5: Bottles (Wine manager)
        var bottles = TryBottles();
        if (bottles != null) return (bottles, "Bottles");

        // Method 6: Brute-force common Linux paths
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

    // ── Steam (native) ──────────────────────────────────────────────

    private static string? TrySteamNative()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Common Steam root paths on Linux
        string[] steamRoots =
        {
            Path.Combine(home, ".steam", "steam"),
            Path.Combine(home, ".local", "share", "Steam"),
            Path.Combine(home, ".steam", "debian-installation"),
        };

        foreach (string steamRoot in steamRoots)
        {
            var result = TrySteamRoot(steamRoot);
            if (result != null) return result;
        }

        return null;
    }

    // ── Steam (Flatpak) ─────────────────────────────────────────────

    private static string? TrySteamFlatpak()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string flatpakRoot = Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".steam", "steam");
        return TrySteamRoot(flatpakRoot);
    }

    /// <summary>
    /// Shared Steam library detection: check default steamapps/common, then parse libraryfolders.vdf
    /// for additional library folders.
    /// </summary>
    private static string? TrySteamRoot(string steamRoot)
    {
        if (!Directory.Exists(steamRoot)) return null;

        try
        {
            // Direct check in default library
            string candidate = Path.Combine(steamRoot, "steamapps", "common", "Crimson Desert");
            if (IsGameDir(candidate)) return candidate;

            // Parse libraryfolders.vdf for additional library paths
            string vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdf)) return null;

            foreach (string line in File.ReadLines(vdf))
            {
                if (!line.Contains("\"path\"")) continue;
                string[] parts = line.Split('"');
                if (parts.Length < 4) continue;

                string libPath = parts[3].Replace("\\\\", "/");
                string c = Path.Combine(libPath, "steamapps", "common", "Crimson Desert");
                if (IsGameDir(c)) return c;
            }
        }
        catch { }

        return null;
    }

    // ── Heroic Games Launcher ───────────────────────────────────────

    private static string? TryHeroic()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Heroic stores game configs in ~/.config/heroic/
        string[] heroicConfigDirs =
        {
            Path.Combine(home, ".config", "heroic"),
            Path.Combine(home, ".var", "app", "com.heroicgameslauncher.hgl", "config", "heroic"),
        };

        foreach (string configDir in heroicConfigDirs)
        {
            var result = TryHeroicConfig(configDir);
            if (result != null) return result;
        }

        // Also check common Heroic install locations
        string[] heroicGameDirs =
        {
            Path.Combine(home, "Games", "Heroic", "Crimson Desert"),
            Path.Combine(home, "Games", "Crimson Desert"),
        };

        foreach (string dir in heroicGameDirs)
        {
            if (IsGameDir(dir)) return dir;
        }

        return null;
    }

    private static string? TryHeroicConfig(string configDir)
    {
        if (!Directory.Exists(configDir)) return null;

        try
        {
            // Check Legendary library (Epic)
            string legendaryLib = Path.Combine(configDir, "store_cache", "legendary_library.json");
            if (File.Exists(legendaryLib))
            {
                string json = File.ReadAllText(legendaryLib);
                if (json.Contains("Crimson Desert", StringComparison.OrdinalIgnoreCase))
                {
                    var path = ExtractInstallPathFromJson(json, "Crimson Desert");
                    if (path != null && IsGameDir(path)) return path;
                }
            }

            // Check GOG library
            string gogLib = Path.Combine(configDir, "store_cache", "gog_library.json");
            if (File.Exists(gogLib))
            {
                string json = File.ReadAllText(gogLib);
                if (json.Contains("Crimson Desert", StringComparison.OrdinalIgnoreCase))
                {
                    var path = ExtractInstallPathFromJson(json, "Crimson Desert");
                    if (path != null && IsGameDir(path)) return path;
                }
            }

            // Check GamesConfig for individual game install paths
            string gamesConfigDir = Path.Combine(configDir, "GamesConfig");
            if (Directory.Exists(gamesConfigDir))
            {
                foreach (string file in Directory.GetFiles(gamesConfigDir, "*.json"))
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        if (!content.Contains("Crimson Desert", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var path = ExtractInstallPathFromJson(content, "Crimson Desert");
                        if (path != null && IsGameDir(path)) return path;
                    }
                    catch { }
                }
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Simple JSON path extraction — looks for install_path or install_location fields
    /// near a "Crimson Desert" reference.
    /// </summary>
    private static string? ExtractInstallPathFromJson(string json, string gameName)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return SearchJsonForInstallPath(doc.RootElement, gameName);
        }
        catch
        {
            return null;
        }
    }

    private static string? SearchJsonForInstallPath(System.Text.Json.JsonElement element, string gameName)
    {
        if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            bool hasGameRef = false;
            string? installPath = null;

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    string val = prop.Value.GetString() ?? "";
                    if (val.Contains(gameName, StringComparison.OrdinalIgnoreCase))
                        hasGameRef = true;

                    if (prop.Name.Equals("install_path", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Equals("install_location", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Equals("InstallLocation", StringComparison.OrdinalIgnoreCase))
                    {
                        installPath = val;
                    }
                }
                else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Object ||
                         prop.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var result = SearchJsonForInstallPath(prop.Value, gameName);
                    if (result != null) return result;
                }
            }

            if (hasGameRef && installPath != null)
                return installPath;
        }
        else if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var result = SearchJsonForInstallPath(item, gameName);
                if (result != null) return result;
            }
        }

        return null;
    }

    // ── Bottles ─────────────────────────────────────────────────────

    private static string? TryBottles()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string bottlesDir = Path.Combine(home, ".local", "share", "bottles", "bottles");
        if (!Directory.Exists(bottlesDir)) return null;

        try
        {
            foreach (string bottleDir in Directory.GetDirectories(bottlesDir))
            {
                string driveC = Path.Combine(bottleDir, "drive_c");
                if (!Directory.Exists(driveC)) continue;

                string[] candidates =
                {
                    Path.Combine(driveC, "Program Files (x86)", "Steam", "steamapps", "common", "Crimson Desert"),
                    Path.Combine(driveC, "Program Files", "Steam", "steamapps", "common", "Crimson Desert"),
                    Path.Combine(driveC, "Program Files", "Epic Games", "Crimson Desert"),
                    Path.Combine(driveC, "Games", "Crimson Desert"),
                };

                foreach (string candidate in candidates)
                {
                    if (IsGameDir(candidate)) return candidate;
                }
            }
        }
        catch { }

        return null;
    }

    // ── Brute-force common paths ────────────────────────────────────

    private static (string? Path, string Platform) TryBruteForce()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Common game directory patterns on Linux
        string[] directPaths =
        {
            Path.Combine(home, "Games", "Crimson Desert"),
            Path.Combine(home, ".local", "share", "Crimson Desert"),
            "/opt/games/Crimson Desert",
        };

        foreach (string p in directPaths)
        {
            if (IsGameDir(p)) return (p, "Unknown");
        }

        // Scan mounted drives (common for external SSDs / secondary drives)
        string[] mountPoints = { "/mnt", "/media", "/run/media" };
        foreach (string mount in mountPoints)
        {
            if (!Directory.Exists(mount)) continue;

            try
            {
                // /mnt/*, /media/user/*, /run/media/user/*
                foreach (string sub in Directory.GetDirectories(mount))
                {
                    string[] steamPatterns =
                    {
                        Path.Combine(sub, "SteamLibrary", "steamapps", "common", "Crimson Desert"),
                        Path.Combine(sub, "steamapps", "common", "Crimson Desert"),
                        Path.Combine(sub, "Games", "Crimson Desert"),
                    };

                    foreach (string p in steamPatterns)
                    {
                        if (IsGameDir(p)) return (p, p.Contains("Steam", StringComparison.OrdinalIgnoreCase) ? "Steam" : "Unknown");
                    }

                    // Also check one level deeper (e.g., /run/media/user/DriveName/SteamLibrary/...)
                    try
                    {
                        foreach (string subsub in Directory.GetDirectories(sub))
                        {
                            string[] deepPatterns =
                            {
                                Path.Combine(subsub, "SteamLibrary", "steamapps", "common", "Crimson Desert"),
                                Path.Combine(subsub, "Games", "Crimson Desert"),
                            };

                            foreach (string p in deepPatterns)
                            {
                                if (IsGameDir(p)) return (p, p.Contains("Steam", StringComparison.OrdinalIgnoreCase) ? "Steam" : "Unknown");
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        return (null, "Unknown");
    }
}
