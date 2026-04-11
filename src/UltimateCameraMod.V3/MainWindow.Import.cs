using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using UltimateCameraMod.V3.Controls;
using UltimateCameraMod.V3.Localization;
using UltimateCameraMod.V3.Models;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.V3;

public partial class MainWindow : Window
{
    private async void OnImportPreset(object sender, RoutedEventArgs e)
    {
        string? mode = await ShowImportTypeOverlayAsync();
        if (mode == null) return;

        switch (mode)
        {
            case "mod_package": ImportModManagerPackage(); break;
            case "xml": ImportRawXml(); break;
            case "json": ImportJsonPatch(); break;
            case "paz": ImportFromPaz(); break;
            case "ucmpreset": ImportUcmPreset(); break;
        }
    }

    private async void ImportModManagerPackage()
    {
        using var folderDlg = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = L("Dlg_BrowseModFolder"),
            UseDescriptionForTitle = true
        };
        if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

        string folder = folderDlg.SelectedPath;
        try
        {
            string manifestPath = Path.Combine(folder, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                SetStatus(L("Status_NoManifestFound"), "Error");
                return;
            }

            using var manifestDoc = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var manifest = manifestDoc.RootElement;
            string title = manifest.TryGetProperty("title", out var tv) ? tv.GetString() ?? "" : Path.GetFileName(folder);
            string author = manifest.TryGetProperty("author", out var av) ? av.GetString() ?? "" : "";
            string rawDesc = manifest.TryGetProperty("description", out var dv) ? dv.GetString() ?? "" : "";
            string version = manifest.TryGetProperty("version", out var vv) ? vv.GetString() ?? "" : "";
            string nexusUrl = manifest.TryGetProperty("nexus_url", out var nu) ? nu.GetString() ?? "" : "";

            string filesDir = manifest.TryGetProperty("files_dir", out var fd) ? fd.GetString() ?? "files" : "files";
            string fullFilesDir = Path.GetFullPath(Path.Combine(folder, filesDir));
            if (!fullFilesDir.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus(L("Status_NoCameraXmlInPackage"), "Error");
                return;
            }
            string? xmlPath = Directory.GetFiles(fullFilesDir, "playercamerapreset.xml", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (xmlPath == null)
            {
                SetStatus(L("Status_NoCameraXmlInPackage"), "Error");
                return;
            }

            string xml = File.ReadAllText(xmlPath);
            string safeStem = new string(title.Trim().Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            if (string.IsNullOrWhiteSpace(safeStem))
                safeStem = "Imported_Mod";

            string shortDesc = rawDesc.Split("\n\n")[0].Replace("\n", " ").Trim();
            if (shortDesc.Length > 200) shortDesc = shortDesc[..197] + "...";

            var metaDlg = await ShowImportMetadataOverlayAsync(
                string.Format(L("Dlg_ImportingModPackage"), Path.GetFileName(folder)),
                safeStem,
                string.IsNullOrWhiteSpace(author) ? null : author,
                string.IsNullOrWhiteSpace(shortDesc) ? null : shortDesc,
                string.IsNullOrWhiteSpace(nexusUrl) ? null : nexusUrl);
            if (metaDlg == null) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                if (!await ShowConfirmOverlayAsync(L("Title_ImportPresetChooser"), L("Msg_OverwriteExists"), L("Btn_Overwrite"), L("Btn_Cancel")))
                    return;
            }

            string sourceLabel = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(version))
                sourceLabel += $" (v{version})";

            var imported = BuildImportedPreset(chosenName, "mod", sourceLabel, xmlPath, xml, null,
                metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(imported);
            RefreshPresetManagerLists(preserveSelection: false);
            SelectImportedPreset(SanitizeFileStem(imported.Name));
            QueueSavedToast(string.Format(L("Status_ImportedModPackage"), imported.Name));
            SetStatus(string.Format(L("Status_ImportedModPackage"), imported.Name), "Success");
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_ImportFailed"), ex.Message), "Error");
        }
    }

    private async void ImportRawXml()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = L("Dlg_ImportXmlFile"),
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        try
        {
            string xml = File.ReadAllText(ofd.FileName);
            string baseName = Path.GetFileNameWithoutExtension(ofd.FileName);
            if (baseName.Equals("playercamerapreset", StringComparison.OrdinalIgnoreCase))
                baseName = "Imported Camera";

            var metaDlg = await ShowImportMetadataOverlayAsync(
                string.Format(L("Dlg_ImportingXml"), Path.GetFileName(ofd.FileName)),
                baseName);
            if (metaDlg == null) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                if (!await ShowConfirmOverlayAsync(L("Msg_ConfirmImportXmlTitle"), L("Msg_OverwriteExists"), L("Btn_Overwrite"), L("Btn_Cancel")))
                    return;
            }

            var preset = BuildImportedPreset(chosenName, "xml", Path.GetFileName(ofd.FileName), ofd.FileName, xml,
                null, metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(preset);
            RefreshPresetManagerLists();
            SelectImportedPreset(preset.Name);
            QueueSavedToast(string.Format(L("Status_ImportedXml"), preset.Name, preset.Values.Count));
            SetStatus(string.Format(L("Status_ImportedXml"), preset.Name, preset.Values.Count), "Success");
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_XmlImportFailed"), ex.Message), "Error");
        }
    }

    private async void ImportFromPaz()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = L("Dlg_ImportPaz"),
            Filter = "PAZ files (*.paz)|*.paz|All files (*.*)|*.*",
            FileName = "0.paz"
        };
        if (ofd.ShowDialog(this) != true) return;

        string pazPath = ofd.FileName;
        string pamtPath;
        try
        {
            pamtPath = ResolvePamtPathForImportedPaz(pazPath);
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_PazImportFailed"), ex.Message), "Error");
            return;
        }

        SetGlobalBusy(true, L("Status_DecryptingPaz"));
        try
        {
            string xml;
            try
            {
                xml = await Task.Run(() => CameraMod.ReadXmlFromPaz(pazPath, pamtPath)).ConfigureAwait(true);
            }
            catch (Exception decEx)
            {
                SetGlobalBusy(false);
                _ = ShowAlertOverlayAsync(L("Title_PazImportFailed"),
                    $"{decEx.Message}\n\n{L("Help_PazImportErrorDetail")}",
                    isError: true);
                return;
            }
            SetGlobalBusy(false);

            var metaDlg = await ShowImportMetadataOverlayAsync(
                string.Format(L("Dlg_ImportingPaz"), Path.GetFileName(pazPath)),
                L("Dlg_ImportedCamera"));
            if (metaDlg == null) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                if (!await ShowConfirmOverlayAsync(L("Title_ImportPresetChooser"), L("Msg_OverwriteExists"), L("Btn_Overwrite"), L("Btn_Cancel")))
                    return;
            }

            var imported = BuildImportedPreset(chosenName, "paz", Path.GetFileName(pazPath), pazPath, xml, null,
                metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(imported);
            RefreshPresetManagerLists(preserveSelection: false);
            SelectImportedPreset(SanitizeFileStem(imported.Name));
            QueueSavedToast(string.Format(L("Status_ImportedPaz"), imported.Name));
            SetStatus(string.Format(L("Status_ImportedPaz"), imported.Name), "Success");
        }
        catch (Exception ex)
        {
            SetGlobalBusy(false);
            SetStatus(string.Format(L("Status_PazImportFailed"), ex.Message), "Error");
        }
    }

    private async void ImportJsonPatch()
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus(L("Status_GameFolderNotSet"), "Warn");
            return;
        }

        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = L("Dlg_ImportJson"),
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };
        if (ofd.ShowDialog(this) != true) return;

        SetGlobalBusy(true, L("Status_ApplyingJsonPatch"));
        try
        {
            string jsonText = File.ReadAllText(ofd.FileName);
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            // Extract metadata from modinfo
            string title = "";
            string author = "";
            string description = "";
            string nexusUrl = "";
            if (root.TryGetProperty("modinfo", out var modinfo))
            {
                title = modinfo.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                author = modinfo.TryGetProperty("author", out var a) ? a.GetString() ?? "" : "";
                description = modinfo.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
                nexusUrl = modinfo.TryGetProperty("nexus_url", out var u) ? u.GetString() ?? "" : "";
            }

            if (string.IsNullOrWhiteSpace(title))
                title = Path.GetFileNameWithoutExtension(ofd.FileName);

            // Two JSON patch formats exist:
            // 1. Full XML fragment patches (CDCamera) — hex decodes to complete XML tags, parsed semantically
            // 2. Byte-level patches (CrimsonCamera) — short hex sequences patched at byte offsets
            // Try semantic first; if no changes found, fall back to binary patching.
            string xml = await Task.Run(() =>
            {
                if (!root.TryGetProperty("patches", out var patches))
                    throw new InvalidOperationException("JSON file has no 'patches' array.");

                // Try semantic parsing first (full XML fragment patches)
                var modSet = BuildModSetFromJsonPatches(patches);
                if (modSet.ElementMods.Count > 0)
                {
                    string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
                    return CameraMod.ApplyModifications(vanillaXml, modSet);
                }

                // Fall back to binary patching against decompressed vanilla payload
                return ApplyBinaryJsonPatches(patches, _gameDir);
            }).ConfigureAwait(true);

            SetGlobalBusy(false);

            string shortDesc = description.Split("\n\n")[0].Replace("\n", " ").Trim();
            if (shortDesc.Length > 200) shortDesc = shortDesc[..197] + "...";

            var metaDlg = await ShowImportMetadataOverlayAsync(
                string.Format(L("Dlg_ImportingJson"), Path.GetFileName(ofd.FileName)),
                string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(ofd.FileName) : title,
                string.IsNullOrWhiteSpace(author) ? null : author,
                string.IsNullOrWhiteSpace(shortDesc) ? null : shortDesc,
                string.IsNullOrWhiteSpace(nexusUrl) ? null : nexusUrl);
            if (metaDlg == null) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                if (!await ShowConfirmOverlayAsync(L("Title_ImportPresetChooser"), L("Msg_OverwriteExists"), L("Btn_Overwrite"), L("Btn_Cancel")))
                    return;
            }

            var imported = BuildImportedPreset(chosenName, "json", Path.GetFileName(ofd.FileName), ofd.FileName, xml, null,
                metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(imported);
            RefreshPresetManagerLists(preserveSelection: false);
            SelectImportedPreset(SanitizeFileStem(imported.Name));
            QueueSavedToast(string.Format(L("Status_ImportedJson"), imported.Name));
            SetStatus(string.Format(L("Status_ImportedJson"), imported.Name), "Success");
        }
        catch (Exception ex)
        {
            SetGlobalBusy(false);
            SetStatus(string.Format(L("Status_JsonImportFailed"), ex.Message), "Error");
        }
    }

    private async void ImportUcmPreset()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = L("Dlg_ImportUcmPreset"),
            Filter = "UCM Preset (*.ucmpreset)|*.ucmpreset|JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = ""
        };
        if (ofd.ShowDialog(this) != true) return;

        try
        {
            string json = File.ReadAllText(ofd.FileName);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("session_xml", out _) && !root.TryGetProperty("RawXml", out _))
            {
                SetStatus(L("Status_NoSessionXmlInPreset"), "Error");
                return;
            }

            string name = root.TryGetProperty("name", out var nv) ? nv.GetString() ?? "" : Path.GetFileNameWithoutExtension(ofd.FileName);
            string safeName = new string(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            string destPath = Path.Combine(MyPresetsDir, $"{safeName}.ucmpreset");

            if (File.Exists(destPath))
            {
                if (!await ShowConfirmOverlayAsync(L("Dlg_ImportUcmPreset"), L("Msg_OverwriteExists"), L("Btn_Overwrite"), L("Btn_Cancel")))
                    return;
            }

            File.Copy(ofd.FileName, destPath, true);
            RefreshPresetManagerLists(preserveSelection: false);
            QueueSavedToast(string.Format(L("Status_ImportedUcmPreset"), name));
            SetStatus(string.Format(L("Status_ImportedUcmPreset"), name), "Success");
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_ImportFailed"), ex.Message), "Error");
        }
    }

    private void OnImportedPresetImportXml(object sender, RoutedEventArgs e)
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = L("Dlg_ImportXmlAsSavedPreset"),
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        try
        {
            string xml = File.ReadAllText(ofd.FileName);
            SaveImportedPresetFromXml("xml", Path.GetFileName(ofd.FileName), ofd.FileName, xml);
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_XmlImportFailed"), ex.Message), "Error");
        }
    }

    private ImportedPreset? LoadImportedPreset(string name)
    {
        string path = ImportedPresetPath(name);
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        var preset = JsonSerializer.Deserialize<ImportedPreset>(json);
        if (preset == null)
            return null;

        preset.Name = string.IsNullOrWhiteSpace(preset.Name) ? name : preset.Name;
        preset.Values ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        return preset;
    }

    private void SaveImportedPreset(ImportedPreset preset)
    {
        preset.Name = string.IsNullOrWhiteSpace(preset.Name) ? "imported_preset" : preset.Name.Trim();
        string path = ImportedPresetPath(preset.Name);
        File.WriteAllText(path, JsonSerializer.Serialize(preset, PresetFileJsonOptions));
    }

    private ImportedPreset BuildImportedPreset(string name, string sourceType, string sourceDisplayName,
        string? sourcePath, string xml, ImportedPresetFingerprint? importedFingerprint = null,
        string? author = null, string? description = null, string? url = null)
    {
        return new ImportedPreset
        {
            Name = name,
            SourceType = sourceType,
            SourceDisplayName = sourceDisplayName,
            SourcePath = sourcePath,
            Author = author,
            Description = description,
            Url = url,
            ImportedAtUtc = DateTime.UtcNow,
            RawXml = xml,
            ImportedSourceFingerprint = importedFingerprint,
            Values = BuildImportedValueMap(xml)
        };
    }

    private static Dictionary<string, string> BuildImportedValueMap(string xml)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in CameraMod.ParseXmlToRows(xml))
            values[row.FullKey] = row.Value;
        return values;
    }

    private static readonly HashSet<string> JsonPatchSubElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "CameraDamping", "CameraBlendParameter", "OffsetByVelocity", "PivotHeight",
        "ZoomLevel", "ApplyCameraSetting"
    };

    /// <summary>
    /// Parses CD JSON Mod Manager patch changes semantically: decodes original and patched hex
    /// as XML fragments, diffs their attributes, and builds a ModificationSet from the differences.
    /// Game-version independent — byte offsets are ignored. Patches are processed in offset order
    /// to track which camera section sub-elements belong to.
    /// </summary>
    private static ModificationSet BuildModSetFromJsonPatches(JsonElement patches)
    {
        var elementMods = new Dictionary<string, Dictionary<string, (string Action, string Value)>>(StringComparer.OrdinalIgnoreCase);
        string currentSection = "";

        foreach (var patch in patches.EnumerateArray())
        {
            if (!patch.TryGetProperty("changes", out var changes)) continue;
            foreach (var change in changes.EnumerateArray())
            {
                string origHex = change.GetProperty("original").GetString() ?? "";
                string patchedHex = change.GetProperty("patched").GetString() ?? "";
                if (origHex.Length == 0 || patchedHex.Length == 0) continue;

                string origText, patchedText;
                try
                {
                    origText = Encoding.UTF8.GetString(Convert.FromHexString(origHex));
                    patchedText = Encoding.UTF8.GetString(Convert.FromHexString(patchedHex));
                }
                catch { continue; }

                var origParsed = ParseXmlFragmentAttrs(origText);
                var patchedParsed = ParseXmlFragmentAttrs(patchedText);
                if (origParsed == null || patchedParsed == null) continue;
                if (origParsed.Value.Tag != patchedParsed.Value.Tag) continue;

                string tag = patchedParsed.Value.Tag;

                // Build the correct modKey based on element type
                string modKey;
                if (tag == "ZoomLevel")
                {
                    string level = patchedParsed.Value.Attrs.GetValueOrDefault("Level", "?");
                    modKey = string.IsNullOrEmpty(currentSection)
                        ? $"ZoomLevel[{level}]"
                        : $"{currentSection}/ZoomLevel[{level}]";
                }
                else if (JsonPatchSubElements.Contains(tag))
                {
                    modKey = string.IsNullOrEmpty(currentSection) ? tag : $"{currentSection}/{tag}";
                }
                else
                {
                    // Section-level tag — update current section tracker
                    currentSection = tag;
                    modKey = tag;
                }

                // Find attributes that changed
                foreach (var (attr, val) in patchedParsed.Value.Attrs)
                {
                    string? origVal = origParsed.Value.Attrs.GetValueOrDefault(attr);
                    if (origVal == val) continue; // unchanged

                    if (!elementMods.TryGetValue(modKey, out var attrs))
                    {
                        attrs = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
                        elementMods[modKey] = attrs;
                    }
                    attrs[attr] = ("SET", val);
                }
            }
        }

        return new ModificationSet { ElementMods = elementMods, FovValue = 0 };
    }

    /// <summary>
    /// Extracts the tag name and attributes from an XML fragment like
    /// &lt;Player_Basic_Default Type="TPS" Fov="40"&gt; or &lt;ZoomLevel Level="2" .../&gt;
    /// </summary>
    private static (string Tag, Dictionary<string, string> Attrs)? ParseXmlFragmentAttrs(string fragment)
    {
        fragment = fragment.Trim();
        if (!fragment.StartsWith("<")) return null;

        // Remove leading < and trailing /> or >
        fragment = fragment.TrimStart('<').TrimEnd('>', '/').Trim();

        // Split tag name from attributes
        int firstSpace = fragment.IndexOf(' ');
        if (firstSpace < 0) return (fragment, new Dictionary<string, string>());

        string tag = fragment[..firstSpace];
        string rest = fragment[firstSpace..];

        var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (System.Text.RegularExpressions.Match m in
            System.Text.RegularExpressions.Regex.Matches(rest, @"(\w+)=""([^""]*)"""))
        {
            attrs[m.Groups[1].Value] = m.Groups[2].Value;
        }

        return (tag, attrs);
    }

    /// <summary>
    /// Applies byte-level JSON patches to the decompressed vanilla payload and returns the resulting XML.
    /// Used for CrimsonCamera-style patches where original/patched are short byte sequences, not full XML fragments.
    /// </summary>
    private static string ApplyBinaryJsonPatches(JsonElement patches, string gameDir)
    {
        var (vanillaBytes, _, _) = CameraMod.ReadStoredVanillaDecompressedPayloadForJson(gameDir);
        byte[] patched = (byte[])vanillaBytes.Clone();

        int applied = 0;
        int skipped = 0;
        foreach (var patch in patches.EnumerateArray())
        {
            if (!patch.TryGetProperty("changes", out var changes)) continue;
            foreach (var change in changes.EnumerateArray())
            {
                int offset = change.GetProperty("offset").GetInt32();
                byte[] original = Convert.FromHexString(change.GetProperty("original").GetString() ?? "");
                byte[] patchedData = Convert.FromHexString(change.GetProperty("patched").GetString() ?? "");

                if (original.Length == 0 || patchedData.Length == 0) continue;
                if (offset + original.Length > patched.Length || offset + patchedData.Length > patched.Length)
                {
                    skipped++;
                    continue;
                }

                bool matches = patched.AsSpan(offset, original.Length).SequenceEqual(original);
                if (matches)
                {
                    Array.Copy(patchedData, 0, patched, offset, patchedData.Length);
                    applied++;
                }
                else
                {
                    skipped++;
                }
            }
        }

        if (applied == 0)
            throw new InvalidOperationException(
                "No patches could be applied — the JSON patch was built for a different game version.\n\n" +
                "Try deleting the 0010 folder, verifying game files on Steam, then import again.");

        string xmlText = Encoding.UTF8.GetString(patched).TrimEnd('\0');
        return CameraMod.StripComments(xmlText);
    }

    private static ImportedPresetFingerprint BuildImportedPresetFingerprint(UltimateCameraMod.Paz.PazEntry entry, byte[] rawBytes)
    {
        return new ImportedPresetFingerprint
        {
            GameFile = entry.Path,
            SourceGroup = Path.GetFileName(Path.GetDirectoryName(entry.PazFile) ?? "0010") ?? "0010",
            CompSize = entry.CompSize,
            OrigSize = entry.OrigSize,
            ContentSha256 = Convert.ToHexString(SHA256.HashData(rawBytes)).ToLowerInvariant()
        };
    }

    private static ImportedPresetFingerprint? TryGetGameFingerprintForDir(string? gameDir)
    {
        if (string.IsNullOrWhiteSpace(gameDir))
            return null;

        var (entry, rawBytes) = CameraMod.ReadCameraEntryWithRawBytes(gameDir);
        return BuildImportedPresetFingerprint(entry, rawBytes);
    }

    private ImportedPresetFingerprint? TryGetCurrentGameFingerprint()
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
            return null;

        if (_cachedGameFingerprint != null
            && string.Equals(_cachedGameFingerprintDir, _gameDir, StringComparison.OrdinalIgnoreCase))
            return _cachedGameFingerprint;

        ImportedPresetFingerprint? fp = TryGetGameFingerprintForDir(_gameDir);
        _cachedGameFingerprintDir = _gameDir;
        _cachedGameFingerprint = fp;
        return fp;
    }

    private static bool FingerprintsMatch(ImportedPresetFingerprint? left, ImportedPresetFingerprint? right)
    {
        return left != null
            && right != null
            && left.GameFile == right.GameFile
            && left.SourceGroup == right.SourceGroup
            && left.CompSize == right.CompSize
            && left.OrigSize == right.OrigSize
            && left.ContentSha256 == right.ContentSha256;
    }

    private void LoadImportedPresetIntoSession(ImportedPreset preset)
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
            throw new InvalidOperationException("Game folder not set. v3 needs the current game to rebuild this preset safely.");

        _sessionIsRawImport = true;

        // Use the raw XML directly if available, otherwise rebuild from values
        string xml;
        if (!string.IsNullOrWhiteSpace(preset.RawXml))
        {
            // Strip BOM and comments — ParseXmlToRows needs clean game-format XML
            xml = preset.RawXml.TrimStart('\uFEFF');
            xml = CameraMod.StripComments(xml);
        }
        else
        {
            xml = BuildRebuiltXmlFromImportedPreset(preset);
            SetStatus(string.Format(L("Status_ImportedPresetNoXml"), preset.Name), "Warn");
        }
        RefreshUIFromSessionXml(xml);
        // Only sync Quick sliders for UCM-managed presets — raw imports disable Quick/Fine Tune
        if (!_sessionIsRawImport)
            TryApplyQuickSlidersFromSessionXml(xml);
        MarkImportedPresetAsBuilt(preset, refreshPresetSidebar: false);

        string authorOrSource = !string.IsNullOrWhiteSpace(preset.Author)
            ? preset.Author
            : preset.SourceDisplayName;

        SetLoadedPresetContext(preset.Name,
            ImportedPresetKindLabel(preset.SourceType),
            authorOrSource,
            BuildImportedPresetStatusText(preset),
            BuildImportedPresetSummaryText(preset),
            preset.Url);

        // Raw imports only support God Mode editing — auto-switch so the user lands there
        if (_sessionIsRawImport)
        {
            ApplyPresetEditingLockUi();
            SwitchEditorTab("expert", captureCurrent: false);
        }
    }

    private string BuildRebuiltXmlFromImportedPreset(ImportedPreset preset)
    {
        if (string.IsNullOrWhiteSpace(_gameDir))
            throw new InvalidOperationException("Game folder not set.");

        string vanillaXml = GetStrippedVanillaXmlForCurrentGame();
        return CameraMod.ApplyModifications(vanillaXml, BuildImportedPresetModSet(preset));
    }

    private static ModificationSet BuildImportedPresetModSet(ImportedPreset preset)
    {
        var mods = new Dictionary<string, Dictionary<string, (string, string)>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (fullKey, value) in preset.Values)
        {
            if (!CameraSessionState.TryParseFullKey(fullKey, out string modKey, out string attribute))
                continue;

            if (!mods.TryGetValue(modKey, out var attrs))
            {
                attrs = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
                mods[modKey] = attrs;
            }

            attrs[attribute] = ("SET", value);
        }

        return new ModificationSet { ElementMods = mods, FovValue = 0 };
    }

    private void MarkImportedPresetAsBuilt(ImportedPreset preset, bool refreshPresetSidebar = true)
    {
        var fingerprint = TryGetCurrentGameFingerprint();
        if (fingerprint == null)
            return;

        preset.LastBuiltAgainst = fingerprint;
        preset.LastBuiltAtUtc = DateTime.UtcNow;
        SaveImportedPreset(preset);
        _selectedImportedPreset = preset;
        if (refreshPresetSidebar)
        {
            RefreshPresetManagerLists();
            SelectImportedPreset(preset.Name);
        }

        UpdateImportedPresetDetails();
    }

    private void UpdateImportedPresetDetails()
    {
        if (_selectedImportedPreset == null)
        {
            UpdatePresetManagerDetails();
            return;
        }

        string status = BuildImportedPresetStatusText(_selectedImportedPreset);
        string summary = BuildImportedPresetSummaryText(_selectedImportedPreset);

        if (_selectedPresetManagerItem != null && _selectedPresetManagerItem.KindId == "imported")
        {
            _selectedPresetManagerItem.StatusText = status;
            _selectedPresetManagerItem.SummaryText = summary;
        }

        UpdatePresetManagerDetails();
    }

    private void SelectImportedPreset(string name)
    {
        var item = _presetManagerItems.FirstOrDefault(i =>
            i.KindId == "imported" && string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        if (item != null)
            SetSelectedPresetManagerItem(item, updateDetails: true);
    }

    private string ResolvePamtPathForImportedPaz(string pazPath)
    {
        string siblingPamt = Path.Combine(Path.GetDirectoryName(pazPath) ?? ".", "0.pamt");
        if (File.Exists(siblingPamt))
            return siblingPamt;

        if (!string.IsNullOrWhiteSpace(_gameDir))
        {
            string currentGamePamt = Path.Combine(_gameDir, "0010", "0.pamt");
            if (File.Exists(currentGamePamt))
                return currentGamePamt;
        }

        throw new InvalidOperationException(
            "No matching 0.pamt was found beside the selected 0.paz. Set your game folder so v3 can use the current 0010\\0.pamt as the archive index.");
    }

    private async void SaveImportedPresetFromXml(string sourceType, string sourceDisplayName, string? sourcePath,
        string xml, ImportedPresetFingerprint? importedFingerprint = null)
    {
        string name = await PromptForImportedPresetNameAsync(sourceDisplayName);
        if (string.IsNullOrWhiteSpace(name))
            return;

        string path = ImportedPresetPath(name);
        if (File.Exists(path))
        {
            if (!await ShowConfirmOverlayAsync(L("Dlg_OverwriteImportedPreset"), L("Msg_OverwriteExists"), L("Btn_Overwrite"), L("Btn_Cancel")))
                    return;
        }

        var preset = BuildImportedPreset(name, sourceType, sourceDisplayName, sourcePath, xml, importedFingerprint);
        SaveImportedPreset(preset);
        RefreshPresetManagerLists();
        SelectImportedPreset(preset.Name);
        QueueSavedToast(string.Format(L("Status_ImportedPresetSaved"), preset.Name, preset.Values.Count));
        SetStatus(string.Format(L("Status_ImportedPresetSaved"), preset.Name, preset.Values.Count), "Success");
    }

    private async Task<string> PromptForImportedPresetNameAsync(string suggestedName)
    {
        string initial = SanitizeFileStem(Path.GetFileNameWithoutExtension(suggestedName));
        string? response = await ShowInputOverlayAsync(L("Dlg_SaveImportedPreset"), L("Dlg_EnterImportedPresetName"), initial);
        if (string.IsNullOrWhiteSpace(response))
            return "";

        string name = SanitizeFileStem(response);
        if (name.Length > 60)
            name = name[..60];
        return name;
    }

    private ImportedPreset? RequireSelectedImportedPreset()
    {
        if (_selectedPresetManagerItem == null || _selectedPresetManagerItem.KindId != "imported")
        {
            SetStatus(L("Status_SelectImportedFirst"), "TextSecondary");
            return null;
        }

        string name = _selectedPresetManagerItem.Name;
        _selectedImportedPreset = LoadImportedPreset(name);
        if (_selectedImportedPreset == null)
            SetStatus(string.Format(L("Status_FailedLoadImported"), name), "Error");
        return _selectedImportedPreset;
    }

    private void RefreshImportedPresetCombo()
    {
        RefreshPresetManagerLists();
    }

    private static string ImportedPresetPath(string name) =>
        Path.Combine(ImportedPresetsDir, $"{SanitizeFileStem(name)}.json");

    private static string ImportedPresetKindLabel(string sourceType)
    {
        if (sourceType.Equals("paz", StringComparison.OrdinalIgnoreCase))
            return L("Label_KindImportedPaz");
        if (sourceType.Equals("mod", StringComparison.OrdinalIgnoreCase))
            return L("Label_KindModPackage");
        return L("Label_KindImportedXml");
    }

    private bool TryBuildImportedPresetManagerItem(string filePath, string fileStem,
        ImportedPresetFingerprint? currentGameFp, out PresetManagerItem? item)
    {
        item = null;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
            var root = doc.RootElement;

            string displayName = fileStem;
            if (root.TryGetProperty("Name", out var n) && n.ValueKind == JsonValueKind.String)
            {
                string? s = n.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    displayName = s;
            }

            string sourceType = "xml";
            if (root.TryGetProperty("SourceType", out var st) && st.ValueKind == JsonValueKind.String)
            {
                string? t = st.GetString();
                if (!string.IsNullOrWhiteSpace(t))
                    sourceType = t;
            }

            string sourceDisplayName = "";
            if (root.TryGetProperty("SourceDisplayName", out var sd) && sd.ValueKind == JsonValueKind.String)
                sourceDisplayName = sd.GetString() ?? "";

            int valueCount = 0;
            if (root.TryGetProperty("Values", out var vals) && vals.ValueKind == JsonValueKind.Object)
            {
                foreach (var _ in vals.EnumerateObject())
                    valueCount++;
            }

            string author = "";
            if (root.TryGetProperty("Author", out var au) && au.ValueKind == JsonValueKind.String)
                author = au.GetString() ?? "";

            string description = "";
            if (root.TryGetProperty("Description", out var desc) && desc.ValueKind == JsonValueKind.String)
                description = desc.GetString() ?? "";

            ImportedPresetFingerprint? lastBuilt = ReadLastBuiltAgainstFromJson(root);
            string statusText = BuildImportedPresetStatusTextFromMetadata(lastBuilt, currentGameFp);

            bool locked = false;
            if (root.TryGetProperty("Locked", out var lk))
                locked = lk.ValueKind == JsonValueKind.True;
            else if (root.TryGetProperty("locked", out var lk2))
                locked = lk2.ValueKind == JsonValueKind.True;

            // Build clean summary — just description + URL if available. Author shown separately in banner.
            string summary;
            if (!string.IsNullOrWhiteSpace(description))
            {
                string shortDesc = description.Replace("\n", " ").Trim();
                if (shortDesc.Length > 200) shortDesc = shortDesc[..197] + "...";
                summary = shortDesc;
            }
            else
            {
                summary = string.Format(L("Label_ImportedFromSource"), sourceType.ToUpperInvariant(), sourceDisplayName);
            }

            string sidebarSource = string.IsNullOrWhiteSpace(author)
                ? sourceDisplayName
                : author;

            item = new PresetManagerItem
            {
                Name = fileStem,
                KindId = "imported",
                KindLabel = ImportedPresetKindLabel(sourceType),
                SourceLabel = sidebarSource,
                StatusText = statusText,
                SummaryText = summary,
                FilePath = filePath,
                CanRebuild = true,
                IsLocked = locked
            };
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string BuildImportedPresetStatusText(ImportedPreset preset) =>
        BuildImportedPresetStatusTextFromMetadata(preset.LastBuiltAgainst, TryGetCurrentGameFingerprint());

    private static string BuildImportedPresetStatusTextFromMetadata(
        ImportedPresetFingerprint? lastBuilt, ImportedPresetFingerprint? currentFingerprint) =>
        lastBuilt == null
            ? L("Status_ImportedNotRebuilt")
            : currentFingerprint == null
                ? L("Status_ImportedReadyNoGameFolder")
                : FingerprintsMatch(lastBuilt, currentFingerprint)
                    ? L("Status_ImportedReadyRebuilt")
                    : L("Status_ImportedNeedsRebuild");

    private static ImportedPresetFingerprint? ReadLastBuiltAgainstFromJson(JsonElement root)
    {
        if (!root.TryGetProperty("LastBuiltAgainst", out var lb) || lb.ValueKind != JsonValueKind.Object)
            return null;

        static string ReadStr(JsonElement obj, string prop) =>
            obj.TryGetProperty(prop, out var p) && p.ValueKind == JsonValueKind.String
                ? p.GetString() ?? ""
                : "";

        static int ReadInt(JsonElement obj, string prop)
        {
            if (!obj.TryGetProperty(prop, out var p))
                return 0;
            return p.ValueKind switch
            {
                JsonValueKind.Number => p.TryGetInt32(out var i) ? i : 0,
                JsonValueKind.String => int.TryParse(p.GetString(), out var j) ? j : 0,
                _ => 0
            };
        }

        var fp = new ImportedPresetFingerprint
        {
            GameFile = ReadStr(lb, "GameFile"),
            SourceGroup = ReadStr(lb, "SourceGroup"),
            CompSize = ReadInt(lb, "CompSize"),
            OrigSize = ReadInt(lb, "OrigSize"),
            ContentSha256 = ReadStr(lb, "ContentSha256")
        };

        if (string.IsNullOrEmpty(fp.GameFile) && fp.CompSize == 0 && fp.OrigSize == 0
            && string.IsNullOrEmpty(fp.ContentSha256))
            return null;

        return fp;
    }

    private string BuildImportedPresetSummaryText(ImportedPreset preset)
    {
        if (!string.IsNullOrWhiteSpace(preset.Description))
            return preset.Description.Replace("\n", " ").Trim();
        return string.Format(L("Label_ImportedFromSource"), preset.SourceType.ToUpperInvariant(), preset.SourceDisplayName);
    }

    private async void OnImportedPresetDelete(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null || item.KindId != "imported")
        {
            SetStatus(L("Status_SelectImportedFirst"), "TextSecondary");
            return;
        }

        string name = item.Name;
        if (!await ShowConfirmOverlayAsync(L("Dlg_DeleteImportedPreset"), L("Msg_ConfirmDelete"), L("Btn_Yes"), L("Btn_Cancel")))
                    return;

        try
        {
            string path = ImportedPresetPath(name);
            if (File.Exists(path))
                File.Delete(path);

            _selectedImportedPreset = null;
            RefreshPresetManagerLists(preserveSelection: false);
            QueueSavedToast(string.Format(L("Status_ImportedPresetDeleted"), name));
            SetStatus(string.Format(L("Status_ImportedPresetDeleted"), name), "Success");
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_DeleteFailed"), ex.Message), "Error");
        }
    }

    private void OnImportedPresetLoadIntoGodMode(object sender, RoutedEventArgs e)
    {
        var preset = RequireSelectedImportedPreset();
        if (preset == null)
            return;

        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus(L("Msg_GameFolderNotSetForImport"), "Warn");
            return;
        }

        try
        {
            // Use raw XML directly if available — don't rebuild through CameraRules
            if (!string.IsNullOrWhiteSpace(preset.RawXml))
            {
                _sessionXml = CameraMod.StripComments(preset.RawXml.TrimStart('\uFEFF'));
                _sessionIsRawImport = true;
            }
            else
            {
                _sessionXml = BuildRebuiltXmlFromImportedPreset(preset);
            }
            SwitchAppMode("expert", captureCurrent: false);
            MarkImportedPresetAsBuilt(preset, refreshPresetSidebar: false);
            ApplyPresetEditingLockUi();
            QueueSavedToast(L("Toast_ImportedPresetLoaded"));
            SetStatus(string.Format(L("Status_LoadedIntoGodMode"), preset.Name), "Success");
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_LoadFailed"), ex.Message), "Error");
        }
    }

    private void OnImportedPresetGenerateJson(object sender, RoutedEventArgs e)
    {
        var preset = RequireSelectedImportedPreset();
        if (preset == null)
            return;

        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus(L("Msg_GameFolderNotSetForImport"), "Warn");
            return;
        }

        string rebuiltXml;
        try
        {
            // Use raw XML directly if available — don't rebuild through CameraRules
            if (!string.IsNullOrWhiteSpace(preset.RawXml))
            {
                rebuiltXml = CameraMod.StripComments(preset.RawXml.TrimStart('\uFEFF'));
                _sessionIsRawImport = true;
            }
            else
            {
                rebuiltXml = BuildRebuiltXmlFromImportedPreset(preset);
            }
            _sessionXml = rebuiltXml;
            MarkImportedPresetAsBuilt(preset, refreshPresetSidebar: false);
        }
        catch (Exception ex)
        {
            SetStatus(string.Format(L("Status_LoadFailed"), ex.Message), "Error");
            return;
        }

        // Use rebuilt XML directly — CaptureSessionXml() would rebuild from God Mode UI and can diverge.
        var ctrl = new ExportJsonDialog(_gameDir, () => rebuiltXml,
            getSettingsPayload: () => BuildCurrentPresetSettingsPayload());
        ctrl.OnCloseRequested = () => CloseOverlay();
        _ = ShowOverlayAsync(ctrl, width: 720, height: 750);
    }

}
