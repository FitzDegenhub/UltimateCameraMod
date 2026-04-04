# Frequently Asked Questions

This page answers the most common questions about Ultimate Camera Mod (UCM). If your question is not here, check the [Troubleshooting](Troubleshooting.md) page for error-specific help.

---

## Table of Contents

- [General Questions](#general-questions)
- [Compatibility](#compatibility)
- [Safety and Security](#safety-and-security)
- [Game Updates](#game-updates)
- [Sharing and Presets](#sharing-and-presets)
- [Technical Details](#technical-details)
- [Features and Limitations](#features-and-limitations)

---

## General Questions

### What is UCM?

Ultimate Camera Mod (UCM) is a standalone camera customization toolkit for Crimson Desert. It lets you adjust the third-person camera distance, height, field of view, and dozens of other camera parameters without manually editing game files. UCM provides a visual interface with sliders, previews, and preset management, and handles all the technical work of packing your changes into the game's archive format.

### Do I need any other tools to use UCM?

No. UCM is a standalone application. You do not need a mod manager, a hex editor, or any other modding tools. Just download UCM, run it, adjust your camera, and click Install to Game.

That said, UCM can also export presets in formats compatible with Crimson Desert JSON Mod Manager and CDUMM, if you prefer to use those tools.

### Does UCM require administrator privileges?

No. UCM is a portable application that runs from whatever folder you put it in. It does not need admin rights. Just make sure the folder is writable (avoid placing it in `Program Files` or other system-protected directories).

### Can I have multiple presets saved at the same time?

Yes. You can create as many presets as you want. They are saved as individual files in your `my_presets/` folder. However, only one preset can be installed to the game at a time. Installing a new preset replaces the previous one.

---

## Compatibility

### Does UCM work with other mods?

Yes, in most cases. UCM only modifies the camera XML data inside the game's `0.paz` archive. Other mods that modify different files, different sections of the PAZ archive, or different game data entirely will work fine alongside UCM.

The exception is **other mods that also modify the camera data**. For example, some "NO HUD" mods touch camera settings as part of their changes. If two mods both try to modify the same camera XML, they will conflict. Whichever one was installed last will overwrite the other.

If you use a mod manager (Crimson Desert JSON Mod Manager or CDUMM), exporting your camera changes as a JSON (Mod Manager) package lets the mod manager handle conflicts more gracefully than direct PAZ installation.

### Can I use UCM with the Epic Games version?

Yes. UCM auto-detects Epic Games Store installations of Crimson Desert. The camera system works identically regardless of whether you bought the game on Steam or Epic. All features, including Install to Game, Restore, and game patch detection, work on both platforms.

### Does UCM work online?

Crimson Desert is primarily a single-player game. UCM modifies local game files only. It does not interact with any online services, servers, or multiplayer systems. Your camera changes exist only on your computer.

### Does UCM work with different screen resolutions or aspect ratios?

Yes. UCM's camera modifications work at any resolution and aspect ratio. Field of view, camera distance, and other settings are not resolution-dependent. The camera will look consistent whether you play at 1080p, 1440p, 4K, or ultrawide.

The one exception is **HUD centering for ultrawide monitors**, which is currently disabled due to a game integrity check issue. See [Features and Limitations](#features-and-limitations) below.

---

## Safety and Security

### Is UCM safe to use?

Yes. UCM is safe for several reasons:

- **Open source.** UCM's full source code is publicly available under the MIT license. Anyone can inspect exactly what the application does.
- **VirusTotal scans.** Every UCM release includes a link to a VirusTotal scan so you can verify the files are clean before running them.
- **Limited scope.** UCM only modifies one file: `0.paz` in your Crimson Desert game folder. It does not touch the Windows registry, system files, or anything outside the game directory and its own folder.
- **Fully reversible.** You can undo all of UCM's changes by clicking the Restore button or by verifying game files on Steam/Epic.

### Can UCM get me banned?

Crimson Desert is primarily a single-player game. UCM modifies local game files and does not interact with any anti-cheat system or online service. As of this writing, there is no known risk of a ban from using UCM.

### Does UCM collect any data?

No. UCM does not send any telemetry, usage data, or personal information anywhere. The only network activity UCM performs is downloading preset catalogs and preset files from GitHub when you use the Browse feature.

### Can I undo everything UCM has done?

Yes, completely. You have two options:

1. **Click Restore in UCM.** This reverts the game's `0.paz` file to the backed-up vanilla version.
2. **Verify game files on Steam or Epic.** This re-downloads the original `0.paz` from the game's servers, erasing all modifications.

Either method returns the game to its original, unmodified state.

---

## Game Updates

### Will a game update break my camera settings?

Yes, most likely. When Crimson Desert receives an update, the update typically replaces the `0.paz` file in the game folder. This overwrites your installed camera settings, returning the camera to vanilla defaults.

Your presets inside UCM are not affected. They are stored in UCM's own folders, not in the game directory. You just need to reinstall your preferred preset after the game updates.

### How does UCM know when the game has been updated?

UCM tracks install metadata, including a reference to Steam's `appmanifest` file (or equivalent for Epic). This file contains version information about the installed game. When UCM detects that the game version has changed since your last install, it shows a warning.

### What should I do after a game update?

1. Launch UCM. You will likely see a warning about a possible game update.
2. Dismiss the warning.
3. Select your preset in the sidebar.
4. Click **Install to Game** to reapply your camera settings.
5. Launch the game and check that everything looks right.

If the camera feels slightly different, the game update may have changed the vanilla camera values. Re-tune your preset as needed. See [Troubleshooting](Troubleshooting.md) for more details.

### Do I need to delete my backup after a game update?

Not usually. UCM's backup is a snapshot of the vanilla `0.paz` from before you first installed. If the game update changes the PAZ structure significantly, UCM will warn you. In most cases, you can just reinstall your preset and it works fine.

If you encounter errors after a game update, the safest approach is: verify game files, delete the `backups/` folder, relaunch UCM, and install again. This gives UCM a fresh backup based on the updated vanilla files.

---

## Sharing and Presets

### Can I share my presets with friends?

Yes. There are several ways:

| Method | How | What They Get |
|---|---|---|
| **UCM Preset file** | Export as `.ucmpreset`, send the file | Full slider control in UCM (recommended) |
| **JSON (Mod Manager)** | Export as JSON, share the folder or zip it | A mod package they can install with a mod manager |
| **Raw XML** | Export as XML, send the file | Camera data they can import into UCM (God Mode only) |
| **0.paz file** | Export as PAZ, send the file | A file they can drop into the game folder directly |

For sharing with other UCM users, the `.ucmpreset` format is strongly recommended because it preserves all slider values and settings.

### Can I submit my preset to the community catalog?

Yes. The community preset catalog is hosted on GitHub at `FitzDegenhub/ucm-community-presets`. To submit:

1. Export your preset as a `.ucmpreset` file.
2. Fork the community repository on GitHub.
3. Add your preset file and update the catalog.
4. Submit a pull request.

Once accepted, your preset will be available to all UCM users through the Browse button. See [UCM and Community Presets](UCM-and-Community-Presets.md) for the full details.

### What is the difference between UCM Presets and Community Presets?

| | UCM Presets | Community Presets |
|---|---|---|
| **Made by** | The UCM development team | Players in the community |
| **Quality** | Curated and tested by the UCM team | Varies, community-contributed |
| **Hosted on** | UCM GitHub repository (`v3-dev` branch) | Separate GitHub repository (`FitzDegenhub/ucm-community-presets`) |
| **How to access** | Browse button next to "UCM Presets" in sidebar | Browse button next to "Community Presets" in sidebar |

Both types can be browsed and downloaded directly inside UCM.

### What is the difference between JSON export and Install to Game?

These serve different purposes:

- **JSON (Mod Manager) export** creates a patch package for use with Crimson Desert JSON Mod Manager or CDUMM. The mod manager handles installing, uninstalling, and resolving conflicts with other mods. This is the best option for users who manage all their mods through a mod manager.

- **Install to Game** writes your camera changes directly into the game's `0.paz` file. UCM manages the backup and offers a Restore button. This is the best option for users who do not use a mod manager and want a quick, direct install.

You do not need to use both. Pick whichever workflow fits your setup.

---

## Technical Details

### What does "value edits only" mean?

In UCM v3, presets only modify **existing values** in the camera XML. UCM does not inject new XML structures, new zoom levels, or new camera states. This is different from UCM v2, which could add things like extra zoom levels and horse first-person mode by inserting new XML nodes.

The "value edits only" approach has an important benefit: presets are safer to share across different game versions. Since UCM only changes values that already exist in the vanilla XML, there is less risk of a preset breaking after a game update. The vanilla structure stays intact; only the numbers change.

### How does UCM modify the game files?

Here is the simplified process:

1. UCM reads the vanilla camera XML from its backup of the original `0.paz` file.
2. UCM applies your preset settings (from sliders or God Mode edits) to produce a modified camera XML.
3. The modified XML is size-matched to fit within the PAZ archive's allocated slot.
4. The data is compressed with LZ4 compression.
5. The compressed data is encrypted with ChaCha20 encryption.
6. UCM writes the result back into the game's `0.paz` file.

For a detailed walkthrough, see [Installing to Game](Installing-to-Game.md).

### What is the PAZ format?

PAZ is the archive format used by Crimson Desert to store game data. Think of it like a ZIP file that contains many game files packed together. The camera XML is one of the files inside the `0.paz` archive. UCM knows how to read and write PAZ archives, including handling the PAMT table of contents, LZ4 compression, and ChaCha20 encryption.

### What is God Mode?

God Mode is UCM's most powerful editing mode. It gives you direct access to the camera XML data in a grid/table format, where you can change any value manually. Unlike the Quick and Fine Tune tabs (which use sliders that map to specific camera behaviors), God Mode lets you edit any XML attribute in the camera file.

God Mode is the only editing mode available for raw imported presets (XML, PAZ, Mod Manager imports) because UCM cannot determine what slider positions would recreate the imported values.

### What are Camera Rules?

Camera Rules are UCM's internal system for translating slider positions into XML changes. When you drag a slider on the Quick or Fine Tune tab, UCM does not just change one value in the XML. It applies a set of rules that may modify multiple XML attributes together to produce the desired camera behavior. For example, changing the "Distance" slider might adjust the zoom distance, field of view compensation, and offset values all at once.

Raw imports do not go through the Camera Rules engine, which is why they are limited to God Mode editing.

---

## Features and Limitations

### Is HUD centering available for ultrawide monitors?

Not currently. HUD centering for ultrawide displays is **temporarily disabled**. A Crimson Desert game update added integrity checks that trigger a **Coherent Gameface watermark** when the HUD XML is modified. This watermark appears on screen and cannot be hidden. UCM has disabled HUD centering until a workaround is found.

### Can I modify the stealth finisher camera?

No. The stealth finisher camera sections (called `SilenceKill` in the XML) **cannot be modified without crashing the game**. This is a hard limitation of Crimson Desert's camera system. UCM leaves these sections untouched, and if you modify them manually in God Mode, the game will crash when a stealth finisher animation plays.

### Can I modify the NPC interaction camera?

The NPC interaction camera sections (`Player_Interaction_LockOn` and `Interaction_LookAt`) are left at vanilla values by UCM. Modifying them can cause crashes. Leaving them at vanilla means there may be a brief camera "jump" when you press CTRL near NPCs to interact, especially if your camera offsets differ significantly from the game's defaults. This is a known cosmetic issue with no clean fix at this time.

### Is there a limit to how much I can change?

Practically speaking, yes. The modified camera XML must compress to fit within the original PAZ slot size. In normal use with value edits, this is not a problem. The limit only becomes an issue if you are making very extensive God Mode changes that add entirely new XML nodes, or if your backup was tainted (made from already-modified game files that contained extra data).

### Can I undo slider changes?

Yes. On the UCM Quick tab, you can press **Ctrl+Z** to undo recent changes (up to 20 steps). This works like undo in any other application.

### Can I lock a preset to prevent accidental edits?

Yes. Click the **padlock icon** next to the preset name in the sidebar to lock it. When a preset is locked, all sliders and editing controls are disabled. Click the padlock again to unlock it. UCM Presets (official) are locked by default to preserve them; duplicate to My Presets if you want to modify.

### Does UCM support multiple game installations?

UCM detects one game installation at a time. If you have Crimson Desert installed on both Steam and Epic, UCM will use whichever one it detects first (Steam is checked before Epic). You can manually set the game path if UCM picks the wrong one.

### Can I use UCM presets made on a different version of the game?

Generally yes, because UCM v3 uses value edits only. Presets modify existing values rather than injecting new structures, so they are more resilient across game versions. However, if a game update changes the vanilla camera XML significantly (adding or removing camera sections), some preset values may not apply cleanly. In that case, you may need to re-tune the preset.

### My preset looks different after a game update. Why?

Game updates can change the vanilla camera XML that UCM uses as its base. Since UCM applies your modifications on top of vanilla, any changes to the base values shift the result. For example, if the vanilla camera distance was 3.4 and you set it to 5.0 (a difference of +1.6), and then a game update changes vanilla to 3.6, your preset would now result in 5.2. The effect is usually subtle, but noticeable if you are particular about your camera feel. Re-tuning with the current vanilla as a base fixes this.
