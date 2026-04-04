# Troubleshooting

This page covers every known error message and common problem you might run into while using UCM, with clear step-by-step solutions for each one. If you are experiencing an issue, find it in the list below and follow the instructions.

---

## Table of Contents

- [Installation Errors](#installation-errors)
  - ["Preset too large to install"](#preset-too-large-to-install)
  - [Game crashes on load after installing](#game-crashes-on-load-after-installing)
  - [Camera not changing in game](#camera-not-changing-in-game)
  - [Corrupted backup message](#corrupted-backup-message)
  - [Game update warning](#game-update-warning)
- [File and Archive Errors](#file-and-archive-errors)
  - ["Archive/PAMT not found"](#archivepamt-not-found)
  - ["Camera file not found"](#camera-file-not-found)
- [Editing and Slider Issues](#editing-and-slider-issues)
  - [Sliders greyed out on Fine Tune](#sliders-greyed-out-on-fine-tune)
  - [UCM Quick and Fine Tune tabs disabled](#ucm-quick-and-fine-tune-tabs-disabled)
- [Display and Formatting Issues](#display-and-formatting-issues)
  - [European decimal separator issues](#european-decimal-separator-issues)
- [In-Game Camera Issues](#in-game-camera-issues)
  - [Camera jumps when pressing CTRL near NPCs](#camera-jumps-when-pressing-ctrl-near-npcs)
  - [Preset looks different after a game update](#preset-looks-different-after-a-game-update)
  - [HUD centering not working on ultrawide](#hud-centering-not-working-on-ultrawide)
- [Still Having Problems?](#still-having-problems)

---

## Installation Errors

### "Preset too large to install"

**What it means:** The modified camera XML, after compression, is larger than the space allocated for it inside the game's PAZ archive. The PAZ archive has a fixed slot for camera data, and your modifications produced output that does not fit.

**Common causes:**

1. **Tainted backup.** This is the most common cause. If your backup was made from game files that were already modified by another camera mod, the backup contains extra data (such as XML comments or injected nodes from the other mod). When UCM applies your changes on top of this inflated base, the result is too large. UCM's vanilla validation tries to catch this, but it is worth checking manually.

2. **Too many God Mode changes that add new XML nodes.** If you have been making extensive edits in God Mode and some of those edits added new XML elements (rather than just changing existing values), the total XML size can grow beyond the slot limit.

**How to fix it:**

1. **Verify your game files on Steam or Epic.**
   - Steam: Right-click Crimson Desert in your library, go to Properties, then Installed Files (or Local Files), and click "Verify integrity of game files."
   - Epic: Click the three dots next to Crimson Desert, click Verify.
   - Wait for the verification to complete. This restores the original, unmodified `0.paz` to your game folder.

2. **Delete the `backups/` folder** located next to the UCM executable. This removes the old (possibly tainted) backup.

3. **Relaunch UCM.** The next time you install a preset, UCM will create a fresh backup from the now-verified vanilla game files.

4. **Try installing again.** If the issue was a tainted backup, this should resolve it.

5. If the error persists after these steps, your preset may genuinely be too large. Try reducing the number of God Mode edits or start from a simpler base.

---

### Game crashes on load after installing

**What it means:** The game fails to start or crashes during loading after you installed a camera preset through UCM.

**Possible causes:**

1. **Corrupted camera XML.** Something went wrong during the install process and the camera data in the PAZ archive is malformed.

2. **Modified a forbidden camera section.** Certain camera sections in the XML, particularly the **SilenceKill** (stealth finisher) sections, will crash the game if they are modified. This is a known limitation of Crimson Desert's camera system.

**How to fix it:**

1. **Click Restore in UCM.** This replaces the modified `0.paz` with your backed-up vanilla version. The game should launch normally after restoring.

2. **Verify game files on Steam or Epic** as an extra safety measure. This will replace the `0.paz` with a fresh copy from the game servers if Restore did not work.

3. **Check your preset.** If you were editing in God Mode, review whether you changed any values in SilenceKill camera sections. If so, undo those changes and install again.

4. **Try a different preset.** If the crash only happens with one specific preset, the issue is with that preset's settings. Try installing a different preset to confirm UCM itself is working correctly.

---

### Camera not changing in game

**What it means:** You have made changes in UCM, but when you play the game, the camera still looks like the default.

**Step-by-step checklist:**

1. **Did you click "Install to Game"?**
   Creating or editing a preset in UCM does not automatically change anything in the game. You must explicitly click the Install to Game button. Check the status bar at the bottom of UCM. If there is no success message, the preset was not installed.

2. **Did you see the success message?**
   After clicking Install to Game, the status bar should show a message like "Installed current session to game. Camera payload updated - X bytes (Y compressed in PAZ)." If you did not see this, the install may have failed. Look for error messages.

3. **Was the game running when you installed?**
   The game must be closed when you install. If Crimson Desert was open, the install could not write to the PAZ file. Close the game, install again, and then relaunch the game.

4. **Did you restart the game after installing?**
   The game reads camera data at launch. If the game was already running (even if you closed it briefly and reopened), make sure you did a full restart.

5. **Is the correct preset selected?**
   Double-check that the preset you want is the one that is currently active in UCM's sidebar before clicking Install to Game.

---

### Corrupted backup message

**What it means:** UCM detected that your backup file in the `backups/` folder was not made from vanilla (unmodified) game files. This can happen if another camera mod had already modified your game files when UCM first created its backup.

**What UCM does automatically:** When it detects a corrupted backup, UCM **automatically deletes it** and shows you a message explaining the situation.

**How to fix it:**

1. **Verify your game files on Steam or Epic.** This restores the real, unmodified `0.paz` to your game folder.
   - Steam: Right-click Crimson Desert > Properties > Installed Files > Verify integrity of game files.
   - Epic: Three dots next to Crimson Desert > Verify.

2. **Delete the `backups/` folder** next to the UCM executable (UCM may have already done this for you, but check to be sure).

3. **Relaunch UCM.** UCM will create a clean backup the next time you install a preset.

---

### Game update warning

**What it means:** UCM detected that Crimson Desert may have been updated since the last time you installed a camera preset. Game updates typically replace the `0.paz` file, which means your camera settings have likely been overwritten with vanilla defaults.

**What to do:**

1. Dismiss the warning.
2. Select the preset you want in UCM's sidebar.
3. Click **Install to Game** to reapply your camera settings.

Your preset is stored safely inside UCM. The game update only overwrites the game files, not your UCM presets. You just need to reinstall.

**Note:** If the game update changed the vanilla camera XML (for example, added new camera behaviors or adjusted default values), your preset may feel slightly different. This is because UCM applies your modifications on top of the new vanilla base. If things feel off, you may need to re-tune some of your settings.

---

## File and Archive Errors

### "Archive/PAMT not found"

**What it means:** UCM cannot locate the game's PAZ archive files. UCM expects to find two files in the game directory:
- `GameFolder/0010/0.paz` (the main archive)
- `GameFolder/0010/0.pamt` (the archive's table of contents)

**How to fix it:**

1. **Check your game folder path.** Open UCM's settings or game directory configuration and make sure it points to the correct Crimson Desert installation folder.

2. **Verify the expected folder structure.** Navigate to your Crimson Desert game folder manually and check that a `0010` subfolder exists, and that it contains both `0.paz` and `0.pamt`.

3. **Verify game files on Steam or Epic.** If the files are missing, a verification will re-download them.

4. **If you moved or reinstalled the game**, UCM may still be pointing to the old location. Update the game path in UCM to the new location.

---

### "Camera file not found"

**What it means:** UCM found the PAZ archive, but could not extract the camera XML from within it. The camera data either does not exist in the archive or the archive structure is different from what UCM expects.

**How to fix it:**

1. **Verify your game files on Steam or Epic.** The most likely cause is a corrupted or incomplete PAZ archive. Verification will replace it with a clean copy.

2. **If verification does not help**, the game may have been updated in a way that moved the camera data to a different location within the archive. Check whether a newer version of UCM is available that supports the current game version.

---

## Editing and Slider Issues

### Sliders greyed out on Fine Tune

**What it means:** Some or all sliders on the Fine Tune tab are greyed out and you cannot drag them. There are three possible reasons:

**Reason 1: The preset is locked.**
- Look for a **padlock icon** next to the preset name in the sidebar.
- If the padlock is closed (locked), click it to unlock the preset.
- Once unlocked, the sliders become interactive.
- Note: UCM Presets (official) are locked by default. Duplicate them to My Presets if you want to edit.

**Reason 2: Steadycam is controlling those values.**
- If you have **Steadycam** enabled, it takes over certain camera parameters to provide smooth, stable camera movement.
- Hover your mouse over any greyed-out slider. If you see a tooltip that says **"Controlled by Steadycam"**, that means Steadycam is managing that value.
- To regain manual control, disable Steadycam or adjust its settings so it releases control of those specific parameters.

**Reason 3: The preset is a raw import.**
- If the preset was imported from an XML file, PAZ archive, or Mod Manager package, it is a raw import.
- Raw imports only support God Mode editing. The Quick and Fine Tune tabs (and their sliders) are disabled entirely for raw imports.
- See the next section for more details.

---

### UCM Quick and Fine Tune tabs disabled

**What it means:** The Quick and Fine Tune tabs are completely disabled (greyed out or showing a message explaining the limitation). You can only use God Mode.

**Why this happens:** You are viewing one of these preset types:

1. **A raw imported preset.** Presets imported from XML files, PAZ archives, or Mod Manager packages contain only the finished camera XML. UCM cannot determine what slider positions would recreate those values, so it cannot offer slider-based editing. Only God Mode (direct XML editing) is available.

2. **A Full Manual Control preset.** These presets are designed for users who want complete control over the raw XML without UCM's Camera Rules engine. By design, they only support God Mode.

**What to do:**
- If you need slider control, create a new **Managed by UCM** preset and use the imported preset as a reference (view it in God Mode to see the values, then set similar values using sliders on your new preset).
- If the preset is a `.ucmpreset` file that someone shared with you, it should have full slider control. If it does not, it may have been created as a Full Manual Control preset by the original author.

---

## Display and Formatting Issues

### European decimal separator issues

**What it means:** In some European Windows locales, the decimal separator is a comma (`,`) instead of a period (`.`). This used to cause problems in older versions of UCM where camera values like `3.4` might be read or written as `3,4`, which would break the camera XML.

**Status:** This was **fixed in UCM v2.5**. UCM now forces `InvariantCulture` globally, which means decimal points are always written and read as periods (`.`) regardless of your Windows locale settings.

**If you are still experiencing this issue:**
- Make sure you are running UCM v2.5 or later.
- If you are on a current version and still seeing comma-related problems, please report it as a bug.

---

## In-Game Camera Issues

### Camera jumps when pressing CTRL near NPCs

**What it means:** When you press CTRL near an NPC to interact with them, the camera snaps to a different position before settling into the interaction view. This can be jarring, especially if your camera offsets are significantly different from the game's default values.

**Why this happens:** The NPC interaction camera sections (`Player_Interaction_LockOn` and `Interaction_LookAt`) are left at vanilla values in UCM presets. This is intentional. Modifying these sections can cause crashes. However, if your regular camera offsets are very different from vanilla, the transition from your custom camera to the vanilla interaction camera can create a visible "jump."

**What to do:**
- This is a known limitation with no clean fix at this time. The jump is cosmetic and brief.
- If the jump is very noticeable, you can try reducing how far your camera offsets differ from the vanilla defaults. Presets that are closer to the game's default camera position will have less noticeable jumps during NPC interactions.

---

### Preset looks different after a game update

**What it means:** After Crimson Desert receives a patch, your camera preset looks or feels slightly different even though you did not change anything in UCM.

**Why this happens:** Game updates can modify the vanilla camera XML (the base that UCM applies your changes on top of). If the vanilla values shift, your modifications are applied to a different starting point, which can change the end result slightly.

**What to do:**

1. Reinstall your preset (the game update likely overwrote the PAZ file, so your camera settings need to be reapplied anyway).
2. Launch the game and check if the camera feels right.
3. If it feels off, open your preset in UCM and re-tune the settings. The sliders are applying changes relative to the current vanilla values, so small adjustments should bring things back to where you want them.

---

### HUD centering not working on ultrawide

**What it means:** If you are playing on an ultrawide monitor and expected UCM to center the game's HUD, this feature is currently disabled.

**Why:** A recent Crimson Desert game update added **integrity checks** on the HUD XML files. When the HUD XML is modified, the game displays a **Coherent Gameface watermark** on screen. This watermark is disruptive and cannot be hidden, so UCM has temporarily disabled HUD centering to avoid triggering it.

**What to do:**
- This is a known limitation. The UCM team is looking for a workaround. Check for UCM updates where this feature may be re-enabled.
- For now, the HUD will remain at its default position on ultrawide displays.

---

## Still Having Problems?

If your issue is not listed here, or if the steps above did not resolve it, here are some general troubleshooting steps:

### General Reset Procedure

This resolves most issues by starting fresh:

1. **Restore the game camera.** Click **Restore** in UCM to put back the vanilla `0.paz`.
2. **Verify game files** on Steam or Epic to make sure the game installation is clean.
3. **Delete the `backups/` folder** next to the UCM executable.
4. **Relaunch UCM.**
5. **Try installing your preset again.**

### Gathering Information for a Bug Report

If you need to report a problem, gather the following:

| Information | Where to Find It |
|---|---|
| UCM version | Shown in the title bar or About dialog |
| The error message | Take a screenshot or copy the exact text |
| `install_trace.txt` | Located in the UCM folder, contains diagnostic data from the last install |
| Your preset file | The `.ucmpreset` file from `my_presets/` if the issue is specific to one preset |
| Your system locale | Windows Settings > Time & Language > Region (relevant for decimal separator issues) |
| Game platform | Steam or Epic Games Store |

### Where to Get Help

- Check the UCM GitHub repository for open issues that may match your problem.
- Open a new issue on GitHub with the information gathered above.
- Community forums and Discord servers for Crimson Desert modding may also have users who can help.
