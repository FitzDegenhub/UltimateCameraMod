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
- [What to Do Next](#what-to-do-next)

---

## What You Need

Before you start, make sure you have:

| Requirement | Details |
|---|---|
| **Crimson Desert** | Installed through Steam or Epic Games Store. The game must be installed, but it should **not** be running while you use UCM to install camera changes. |
| **Windows** | UCM is a Windows desktop application. |
| **Disk space** | UCM itself is small (a few MB). It creates backup files and preset folders alongside the exe, so keep a little extra space available. |

You do **not** need any special frameworks, installers, or admin privileges. UCM is a standalone portable application.

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

2. **Make sure the folder is writable.** UCM creates several subfolders next to the exe:
   - `ucm_presets/` - Official UCM preset styles
   - `my_presets/` - Your personal presets
   - `community_presets/` - Community-shared presets you download
   - `import_presets/` - Presets imported from files or other mods
   - `backups/` - Backup of your original game files (created automatically on first install)

   Avoid placing UCM inside `Program Files` or other system-protected directories, because Windows may block UCM from creating these folders.

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

**Why?** UCM creates a backup of your original game camera data the first time it installs changes. If your game files are already modified by another camera mod, UCM's backup will contain those modified files instead of the real originals. This can cause problems later (like the "Preset too large to install" error). Starting from verified vanilla files avoids this entirely.

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

Let's go through each of these.

---

## Game Directory Detection

UCM needs to know where Crimson Desert is installed so it can read the camera data from the game's PAZ archive and later write your changes back.

UCM tries to find the game automatically using three methods, in this order:

| Method | What it does |
|---|---|
| **Steam registry** | Checks the Windows registry for Steam's installation records to locate the Crimson Desert game folder. |
| **Epic Games Store** | Checks for Epic's installation manifests to find the game path. |
| **Brute-force drive scan** | If neither Steam nor Epic methods find the game, UCM scans your drives looking for the Crimson Desert installation folder. |

In most cases, detection is instant and automatic. You will see the detected path displayed in the application. If UCM cannot find the game, you will be prompted to locate the game folder manually.

---

## The Welcome Overlay

After game detection, UCM shows a **Welcome Overlay** on top of the main interface. This is a one-time first-run screen that covers important setup information.

The welcome overlay will:

- Confirm that UCM found your game directory
- **Ask you to verify your game files on Steam (or Epic) before proceeding.** This is the same verification step described above. If you already did it, you can skip ahead. If you did not, this is your chance to do it before UCM reads anything from the game.
- Explain what UCM will do with your game files (read the camera XML, create a backup on first install)

Read through the welcome overlay carefully, especially if this is your first time using any camera mod for Crimson Desert. Once you acknowledge it, the overlay closes and you will not see it again on future launches.

---

## The Tutorial Walkthrough

Immediately after the welcome overlay (or when you dismiss it), UCM launches an **interactive tutorial** that walks you through the interface with spotlight highlights.

The tutorial works like this:

- A dark overlay covers most of the screen
- A bright spotlight circle highlights one element of the interface at a time
- A text box explains what the highlighted element does
- You click "Next" or press a key to move to the next highlight

The tutorial covers:

- The **sidebar** with preset groups and the Install/Restore buttons
- The **UCM Quick tab** with its sliders and preview panels
- The **Fine Tune tab** and how to navigate its card sections
- The **God Mode tab** and the DataGrid
- The **Camera Preview** and **FoV Preview** panels
- How to create, save, and install presets

You can skip the tutorial if you want to explore on your own, but it is worth going through at least once. It takes about two minutes and gives you a solid understanding of the entire interface.

---

## Legacy Preset Migration (Upgrading from v2)

If you previously used UCM version 2.x, you may have old presets saved as `.json` files. When UCM v3 launches, it automatically detects and converts these old `.json` presets to the newer `.ucmpreset` format.

- The conversion happens silently in the background
- Your old presets will appear in the sidebar just as before
- No settings are lost during conversion
- The original `.json` files are left in place (they are not deleted)

If you are a brand-new user, this step does not apply to you and nothing happens.

---

## Creating Your First Preset

Now that UCM is open and ready, let's create your first custom camera preset.

### Step 1: Start from a built-in style (optional but recommended)

In the sidebar on the left, you will see the **UCM Presets** group. These are official presets that ship with UCM. Click on one to load it. Good starting points:

| Preset | What it looks like |
|---|---|
| **Vanilla** | The game's default camera, unchanged. A safe starting point. |
| **Heroic** | Shoulder-level over-the-shoulder view. A classic third-person action game feel. |
| **Panoramic** | Head-height with a wider pullback. Shows more of the world around you. |

Pick one that sounds close to what you want.

### Step 2: Create your own preset

1. Click the **New Preset** button (usually at the top of the sidebar or via a menu)
2. Give your preset a name, something like "My First Camera"
3. Choose a preset type:
   - **Managed by UCM** (recommended for beginners) - Gives you access to all three editors and applies UCM's helpful camera rules automatically
   - **Full Manual Control** - For advanced users who want to edit raw values with no UCM assistance

For your first preset, choose **Managed by UCM**.

### Step 3: Adjust the camera

You are now on the **UCM Quick** tab, which has the main camera sliders:

- **Distance** - Drag this to move the camera closer or farther from your character. Try pulling it to 6 or 7 for a wider view, or down to 3 for a tighter shot.
- **Height** - Drag up or down to raise or lower the camera. Negative values bring the camera lower (more cinematic), positive values raise it up (better overview).
- **Field of View (FoV)** - Use the dropdown to add extra degrees of field of view. Higher values give a wider, more expansive look. Try +10 or +20 to start.

As you adjust sliders, watch the **Camera Preview** panel. It gives you a rough visual representation of where the camera will sit relative to your character.

### Step 4: Your changes are saved automatically

If your preset is unlocked (which it is by default for presets you create), every change you make is automatically saved back to the preset file. You do not need to manually save.

---

## Installing Your Camera to the Game

Creating a preset and adjusting sliders only changes settings inside UCM. To actually see your camera in Crimson Desert, you need to **install** the preset to the game.

### Step 1: Make sure the game is not running

UCM needs to modify the game's `0.paz` archive file. If Crimson Desert is running, this file is locked and UCM cannot write to it. Close the game first.

### Step 2: Click "Install to Game"

In the sidebar, click the **Install to Game** button.

Here is what happens behind the scenes:

1. **First-time backup**: If this is the very first time you are installing any preset, UCM creates a backup of the original `0.paz` file and stores it in the `backups/` folder next to the exe. This backup is your safety net. UCM validates that this backup comes from actual vanilla game files to prevent issues later.
2. **Camera XML injection**: UCM takes your modified camera XML and writes it into the game's `0.paz` archive, replacing the original camera data.
3. **Status update**: The status bar at the bottom of UCM shows the compressed and uncompressed size of the data that was written.

### Step 3: Check the status bar

After installation, look at the status bar at the bottom of the UCM window. It should confirm that the install was successful and show file size information.

---

## Verifying It Worked

1. Launch Crimson Desert
2. Load into the game world (main menu camera may not reflect all changes)
3. Look at the third-person camera while moving your character around

You should see the camera positioned according to your Distance, Height, and FoV settings. If you used the Heroic preset as a base and pulled the distance back, you will notice the camera is farther from the character with a wider view of the surroundings.

**If the camera looks unchanged**, check these things:
- Did you click "Install to Game"? Creating a preset alone does not change the game.
- Is the correct preset selected in the sidebar? The installed preset should be highlighted.
- Did you restart the game after installing? The game reads camera data on launch, not while running.

---

## What to Do Next

Now that you have your first custom camera working, here are some next steps:

- **Explore the built-in UCM presets** to find a style you like, then tweak it. See the [Presets](Presets.md) page for details on all 8 built-in styles.
- **Learn about each camera setting in depth** on the [Camera Settings Explained](Camera-Settings-Explained.md) page. Understanding what Distance, Height, FoV, Centered Camera, Lock-on Zoom, and Steadycam actually do will help you dial in exactly the camera feel you want.
- **Try the Fine Tune tab** for more granular control over specific camera states (on foot, mounted, combat, aiming). See [The Three Editors](The-Three-Editors.md) for a full guide.
- **Browse community presets** using the Browse button to download camera styles that other players have created and shared.
- **Learn about preset types** on the [Preset Types](Preset-Types.md) page to understand the difference between Managed and Full Manual Control presets.

If you run into problems, check the [Troubleshooting](Troubleshooting.md) page for solutions to common issues.

---

## Quick Reference

| Task | How |
|---|---|
| Create a new preset | Click New Preset in the sidebar, name it, choose Managed by UCM |
| Adjust camera | Use the sliders on the UCM Quick tab |
| Install to game | Click "Install to Game" in the sidebar (game must be closed) |
| Undo a change | Press Ctrl+Z on the UCM Quick tab (up to 20 steps) |
| Restore original camera | Click "Restore" in the sidebar to revert to the backup |
| Lock a preset | Click the padlock icon next to the preset name |
