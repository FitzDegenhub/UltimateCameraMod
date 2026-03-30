using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UltimateCameraMod.Models;

public class AdvancedRow : INotifyPropertyChanged
{
    private string _value = "";

    public string Section { get; set; } = "";
    public string SubElement { get; set; } = "";
    public string Attribute { get; set; } = "";
    public string VanillaValue { get; init; } = "";

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

    public string ModKey => string.IsNullOrEmpty(SubElement) ? Section : $"{Section}/{SubElement}";
    public string FullKey => $"{ModKey}.{Attribute}";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
