# Getting Started with Ultimate Camera Mod (UCM)

Welcome to Ultimate Camera Mod! This guide walks you through everything from downloading UCM to seeing your first custom camera in Crimson Desert. If you follow these steps in order, you will be up and running in about five minutes.

---

## Table of Contents

- [What You Need](#what-you-need)
- [Downloading UCM](#downloading-ucm)
- [Where to Put the Files](#where-to-put-the-files)
- [Before You Launch: Verify Game Files](#before-you-launch-verify-game-files)
- [First Launch](#first-launch)
- [Game Directory Detection](#game-directory-detection)
- [The Welcome Overlay](#the-welcome-overlay)
- [The Tutorial Walkthrough](#the-tutorial-walkthrough)
- [Legacy Preset Migration (Upgrading from v2)](#legacy-preset-migration-upgrading-from-v2)
- [Creating Your First Preset](#creating-your-first-preset)
- [Installing Your Camera to the Game](#installing-your-camera-to-the-game)
- [Verifying It Worked](#verifying-it-worked)
- [Restoring the Original Camera](#restoring-the-original-camera)
- [What to Do Next](#what-to-do-next)

---

## What You Need

Before you start, make sure you have:

| Requirement | Details |
|---|---|
| **Crimson Desert** | Installed through Steam or Epic Games Store. The game must be installed, but it should **not** be running while you use UCM to install camera changes. |
| **Windows 10 or later** | UCM is a Windows desktop application. |
| **Disk space** | UCM itself is small (a few MB). It creates backup files and preset folders alongside the exe, so keep a little extra space available. |

You do **not** need any special frameworks, runtime installers, or admin privileges. UCM is a standalone portable application. There is nothing to inject into the game, no DLL files to place, and no game executable modifications. UCM works by reading and writing the game's PAZ archive files, which is the same data format the game itself uses to store camera settings.

---

## Downloading UCM

1. Download the latest UCM release. You will get a `.zip` file containing the UCM executable and supporting files.
2. There is no installer. The zip contains everything you need.

---

## Where to Put the Files

UCM is a **portable application**, meaning it runs from wherever you place it. There is no installation wizard.

1. **Extract the zip** to a folder of your choice. Good examples:
   - `C:\Tools\UltimateCameraMod\`
   - `D:\Games\Mods\UltimateCameraMod\`
   - Your Desktop (works fine, though a dedicated folder is tidier)

2. **Make sure the folder is writable.** UCM creates several subfolders next to the exe the first time it runs:

   | Folder | Purpose |
   |---|---|
   | `ucm_presets/` | Official UCM preset styles (shipped with the app) |
   | `my_presets/` | Your personal presets that you create |
   | `community_presets/` | Community-shared presets you download through the Browse feature |
   | `import_presets/` | Presets imported from external files or other mods |
   | `backups/` | Backup of your original game files (created automatically on first install) |

   Avoid placing UCM inside `Program Files` or other system-protected directories, because Windows may block UCM from creating these folders or writing preset files.

3. **Do not put UCM inside the Crimson Desert game folder.** It does not need to live there. UCM locates the game automatically.

Your folder should look something like this after extraction:

```
UltimateCameraMod/
    UltimateCameraMod.exe
    (other supporting files)
```

After first launch, it will grow to look like:

```
UltimateCameraMod/
    UltimateCameraMod.exe
    ucm_presets/
    my_presets/
    community_presets/
    import_presets/
    backups/
    (other supporting files)
```

---

## Before You Launch: Verify Game Files

This step is **strongly recommended** before using UCM for the first time, and UCM itself will ask you to do this on first launch.

**Why does this matter?** UCM creates a backup of your original game camera data the first time it installs changes. If your game files are already modified by another camera mod or a corrupted update, UCM's backup will contain those modified files instead of the real originals. This can cause problems later, most notably the "Preset too large to install" error where the modified XML compresses to more bytes than the original PAZ slot allows. Starting from verified vanilla files avoids this entirely.

**How to verify on Steam:**
1. Open your Steam Library
2. Right-click **Crimson Desert**
3. Click **Properties**
4. Go to the **Installed Files** tab (or Local Files, depending on your Steam version)
5. Click **Verify integrity of game files**
6. Wait for Steam to finish checking and redownloading any modified files

**How to verify on Epic Games Store:**
1. Open your Epic Games Library
2. Click the three dots (...) next to Crimson Desert
3. Click **Verify**
4. Wait for the process to complete

Once verification finishes, you are ready to launch UCM.

---

## First Launch

Double-click `UltimateCameraMod.exe` to start UCM.

On the very first launch, several things happen in sequence:

1. **Game directory detection** - UCM searches for your Crimson Desert installation
2. **Welcome overlay** - A first-run screen with important information
3. **Tutorial walkthrough** - An interactive guide highlighting each part of the interface
4. **Legacy migration** (if upgrading from v2) - Old presets are converted automatically

Let's go through each of these.

---

## Game Directory Detection

UCM needs to know where Crimson Desert is installed so it can read the camera data from the game's PAZ archive and later write your changes back.

UCM tries to find the game automatically using three methods, in this order:

| Method | What it does |
|---|---|
| **Steam registry** | Checks the Windows registry for Steam's installation records to locate the Crimson Desert game folder. This is the fastest and most reliable method. |
| **Epic Games Store** | Checks for Epic's installation manifests to find the game path. |
| **Brute-force drive scan** | If neither Steam nor Epic methods find the game, UCM scans your hard drives looking for the Crimson Desert installation folder. This takes a bit longer but covers non-standard install locations. |

In most cases, detection is instant and automatic. You will see the detected path displayed in the application. If UCM cannot find the game through any of these methods, you will be prompted to locate the game folder manually using a folder browser.

---

## The Welcome Overlay

After game detection, UCM shows a **Welcome Overlay** on top of the main interface. This is a one-time first-run screen that covers important setup information.

The welcome overlay will:

- Confirm that UCM found your game directory
- **Ask you to verify your game files on Steam (or Epic) before proceeding.** This is the same verification step described above. If you already did it, you can move ahead. If you did not, this is your chance to do it before UCM reads anything from the game.
- Explain what UCM will do with your game files (read the camera XML, create a backup on first install)

Read through the welcome overlay carefully, especially if this is your first time using any camera mod for Crimson Desert. Once you acknowledge it, the overlay closes and you will not see it again on future launches.

---

## The Tutorial Walkthrough

Immediately after the welcome overlay, UCM launches an **interactive tutorial** that walks you through the interface with spotlight highlights.

The tutorial works like this:

- A dark overlay covers most of the screen
- A bright spotlight circle highlights one element of the interface at a time
- A text box explains what the highlighted element does
- You click "Next" or press a key to move to the next highlight

The tutorial covers:

- The **sidebar** on the left, showing preset groups, the Install to Game button, and the Restore button
- The **UCM Quick tab** with its main camera sliders (Distance, Height, FoV, and more)
- The **Fine Tune tab** and how to navigate its organized card sections
- The **God Mode tab** and the raw XML DataGrid
- The **Camera Preview** panel that visualizes your camera position in real time
- The **FoV Preview** panel that shows how your field of view compares to vanilla
- How to create, save, and install presets

You can skip the tutorial if you want to explore on your own, but it is worth going through at least once. It takes about two minutes and gives you a solid understanding of the entire interface. The tutorial only appears on your first launch.

---

## Legacy Preset Migration (Upgrading from v2)

If you previously used UCM version 2.x, you may have old presets saved as `.json` files. When UCM v3 launches and detects these, it automatically converts them to the newer `.ucmpreset` format.

- The conversion happens silently in the background
- Your old presets will appear in the sidebar just as before
- No settings are lost during conversion
- The original `.json` files are left in place (they are not deleted)

If you are a brand-new user, this step does not apply to you and nothing happens.

---

## Creating Your First Preset

Now that UCM is open and ready, let's create your first custom camera preset. There are two approaches.

### Approach A: Start from a Built-In Style (Recommended for Beginners)

In the sidebar on the left, you will see the **UCM Presets** group. These are official presets that ship with UCM. Click on one to load it. Good starting points:

| Preset | What it looks like |
|---|---|
| **Vanilla** | The game's default camera, completely unchanged. A safe baseline. |
| **Heroic** | Shoulder-level over-the-shoulder view. A classic third-person action game feel. |
| **Panoramic** | Head-height with a wider pullback. Shows more of the world around your character. |
| **Close-Up** | Brings the camera in tight for an intimate, immersive view. |

Pick one that sounds close to what you want.

Built-in UCM presets are **locked** (you will see a padlock icon next to them). You cannot directly edit locked presets. To make your own customized version:

1. Select the preset you like in the sidebar.
2. **Duplicate** it (right-click or use the duplicate option) to create an editable copy.
3. Your duplicate is saved under "My Presets" and is unlocked, so you can freely adjust any slider.

### Approach B: Start from Scratch

1. Click the **New Preset** button in the sidebar.
2. Give your preset a name, something like "My First Camera."
3. Choose a preset type:
   - **Managed by UCM** (recommended for beginners): Gives you access to all three editors and applies UCM's helpful camera rules automatically (FoV consistency, smooth transitions, lock-on scaling, mount sync).
   - **Full Manual Control**: For advanced users who want to edit raw XML values with no UCM assistance. See [Preset Types](Preset-Types.md) for details.
4. For your first preset, choose **Managed by UCM**.
5. Your new preset starts with vanilla (default game) camera values.

### Adjusting Your Camera

You are now on the **UCM Quick** tab, which has the main camera sliders. Try these changes to see immediate visual feedback in the preview panels:

- **Distance slider**: Drag it to the right to pull the camera farther back from your character, or to the left to bring it closer. The range goes from 1.5 (very tight, almost first-person) to 12 (very far, showing a large area around the character). Try setting it to 6 or 7 for a comfortable wider view.

- **Height slider**: Drag up to raise the camera above your character's head, or down to lower it toward the ground. The range is -1.6 (well below hip level) to 1.5 (high above the head). Try a small negative value like -0.3 for a slightly lower, more cinematic angle.

- **Field of View (FoV) dropdown**: Select a value to widen your field of view. Each step adds degrees on top of the game's base 40-degree FoV. Selecting "+10" gives you a 50-degree field of view. Selecting "+20" gives you 60. Higher values show more of the world but can start to look fisheye at the extremes.

As you move sliders, watch the **Camera Preview** panel on the right side of the UCM Quick tab. It updates in real time to show a rough representation of where the camera sits relative to the character. The **FoV Preview** panel shows a visual comparison of your current field of view versus the default.

### Auto-Save

If your preset is unlocked (which it is by default for presets you create), every change you make is **automatically saved** back to the preset file on disk. There is no "Save" button you need to remember to click.

If you make a mistake, press **Ctrl+Z** to undo your last change. Undo works on the UCM Quick tab and supports up to 20 steps back.

---

## Installing Your Camera to the Game

Creating a preset and adjusting sliders only changes settings inside UCM. To actually see your custom camera in Crimson Desert, you need to **install** the preset to the game's files.

### Step 1: Make Sure the Game Is Not Running

UCM needs to modify the game's `0.paz` archive file. If Crimson Desert is running, this file is locked by the game process and UCM cannot write to it. Close the game completely before proceeding.

### Step 2: Click "Install to Game"

In the sidebar, click the **Install to Game** button. Here is what happens behind the scenes:

1. **First-time backup**: If this is the very first time you are installing any preset, UCM creates a backup of the original `0.paz` file and stores it in the `backups/` folder next to the exe. This backup is your safety net for restoring the vanilla camera later. UCM also validates that this backup was made from actual vanilla game files, not already-modded files.

2. **Camera XML injection**: UCM takes your modified camera XML and writes it into the game's `0.paz` archive, replacing the original camera data inside the archive.

3. **Status update**: The status bar at the bottom of UCM shows the compressed and uncompressed size of the data that was written, confirming the install succeeded.

### Step 3: Check the Status Bar

After installation, look at the status bar at the bottom of the UCM window. It should confirm that the install was successful and show file size information.

---

## Verifying It Worked

1. Launch Crimson Desert normally through Steam or Epic.
2. Load into the game world (the main menu camera may not reflect all changes).
3. Look at the third-person camera while moving your character around.

You should see the camera positioned according to your Distance, Height, and FoV settings. If you used the Heroic preset as a base and pulled the distance back, you will notice the camera is farther from the character with a wider view of the surroundings.

**If the camera looks unchanged**, check these things:

| Problem | Solution |
|---|---|
| Camera looks the same as vanilla | Did you click "Install to Game"? Creating a preset alone does not change the game files. |
| Wrong preset seems to be active | Is the correct preset selected in the sidebar? Make sure the preset you edited is the one you installed. |
| Changes not visible | Did you restart the game after installing? The game reads camera data on launch, so changes made while the game is running require a restart. |
| Install button gave an error | Make sure Crimson Desert is fully closed. Check the status bar for error messages. |

---

## Restoring the Original Camera

If you ever want to go back to the default camera:

1. Click the **Restore** button in the sidebar.
2. UCM replaces the modified `0.paz` in the game folder with the backup copy it created on first install.
3. Your game is now back to its original, unmodified camera.

You can also restore the vanilla camera by verifying game files through Steam or Epic, which re-downloads the original `0.paz` from the game servers. Note that this also deletes UCM's backup, so you should delete the `backups/` folder in UCM as well to avoid confusion on your next install.

---

## What to Do Next

Now that you have your first custom camera working, here are some next steps:

- **Explore the built-in UCM presets** to find a style you like, then duplicate and tweak it. See [Presets](Presets.md) for descriptions of all eight styles and everything about managing presets.
- **Learn what each camera setting does in depth** on the [Camera Settings Explained](Camera-Settings-Explained.md) page. Understanding Distance, Height, FoV, Centered Camera, Lock-on Zoom, and Steadycam will help you dial in exactly the camera feel you want.
- **Try the Fine Tune and God Mode tabs** for deeper control over specific camera states (on foot, mounted, combat, aiming, and more). See [The Three Editors](The-Three-Editors.md) for a full guide.
- **Understand preset types** on the [Preset Types](Preset-Types.md) page, especially if you plan to import camera settings from other mods or want full raw XML control.
- **Browse community presets** using the Browse button in the sidebar to download camera styles that other players have created and shared.
- **Share your own presets** by exporting them. See [Exporting and Sharing](Exporting-and-Sharing.md) for all four export formats.

If you run into problems, check the [Troubleshooting](Troubleshooting.md) page for solutions to common issues like "Preset too large to install," game crashes on load, or greyed-out sliders.

---

## Quick Reference

| Task | How |
|---|---|
| Create a new preset | Click New Preset in the sidebar, name it, choose Managed by UCM |
| Duplicate a locked preset | Right-click the preset and select Duplicate |
| Adjust camera | Use the sliders on the UCM Quick tab |
| Undo a change | Press Ctrl+Z on the UCM Quick tab (up to 20 steps) |
| Install to game | Click "Install to Game" in the sidebar (game must be closed) |
| Restore original camera | Click "Restore" in the sidebar to revert to the backup |
| Lock/unlock a preset | Click the padlock icon next to the preset name |
| Find your preset files | Look in the `my_presets/` folder next to the UCM executable |
