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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Controls;
// using Avalonia.Data;
using Avalonia.Input;

using Avalonia.Media;
using Avalonia.Threading;
using UltimateCameraMod.Avalonia.Controls;
using UltimateCameraMod.Avalonia.Models;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.Avalonia;

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
            case "paz": ImportFromPaz(); break;
            case "ucmpreset": ImportUcmPreset(); break;
        }
    }

    private async void ImportModManagerPackage()
    {
        using var folderDlg = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select the mod folder (containing manifest.json)",
            UseDescriptionForTitle = true
        };
        if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

        string folder = folderDlg.SelectedPath;
        try
        {
            string manifestPath = Path.Combine(folder, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                SetStatus("No manifest.json found in that folder.", "Error");
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
            string fullFilesDir = Path.Combine(folder, filesDir);
            string? xmlPath = Directory.GetFiles(fullFilesDir, "playercamerapreset.xml", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (xmlPath == null)
            {
                SetStatus("No playercamerapreset.xml found in the mod package.", "Error");
                return;
            }

            string xml = File.ReadAllText(xmlPath);
            string safeStem = new string(title.Trim().Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            if (string.IsNullOrWhiteSpace(safeStem))
                safeStem = "Imported_Mod";

            string shortDesc = rawDesc.Split("\n\n")[0].Replace("\n", " ").Trim();
            if (shortDesc.Length > 200) shortDesc = shortDesc[..197] + "...";

            var metaDlg = await ShowImportMetadataOverlayAsync(
                $"Importing mod package: {Path.GetFileName(folder)}",
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
                if (!await ShowConfirmOverlayAsync("Import Preset", "A preset with this name already exists. Overwrite?", "Overwrite", "Cancel"))
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
            QueueSavedToast($"Imported '{imported.Name}'");
            SetStatus($"Imported mod package as '{imported.Name}'.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Import failed: {ex.Message}", "Error");
        }
    }

    private async void ImportRawXml()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import playercamerapreset.xml",
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
                $"Importing XML: {Path.GetFileName(ofd.FileName)}",
                baseName);
            if (metaDlg == null) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                if (!await ShowConfirmOverlayAsync("Import XML", "A preset with this name already exists. Overwrite?", "Overwrite", "Cancel"))
                    return;
            }

            var preset = BuildImportedPreset(chosenName, "xml", Path.GetFileName(ofd.FileName), ofd.FileName, xml,
                null, metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(preset);
            RefreshPresetManagerLists();
            SelectImportedPreset(preset.Name);
            QueueSavedToast("Imported preset saved");
            SetStatus($"Imported '{preset.Name}' with {preset.Values.Count} values.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"XML import failed: {ex.Message}", "Error");
        }
    }

    private async void ImportFromPaz()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import 0.paz — extract camera XML",
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
            SetStatus($"0.paz import failed: {ex.Message}", "Error");
            return;
        }

        SetGlobalBusy(true, "Decrypting 0.paz\u2026");
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
                _ = ShowAlertOverlayAsync("PAZ Import Failed",
                    $"{decEx.Message}\n\n" +
                    "This usually means the PAZ file is from a different game version than your current install. " +
                    "The archive index (0.pamt) doesn't match the PAZ file structure.\n\n" +
                    "TO FIX:\n" +
                    "Place the matching 0.pamt file in the same folder as the 0.paz you're importing. " +
                    "Both files must be from the same game version.",
                    isError: true);
                return;
            }
            SetGlobalBusy(false);

            var metaDlg = await ShowImportMetadataOverlayAsync(
                $"Importing PAZ: {Path.GetFileName(pazPath)}",
                "Imported Camera");
            if (metaDlg == null) return;

            string chosenName = SanitizeFileStem(metaDlg.PresetName);
            if (chosenName.Length > 60) chosenName = chosenName[..60];

            string importPath = ImportedPresetPath(chosenName);
            if (File.Exists(importPath))
            {
                if (!await ShowConfirmOverlayAsync("Import 0.paz", "A preset with this name already exists. Overwrite?", "Overwrite", "Cancel"))
                    return;
            }

            var imported = BuildImportedPreset(chosenName, "paz", Path.GetFileName(pazPath), pazPath, xml, null,
                metaDlg.PresetAuthor, metaDlg.PresetDescription, metaDlg.PresetUrl);
            SaveImportedPreset(imported);
            RefreshPresetManagerLists(preserveSelection: false);
            SelectImportedPreset(SanitizeFileStem(imported.Name));
            QueueSavedToast($"Imported '{imported.Name}'");
            SetStatus($"Imported 0.paz as '{imported.Name}'.", "Success");
        }
        catch (Exception ex)
        {
            SetGlobalBusy(false);
            SetStatus($"0.paz import failed: {ex.Message}", "Error");
        }
    }

    private async void ImportUcmPreset()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import UCM Preset",
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
                SetStatus("This file doesn't appear to be a UCM preset (no session_xml found).", "Error");
                return;
            }

            string name = root.TryGetProperty("name", out var nv) ? nv.GetString() ?? "" : Path.GetFileNameWithoutExtension(ofd.FileName);
            string safeName = new string(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            string destPath = Path.Combine(MyPresetsDir, $"{safeName}.ucmpreset");

            if (File.Exists(destPath))
            {
                if (!await ShowConfirmOverlayAsync("Import UCM Preset", "A preset with this name already exists. Overwrite?", "Overwrite", "Cancel"))
                    return;
            }

            File.Copy(ofd.FileName, destPath, true);
            RefreshPresetManagerLists(preserveSelection: false);
            QueueSavedToast($"Imported '{name}'");
            SetStatus($"Imported UCM preset '{name}'.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Import failed: {ex.Message}", "Error");
        }
    }

    private void OnImportedPresetImportXml(object sender, RoutedEventArgs e)
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import playercamerapreset.xml as saved preset",
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
            SetStatus($"XML import failed: {ex.Message}", "Error");
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
            SetStatus($"Imported preset '{preset.Name}' had no embedded XML; rebuilt from saved settings.", "Warn");
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
            if (!await ShowConfirmOverlayAsync("Overwrite Imported Preset", "A preset with this name already exists. Overwrite?", "Overwrite", "Cancel"))
                    return;
        }

        var preset = BuildImportedPreset(name, sourceType, sourceDisplayName, sourcePath, xml, importedFingerprint);
        SaveImportedPreset(preset);
        RefreshPresetManagerLists();
        SelectImportedPreset(preset.Name);
        QueueSavedToast("Imported preset saved");
        SetStatus($"Imported preset '{preset.Name}' saved with {preset.Values.Count} values.", "Success");
    }

    private async Task<string> PromptForImportedPresetNameAsync(string suggestedName)
    {
        string initial = SanitizeFileStem(Path.GetFileNameWithoutExtension(suggestedName));
        string? response = await ShowInputOverlayAsync("Save Imported Preset", "Enter a name for this imported preset:", initial);
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
            SetStatus("Select an imported preset first.", "TextSecondary");
            return null;
        }

        string name = _selectedPresetManagerItem.Name;
        _selectedImportedPreset = LoadImportedPreset(name);
        if (_selectedImportedPreset == null)
            SetStatus($"Failed to load imported preset '{name}'.", "Error");
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
            return "Imported 0.paz";
        if (sourceType.Equals("mod", StringComparison.OrdinalIgnoreCase))
            return "Mod package";
        return "Imported XML";
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
                summary = $"Imported from {sourceType.ToUpperInvariant()} ({sourceDisplayName})";
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
            ? "Imported only - not yet rebuilt for this game."
            : currentFingerprint == null
                ? "Saved preset ready - set a game folder to check rebuild status."
                : FingerprintsMatch(lastBuilt, currentFingerprint)
                    ? "Ready - already rebuilt for this game version."
                    : "Needs rebuild - current game version differs from the last build.";

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
        return $"Imported from {preset.SourceType.ToUpperInvariant()} ({preset.SourceDisplayName})";
    }

    private async void OnImportedPresetDelete(object sender, RoutedEventArgs e)
    {
        var item = RequireSelectedPresetManagerItem();
        if (item == null || item.KindId != "imported")
        {
            SetStatus("Select an imported preset first.", "TextSecondary");
            return;
        }

        string name = item.Name;
        if (!await ShowConfirmOverlayAsync("Delete Imported Preset", "Are you sure? This cannot be undone.", "Yes", "Cancel"))
                    return;

        try
        {
            string path = ImportedPresetPath(name);
            if (File.Exists(path))
                File.Delete(path);

            _selectedImportedPreset = null;
            RefreshPresetManagerLists(preserveSelection: false);
            QueueSavedToast("Imported preset deleted");
            SetStatus($"Imported preset '{name}' deleted.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Delete failed: {ex.Message}", "Error");
        }
    }

    private void OnImportedPresetLoadIntoGodMode(object sender, RoutedEventArgs e)
    {
        var preset = RequireSelectedImportedPreset();
        if (preset == null)
            return;

        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus("Game folder not set. v3 needs the current game to rebuild this preset safely.", "Warn");
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
            QueueSavedToast("Imported preset loaded");
            SetStatus($"Loaded imported preset '{preset.Name}' into God Mode.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Load failed: {ex.Message}", "Error");
        }
    }

    private void OnImportedPresetGenerateJson(object sender, RoutedEventArgs e)
    {
        var preset = RequireSelectedImportedPreset();
        if (preset == null)
            return;

        if (string.IsNullOrWhiteSpace(_gameDir))
        {
            SetStatus("Game folder not set. v3 needs the current game to rebuild this preset safely.", "Warn");
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
            SetStatus($"Rebuild failed: {ex.Message}", "Error");
            return;
        }

        // Use rebuilt XML directly — CaptureSessionXml() would rebuild from God Mode UI and can diverge.
        var ctrl = new ExportJsonDialog(_gameDir, () => rebuiltXml,
            getSettingsPayload: () => BuildCurrentPresetSettingsPayload());
        ctrl.OnCloseRequested = () => CloseOverlay();
        _ = ShowOverlayAsync(ctrl, width: 720, height: 750);
    }

}
