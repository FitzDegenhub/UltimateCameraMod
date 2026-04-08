using System.Runtime.InteropServices;

namespace UltimateCameraMod.Services;

/// <summary>
/// Creates the appropriate game detector for the current platform.
/// </summary>
public static class GameDetectorFactory
{
    public static IGameDetector Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsGameDetector();
        else
            return new LinuxGameDetector();
    }
}
