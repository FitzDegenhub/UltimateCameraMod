# Presets

Presets are the heart of UCM. A preset is a complete set of camera settings that you can create, customize, save, share, and install to the game. This page covers everything about working with presets: creating them, organizing them, locking and unlocking them, the auto-save system, undo support, and detailed descriptions of every built-in UCM style.

---

## Table of Contents

- [What Is a Preset?](#what-is-a-preset)
- [Where Presets Are Stored on Disk](#where-presets-are-stored-on-disk)
- [The Sidebar: Preset Groups](#the-sidebar-preset-groups)
- [Creating a New Preset](#creating-a-new-preset)
- [Selecting and Loading a Preset](#selecting-and-loading-a-preset)
- [Locking and Unlocking Presets](#locking-and-unlocking-presets)
- [Auto-Save Behavior](#auto-save-behavior)
- [Undo (Ctrl+Z)](#undo-ctrlz)
- [Duplicating a Preset](#duplicating-a-preset)
- [Renaming a Preset](#renaming-a-preset)
- [Deleting a Preset](#deleting-a-preset)
- [The 8 Built-In UCM Styles](#the-8-built-in-ucm-styles)
- [Browsing for More Presets](#browsing-for-more-presets)

---

## What Is a Preset?

A preset is a saved collection of camera settings stored in a `.ucmpreset` file. Each preset contains:

- All your camera values (distance, height, FoV, offsets, and every other setting)
- The preset name
- Whether it is locked or unlocked
- The preset type (Managed by UCM or Full Manual Control)
- The complete camera XML that will be written to the game

When you select a preset in the sidebar, UCM loads all of its settings into the editors. When you install a preset to the game, UCM writes that preset's camera XML into the game's PAZ archive.

---

## Where Presets Are Stored on Disk

All presets are stored as `.ucmpreset` files in folders next to the UCM executable. The file format is JSON, so if you ever need to inspect one manually, you can open it in any text editor.

| Folder | What Goes Here |
|---|---|
| `ucm_presets/` | Official UCM preset styles that ship with the application. These are the built-in styles like Heroic, Panoramic, and Vanilla. |
| `my_presets/` | Presets that you create yourself. Also where imported `.ucmpreset` files are saved. |
| `community_presets/` | Community presets that you download through the Browse feature inside UCM. |
| `import_presets/` | Presets imported from raw sources (XML files, PAZ archives, mod manager packages). These are not `.ucmpreset` originals, so they only support God Mode editing. |

All of these folders are located in the same directory as `UltimateCameraMod.exe`. UCM creates them automatically on first launch if they do not already exist.

---

## The Sidebar: Preset Groups

The sidebar on the left side of the UCM window lists all your presets, organized into groups:

### Game Default (Vanilla)

This is a single preset that represents the completely unmodified game camera. It is always available and always locked. Selecting it loads the exact camera values that Crimson Desert ships with.

Use this as a reference point when you want to see what the vanilla settings are, or as a starting point for a new preset.

### UCM Presets

These are the official presets that come with UCM. There are 8 built-in styles (described in detail below). UCM Presets are always **locked** and cannot be edited. If you want to customize one, duplicate it first to create an editable copy in My Presets.

You can also discover additional UCM presets through the **Browse** button, which connects to an online catalog of official presets.

### Community Presets

These are presets created and shared by other UCM users. You can discover and download them through the **Browse** button in the sidebar. Downloaded community presets are saved to the `community_presets/` folder.

Community presets may be locked or unlocked depending on how the creator configured them.

### My Presets

This is your personal preset collection. Every preset you create yourself ends up here. Imported `.ucmpreset` files also land here. Presets in My Presets are unlocked by default, so you can freely edit them.

### Imported

Presets imported from raw sources (XML files, PAZ archives, mod manager packages) appear here. These are saved in the `import_presets/` folder. Because they come from raw camera data that UCM did not create, they only support God Mode editing. Quick and Fine Tune tabs are disabled for these presets.

For more on importing, see [Importing Presets](Importing-Presets.md).

---

## Creating a New Preset

To create a new preset:

1. Click the **New Preset** button in the sidebar.
2. Enter a name for your preset. Choose something descriptive so you can find it later (for example, "Wide Combat Camera" or "Low Cinematic").
3. Choose a **preset type**:
   - **Managed by UCM** (default, recommended): You get access to all three editors (Quick, Fine Tune, God Mode). UCM applies camera rules automatically to keep things consistent.
   - **Full Manual Control**: God Mode only. No UCM rules. For advanced users. See [Preset Types](Preset-Types.md) for a full explanation.
4. Your new preset is created in the `my_presets/` folder and appears under My Presets in the sidebar.
5. The preset starts with **vanilla camera values** (the game's defaults). You can now start adjusting sliders.

---

## Selecting and Loading a Preset

Click on any preset in the sidebar to select and load it. When you select a preset:

- The UCM Quick sliders update to reflect that preset's values.
- The Camera Preview and FoV Preview update in real time.
- Fine Tune sliders load the preset's detailed values.
- God Mode loads the preset's raw XML data.

Switching between presets is instant. You can click through different presets to quickly compare how they look in the preview panels.

**Important:** Selecting a preset only loads it into the editor. It does **not** install it to the game. To apply a preset to Crimson Desert, you must click "Install to Game" after selecting it.

---

## Locking and Unlocking Presets

Every preset has a lock state, shown by a **padlock icon** next to its name in the sidebar.

### What Locking Does

| Lock State | What Happens |
|---|---|
| **Locked** (padlock closed) | You can view the preset's settings but cannot change them. All sliders and fields are read-only. If you try to edit a locked preset, UCM shows a red toast notification reminding you that the preset is locked. |
| **Unlocked** (padlock open) | Full editing access. You can change any slider, checkbox, or value. Changes are auto-saved. |

### Which Presets Are Locked by Default

| Preset Group | Default Lock State | Can You Change It? |
|---|---|---|
| Game Default (Vanilla) | Locked | No, always locked |
| UCM Presets | Locked | No, UCM presets are always locked |
| Community Presets | Varies | Depends on the creator's choice |
| My Presets | Unlocked | Yes, you can lock or unlock them |
| Imported | Unlocked | Yes, you can lock or unlock them |

### How to Lock or Unlock

For presets that support toggling the lock state (My Presets, Imported, some Community Presets):

- Click the **padlock icon** next to the preset name in the sidebar.
- A closed padlock means locked; an open padlock means unlocked.

### Why Lock Your Own Presets?

Locking a preset you have finished working on is a good safety measure. It prevents accidental changes if you click on it later. Since auto-save is always active for unlocked presets, an unintended slider adjustment would be saved immediately. Locking the preset prevents this.

---

## Auto-Save Behavior

UCM automatically saves changes to unlocked presets as you make them. There is no "Save" button.

### How It Works

- Every time you adjust a slider, checkbox, dropdown, or raw value on an unlocked preset, the change is written to the `.ucmpreset` file on disk within moments.
- You never need to manually save. Your work is always preserved.
- If UCM closes unexpectedly (crash, power outage, etc.), your most recent changes are already saved.

### When Auto-Save Does Not Apply

- **Locked presets**: Changes are blocked entirely, so there is nothing to save.
- **The "Install to Game" action**: Auto-save saves the preset file. Installing to the game is a separate action that writes to the game's PAZ archive. These are independent operations.

### Can I Disable Auto-Save?

No. Auto-save is always active for unlocked presets. If you want to protect a preset from changes, lock it.

---

## Undo (Ctrl+Z)

UCM supports undo on the **UCM Quick** tab.

| Detail | Value |
|---|---|
| **Shortcut** | Ctrl+Z |
| **Maximum steps** | 20 |
| **Where it works** | UCM Quick tab only |
| **What it undoes** | Slider movements, checkbox toggles, dropdown changes |

### How It Works

Each change you make on the UCM Quick tab is recorded in an undo history. Pressing Ctrl+Z reverts the most recent change, restoring the previous slider position, checkbox state, or dropdown selection. You can press Ctrl+Z repeatedly to step back up to 20 changes.

### Limitations

- Undo only works on the UCM Quick tab. Changes made in Fine Tune or God Mode do not have undo support.
- The undo history is cleared when you switch to a different preset.
- Undo history is not preserved between UCM sessions. When you close and reopen UCM, the history starts fresh.

### Tip

If you are experimenting with a lot of changes and are not sure you will like the results, you can always duplicate your preset first as a backup. That way, even if you exceed the 20-step undo limit, you have a clean copy to go back to.

---

## Duplicating a Preset

Duplicating creates a copy of an existing preset. The copy is saved under My Presets and is unlocked, regardless of whether the original was locked.

### When to Duplicate

- You want to customize a locked UCM Preset or Community Preset.
- You want to create a variation of an existing preset without changing the original.
- You want a backup of your current preset before making major changes.

### How to Duplicate

Right-click on the preset in the sidebar (or use the duplicate option in the preset's context menu). A new preset will appear in My Presets with the same settings as the original. You can rename it to something descriptive.

---

## Renaming a Preset

You can rename any preset that you own (presets in My Presets or Imported). UCM Presets and Game Default cannot be renamed.

To rename a preset, right-click it in the sidebar and select the rename option, or look for an edit/rename button in the preset details area. Type the new name and confirm. The file on disk is updated to reflect the new name.

---

## Deleting a Preset

You can delete presets that you own (My Presets, Imported). UCM Presets and Game Default cannot be deleted.

To delete a preset, right-click it in the sidebar and select the delete option. UCM will ask for confirmation before removing the preset. Once deleted, the `.ucmpreset` file is removed from disk. This action cannot be undone.

**Caution:** If the deleted preset is currently installed to the game, the game will still use those camera settings until you install a different preset or click Restore.

---

## The 8 Built-In UCM Styles

UCM ships with 8 carefully designed camera presets. These are found in the UCM Presets group in the sidebar. All are locked (you cannot edit them directly, but you can duplicate them).

### Vanilla

**What it is:** The game's default camera, completely unchanged.

**What it looks like:** Exactly what Crimson Desert looks like out of the box. Standard over-the-shoulder perspective, default distance, default height, default FoV.

**When to use it:** As a reference point for comparison, or as a clean starting point to duplicate and customize.

---

### Heroic

**What it is:** A shoulder-level over-the-shoulder camera.

**What it looks like:** The camera sits right at shoulder height, slightly to the side, at a moderate distance. Your character is prominently framed with a clear view of what is ahead. The camera feel is similar to popular action-adventure games. It feels polished and confident.

**When to use it:** If you want a classic, well-balanced third-person action game camera. Good for both exploration and combat. A safe and comfortable starting point for customization.

---

### Panoramic

**What it is:** A head-height camera with a wider pullback.

**What it looks like:** The camera is positioned at roughly head height and pulled back further than default. You can see more of the environment around your character. The wider view gives a stronger sense of place and makes large environments feel more impressive.

**When to use it:** When you want to appreciate Crimson Desert's landscapes and world design. Great for exploration-focused gameplay. Also good if you want better spatial awareness in combat without a fully tactical overhead view.

---

### Close-Up

**What it is:** A tight, intimate camera.

**What it looks like:** The camera is brought in close to the character, almost like a first-person view with your character still visible. Details on armor, clothing, and the immediate environment are very prominent. The world feels immediate and personal.

**When to use it:** When you want a highly immersive, intimate experience. Good for roleplaying, atmospheric exploration, or taking detailed screenshots of your character.

---

### Low Rider

**What it is:** A hip-level camera.

**What it looks like:** The camera drops down to roughly hip height, looking slightly upward at the character and the world. Your character appears taller and more imposing. The sky and tall structures are more visible. The perspective has a slightly heroic, cinematic quality.

**When to use it:** If you want a more dramatic perspective that makes your character feel powerful. Pairs well with a moderate FoV increase.

---

### Knee Cam

**What it is:** A knee-level camera.

**What it looks like:** The camera is positioned at roughly knee height. Everything above you looks significantly larger. The character towers overhead, and buildings, trees, and mountains loom dramatically. This is a more extreme version of the Low Rider perspective.

**When to use it:** For a dramatic, stylized look. Makes everything feel larger than life. Not ideal for navigation (you cannot see as far ahead), but creates striking visuals.

---

### Dirt Cam

**What it is:** A ground-level camera.

**What it looks like:** The camera is positioned near the ground, almost at dirt level. You are looking up at everything. The character is a towering figure above you. The world feels enormous and slightly intimidating. Grass, rocks, and ground-level details fill the foreground.

**When to use it:** For a unique, extremely dramatic perspective. Great for screenshots and short gameplay sessions where you want a completely different visual experience. Not practical for extended play due to limited forward visibility.

---

### Survival

**What it is:** A tight, horror-game-style over-the-shoulder camera.

**What it looks like:** The camera is close to the character with a narrow field of view. The character fills much of the screen, and your peripheral vision is limited. The world feels claustrophobic and tense. Similar to the camera in survival horror games where limited visibility creates atmosphere and suspense.

**When to use it:** If you want Crimson Desert to feel more tense and atmospheric. Good for dungeon exploration, nighttime gameplay, or any situation where you want the environment to feel more threatening and enclosed.

---

### Quick Comparison of All 8 Styles

| Preset | Camera Height | Camera Distance | FoV | Overall Feel |
|---|---|---|---|---|
| **Vanilla** | Shoulder | Default | Default | The standard game camera |
| **Heroic** | Shoulder | Moderate | Moderate | Classic action game |
| **Panoramic** | Head | Wide | Moderate | Open, scenic exploration |
| **Close-Up** | Shoulder | Very close | Default | Intimate, immersive |
| **Low Rider** | Hip | Moderate | Moderate | Heroic, cinematic |
| **Knee Cam** | Knee | Moderate | Moderate | Dramatic, stylized |
| **Dirt Cam** | Ground | Moderate | Moderate | Extreme, ground-level |
| **Survival** | Shoulder | Very close | Narrow | Tense, horror-game style |

---

## Browsing for More Presets

The sidebar includes a **Browse** button that connects to online catalogs of presets:

- **UCM Presets catalog**: Official presets published by the UCM team. These may include new styles added after your version of UCM was released.
- **Community Presets catalog**: Presets shared by other UCM users.

When you browse, UCM shows you available presets with their names and descriptions. You can download any preset directly into UCM. Downloaded UCM Presets go to `ucm_presets/`, and downloaded Community Presets go to `community_presets/`.

Browsing requires an internet connection. The presets themselves are small files, so downloading is nearly instant.

---

## Summary

| Action | How | Notes |
|---|---|---|
| Create a preset | New Preset button in sidebar | Choose Managed by UCM or Full Manual Control |
| Load a preset | Click it in the sidebar | Does not install to game automatically |
| Install to game | Click "Install to Game" in sidebar | Game must be closed |
| Lock/unlock | Click padlock icon | UCM Presets are always locked |
| Duplicate | Right-click, Duplicate | Copy goes to My Presets, unlocked |
| Rename | Right-click, Rename | Only for presets you own |
| Delete | Right-click, Delete | Confirmation required, cannot be undone |
| Undo changes | Ctrl+Z on UCM Quick tab | Up to 20 steps |
| Browse online | Browse button in sidebar | Downloads UCM and Community presets |
