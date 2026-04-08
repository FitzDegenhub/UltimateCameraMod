using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UltimateCameraMod.Models;

public class AdvancedRow : INotifyPropertyChanged
{
    private string _value = "";
    private bool _isUserEdited;

    public string Section { get; set; } = "";
    public string SubElement { get; set; } = "";
    public string Attribute { get; set; } = "";
    public string VanillaValue { get; init; } = "";

    public string? AttributeDoc => CameraParamDocs.Get(Attribute);

    public string Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsModified));
        }
    }

    public bool IsModified => Value != VanillaValue;

    /// <summary>True when the user explicitly edited this value in God Mode. Sacred values are protected from Quick/Fine Tune rebuilds.</summary>
    public bool IsUserEdited
    {
        get => _isUserEdited;
        set { if (_isUserEdited == value) return; _isUserEdited = value; OnPropertyChanged(); }
    }

    public string ModKey => string.IsNullOrEmpty(SubElement) ? Section : $"{Section}/{SubElement}";
    public string FullKey => $"{ModKey}.{Attribute}";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
