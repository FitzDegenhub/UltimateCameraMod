# Importing Presets

UCM lets you import camera presets from four different sources. This page walks you through each one, explains what happens behind the scenes, and covers the limitations you should know about.

---

## Overview

| Import Source | File / Folder Type | Where It Gets Saved | Editing Modes Available |
|---|---|---|---|
| Mod Manager Package | Folder containing `manifest.json` | `import_presets/` | God Mode only |
| Camera XML File | `.xml` file | `import_presets/` | God Mode only |
| PAZ Archive | `.paz` file | `import_presets/` | God Mode only |
| UCM Preset | `.ucmpreset` file | `my_presets/` | Quick, Fine Tune, and God Mode |

The key distinction here is between **raw imports** (Mod Manager, XML, PAZ) and **UCM Preset imports**. Raw imports contain camera data that UCM did not originally create, so UCM can only offer God Mode editing for them. UCM Presets were created inside UCM and carry all the metadata needed for full slider control.

---

## How to Open the Import Dialog

1. In UCM's main window, look for the **Import** button in the sidebar.
2. Click it to open the Import dialog.
3. You will see four options presented as buttons or tabs, one for each import type.
4. Select the type that matches what you want to import.

---

## Import Type 1: Mod Manager Package

A Mod Manager Package is a folder that was created for use with Crimson Desert JSON Mod Manager or CDUMM. These folders have a specific structure:

```
MyMod/
  manifest.json
  files/
    playercamerapreset.xml
```

### Step by Step

1. Click **Import** in the sidebar.
2. Select **Mod Manager Package**.
3. A folder browser will open. Navigate to the mod's root folder (the one that contains `manifest.json`).
4. Select the folder and confirm.
5. UCM reads the `manifest.json` file to pull out the mod's **name**, **author**, and **description**.
6. UCM then locates the `playercamerapreset.xml` inside the `files/` subdirectory.
7. A **metadata dialog** appears showing you the name, author, description, and URL fields. You can review and edit any of these before saving.
8. Click **Save** (or **OK**) to finish the import.

### What Happens After Import

- The preset is saved into UCM's `import_presets/` folder.
- It appears in the sidebar under the **Imported** group.
- Because this is a raw import, **Quick and Fine Tune tabs are disabled**. If you try to switch to those tabs, a dialog will explain that only God Mode editing is available for imported presets.
- You can still make changes in God Mode and then export the result in any format.

### Tips

- Make sure the folder you select actually contains a `manifest.json` at the top level. If it does not, the import will fail.
- The `playercamerapreset.xml` must be inside a `files/` subdirectory within that folder. This is the standard structure used by Crimson Desert mod managers.

---

## Import Type 2: Camera XML File

This is the simplest import type. You are importing a raw `playercamerapreset.xml` file, or any `.xml` file that contains Crimson Desert camera data.

### Step by Step

1. Click **Import** in the sidebar.
2. Select **Camera XML File**.
3. A file browser will open. Navigate to your `.xml` file and select it.
4. A **metadata dialog** appears. Since a raw XML file has no embedded name or author, all fields will be blank. Fill in at least a name so you can identify the preset later. You can also add author, description, and URL.
5. Click **Save** to finish the import.

### What Happens After Import

- The preset is saved into UCM's `import_presets/` folder.
- It appears in the sidebar under the **Imported** group.
- Only **God Mode** editing is available. Quick and Fine Tune are disabled with a clear explanation.

### Where Would You Get a Raw XML File?

- Someone might share a `playercamerapreset.xml` they extracted manually from the game files.
- You might have exported one yourself from UCM (see [Exporting and Sharing](Exporting-and-Sharing.md)).
- Some modding communities distribute raw XML camera files.

---

## Import Type 3: PAZ Archive

A `.paz` file is the game's archive format. Crimson Desert stores many of its data files inside PAZ archives, including the camera preset XML. UCM can read PAZ archives directly and pull out the camera data.

### Step by Step

1. Click **Import** in the sidebar.
2. Select **PAZ Archive**.
3. A file browser will open. Navigate to the `.paz` file and select it. (This is typically `0.paz` from the game's `0010/` folder, but could be any PAZ file that contains camera data.)
4. UCM uses its built-in PAZ/PAMT reader to locate and extract the camera XML from within the archive. This is the same extraction code the main app uses, so it handles the archive format, compression, and encryption automatically.
5. A **metadata dialog** appears. Fill in a name and any other details you want.
6. Click **Save** to finish the import.

### What Happens After Import

- The extracted camera data is saved into UCM's `import_presets/` folder.
- It appears in the sidebar under the **Imported** group.
- Only **God Mode** editing is available.

### When Would You Use This?

- If you want to import camera settings from a different version of the game (for example, you have a backup of an older `0.paz` with camera settings you liked).
- If someone shares a modified `0.paz` file and you want to pull the camera data out of it without replacing your entire game archive.

---

## Import Type 4: UCM Preset (.ucmpreset)

This is UCM's own preset format. It is the richest format because it contains not just the camera XML but also all of UCM's internal settings, the preset name, author, description, and session data.

### Step by Step

1. Click **Import** in the sidebar.
2. Select **UCM Preset**.
3. A file browser will open. Navigate to the `.ucmpreset` file and select it.
4. A **metadata dialog** appears, pre-filled with the name, author, description, and URL that the preset creator included. You can edit any of these if you want.
5. Click **Save** to finish the import.

### What Happens After Import

- The preset is saved into UCM's `my_presets/` folder (not `import_presets/`).
- It appears in the sidebar, and you get **full slider control**: Quick, Fine Tune, and God Mode are all available.
- This is the recommended way to share presets between UCM users because nothing is lost in translation.

### Why Does This Get Full Slider Control?

When UCM creates a preset, it stores not just the final XML output but also the individual slider values and camera rules that produced it. Raw imports (XML, PAZ, Mod Manager) only have the final XML, so UCM cannot reverse-engineer which slider positions would recreate those values. That is why raw imports are limited to God Mode.

---

## The Metadata Dialog

Every import type shows a metadata dialog before the preset is saved. This dialog has four fields:

| Field | Description | Required? |
|---|---|---|
| **Name** | A display name for the preset. This is what shows up in the sidebar. | Yes |
| **Author** | Who created the preset. | No |
| **Description** | A short summary of what the preset does or how it changes the camera. | No |
| **URL** | A link to the preset's page (for example, on Nexus Mods). | No |

- For **Mod Manager Package** imports, the name, author, and description are pre-filled from `manifest.json`.
- For **UCM Preset** imports, all fields are pre-filled from the `.ucmpreset` file.
- For **XML** and **PAZ** imports, fields start blank because those formats do not carry metadata.

You can always edit these fields. The values you enter here are saved with the preset and will show up when you view preset details later.

---

## Where Imported Presets Are Stored

| Import Type | Saved To | Sidebar Group |
|---|---|---|
| Mod Manager Package | `import_presets/` | Imported |
| Camera XML File | `import_presets/` | Imported |
| PAZ Archive | `import_presets/` | Imported |
| UCM Preset | `my_presets/` | My Presets |

The `import_presets/` and `my_presets/` folders are inside UCM's application directory (the same folder where `UltimateCameraMod.exe` is located).

---

## Understanding the God Mode Limitation

When you import a raw preset (Mod Manager, XML, or PAZ), you will notice that the **Quick** and **Fine Tune** tabs are disabled. If you click on them, UCM will show a dialog explaining that these tabs are not available for this preset type.

Here is why: Quick and Fine Tune work through UCM's **Camera Rules engine**. This engine takes your slider positions and generates the correct XML values. But when you import raw camera data, UCM does not know what slider positions would produce those values. It only has the finished XML.

**God Mode** lets you edit the XML directly, which is why it always works regardless of import type.

### Can I Convert a Raw Import to a Full Preset?

Not directly. If you want full slider control, you would need to recreate the camera settings from scratch using UCM's Quick or Fine Tune tabs on a new preset, using the imported preset as a reference in God Mode.

---

## Summary

- Use **Mod Manager Package** import when you have a mod folder with `manifest.json` from Crimson Desert JSON Mod Manager or CDUMM.
- Use **Camera XML File** import when you have a standalone `.xml` camera file.
- Use **PAZ Archive** import when you have a `.paz` game archive and want to extract the camera data from it.
- Use **UCM Preset** import when someone shares a `.ucmpreset` file with you. This gives you the best experience with full slider control.

All imported presets can be further edited (in God Mode at minimum) and exported in any format.
