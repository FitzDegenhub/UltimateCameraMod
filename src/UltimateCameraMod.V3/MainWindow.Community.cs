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
using UltimateCameraMod.V3.Models;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.V3;

public partial class MainWindow : Window
{
    private void OnBrowsePresetCatalog(object sender, RoutedEventArgs e)
    {
        // Determine which catalog to browse based on the group header that was clicked
        string groupName = "";
        if (sender is System.Windows.Controls.Button btn && btn.DataContext is CollectionViewGroup group)
            groupName = group.Name?.ToString() ?? "";

        if (groupName == "UCM presets")
        {
            var ctrl = new CommunityBrowserDialog(UcmPresetsDir, () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(_gameDir))
                        GenerateBuiltInPresets();
                    RefreshPresetManagerLists(preserveSelection: true);
                });
            },
            catalogUrl: UcmPresetsCatalogUrl,
            rawBaseUrl: UcmPresetsRawBaseUrl,
            title: "UCM Presets",
            needsSessionXmlBake: true);
            ctrl.OnCloseRequested = () => CloseOverlay();
            _ = ShowOverlayAsync(ctrl, width: 660, height: 700);
        }
        else
        {
            var ctrl = new CommunityBrowserDialog(CommunityPresetsDir, () =>
            {
                Dispatcher.Invoke(() => RefreshPresetManagerLists(preserveSelection: true));
            },
            title: "Community Presets");
            ctrl.OnCloseRequested = () => CloseOverlay();
            _ = ShowOverlayAsync(ctrl, width: 660, height: 700);
        }
    }

    // Keep for any direct calls
    private void OnBrowseCommunity(object sender, RoutedEventArgs e) => OnBrowsePresetCatalog(sender, e);

    private async void CheckGitHubVersion()
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("UltimateCameraMod/" + Ver);
            http.Timeout = TimeSpan.FromSeconds(8);
            string json = await http.GetStringAsync(
                "https://api.github.com/repos/FitzDegenhub/UltimateCameraMod/releases/latest");
            using var doc = JsonDocument.Parse(json);
            string tag = doc.RootElement.GetProperty("tag_name").GetString() ?? "";
            string latest = tag.TrimStart('v', 'V');

            string pad(string v) => v.Contains('.') && v.Split('.').Length == 2 ? v + ".0" : v;
            // Strip pre-release suffix before numeric compare ("3-beta" -> "3", "3.0-beta" -> "3.0")
            string numericLatest = latest.Split('-')[0];
            string numericVer   = Ver.Split('-')[0];
            bool isOutdated = !string.IsNullOrEmpty(latest)
                && Version.TryParse(pad(numericLatest), out var remote)
                && Version.TryParse(pad(numericVer), out var local)
                && remote > local;

            Dispatcher.Invoke(() =>
            {
                if (isOutdated)
                {
                    VersionDot.Fill = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                    VersionStatus.Text = $"v{latest} available";
                    VersionStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                    VersionUpdateBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    VersionDot.Fill = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                    VersionStatus.Text = "up to date";
                    VersionStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                }
            });
        }
        catch
        {
            Dispatcher.Invoke(() =>
            {
                VersionDot.Fill = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                VersionStatus.Text = "offline";
                VersionStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            });
        }
    }

    private void OnUpdateNexusClick(object s, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(NexusUrl) { UseShellExecute = true });
    private void OnUpdateGitHubClick(object s, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(GitHubUrl + "/releases/latest") { UseShellExecute = true });

    // -- UCM preset fetch (background, every launch) -------------------------

    /// <summary>
    /// Fetches the UCM preset catalog from GitHub and downloads any new or stale preset
    /// definition files into ucm_presets/. The session_xml is not stored in the repo —
    /// GenerateBuiltInPresets() bakes it in once the game directory is known.
    /// </summary>
    /// <summary>
    /// Downloads any UCM presets from the catalog that don't exist locally.
    /// Presets are complete files (with session_xml) — no baking needed.
    /// </summary>
    private async void FetchUcmPresetsAsync()
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("UltimateCameraMod/" + Ver);
            http.Timeout = TimeSpan.FromSeconds(10);

            string catalogJson = await http.GetStringAsync(UcmPresetsCatalogUrl);
            using var doc = JsonDocument.Parse(catalogJson);
            if (!doc.RootElement.TryGetProperty("presets", out var presetsArr)
                || presetsArr.ValueKind != JsonValueKind.Array)
                return;

            bool anyNew = false;
            foreach (var el in presetsArr.EnumerateArray())
            {
                string file = el.TryGetProperty("file", out var fEl) ? fEl.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(file)) continue;

                string localPath = Path.Combine(UcmPresetsDir, file);

                // Only download presets that don't exist locally.
                // SHA mismatches on existing files are handled by CheckUcmPresetUpdatesAsync (⟳ icon).
                if (File.Exists(localPath)) continue;

                try
                {
                    string downloadUrl = UcmPresetsRawBaseUrl + Uri.EscapeDataString(file);
                    byte[] rawBytes = await http.GetByteArrayAsync(downloadUrl);

                    // Write the complete preset file as-is
                    File.WriteAllBytes(localPath, rawBytes);

                    // Track the download hash in sidecar for update detection
                    string rawSha = Convert.ToHexString(
                        System.Security.Cryptography.SHA256.HashData(rawBytes)).ToLowerInvariant();
                    UpdateCatalogStateEntry(file, rawSha);
                    anyNew = true;
                }
                catch { /* skip individual download failures */ }
            }

            if (anyNew)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    RefreshPresetManagerLists(preserveSelection: true);
                });
            }
        }
        catch { /* network unavailable — silently skip */ }
    }

    private void RefreshGameUpdateNotice()
    {
        try
        {
            if (!IsLoaded)
                return;

            if (string.IsNullOrWhiteSpace(_gameDir))
            {
                if (IsLoaded) GameUpdateStrip.Visibility = Visibility.Collapsed;
                return;
            }

            string backupsDir = Path.Combine(ExeDir, "backups");
            var ev = GameInstallBaselineTracker.Evaluate(ExeDir, Ver, _gameDir, _detectedPlatform, backupsDir);
            if (!ev.ShowWarning)
            {
                _gameUpdateAutoBackupDispatched = false;
                _gameUpdatePostRefreshNote = null;
                if (!_gameUpdateNoticeSessionDismissed)
                    GameUpdateStrip.Visibility = Visibility.Collapsed;
                return;
            }

            // Drift vs last Install baseline: re-snapshot camera from live 0.paz once (even if the strip is dismissed).
            if (!_gameUpdateAutoBackupDispatched)
            {
                _gameUpdateAutoBackupDispatched = true;
                string gameDir = _gameDir;
                Task.Run(() =>
                {
                    try
                    {
                        CameraMod.RefreshVanillaBackupFromLivePaz(gameDir, _ => { });
                        Dispatcher.BeginInvoke(() =>
                        {
                            _gameUpdatePostRefreshNote =
                                "Camera backup was refreshed from your current game files. Use Install when you are ready to re-apply your preset.";
                            RefreshGameUpdateNotice();
                        }, DispatcherPriority.Background);
                    }
                    catch
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            _gameUpdatePostRefreshNote =
                                "Could not refresh camera backup automatically. If the camera is still modded, verify game files in Steam and restart UCM.";
                            RefreshGameUpdateNotice();
                        }, DispatcherPriority.Background);
                    }
                });
            }

            if (_gameUpdateNoticeSessionDismissed)
                return;

            string message = string.IsNullOrEmpty(_gameUpdatePostRefreshNote)
                ? ev.Message
                : ev.Message + "\n\n" + _gameUpdatePostRefreshNote;
            ShowGameUpdateOverlay(message);
        }
        catch
        {
            if (IsLoaded) GameUpdateStrip.Visibility = Visibility.Collapsed;
        }
    }

    private void OnGameUpdateDismissClick(object sender, RoutedEventArgs e)
    {
        _gameUpdateNoticeSessionDismissed = true;
        CloseOverlay();
    }

    private void OnGameUpdateSnoozeClick(object sender, RoutedEventArgs e)
    {
        GameInstallBaselineTracker.SetSnooze(ExeDir, TimeSpan.FromDays(7));
        RefreshGameUpdateNotice();
    }

    // ── Preset update detection ──────────────────────────────────────

    private async void CheckUcmPresetUpdatesAsync()
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("UltimateCameraMod/" + Ver);
            http.Timeout = TimeSpan.FromSeconds(8);

            string json = await http.GetStringAsync(UcmPresetsCatalogUrl);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("presets", out var presetsArr)
                || presetsArr.ValueKind != JsonValueKind.Array)
                return;

            // Build catalog: filename → sha256
            var catalogHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var el in presetsArr.EnumerateArray())
            {
                string file = el.TryGetProperty("file", out var fEl) ? fEl.GetString() ?? "" : "";
                string sha = el.TryGetProperty("sha256", out var shaEl) ? shaEl.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(sha))
                    catalogHashes[file] = sha;
            }

            // Read sidecar: last known remote SHA per file (written by FetchUcmPresetsAsync / OnPresetUpdateClick).
            var localState = ReadCatalogState();

            await Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in _presetManagerItems)
                {
                    if (item.KindId is not ("style" or "default") || item.IsPlaceholder) continue;

                    string localPath = item.FilePath;
                    if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath)) continue;

                    string fileName = Path.GetFileName(localPath);

                    // Only flag updates for presets that have a sidecar entry (i.e., were downloaded through the app).
                    // Presets without a sidecar entry are manually placed or pre-installed — don't flash them.
                    if (!localState.TryGetValue(fileName, out string? storedSha) || string.IsNullOrEmpty(storedSha))
                        continue;

                    if (catalogHashes.TryGetValue(fileName, out string? catalogSha)
                        && !string.Equals(storedSha, catalogSha, StringComparison.OrdinalIgnoreCase))
                    {
                        item.HasUpdate = true;
                    }
                }
            });
        }
        catch { /* network unavailable — silently skip update check */ }
    }

    private async void CheckCommunityPresetUpdatesAsync()
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("UltimateCameraMod/" + Ver);
            http.Timeout = TimeSpan.FromSeconds(8);

            string json = await http.GetStringAsync(
                "https://raw.githubusercontent.com/FitzDegenhub/ucm-community-presets/main/catalog.json");
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("presets", out var presetsArr)
                || presetsArr.ValueKind != JsonValueKind.Array)
                return;

            // Build catalog: id → sha256
            var catalogHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var el in presetsArr.EnumerateArray())
            {
                string id = el.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : "";
                string sha = el.TryGetProperty("sha256", out var shaEl) ? shaEl.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(sha))
                    catalogHashes[id] = sha;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in _presetManagerItems)
                {
                    if (item.KindId != "community" || item.IsPlaceholder) continue;
                    item.HasUpdate = false;
                }

                foreach (var item in _presetManagerItems)
                {
                    if (item.KindId != "community" || item.IsPlaceholder) continue;
                    string localPath = item.FilePath;
                    if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath)) continue;

                    // Compute local file SHA256
                    string localSha;
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(localPath);
                        localSha = Convert.ToHexString(
                            System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
                    }
                    catch { continue; }

                    string presetId = Path.GetFileNameWithoutExtension(localPath);
                    if (catalogHashes.TryGetValue(presetId, out string? catalogSha)
                        && !string.Equals(localSha, catalogSha, StringComparison.OrdinalIgnoreCase))
                    {
                        item.HasUpdate = true;
                    }
                }
            });
        }
        catch { /* network unavailable — silently skip */ }
    }

    private async void OnPresetUpdateClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not PresetManagerItem item) return;
        if (!item.HasUpdate) return;

        // Ask if they want to duplicate first
        var result = MessageBox.Show(
            $"An update is available for '{item.Name}'.\n\nWould you like to save a copy of the current version to My Presets before updating?",
            "Preset Update",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Cancel) return;

        if (result == MessageBoxResult.Yes)
        {
            // Duplicate to My Presets
            try
            {
                string dupName = $"{item.Name}_old";
                string dupPath = Path.Combine(MyPresetsDir, $"{SanitizeFileStem(dupName)}.ucmpreset");
                if (File.Exists(item.FilePath))
                {
                    string fileJson = File.ReadAllText(item.FilePath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(fileJson) ?? new();
                    dict["name"] = dupName;
                    dict["kind"] = "user";
                    dict["locked"] = false;
                    File.WriteAllText(dupPath, JsonSerializer.Serialize(dict, PresetFileJsonOptions));
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to duplicate: {ex.Message}", "Error");
                return;
            }
        }

        // Download the update
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("UltimateCameraMod/" + Ver);
            http.Timeout = TimeSpan.FromSeconds(10);

            string fileName = Path.GetFileName(item.FilePath);
            bool isCommunity = item.KindId == "community";
            string downloadUrl;
            if (isCommunity)
                downloadUrl = "https://raw.githubusercontent.com/FitzDegenhub/ucm-community-presets/main/presets/"
                    + Uri.EscapeDataString(fileName);
            else
                downloadUrl = UcmPresetsRawBaseUrl + Uri.EscapeDataString(fileName);

            byte[] rawBytes = await http.GetByteArrayAsync(downloadUrl);

            if (isCommunity)
            {
                // Community presets: rebuild with metadata so url, author, description are preserved
                string content = Encoding.UTF8.GetString(rawBytes);
                using var dlDoc = JsonDocument.Parse(content);
                var root = dlDoc.RootElement;
                string sessionXml = root.TryGetProperty("session_xml", out var sxEl) ? sxEl.GetString() ?? "" : "";
                string presetName = root.TryGetProperty("name", out var nEl) ? nEl.GetString() ?? item.Name : item.Name;
                string presetAuthor = root.TryGetProperty("author", out var aEl) ? aEl.GetString() ?? "" : "";
                string presetDesc = root.TryGetProperty("description", out var dEl) ? dEl.GetString() ?? "" : "";
                string presetUrl = root.TryGetProperty("url", out var uEl) ? uEl.GetString() ?? "" : "";
                // Preserve existing url if the downloaded file doesn't have one
                if (string.IsNullOrEmpty(presetUrl)) presetUrl = item.Url;

                object? settingsObj = null;
                if (root.TryGetProperty("settings", out var settingsEl))
                    settingsObj = JsonSerializer.Deserialize<JsonElement>(settingsEl.GetRawText());

                var rebuilt = new Dictionary<string, object?>
                {
                    ["name"] = presetName,
                    ["author"] = presetAuthor,
                    ["url"] = presetUrl,
                    ["description"] = presetDesc,
                    ["kind"] = "community",
                    ["locked"] = true,
                    ["settings"] = settingsObj,
                    ["session_xml"] = sessionXml,
                };
                File.WriteAllText(item.FilePath, JsonSerializer.Serialize(rebuilt, PresetFileJsonOptions));
            }
            else
            {
                // UCM presets: write raw definition, baking will add session_xml
                File.WriteAllBytes(item.FilePath, rawBytes);
                string rawSha = Convert.ToHexString(
                    System.Security.Cryptography.SHA256.HashData(rawBytes)).ToLowerInvariant();
                UpdateCatalogStateEntry(fileName, rawSha);
            }

            await Dispatcher.InvokeAsync(() =>
            {
                // Clear the active key so the preset reloads from disk after list refresh
                _activePickerKey = null;
                RefreshPresetManagerLists(preserveSelection: true);

                // Re-activate the selected preset to refresh UI with updated values
                if (_selectedPresetManagerItem != null)
                    ActivatePickerFromSelection(_selectedPresetManagerItem, skipCapture: true);

                // Re-check both catalogs so update icons on other presets aren't lost
                CheckUcmPresetUpdatesAsync();
                CheckCommunityPresetUpdatesAsync();
            });

            QueueSavedToast($"Updated '{item.Name}'");
            SetStatus($"Preset '{item.Name}' updated.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Update failed: {ex.Message}", "Error");
        }
    }

}
