# UCM and Community Presets

UCM comes with two curated collections of presets that you can browse and download without ever leaving the app: **UCM Presets** (official presets made by the UCM team) and **Community Presets** (presets shared by other players). This page explains how to find, download, update, and manage both types.

---

## Table of Contents

- [Overview](#overview)
- [UCM Presets (Official)](#ucm-presets-official)
- [Community Presets](#community-presets)
- [Browsing and Downloading Presets](#browsing-and-downloading-presets)
- [Update Detection](#update-detection)
- [How Updates Work Behind the Scenes](#how-updates-work-behind-the-scenes)
- [Managing Downloaded Presets](#managing-downloaded-presets)
- [Submitting Your Own Community Preset](#submitting-your-own-community-preset)

---

## Overview

| Collection | Who Makes Them | Where They Live | How to Access |
|---|---|---|---|
| **UCM Presets** | The UCM development team | Hosted on the UCM GitHub repository (`v3-dev` branch, `ucm_presets/` folder) | Click **Browse** next to the "UCM Presets" header in the sidebar |
| **Community Presets** | Players like you | Hosted in a separate GitHub repository (`FitzDegenhub/ucm-community-presets`) | Click **Browse** next to the "Community Presets" header in the sidebar |

Both collections are **downloaded on demand**. UCM does not bundle all presets in the download. Instead, it fetches them from GitHub when you choose to browse or download one. This keeps the app lightweight and lets the preset catalogs grow independently of UCM releases.

---

## UCM Presets (Official)

UCM Presets are camera styles designed and tested by the UCM team. They are meant to serve as high-quality starting points that cover a range of play styles. Think of them as the "built-in" styles, but hosted online so they can be updated without requiring a new UCM release.

### Where They Come From

UCM presets are stored on the `v3-dev` branch of the UCM GitHub repository, inside the `ucm_presets/` folder. A file called `catalog.json` acts as the master list. It contains metadata for every available preset, including:

- Preset name
- Description
- Tags (for example: cinematic, action, exploration, combat-focused)
- Revision number (used for update detection)
- Download URL

When you click Browse, UCM fetches this `catalog.json` to build the list of available presets.

### How UCM Presets Appear in the Sidebar

- UCM Presets that you have downloaded appear in the sidebar under the **UCM Presets** group.
- Presets you have not downloaded yet do not appear in the sidebar. You need to use the Browse button to find and download them.
- Once downloaded, a UCM Preset behaves like any other preset. You can load it, view its settings, and install it to the game.

### Can I Edit UCM Presets?

UCM Presets are typically locked by default. This means you can view their settings and install them, but you cannot modify them directly. If you want to tweak a UCM Preset:

1. Load the UCM Preset.
2. Duplicate it to **My Presets** (right-click or use the duplicate option).
3. Edit your duplicated copy freely.

This way the original UCM Preset stays intact and can still receive updates.

---

## Community Presets

Community Presets are camera styles shared by other Crimson Desert players. They cover an even wider range of preferences, from ultra-cinematic letterbox-style cameras to tight over-the-shoulder action views and everything in between.

### Where They Come From

Community presets are hosted in a dedicated GitHub repository at `FitzDegenhub/ucm-community-presets`. This repository has its own catalog file that UCM reads to show you what is available.

### How Community Presets Appear in the Sidebar

- Downloaded community presets appear under the **Community Presets** group in the sidebar.
- Like UCM Presets, they are only visible in the sidebar after you download them.
- The Browse button next to the "Community Presets" header opens the catalog for community presets specifically.

---

## Browsing and Downloading Presets

The process is the same for both UCM and Community presets. The only difference is which Browse button you click.

### Step by Step

1. In the sidebar, find the group header you want:
   - **UCM Presets** for official presets
   - **Community Presets** for player-shared presets

2. Click the **Browse** button next to that group header.

3. The **Catalog Browser** dialog opens. This is a window that shows you all available presets in that collection. For each preset, you will see:

   | Information | Description |
   |---|---|
   | **Name** | The preset's display name |
   | **Description** | A summary of what the camera feels like and what it changes |
   | **Tags** | Labels like "cinematic", "action", "exploration" that help you find what you are looking for |
   | **Download button** | Click to download and add the preset to your sidebar |

4. Browse through the list. You can read the descriptions and tags to find presets that match what you are looking for.

5. When you find one you want, click its **Download** button.

6. UCM downloads the preset file from GitHub and saves it locally.

7. The preset now appears in the appropriate sidebar group (UCM Presets or Community Presets) and is ready to use.

### What Happens When You Download

- The preset file is saved into the appropriate local folder alongside UCM.
- UCM records a **SHA256 fingerprint** of the downloaded file. This fingerprint is used later to detect whether the preset has been updated on GitHub (more on this below).
- The preset is immediately available for loading, previewing, and installing to the game.

---

## Update Detection

One of UCM's most useful features for catalog presets is **automatic update detection**. Both UCM and Community presets can be updated by their authors after you have already downloaded them. UCM keeps track of this for you.

### How It Works

1. **In the background**, UCM periodically checks the revision numbers of presets in the online catalogs against the versions you have downloaded locally.

2. Each preset has a **revision number** in the catalog. When the preset author publishes an update, they increment this number.

3. UCM also uses **SHA256 fingerprinting** to track exactly which version of each preset you have on your computer. The fingerprint is a unique identifier based on the file's contents. If even a single byte changes, the fingerprint changes.

4. When UCM detects that a preset on GitHub has a newer revision than the one you have locally, it flags that preset for update.

### What You See

- In the sidebar, any preset that has an available update shows a **pulsating update icon** next to its name. This icon gently pulses to draw your attention without being too intrusive.

- The pulsating icon means: "The author has published a newer version of this preset."

### Updating a Preset

1. Click on the preset with the pulsating update icon in the sidebar.

2. UCM opens an update prompt. Before downloading the new version, UCM offers to **duplicate your current version to My Presets**. This is important because:
   - If you have been using the old version and like it, you might want to keep it.
   - The update will replace the old version in the UCM Presets or Community Presets group.
   - By duplicating to My Presets first, you have a personal copy of the old version that will not be affected by updates.

3. Choose whether to duplicate the old version:
   - **Yes, duplicate first**: UCM copies the current preset to your My Presets group, then downloads the update.
   - **No, just update**: UCM downloads the new version and replaces the old one. The old version is gone.

4. UCM downloads the updated preset and replaces the local copy. The SHA256 fingerprint is updated to match the new version.

5. The pulsating icon disappears, and the preset now reflects the latest version.

### When Does UCM Check for Updates?

UCM checks for updates in the background, typically when the application starts up or when you open the sidebar. This check is lightweight (it only fetches the catalog manifest, not the full preset files) and does not slow down the app.

---

## How Updates Work Behind the Scenes

Here is a more detailed look at the update detection flow, in case you are curious:

1. UCM downloads the `catalog.json` from GitHub (either the UCM repository or the community repository).
2. For each preset listed in the catalog, UCM compares the catalog's **revision number** against the revision number stored locally.
3. If the catalog revision is higher than the local revision, the preset is marked as having an update available.
4. Additionally, UCM compares the **SHA256 fingerprint** of your local preset file against the expected fingerprint. This catches cases where a preset might have been corrupted or manually modified.
5. Presets that need updates get the pulsating sidebar icon.

This system means:
- You are never forced to update. The icon is informational. You can ignore it if you prefer your current version.
- Updates do not happen automatically. You always choose when to update.
- Your personal copies in My Presets are never touched by the update system. Only presets in the UCM Presets and Community Presets groups are subject to updates.

---

## Managing Downloaded Presets

### Viewing Preset Details

Click on any downloaded UCM or Community preset in the sidebar to load it. You can view all its camera settings in the UCM Quick, Fine Tune, or God Mode tabs (depending on the preset type).

### Deleting a Downloaded Preset

If you no longer want a downloaded preset:
- Right-click the preset in the sidebar and choose Delete (or use whatever removal option is available).
- The local copy is removed. You can always re-download it later from the catalog browser.

### Duplicating to My Presets

If you want to use a UCM or Community preset as a starting point for your own customization:
1. Load the preset.
2. Duplicate it to My Presets.
3. Your duplicate is now a personal preset that you fully control. You can edit it freely, and it will not be affected by catalog updates.

---

## Submitting Your Own Community Preset

If you have created a camera style that you think other players would enjoy, you can submit it to the Community Presets catalog.

### How to Submit

1. **Export your preset** as a `.ucmpreset` file (see [Exporting and Sharing](Exporting-and-Sharing.md) for details on how to export).

2. **Submit it to the community repository** on GitHub at `FitzDegenhub/ucm-community-presets`. The typical process is:
   - Fork the repository
   - Add your `.ucmpreset` file
   - Update the catalog file with your preset's metadata (name, description, tags, author)
   - Submit a pull request

3. Once your pull request is reviewed and merged, your preset will appear in the Community Presets catalog for all UCM users.

### Tips for a Good Community Preset

- **Give it a clear, descriptive name.** Something like "Cinematic Ultrawide" or "Tight Combat Camera" tells people what to expect.
- **Write a good description.** Explain what the camera feels like, what situations it works best in, and what the main changes are from vanilla.
- **Use relevant tags.** Tags help people find your preset when browsing.
- **Test it thoroughly.** Make sure it works well in different game situations: on foot, mounted, in combat, aiming, and during cutscene transitions.

---

## Summary

| Task | How to Do It |
|---|---|
| Browse official presets | Click **Browse** next to "UCM Presets" in the sidebar |
| Browse community presets | Click **Browse** next to "Community Presets" in the sidebar |
| Download a preset | Click **Download** next to the preset in the catalog browser |
| Check for updates | Look for the pulsating icon next to presets in the sidebar |
| Update a preset | Click the preset with the pulsating icon, choose whether to keep the old version |
| Submit your own preset | Export as `.ucmpreset`, submit a pull request to the community GitHub repository |
| Keep an old version before updating | Choose "Yes, duplicate first" when the update prompt appears |
