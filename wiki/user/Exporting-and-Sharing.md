# Exporting and Sharing

Once you have tweaked your camera settings in UCM, you can export them in several formats to share with others, use with mod managers, or install manually. This page covers all four export formats, when to use each one, and detailed steps for the most common workflows.

---

## How to Open the Export Dialog

1. Select the preset you want to export in UCM's sidebar.
2. Click the **Export** button on the sidebar.
3. The Export dialog opens, showing you four format options.

---

## Export Formats at a Glance

| Format | File Extension / Output | Best For | Compatibility |
|---|---|---|---|
| JSON (Mod Manager) | Folder with `manifest.json` + `files/` | Sharing on Nexus Mods, use with CDUMM or Crimson Desert JSON Mod Manager | Crimson Desert JSON Mod Manager, CDUMM |
| XML File | `.xml` | Manual modding, sharing raw camera data | Any XML editor, other modding tools |
| 0.paz File | `.paz` | Dropping directly into the game folder | Crimson Desert (manual install) |
| UCM Preset | `.ucmpreset` | Sharing with other UCM users | UCM only |

---

## Format 1: JSON (Mod Manager)

This is the format you want if you plan to upload your preset to Nexus Mods or share it for use with Crimson Desert JSON Mod Manager or CDUMM. It creates a proper mod package that these tools understand.

### What It Produces

A folder with this structure:

```
YourModName/
  manifest.json
  files/
    (binary diff patch files)
```

The `manifest.json` contains your mod's title, version, author, description, and Nexus URL. The `files/` directory contains **binary diff patches**, not the full camera XML. These patches represent byte-level differences between the vanilla (unmodified) decompressed camera data and your modified version. The mod manager applies these diffs at install time.

### Why Binary Diffs Instead of Full Files?

Binary diff patches are smaller and more portable across game versions. Instead of replacing the entire camera file, the mod manager only changes the specific bytes that differ from vanilla. This approach is also what Crimson Desert JSON Mod Manager and CDUMM expect.

### Step by Step

1. Click **Export** in the sidebar.
2. Select **JSON (Mod Manager)**.
3. A dedicated dialog opens with the following fields:

| Field | Description | Example |
|---|---|---|
| **Mod Title** | The name of your mod as it will appear in the mod manager. | "Cinematic Camera Overhaul" |
| **Version** | The version number of your mod. | "1.0.0" |
| **Author** | Your name or username. | "CameraEnthusiast" |
| **Description** | A description of what your camera mod changes. | "Wider FoV, lower camera angle, reduced shake" |
| **Nexus URL** | A link to your Nexus Mods page (optional but recommended if uploading there). | "https://www.nexusmods.com/crimsondesert/mods/123" |

4. Fill in the fields. The dialog also shows you **patch count** (how many byte-level diffs were generated) and **file size** information so you know what to expect.
5. Choose a save location and click **Export**.
6. UCM generates the folder structure with `manifest.json` and the binary diff files.

### After Exporting

- To share on Nexus Mods: Zip the entire output folder and upload it.
- To use locally with a mod manager: Point Crimson Desert JSON Mod Manager or CDUMM at the folder.
- Other users install your mod through their mod manager, which reads the `manifest.json` and applies the patches.

---

## Format 2: XML File

This exports the raw `playercamerapreset.xml` with all your modifications applied. It is the simplest format and gives you (or anyone) a plain text file that can be opened in any text or XML editor.

### Step by Step

1. Click **Export** in the sidebar.
2. Select **XML File**.
3. Choose where to save the `.xml` file.
4. UCM writes the complete camera XML with your changes baked in.

### When to Use This

- When you want to inspect your changes in a text editor.
- When you are sharing with someone who wants to see the raw XML.
- When another modding tool expects a plain XML file.
- As a personal backup of your camera settings in a human-readable format.

### Limitations

- Anyone who imports this XML into UCM will only get **God Mode** editing (no sliders). If the other person also uses UCM, consider exporting as `.ucmpreset` instead.
- The XML file is not directly installable into the game. It would need to be repacked into a PAZ archive or used with a mod manager.

---

## Format 3: 0.paz File

This exports a complete PAZ archive with your camera changes already applied inside it. The output file is ready to drop directly into the game's folder.

### Step by Step

1. Click **Export** in the sidebar.
2. Select **0.paz File**.
3. Choose where to save the `.paz` file.
4. UCM builds a full PAZ archive containing your modified camera data.

### When to Use This

- When you want a file that can be manually placed into the game folder to apply your camera settings without using UCM or a mod manager.
- When sharing with someone who prefers a simple drag-and-drop install.

### How to Install a PAZ File Manually

1. Navigate to your Crimson Desert game folder.
2. Go into the `0010/` subfolder.
3. **Back up** the existing `0.paz` file first (copy it somewhere safe).
4. Replace `0.paz` with the exported file.
5. Launch the game.

### Important Notes

- Make sure the game is **not running** when you replace the PAZ file. The game locks the file while running.
- To undo a manual PAZ install, either restore your backup of `0.paz` or verify game files through Steam/Epic.
- This format does not go through UCM's install system, so UCM will not track it or offer a Restore button for it. If you want UCM to manage the install, use the **Install to Game** button instead (see [Installing to Game](Installing-to-Game.md)).

---

## Format 4: UCM Preset (.ucmpreset)

This is UCM's own shareable preset format. It is the best option when sharing with other UCM users because it preserves everything.

### What It Contains

| Data | Description |
|---|---|
| **Preset Name** | The display name of your preset. |
| **Author** | Your name or username. |
| **Description** | What the preset does. |
| **Settings** | All of your slider values and configuration options. |
| **Session XML** | The complete camera XML from your editing session. |

### Step by Step

1. Click **Export** in the sidebar.
2. Select **UCM Preset**.
3. Choose where to save the `.ucmpreset` file.
4. UCM packages everything into a single file.

### When to Use This

- When sharing with someone who also uses UCM. They will get **full slider control** (Quick, Fine Tune, and God Mode) when they import it.
- When you want to back up your preset in a way that preserves all of your work, not just the output XML.
- When submitting to the UCM Community Presets catalog.

### Why This Format Is Preferred for UCM Users

When someone imports a `.ucmpreset`, they get the exact same editing experience you had. All sliders, all tabs, full control. With any other format (XML, PAZ, Mod Manager), the importer is limited to God Mode only because UCM cannot reconstruct slider positions from raw XML data.

---

## Choosing the Right Export Format

Here is a quick decision guide:

**"I want to upload to Nexus Mods."**
Use **JSON (Mod Manager)**. This is the standard format for Crimson Desert mod sites, and mod manager users can install it easily.

**"I want to share with a friend who uses UCM."**
Use **UCM Preset (.ucmpreset)**. Your friend will get full slider control.

**"I want to share with a friend who does NOT use UCM."**
Use **JSON (Mod Manager)** if they use a mod manager, or **0.paz** if they want a simple drag-and-drop file.

**"I want to inspect the raw camera data."**
Use **XML File**. Open it in any text editor.

**"I want a simple file I can drop into the game folder."**
Use **0.paz File**.

**"I want to back up my work."**
Use **UCM Preset (.ucmpreset)** for a complete backup of your settings and XML. Use **XML File** for a human-readable backup.

---

## JSON Export vs. Install to Game

These two features serve different purposes, and it is worth understanding the difference:

| | JSON (Mod Manager) Export | Install to Game |
|---|---|---|
| **What it does** | Creates a patch package for use with a mod manager | Writes your changes directly into the game's `0.paz` file |
| **Who manages it** | The mod manager handles install, uninstall, and conflicts | UCM handles install and offers a Restore button |
| **Conflict handling** | The mod manager can detect and resolve conflicts with other mods | UCM replaces the camera data directly; no conflict detection |
| **Undo method** | Uninstall through the mod manager | Click Restore in UCM, or verify game files on Steam/Epic |
| **Best for** | Sharing with others, Nexus Mods uploads, users who prefer mod managers | Personal use, quick testing, users who do not use a mod manager |

If you use Crimson Desert JSON Mod Manager or CDUMM to manage your mods, the JSON export is the safer choice because the mod manager handles conflicts and can easily uninstall the mod. If you just want your camera changes applied right now without extra tools, use Install to Game.

---

## Export Dialog Information

When you open the export dialog, UCM shows you some useful information depending on the format:

- **Patch count** (JSON format): How many byte-level differences were found between vanilla and your modified camera data. More patches means more changes.
- **File size**: The size of the output file or package. This helps you know what to expect before saving.

This information is shown for reference only. You do not need to do anything with it.

---

## Sharing Your Presets with the Community

Beyond exporting files and sharing them directly, UCM has a community preset system:

- **Community Presets catalog**: You can submit your preset to the community catalog hosted on GitHub. Other UCM users can then browse and download it directly inside UCM. See [UCM and Community Presets](UCM-and-Community-Presets.md) for details on how this works.
- **Nexus Mods**: Export as JSON (Mod Manager) format and upload to the Crimson Desert section on Nexus Mods. Include screenshots and a description of what your camera changes do.

When sharing presets, it helps to include:
- A clear description of what the preset changes (FoV, camera distance, camera height, etc.)
- Screenshots or video showing the camera in action
- Which version of the game and UCM the preset was made with
