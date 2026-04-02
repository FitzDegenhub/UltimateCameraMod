namespace UltimateCameraMod.V3.Models;

public sealed class PresetManagerItem
{
    public string Name { get; set; } = "";
    public string KindId { get; set; } = "";
    public string KindLabel { get; set; } = "";
    public string SourceLabel { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string SummaryText { get; set; } = "";
    public string FilePath { get; set; } = "";
    public bool CanRebuild { get; set; }

    public string DisplayName => Name;

    public string SecondaryLine => StatusText;

    /// <summary>Group label for sidebar display (ucm_presets, my_presets, import_presets).</summary>
    public string GroupLabel => KindId switch
    {
        "default" => "UCM presets",
        "style" => "UCM presets",
        "user" => "My presets",
        "imported" => "Imported",
        _ => "Other"
    };
}
