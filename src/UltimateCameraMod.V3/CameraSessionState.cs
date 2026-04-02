using System.Globalization;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.V3;

public sealed class CameraSessionState
{
    public const string DefaultNexusUrl = "https://www.nexusmods.com/crimsondesert/mods/438";

    public string StyleId { get; set; } = "cinematic";
    public int FovDelta { get; set; } = 25;
    public string CombatId { get; set; } = "default";
    public bool CenterCharacter { get; set; }
    public bool MatchMountHeight { get; set; }
    public bool Steadycam { get; set; } = true;

    public double CustomDistance { get; set; } = 5.0;
    public double CustomHeight { get; set; }
    public double CustomRightOffset { get; set; }

    public string JsonTitle { get; set; } = "Ultimate Camera Mod V3";
    public string JsonVersion { get; set; } = "3.0.0";
    public string JsonAuthor { get; set; } = "0xFitz";
    public string JsonDescription { get; set; } = "Exported camera profile for Crimson Desert Mod Manager.";
    public string JsonNexusUrl { get; set; } = DefaultNexusUrl;

    public Dictionary<string, string> ParameterOverrides { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public ModificationSet BuildBaseModSet()
    {
        if (StyleId.Equals("custom", StringComparison.OrdinalIgnoreCase))
            CameraRules.RegisterCustomStyle(CustomDistance, CustomHeight, CustomRightOffset);

        return CameraRules.BuildModifications(
            StyleId,
            FovDelta,
            CenterCharacter,
            CombatId,
            mountHeight: MatchMountHeight,
            customUp: StyleId.Equals("custom", StringComparison.OrdinalIgnoreCase) ? CustomHeight : null,
            steadycam: Steadycam);
    }

    public ModificationSet BuildEffectiveModSet()
    {
        var modSet = BuildBaseModSet();
        foreach (var (fullKey, value) in ParameterOverrides)
        {
            if (!TrySplitFullKey(fullKey, out string modKey, out string attr))
                continue;

            if (!modSet.ElementMods.TryGetValue(modKey, out var attrs))
            {
                attrs = new Dictionary<string, (string Action, string Value)>(StringComparer.OrdinalIgnoreCase);
                modSet.ElementMods[modKey] = attrs;
            }

            attrs[attr] = ("SET", value);
        }

        return modSet;
    }

    public SessionPreview BuildPreview(string vanillaXml)
    {
        string baseXml = CameraMod.ApplyModifications(vanillaXml, BuildBaseModSet());
        string effectiveXml = CameraMod.ApplyModifications(vanillaXml, BuildEffectiveModSet());

        var baseRows = CameraMod.ParseXmlToRows(baseXml);
        var effectiveRows = CameraMod.ParseXmlToRows(effectiveXml);

        var baseValues = baseRows.ToDictionary(r => r.FullKey, r => r.Value, StringComparer.OrdinalIgnoreCase);
        var effectiveValues = effectiveRows.ToDictionary(r => r.FullKey, r => r.Value, StringComparer.OrdinalIgnoreCase);

        var rows = new List<AdvancedRow>(effectiveRows.Count);
        foreach (var row in effectiveRows)
        {
            string baseValue = baseValues.TryGetValue(row.FullKey, out string? resolvedBase)
                ? resolvedBase
                : row.Value;

            rows.Add(new AdvancedRow
            {
                Section = row.Section,
                SubElement = row.SubElement,
                Attribute = row.Attribute,
                VanillaValue = baseValue,
                Value = row.Value,
            });
        }

        return new SessionPreview(baseValues, effectiveValues, rows, ParameterOverrides.Count);
    }

    public void SetOverride(string fullKey, string? value, string? baseValue = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ParameterOverrides.Remove(fullKey);
            return;
        }

        string normalizedValue = Normalize(value);
        if (!string.IsNullOrEmpty(baseValue) && Normalize(baseValue) == normalizedValue)
        {
            ParameterOverrides.Remove(fullKey);
            return;
        }

        ParameterOverrides[fullKey] = normalizedValue;
    }

    public string BuildJourneySummary()
    {
        string style = StyleId switch
        {
            "western" => "Heroic",
            "cinematic" => "Panoramic",
            "default" => "Vanilla",
            "immersive" => "Close-Up",
            "lowcam" => "Low Rider",
            "vlowcam" => "Knee Cam",
            "ulowcam" => "Dirt Cam",
            "re2" => "Survival",
            "custom" => "Custom",
            _ => StyleId,
        };

        return $"{style} | FoV +{FovDelta} | {ParameterOverrides.Count} shared overrides";
    }

    public static string FormatDouble(double value)
    {
        string text = value.ToString("0.##", CultureInfo.InvariantCulture);
        return text == "-0" ? "0" : text;
    }

    public static bool TryParseFullKey(string fullKey, out string modKey, out string attribute)
    {
        return TrySplitFullKey(fullKey, out modKey, out attribute);
    }

    private static bool TrySplitFullKey(string fullKey, out string modKey, out string attribute)
    {
        modKey = "";
        attribute = "";

        int idx = fullKey.LastIndexOf('.');
        if (idx <= 0 || idx >= fullKey.Length - 1)
            return false;

        modKey = fullKey[..idx];
        attribute = fullKey[(idx + 1)..];
        return true;
    }

    private static string Normalize(string value)
    {
        string trimmed = value.Trim();
        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            return FormatDouble(number);

        return trimmed;
    }
}

public sealed record SessionPreview(
    IReadOnlyDictionary<string, string> BaseValues,
    IReadOnlyDictionary<string, string> EffectiveValues,
    IReadOnlyList<AdvancedRow> Rows,
    int OverrideCount);

