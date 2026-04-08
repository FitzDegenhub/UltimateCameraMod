// Auto-generated. Do not edit manually.
// Generated from Properties/Resources.resx

using System.Resources;
using System.Globalization;

namespace UltimateCameraMod.V3.Properties;

public static class Resources
{
    private static ResourceManager? _rm;

    public static ResourceManager ResourceManager =>
        _rm ??= new ResourceManager("UltimateCameraMod.V3.Properties.Resources",
            typeof(Resources).Assembly);

    public static string GetString(string name) =>
        ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
}
