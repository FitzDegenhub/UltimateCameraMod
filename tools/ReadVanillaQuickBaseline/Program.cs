using UltimateCameraMod.Services;

string gameDir = args.Length > 0
    ? args[0]
    : @"c:\Program Files (x86)\Steam\steamapps\common\Crimson Desert";

if (!Directory.Exists(gameDir))
{
    Console.Error.WriteLine($"Game dir not found: {gameDir}");
    return 1;
}

string tmp = Path.Combine(Path.GetTempPath(), "ucm_vanilla_check_backups");
Directory.CreateDirectory(tmp);
CameraMod.BackupsDirOverride = () => tmp;
CameraMod.AppVersion = "";

try
{
    string xml = CameraMod.ReadVanillaXml(gameDir);
    bool ok = CameraMod.TryParseUcmQuickFootBaselineFromXml(xml, out double dist, out double up, out double ro);
    Console.WriteLine($"GameDir: {gameDir}");
    Console.WriteLine($"TryParseUcmQuickFootBaselineFromXml: ok={ok}");
    Console.WriteLine($"  Player_Basic_Default/ZoomLevel[2].ZoomDistance -> distance slider: {dist}");
    Console.WriteLine($"  Player_Basic_Default/ZoomLevel[2].UpOffset      -> height slider:   {up}");
    Console.WriteLine($"  Player_Basic_Default/ZoomLevel[2].RightOffset   -> h-shift slider:  {ro}");
    Console.WriteLine($"XML length (chars): {xml.Length}");
    return ok ? 0 : 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}
