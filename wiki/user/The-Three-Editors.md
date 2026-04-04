# The Three Editors

UCM gives you three different ways to edit your camera, each offering a different level of control. Think of them as three zoom levels on the same data: UCM Quick gives you the big-picture sliders, Fine Tune lets you adjust individual camera states, and God Mode shows you every single raw value the game uses.

All three editors work on the same underlying camera data. Changes you make in one editor are reflected in the others. They are not separate systems; they are three views into the same preset.

---

## Table of Contents

- [Overview: Which Editor Should I Use?](#overview-which-editor-should-i-use)
- [UCM Quick](#ucm-quick)
- [Fine Tune](#fine-tune)
- [God Mode](#god-mode)
- [How Edits Flow Between Editors](#how-edits-flow-between-editors)
- [When Editors Are Disabled](#when-editors-are-disabled)

---

## Overview: Which Editor Should I Use?

| Editor | Best For | Skill Level | Number of Controls |
|---|---|---|---|
| **UCM Quick** | Quickly dialing in your preferred camera feel. Distance, height, FoV, and major toggles. | Beginner | About 10 controls |
| **Fine Tune** | Adjusting specific camera states independently. Want the combat camera different from the exploration camera? This is where you do it. | Intermediate | About 150 sliders |
| **God Mode** | Editing any raw XML value the game uses. Full control with no guardrails. | Advanced | Every value in the camera XML |

**If you are new to UCM**, start with UCM Quick. It covers the settings that make the biggest visual difference and is designed to be approachable. You can always go deeper into Fine Tune or God Mode later.

**If you want to tweak a specific situation** (like "the camera during horseback combat is too close"), use Fine Tune to find and adjust that specific camera state.

**If you know exactly which XML attribute you want to change**, or if you are trying to match values from another camera mod, use God Mode.

---

## UCM Quick

UCM Quick is the main tab and the first thing you see when you open UCM. It is designed for fast, intuitive camera adjustments with immediate visual feedback.

### What Is on This Tab

UCM Quick contains the following controls:

#### Sliders

| Slider | Range | What It Does |
|---|---|---|
| **Distance** | 1.5 to 12 | Controls how far the camera sits behind your character. Low values bring the camera in close; high values pull it far back for a wider view. |
| **Height** | -1.6 to 1.5 | Controls the vertical position of the camera. Negative values lower the camera (below the character's shoulders), positive values raise it (above the character's head). |
| **Horizontal Shift** | -3 to 3 | Adjusts the left-right offset of the camera. At 0, the camera uses the game's default side bias. Moving toward positive values shifts the camera toward center. Negative values push the camera further to the left. |
| **Lock-on Zoom** | -60% to +60% | Controls how much the camera zooms when you lock onto an enemy. 0% means the lock-on camera matches your normal on-foot distance. Positive values pull back wider during lock-on. Negative values zoom in closer for a more cinematic, focused feel. |

#### Dropdown

| Control | Options | What It Does |
|---|---|---|
| **Field of View (FoV)** | 0 to +40 degrees (in steps) | Adds extra degrees of field of view on top of the game's base 40-degree FoV. Selecting "+20" gives you 60 degrees total. Higher values show more of the world but can start to look distorted at the extremes. |

#### Checkboxes

| Checkbox | What It Does |
|---|---|
| **Centered Camera** | Removes the over-the-shoulder offset across 150+ camera states. The camera sits directly behind your character instead of off to one side. When enabled, the Horizontal Shift slider is locked to 0. |
| **Mount Camera** | Syncs your horse/mount camera height to match your player camera height setting. Without this, the mount camera might use a different height than your on-foot camera. |
| **Steadycam** | Smooths camera transitions across 30+ camera states. Normalizes blend timing and velocity sway so the camera does not jerk around when you start moving, stop moving, or change directions. When enabled, it takes control of certain Fine Tune sliders (those sliders become greyed out). |

#### Preview Panels

| Panel | What It Shows |
|---|---|
| **Camera Preview** | A live visualization showing where your camera sits relative to the character model. As you move sliders, this updates in real time so you can see the effect before launching the game. |
| **FoV Preview** | A live visualization comparing your current field of view to the vanilla default. Helps you gauge how much wider (or narrower) your view will be. |

### How the Sliders Work Technically

You do not need to understand the technical details to use UCM Quick, but here is what happens behind the scenes for the curious:

- **Distance** controls the `ZoomDistance` attribute on ZoomLevel 2 (idle distance), ZoomLevel 3 (medium distance), and ZoomLevel 4 (far distance). UCM uses proportional scaling so that changing the distance keeps your character at the same relative position on screen.
- **Height** controls the `UpOffset` attribute on ZoomLevel 2 and 3.
- **Horizontal Shift** applies a delta (change) to the `RightOffset` value. The vanilla game has a slight rightward offset for the over-the-shoulder look.
- **FoV** adds your chosen value on top of the vanilla FoV values. It is applied universally across all `Player_`, `Cinematic_`, and `Glide_` sections in the camera XML, so every camera state gets the same FoV boost.

### Undo Support

UCM Quick supports **Ctrl+Z** to undo changes, up to 20 steps back. This only works on the UCM Quick tab. If you move a slider too far and want to revert, just press Ctrl+Z.

---

## Fine Tune

Fine Tune is the second tab. It gives you access to around 150 individual sliders, organized into logical groups. This is where you go when you want to adjust specific camera behaviors without touching others.

### What Is on This Tab

Fine Tune presents its controls in **bordered card sections**, each covering a specific aspect of the camera:

| Card Section | What It Contains |
|---|---|
| **On Foot** | Camera settings for when your character is walking or running on foot. Distance, height, offsets, zoom levels, and related values for the standard exploration camera. |
| **Mount** | Camera settings for when your character is riding a horse or other mount. Distance, height, offsets, and transition behavior specific to mounted movement. |
| **Global** | Settings that apply across multiple camera states. Values that affect the camera regardless of whether you are on foot, mounted, or in combat. |
| **Special Mounts** | Camera settings for unique or special mount types that behave differently from the standard horse. |
| **Combat** | Camera settings during combat encounters. This includes lock-on distances, combat camera angles, and how the camera behaves when you are fighting enemies. |
| **Smoothing** | Controls for camera transition timing, blend speeds, and interpolation. These determine how quickly or slowly the camera moves from one state to another. |
| **Aim** | Camera settings for when you are aiming a ranged weapon or using a targeting mode. Includes aim zoom, aim offsets, and aim camera positioning. |

### How Each Slider Looks

Every slider in Fine Tune shows two pieces of information:

- **Current value**: The value your preset is currently using (what will be installed to the game).
- **Vanilla value**: The value the game uses by default, shown for comparison.

This makes it easy to see exactly how far you have deviated from the default for any given setting. If a slider shows "Current: 5.2 / Vanilla: 4.0", you know your camera distance for that state is about 30% farther than default.

### Greyed-Out Sliders (Steadycam)

Some Fine Tune sliders may appear **greyed out** and uneditable. This happens when the **Steadycam** checkbox is enabled on the UCM Quick tab. Steadycam takes control of certain smoothing and transition values to ensure consistent, jerk-free camera behavior. The specific sliders it controls become locked because changing them would conflict with Steadycam's adjustments.

If you hover over a greyed-out slider, a **tooltip** will appear explaining that Steadycam is controlling that value. To regain manual control of those sliders, go back to UCM Quick and uncheck Steadycam.

### Search Box

At the top of the Fine Tune tab, there is a **search box**. Type any keyword to filter the visible sliders. For example:

- Type "zoom" to see only sliders related to zoom distances
- Type "offset" to see only offset-related values
- Type "combat" to narrow down to combat camera settings
- Type "blend" to find smoothing and transition timing controls

The search filters across all card sections, hiding sections that have no matching sliders and showing only the ones that match your search term.

### When to Use Fine Tune

Fine Tune is ideal when you want to:

- Make the combat camera behave differently from the exploration camera
- Adjust mount camera settings independently from on-foot settings
- Tweak aim camera zoom or positioning without affecting anything else
- Fine-tune smoothing and transition speeds for specific situations
- Compare your current values against vanilla for any specific camera state

---

## God Mode

God Mode is the third tab. It shows you the complete, raw camera XML data in a spreadsheet-like DataGrid. Every value the game uses for camera behavior is visible and editable here. There are no abstractions, no grouping by theme, and no UCM rules applied on top. You see exactly what the game sees.

### What Is on This Tab

God Mode displays a **DataGrid** (a table) with the following columns:

| Column | What It Shows |
|---|---|
| **Section Name** | The XML section this value belongs to (for example, `Player_Default`, `Player_Combat`, `Player_Horse`). Every section that starts with `Player_` from the camera XML is listed. |
| **Sub-Element** | The XML sub-element within that section (for example, `ZoomLevel`, `CameraShake`, `BlendOption`). |
| **Attribute** | The specific XML attribute name (for example, `ZoomDistance`, `UpOffset`, `RightOffset`, `BlendTime`). |
| **Vanilla Value** | The original game default value for this attribute. |
| **Current Value** | Your preset's current value. If you have modified it, this will differ from the vanilla value. |

You can directly click on any **Current Value** cell and type a new number to change it.

### Filtering

God Mode has two filtering options:

- **Section prefix filter**: Type a section name prefix (like "Player_Combat" or "Player_Horse") to show only rows from sections matching that prefix. This is useful when the game has dozens of camera sections and you only care about one.
- **Modified only**: A checkbox that, when enabled, hides all rows where the current value matches the vanilla value. This shows you only the values you have actually changed, making it easy to review your modifications at a glance.

### Exporting and Importing God Mode Presets

God Mode has its own export and import capability:

- **Export**: Saves the current God Mode data (all your raw modifications) to a file. This is useful for creating a snapshot of your raw edits.
- **Import**: Loads a previously exported God Mode preset, applying those raw values to your current preset.

This is separate from the main Export/Import system (which handles .ucmpreset, XML, PAZ, and JSON formats). God Mode export/import is specifically for sharing or backing up raw value sets.

### When to Use God Mode

God Mode is ideal when you want to:

- See every single camera value the game uses, with nothing hidden
- Edit a specific XML attribute that is not exposed in Quick or Fine Tune
- Match exact values from another camera mod (you know the attribute names and values you want)
- Debug unexpected camera behavior by inspecting the raw data
- Make highly specific changes to obscure camera states
- Work with a raw import (XML, PAZ, or mod manager package) where Quick and Fine Tune are not available

### A Word of Caution

God Mode gives you full, unrestricted access to camera values. UCM does not validate or sanity-check your changes in God Mode. If you set a value that the game does not expect (like a negative zoom distance or an extremely large offset), the game may behave unpredictably or crash on load. If that happens, verify your game files on Steam/Epic and start over.

---

## How Edits Flow Between Editors

All three editors operate on the same underlying camera data. Here is how changes propagate:

**UCM Quick to Fine Tune and God Mode:**
When you move a slider on UCM Quick (for example, increasing Distance to 8), UCM updates the relevant XML attributes behind the scenes. If you then switch to Fine Tune, you will see the affected sliders reflect the new distance values. If you switch to God Mode, you will see the specific `ZoomDistance` attributes on ZoomLevel 2, 3, and 4 updated to their new values.

**Fine Tune to God Mode:**
When you adjust a slider in Fine Tune, the corresponding XML attribute is updated. Switching to God Mode will show that attribute's Current Value changed.

**God Mode to UCM Quick and Fine Tune:**
When you edit a raw value in God Mode, that change is reflected if you switch to Fine Tune (the corresponding slider will show the new value) or UCM Quick (if the value is one that a Quick slider controls, the slider position will update).

**The key point:** There is one set of camera data, and the three editors are three ways to view and modify it. You can freely switch between tabs without losing changes. Nothing is stored separately per editor.

### Where UCM Rules Apply

For **Managed by UCM** presets (the default type), UCM applies certain rules when you use the Quick sliders:

- FoV changes are applied consistently across all camera sections
- Distance changes use proportional scaling to maintain character screen position
- Steadycam normalizes transition timing across 30+ states
- Mount Camera syncs mount height to player height
- Centered Camera zeroes out RightOffset across 150+ states
- Lock-on Zoom scales lock-on distances proportionally

These rules make the Quick sliders behave intuitively. But in Fine Tune and God Mode, you are working closer to the raw data, so you have more direct control (and more responsibility to keep things consistent).

For **Full Manual Control** presets, no UCM rules are applied at all. Quick and Fine Tune tabs are disabled, and you only have God Mode. See [Preset Types](Preset-Types.md) for details.

---

## When Editors Are Disabled

In certain situations, some editor tabs may be disabled (greyed out or showing an explanatory message when you click them):

| Situation | Available Editors | Why |
|---|---|---|
| **Managed by UCM preset** (normal) | Quick, Fine Tune, God Mode | All editors available. This is the default experience. |
| **Full Manual Control preset** | God Mode only | You chose full manual control when creating the preset. No UCM rules, no sliders. |
| **Raw import** (XML, PAZ, or Mod Manager package) | God Mode only | UCM cannot reconstruct slider positions from raw XML data. The imported data did not come from UCM, so only direct value editing is possible. |
| **Locked preset** | All editors visible but read-only | You can view values but cannot change them. Unlock the preset (padlock icon) or duplicate it to create an editable copy. |

If you encounter greyed-out editors and are not sure why, check:

1. Is the preset locked? Look for the padlock icon.
2. Is it a raw import? Check the sidebar group (Imported presets are raw imports).
3. Is it a Full Manual Control preset? Check the preset type.

For more on preset types and how they affect editor availability, see [Preset Types](Preset-Types.md).

---

## Summary

| | UCM Quick | Fine Tune | God Mode |
|---|---|---|---|
| **Controls** | ~10 sliders, checkboxes, dropdown | ~150 sliders in themed cards | Full DataGrid of every XML value |
| **Organization** | Single page, all visible at once | Grouped into On Foot, Mount, Global, Special Mounts, Combat, Smoothing, Aim | Flat table, filterable by section or modified-only |
| **Previews** | Camera Preview, FoV Preview | None | None |
| **Search** | Not needed (few controls) | Search box to filter sliders | Section prefix filter, modified-only checkbox |
| **Undo** | Ctrl+Z (up to 20 steps) | No dedicated undo | No dedicated undo |
| **UCM Rules** | Applied automatically | Partially applied (Steadycam can lock sliders) | No rules applied |
| **Vanilla comparison** | Shown in preview panels | Each slider shows vanilla vs. current | Vanilla column in the DataGrid |
| **Best for** | Quick adjustments, beginners | Situation-specific tuning | Raw editing, debugging, advanced users |
