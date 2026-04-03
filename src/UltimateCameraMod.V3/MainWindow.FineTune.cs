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
    // â”€â”€ Advanced Controls â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    // Stores all slider controls keyed by ModKey.Attribute (same as AdvancedRow.FullKey)
    private readonly Dictionary<string, Slider> _advCtrlSliders = new();
    private readonly Dictionary<string, TextBlock> _advCtrlValueLabels = new();
    private readonly Dictionary<string, string> _advCtrlVanilla = new();
    // Every slider instance ever created — used for lock/unlock so duplicate-key sliders aren't missed
    private readonly List<Slider> _advCtrlAllSliders = new();
    // Keys controlled by Steadycam -- sliders for these are locked when Steadycam is on
    private HashSet<string>? _steadycamKeys;

    // ── Cached resources (avoid repeated FindResource lookups) ──
    private Style? _accentButtonStyle;
    private Style? _subtleButtonStyle;
    private Brush? _textSecondaryBrush;
    private Brush? _accentBrush;
    private Brush? _textDimBrush;
    private DispatcherTimer? _advCtrlSearchDebounceTimer;
    // AdvCtrlPresetsDir removed — session presets live under ucm_presets / my_presets

    private void EnterAdvancedControlsMode()
    {
        if (_advCtrlSliders.Count > 0)
        {
            string xml = _sessionXml ?? BuildCuratedSessionXml();
            ApplySessionXmlToAdvancedControls(xml);
            ApplyPresetEditingLockUi();
            return;
        }

        try
        {
            // Load vanilla values for display and reset (cached after first load)
            if (_advCtrlVanilla.Count == 0)
            {
                string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
                var vanillaRows = CameraMod.ParseXmlToRows(vanillaXml);
                foreach (var r in vanillaRows) _advCtrlVanilla[r.FullKey] = r.VanillaValue;
            }

            // Suppress layout passes while building ~150 slider controls
            AdvCtrlPanel.BeginInit();
            try
            {
                BuildAdvCtrlSection_OnFoot();
                BuildAdvCtrlSection_Mount();
                BuildAdvCtrlSection_Global();
                BuildAdvCtrlSection_SpecialMounts();
                BuildAdvCtrlSection_Combat();
                BuildAdvCtrlSection_Smooth();
                BuildAdvCtrlSection_Aim();
            }
            finally
            {
                AdvCtrlPanel.EndInit();
            }

            AdvCtrlRefreshPresetCombo();
            string sessionXml = _sessionXml ?? BuildCuratedSessionXml();
            ApplySessionXmlToAdvancedControls(sessionXml);
            AdvCtrlUpdateChangedLabel();
            ApplyAdvCtrlSearch();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load camera XML:\n{ex.Message}", "UCM Fine Tune",
                MessageBoxButton.OK, MessageBoxImage.Error);
            SwitchAppMode("simple");
        }

        ApplyPresetEditingLockUi();
    }

    private Grid BuildSliderRow(string modKey, string attribute, double min, double max, double step,
        string? tooltip = null)
    {
        string fullKey = $"{modKey}.{attribute}";
        _advCtrlVanilla.TryGetValue(fullKey, out string? vanillaStr);
        double vanillaVal = double.TryParse(vanillaStr, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double vv) ? vv : (min + max) / 2;
        double current = vanillaVal;

        string searchText = $"{modKey} {attribute} {tooltip ?? CameraParamDocs.Get(attribute)}";
        var row = new Grid { Margin = new Thickness(0, 2, 0, 2), Tag = searchText };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200), MinWidth = 130 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 60 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46), MinWidth = 46 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46), MinWidth = 46 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56), MinWidth = 56 });

        // Label
        var label = new TextBlock
        {
            Text = attribute,
            FontSize = 11,
            Foreground = _textSecondaryBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            ToolTip = tooltip ?? CameraParamDocs.Get(attribute)
        };
        Grid.SetColumn(label, 0);

        // Slider
        var slider = new Slider
        {
            Minimum = min, Maximum = max,
            Value = current,
            TickFrequency = step,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 4, 0),
            ToolTip = tooltip ?? CameraParamDocs.Get(attribute)
        };
        ToolTipService.SetShowOnDisabled(slider, true);
        Grid.SetColumn(slider, 1);

        // Value label
        var valueLabel = new TextBlock
        {
            Text = $"{current:F2}",
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 10,
            Foreground = _accentBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 0, 2, 0)
        };
        Grid.SetColumn(valueLabel, 2);

        // Vanilla label
        var vanillaLabel = new TextBlock
        {
            Text = $"{vanillaVal:F2}",
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 10,
            Foreground = _textDimBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 0, 2, 0),
            ToolTip = $"Vanilla: {vanillaVal:F2}"
        };
        Grid.SetColumn(vanillaLabel, 3);

        // Reset button
        var resetBtn = new System.Windows.Controls.Button
        {
            Content = "Reset",
            FontSize = 10,
            Width = 50,
            Height = 22,
            Padding = new Thickness(6, 2, 6, 2),
            Margin = new Thickness(2, 0, 0, 0),
            ToolTip = $"Reset to vanilla ({vanillaVal:F2})",
            Style = _subtleButtonStyle,
            Tag = (slider, vanillaVal)
        };
        Grid.SetColumn(resetBtn, 4);
        resetBtn.Click += (s, e) =>
        {
            if (((System.Windows.Controls.Button)s).Tag is (Slider sl, double vv2))
                sl.Value = vv2;
        };

        slider.ValueChanged += (s, e) =>
        {
            if (_suppressEvents) return;
            if (IsActivePresetEditingLocked() || IsActivePresetDeepEditLocked()) { ShowLockedToastIfNeeded(); slider.Value = e.OldValue; return; }
            valueLabel.Text = $"{e.NewValue:F2}";
            bool changed = Math.Abs(e.NewValue - vanillaVal) > 0.001;
            valueLabel.Foreground = changed
                ? _accentBrush
                : _textDimBrush;
            AdvCtrlUpdateChangedLabel();
            SaveCurrentUiState();
            QueueSavedToast();
        };

        row.Children.Add(label);
        row.Children.Add(slider);
        row.Children.Add(valueLabel);
        row.Children.Add(vanillaLabel);
        row.Children.Add(resetBtn);

        _advCtrlSliders[fullKey] = slider;
        _advCtrlAllSliders.Add(slider);
        _advCtrlValueLabels[fullKey] = valueLabel;

        return row;
    }

    private TextBlock BuildAdvCtrlSubHeader(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = _textSecondaryBrush,
            Margin = new Thickness(0, 10, 0, 4)
        };
    }

    private Border WrapInCard(string title, params UIElement[] children)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = _accentBrush,
            Margin = new Thickness(0, 0, 0, 8)
        });
        foreach (var child in children)
            stack.Children.Add(child);

        return new Border
        {
            Background = (Brush)FindResource("BgInputBrush"),
            BorderBrush = (Brush)FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 10, 10, 10),
            Margin = new Thickness(0, 8, 0, 6),
            Tag = title,
            Child = stack
        };
    }

    private Grid BuildSharedSliderRow(string labelText, string[] modKeys, string attribute,
        double min, double max, double step, string? tooltip = null)
    {
        string representative = modKeys[0];
        var row = BuildSliderRow(representative, attribute, min, max, step, tooltip);
        if (row.Children[0] is TextBlock label)
            label.Text = labelText;

        var actualSlider = _advCtrlSliders[$"{representative}.{attribute}"];
        foreach (string modKey in modKeys)
            _advCtrlSliders[$"{modKey}.{attribute}"] = actualSlider;

        return row;
    }

    private FrameworkElement BuildZoomLevelGroup(string title, string[] sections, int zoomLevel,
        (string Attr, double Min, double Max, double Step)[] attrs)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = _accentBrush,
            Margin = new Thickness(0, 0, 0, 8)
        });

        // Use first section as the representative key (all sections share same values in UCM)
        string repSection = sections[0];
        string modKey = $"{repSection}/ZoomLevel[{zoomLevel}]";

        foreach (var (attr, min, max, step) in attrs)
        {
            // Register all sections' keys so BuildAdvancedControlsModSet can apply to all
            foreach (var sec in sections)
                _advCtrlSliders[$"{sec}/ZoomLevel[{zoomLevel}].{attr}"] = null!; // placeholder

            var row = BuildSliderRow(modKey, attr, min, max, step);
            stack.Children.Add(row);

            // Re-register the actual slider for all sections
            var actualSlider = _advCtrlSliders[$"{modKey}.{attr}"];
            foreach (var sec in sections)
            {
                string k = $"{sec}/ZoomLevel[{zoomLevel}].{attr}";
                _advCtrlSliders[k] = actualSlider;
            }
        }
        return new Border
        {
            Background = (Brush)FindResource("BgInputBrush"),
            BorderBrush = (Brush)FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 10, 10, 10),
            Margin = new Thickness(0, 8, 0, 6),
            Tag = title,
            Child = stack
        };
    }

    private void BuildAdvCtrlSection_OnFoot()
    {
        var panel = new StackPanel();
        string[] allOnFoot = {
            "Player_Basic_Default", "Player_Basic_Default_Walk",
            "Player_Basic_Default_Run", "Player_Basic_Default_Runfast",
            "Player_Weapon_Default", "Player_Weapon_Default_Walk",
            "Player_Weapon_Default_Run", "Player_Weapon_Default_RunFast",
            "Player_Weapon_Default_RunFast_Follow", "Player_Weapon_Rush", "Player_Weapon_Guard"
        };

        (string, double, double, double)[] zoomAttrs = {
            ("ZoomDistance",    1.0, 20.0, 0.1),
            ("UpOffset",       -2.0,  1.0, 0.1),
            ("InDoorUpOffset", -2.0,  1.0, 0.1),
            ("RightOffset",    -1.0,  3.0, 0.05),
        };

        foreach (int zl in new[] { 2, 3, 4 })
            panel.Children.Add(BuildZoomLevelGroup($"Zoom Level {zl}", allOnFoot, zl, zoomAttrs));

        AdvCtrlOnFootGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Mount()
    {
        var panel = new StackPanel();
        string[] horseSections = {
            "Player_Ride_Horse", "Player_Ride_Horse_Run", "Player_Ride_Horse_Fast_Run",
            "Player_Ride_Horse_Dash", "Player_Ride_Horse_Dash_Att",
            "Player_Ride_Horse_Att_Thrust", "Player_Ride_Horse_Att_R", "Player_Ride_Horse_Att_L"
        };

        (string, double, double, double)[] zoomAttrs = {
            ("ZoomDistance", 0.5, 25.0, 0.1),
            ("UpOffset",    -2.0,  2.0, 0.1),
            ("RightOffset", -1.0,  4.0, 0.05),
        };

        foreach (int zl in new[] { 2, 3 })
            panel.Children.Add(BuildZoomLevelGroup($"Zoom Level {zl}", horseSections, zl, zoomAttrs));

        AdvCtrlMountGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Global()
    {
        var panel = new StackPanel();

        var sharedFovSliders = new List<UIElement>();
        sharedFovSliders.Add(BuildSharedSliderRow("On-foot FoV",
            new[]
            {
                "Player_Basic_Default_Run", "Player_Basic_Default_Runfast", "Player_Basic_Default_Walk"
            },
            "Fov", 25.0, 75.0, 1.0, "Shared field of view for the main on-foot movement cameras."));
        sharedFovSliders.Add(BuildSharedSliderRow("Combat FoV",
            new[]
            {
                "Player_Weapon_Default", "Player_Weapon_Default_Run", "Player_Weapon_Default_RunFast",
                "Player_Weapon_Default_RunFast_Follow", "Player_Weapon_Default_Walk",
                "Player_Weapon_Rush", "Player_Weapon_Guard"
            },
            "Fov", 25.0, 75.0, 1.0, "Shared field of view for the core weapon and combat movement cameras."));
        sharedFovSliders.Add(BuildSharedSliderRow("Force/Titan/Cinematic FoV",
            new[]
            {
                "Player_Force_LockOn", "Player_LockOn_Titan", "Cinematic_LockOn"
            },
            "Fov", 25.0, 75.0, 1.0));
        sharedFovSliders.Add(BuildSharedSliderRow("Warmachine/Broom FoV",
            new[]
            {
                "Player_Ride_Warmachine", "Player_Ride_Warmachine_Aim",
                "Player_Ride_Warmachine_Dash", "Player_Ride_Broom"
            },
            "Fov", 25.0, 75.0, 1.0));
        {
            var elephantFovRow = BuildSliderRow("Player_Ride_Elephant", "Fov", 25.0, 75.0, 1.0, "Elephant field of view.");
            if (elephantFovRow.Children[0] is TextBlock elephantFovLabel)
                elephantFovLabel.Text = "Elephant FoV";
            sharedFovSliders.Add(elephantFovRow);
        }
        {
            var wyvernFovRow = BuildSliderRow("Player_Ride_Wyvern", "Fov", 25.0, 75.0, 1.0, "Wyvern field of view.");
            if (wyvernFovRow.Children[0] is TextBlock wyvernFovLabel)
                wyvernFovLabel.Text = "Wyvern FoV";
            sharedFovSliders.Add(wyvernFovRow);
        }
        {
            var swimFovRow = BuildSliderRow("Player_Swim_Default", "Fov", 25.0, 75.0, 1.0, "Swimming field of view.");
            if (swimFovRow.Children[0] is TextBlock swimFovLabel)
                swimFovLabel.Text = "Swim FoV";
            sharedFovSliders.Add(swimFovRow);
        }
        panel.Children.Add(WrapInCard("Shared FoV", sharedFovSliders.ToArray()));

        var twoTargetSliders = new List<UIElement>();
        {
            var interactionZl3Row = BuildSliderRow("Player_Interaction_TwoTarget/ZoomLevel[3]", "MaxZoomDistance", 4.0, 20.0, 0.5);
            if (interactionZl3Row.Children[0] is TextBlock interactionZl3Label)
                interactionZl3Label.Text = "Interaction ZL3 Max";
            twoTargetSliders.Add(interactionZl3Row);
        }
        {
            var interactionZl4Row = BuildSliderRow("Player_Interaction_TwoTarget/ZoomLevel[4]", "MaxZoomDistance", 4.0, 20.0, 0.5);
            if (interactionZl4Row.Children[0] is TextBlock interactionZl4Label)
                interactionZl4Label.Text = "Interaction ZL4 Max";
            twoTargetSliders.Add(interactionZl4Row);
        }
        panel.Children.Add(WrapInCard("Two-target framing", twoTargetSliders.ToArray()));

        var traversalFovSliders = new List<UIElement>();
        foreach (var (modKey, labelText) in new[]
        {
            ("Player_Swim_Default", "Swim FoV"),
            ("Player_Basic_Climb", "Climb FoV"),
            ("Player_Basic_Gliding", "Glide FoV"),
            ("Player_Basic_FreeFall", "Freefall FoV")
        })
        {
            var row = BuildSliderRow(modKey, "Fov", 25.0, 75.0, 1.0);
            if (row.Children[0] is TextBlock label)
                label.Text = labelText;
            traversalFovSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Traversal FoV", traversalFovSliders.ToArray()));

        AdvCtrlGlobalGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_SpecialMounts()
    {
        var panel = new StackPanel();

        var elephantSliders = new List<UIElement>();
        foreach (int zl in new[] { 1, 2, 3, 4 })
        {
            var row = BuildSliderRow($"Player_Ride_Elephant/ZoomLevel[{zl}]", "ZoomDistance", 0.5, 25.0, 0.1);
            if (row.Children[0] is TextBlock label)
                label.Text = $"Elephant ZL{zl}";
            elephantSliders.Add(row);
        }
        foreach (int zl in new[] { 2, 3, 4 })
        {
            var row = BuildSliderRow($"Player_Ride_Elephant/ZoomLevel[{zl}]", "UpOffset", -2.0, 3.0, 0.1);
            if (row.Children[0] is TextBlock label)
                label.Text = $"Elephant ZL{zl} Height";
            elephantSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Elephant", elephantSliders.ToArray()));

        var wyvernSliders = new List<UIElement>();
        foreach (int zl in new[] { 1, 2, 3, 4 })
        {
            var row = BuildSliderRow($"Player_Ride_Wyvern/ZoomLevel[{zl}]", "ZoomDistance", 1.0, 30.0, 0.1);
            if (row.Children[0] is TextBlock label)
                label.Text = $"Wyvern ZL{zl}";
            wyvernSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Wyvern", wyvernSliders.ToArray()));

        var canoeMiscSliders = new List<UIElement>();
        foreach (var (modKey, attr, min, max, step, labelText) in new[]
        {
            ("Player_Ride_Canoe/ZoomLevel[2]", "ZoomDistance", 1.0, 20.0, 0.1, "Canoe ZL2"),
            ("Player_Ride_Canoe/ZoomLevel[3]", "ZoomDistance", 1.0, 20.0, 0.1, "Canoe ZL3"),
            ("Player_Ride_Warmachine/ZoomLevel[2]", "ZoomDistance", 1.0, 20.0, 0.1, "Warmachine ZL2"),
            ("Player_Ride_Warmachine/ZoomLevel[3]", "ZoomDistance", 1.0, 20.0, 0.1, "Warmachine ZL3"),
            ("Player_Ride_Broom/ZoomLevel[2]", "ZoomDistance", 1.0, 24.0, 0.1, "Broom ZL2"),
            ("Player_Ride_Broom/ZoomLevel[3]", "ZoomDistance", 1.0, 24.0, 0.1, "Broom ZL3")
        })
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock label)
                label.Text = labelText;
            canoeMiscSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Canoe / Warmachine / Broom", canoeMiscSliders.ToArray()));

        AdvCtrlSpecialMountGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Combat()
    {
        var panel = new StackPanel();

        // Section-level attributes
        var sectionAttrs = new[]
        {
            ("Player_Weapon_LockOn",    "TargetRate",      0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn",    "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_TwoTarget", "TargetRate",      0.0, 1.0, 0.05),
            ("Player_Weapon_TwoTarget", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_TwoTarget", "LimitUnderDistance", 0.5, 10.0, 0.5),
            ("Player_Interaction_TwoTarget", "TargetRate", 0.0, 1.0, 0.05),
            ("Player_Interaction_TwoTarget", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn_System", "TargetRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn_System", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Revive_LockOn_System", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn_Non_Rotate", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Weapon_LockOn_WrestleOnly", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_StartAggro_TwoTarget", "ScreenClampRate", 0.0, 1.0, 0.05),
            ("Player_Wanted_TwoTarget", "ScreenClampRate", 0.0, 1.0, 0.05),
        };

        var trackingSliders = new List<UIElement>();
        foreach (var (sec, attr, min, max, step) in sectionAttrs)
        {
            var row = BuildSliderRow(sec, attr, min, max, step);
            if (row.Children[0] is TextBlock lbl)
                lbl.Text = $"{sec.Replace("Player_", "")} - {attr}";
            trackingSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Lock-On Tracking", trackingSliders.ToArray()));

        // ZoomDistance per lock-on section per zoom level
        // All sections now expose ZL2+ZL3+ZL4; missing levels are injected by Steadycam.
        var lockOnSections = new[]
        {
            ("Player_Weapon_LockOn",              new[] { 2, 3, 4 }),
            ("Player_Weapon_TwoTarget",           new[] { 1, 2, 3, 4 }),
            ("Player_Interaction_TwoTarget",      new[] { 1, 2, 3, 4 }),
            ("Player_FollowLearn_LockOn_Boss",    new[] { 2, 3, 4 }),
            ("Player_Weapon_LockOn_System",       new[] { 2, 3, 4 }),
            ("Player_Revive_LockOn_System",       new[] { 2, 3, 4 }),
            ("Player_Weapon_LockOn_Non_Rotate",   new[] { 2, 3, 4 }),
            ("Player_Weapon_LockOn_WrestleOnly",  new[] { 2, 3, 4 }),
        };

        foreach (var (sec, levels) in lockOnSections)
        {
            var zoomSliders = new List<UIElement>();
            foreach (int zl in levels)
            {
                var row = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "ZoomDistance", 1.0, 20.0, 0.5);
                if (row.Children[0] is TextBlock lbl) lbl.Text = $"ZL{zl} ZoomDistance";
                zoomSliders.Add(row);
            }
            panel.Children.Add(WrapInCard($"{sec.Replace("Player_", "")} - Zoom Distances", zoomSliders.ToArray()));
        }

        var fovSliders = new List<UIElement>();
        foreach (var (modKey, labelText) in new[]
        {
            ("Player_Weapon_LockOn", "Weapon LockOn FoV"),
            ("Player_Weapon_TwoTarget", "Weapon TwoTarget FoV"),
            ("Player_Interaction_TwoTarget", "Interaction TwoTarget FoV"),
            ("Player_FollowLearn_LockOn_Boss", "Boss LockOn FoV"),
            ("Player_Weapon_LockOn_System", "LockOn System FoV"),
            ("Player_Revive_LockOn_System", "Revive LockOn FoV"),
            ("Player_Weapon_LockOn_Non_Rotate", "Non-Rotate LockOn FoV"),
            ("Player_Weapon_LockOn_WrestleOnly", "Wrestle LockOn FoV"),
            ("Player_StartAggro_TwoTarget", "StartAggro FoV"),
            ("Player_Wanted_TwoTarget", "Wanted FoV")
        })
        {
            var row = BuildSliderRow(modKey, "Fov", 25.0, 75.0, 1.0);
            if (row.Children[0] is TextBlock label)
                label.Text = labelText;
            fovSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("FoV Touch Points", fovSliders.ToArray()));

        AdvCtrlCombatGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Smooth()
    {
        var panel = new StackPanel();

        var smoothSliders = new List<UIElement>();
        var smoothEntries = new[]
        {
            ("Player_Basic_Default_Run/CameraBlendParameter",  "BlendInTime",  0.0, 3.0, 0.1, "Run blend-in"),
            ("Player_Basic_Default_Run/CameraBlendParameter",  "BlendOutTime", 0.0, 3.0, 0.1, "Run blend-out"),
            ("Player_Weapon_Guard/CameraBlendParameter",       "BlendInTime",  0.0, 3.0, 0.1, "Guard blend-in"),
            ("Player_Weapon_Guard/CameraBlendParameter",       "BlendOutTime", 0.0, 3.0, 0.1, "Guard blend-out"),
            ("Player_Weapon_Rush/CameraBlendParameter",        "BlendInTime",  0.0, 3.0, 0.1, "Rush blend-in"),
            ("Player_Weapon_Rush/CameraBlendParameter",        "BlendOutTime", 0.0, 3.0, 0.1, "Rush blend-out"),
            ("Player_Basic_Default_Run/OffsetByVelocity",      "OffsetLength", 0.0, 2.0, 0.1, "Run sway"),
            ("Player_Weapon_Default_Run/OffsetByVelocity",     "OffsetLength", 0.0, 2.0, 0.1, "Combat run sway"),
            ("Player_Weapon_Default_RunFast/OffsetByVelocity", "OffsetLength", 0.0, 2.0, 0.1, "Combat sprint sway"),
            ("Player_Weapon_Default_RunFast_Follow/OffsetByVelocity", "OffsetLength", 0.0, 2.0, 0.1, "Follow sprint sway"),
            ("Player_Animal_Default/CameraBlendParameter",     "BlendInTime",  0.0, 3.0, 0.1, "Animal idle blend-in"),
            ("Player_Animal_Default_Run/CameraBlendParameter", "BlendInTime",  0.0, 3.0, 0.1, "Animal run blend-in"),
            ("Player_Animal_Default_Run/OffsetByVelocity",     "OffsetLength", 0.0, 2.0, 0.1, "Animal run sway"),
            ("Player_Animal_Default_Runfast/CameraBlendParameter", "BlendInTime", 0.0, 3.0, 0.1, "Animal sprint blend-in"),
            ("Player_Animal_Default_Runfast/OffsetByVelocity", "OffsetLength", 0.0, 2.0, 0.1, "Animal sprint sway"),
            ("Player_Animal_Default_Runfast/OffsetByVelocity", "DampSpeed",    0.0, 2.0, 0.1, "Animal sprint damp"),
            ("Player_Weapon_LockOn/CameraBlendParameter",               "BlendInTime",  0.0, 3.0, 0.1, "LockOn blend-in"),
            ("Player_Weapon_LockOn/CameraBlendParameter",               "BlendOutTime", 0.0, 3.0, 0.1, "LockOn blend-out"),
            ("Player_Weapon_LockOn_System/CameraBlendParameter",        "BlendInTime",  0.0, 3.0, 0.1, "LockOn System blend-in"),
            ("Player_Weapon_LockOn_System/CameraBlendParameter",        "BlendOutTime", 0.0, 3.0, 0.1, "LockOn System blend-out"),
            ("Player_FollowLearn_LockOn_Boss/CameraBlendParameter",     "BlendInTime",  0.0, 3.0, 0.1, "Boss LockOn blend-in"),
            ("Player_FollowLearn_LockOn_Boss/CameraBlendParameter",     "BlendOutTime", 0.0, 3.0, 0.1, "Boss LockOn blend-out"),
        };

        foreach (var (modKey, attr, min, max, step, friendlyName) in smoothEntries)
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock lbl) lbl.Text = friendlyName;
            smoothSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("On-foot and combat smoothing", smoothSliders.ToArray()));

        // Movement transitions card -- freefall, rope, super jump, hit
        var movementTransitionSliders = new List<UIElement>();
        var movementTransitionEntries = new[]
        {
            ("Player_Basic_FreeFall_Start/CameraBlendParameter", "BlendInTime",  0.0, 3.0, 0.1, "Freefall entry blend-in"),
            ("Player_Basic_FreeFall_Start/CameraBlendParameter", "BlendOutTime", 0.0, 3.0, 0.1, "Freefall entry blend-out"),
            ("Player_Basic_FreeFall/CameraBlendParameter",       "BlendInTime",  0.0, 3.0, 0.1, "Freefall blend-in"),
            ("Player_Basic_FreeFall/CameraBlendParameter",       "BlendOutTime", 0.0, 3.0, 0.1, "Freefall blend-out"),
            ("Player_Basic_SuperJump/CameraBlendParameter",      "BlendInTime",  0.0, 3.0, 0.1, "Super jump blend-in"),
            ("Player_Basic_SuperJump/CameraBlendParameter",      "BlendOutTime", 0.0, 3.0, 0.1, "Super jump blend-out"),
            ("Player_Basic_RopePull/CameraBlendParameter",       "BlendInTime",  0.0, 3.0, 0.1, "Rope pull blend-in"),
            ("Player_Basic_RopePull/CameraBlendParameter",       "BlendOutTime", 0.0, 3.0, 0.1, "Rope pull blend-out"),
            ("Player_Basic_RopeSwing/CameraBlendParameter",      "BlendInTime",  0.0, 3.0, 0.1, "Rope swing blend-in"),
            ("Player_Basic_RopeSwing/CameraBlendParameter",      "BlendOutTime", 0.0, 3.0, 0.1, "Rope swing blend-out"),
            ("Player_Hit_Throw/CameraBlendParameter",            "BlendInTime",  0.0, 3.0, 0.1, "Hit/thrown blend-in"),
            ("Player_Hit_Throw/CameraBlendParameter",            "BlendOutTime", 0.0, 3.0, 0.1, "Hit/thrown blend-out"),
        };

        foreach (var (modKey, attr, min, max, step, friendlyName) in movementTransitionEntries)
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock lbl) lbl.Text = friendlyName;
            movementTransitionSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Movement transitions", movementTransitionSliders.ToArray()));

        // Extended lock-on smoothing card -- mount lock-on, revive, force, titan, non-rotate, wrestle, aggro, wanted
        var extLockOnSliders = new List<UIElement>();
        var extLockOnEntries = new[]
        {
            ("Player_Ride_Aim_LockOn/CameraBlendParameter",          "BlendInTime",  0.0, 3.0, 0.1, "Mount LockOn blend-in"),
            ("Player_Ride_Aim_LockOn/CameraBlendParameter",          "BlendOutTime", 0.0, 3.0, 0.1, "Mount LockOn blend-out"),
            ("Player_Revive_LockOn_System/CameraBlendParameter",     "BlendInTime",  0.0, 3.0, 0.1, "Revive LockOn blend-in"),
            ("Player_Revive_LockOn_System/CameraBlendParameter",     "BlendOutTime", 0.0, 3.0, 0.1, "Revive LockOn blend-out"),
            ("Player_Force_LockOn/CameraBlendParameter",             "BlendInTime",  0.0, 3.0, 0.1, "Force LockOn blend-in"),
            ("Player_Force_LockOn/CameraBlendParameter",             "BlendOutTime", 0.0, 3.0, 0.1, "Force LockOn blend-out"),
            ("Player_LockOn_Titan/CameraBlendParameter",             "BlendInTime",  0.0, 3.0, 0.1, "Titan LockOn blend-in"),
            ("Player_LockOn_Titan/CameraBlendParameter",             "BlendOutTime", 0.0, 3.0, 0.1, "Titan LockOn blend-out"),
            ("Player_Weapon_LockOn_Non_Rotate/CameraBlendParameter", "BlendInTime",  0.0, 3.0, 0.1, "Non-rotate LockOn blend-in"),
            ("Player_Weapon_LockOn_Non_Rotate/CameraBlendParameter", "BlendOutTime", 0.0, 3.0, 0.1, "Non-rotate LockOn blend-out"),
            ("Player_Weapon_LockOn_WrestleOnly/CameraBlendParameter","BlendInTime",  0.0, 3.0, 0.1, "Wrestle LockOn blend-in"),
            ("Player_Weapon_LockOn_WrestleOnly/CameraBlendParameter","BlendOutTime", 0.0, 3.0, 0.1, "Wrestle LockOn blend-out"),
            ("Player_StartAggro_TwoTarget/CameraBlendParameter",     "BlendInTime",  0.0, 3.0, 0.1, "Aggro blend-in"),
            ("Player_StartAggro_TwoTarget/CameraBlendParameter",     "BlendOutTime", 0.0, 3.0, 0.1, "Aggro blend-out"),
            ("Player_Wanted_TwoTarget/CameraBlendParameter",         "BlendInTime",  0.0, 3.0, 0.1, "Wanted blend-in"),
            ("Player_Wanted_TwoTarget/CameraBlendParameter",         "BlendOutTime", 0.0, 3.0, 0.1, "Wanted blend-out"),
            ("Player_Ride_Warmachine_Aim/CameraBlendParameter",      "BlendInTime",  0.0, 3.0, 0.1, "Warmachine aim blend-in"),
            ("Player_Ride_Warmachine_Aim/CameraBlendParameter",      "BlendOutTime", 0.0, 3.0, 0.1, "Warmachine aim blend-out"),
            ("Player_Ride_Warmachine_Dash/CameraBlendParameter",     "BlendInTime",  0.0, 3.0, 0.1, "Warmachine dash blend-in"),
            ("Player_Ride_Warmachine_Dash/CameraBlendParameter",     "BlendOutTime", 0.0, 3.0, 0.1, "Warmachine dash blend-out"),
        };

        foreach (var (modKey, attr, min, max, step, friendlyName) in extLockOnEntries)
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock lbl) lbl.Text = friendlyName;
            extLockOnSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Extended lock-on and combat transitions", extLockOnSliders.ToArray()));

        string[] onFootFollowSections =
        {
            "Player_Basic_Default", "Player_Basic_Default_Walk",
            "Player_Basic_Default_Run", "Player_Basic_Default_Runfast"
        };

        var onFootFollowSliders = new List<UIElement>();
        onFootFollowSliders.Add(BuildSharedSliderRow("On-foot yaw follow", onFootFollowSections, "FollowYawSpeedRate", 0.0, 2.0, 0.05));
        onFootFollowSliders.Add(BuildSharedSliderRow("On-foot pitch follow", onFootFollowSections, "FollowPitchSpeedRate", 0.0, 2.0, 0.05));
        onFootFollowSliders.Add(BuildSharedSliderRow("On-foot follow delay", onFootFollowSections, "FollowStartTime", 0.0, 5.0, 0.1));
        onFootFollowSliders.Add(BuildSharedSliderRow("On-foot pivot damping", Array.ConvertAll(onFootFollowSections, s => $"{s}/CameraDamping"), "PivotDampingMaxDistance", 0.0, 2.0, 0.05));
        panel.Children.Add(WrapInCard("On-foot follow behavior", onFootFollowSliders.ToArray()));

        string[] horseSections =
        {
            "Player_Ride_Horse", "Player_Ride_Horse_Run", "Player_Ride_Horse_Fast_Run",
            "Player_Ride_Horse_Dash", "Player_Ride_Horse_Dash_Att",
            "Player_Ride_Horse_Att_Thrust", "Player_Ride_Horse_Att_R", "Player_Ride_Horse_Att_L"
        };

        var horseSyncSliders = new List<UIElement>();
        horseSyncSliders.Add(BuildSharedSliderRow("Horse yaw follow", horseSections, "FollowYawSpeedRate", 0.0, 2.0, 0.05));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse pitch follow", horseSections, "FollowPitchSpeedRate", 0.0, 2.0, 0.05));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse follow delay", horseSections, "FollowStartTime", 0.0, 5.0, 0.1));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse default pitch", horseSections, "FollowDefaultPitch", -10.0, 30.0, 0.5));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse blend-in", Array.ConvertAll(horseSections, s => $"{s}/CameraBlendParameter"), "BlendInTime", 0.0, 3.0, 0.1));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse blend-out", Array.ConvertAll(horseSections, s => $"{s}/CameraBlendParameter"), "BlendOutTime", 0.0, 3.0, 0.1));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse pivot damping", Array.ConvertAll(horseSections, s => $"{s}/CameraDamping"), "PivotDampingMaxDistance", 0.0, 2.0, 0.05));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse sway", Array.ConvertAll(horseSections, s => $"{s}/OffsetByVelocity"), "OffsetLength", 0.0, 2.0, 0.1));
        horseSyncSliders.Add(BuildSharedSliderRow("Horse sway damp", Array.ConvertAll(horseSections, s => $"{s}/OffsetByVelocity"), "DampSpeed", 0.0, 2.0, 0.1));
        panel.Children.Add(WrapInCard("Horse state synchronization", horseSyncSliders.ToArray()));

        AdvCtrlSmoothGrid.Children.Add(panel);
    }

    private void BuildAdvCtrlSection_Aim()
    {
        var panel = new StackPanel();

        // Group aim sections by type
        var aimGroups = new[]
        {
            ("Lantern / Spotlight", new[] {
                ("Player_Basic_Default_Aim_Zoom", 2), ("Player_Basic_Default_Aim_Zoom", 3), ("Player_Basic_Default_Aim_Zoom", 4) }),
            ("Blinding Flash", new[] {
                ("Player_Taeguk_Aim", 2), ("Player_Taeguk_Aim", 3) }),
            ("Weapon Aim / Zoom", new[] {
                ("Player_Weapon_Aim_Zoom", 2), ("Player_Weapon_Aim_Zoom", 3),
                ("Player_Weapon_Zoom", 2), ("Player_Weapon_Zoom", 3) }),
            ("Bow", new[] {
                ("Player_Bow_Aim_Zoom", 2), ("Player_Bow_Aim_LockOn", 2) }),
            ("Glide / FreeFall", new[] {
                ("Glide_Kick_Aim_Zoom", 2), ("Player_Basic_FreeFall_Aim", 2) }),
        };

        foreach (var (groupName, entries) in aimGroups)
        {
            var aimGroupSliders = new List<UIElement>();
            foreach (var (sec, zl) in entries)
            {
                string shortName = $"{sec.Replace("Player_", "").Replace("_Aim_Zoom", "").Replace("_Aim", "")} ZL{zl}";

                var distRow = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "ZoomDistance", 0.5, 20.0, 0.1);
                if (distRow.Children[0] is TextBlock distLabel)
                    distLabel.Text = $"{shortName} Dist";
                aimGroupSliders.Add(distRow);

                var upRow = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "UpOffset", -2.0, 2.0, 0.1);
                if (upRow.Children[0] is TextBlock upLabel)
                    upLabel.Text = $"{shortName} Height";
                aimGroupSliders.Add(upRow);

                var rightRow = BuildSliderRow($"{sec}/ZoomLevel[{zl}]", "RightOffset", -1.0, 3.0, 0.05);
                if (rightRow.Children[0] is TextBlock rightLabel)
                    rightLabel.Text = $"{shortName} Shift";
                aimGroupSliders.Add(rightRow);
            }
            panel.Children.Add(WrapInCard(groupName, aimGroupSliders.ToArray()));
        }

        var traversalFramingSliders = new List<UIElement>();
        foreach (var (modKey, attr, min, max, step, labelText) in new[]
        {
            ("Player_Swim_Default/ZoomLevel[2]", "ZoomDistance", 0.5, 20.0, 0.1, "Swim ZL2 Dist"),
            ("Player_Swim_Default/ZoomLevel[2]", "UpOffset", -2.0, 2.0, 0.1, "Swim ZL2 Height"),
            ("Player_Basic_Climb/ZoomLevel[2]", "ZoomDistance", 0.5, 20.0, 0.1, "Climb ZL2 Dist"),
            ("Player_Basic_Climb/ZoomLevel[2]", "UpOffset", -2.0, 2.0, 0.1, "Climb ZL2 Height"),
            ("Player_Basic_Gliding/ZoomLevel[2]", "ZoomDistance", 0.5, 20.0, 0.1, "Glide ZL2 Dist"),
            ("Player_Basic_Gliding/ZoomLevel[2]", "UpOffset", -2.0, 2.0, 0.1, "Glide ZL2 Height"),
            ("Player_Basic_FreeFall/ZoomLevel[2]", "ZoomDistance", 0.5, 20.0, 0.1, "Freefall ZL2 Dist"),
            ("Player_Basic_FreeFall/ZoomLevel[2]", "UpOffset", -2.0, 2.0, 0.1, "Freefall ZL2 Height")
        })
        {
            var row = BuildSliderRow(modKey, attr, min, max, step);
            if (row.Children[0] is TextBlock label)
                label.Text = labelText;
            traversalFramingSliders.Add(row);
        }
        panel.Children.Add(WrapInCard("Traversal framing", traversalFramingSliders.ToArray()));

        AdvCtrlAimGrid.Children.Add(panel);
    }

    private ModificationSet BuildAdvancedControlsModSet()
    {
        var mods = new Dictionary<string, Dictionary<string, (string, string)>>();

        foreach (var (fullKey, slider) in _advCtrlSliders)
        {
            if (slider == null) continue;
            _advCtrlVanilla.TryGetValue(fullKey, out string? vanillaStr);
            double vanillaVal = double.TryParse(vanillaStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double vv) ? vv : double.NaN;

            if (Math.Abs(slider.Value - vanillaVal) < 0.001) continue; // unchanged

            int dotIdx = fullKey.LastIndexOf('.');
            if (dotIdx < 0) continue;
            string modKey = fullKey[..dotIdx];
            string attr = fullKey[(dotIdx + 1)..];

            if (!mods.TryGetValue(modKey, out var attrs))
            {
                attrs = new Dictionary<string, (string, string)>();
                mods[modKey] = attrs;
            }
            attrs[attr] = ("SET", $"{slider.Value:F2}");
        }

        return new ModificationSet { ElementMods = mods, FovValue = 0 };
    }

    private void AdvCtrlUpdateChangedLabel()
    {
        if (!IsLoaded) return;
        int changed = _advCtrlSliders.Values
            .Where(s => s != null)
            .Count(s =>
            {
                string? key = _advCtrlSliders.FirstOrDefault(kv => kv.Value == s).Key;
                if (key == null) return false;
                _advCtrlVanilla.TryGetValue(key, out string? vs);
                double vv = double.TryParse(vs, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : double.NaN;
                return Math.Abs(s.Value - vv) > 0.001;
            });
    }

    private void ApplyAdvCtrlSearch()
    {
        if (!IsLoaded) return;

        string search = AdvCtrlSearchBox?.Text?.Trim() ?? "";
        bool hasSearch = !string.IsNullOrWhiteSpace(search);

        if (hasSearch)
        {
            AdvSectionA.IsExpanded = true;
            AdvSectionB.IsExpanded = true;
            AdvSectionC.IsExpanded = true;
            AdvSectionD.IsExpanded = true;
            AdvSectionE.IsExpanded = true;
            AdvSectionF.IsExpanded = true;
            AdvSectionG.IsExpanded = true;
        }

        ApplyAdvCtrlSearchToElement(AdvCtrlOnFootGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlMountGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlGlobalGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlSpecialMountGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlCombatGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlSmoothGrid, search);
        ApplyAdvCtrlSearchToElement(AdvCtrlAimGrid, search);
    }

    private bool ApplyAdvCtrlSearchToElement(UIElement element, string search)
    {
        bool hasSearch = !string.IsNullOrWhiteSpace(search);

        if (!hasSearch)
        {
            element.Visibility = Visibility.Visible;

            if (element is Border visibleBorder && visibleBorder.Child is UIElement borderChild)
                ApplyAdvCtrlSearchToElement(borderChild, search);
            else if (element is Panel visiblePanel)
                foreach (UIElement child in visiblePanel.Children)
                    ApplyAdvCtrlSearchToElement(child, search);

            return true;
        }

        if (element is Grid row && row.Tag is string rowTag)
        {
            bool match = rowTag.Contains(search, StringComparison.OrdinalIgnoreCase);
            row.Visibility = match ? Visibility.Visible : Visibility.Collapsed;
            return match;
        }

        if (element is TextBlock textBlock)
        {
            bool match = textBlock.Text.Contains(search, StringComparison.OrdinalIgnoreCase);
            textBlock.Visibility = match ? Visibility.Visible : Visibility.Collapsed;
            return match;
        }

        if (element is Border border)
        {
            bool titleMatch = border.Tag is string borderTag &&
                              borderTag.Contains(search, StringComparison.OrdinalIgnoreCase);
            bool childMatch = border.Child is UIElement borderChild &&
                              ApplyAdvCtrlSearchToElement(borderChild, search);
            bool visible = titleMatch || childMatch;
            border.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            return visible;
        }

        if (element is Panel panel)
        {
            bool anyVisible = false;
            foreach (UIElement child in panel.Children)
                anyVisible |= ApplyAdvCtrlSearchToElement(child, search);

            if (panel != AdvCtrlPanel)
                panel.Visibility = anyVisible ? Visibility.Visible : Visibility.Collapsed;

            return anyVisible;
        }

        return true;
    }

    private void OnAdvCtrlSearchChanged(object sender, TextChangedEventArgs e)
    {
        _advCtrlSearchDebounceTimer?.Stop();
        _advCtrlSearchDebounceTimer?.Start();
    }

    private void OnAdvCtrlClearSearch(object sender, RoutedEventArgs e)
    {
        AdvCtrlSearchBox.Clear();
        ApplyAdvCtrlSearch();
    }

    private void OnAdvCtrlApply(object sender, RoutedEventArgs e)
    {
        SetStatus("UCM Fine Tune no longer writes to game files in v3. Use Export JSON instead.", "Warn");
    }

    private void OnAdvCtrlLoadFromSimple(object sender, RoutedEventArgs e)
    {
        if (_advCtrlSliders.Count == 0) return;
        try
        {
            var modSet = BuildCurrentSimpleModSet();
            string vanillaXml = CameraMod.ReadVanillaXml(_gameDir);
            string modifiedXml = CameraMod.ApplyModifications(vanillaXml, modSet);
            var rows = CameraMod.ParseXmlToRows(modifiedXml);
            var lookup = new Dictionary<string, string>();
            foreach (var r in rows) lookup[r.FullKey] = r.Value;

            _suppressEvents = true;
            foreach (var (key, slider) in _advCtrlSliders)
            {
                if (slider == null) continue;
                if (lookup.TryGetValue(key, out string? val) &&
                    double.TryParse(val, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double d))
                    slider.Value = Math.Clamp(d, slider.Minimum, slider.Maximum);
            }
            _suppressEvents = false;
            AdvCtrlUpdateChangedLabel();
            SaveCurrentUiState(immediate: true);
            SetStatus("Loaded values from UCM Quick.", "Success");
        }
        catch (Exception ex)
        {
            _suppressEvents = false;
            SetStatus($"Load failed: {ex.Message}", "Error");
        }
    }

    private void OnAdvCtrlResetVanilla(object sender, RoutedEventArgs e)
    {
        if (_advCtrlSliders.Count == 0) return;
        _suppressEvents = true;
        foreach (var (key, slider) in _advCtrlSliders)
        {
            if (slider == null) continue;
            if (_advCtrlVanilla.TryGetValue(key, out string? vs) &&
                double.TryParse(vs, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d))
                slider.Value = Math.Clamp(d, slider.Minimum, slider.Maximum);
        }
        _suppressEvents = false;
        AdvCtrlUpdateChangedLabel();
        SaveCurrentUiState(immediate: true);
        SetStatus("Reset all UCM Fine Tune controls to vanilla.", "Success");
    }

    private void AdvCtrlRefreshPresetCombo()
    {
        // AdvCtrlPresetCombo removed from XAML — preset selection is sidebar-only now.
    }

    private void ApplySessionXmlToAdvancedControls(string xmlText)
    {
        if (_advCtrlSliders.Count == 0) return;

        var rows = CameraMod.ParseXmlToRows(xmlText);
        var lookup = new Dictionary<string, string>();
        foreach (var r in rows) lookup[r.FullKey] = r.Value;

        // Shared sliders: multiple keys point to the same Slider object (e.g. all on-foot sections
        // share one ZoomDistance slider). We must only set each physical slider once, using the first
        // matching key (the representative section), otherwise the last iterated key wins and may
        // revert the slider to vanilla if that section wasn't modified by the style.
        var alreadySet = new HashSet<Slider>();
        _suppressEvents = true;
        try
        {
            foreach (var (key, slider) in _advCtrlSliders)
            {
                if (slider == null || alreadySet.Contains(slider)) continue;
                if (lookup.TryGetValue(key, out string? val) &&
                    double.TryParse(val, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double d))
                {
                    double clamped = Math.Clamp(d, slider.Minimum, slider.Maximum);
                    slider.Value = clamped;
                    alreadySet.Add(slider);

                    if (_advCtrlValueLabels.TryGetValue(key, out var valueLabel))
                    {
                        valueLabel.Text = $"{clamped:F2}";
                        _advCtrlVanilla.TryGetValue(key, out string? vanStr);
                        double vanVal = double.TryParse(vanStr,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double vv) ? vv : clamped;
                        valueLabel.Foreground = Math.Abs(clamped - vanVal) > 0.001
                            ? _accentBrush
                            : _textDimBrush;
                    }
                }
            }
        }
        finally
        {
            _suppressEvents = false;
        }

        AdvCtrlUpdateChangedLabel();
        ApplyAdvCtrlSearch();
    }

}
