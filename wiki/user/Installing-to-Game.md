# Installing to Game

This page explains how UCM writes your camera settings into Crimson Desert's game files, how the backup system works, what the status bar messages mean, and how to restore the original camera if anything goes wrong.

---

## Table of Contents

- [Overview](#overview)
- [Before You Install](#before-you-install)
- [How to Install a Preset](#how-to-install-a-preset)
- [What Happens During Installation](#what-happens-during-installation)
- [The Backup System](#the-backup-system)
- [Vanilla Validation](#vanilla-validation)
- [What the Status Bar Messages Mean](#what-the-status-bar-messages-mean)
- [Restoring the Original Camera](#restoring-the-original-camera)
- [Game Patch Awareness](#game-patch-awareness)
- [The install_trace.txt File](#the-install_tracetxt-file)
- [Installing Over a Previous Install](#installing-over-a-previous-install)
- [Common Questions About Installing](#common-questions-about-installing)

---

## Overview

Creating or editing a preset in UCM only changes settings inside the UCM application. Your game is not affected until you explicitly click **Install to Game**. This button takes your current camera preset and writes it directly into Crimson Desert's `0.paz` archive, which is the game file that contains camera data.

The install process is designed to be safe and reversible:
- UCM creates a backup of the original game file before making any changes.
- You can restore the original camera at any time with a single click.
- UCM validates that your backup is clean and unmodified.
- UCM tracks game updates so it can warn you if your camera settings might have been overwritten.

---

## Before You Install

There are two important things to check before clicking Install to Game:

### 1. Close Crimson Desert

The game **must not be running** when you install. Crimson Desert locks its `0.paz` file while the game is open, which prevents UCM from writing to it. If you try to install while the game is running, the install will fail.

Close the game completely before installing. This includes making sure the game process is fully shut down (check your system tray if you are not sure).

### 2. Verify Game Files (First Time Only)

If this is your very first time using UCM, it is strongly recommended to verify your game files through Steam or Epic before installing. This ensures that UCM's backup captures the real, unmodified game files. See [Getting Started](Getting-Started.md) for instructions on how to verify.

If you have already verified your game files and this is not your first install, you can skip this step.

---

## How to Install a Preset

1. In UCM's sidebar, select the preset you want to install. Make sure it is highlighted/active.
2. Click the **Install to Game** button at the bottom of the sidebar.
3. Wait a moment while UCM processes the installation.
4. Check the **status bar** at the bottom of the UCM window for a success message.
5. Launch Crimson Desert and load into the game world to see your new camera.

That is it from a user perspective. The rest of this page goes into detail about what happens behind the scenes.

---

## What Happens During Installation

When you click Install to Game, UCM performs several steps in sequence. Here is exactly what happens:

### Step 1: Backup Check

UCM checks whether a backup of the original `0.paz` file already exists in the `backups/` folder (located next to the UCM executable). 

- **If no backup exists** (first install ever): UCM creates one. It copies the original `0.paz` from your game folder into the `backups/` directory. Before saving it, UCM validates that the file is truly from vanilla (unmodified) game files. See the [Vanilla Validation](#vanilla-validation) section below for how this works.
- **If a backup already exists**: UCM skips this step and moves on.

### Step 2: Read Vanilla Camera Data

UCM reads the camera XML from the backed-up vanilla `0.paz` file. This is the clean, unmodified camera data that the game originally shipped with. UCM always starts from this clean base, not from whatever is currently in the game folder.

### Step 3: Apply Your Modifications

UCM's **Camera Rules engine** takes your preset settings and applies them on top of the vanilla camera XML. This produces a new camera XML document that contains your customizations. The engine modifies existing values in the XML rather than injecting entirely new structures, which keeps the output compatible and predictable.

### Step 4: Size Matching

The modified camera XML needs to fit within the original PAZ archive slot. The PAZ archive has a fixed amount of space allocated for the camera data, and UCM cannot make that space larger. UCM adjusts the output to ensure it matches the expected size.

If the modified XML is too large to fit (which is rare), you will see a "Preset too large to install" error. See [Troubleshooting](Troubleshooting.md) for how to fix this.

### Step 5: Compression

The size-matched XML is compressed using **LZ4 compression**. This is the same compression format that the game uses for data inside PAZ archives.

### Step 6: Encryption

The compressed data is encrypted using **ChaCha20 encryption**. Again, this matches how the game's PAZ archives are encrypted. Without this step, the game would not be able to read the data.

### Step 7: Write to PAZ Archive

UCM writes the compressed and encrypted camera data back into the `0.paz` file in your Crimson Desert game folder, replacing the camera section of the archive.

### Step 8: Write Install Trace

UCM writes an `install_trace.txt` file with details about the installation (see the [install_trace.txt section](#the-install_tracetxt-file) below).

### Step 9: Status Bar Update

The status bar at the bottom of UCM updates to confirm the installation succeeded and shows size information.

---

## The Backup System

The backup system is one of the most important safety features in UCM. It ensures you can always get back to the original game camera.

### Where Backups Are Stored

Backups are stored in a `backups/` folder located next to the UCM executable. For example, if UCM is at:

```
C:\Tools\UltimateCameraMod\UltimateCameraMod.exe
```

Then the backup will be at:

```
C:\Tools\UltimateCameraMod\backups\0.paz
```

### When the Backup Is Created

The backup is created **once**, the very first time you install any preset to the game. After that, UCM reuses the same backup for all future installs and restores.

### What the Backup Contains

The backup is a copy of the original, unmodified `0.paz` file from your Crimson Desert game folder. This is the file that contains the camera data (along with other game data).

### Why Only One Backup?

UCM only needs one backup because it always applies your camera changes on top of the vanilla base. When you install a different preset, UCM does not modify the previous install. Instead, it goes back to the vanilla backup, applies the new preset's changes, and writes the result. This means each install is a clean application of your settings, not a chain of modifications on top of modifications.

### Protecting the Backup

UCM validates the backup when it is first created to make sure it comes from genuine, unmodified game files. See the next section for details on how this validation works.

---

## Vanilla Validation

When UCM creates a backup of your `0.paz` file for the first time, it runs a series of checks to confirm that the file is from a vanilla (unmodified) Crimson Desert installation. This is called **vanilla validation**.

### Why This Matters

If your game files were already modified by another camera mod when UCM creates its backup, then the backup is "tainted." A tainted backup causes problems because:

- UCM thinks the tainted data is the vanilla baseline, so your presets may look wrong.
- The tainted file may contain extra data (like XML comments from another mod) that inflates the size, causing "Preset too large to install" errors later.
- Restoring from a tainted backup does not actually restore the original camera.

### What UCM Checks

UCM validates the camera data using **five signature checks**:

| Check | What It Looks For |
|---|---|
| **FoV values** | Field of View values match the known vanilla defaults |
| **ZoomDistance** | The ZoomDistance attribute is set to `"3.4"` (the vanilla value) |
| **OffsetByVelocity zeroing** | OffsetByVelocity values are zeroed out, matching vanilla |
| **MaxZoomDistance** | The MaxZoomDistance attribute is set to `"30"` (the vanilla value) |
| **Padding comments** | The XML contains the standard padding comments that vanilla files have |

All five checks must pass for the backup to be considered valid.

### What Happens If Validation Fails

If UCM detects that the `0.paz` file is not vanilla (one or more signature checks fail), the following happens:

1. UCM **automatically deletes the tainted backup**. It will not keep a backup that it knows is corrupted.
2. UCM **shows you step-by-step fix instructions**. The instructions are:
   - Verify your game files on Steam (or Epic) to restore the real vanilla files.
   - Delete the `backups/` folder next to UCM.
   - Relaunch UCM.
3. After you follow these steps, UCM will create a fresh, clean backup the next time you install.

### How to Avoid This Problem

The simplest way to avoid tainted backup issues is to **verify your game files before using UCM for the first time**. If you have never installed any other camera mods, your files are already clean and there is nothing to worry about.

---

## What the Status Bar Messages Mean

After an installation, the status bar at the bottom of the UCM window shows a message. Here is what each message means:

### Successful Install

**"Installed current session to game. Camera payload updated - X bytes (Y compressed in PAZ)."**

This means the install worked. The numbers tell you:
- **X bytes**: The uncompressed size of the camera XML that was written.
- **Y compressed in PAZ**: The size of the data after LZ4 compression and ChaCha20 encryption, as it exists inside the PAZ archive.

These numbers are informational. You do not need to act on them unless you are troubleshooting a size issue.

### Successful Restore

After clicking Restore, the status bar will confirm that the vanilla camera has been restored.

### Error Messages

If something goes wrong during installation, the status bar (or a dialog box) will show an error. Common errors are covered on the [Troubleshooting](Troubleshooting.md) page.

---

## Restoring the Original Camera

If you want to undo your camera changes and go back to the game's default camera, UCM makes this simple.

### How to Restore

1. Click the **Restore** button in the sidebar (near the Install to Game button).
2. UCM copies the backed-up vanilla `0.paz` file back into your game folder, replacing whatever was there.
3. The status bar confirms the restore was successful.
4. Launch the game. The camera will be back to its original default.

### What Restore Does Not Do

- Restore does not delete your presets. All your UCM presets are still saved and available. You can re-install any of them later.
- Restore does not delete the backup. The backup remains in the `backups/` folder for future use.
- Restore does not affect any other mods or game files. It only replaces the `0.paz` file.

### Alternative: Verify Game Files

If you do not want to use UCM's Restore button (or if you suspect the backup itself is corrupted), you can always use Steam or Epic to verify your game files. This will re-download the original `0.paz` from the game servers and put everything back to default.

---

## Game Patch Awareness

When Crimson Desert receives a game update (patch), the update typically replaces the `0.paz` file. This means your installed camera settings are overwritten by the update and the camera goes back to vanilla defaults.

UCM has a built-in system to detect when this happens.

### How It Works

UCM tracks **install metadata**, including information about the game's current state at the time of installation. It does this by reading Steam's `appmanifest` file (or equivalent for Epic), which contains version information about the installed game.

When you launch UCM after a game update, UCM compares the current game version against the version recorded during your last install. If they do not match, UCM suspects that a game update has occurred.

### What You See

If UCM detects a possible game update since your last install, it shows a **warning message**. The warning tells you:

- The game may have been updated since your last camera install.
- Your camera settings are likely back to vanilla defaults.
- You should reinstall your preset.

### What to Do

1. Read the warning and dismiss it.
2. Select the preset you want in the sidebar.
3. Click **Install to Game** again to reapply your camera settings.

In most cases, this is all you need to do. Your preset is reapplied on top of the new vanilla base, and everything works as before.

### What If the Game Changed the Camera?

Occasionally, a game update may change the vanilla camera XML itself (for example, adding new camera states or adjusting default values). Since UCM applies your modifications on top of the vanilla base, changes to the base can slightly shift how your preset looks in-game. If you notice your camera feels a little different after a game update, you may need to re-tune your preset with the current vanilla values as the new baseline.

---

## The install_trace.txt File

Every time you install a preset to the game, UCM writes a file called `install_trace.txt` in the UCM folder. This file is a diagnostic log that records details about the installation.

### What It Contains

| Information | Purpose |
|---|---|
| **SHA256 hashes** | Cryptographic fingerprints of the backup file, the modified data, and the final PAZ file. Useful for verifying file integrity. |
| **File sizes** | The sizes of the input and output data at each stage (raw XML, compressed, encrypted). |
| **Metadata** | The preset name, timestamp, and other details about the install. |

### When Is This Useful?

In normal use, you will never need to look at this file. It exists primarily for troubleshooting. If you report a bug or ask for help, the UCM team may ask you to share the contents of `install_trace.txt` to help diagnose the problem.

---

## Installing Over a Previous Install

You can install different presets as many times as you want. Each install replaces the previous one. UCM does not stack changes. Here is the flow:

1. You install Preset A. The game now uses Preset A's camera.
2. You install Preset B. The game now uses Preset B's camera. Preset A's changes are completely gone from the game files.
3. You click Restore. The game goes back to vanilla. Neither Preset A nor Preset B is in the game files.

This works because UCM always starts from the vanilla backup and applies the selected preset fresh. There is no layering or accumulation of changes.

---

## Common Questions About Installing

**Do I need to restart the game after installing?**
Yes. Crimson Desert reads the camera data when the game launches. If the game is already running, it will not pick up the changes. (Also, you cannot install while the game is running because the PAZ file is locked.)

**Can I install while the game is running?**
No. The game locks the `0.paz` file while it is open. Close the game first, install, then relaunch.

**Does Install to Game affect anything besides the camera?**
No. UCM only modifies the camera section inside `0.paz`. All other game data in the archive is left untouched.

**What if I accidentally delete the backups folder?**
UCM will create a new backup the next time you install. As long as your game files are currently vanilla (or you verify them on Steam/Epic first), the new backup will be fine. If your game files are currently modified, verify them first before letting UCM create a new backup.

**Is Install to Game the same as the PAZ export?**
Not exactly. Install to Game writes directly into the game's existing `0.paz` file (replacing just the camera section). The PAZ export creates a brand new `.paz` file that you would manually place in the game folder. Install to Game also manages backups, validation, and tracking. The PAZ export is a standalone file with no management features.

**How is Install to Game different from JSON (Mod Manager) export?**
JSON export creates a patch package for use with Crimson Desert JSON Mod Manager or CDUMM. The mod manager handles installing, uninstalling, and conflict resolution. Install to Game bypasses mod managers entirely and writes to the game files directly. JSON export is safer if you use a mod manager. Install to Game is more direct and does not require any additional tools. See [Exporting and Sharing](Exporting-and-Sharing.md) for a detailed comparison.

---

## Summary

| Task | How |
|---|---|
| Install a preset to the game | Select the preset, click **Install to Game** (game must be closed) |
| Check if the install worked | Read the status bar at the bottom of UCM |
| Restore the original camera | Click **Restore** in the sidebar |
| Handle a game update warning | Dismiss the warning and reinstall your preset |
| Find install diagnostic info | Open `install_trace.txt` in the UCM folder |
