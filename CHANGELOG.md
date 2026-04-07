# Changelog

All notable changes to Ultimate Camera Mod are documented here.

Format based on [Keep a Changelog](https://keepachangelog.com/). Versioning follows [Semantic Versioning](https://semver.org/).

---

## [Unreleased]

### Features
- **Center HUD** - New checkbox + dropdown in UCM Quick to center gameplay HUD elements for ultrawide displays. Choose between 16:9 (1920px) or 21:9 (2520px) safe areas. Modifies HTML/CSS in PAZ archive 0012 independently from camera. Switching modes or unchecking restores vanilla automatically. Credits to wsres for the technique.

### Bug Fixes
- **Center HUD not applying** - Size matching was scattering XML comments throughout HTML content, triggering the Coherent Gameface watermark. Now uses a single trailing comment matching CentreHUD's proven approach.
- **Center HUD mode switching had no effect** - Reinstalling with a different safe area (16:9 vs 21:9) was detected as "already installed" and skipped. Now restores vanilla first before reinstalling with the new mode.
- **Sacred values missing on raw import/Full Manual Control preset path** - BuildGodModeSessionXml's raw import path returned before ReapplyGodModeOverrides ran.

---

## [v3.1.2] - 2026-04-06

### Bug Fixes
- **Sacred values missing from Install and exports when on God Mode tab** - `BuildGodModeSessionXml` was missing the `ReapplyGodModeOverrides` call. Sacred values that matched vanilla were excluded from `BuildExpertModSet` and lost during rebuild. Now all Install and export paths correctly include sacred values regardless of which tab you're on.

---

## [v3.1.1] - 2026-04-05

### Bug Fixes
- **False-positive tainted backup detection** - ZoomDistance=3.4 and RightOffset=0.5 checks matched vanilla values, causing all users with clean game files to see the tainted backup error on launch. Reverted to FoV=40 and OffsetByVelocity=0 checks only.

---

## [v3.1] - 2026-04-05

### Camera
- **Lock-on auto-rotate toggle** - New checkbox in UCM Quick to disable camera snap-to-target when locking on. Prevents the camera from whipping around to face a target behind you. Discovered by [sillib1980](https://github.com/sillib1980). Sets `IsAutoRotate` and `IsTargetFixed` on lock-on camera sections.

### God Mode
- **Sacred God Mode overrides** - Values you edit in God Mode are now permanently protected from Quick/Fine Tune rebuilds. The camera rule engine skips any field you've explicitly touched. Your values stay exactly where you put them, no matter what happens in Quick or Fine Tune. Addresses issues #18, #20.
- **Green sacred indicators** - Sacred values show in green in the God Mode grid (white = vanilla, gold = rules-modified, green = sacred/protected). Row count shows sacred count.
- **"Sacred only" filter** - New filter option in God Mode dropdown to show only your protected values.
- **Fine Tune slider locking** - Fine Tune sliders that control sacred God Mode values are greyed out with a green label and tooltip "Sacred -- controlled by God Mode".
- **One-time sacred toast** - First God Mode edit on a managed preset shows a status message explaining the value is protected.

### UI
- **Version-aware upgrade overlay** - Existing users upgrading between versions (e.g. v3.0.2 to v3.1) now see a "What's new" overlay on first launch. Previously only triggered for users without tutorial_done.flag.

### Bug Fixes
- **Full Manual Control presets not installing edits** - Install was using the original vanilla XML from preset creation instead of rebuilding from current God Mode edits. God Mode changes on Full Manual Control presets now work correctly.
- **God Mode overrides file polluted with CameraRules values** - `advanced_overrides.json` previously saved ALL values that differed from vanilla, including values set by CameraRules (not the user). Now only saves values the user explicitly edited.
- **Fresh install showing upgrade overlay** - Startup created files (backups, preset dirs) before the fresh-install check ran, so it always detected "existing data". Detection now runs before any file creation.

---

## [v3.0.2] - 2026-04-04

### UI
- **In-app overlay dialogs** - All dialogs converted from Windows popups to themed in-app overlays (gold-bordered dark cards). X close button on all overlays (except fatal errors). No more Windows-style popups anywhere.
- **Preset type selection** - New preset dialog with "Managed by UCM" and "Full Manual Control" cards. Feature tags show what's enabled (green ticks/red crosses). OR divider between options.
- **Import type picker** - Cards with hover highlight instead of plain buttons.
- **Import metadata dialog** - Textboxes enlarged (54px, 18px font, white text on dark background) for readability.
- **Export dialog** - Removed redundant description under header. Auto-populates name, author, description, URL from active preset.
- **Browse catalog cards** - Description/tags on left, Download/Nexus buttons on right.
- **Game Default sidebar group** - Vanilla preset separated from UCM presets.
- **Height slider** range extended to -1.6/1.5.
- **Preset author text** enlarged in the active preset header.
- **God Mode columns** - Fixed squished columns on first load with double-bind workaround.
- **Camera preview** - Now updates immediately when switching presets.
- **Error messages** - All major errors rewritten with clear, actionable guidance as overlay popups.
- **First-run welcome overlay** - Verification prompt before tutorial. "No, close UCM" shuts down app.
- **Game update warning** - Shows as overlay with Snooze/Dismiss instead of bottom strip.
- **Fatal error overlay** - Unclosable overlay with "Close UCM" for critical errors (tainted backup, etc.).

### Presets
- **God Mode overrides persist across tabs** - Edits saved to `advanced_overrides.json` and re-applied when Quick/Fine Tune rebuilds. No more losing God Mode tweaks.
- **Duplicate God Mode conversion** - When duplicating a God Mode preset, choose "Keep God Mode" or "Convert to UCM Managed" with warning about value changes.
- **Raw XML import separation** - Imported XML/PAZ/mod manager presets are standalone with no UCM rules. Only God Mode editing available.
- **Export writes `preset_mode`** - Exported .ucmpreset files preserve whether they're UCM managed or God Mode only.
- **UCM presets can be deleted locally** - Re-download from catalog anytime. Rename still blocked.
- **Community preset duplicates go to My Presets** - Instead of staying in the community_presets folder.

### Catalog
- **Community presets moved to main repo** - From separate `ucm-community-presets` repo to `community_presets/` folder. Single unified GitHub Actions workflow for both catalogs.
- **New community presets** - Shoulder Camera (latranchedepain), Proper 3rd Person Camera (orangeees).
- **All catalog URLs point to main branch** - Long-term stability.
- **Workflow preserves existing SHAs** - Adding a new preset no longer changes SHAs of unchanged presets. No more false update icons.
- **Raw byte downloads** - Both Browse and Update write raw bytes to preserve SHA hash for update detection.

### Camera
- **Game crash fix** - Removed Player_Interaction_LockOn and Interaction_LookAt from LockOnSections (modifying these NPC dialogue sections crashed the game).
- **Game crash fix** - Removed unsafe byte-replacement fallback in ArchiveWriter that produced invalid XML.
- **Vanilla validation relaxed** - June 2026 game patch added MaxZoomDistance=30 and XML comments to vanilla. Checks updated to only flag UCM-specific signatures.
- **Auto-fix tainted backups** - Automatically deletes tainted backup AND 0.paz when validation fails. Steam verify alone doesn't restore modded files.
- **God Mode attribute tooltips** - 54 attributes documented (up from 29). Covers interaction, blend, zoom, damping, and targeting.

### Documentation
- **Wiki** - 21 pages (11 user guide + 10 developer) published to GitHub Wiki tab with sidebar navigation.

### Bug Fixes
- **HShift slider staying gold on locked presets** - `ApplyCenteredLock` was re-enabling after lock.
- **Community preset "View on Nexus" link not showing** - URL field past 4KB header. Fixed by ordering metadata before session_xml.
- **Community preset update SHA mismatch** - Updates write raw bytes instead of re-serializing.
- **False update icons after downloading** - Raw byte downloads preserve SHA hash.
- **PAZ import from different game version** - Shows helpful overlay explaining PAMT version mismatch.
- **Export missing `preset_mode`** - Exported files now include the correct mode tag.
- **Vanilla validation false positive after game patch** - Relaxed checks for new vanilla values.

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
