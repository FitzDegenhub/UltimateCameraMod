using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UltimateCameraMod.V3.Models;

public sealed class PresetManagerItem : INotifyPropertyChanged
{
    private bool _isLocked;

    public string Name { get; set; } = "";
    public string KindId { get; set; } = "";
    public string KindLabel { get; set; } = "";
    public string SourceLabel { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string SummaryText { get; set; } = "";
    public string FilePath { get; set; } = "";
    public bool CanRebuild { get; set; }
    public string Url { get; set; } = "";

    /// <summary>Invisible placeholder used to keep a group header visible when no real items exist.</summary>
    public bool IsPlaceholder { get; set; }

    /// <summary>
    /// When true, the preset file is treated as read-only in the editor (toggled via sidebar padlock).
    /// Persisted as <c>locked</c> in session JSON or <c>Locked</c> on imported presets.
    /// </summary>
    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            if (_isLocked == value) return;
            _isLocked = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LockGlyph));
            OnPropertyChanged(nameof(LockToolTip));
        }
    }

    /// <summary>UCM-shipped presets (style/default) are permanently locked.</summary>
    public bool IsUcmPreset => KindId is "style" or "default";

    /// <summary>Sidebar padlock icon (closed = locked).</summary>
    public string LockGlyph => IsLocked ? "\uD83D\uDD12" : "\uD83D\uDD13";

    public string LockToolTip =>
        IsUcmPreset
            ? "UCM preset — duplicate to create an editable copy"
            : IsLocked
                ? "Locked — click to allow editing this preset"
                : "Unlocked — click to lock and prevent changes";

    public string DisplayName => Name;

    public string SecondaryLine => StatusText;

    /// <summary>Group label for sidebar display (ucm_presets, community_presets, my_presets, import_presets).</summary>
    public string GroupLabel => KindId switch
    {
        "default" => "UCM presets",
        "style" => "UCM presets",
        "community" => "Community presets",
        "user" => "My presets",
        "imported" => "Imported",
        _ => "Other"
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
