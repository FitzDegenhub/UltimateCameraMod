namespace UltimateCameraMod.Services;

/// <summary>
/// Platform-abstracted game directory detection.
/// </summary>
public interface IGameDetector
{
    /// <summary>
    /// Attempt to locate the Crimson Desert game directory.
    /// Returns the path and detected platform (e.g. "Steam", "Epic", "Unknown").
    /// </summary>
    (string? Path, string Platform) FindGameDir();

    /// <summary>Backwards-compatible wrapper that returns just the path.</summary>
    string? FindGameDirLegacy() => FindGameDir().Path;

    /// <summary>Check whether the game directory's PAZ archive is writable.</summary>
    bool CheckWritePermission(string gameDir);
}
