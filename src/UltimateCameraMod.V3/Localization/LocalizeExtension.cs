using System.Windows.Markup;

namespace UltimateCameraMod.V3.Localization;

public class LocExtension : MarkupExtension
{
    public string Key { get; set; } = "";

    public LocExtension() { }
    public LocExtension(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new System.Windows.Data.Binding($"[{Key}]")
        {
            Source = TranslationSource.Instance,
            Mode = System.Windows.Data.BindingMode.OneWay,
        }.ProvideValue(serviceProvider);
}
