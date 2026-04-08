using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace UltimateCameraMod.V3.Localization;

public sealed class TranslationSource : INotifyPropertyChanged
{
    public static TranslationSource Instance { get; } = new();

    private readonly ResourceManager _rm = V3.Properties.Resources.ResourceManager;
    private CultureInfo _currentCulture = CultureInfo.InvariantCulture;

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            if (Equals(_currentCulture, value)) return;
            _currentCulture = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }

    public string this[string key] =>
        _rm.GetString(key, _currentCulture) ?? key;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly CultureInfo[] AvailableLanguages =
    {
        new("en"),
        new("ko"),
        new("ja"),
        new("zh-CN"),
        new("sv"),
        new("de"),
        new("fr"),
        new("es"),
        new("pt-BR"),
        new("ru"),
    };
}
