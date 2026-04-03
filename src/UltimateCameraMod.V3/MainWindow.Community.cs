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
            var dlg = new CommunityBrowserDialog(UcmPresetsDir, () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(_gameDir))
                        GenerateBuiltInPresets(); // Bake session_xml into downloaded presets
                    RefreshPresetManagerLists(preserveSelection: true);
                });
            },
            catalogUrl: UcmPresetsCatalogUrl,
            rawBaseUrl: UcmPresetsRawBaseUrl,
            title: "UCM Presets",
            needsSessionXmlBake: true)
            {
                Owner = this
            };
            dlg.ShowDialog();
        }
        else
        {
            var dlg = new CommunityBrowserDialog(CommunityPresetsDir, () =>
            {
                Dispatcher.Invoke(() => RefreshPresetManagerLists(preserveSelection: true));
            },
            title: "Community Presets")
            {
                Owner = this
            };
            dlg.ShowDialog();
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
                && !string.Equals(latest.Trim(), Ver.Trim(), StringComparison.OrdinalIgnoreCase)
                && Version.TryParse(pad(numericLatest), out var remote)
                && Version.TryParse(pad(numericVer), out var local)
                && remote >= local;

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
                int catalogRev = el.TryGetProperty("ucm_preset_rev", out var rEl) && rEl.ValueKind == JsonValueKind.Number
                    ? rEl.GetInt32() : 0;
                if (string.IsNullOrEmpty(file)) continue;

                string localPath = Path.Combine(UcmPresetsDir, file);

                // Check if the local file is up-to-date
                bool needsDownload = !File.Exists(localPath) || UcmStylePresetNeedsRefresh(localPath);
                if (!needsDownload && catalogRev > UcmStylePresetRevision) needsDownload = true;
                if (!needsDownload) continue;

                try
                {
                    string downloadUrl = UcmPresetsRawBaseUrl + Uri.EscapeDataString(file);
                    string content = await http.GetStringAsync(downloadUrl);

                    // Validate it's a proper UCM preset definition
                    using var presetDoc = JsonDocument.Parse(content);
                    var presetRoot = presetDoc.RootElement;
                    string? kind = presetRoot.TryGetProperty("kind", out var kEl) ? kEl.GetString() : null;
                    if (kind != "style") continue;

                    // Write the definition file (no session_xml — GenerateBuiltInPresets bakes it in)
                    File.WriteAllText(localPath, content);
                    anyNew = true;
                }
                catch { /* skip individual download failures */ }
            }

            if (anyNew)
            {
                // Refresh the preset list so newly downloaded presets appear
                await Dispatcher.InvokeAsync(() =>
                {
                    RefreshPresetManagerLists(preserveSelection: true);
                    // If game dir is available, bake session_xml into the new presets immediately
                    if (!string.IsNullOrEmpty(_gameDir))
                    {
                        GenerateBuiltInPresets();
                        if (_sessionXml == null && _selectedPresetManagerItem != null)
                        {
                            _activePickerKey = null;
                            ActivatePickerFromSelection(_selectedPresetManagerItem, skipCapture: true);
                        }
                    }
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
            {
                GameUpdateStrip.Visibility = Visibility.Collapsed;
                return;
            }

            GameUpdateText.Text = string.IsNullOrEmpty(_gameUpdatePostRefreshNote)
                ? ev.Message
                : ev.Message + "\n\n" + _gameUpdatePostRefreshNote;
            GameUpdateStrip.Visibility = Visibility.Visible;
        }
        catch
        {
            if (IsLoaded) GameUpdateStrip.Visibility = Visibility.Collapsed;
        }
    }

    private void OnGameUpdateDismissClick(object sender, RoutedEventArgs e)
    {
        _gameUpdateNoticeSessionDismissed = true;
        GameUpdateStrip.Visibility = Visibility.Collapsed;
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

            var catalogRevisions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var el in presetsArr.EnumerateArray())
            {
                string file = el.TryGetProperty("file", out var fEl) ? fEl.GetString() ?? "" : "";
                int rev = el.TryGetProperty("ucm_preset_rev", out var rEl) && rEl.ValueKind == JsonValueKind.Number
                    ? rEl.GetInt32() : 0;
                if (!string.IsNullOrEmpty(file))
                    catalogRevisions[Path.GetFileNameWithoutExtension(file)] = rev;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in _presetManagerItems)
                {
                    if (item.KindId is not ("style" or "default") || item.IsPlaceholder) continue;

                    string localPath = item.FilePath;
                    if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath)) continue;

                    // Read local revision from the file header
                    int localRev = 0;
                    try
                    {
                        using var reader = new StreamReader(localPath);
                        var buf = new char[512];
                        int read = reader.Read(buf, 0, buf.Length);
                        string head = new string(buf, 0, read);
                        string? revStr = ExtractJsonStringField(head, "ucm_preset_rev");
                        if (revStr != null) int.TryParse(revStr, out localRev);
                    }
                    catch { }

                    string presetFileName = Path.GetFileNameWithoutExtension(localPath);
                    if (catalogRevisions.TryGetValue(presetFileName, out int catalogRev) && catalogRev > localRev)
                    {
                        item.HasUpdate = true;
                    }
                }
            });
        }
        catch { /* network unavailable — silently skip update check */ }
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
            string downloadUrl = UcmPresetsRawBaseUrl + Uri.EscapeDataString(fileName);
            string content = await http.GetStringAsync(downloadUrl);

            File.WriteAllText(item.FilePath, content);

            // Bake in session_xml if game dir is available
            if (!string.IsNullOrEmpty(_gameDir))
                GenerateBuiltInPresets();

            item.HasUpdate = false;
            RefreshPresetManagerLists(preserveSelection: true);

            // Reload if this was the active preset
            if (ReferenceEquals(_selectedPresetManagerItem, item))
            {
                _activePickerKey = null;
                ActivatePickerFromSelection(item, skipCapture: true);
            }

            QueueSavedToast($"Updated '{item.Name}'");
            SetStatus($"Preset '{item.Name}' updated.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Update failed: {ex.Message}", "Error");
        }
    }

}
