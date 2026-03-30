using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod;

public partial class AdvancedEditorWindow : Window
{
    private static readonly string ExeDir =
        Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

    private static string OverridesPath => Path.Combine(ExeDir, "advanced_overrides.json");

    private readonly string _gameDir;
    private readonly Func<ModificationSet>? _buildDefaultMods;

    private List<AdvancedRow> _allRows = new();
    private ObservableCollection<AdvancedRow> _filteredRows = new();

    public AdvancedEditorWindow(string gameDir, Func<ModificationSet>? buildDefaultMods = null)
    {
        InitializeComponent();
        _gameDir = gameDir;
        _buildDefaultMods = buildDefaultMods;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            string liveXml = CameraMod.ReadLiveXml(_gameDir);
            _allRows = CameraMod.ParseXmlToRows(liveXml);

            LoadOverrides();
            BindGrid();
            PopulateFilter();
            UpdateRowCount();
            SetStatus("Loaded — edit values and click Apply to Game.", "TextDim");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load camera XML:\n{ex.Message}", "Advanced Editor",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    private void BindGrid()
    {
        _filteredRows = new ObservableCollection<AdvancedRow>(_allRows);
        var view = CollectionViewSource.GetDefaultView(_filteredRows);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Section"));
        DataGridView.ItemsSource = view;
    }

    private void PopulateFilter()
    {
        FilterCombo.Items.Clear();
        FilterCombo.Items.Add("All");
        FilterCombo.Items.Add("Modified only");

        var prefixes = _allRows.Select(r => r.Section.Split('_')[1])
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .OrderBy(p => p);
        foreach (var p in prefixes)
            FilterCombo.Items.Add(p);

        FilterCombo.SelectedIndex = 0;
    }

    private void UpdateRowCount()
    {
        int modified = _allRows.Count(r => r.IsModified);
        RowCountLabel.Text = $"{_filteredRows.Count} rows shown  |  {modified} modified";
    }

    // ── Search & Filter ──────────────────────────────────────────────

    private void OnSearchChanged(object sender, System.Windows.Input.KeyEventArgs e)
    {
        ApplyFilter();
    }

    private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        string search = SearchBox.Text?.Trim().ToLowerInvariant() ?? "";
        string filter = FilterCombo.SelectedItem?.ToString() ?? "All";

        var filtered = _allRows.AsEnumerable();

        if (filter == "Modified only")
            filtered = filtered.Where(r => r.IsModified);
        else if (filter != "All")
            filtered = filtered.Where(r => r.Section.Contains(filter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(r =>
                r.Section.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.SubElement.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Attribute.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Value.Contains(search, StringComparison.OrdinalIgnoreCase));

        _filteredRows.Clear();
        foreach (var r in filtered)
            _filteredRows.Add(r);

        var view = CollectionViewSource.GetDefaultView(_filteredRows);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Section"));
        DataGridView.ItemsSource = view;

        UpdateRowCount();
    }

    // ── Toolbar Actions ──────────────────────────────────────────────

    private void OnResetDefaults(object sender, RoutedEventArgs e)
    {
        try
        {
            string vanillaXml;
            if (_buildDefaultMods != null)
            {
                vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
                var modSet = _buildDefaultMods();
                vanillaXml = CameraMod.ApplyModifications(vanillaXml, modSet);
            }
            else
            {
                vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
            }

            var defaultRows = CameraMod.ParseXmlToRows(vanillaXml);
            var lookup = defaultRows.ToDictionary(r => r.FullKey, r => r.Value);

            foreach (var row in _allRows)
            {
                if (lookup.TryGetValue(row.FullKey, out string? val))
                    row.Value = val;
                else
                    row.Value = row.VanillaValue;
            }

            ApplyFilter();
            SetStatus("Reset to defaults (your preset + vanilla).", "Success");
        }
        catch (Exception ex)
        {
            SetStatus($"Reset failed: {ex.Message}", "Error");
        }
    }

    private void OnExportString(object sender, RoutedEventArgs e)
    {
        var modified = _allRows.Where(r => r.IsModified).ToList();
        if (modified.Count == 0)
        {
            SetStatus("No modified values to export.", "TextSecondary");
            return;
        }

        var payload = new Dictionary<string, string>();
        foreach (var r in modified)
            payload[r.FullKey] = r.Value;

        string json = JsonSerializer.Serialize(payload);
        string encoded = "UCM_ADV:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var dlg = new ExportDialog("Advanced Overrides", encoded) { Owner = this };
        dlg.ShowDialog();
        SetStatus($"Exported {modified.Count} modified values.", "Success");
    }

    private void OnImportString(object sender, RoutedEventArgs e)
    {
        var dlg = new AdvancedImportDialog { Owner = this };
        if (dlg.ShowDialog() != true || dlg.Result == null) return;

        int applied = 0;
        var lookup = _allRows.ToDictionary(r => r.FullKey, r => r);

        foreach (var (key, val) in dlg.Result)
        {
            if (lookup.TryGetValue(key, out var row))
            {
                row.Value = val;
                applied++;
            }
        }

        ApplyFilter();
        SetStatus($"Imported {applied} values.", "Success");
    }

    // ── Apply to Game ────────────────────────────────────────────────

    private void OnApplyToGame(object sender, RoutedEventArgs e)
    {
        var modifiedRows = _allRows.Where(r => r.IsModified).ToList();
        if (modifiedRows.Count == 0)
        {
            SetStatus("No changes to apply.", "TextSecondary");
            return;
        }

        ApplyBtn.IsEnabled = false;
        SetStatus("Applying...", "Accent");

        var modSet = BuildModSetFromRows(modifiedRows);

        Task.Run(() =>
        {
            try
            {
                CameraMod.InstallWithModSet(_gameDir, modSet,
                    msg => Dispatcher.Invoke(() => SetStatus(msg, "Accent")));

                Dispatcher.Invoke(() =>
                {
                    SaveOverrides();
                    SetStatus($"Applied {modifiedRows.Count} changes to game files.", "Success");
                    ApplyBtn.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Apply failed: {ex.Message}", "Error");
                    ApplyBtn.IsEnabled = true;
                });
            }
        });
    }

    private static ModificationSet BuildModSetFromRows(List<AdvancedRow> rows)
    {
        var elementMods = new Dictionary<string, Dictionary<string, (string Action, string Value)>>();

        foreach (var row in rows)
        {
            string key = row.ModKey;
            if (!elementMods.TryGetValue(key, out var attrs))
            {
                attrs = new Dictionary<string, (string, string)>();
                elementMods[key] = attrs;
            }
            attrs[row.Attribute] = ("SET", row.Value);
        }

        return new ModificationSet { ElementMods = elementMods, FovValue = 0 };
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    // ── Persistence ──────────────────────────────────────────────────

    private void SaveOverrides()
    {
        try
        {
            var modified = _allRows.Where(r => r.IsModified)
                .ToDictionary(r => r.FullKey, r => r.Value);

            if (modified.Count == 0)
            {
                if (File.Exists(OverridesPath)) File.Delete(OverridesPath);
                return;
            }

            string json = JsonSerializer.Serialize(modified, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(OverridesPath, json);
        }
        catch { }
    }

    private void LoadOverrides()
    {
        try
        {
            if (!File.Exists(OverridesPath)) return;
            string json = File.ReadAllText(OverridesPath);
            var overrides = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (overrides == null) return;

            var lookup = _allRows.ToDictionary(r => r.FullKey, r => r);
            foreach (var (key, val) in overrides)
            {
                if (lookup.TryGetValue(key, out var row))
                    row.Value = val;
            }
        }
        catch { }
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private void SetStatus(string text, string brushKey)
    {
        StatusLabel.Text = text;
        StatusLabel.Foreground = brushKey switch
        {
            "Success" => (Brush)FindResource("SuccessBrush"),
            "Error" => (Brush)FindResource("ErrorBrush"),
            "Accent" => (Brush)FindResource("AccentBrush"),
            "TextSecondary" => (Brush)FindResource("TextSecondaryBrush"),
            _ => (Brush)FindResource("TextDimBrush"),
        };
    }
}
