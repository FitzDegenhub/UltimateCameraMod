# PR: `v3-dev` → `main` - UCM v3.0-beta

**Historical document.** This was the PR description for the v3-dev to main merge. The `v3-dev` branch no longer exists; active development now happens on `development`.

---

## Summary

Introduces **Ultimate Camera Mod v3**: a second WPF front-end (`src/UltimateCameraMod.V3/`) built around an **export-first workflow** - tune the camera in-app, export `.json` for **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) and **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), and treat direct PAZ install as secondary. Core PAZ/XML logic stays in `src/UltimateCameraMod/` and is shared with v3.

v3 is now tagged as **v3.0-beta** on GitHub Releases. v2.5 remains on `main` while Nexus source review is in progress.

---

## Goals

- Preset workflow centred on **real JSON files** (shareable, versionable) instead of only in-app state
- **One session XML** driving UCM Quick, Fine Tune, and God Mode
- **JSON patch export** aligned with JSON Mod Manager and CDUMM (byte patches, `modinfo`, decompressed offsets, human-readable region labels)
- **True "Vanilla" built-in preset** matching stock game camera data with Quick-slider baselines
- **Windows polish**: correct taskbar grouping and icon for the v3 exe
- **Game-update awareness**: warnings when install metadata drifts after patches
- **Seamless camera transitions**: Steadycam expanded to 30+ states, lock-on distances scaled dynamically

---

## What shipped on this branch

### New app and UX (`UltimateCameraMod.V3`)

- **Two-panel shell**: sidebar preset manager + tabbed editor (UCM Quick / Fine Tune / God Mode)
- **Three-tier editor**:
  - *UCM Quick* - distance, height, shift, FoV, lock-on zoom, centered camera, mount sync, steadycam, live camera + FoV previews with distance ruler
  - *Fine Tune* - curated deep-tuning in searchable bordered cards. On-foot zoom, horse/mount zoom, global FoV, special mounts, traversal, combat, lock-on, camera smoothing (Steadycam), aiming
  - *God Mode* - full raw XML DataGrid, vanilla comparison column, modified values highlighted, expand/collapse all, per-state filtering
- **Quick → Fine Tune / God Mode sync**: Quick slider changes propagate into deeper tabs so all three tiers stay consistent
- **Preset groups**: built-ins under `ucm_presets/`, community under `community_presets/`, user presets under `my_presets/`, imported under `import_presets/`; migration from legacy `presets/` layout
- **Dialogs**: Export for sharing wizard (`ExportJsonDialog` - JSON / XML / 0.paz / `.ucmpreset`), import preset (`ImportPresetDialog`), import metadata (`ImportMetadataDialog` - author / description / URL), new preset (`NewPresetDialog`), community browser (`CommunityBrowserDialog`)
- **Locked preset UX**: editing a locked preset surfaces a toast instead of silently failing; UCM presets permanently locked, user presets toggleable via padlock icon
- **Active preset header**: name, author, description (full brightness, readable size), and a "View on Nexus" button when the preset has a URL
- **Performance-minded UI**: cached resources, debounced search, async file I/O, partial 4 KB header reads for sidebar metadata (avoids deserialising full 300 KB session XMLs)
- **Branding / shell**: `ucm.ico` + PNG asset; App User Model ID + `RelaunchIconResource` via `SHGetPropertyStoreForWindow` (`ShellTaskbarPropertyStore`, `ApplicationIdentity`)

### File-based preset system

- **`.ucmpreset` file format** - dedicated shareable format. Drop into any preset folder and it just works
- **Sidebar manager** with collapsible grouped sections: UCM Presets, Community Presets, My Presets, Imported
- **New / Duplicate / Rename / Delete** from the sidebar
- **Auto-save** - changes to unlocked presets write back to the preset file automatically (debounced)
- **True Vanilla preset** - decoded directly from game backup; Quick sliders synced to actual game baseline values via `TryParseUcmQuickFootBaselineFromXml` and `QuickShiftDeltaFromFootZl2RightOffset`
- **Import** from `.ucmpreset`, raw XML, PAZ archives, or Mod Manager packages with optional metadata
- **Auto-migration** from legacy `.json` presets on first launch
- **`vanilla_preset_rev`** - forces regeneration of built-in Vanilla JSON when the baseline changes

### Community preset catalog

- **`CommunityBrowserDialog`**: fetches `catalog.json` from `ucm-community-presets` GitHub repo, renders preset cards with name, author, description, tags, and Nexus link
- One-click download; downloaded presets are **rebuilt** with metadata fields (`url`, `name`, `author`, `description`) guaranteed before `session_xml` so the 4 KB header read always finds them
- 2 MB size limit and JSON validation
- Shipped preset (`RDR2` by orangeees) embedded as assembly resource (`ShippedPresets/*.json`), deployed to `ucm_presets/` on first launch via `DeployShippedCommunityPresets()`

### Multi-format export

- **JSON** - binary diff → byte patches with human-readable region labels (`JsonModExporter`); `modinfo` block; vanilla-guarded Prepare (`IsLiveCameraPayloadMatchingStoredBackup` blocks export when live PAZ no longer matches backup)
- **XML** - raw `playercamerapreset.xml`
- **0.paz** - patched archive
- **.ucmpreset** - full UCM preset
- Export dialog includes title, version, author, Nexus URL, description; shows patch region count and bytes changed

### Camera improvements

#### Steadycam - expanded to 30+ states

Previously Steadycam covered on-foot run/sprint, guard, horse/mount states, animal form, and core lock-on sections. v3-beta adds:

| New section | Vanilla blend | UCM |
|-------------|--------------|-----|
| `Player_Weapon_Rush` (charge attack) | 0.25s | 0.6s |
| `Player_Basic_FreeFall_Start` / `FreeFall` | 0.65s | 1.0s |
| `Player_Basic_SuperJump` | 0.5s | 0.8s |
| `Player_Basic_RopePull` / `RopeSwing` | 0.5s | 0.8s |
| `Player_Hit_Throw` (knockback) | 0.5s | 0.8s |
| `Player_Ride_Warmachine_Aim` / `Dash` | 0.5s | 0.8s |
| `Player_Ride_Aim_LockOn` (mount lock-on) | 0.5s | 1.0s |
| `Player_Revive_LockOn_System` | 0s (instant) | 0.8s |
| `Player_Force_LockOn` / `Player_LockOn_Titan` | unsmoothed | 0.8–1.0s |
| `Player_Weapon_LockOn_Non_Rotate` / `WrestleOnly` | unsmoothed | 0.8–1.0s |
| `Player_StartAggro_TwoTarget` / `Wanted_TwoTarget` | 0.5s out | 1.0s |

Two new Fine Tune cards: **Movement transitions** (12 sliders) and **Extended lock-on and combat transitions** (20 sliders).

#### Lock-on zoom slider (replaces Combat Camera dropdown)

- Range: -60% (zoom in) to +60% (pull back)
- Works independently of Steadycam
- Affects all lock-on, guard, and rush states
- `MaxZoomDistance=30` moved to `BuildSharedBase()` - always applied, not just when Steadycam is on

#### Lock-on distance scaling

Lock-on `ZoomDistance` values now derive from the user's actual on-foot ZL2/ZL3/ZL4 distances (from whichever style/Fine Tune/God Mode values are active). Eliminates the jarring zoom-in that occurred when using large camera distances.

#### Finisher camera smoothing

`Player_Weapon_Down` (combat finisher): vanilla 0.5s → UCM 1.2s in / 1.5s out.

### Core services (now inside `UltimateCameraMod.V3`)

> **Note:** These files originally lived in a separate `src/UltimateCameraMod/` project and were linked via csproj `<Compile Include>`. As of April 2026 they have been consolidated directly into `src/UltimateCameraMod.V3/` (Models/, Services/, Paz/ subdirectories). The old v2.5 project folder has been removed.

- **`JsonModExporter`**: binary diff → JSON patches; `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` for preset files; `ExtractJsonStringField` decodes JSON escapes in sidebar metadata; human-readable XML offset map labels each diff region
- **`CameraMod`**: vanilla XML read path for Vanilla preset; `TryParseUcmQuickFootBaselineFromXml`; `vanilla_preset_rev`; `IsLiveCameraPayloadMatchingStoredBackup` for vanilla guard; `StripComments` for clean session XML
- **`CameraRules`**: `BuildSmoothing()` expanded to 30+ sections; `BuildSharedBase()` always applies `MaxZoomDistance=30`; `BuildCombatPullback()` replaces `BuildCombatWide/Max`; `BuildLockOnDistances()` scales lock-on ZoomDistance dynamically; `GetSteadycamKeys()` for Fine Tune lock-out; architecture comment block documents all 6 modification layers
- **`GameInstallBaselineTracker`**: persists `game_install_baseline.json` after successful apply; tracks Steam `appmanifest` fields; MainWindow wiring for snooze + informational banner
- **`ArchiveWriter`** and PAZ path tweaks as needed

### Design philosophy change

v3 removes structural XML injection (extra zoom levels, horse first-person, horse camera overhaul with additional zoom tiers). UCM v3 modifies only existing values - same line count, same element structure, same attributes. Safer to share, more resilient across game patches.

### Docs and repo

- **README**: full v3 feature documentation, branch table, three-tier editor table, multi-format export table, design philosophy note, build instructions, project structure, FAQ, version history
- **`docs/release-notes/release_notes_v3-beta.md`**: full user-facing release notes
- **`docs/V3_DEV_PR_SUMMARY.md`**: this file - PR description kept up to date with branch tip
- **`docs/NEXUS_MOD_PAGE.md`**: Nexus-facing stub
- Repo cleanup: release notes under `docs/release-notes/`, `.gitignore` hardened, tracked binaries removed

---

## Commit reference (`main`..`v3-dev`)

| Commit | Topic |
|--------|-------|
| `3013ac0` | fix: community preset link button, description display, v3.0-beta version |
| `c2e5bc4` | docs: update README for v3-beta - steadycam expansion, lock-on zoom, branch status |
| `8a0d51f` | feat(steadycam): expand smoothing to 30+ states; lock-on zoom -60%/+60% |
| `db62f37` | Fix lock-on zoom not applying to guard/rush; cover all LockOnSections |
| `836d9a2` | Move lock-on MaxZoomDistance to SharedBase so it applies without Steadycam |
| `54e13ea` | Extend lock-on zoom slider to negative range (-40% to +60%) |
| `ed636d0` | Fix Swim FoV slider staying yellow when locked (duplicate key overwrite) |
| `e64575c` | Polish lock-on zoom slider: label style, description text |
| `e113fff` | Fix slider thumb staying yellow when disabled (UCM preset lock) |
| `fbd6106` | Replace Combat Camera dropdown with Lock-on zoom slider (0–60%) |
| `c780a7f` | feat(steadycam): finisher camera smoothing and architecture docs |
| `3d5420e` | feat(ui): grey out locked sliders, FoV preview distance ruler, preset URL field |
| `c96afc8` | fix(steadycam): re-sync lock-on distances after God Mode overrides |
| `b7432ba` | feat(steadycam): re-sync lock-on distances after Fine Tune overrides |
| `d1ecd28` | feat(steadycam): scale lock-on ZoomDistance with user's chosen style distance |
| `18a3ed3` | v3: `.ucmpreset` format, community preset catalog, collapsible sidebar |
| `c1b83b1` | v3-dev: shipped RDR2 preset embed, Quick delta fix, import UX |
| `4460b42` | v3: import metadata, locked-preset UX, Quick→editor sync, Fine Tune cards, JSON patch labels |
| `8d1f328` | v3-dev: taskbar icon, vanilla preset baseline, JSON export polish, baseline tracker, docs |
| `a8f4ce7` | v3.0-dev: complete UI redesign with file-based preset system |
| `c9f3657` | Repo cleanup: release notes, `.gitignore`, remove tracked binaries |

---

## How to build and run v3 (Windows)

Requires **.NET 6 SDK** (or later). Stop any running `UltimateCameraMod.V3` before building - the exe copy step fails if the file is locked (MSB3027).

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

Single-file self-contained publish (for release zip):

```powershell
dotnet publish "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

v2.x publish flow is unchanged - see README **Building from source (v2.x)**.

---

## Risk and review focus

- **Large shared surface**: `CameraMod`, `JsonModExporter`, PAZ writer - regressions could affect v2 users. Smoke-test v2 publish/run if anything in shared code paths changed
- **Preset file format**: `.ucmpreset` is a superset of the old `.json` format; migration is one-way. Verify auto-migration on a clean profile
- **Community preset download**: rebuilt on download to guarantee metadata field order - verify Nexus link shows on activation
- **Steadycam new sections**: 14 new sections added to `BuildSmoothing()` - verify no game crashes on sections that are engine-sensitive (e.g. `Player_SilenceKill` is intentionally excluded)

---

## Checklist (for maintainers)

- [ ] v2.x: backup / install / restore still sane on `main`-equivalent + shared changes
- [ ] v3: preset create / duplicate / import / export JSON round-trip (spot-check import in JSON Mod Manager and/or CDUMM after Steam verify)
- [ ] v3: Vanilla built-in preset matches expected stock XML + Quick sliders
- [ ] v3: taskbar icon + window title icon on a clean Windows profile
- [ ] v3: community preset download → activate → Nexus link visible
- [ ] v3: Steadycam new sections - no crashes on freefall, rope, super jump, warmachine, revive lock-on
- [ ] v3: lock-on zoom at -60% and +60% - applies to guard and rush without Steadycam
- [ ] README / release messaging correct for v3-beta

---

*Last refreshed: April 2026 - v3.0-beta. Covers full branch tip including steadycam expansion (30+ states), lock-on zoom slider (-60%/+60%), community preset link fix, and all commits from initial v3 redesign through beta.*
