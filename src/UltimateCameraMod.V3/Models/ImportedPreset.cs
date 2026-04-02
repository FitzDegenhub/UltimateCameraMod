using System.Text.Json.Serialization;

namespace UltimateCameraMod.V3.Models;

public sealed class ImportedPreset
{
    public const int CurrentFormatVersion = 1;

    public int FormatVersion { get; set; } = CurrentFormatVersion;
    public string Name { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string SourceDisplayName { get; set; } = "";
    public string? SourcePath { get; set; }
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastBuiltAtUtc { get; set; }
    public string RawXml { get; set; } = "";
    public ImportedPresetFingerprint? ImportedSourceFingerprint { get; set; }
    public ImportedPresetFingerprint? LastBuiltAgainst { get; set; }

    public Dictionary<string, string> Values { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>When true, sidebar shows locked and the editor does not apply changes to this preset.</summary>
    public bool Locked { get; set; }

    [JsonIgnore]
    public bool HasValues => Values.Count > 0;
}

public sealed class ImportedPresetFingerprint
{
    public string GameFile { get; set; } = "";
    public string SourceGroup { get; set; } = "";
    public int CompSize { get; set; }
    public int OrigSize { get; set; }
    public string ContentSha256 { get; set; } = "";
}
