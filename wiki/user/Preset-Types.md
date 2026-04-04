# Preset Types

Starting with UCM v3.1.0, every preset has a **type** that determines how UCM handles it. The preset type affects which editors are available, whether UCM applies its camera rules, and how the preset behaves when you make changes.

There are two preset types:

- **Managed by UCM** (the default)
- **Full Manual Control**

This page explains both types in detail, when to use each one, and how imported presets are handled.

---

## Table of Contents

- [The Two Preset Types at a Glance](#the-two-preset-types-at-a-glance)
- [Managed by UCM](#managed-by-ucm)
- [Full Manual Control](#full-manual-control)
- [How to Choose When Creating a New Preset](#how-to-choose-when-creating-a-new-preset)
- [How Raw Imports Are Handled](#how-raw-imports-are-handled)
- [How .ucmpreset Imports Are Handled](#how-ucmpreset-imports-are-handled)
- [Can I Change a Preset's Type After Creation?](#can-i-change-a-presets-type-after-creation)
- [How the Preset Type Is Stored](#how-the-preset-type-is-stored)
- [Frequently Asked Questions](#frequently-asked-questions)

---

## The Two Preset Types at a Glance

| | Managed by UCM | Full Manual Control |
|---|---|---|
| **Available editors** | UCM Quick, Fine Tune, God Mode | God Mode only |
| **UCM camera rules** | Applied automatically | Not applied |
| **Starting values** | Vanilla (game defaults) | Vanilla (game defaults) |
| **Best for** | Most users; intuitive slider control with helpful automation | Advanced users; exact XML value editing with no interference |
| **FoV consistency** | UCM applies FoV across all sections automatically | You must set FoV values manually per section |
| **Smooth transitions** | Steadycam available | You must adjust blend values manually |
| **Lock-on scaling** | UCM scales lock-on distances proportionally | You must set lock-on values manually |
| **Mount sync** | Mount Camera checkbox available | You must sync mount values manually |

---

## Managed by UCM

This is the **default preset type** and the one recommended for most players.

### What It Means

When a preset is Managed by UCM, the application acts as an intelligent intermediary between you and the raw camera XML. You interact with user-friendly sliders and checkboxes, and UCM translates your choices into the correct XML values using a set of camera rules.

### What UCM's Camera Rules Do

UCM applies the following rules automatically when you use the Quick sliders:

| Rule | What It Does |
|---|---|
| **FoV consistency** | When you change the FoV dropdown, UCM applies the value uniformly across all `Player_`, `Cinematic_`, and `Glide_` sections. You do not need to manually update dozens of XML entries. |
| **Smooth transitions** | When Steadycam is enabled, UCM normalizes blend timing and velocity sway across 30+ camera states, preventing jerky camera movement. |
| **Lock-on scaling** | When you adjust the Lock-on Zoom slider, UCM scales lock-on camera distances proportionally relative to your on-foot distance. The math is handled for you. |
| **Mount sync** | When Mount Camera is checked, UCM matches the mount camera height to your player camera height automatically. |
| **Distance proportional scaling** | When you change the Distance slider, UCM adjusts ZoomDistance on multiple zoom levels proportionally, keeping the character at the same relative screen position. |
| **Centered Camera propagation** | When you enable Centered Camera, UCM sets RightOffset to 0 across 150+ camera states. You do not need to find and zero out each one individually. |

### Which Editors Are Available

All three editors are available:

- **UCM Quick**: Full access to all sliders, checkboxes, dropdowns, and preview panels.
- **Fine Tune**: Full access to all ~150 sliders organized by camera category. Some sliders may be greyed out if Steadycam is controlling them.
- **God Mode**: Full access to the raw XML DataGrid. You can view and edit any value. Note that if you change a value in God Mode that UCM normally manages, your manual change may be overwritten the next time you adjust a related Quick slider.

### When to Use This Type

- You are new to UCM or camera modding in general.
- You want the convenience of sliders and checkboxes that "just work."
- You want FoV, transitions, lock-on, and mount sync handled automatically.
- You want to start with the UCM Quick tab and only go deeper when needed.

This is the right choice for the vast majority of users.

---

## Full Manual Control

This is the advanced preset type for users who want direct, unfiltered access to the camera XML.

### What It Means

When a preset is set to Full Manual Control, UCM does **not** apply any camera rules on top of your changes. You interact directly with the raw XML values through God Mode. What you set is exactly what gets written to the game, with no adjustments, scaling, or propagation.

### Which Editors Are Available

Only **God Mode** is available.

- **UCM Quick**: Disabled. Clicking this tab shows a message explaining that Quick sliders are not available for Full Manual Control presets.
- **Fine Tune**: Disabled. Same explanation message.
- **God Mode**: Full access. This is where you make all your changes.

### What You Give Up

Without UCM's camera rules, the following conveniences are gone:

| Feature | What You Must Do Manually |
|---|---|
| **FoV consistency** | If you want to change FoV, you need to find and update the FoV attribute in every relevant section (Player_, Cinematic_, Glide_) yourself. |
| **Steadycam** | If you want smooth transitions, you need to find and adjust each blend timing and velocity sway value individually across 30+ states. |
| **Lock-on scaling** | If you want to adjust lock-on camera distance, you need to calculate and set the values yourself. |
| **Mount sync** | If you want mount and player cameras to match, you need to set the values identically in both sections yourself. |
| **Distance proportional scaling** | If you change the camera distance, you need to update ZoomDistance on each relevant zoom level and ensure the proportions are correct. |
| **Centered Camera** | If you want a centered camera, you need to set RightOffset to 0 in every relevant section (150+ entries). |

### When to Use This Type

- You are experienced with camera XML modding and know exactly which values to change.
- You want to match specific XML values from another camera mod (for example, reproducing settings you found in a guide or forum post).
- You are debugging a camera issue and want to test exact raw values without UCM modifying them.
- You want guaranteed 1:1 correspondence between what you type and what the game reads.
- You are porting camera settings from another tool or game version.

### Starting Values

A new Full Manual Control preset starts with the complete vanilla camera XML, just like a Managed preset. The difference is that UCM does not layer any rules on top as you make edits.

---

## How to Choose When Creating a New Preset

When you click the New Preset button, UCM asks you to choose between "Managed by UCM" and "Full Manual Control."

**Choose Managed by UCM if:**
- You are not sure which to pick (this is the safe default)
- You want slider controls and preview panels
- You want UCM to handle the tedious parts (FoV propagation, transition smoothing, mount sync, etc.)
- You plan to use the UCM Quick tab as your primary editing interface

**Choose Full Manual Control if:**
- You specifically need to set exact raw XML values
- You are trying to reproduce settings from another source that specifies exact attribute values
- You understand the camera XML structure and prefer working with it directly
- You do not want any automated adjustments applied to your values

If you are reading this page and are unsure, choose **Managed by UCM**. You can always access God Mode to make raw edits even with a Managed preset. The only difference is that UCM's rules will be active in the background.

---

## How Raw Imports Are Handled

When you import camera data from a **raw source** (not a `.ucmpreset` file), UCM automatically treats the imported preset as **Full Manual Control** with no UCM rules applied.

Raw import sources include:

| Import Source | File Type |
|---|---|
| Mod Manager Package | Folder with `manifest.json` |
| Camera XML File | `.xml` file |
| PAZ Archive | `.paz` file |

### Why Raw Imports Become Full Manual Control

Raw imports contain finished camera XML that was not created by UCM. UCM has no way to reverse-engineer which slider positions, checkboxes, or rules would have produced that XML. Trying to apply UCM rules on top of unknown XML could conflict with the values already in the data, producing unexpected or broken results.

By treating raw imports as Full Manual Control:

- The imported XML is preserved exactly as it was, with no modifications.
- You can view and edit every value in God Mode.
- UCM does not apply any rules that might conflict with the imported data.
- The imported camera behaves exactly as the original creator intended.

### What This Means for You

If you import a camera XML from another mod and want to tweak it:

- Use God Mode to make your changes. Quick and Fine Tune are not available.
- If you want slider access, you have two options:
  1. Create a new Managed by UCM preset and use the imported preset as a reference (open it in God Mode to see its values, then set similar values on your Managed preset using sliders).
  2. Look for a `.ucmpreset` version of the same camera mod, which would give you full slider access.

### Where Raw Imports Are Saved

All raw imports are saved to the `import_presets/` folder and appear under the **Imported** group in the sidebar.

---

## How .ucmpreset Imports Are Handled

When you import a `.ucmpreset` file, the preset **retains its original type**. This means:

- If the `.ucmpreset` was created as a Managed by UCM preset, it remains Managed by UCM after import. You get full access to Quick, Fine Tune, and God Mode.
- If the `.ucmpreset` was created as a Full Manual Control preset, it remains Full Manual Control after import. Only God Mode is available.

This is the key advantage of the `.ucmpreset` format over raw XML or PAZ imports. The preset format preserves all of UCM's internal data, including the type, slider positions, and rule settings.

### Where .ucmpreset Imports Are Saved

Imported `.ucmpreset` files are saved to the `my_presets/` folder (not `import_presets/`) and appear under **My Presets** in the sidebar. This is because `.ucmpreset` files are full UCM presets with complete data, unlike raw imports that only contain camera XML.

---

## Can I Change a Preset's Type After Creation?

No. The preset type is set when the preset is created (or imported) and cannot be changed afterward.

If you have a Full Manual Control preset and want slider access, your best option is to:

1. Open the Full Manual Control preset and go to God Mode.
2. Note the values you want to keep (or use the "Modified only" filter to see just your changes).
3. Create a new Managed by UCM preset.
4. Use the Quick and Fine Tune sliders to approximate those values.
5. Use God Mode on the new preset to fine-tune any values that need to match exactly.

If you have a Managed by UCM preset and wish it were Full Manual Control, you can use God Mode on the Managed preset to make raw edits. The only difference is that UCM rules are still active in the background. If a Quick slider is adjusted later, it might overwrite some of your God Mode changes. If this is a concern, export the preset as XML, then re-import it (which creates a Full Manual Control copy).

---

## How the Preset Type Is Stored

The preset type is stored inside the `.ucmpreset` file as part of its JSON data. It is a simple field that UCM reads when loading the preset. The type persists across:

- UCM sessions (closing and reopening UCM)
- Moving the preset file to a different folder
- Sharing the file with another UCM user
- Exporting and re-importing
- UCM updates (the field is preserved by newer versions)

You do not need to manage this field yourself. UCM handles it automatically based on your choice at creation time (or the import method used).

---

## Frequently Asked Questions

### Can I use God Mode on a Managed by UCM preset?

Yes. All three editors are available for Managed presets. You can freely use God Mode to view or change any raw value. Just be aware that if you later adjust a Quick slider that affects the same value, UCM's rules may overwrite your God Mode change.

### Will UCM rules overwrite my God Mode edits?

Only if you subsequently change a related Quick slider. For example, if you manually set a specific ZoomDistance value in God Mode and then move the Distance slider on UCM Quick, the Distance slider's proportional scaling will recalculate and overwrite your manual ZoomDistance value.

If you make God Mode edits and then leave the Quick sliders alone, your edits are preserved.

### Why can't I use Quick sliders on a raw import?

Because UCM cannot figure out what slider positions would produce the XML values in the imported data. The sliders work by applying rules to generate XML, not the other way around. Without knowing the original slider positions, UCM cannot present meaningful slider controls.

### I imported a .ucmpreset and I have full slider access. Is that normal?

Yes. `.ucmpreset` files contain the slider positions and rule settings in addition to the raw XML, so UCM can load them into the editors. This is the intended behavior and the main advantage of sharing presets in the `.ucmpreset` format.

### What happens if I export a Full Manual Control preset as .ucmpreset and someone else imports it?

The other person will also get a Full Manual Control preset with God Mode only. The preset type travels with the `.ucmpreset` file.

### I just want the simplest experience. What should I pick?

Choose **Managed by UCM**. Use the UCM Quick tab for your main adjustments. You do not need to think about God Mode or raw XML unless you want to.
