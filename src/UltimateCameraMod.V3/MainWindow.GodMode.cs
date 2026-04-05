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
    private void EnterExpertMode()
    {
        try
        {
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
            _advAllRows = CameraMod.ParseXmlToRows(vanillaXml)
                .Where(r => r.Section.StartsWith("Player_", StringComparison.Ordinal))
                .ToList();

            string liveXml = _sessionXml ?? CameraMod.ReadLiveXml(_gameDir);
            var liveRows = CameraMod.ParseXmlToRows(liveXml);
            var liveLookup = new Dictionary<string, string>();
            foreach (var lr in liveRows)
                liveLookup[lr.FullKey] = lr.Value;
            foreach (var row in _advAllRows)
                if (liveLookup.TryGetValue(row.FullKey, out string? liveVal))
                    row.Value = liveVal;

            // File overlays session: after Quick tab, _sessionXml is curated XML and omits God-only cells;
            // advanced_overrides.json keeps those edits. Preset / full session loads clear that file first.
            // Load overrides first, then bind grid so cells render with correct IsUserEdited state.
            AdvLoadOverrides();
            AdvBindGrid();
            AdvPopulateFilter();
            AdvRefreshPresetCombo();
            AdvUpdateRowCount();

            // WPF DataGrid doesn't measure columns correctly on first load with collapsed groups.
            // Force a re-bind after render so columns size properly.
            // Re-load overrides to ensure IsUserEdited triggers are picked up by fresh cells.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ExpertDataGrid.ItemsSource = null;
                AdvLoadOverrides();
                AdvBindGrid();
                AdvUpdateRowCount();
            }), System.Windows.Threading.DispatcherPriority.Loaded);

            var lightText = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
            AdvSearchBox.Foreground = lightText;
            AdvSearchBox.CaretBrush = lightText;

            SetStatus("God Mode — edit raw XML values and export them as JSON.", "TextDim");
        }
        catch (Exception ex)
        {
            _ = ShowAlertOverlayAsync("God Mode Error", $"Failed to load camera XML:\n{ex.Message}", isError: true);
            SwitchAppMode("simple");
        }

        ApplyPresetEditingLockUi();
    }

    private void AdvBindGrid()
    {
        _advFilteredRows = new ObservableCollection<AdvancedRow>(_advAllRows);
        var view = CollectionViewSource.GetDefaultView(_advFilteredRows);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Section"));
        ExpertDataGrid.ItemsSource = view;
        ScheduleGodModeExpandHook();
    }

    private void AdvPopulateFilter()
    {
        AdvFilterCombo.Items.Clear();
        AdvFilterCombo.Items.Add("All");
        AdvFilterCombo.Items.Add(new System.Windows.Controls.TextBlock
        {
            Text = "Modified only",
            Foreground = (Brush)FindResource("AccentBrush"),
            Tag = "Modified only"
        });
        AdvFilterCombo.Items.Add(new System.Windows.Controls.TextBlock
        {
            Text = "Sacred only",
            Foreground = (Brush)FindResource("SuccessBrush"),
            Tag = "Sacred only"
        });

        var prefixes = _advAllRows
            .Select(r =>
            {
                var parts = r.Section.Split('_');
                return parts.Length > 1 ? parts[1] : r.Section;
            })
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .OrderBy(p => p);
        foreach (var p in prefixes)
            AdvFilterCombo.Items.Add(p);

        AdvFilterCombo.SelectedIndex = 0;
    }

    private void AdvUpdateRowCount()
    {
        int modified = _advAllRows.Count(r => r.IsModified);
        int sacred = _advAllRows.Count(r => r.IsUserEdited);
        AdvRowCountLabel.Text = sacred > 0
            ? $"{_advFilteredRows.Count} rows  |  {modified} modified  |  {sacred} sacred"
            : $"{_advFilteredRows.Count} rows  |  {modified} modified";
    }

    private void OnAdvSearchChanged(object sender, System.Windows.Input.KeyEventArgs e) => AdvApplyFilter();

    private void OnAdvFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || !_isExpertMode) return;
        AdvApplyFilter();
    }

    private void AdvApplyFilter()
    {
        string search = AdvSearchBox.Text?.Trim().ToLowerInvariant() ?? "";
        var selectedItem = AdvFilterCombo.SelectedItem;
        string filter = selectedItem is System.Windows.Controls.TextBlock tb ? (tb.Tag?.ToString() ?? "All") : selectedItem?.ToString() ?? "All";

        var filtered = _advAllRows.AsEnumerable();

        if (filter == "Modified only")
            filtered = filtered.Where(r => r.IsModified);
        else if (filter == "Sacred only")
            filtered = filtered.Where(r => r.IsUserEdited);
        else if (filter != "All")
            filtered = filtered.Where(r => r.Section.Contains(filter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(r =>
                r.Section.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.SubElement.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Attribute.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Value.Contains(search, StringComparison.OrdinalIgnoreCase));

        _advFilteredRows.Clear();
        foreach (var r in filtered) _advFilteredRows.Add(r);

        var view = CollectionViewSource.GetDefaultView(_advFilteredRows);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Section"));
        ExpertDataGrid.ItemsSource = view;
        AdvUpdateRowCount();
        ScheduleGodModeExpandHook();
    }

    private void OnExpertCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        if (IsActivePresetEditingLocked())
            return;

        if (e.EditAction != DataGridEditAction.Commit)
            return;

        // Capture the row reference now -- dispatcher may fire after DataGrid recycles the row
        var editedRow = e.Row.DataContext as AdvancedRow;

        // CellEditEnding fires BEFORE the binding commits the new value,
        // so we must defer to let the Value property update first.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (editedRow != null)
            {
                // Any explicit God Mode edit is sacred -- even if the value matches vanilla.
                // The user's intent is to lock this value from Quick/Fine Tune rebuilds.
                editedRow.IsUserEdited = true;
            }
            AdvSaveOverrides();
            AdvUpdateRowCount();
            SaveCurrentUiState();
            QueueSavedToast();
            if (!_sacredToastShown && editedRow != null && editedRow.IsUserEdited)
            {
                _sacredToastShown = true;
                SetStatus("Sacred edit: this value is now protected from Quick/Fine Tune rebuilds.", "Success");
            }
        }), DispatcherPriority.ContextIdle);
    }

    private void OnAdvExpandAll(object sender, RoutedEventArgs e)
    {
        bool expanding = (sender as System.Windows.Controls.Button)?.Content?.ToString() == "Expand All";
        if (expanding)
        {
            _godModeExpandedSections.Clear();
            foreach (var r in _advFilteredRows)
                _godModeExpandedSections.Add(r.Section);
        }
        else
            _godModeExpandedSections.Clear();

        ExpertDataGrid.GroupStyle.Clear();
        ExpertDataGrid.GroupStyle.Add(BuildAdvGroupStyle(expanding));
        if (sender is System.Windows.Controls.Button btn)
            btn.Content = expanding ? "Collapse All" : "Expand All";

        ScheduleGodModeExpandHook();
    }

    private static GroupStyle BuildAdvGroupStyle(bool expanded)
    {
        string xaml =
            "<GroupStyle xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
            "<GroupStyle.ContainerStyle>" +
            "<Style TargetType='GroupItem'>" +
            "<Setter Property='Template'><Setter.Value>" +
            "<ControlTemplate TargetType='GroupItem'>" +
            "<Expander IsExpanded='" + (expanded ? "True" : "False") + "' Background='Transparent' BorderThickness='0'>" +
            "<Expander.Header>" +
            "<TextBlock Text='{Binding Name}' FontSize='11' FontWeight='SemiBold' Foreground='#c8a24e' Margin='4,2'/>" +
            "</Expander.Header>" +
            "<ItemsPresenter/>" +
            "</Expander>" +
            "</ControlTemplate>" +
            "</Setter.Value></Setter>" +
            "</Style>" +
            "</GroupStyle.ContainerStyle>" +
            "</GroupStyle>";
        return (GroupStyle)System.Windows.Markup.XamlReader.Parse(xaml);
    }

    private void OnAdvResetDefaults(object sender, RoutedEventArgs e)
    {
        if (IsActivePresetEditingLocked())
        {
            SetStatus("Unlock this preset in the sidebar to reset God Mode values.", "Warn");
            return;
        }

        try
        {
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);

            string styleId = GetSelectedStyleId();
            int fov = GetSelectedFov();
            bool bane = BaneCheck.IsChecked == true;
            double pullback = GetCombatPullback();
            bool mount = MountHeightCheck.IsChecked == true;
            double? customUp = null;
            if (styleId == "custom")
            {
                CameraRules.RegisterCustomStyle(DistSlider.Value, HeightSlider.Value, HShiftSlider.Value);
                customUp = HeightSlider.Value;
            }
            bool sc = SteadycamCheck.IsChecked == true;
            bool lockOnAutoRotate = LockOnAutoRotateCheck.IsChecked != true;
            var modSet = CameraRules.BuildModifications(styleId, fov, bane, combatPullback: pullback, mountHeight: mount, customUp: customUp, steadycam: sc, lockOnAutoRotate: lockOnAutoRotate);
            vanillaXml = CameraMod.ApplyModifications(vanillaXml, modSet);

            var defaultRows = CameraMod.ParseXmlToRows(vanillaXml);
            var lookup = new Dictionary<string, string>();
            foreach (var dr in defaultRows)
                lookup[dr.FullKey] = dr.Value;

            foreach (var row in _advAllRows)
            {
                row.Value = lookup.TryGetValue(row.FullKey, out string? val) ? val : row.VanillaValue;
                row.IsUserEdited = false;
            }

            AdvApplyFilter();
            AdvSaveOverrides();
            SaveCurrentUiState(immediate: true);
            _sacredToastShown = false;
            SetStatus("Reset to UCM Quick defaults plus vanilla.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Reset failed: {ex.Message}", "Error");
        }
    }

    private void OnAdvExport(object sender, RoutedEventArgs e)
    {
        var modified = _advAllRows.Where(r => r.IsModified).ToList();
        if (modified.Count == 0) { SetStatus("No modified values to export.", "TextSecondary"); return; }

        var payload = new Dictionary<string, string>();
        foreach (var r in modified) payload[r.FullKey] = r.Value;
        string json = JsonSerializer.Serialize(payload);
        string encoded = "UCM_ADV:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        _ = ShowExportStringOverlayAsync("God Mode Overrides", encoded);
        SetStatus($"Exported {modified.Count} modified values.", "Success");
    }

    private async void OnAdvImport(object sender, RoutedEventArgs e)
    {
        var importResult = await ShowAdvancedImportOverlayAsync();
        if (importResult == null) return;

        int applied = 0;
        var lookup = new Dictionary<string, AdvancedRow>();
        foreach (var r in _advAllRows) lookup[r.FullKey] = r;
        foreach (var (key, val) in importResult)
        {
            if (lookup.TryGetValue(key, out var row)) { row.Value = val; row.IsUserEdited = true; applied++; }
        }

        AdvApplyFilter();
        AdvSaveOverrides();
        SaveCurrentUiState(immediate: true);
        SetStatus($"Imported {applied} values.", "Success");
    }

    private void OnAdvImportXml(object sender, RoutedEventArgs e)
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
            var importedRows = CameraMod.ParseXmlToRows(xml);
            var lookup = new Dictionary<string, string>();
            foreach (var r in importedRows)
                lookup[r.FullKey] = r.Value;

            int applied = 0;
            foreach (var row in _advAllRows)
            {
                if (lookup.TryGetValue(row.FullKey, out string? val) && val != row.Value)
                {
                    row.Value = val;
                    row.IsUserEdited = true;
                    applied++;
                }
            }

            AdvApplyFilter();
            AdvSaveOverrides();
            SaveCurrentUiState(immediate: true);
            SetStatus($"Imported {applied} values from {Path.GetFileName(ofd.FileName)}.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"XML import failed: {ex.Message}", "Error");
        }
    }

    private void OnAdvApply(object sender, RoutedEventArgs e)
    {
        SetStatus("Apply to game has been removed from v3. Use Export JSON to package these overrides.", "Warn");
    }

    private void AdvSaveOverrides()
    {
        try
        {
            var modified = new Dictionary<string, string>();
            foreach (var r in _advAllRows.Where(r => r.IsUserEdited)) modified[r.FullKey] = r.Value;
            if (modified.Count == 0) { if (File.Exists(AdvOverridesPath)) File.Delete(AdvOverridesPath); return; }
            File.WriteAllText(AdvOverridesPath,
                JsonSerializer.Serialize(modified, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    private void AdvLoadOverrides()
    {
        try
        {
            if (!File.Exists(AdvOverridesPath)) return;
            string json = File.ReadAllText(AdvOverridesPath);
            var overrides = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (overrides == null) return;
            var lookup = new Dictionary<string, AdvancedRow>();
            foreach (var r in _advAllRows) lookup[r.FullKey] = r;
            foreach (var (key, val) in overrides)
            {
                if (lookup.TryGetValue(key, out var row))
                {
                    row.Value = val;
                    row.IsUserEdited = true;
                }
            }
        }
        catch { }
    }

    private void AdvRefreshPresetCombo()
    {
        // AdvPresetCombo removed from XAML — preset selection is sidebar-only now.
    }

    private async void OnExportXmlFile(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir))
        {
            SetStatus("Game folder not set.", "Warn");
            return;
        }

        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Export Camera XML",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (sfd.ShowDialog(this) != true) return;

        string gameDir = _gameDir;
        string destPath = sfd.FileName;
        SetGlobalBusy(true, "Exporting camera XML\u2026");
        try
        {
            await Task.Run(() => CameraMod.ExportLiveXml(gameDir, destPath)).ConfigureAwait(true);
            SetStatus($"Exported to {Path.GetFileName(destPath)}.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Export failed: {ex.Message}", "Error");
        }
        finally
        {
            SetGlobalBusy(false);
        }
    }

    private async void OnImportXmlFile(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_gameDir))
        {
            SetStatus("Game folder not set.", "Warn");
            return;
        }

        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import Camera XML â€” installs directly to game",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "playercamerapreset.xml"
        };
        if (ofd.ShowDialog(this) != true) return;

        if (!await ShowConfirmOverlayAsync("Import XML",
            $"Install '{Path.GetFileName(ofd.FileName)}' directly to the game?\n\nThis will overwrite your current camera settings.",
            "Install", "Cancel")) return;

        string gameDir = _gameDir;
        string platform = _detectedPlatform;
        string srcPath = ofd.FileName;
        SetGlobalBusy(true, "Installing camera XML\u2026");
        try
        {
            await Task.Run(() =>
            {
                string xml = File.ReadAllText(srcPath);
                CameraMod.InstallRawXml(gameDir, xml);
            }).ConfigureAwait(true);
            try
            {
                GameInstallBaselineTracker.SaveAfterSuccessfulInstall(ExeDir, Ver, gameDir, platform);
                _gameUpdateNoticeSessionDismissed = false;
            }
            catch { }
            RefreshGameUpdateNotice();
            SetStatus($"Installed {Path.GetFileName(srcPath)} to game.", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Import failed: {ex.Message}", "Error");
        }
        finally
        {
            SetGlobalBusy(false);
        }
    }

    private void ExpertDataGrid_OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (!_isExpertMode || ExpertDataGrid.ItemsSource == null)
            return;

        _godModeExpandHookDebounceTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(75) };
        _godModeExpandHookDebounceTimer.Stop();
        _godModeExpandHookDebounceTimer.Tick -= OnGodModeExpandHookDebounceTick;
        _godModeExpandHookDebounceTimer.Tick += OnGodModeExpandHookDebounceTick;
        _godModeExpandHookDebounceTimer.Start();
    }

    private void OnGodModeExpandHookDebounceTick(object? sender, EventArgs e)
    {
        _godModeExpandHookDebounceTimer?.Stop();
        HookGodModeGroupExpanders();
    }

    private void ScheduleGodModeExpandHook()
    {
        if (!_isExpertMode)
            return;
        Dispatcher.BeginInvoke(new Action(HookGodModeGroupExpanders), DispatcherPriority.Loaded);
    }

    private static T? FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        => FindVisualChildren<T>(depObj).FirstOrDefault();

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null)
            yield break;
        int count = VisualTreeHelper.GetChildrenCount(depObj);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
            if (child is T typed)
                yield return typed;
            foreach (T nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private void HookGodModeGroupExpanders()
    {
        if (!_isExpertMode || ExpertDataGrid == null || ExpertDataGrid.ItemsSource == null)
            return;

        _godModeExpandSyncSuppress = true;
        try
        {
            foreach (var expander in FindVisualChildren<System.Windows.Controls.Expander>(ExpertDataGrid))
            {
                if (expander.DataContext is not CollectionViewGroup g)
                    continue;
                string name = g.Name?.ToString() ?? "";
                if (string.IsNullOrEmpty(name))
                    continue;

                if (!_godModeExpanderHooked.TryGetValue(expander, out _))
                {
                    _godModeExpanderHooked.Add(expander, null);
                    expander.Expanded += GodModeSectionExpander_OnExpanded;
                    expander.Collapsed += GodModeSectionExpander_OnCollapsed;
                }

                expander.IsExpanded = _godModeExpandedSections.Contains(name);
            }
        }
        finally
        {
            _godModeExpandSyncSuppress = false;
        }
    }

    private void GodModeSectionExpander_OnExpanded(object sender, RoutedEventArgs e)
    {
        if (_godModeExpandSyncSuppress)
            return;
        if (sender is System.Windows.Controls.Expander exp && exp.DataContext is CollectionViewGroup gv)
        {
            string name = gv.Name?.ToString() ?? "";
            if (name.Length > 0)
                _godModeExpandedSections.Add(name);
        }
    }

    private void GodModeSectionExpander_OnCollapsed(object sender, RoutedEventArgs e)
    {
        if (_godModeExpandSyncSuppress)
            return;
        if (sender is System.Windows.Controls.Expander exp && exp.DataContext is CollectionViewGroup gv)
        {
            string name = gv.Name?.ToString() ?? "";
            if (name.Length > 0)
                _godModeExpandedSections.Remove(name);
        }
    }

}
