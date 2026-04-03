> **Nexus Mods page temporarily unavailable** — Hey everyone, apologies for the Nexus page being down. As a new author on the platform, the Nexus Mods team are currently reviewing my source code before allowing the project back on the site - which honestly makes total sense and I fully support it. I've already been in contact with them and they've been really helpful, so we just have to wait it out. In the meantime all downloads are available right here on **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)**. Thank you so much for your patience - I've been busy working on v3 in the meantime!
>
> **Branch note:** `main` has been left at v2.5 (the version currently under Nexus review). The source code for **v3.0.1 Beta** lives on the [`v3-dev`](https://github.com/FitzDegenhub/UltimateCameraMod/tree/v3-dev) branch. You can download v3.0.1 Beta from [GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.0.1).

# Ultimate Camera Mod - Crimson Desert

Standalone camera toolkit for Crimson Desert. Full GUI, live camera preview, three editing tiers, file-based presets, **JSON export for [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** and **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), and ultrawide HUD support.

<p align="center">
  <img src="screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert banner" width="100%" />
</p>

<p align="center">

[![Download v2.5](https://img.shields.io/badge/Download-v2.5_stable-brightgreen?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v2.5)
[![Download v3.0.1 Beta](https://img.shields.io/badge/Download-v3.0.1_Beta-orange?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.0.1)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-UCM-d98f40?style=for-the-badge&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/crimsondesert/mods/438)

[![VirusTotal v2.5](https://img.shields.io/badge/VirusTotal_v2.5-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file/091bdb6456df85b25ce80a90d26710ae1a7f55edf189f8921cbafb153262074a)
[![VirusTotal v3.0.1](https://img.shields.io/badge/VirusTotal_v3.0.1-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file/1773dbf1d835cc9b29ed0b2e06347779fb8d0138e371a59d2bac09395766f49b)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)
[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

</p>

---

## Branch overview

| Branch | Status | What it is |
|--------|--------|------------|
| **`main`** | Stable (v2.5) — under Nexus source review | v2.x direct-install app (single exe, writes straight to game PAZ) |
| **`v3-dev`** | Beta — source for v3.0 Beta release | v3 export-first app — tune in-app, export **.json** for **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** or **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** |

v3 includes every camera feature from v2 plus a redesigned UI, file-based presets, a three-tier editor, and multi-format export. Direct PAZ install is still available in v3 as a secondary option.

---

## Features

### Camera controls

| Feature | Details |
|---------|---------|
| **8 built-in presets** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival - with live preview |
| **Custom camera** | Sliders for distance (1.5–12), height (-1.6–0.5), and horizontal shift (-3–3). Proportional scaling keeps the character at the same screen position across all zoom levels |
| **Field of view** | Vanilla 40° up to 80°. Universal FoV consistency across guard, aim, mount, glide, and cinematic states |
| **Centered camera** | Dead-center character across 150+ camera states, eliminating the left-offset shoulder cam |
| **Lock-on zoom** | Slider from -60% (zoom in on target) to +60% (pull back wide). Affects all lock-on, guard, and rush states. Works independently of Steadycam |
| **Mount camera sync** | Mount cameras match your chosen player camera height |
| **Horizontal shift on all mounts** | Horse, elephant, wyvern, canoe, warmachine, and broom all respect your shift setting with proportional scaling |
| **Skill aiming consistency** | Lantern, Blinding Flash, Bow, and all aim/zoom/interaction skills respect horizontal shift. No camera snap when activating abilities |
| **Steadycam smoothing** | Normalized blend timing and velocity sway across 30+ camera states: idle, walk, run, sprint, combat, guard, rush/charge, freefall, super jump, rope pull/swing, knockback, all lock-on variants, mount lock-on, revive lock-on, aggro/wanted, warmachine, and all mount states. Every value is community-tunable via the Fine Tune editor |
| **HUD centering** | Width slider (1200–3840 px) for ultrawide. *Currently disabled - a game update added integrity checks that trigger a Coherent Gameface watermark. Will be re-enabled when a workaround is found.* |

> **v3 design philosophy: value edits only, no structural injection.**
>
> Earlier versions injected new XML lines into the camera file (extra zoom levels, horse first-person mode, horse camera overhaul with additional zoom tiers). v3 removes these features intentionally. Injecting structure has a much higher chance of breaking after game updates, and personal preferences for niche camera modes are better served by dedicated mods distributed through mod managers. UCM now modifies only existing values - the same line count, the same element structure, the same attributes. This makes presets safer to share and more resilient across game patches.

### Three-tier editor (v3)

v3 organizes editing into three tabs so you can go as deep as you want:

| Tier | Tab | What it does |
|------|-----|--------------|
| 1 | **UCM Quick** | The fast layer - distance/height/shift sliders, FoV, centered camera, lock-on zoom (-60% to +60%), mount sync, steadycam, live camera + FoV previews |
| 2 | **Fine Tune** | Curated deep-tuning. Searchable sections for on-foot zoom levels, horse/mount zoom, global FoV, special mounts & traversal, combat & lock-on, camera smoothing, and aiming & crosshair position. Builds on top of UCM Quick |
| 3 | **God Mode** | Full raw XML editor - every parameter in a searchable, filterable DataGrid grouped by camera state. Vanilla comparison column with modified values highlighted. Expand/collapse all, search, and per-state filtering |

### File-based preset system (v3)

- **`.ucmpreset` file format** - dedicated shareable format for UCM camera presets. Drop into any preset folder and it just works
- **Sidebar manager** with collapsible grouped sections: UCM Presets, Community Presets, My Presets, Imported
- **New / Duplicate / Rename / Delete** from the sidebar
- **Lock** presets to prevent accidental edits - UCM presets are permanently locked; user presets toggleable via padlock icon
- **True Vanilla preset** - raw decoded `playercamerapreset` from your game backup with no modifications applied. Quick sliders are synced to the actual game baseline values
- **Import** from `.ucmpreset`, raw XML, PAZ archives, or Mod Manager packages
- **Auto-save** - changes to unlocked presets write back to the preset file automatically (debounced)
- Auto-migration from legacy `.json` presets to `.ucmpreset` on first launch

### Community preset catalog (v3) - NEW

Browse and download community camera presets directly from UCM. One-click download, no accounts needed.

- **[Community presets repo](https://github.com/FitzDegenhub/ucm-community-presets)** - presets hosted on GitHub, catalog auto-generated by GitHub Actions
- **Browse button** on the "Community presets" sidebar header opens the catalog browser
- Each preset shows name, author, description, tags, and a link to the creator's Nexus page
- Downloaded presets appear in the sidebar under "Community presets" (locked by default - duplicate to edit)
- **2MB file size limit** and JSON validation for safety

**Want to share your preset with the community?** Export as `.ucmpreset` from UCM, then either:
- Submit a [Pull Request](https://github.com/FitzDegenhub/ucm-community-presets/pulls) to the community presets repo
- Or send your `.ucmpreset` file to 0xFitz on Discord/Nexus and we'll add it for you

### Multi-format export (v3)

The **Export for sharing** dialog outputs your session in four ways:

| Format | Use case |
|--------|----------|
| **JSON** (mod managers) | Byte patches + `modinfo` for **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) or **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Export in UCM → import in the manager you use; recipients do not need UCM. **Prepare** is only offered when the live `playercamerapreset` entry still matches UCM’s vanilla backup (verify game files if you already applied camera mods). |
| **XML** | Raw `playercamerapreset.xml` for other tools or manual editing |
| **0.paz** | Patched archive ready to drop into the game's `0010` folder |
| **.ucmpreset** | Full UCM preset for other UCM users |

Includes title, version, author, Nexus URL, and description fields for JSON/XML. Shows patch region count and bytes changed before saving `.json`.

### Quality of life

- **Auto game detection** - Steam, Epic Games, Xbox / Game Pass
- **Automatic backup** - vanilla backup before any modification; one-click restore. Version-aware with auto-cleanup on upgrade
- **Install config banner** - shows your full active config (FoV, distance, height, shift, settings)
- **Game patch awareness** - tracks install metadata after apply; warns when the game may have updated so you can re-export
- **Live camera + FoV preview** - distance-aware top-down view with horizontal shift and field of view cone
- **Update notifications** - checks GitHub releases on launch
- **Game folder shortcut** - opens your game directory from the header
- **Windows taskbar identity** - proper icon grouping and title bar icon via shell property store
- **Settings persistence** - all selections remembered between sessions
- **Resizable window** - size persists between sessions
- **Portable** - single `.exe`, no installer required

### Preset sharing (v2 + v3)

> **Nobody has perfected Crimson Desert's camera yet - and that's the point.**
>
> The vanilla game has over 150 camera states, each with dozens of parameters. No single developer can tune all of that for every playstyle and display. UCM was built with sharing at its core.
>
> Every setting you tweak can be exported and shared. The guard-camera zoom snap that plagued the vanilla game was solved by a single user adjusting one FoV value. That kind of community-driven fine-tuning is exactly what this tool is for.

**v2 string formats** (still supported):

| Format | Contains |
|--------|----------|
| `UCM:...` | Distance, height, horizontal shift |
| `UCM_ADV:...` | Full XML parameter overrides |

**v3** adds file-based JSON presets and multi-format export on top of these.

---

## How it works

1. Locates the game's PAZ archive containing `playercamerapreset.xml`
2. Creates a backup of the original file (only once - never overwrites a clean backup)
3. Decrypts the archive entry (ChaCha20 + Jenkins hash key derivation)
4. Decompresses via LZ4
5. Parses and modifies the XML camera parameters based on your selections
6. Re-compresses, re-encrypts, and writes the modified entry back into the archive

HUD modifications follow the same pipeline for `ui/minimaphudview2.html`, `ui/statusgaugeview2.html`, and `ui/gamecommon.css` in archive `0012`.

No DLL injection, no memory hacking, no internet connection required - pure data file modification.

---

## Building from source

Requires [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (or later). Windows x64.

### v3 (recommended)

Close any running instance before building - the exe copy step fails if the file is locked.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### v2.x (single-file publish)

```bash
cd src/UltimateCameraMod
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `bin/Release/net6.0-windows/win-x64/publish/`

### Dependencies (NuGet - restored automatically)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) - LZ4 block compression/decompression

---

## Project structure

```
src/UltimateCameraMod/              Shared library + v2.x WPF app
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, data models
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x UI
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 export-first WPF app (references shared code above)
├── Controls/                       CameraPreview, FovPreview (v3 variants)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Embedded community presets deployed on first launch
├── MainWindow.xaml                 Two-panel shell: sidebar + tabbed editor
├── ExportJsonDialog.xaml           Multi-format export wizard (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Import from .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Preset metadata entry (name, author, description, URL)
├── CommunityBrowserDialog.xaml     Browse & download community presets from GitHub
├── NewPresetDialog.xaml            Create / name new presets
├── ShellTaskbarPropertyStore.cs    Windows taskbar icon via shell property store
├── ApplicationIdentity.cs          Shared App User Model ID
└── UltimateCameraMod.V3.csproj

docs/                               Release notes, Nexus stub, PR summary
```

---

## Compatibility

- **Platforms:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **Display:** Any aspect ratio - 16:9, 21:9, 32:9

---

## FAQ

**Will this get me banned?**
UCM modifies offline data files only. It does not touch game memory, inject code, or interact with running processes. Use at your own discretion in online/multiplayer modes.

**The game updated and my camera is back to vanilla.**
Normal - game updates overwrite modded files. Re-open UCM and click Install (or re-export JSON for JSON Mod Manager / CDUMM). Your settings are saved automatically.

**My antivirus flagged the exe.**
Known false positive with self-contained .NET apps. VirusTotal scans are clean: [v2.5](https://www.virustotal.com/gui/file/091bdb6456df85b25ce80a90d26710ae1a7f55edf189f8921cbafb153262074a) / [v3-beta](https://www.virustotal.com/gui/file/c4c3451d9dff70ed36d6d60a4e59de4718a5cfdd248ac9e7bc4a9ef50d22c947/detection). Full source is available here to review and build yourself.

**What does horizontal shift 0 mean?**
0 = vanilla camera position (character slightly to the left). 0.5 = character centered on screen. Negative values move further left, positive values move further right.

**Upgrading from a previous version?**
v2.4+ automatically cleans stale data from previous versions on first launch. v3 migrates legacy presets on first run.

---

## Version history

- **v3-beta** (`v3-dev`) - First public beta of the v3 export-first redesign. Three-tier editor (UCM Quick / Fine Tune / God Mode), `.ucmpreset` file format, file-based preset system with collapsible sidebar manager, **[community preset catalog](https://github.com/FitzDegenhub/ucm-community-presets)** with GitHub-hosted browse & download, multi-format export (JSON for **JSON Mod Manager** + **CDUMM**, XML, 0.paz, `.ucmpreset`), preset lock system (UCM presets permanently locked, user presets toggleable), auto-save to preset files, Quick→Fine Tune→God Mode settings sync, raw XML import for PAZ/XML presets, vanilla-guarded JSON prepare, true Vanilla preset from raw game XML, game patch awareness, Windows taskbar identity, new app icon. **New in beta:** Steadycam expanded to 30+ camera states (freefall, super jump, rope, knockback, all lock-on variants, warmachine, aggro/wanted — every new section individually tunable in Fine Tune). Lock-on zoom slider replaces the old combat camera dropdown (-60% zoom in to +60% pull back, works without Steadycam).
- **v2.5** - Current stable release on GitHub Releases.
- **v2.4** - Proportional horizontal shift, shift on all mounts and aim abilities, horse camera overhaul, version-aware backups, FoV preview, resizable window.
- **v2.3** - Horizontal shift fix for 16:9, delta-based slider, full install config banner.
- **v2.2** - Steadycam, extra zoom levels, horse first person, horizontal shift, universal FoV, skill aiming consistency, Import XML, preset sharing, update notifications.
- **v2.1** - Fixed custom preset sliders not writing to all zoom levels.
- **v2.0** - Complete rewrite from Python to C# / .NET 6 / WPF. Advanced XML editor, preset management, auto game detection.
- **v1.5** - Python version with customtkinter GUI.

---

## Credits & acknowledgements

- **0xFitz** - UCM development, camera tuning, advanced editor, ultrawide HUD support

### C# rewrite (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** - CrimsonDesertTools - C# PAZ/PAMT parser, ChaCha20 encryption, LZ4 compression, PaChecksum, archive repacker (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** - Pure C# ChaCha20 stream cipher implementation (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** - PAZ repacking guide: 16-byte alignment, PAMT checksum, PAPGT root index patching

### Original Python version (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** - crimson-desert-unpacker - PAZ archive tooling, decryption research
- **@Maszradine** - CDCamera - Camera rules, steadycam system, style presets
- **@manymanecki** - CrimsonCamera - Dynamic PAZ modification architecture

## Support

If you find this useful, consider supporting development:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## License

MIT
