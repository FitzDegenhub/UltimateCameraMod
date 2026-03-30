using System.Text;
using System.Text.Json;

namespace UltimateCameraMod.Models;

/// <summary>
/// Base64 import/export of custom presets. Port of the import/export logic from main.py.
/// </summary>
public static class PresetCodec
{
    private const string Prefix = "UWD:";

    public static string Encode(string name, double distance, double height, double rightOffset)
    {
        var obj = new { n = name, d = Math.Round(distance, 2), h = Math.Round(height, 2), r = Math.Round(rightOffset, 2) };
        string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = false });
        string b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .Replace('+', '-').Replace('/', '_');
        return Prefix + b64;
    }

    public static (string Name, double Distance, double Height, double RightOffset) Decode(string text)
    {
        text = text.Trim();
        if (!text.StartsWith(Prefix))
            throw new FormatException("Not a valid preset string (must start with UWD:)");

        string b64 = text[Prefix.Length..].Replace('-', '+').Replace('_', '/');
        int pad = (4 - b64.Length % 4) % 4;
        b64 += new string('=', pad);

        byte[] raw;
        try { raw = Convert.FromBase64String(b64); }
        catch { throw new FormatException("Corrupt import string — base64 decode failed"); }

        JsonDocument doc;
        try { doc = JsonDocument.Parse(raw); }
        catch { throw new FormatException("Corrupt import string — JSON decode failed"); }

        var root = doc.RootElement;
        string name = root.TryGetProperty("n", out var n) ? n.GetString() ?? "Imported" : "Imported";
        if (name.Length > 30) name = name[..30];

        double distance = root.TryGetProperty("d", out var d) ? d.GetDouble() : 5.0;
        double height = root.TryGetProperty("h", out var h) ? h.GetDouble() : 0.0;
        double rightOffset = root.TryGetProperty("r", out var r) ? r.GetDouble() : 0.0;

        distance = Math.Clamp(distance, 1.5, 12.0);
        height = Math.Clamp(height, -1.6, 0.5);
        rightOffset = Math.Clamp(rightOffset, -1.0, 1.0);

        return (name, distance, height, rightOffset);
    }
}
