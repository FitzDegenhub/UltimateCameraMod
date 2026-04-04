# Changelog

All notable changes to Ultimate Camera Mod are documented here.

Format based on [Keep a Changelog](https://keepachangelog.com/). Versioning follows [Semantic Versioning](https://semver.org/).

---

## [Unreleased] (v3.1.0)

### Added
- **Preset type selection** - New preset dialog now offers two modes: "Managed by UCM" (default, full slider control with UCM camera rules) and "Full Manual Control" (vanilla XML start, God Mode only, no UCM rules). Choice stored as `preset_mode` in preset file and persists across saves, reloads, and duplicates.
- **Raw XML import separation** - Raw XML / PAZ / mod manager imports are now standalone presets with no UCM rules applied on top. UCM Quick and Fine Tune tabs are disabled with a clear explanation. Only God Mode editing is available. Prevents UCM from silently overwriting imported mod values (FoV, zoom effects, camera sway).
- **Vanilla backup validation** - `ValidateVanilla()` now checks 5 modification signatures (FoV values, ZoomDistance="3.4", OffsetByVelocity zeroing, MaxZoomDistance="30", padding comments). Existing backups without a `vanilla_verified` stamp are re-validated on launch. Tainted backups auto-deleted with step-by-step fix instructions.
- **First-run welcome overlay** - Styled verification prompt (matching tutorial gold/dark theme) before the tutorial on first launch. Asks users to confirm game files are verified on Steam. "No, close UCM" shuts down the app for verification first.

### Changed
- **Error messages overhauled** - All major error messages rewritten with clear, actionable guidance: archive not found explains folder structure, camera file not found suggests verifying on Steam, size too large lists common causes, corrupted backup explains auto-clear with next steps, install/restore uses plain language, imported preset failures explain possible causes.
- **Release notes** - Clean install section updated with warning emoji and stronger wording about verifying game files.

### Fixed
- **Tainted vanilla backups from v2.5 upgrades** - Users upgrading from v2.5 or running other PAZ mods (e.g. NO HUD) could end up with a backup captured from modified game files. The old `ValidateVanilla()` only checked FoV on two sections and allowed "40" which UCM itself sets. Now caught automatically.

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
