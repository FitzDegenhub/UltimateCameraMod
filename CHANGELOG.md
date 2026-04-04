# Changelog

All notable changes to Ultimate Camera Mod are documented here.

Format based on [Keep a Changelog](https://keepachangelog.com/). Versioning follows [Semantic Versioning](https://semver.org/).

---

## [Unreleased] (v3.1.0)

### Added
- **In-app overlay dialog system** - All dialogs converted from Windows popups to themed in-app overlays (gold-bordered dark cards). New `MainWindow.Overlay.cs` with reusable `ShowOverlayAsync`, `ShowAlertOverlayAsync`, `ShowConfirmOverlayAsync`, `ShowThreeChoiceOverlayAsync`, `ShowFatalOverlayAndClose`. X close button on all overlays (except fatal). Backdrop clicks disabled.
- **Preset type selection** - New preset dialog with two clickable cards: "Managed by UCM" (recommended, full slider control) and "Full Manual Control" (vanilla XML, God Mode only). Feature tags with green ticks/red crosses show what's enabled. OR divider between options. Choice stored as `preset_mode` in preset file.
- **Raw XML import separation** - Raw XML / PAZ / mod manager imports are standalone presets with no UCM rules. UCM Quick and Fine Tune disabled. Only God Mode available. Prevents UCM from overwriting imported mod values.
- **Vanilla backup validation** - `ValidateVanilla()` checks FoV=40 and OffsetByVelocity=0 signatures. Relaxed checks for June 2026 game patch (MaxZoomDistance=30 and XML comments now vanilla). Auto-deletes tainted backup AND 0.paz when validation fails (Steam verify alone doesn't restore modded files).
- **First-run welcome overlay** - Styled verification prompt before tutorial. "No, close UCM" shuts down app.
- **Game update warning overlay** - Detected game updates show as overlay with Snooze/Dismiss instead of bottom strip.
- **Fatal error overlay** - Unclosable overlay with "Close UCM" button for critical errors (tainted backup, unreadable camera files). Cannot be dismissed by clicking backdrop.
- **Wiki** - 21 pages (11 user guide + 10 developer) published to GitHub Wiki tab with sidebar navigation.

### Changed
- **All MessageBox.Show calls replaced** - Every alert, confirmation, and error dialog is now a themed overlay. No more Windows-style popups.
- **Error messages overhauled** - All major error messages rewritten with clear, actionable guidance. Tainted backup error auto-deletes 0.paz and provides simple Steam verify steps.
- **Export dialog** - Removed redundant description under header (each format has its own description).
- **Import type picker** - Cards with hover highlight instead of plain buttons.
- **Browse catalog cards** - Description/tags on left, Download/Nexus buttons on right.
- **Game Default sidebar group** - Vanilla preset separated from UCM presets.
- **Height slider** range extended to -1.6/1.5.

### Fixed
- **Tainted vanilla backups from v2.5 upgrades** - Auto-detected and auto-cleaned with proper fix instructions.
- **HShift slider staying gold on locked presets** - `ApplyCenteredLock` was re-enabling the slider after `ApplyPresetEditingLockUi` disabled it.
- **God Mode columns squished on first load** - WPF DataGrid doesn't measure columns with collapsed groups. Fixed with double-bind on first entry and adjusted column widths (SECTION 200, SUB-ELEMENT 140, ATTRIBUTE 250, VALUE 120, VANILLA 120).
- **Camera preview not updating when switching presets** - `SyncPreview` was blocked by `_suppressEvents` during preset loading.
- **Community preset "View on Nexus" link lost after update** - Updates now preserve URL, author, description metadata.
- **False update icons after downloading presets** - Raw byte downloads preserve SHA hash for update detection.
- **Game crash on load** - Removed Player_Interaction_LockOn/Interaction_LookAt from LockOnSections. Removed unsafe byte-replacement fallback in ArchiveWriter.
- **PAZ import from different game version** - Shows helpful overlay explaining PAMT version mismatch.

---

## [v3.0.1] - 2026-04-03

### Added
- **UCM preset catalog browser** - UCM style presets downloaded on demand via Browse button. Official presets hosted on v3-dev branch with auto-generated `catalog.json`.
- **Preset update detection** - Background catalog check compares revision numbers. Outdated presets show update icon in sidebar. Update prompt offers to duplicate old version before downloading.
- **Game Default sidebar group** - Vanilla preset separated into its own group.
- **Community and UCM update detection** - SHA256 comparison against GitHub catalogs with pulsating update icons.
- **Browse catalog redesign** - Cleaner card layout with description/tags on the left, Download and Nexus link buttons on the right.

### Changed
- **Style IDs renamed** - Old cryptic IDs (`western`, `cinematic`, `immersive`, `lowcam`, `vlowcam`, `ulowcam`, `re2`) replaced with self-documenting names (`heroic`, `panoramic`, `close-up`, `low-rider`, `knee-cam`, `dirt-cam`, `survival`).
- **Definition-based UCM presets** - Presets stored as style definitions (style_id + settings), session XML baked locally from game files + current CameraRules.
- **UCM Quick layout** - Custom Offsets and Global Settings side-by-side at top, camera previews side-by-side below.
- **God Mode SECTION column** - Auto-stretches to fit window width.
- **Live camera preview** - Shows selected preset name.
- **Height slider** - Range extended to -1.6/1.5.

### Fixed
- **Game crash on load** - Removed `Player_Interaction_LockOn` and `Interaction_LookAt` from LockOnSections (modifying NPC dialogue camera sections caused crash).
- **Game crash on load** - Removed unsafe byte-replacement fallback in ArchiveWriter that produced invalid XML.
- **Session XML corruption** - Removed `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` from preset serialization.
- **Camera preview and FoV** - Now update immediately when switching presets.
- **Community preset "View on Nexus" link** - No longer disappears after update.
- **False update icons** - No longer show on all presets after download.
- **Locked preset toast** - Now appears on Fine Tune and God Mode tabs.
- **Steadycam tooltip** - Now shows on disabled sliders.

---

## [v3.0.0] - 2026-04-03

### Added
- **Export-first workflow** - Tune camera in-app, export as JSON patch for mod managers. Direct PAZ install still available as secondary option.
- **Three-tier editor** - UCM Quick (fast sliders), Fine Tune (curated deep control), God Mode (full raw XML editor with vanilla comparison).
- **`.ucmpreset` file format** - Dedicated shareable preset files.
- **Sidebar preset manager** - Collapsible grouped sections: Game Default, UCM Presets, Community Presets, My Presets, Imported.
- **Multi-format export** - JSON (Mod Manager), XML, 0.paz, .ucmpreset with metadata fields.
- **Community preset catalog** - Browse and download presets from GitHub directly in-app.
- **Preset lock system** - UCM presets permanently locked, user presets toggleable via padlock icon.
- **True Vanilla preset** - Decoded directly from game backup with no modifications.
- **Lock-on zoom slider** - Replaces combat camera dropdown (-60% to +60%).
- **Steadycam expanded** - 30+ camera states smoothed, individually tunable in Fine Tune.
- **Lock-on distance scaling** - ZoomDistance values scale dynamically with chosen camera distance.
- **Live camera + FoV preview** - Distance-aware top-down view with FoV cone.
- **Game patch awareness** - Tracks install metadata, warns when game may have updated.
- **Auto-save** - Changes to unlocked presets write back to file automatically.
- **Auto-migration** - Legacy `.json` presets migrated to `.ucmpreset` on first launch.

### Changed
- **Value-edits only** - Removed structural XML injection from v2 (extra zoom levels, horse first-person, horse camera overhaul). Modifies only existing values for safer sharing across game patches.

---

## [v2.5] - 2026-03-31

### Fixed
- **European locale camera values** - `InvariantCulture` forced for all number formatting so decimal separators are always periods regardless of Windows locale.

---

## [v2.4] - 2026-03-31

Incremental improvements and tuning.

---

## [v2.3] - 2026-03-31

Incremental improvements and tuning.

---

## [v2.2] - 2026-03-31

### Added
- Steadycam system
- Extra zoom levels
- Horse first person mode
- Horizontal shift control
- Universal FoV
- Skill aiming consistency
- Import XML support
- Preset sharing
- Update notifications

---

## [v2.1] - 2026-03-30

### Added
- Advanced editor with full XML parameter control
- Export/import preset strings (`UCM:` and `UCM_ADV:` formats)

---

## [v2.0] - 2026-03-30

### Changed
- Complete rewrite from Python to C# WPF application
- Single-file `.exe`, no installer required

---

## [v1.5] - 2026-03-30

Initial public release (Python version).
